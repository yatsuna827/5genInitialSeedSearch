using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using static System.Console;

namespace _5genInitialSeedSearch
{
    static class Program
    {
        static void Main(string[] args)
        {
            SearchDateTime(ROM.B1JP, new uint[] {0, 0x21, 0x47, 0x47, 0x26, 0xf4}, 0xC7A, 6);
            ReadKey();
        }

        static void SearchDT()
        {
            if (!File.Exists("./setting.txt"))
            {
                WriteLine("setting.txtが見つかりませんでした");
                ReadKey();
                return;
            }
            var setting = File.ReadAllText("./setting.txt").Replace("\r\n", "\n").Split('\n');
            ROM rom;
            switch (setting[1])
            {
                case "ブラック":
                case "ブラックJP":
                case "B":
                case "B1":
                case "B1JP":
                    rom = ROM.B1JP; break;
                case "ホワイト":
                case "ホワイトJP":
                case "W":
                case "W1":
                case "W1JP":
                    rom = ROM.W1JP; break;
                case "ブラック2":
                case "ブラック2JP":
                case "B2":
                case "B2JP":
                    rom = ROM.B2JP; break;
                case "ホワイト2":
                case "ホワイト2JP":
                case "W2":
                case "W2JP":
                    rom = ROM.W2JP; break;
                default:
                    WriteLine("バージョンの指定が不正です.");
                    ReadKey();
                    return;
            }
            uint[] mac, timer0;
            try
            {
                mac = setting[0].Split(' ').Select(_ => Convert.ToUInt32(_, 16)).ToArray();
            }
            catch
            {
                WriteLine("macアドレスの読込に失敗しました...");
                ReadKey(); return;
            }

            try
            {
                timer0 = setting[2].Split(' ').Select(_ => Convert.ToUInt32(_, 16)).ToArray();
            }
            catch
            {
                WriteLine("Timer0の読込に失敗しました. デフォルトの検索範囲を使用します.");
                timer0 = ConstantParameters.timer0[(int)rom];
            }

            Write("初代DSの場合は0, DSLiteの場合はそれ以外を入力 > ");
            var ds = ReadLine() == "0";

            if (!File.Exists("./seedList.txt"))
            {
                WriteLine("seedList.txtが見つかりませんでした");
                ReadKey();
                return;
            }
            var target = File.ReadAllText("./seedList.txt").Replace("\r\n", "\n").Split('\n').Where(_ => _ != string.Empty).Select(_ => Convert.ToUInt32(_, 16)).Distinct().ToArray();

            WriteLine($"{target.Length}個のseedを読み込みました");
            SearchDateTime(rom, mac, timer0, ds ? 8u : 6u, target);
            ReadKey();
        }

        static void ToIVsCode(params uint[] ivs)
        {
            uint code = 0;
            for (int i = 0; i < 6; i++) code |= ivs[i] << (5 * i);
            WriteLine($"{code:X8}");
        }

        static void SearchMTSeed()
        {
            WriteLine($"{DateTime.Now} 開始");
            Parallel.For(0, 8, i =>
            {
                uint upper = (uint)i << 28;
                for (uint seed = 0x0; seed < 0x80_0000; seed++)
                {
                    var ivs = MT.GetBWIVsCode(upper | seed);
                    // if (ivs == 0x3FFFFFFF) WriteLine($"6V {upper | seed:X8}");
                    // if (ivs == 0x3FFFFC1F) WriteLine($"5VA0 {upper | seed:X8}");
                    // if (ivs == 0x01FFFFFF) WriteLine($"5VS0 {upper | seed:X8}");
                    // if (ivs == 0x3FFFFFFF) WriteLine($"めざ氷 {upper | seed:X8}");
                    // if (ivs == 0x3E0F83E0) WriteLine($"逆ベンツ {upper | seed:X8}");

                }
            });
            WriteLine($"{DateTime.Now} 終了");
        }
        static void SearchMTSeed_()
        {
            WriteLine($"{DateTime.Now} 開始");
            for(int i=0;i<8;i++)
            {
                uint upper = (uint)i << 28;
                for (uint seed = 0x0; seed < 0x300000; seed++)
                {
                    var ivs = MT.GetBWIVsCode(upper | seed);
                    // if (ivs == 0x3FFFFFFF) WriteLine($"6V {upper | seed:X8}");
                    // if (ivs == 0x3FFFFC1F) WriteLine($"5VA0 {upper | seed:X8}");
                    // if (ivs == 0x01FFFFFF) WriteLine($"5VS0 {upper | seed:X8}");
                    if (ivs == 0x3FFFFFFF) WriteLine($"めざ氷 {upper | seed:X8}");
                    // if (ivs == 0x3E0F83E0) WriteLine($"逆ベンツ {upper | seed:X8}");

                }
            }
            WriteLine($"{DateTime.Now} 終了");
        }

