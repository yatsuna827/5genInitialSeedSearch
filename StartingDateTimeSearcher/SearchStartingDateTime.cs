using _5genInitialSeedSearchNet7;
using System.Net.Mail;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Console;

static class SearchStartingDateTime
{
    public static void Execute(Version version, uint[] macAddress, uint frames, HashSet<uint> targets, bool useParallel)
    {
        var (_, nazo, vCount, timer0Range) = version;
        var param = new SearchDateTimeParams(
            macAddress,
            nazo,
            vCount,
            frames,
            timer0Range
        );

        if (Vector256.IsHardwareAccelerated)
            SIMD256.Search(param, targets, useParallel);
        else if (Vector128.IsHardwareAccelerated)
            SIMD128.Search(param, targets, useParallel);
        else
            NotUseSIMD.Search(param, targets, useParallel);
    }

    record SearchDateTimeParams(uint[] MacAddress, uint[] NazoValues, uint VCount, uint Frames, uint[] Timer0Range);

    static class NotUseSIMD
    {
        public static void Search(SearchDateTimeParams parameters, HashSet<uint> targets, bool useParallel)
        {
            WriteLine("Not use SIMD");
            WriteLine($"{DateTime.Now} 開始");
            if (useParallel) 
                SearchParallel(parameters, targets);
            else 
                SearchSerial(parameters, targets);
            WriteLine($"{DateTime.Now} 終了");
        }

        private static void SearchParallel(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;

            var pCount = Environment.ProcessorCount;
            var parts = new int[pCount];
            for (int i = 0; i < parts.Length; i++) parts[i] = 36525 / pCount;
            var sum = parts.Sum();
            for (int i = 0; i < parts.Length && sum < 36525; i++)
            {
                parts[i]++;
                sum++;
            }
            var starts = new int[pCount];
            for (int i = 1; i < starts.Length; i++) starts[i] = starts[i - 1] + parts[i - 1];

            foreach (var timer0 in timer0Range)
            {
                WriteLine($"Timer0: 0x{timer0:X}");
                Parallel.For(0, pCount, (p) =>
                {
                    var generator = new InitialSeedGenerator(macAddress, nazoValues, vCount, frames, timer0);
                    generator.GenerateMTSeed(starts[p], parts[p], targets);
                });
            }
        }
        private static void SearchSerial(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;
            foreach (var timer0 in timer0Range)
            {
                WriteLine($"Timer0: 0x{timer0:X}");
                var generator = new InitialSeedGenerator(macAddress, nazoValues, vCount, frames, timer0);
                generator.GenerateMTSeed(0, 36525, targets);
            }
        }

    }

    static class SIMD128
    {
        public static void Search(SearchDateTimeParams parameters, HashSet<uint> targets, bool useParallel)
        {
            WriteLine("Use SIMD128");
            WriteLine($"{DateTime.Now} 開始");
            if (useParallel)
                SearchParallel(parameters, targets);
            else
                SearchSerial(parameters, targets);
            WriteLine($"{DateTime.Now} 終了");
        }

        private static void SearchParallel(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;

            var pCount = Environment.ProcessorCount;
            var parts = new int[pCount];
            for (int i = 0; i < parts.Length; i++) parts[i] = 36525 / pCount;
            var sum = parts.Sum();
            for (int i = 0; i < parts.Length && sum < 36525; i++)
            {
                parts[i]++;
                sum++;
            }
            var starts = new int[pCount];
            for (int i = 1; i < starts.Length; i++) starts[i] = starts[i - 1] + parts[i - 1];

            foreach (var timer0 in timer0Range)
            {
                WriteLine($"Timer0: 0x{timer0:X}");
                Parallel.For(0, pCount, (p) =>
                {
                    var generator = new InitialSeedGenerator128(macAddress, nazoValues, vCount, frames, timer0);
                    generator.GenerateMTSeed(starts[p], parts[p], targets);
                });
            }
        }
        private static void SearchSerial(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;
            foreach (var timer0 in timer0Range)
            {
                WriteLine($"Timer0: 0x{timer0:X}");
                var generator = new InitialSeedGenerator128(macAddress, nazoValues, vCount, frames, timer0);
                generator.GenerateMTSeed(0, 36525, targets);
            }
        }
    }

    static class SIMD256
    {
        public static void Search(SearchDateTimeParams parameters, HashSet<uint> targets, bool useParallel)
        {
            WriteLine("Use SIMD256");
            WriteLine($"{DateTime.Now} 開始");
            if (useParallel)
                SearchParallel(parameters, targets);
            else
                SearchSerial(parameters, targets);
            WriteLine($"{DateTime.Now} 終了");
        }

        private static void SearchParallel(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;

            var pCount = Environment.ProcessorCount;
            var parts = new int[pCount];
            for (int i = 0; i < parts.Length; i++) parts[i] = 36525 / pCount;
            var sum = parts.Sum();
            for (int i = 0; i < parts.Length && sum < 36525; i++)
            {
                parts[i]++;
                sum++;
            }
            var starts = new int[pCount];
            for (int i = 1; i < starts.Length; i++) starts[i] = starts[i - 1] + parts[i - 1];

            foreach (var timer0 in timer0Range)
            {
                WriteLine($"Timer0: 0x{timer0:X}");
                Parallel.For(0, pCount, (p) =>
                {
                    var generator = new InitialSeedGenerator256(macAddress, nazoValues, vCount, frames, timer0);
                    generator.GenerateMTSeed(starts[p], parts[p], targets);
                });
            }
        }
        private static void SearchParallelZURU(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;
            var part = 36525 / 4; // 9131
            Parallel.For(0, 8, (y) =>
            {
                if (y < 4)
                {
                    var generator = new InitialSeedGenerator256(macAddress, nazoValues, vCount, frames, timer0Range[0]);

                    var start = part * y;
                    var range = y != 3 ? part : part + 1;
                    generator.GenerateMTSeed(start, range, targets);
                }
                else
                {
                    var generator = new InitialSeedGenerator256(macAddress, nazoValues, vCount, frames, timer0Range[1]);

                    var start = part * (y - 4);
                    var range = y != 7 ? part : part + 1;
                    generator.GenerateMTSeed(start, range, targets);
                }

                WriteLine(DateTime.Now);
            });
        }
        private static void SearchSerial(SearchDateTimeParams parameters, HashSet<uint> targets)
        {
            var (macAddress, nazoValues, vCount, frames, timer0Range) = parameters;
            foreach (var timer0 in timer0Range)
            {
                WriteLine($"Timer0: 0x{timer0:X}");
                var generator = new InitialSeedGenerator256(macAddress, nazoValues, vCount, frames, timer0);
                generator.GenerateMTSeed(0, 36525, targets);
            }
        }

    }

}