        // 日時コードの事前計算
        // 並列/非並列に分けてライブラリに詰める.
        static void SearchDateTime(ROM rom, uint[] mac, uint[] timer0Range, uint frame, uint[] seedList)
        {
            var nazo = ConstantParameters.nazo[(int)rom];
            var vcount = ConstantParameters.VCount[(int)rom];
            var lockToken = new object();

            var resList = new List<string>();

            Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, e)=> {
                File.WriteAllText("./result.txt", string.Join(Environment.NewLine, resList));
            });

            WriteLine($"{DateTime.Now} 開始");
            foreach (var timer0 in timer0Range)
            {
                int found = 0;
                int doneCount = 0;
                WriteLine($"Timer0: 0x{timer0:X3}");
                Write($"{doneCount} / 100, found: 0");

                uint[][] month_ends = new uint[][]{
                    new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 },
                    new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 },
                };
                Parallel.For(0, 100, (year) =>
                {
                    var generator = new InitialSeedGenerator(mac, nazo, vcount, frame, timer0);

                    var y_code = (uint)(((year / 10) << 28) | ((year % 10) << 24));
                    var month_end = (year % 4) == 0 ? month_ends[1] : month_ends[0];

                    var yy = (uint)year + 2000 - 1;
                    uint day = (yy + (yy / 4) - (yy / 100) + (yy / 400) + ((13 * 13 + 8) / 5) + 1) % 7;
                    for (uint month = 1; month < 13; ++month)
                    {
                        var m_code = ((month / 10) << 20) | ((month % 10) << 16);
                        for (uint date = 1; date < month_end[month]; ++date)
                        {
                            var d_code = ((date / 10) << 12) | ((date % 10) << 8);
                            for (uint hour =0; hour<24; ++hour)
                            {
                                var h_code = ((hour / 10) << 28) | ((hour % 10) << 24);
                                if (hour >= 12) h_code |= 0x40000000;
                                for (uint minute=0; minute<60; ++minute)
                                {
                                    var min_code = ((minute / 10) << 20) | ((minute % 10) << 16);
                                    for (uint second=0; second<60; ++second)
                                    {
                                        var seed = generator.GenerateMTSeed(y_code | m_code | d_code | day, h_code | min_code | ((second / 10) << 12) | ((second % 10) << 8));
                                        if (seedList.Contains(seed))
                                        {
                                            lock (resList)
                                            {
                                                resList.Add($"20{year:d2}/{month:d2}/{date:d2} {hour:d2}:{minute:d2}.{second:d2},0x{timer0:X3},{seed:X8}");
                                                found++;
                                                Console.CursorLeft = 0;
                                                Write($"{doneCount} / 100, Found: {found}");
                                            }
                                        }

                                    }
                                }
                            }
                            day++; if (day == 7) day = 0;
                        }
                    }

                    lock(lockToken) { Console.CursorLeft = 0; Write($"{++doneCount} / 100, Found: {found}"); }
                });
                WriteLine();
            }

            File.WriteAllText("./result.txt", string.Join(Environment.NewLine, resList));
            WriteLine($"{DateTime.Now} 終了");
        }

        static void SearchDateTime2(ROM rom, uint[] mac, uint[] timer0Range, uint frame, uint[] seedList)
        {
            var nazo = ConstantParameters.nazo[(int)rom];
            var vcount = ConstantParameters.VCount[(int)rom];
            var lockToken = new object();

            var resList = new List<string>();

            Console.CancelKeyPress += new ConsoleCancelEventHandler((sender, e) => {
                File.WriteAllText("./result.txt", string.Join(Environment.NewLine, resList));
            });

            WriteLine($"{DateTime.Now} 開始");
            foreach (var timer0 in timer0Range)
            {
                int found = 0;
                int doneCount = 0;
                WriteLine($"Timer0: 0x{timer0:X3}");
                Write($"{doneCount} / 100, found: 0");

                uint[][] month_ends = new uint[][]{
                    new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 },
                    new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 },
                };
                Parallel.For(0, 8, (year) =>
                {
                    int month = 1, date = 1, hour = 0, minute = 0, second = 0;
                    int yy = year + 2000;
                    int mm = month;
                    if (month < 3) { yy--; mm += 12; }
                    int day = (yy + (yy / 4) - (yy / 100) + (yy / 400) + ((13 * mm + 8) / 5) + date) % 7;
                    var month_end = (month == 2) && ((year % 4) == 0) ? month_ends[1] : month_ends[0];

                    var y_code = ((year / 10) << 28) | ((year % 10) << 24);
                    var m_code = ((month / 10) << 20) | ((month % 10) << 16);
                    var d_code = ((date / 10) << 12) | ((date % 10) << 8);

                    var h_code = ((12 <= hour ? 1 : 0) << 30) | ((hour / 10) << 28) | ((hour % 10) << 24);
                    var min_code = ((minute / 10) << 20) | ((minute % 10) << 16);

                    var generator = new InitialSeedGenerator(mac, nazo, vcount, frame, timer0);
                    while (true)
                    {
                        var seed = generator.GenerateMTSeed((uint)(y_code | m_code | d_code | day), (uint)(h_code | min_code | ((second / 10) << 12) | ((second % 10) << 8)));
                        if (seedList.Contains(seed))
                        {
                            lock (resList)
                            {
                                resList.Add($"20{year:d2}/{month:d2}/{date:d2} {hour:d2}:{minute:d2}.{second:d2},0x{timer0:X3},{seed:X8}");
                                found++;
                                Console.CursorLeft = 0;
                                Write($"{doneCount} / 100, Found: {found}");
                            }
                        }

                        if (++second == 60)
                        {
                            second = 0;
                            if (++minute == 60)
                            {
                                minute = 0;
                                if (++hour == 24)
                                {
                                    hour = 0;
                                    day++; if (day == 7) day = 0;
                                    if (++date == month_end[month])
                                    {
                                        date = 1;
                                        if (++month == 13) break;
                                        m_code = ((month / 10) << 20) | ((month % 10) << 16);
                                    }
                                    d_code = ((date / 10) << 12) | ((date % 10) << 8);
                                }
                                h_code = ((hour / 10) << 28) | ((hour % 10) << 24);
                                if (hour >= 12) h_code |= 0x40000000;
                            }
                            min_code = ((minute / 10) << 20) | ((minute % 10) << 16);
                        }
                    }
                    lock (lockToken) { Console.CursorLeft = 0; Write($"{++doneCount} / 100, Found: {found}"); }
                });
                WriteLine();
            }

            File.WriteAllText("./result.txt", string.Join(Environment.NewLine, resList));
            WriteLine($"{DateTime.Now} 終了");
        }

        static void test()
        {
        }

        static void SearchDateTime(ROM rom, uint[] mac, uint timer0, uint frame)
        {
            var nazo = ConstantParameters.nazo[(int)rom];
            var vcount = ConstantParameters.VCount[(int)rom];

            var resList = new List<string>();

            uint[][] month_ends = new uint[][]{
                new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 },
                new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 },
            };


            var generator = new InitialSeedGenerator(mac, nazo, vcount, frame, timer0);


            var year = 0u;
            var y_code = (uint)(((year / 10) << 28) | ((year % 10) << 24));
            var month_end = (year % 4) == 0 ? month_ends[1] : month_ends[0];

            var yy = (uint)year + 2000 - 1;
            uint day = (yy + (yy / 4) - (yy / 100) + (yy / 400) + ((13 * 13 + 8) / 5) + 1) % 7;
            for (uint month = 1; month < 2; ++month)
            {
                var m_code = ((month / 10) << 20) | ((month % 10) << 16);
                for (uint date = 1; date < 2; ++date)
                {
                    var d_code = ((date / 10) << 12) | ((date % 10) << 8);
                    for (uint hour = 0; hour < 1; ++hour)
                    {
                        var h_code = ((hour / 10) << 28) | ((hour % 10) << 24);
                        if (hour >= 12) h_code |= 0x40000000;
                        for (uint minute = 0; minute < 1; ++minute)
                        {
                            var min_code = ((minute / 10) << 20) | ((minute % 10) << 16);
                            for (uint second = 0; second < 60; ++second)
                            {
                                var seed = generator.GenerateMTSeed(y_code | m_code | d_code | day, h_code | min_code | ((second / 10) << 12) | ((second % 10) << 8));
                                WriteLine($"20{year:d2}/{month:d2}/{date:d2} {hour:d2}:{minute:d2}.{second:d2},0x{timer0:X3},{seed:X8}");
                            }
                        }
                    }
                    day++; if (day == 7) day = 0;
                }
            }

        }

    }
}
