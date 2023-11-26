using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using static System.Buffers.Binary.BinaryPrimitives;

namespace _5genInitialSeedSearchNet7
{
    class InitialSeedGenerator
    {
        const uint H0 = 0x67452301;
        const uint H1 = 0xEFCDAB89;
        const uint H2 = 0x98BADCFE;
        const uint H3 = 0x10325476;
        const uint H4 = 0xC3D2E1F0;

        const int N = 80;

        private readonly uint[] W = new uint[N];
        private static readonly uint[] _timeCodes, _dateCodes;

        static InitialSeedGenerator()
        {
            {
                var timeCodes = new List<uint>();
                for (uint hour = 0; hour < 24; ++hour)
                {
                    var h_code = hour / 10 << 28 | hour % 10 << 24;
                    if (hour >= 12) h_code |= 0x40000000;
                    for (uint minute = 0; minute < 60; ++minute)
                    {
                        var min_code = minute / 10 << 20 | minute % 10 << 16;
                        for (uint second = 0; second < 60; ++second)
                        {
                            timeCodes.Add(h_code | min_code | second / 10 << 12 | second % 10 << 8);
                        }
                    }
                }

                _timeCodes = timeCodes.ToArray();
            }

            {
                var dateCodes = new List<uint>();
                var month_ends = new uint[][] { new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, };
                for (uint year = 0; year < 100; year++)
                {
                    var month_end = year % 4 == 0 ? month_ends[1] : month_ends[0];

                    var y_code = year / 10 << 28 | year % 10 << 24;
                    var yy = 2000u - 1;
                    var day = (yy + yy / 4 - yy / 100 + yy / 400 + (13 * 13 + 8) / 5 + 1) % 7;
                    for (uint month = 1; month < 13; ++month)
                    {
                        var m_code = month / 10 << 20 | month % 10 << 16;
                        for (uint date = 1; date < month_end[month]; ++date)
                        {
                            var d_code = date / 10 << 12 | date % 10 << 8;

                            dateCodes.Add(y_code | m_code | d_code | day);

                            day++; if (day == 7) day = 0;
                        }
                    }
                }

                _dateCodes = dateCodes.ToArray();
            }
        }

        public InitialSeedGenerator(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
        {
            W[0] = ReverseEndianness(nazo[0]);
            W[1] = ReverseEndianness(nazo[1]);
            W[2] = ReverseEndianness(nazo[2]);
            W[3] = ReverseEndianness(nazo[3]);
            W[4] = ReverseEndianness(nazo[4]);

            W[5] = ReverseEndianness(v << 16 | t0);
            W[6] = mac[4] << 8 | mac[5];
            W[7] = ReverseEndianness(0x6000000 ^ frame ^ mac[3] << 24 | mac[2] << 16 | mac[1] << 8 | mac[0]);

            W[10] = 0x00000000;
            W[11] = 0x00000000;
            W[12] = 0xFF2F0000;
            W[13] = 0x80000000;
            W[14] = 0x00000000;
            W[15] = 0x000001A0;
        }

        public void GenerateMTSeed(int dateStart, int range, HashSet<uint> targets)
        {
            foreach (var dc in _dateCodes.AsSpan().Slice(dateStart, range))
            {
                W[8] = dc;
                foreach (var tc in _timeCodes)
                {
                    W[9] = tc;

                    for (int t = 16; t < W.Length; t++)
                    {
                        var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                        W[t] = BitOperations.RotateLeft(w, 1);
                    }

                    var A = H0;
                    var B = H1;
                    var C = H2;
                    var D = H3;
                    var E = H4;

                    foreach (var w in W.AsSpan()[0..20])
                    {
                        var temp = BitOperations.RotateLeft(A, 5) + (B & C | ~B & D) + E + w + 0x5A827999;
                        E = D;
                        D = C;
                        C = BitOperations.RotateRight(B, 2);
                        B = A;
                        A = temp;
                    }
                    foreach (var w in W.AsSpan()[20..40])
                    {
                        var temp = BitOperations.RotateLeft(A, 5) + (B ^ C ^ D) + E + w + 0x6ED9EBA1;
                        E = D;
                        D = C;
                        C = BitOperations.RotateRight(B, 2);
                        B = A;
                        A = temp;
                    }
                    foreach (var w in W.AsSpan()[40..60])
                    {
                        var temp = BitOperations.RotateLeft(A, 5) + (B & C | B & D | C & D) + E + w + 0x8F1BBCDC;
                        E = D;
                        D = C;
                        C = BitOperations.RotateRight(B, 2);
                        B = A;
                        A = temp;
                    }
                    foreach (var w in W.AsSpan()[60..80])
                    {
                        var temp = BitOperations.RotateLeft(A, 5) + (B ^ C ^ D) + E + w + 0xCA62C1D6;
                        E = D;
                        D = C;
                        C = BitOperations.RotateRight(B, 2);
                        B = A;
                        A = temp;
                    }

                    var lcgSeed = (ulong)ReverseEndianness(H1 + B) << 32 | ReverseEndianness(H0 + A);
                    var seed = (uint)(lcgSeed * 0x5D588B656C078965UL + 0x269EC3UL >> 32);
                    if (targets.Contains(seed))
                    {
                        Console.WriteLine($"{lcgSeed:X16} {seed:X8} {dc >> 8:X6} {(tc >> 8) & 0x3FFFFF:X6}");
                    }
                }
            }
        }

    }

    class InitialSeedGenerator128
    {
        private static readonly Vector128<uint> H0 = Vector128.Create(0x67452301u);
        private static readonly Vector128<uint> H1 = Vector128.Create(0xEFCDAB89);
        private static readonly Vector128<uint> H2 = Vector128.Create(0x98BADCFE);
        private static readonly Vector128<uint> H3 = Vector128.Create(0x10325476u);
        private static readonly Vector128<uint> H4 = Vector128.Create(0xC3D2E1F0);

        private static readonly Vector128<uint> C0 = Vector128.Create(0x5A827999u);
        private static readonly Vector128<uint> C1 = Vector128.Create(0x6ED9EBA1u);
        private static readonly Vector128<uint> C2 = Vector128.Create(0x8F1BBCDCu);
        private static readonly Vector128<uint> C3 = Vector128.Create(0xCA62C1D6u);

        private static readonly Vector128<uint>[] _dateCodes = new Vector128<uint>[36525];
        private static readonly Vector128<uint>[] _timeCodes = new Vector128<uint>[86400 / 4];

        static InitialSeedGenerator128()
        {
            {
                var i = 0;
                var container = new uint[4];
                for (uint hour = 0; hour < 24; ++hour)
                {
                    var h_code = hour / 10 << 28 | hour % 10 << 24;
                    if (hour >= 12) h_code |= 0x40000000;
                    for (uint minute = 0; minute < 60; ++minute)
                    {
                        var min_code = minute / 10 << 20 | minute % 10 << 16;
                        for (uint second = 0; second < 60; ++second)
                        {
                            container[i++] = h_code | min_code | second / 10 << 12 | second % 10 << 8;
                            if (i == 4)
                            {
                                _timeCodes[(second + minute * 60 + hour * 3600) / 4] = Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(container.AsSpan()));
                                i = 0;
                            }
                        }
                    }
                }
            }

            {
                var i = 0;
                var month_ends = new uint[][] { new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, };
                for (uint year = 0; year < 100; year++)
                {
                    var month_end = year % 4 == 0 ? month_ends[1] : month_ends[0];

                    var y_code = year / 10 << 28 | year % 10 << 24;
                    var yy = 2000u + year - 1;
                    var day = (yy + yy / 4 - yy / 100 + yy / 400 + (13 * 13 + 8) / 5 + 1) % 7;
                    for (uint month = 1; month < 13; ++month)
                    {
                        var m_code = month / 10 << 20 | month % 10 << 16;
                        for (uint date = 1; date < month_end[month]; ++date)
                        {
                            var d_code = date / 10 << 12 | date % 10 << 8;

                            _dateCodes[i++] = Vector128.Create(y_code | m_code | d_code | day);

                            day++; if (day == 7) day = 0;
                        }
                    }
                }
            }
        }

        const int N = 80;
        private readonly Vector128<uint>[] W = new Vector128<uint>[N];
        public InitialSeedGenerator128(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
        {
            W[0] = Vector128.Create(ReverseEndianness(nazo[0]));
            W[1] = Vector128.Create(ReverseEndianness(nazo[1]));
            W[2] = Vector128.Create(ReverseEndianness(nazo[2]));
            W[3] = Vector128.Create(ReverseEndianness(nazo[3]));
            W[4] = Vector128.Create(ReverseEndianness(nazo[4]));

            W[5] = Vector128.Create(ReverseEndianness(v << 16 | t0));
            W[6] = Vector128.Create(mac[4] << 8 | mac[5]);
            W[7] = Vector128.Create(ReverseEndianness(0x6000000 ^ frame ^ mac[3] << 24 | mac[2] << 16 | mac[1] << 8 | mac[0]));

            W[10] = Vector128<uint>.Zero;
            W[11] = Vector128<uint>.Zero;
            W[12] = Vector128.Create(0xFF2F0000);
            W[13] = Vector128.Create(0x80000000);
            W[14] = Vector128<uint>.Zero;
            W[15] = Vector128.Create(0x000001A0u);
        }

        public void GenerateMTSeed(int dateStart, int range, HashSet<uint> targets)
        {
            foreach (var dc in _dateCodes.AsSpan().Slice(dateStart, range))
            {
                W[8] = dc;
                foreach (var tc in _timeCodes)
                {
                    W[9] = tc;
                    for (int t = 16; t < W.Length; t++)
                    {
                        var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                        W[t] = Vector128.ShiftLeft(w, 1) ^ Vector128.ShiftRightLogical(w, 31);
                    }

                    var (A, B, C, D, E) = (H0, H1, H2, H3, H4);

                    foreach (var w in W.AsSpan()[0..20])
                    {
                        (A, B, C, D, E) =
                            ((Vector128.ShiftLeft(A, 5) ^ Vector128.ShiftRightLogical(A, 27)) + (B & C | ~B & D) + E + w + C0,
                            A,
                            Vector128.ShiftLeft(B, 30) ^ Vector128.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }
                    foreach (var w in W.AsSpan()[20..40])
                    {
                        (A, B, C, D, E) =
                            ((Vector128.ShiftLeft(A, 5) ^ Vector128.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C1,
                            A,
                            Vector128.ShiftLeft(B, 30) ^ Vector128.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }
                    foreach (var w in W.AsSpan()[40..60])
                    {
                        (A, B, C, D, E) =
                            ((Vector128.ShiftLeft(A, 5) ^ Vector128.ShiftRightLogical(A, 27)) + (B & C | B & D | C & D) + E + w + C2,
                            A,
                            Vector128.ShiftLeft(B, 30) ^ Vector128.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }
                    foreach (var w in W.AsSpan()[60..80])
                    {
                        (A, B, C, D, E) =
                            ((Vector128.ShiftLeft(A, 5) ^ Vector128.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C3,
                            A,
                            Vector128.ShiftLeft(B, 30) ^ Vector128.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }

                    var h32 = H1 + B;
                    var l32 = H0 + A;
                    for (int i = 0; i < 4; i++)
                    {
                        var lcgSeed = (ulong)ReverseEndianness(h32[i]) << 32 | ReverseEndianness(l32[i]);
                        var seed = (uint)(lcgSeed * 0x5D588B656C078965UL + 0x269EC3UL >> 32);
                        if (targets.Contains(seed))
                        {
                            Console.WriteLine($"{lcgSeed:X16} {seed:X8} {dc[i] >> 8:X6} {(tc[i] >> 8) & 0x3FFFFF:X6}");
                        }
                    }
                }
            }
        }

    }

    class InitialSeedGenerator256
    {
        private readonly Vector256<uint> H0 = Vector256.Create(0x67452301u);
        private readonly Vector256<uint> H1 = Vector256.Create(0xEFCDAB89);
        private readonly Vector256<uint> H2 = Vector256.Create(0x98BADCFE);
        private readonly Vector256<uint> H3 = Vector256.Create(0x10325476u);
        private readonly Vector256<uint> H4 = Vector256.Create(0xC3D2E1F0);

        private readonly Vector256<uint> C0 = Vector256.Create(0x5A827999u);
        private readonly Vector256<uint> C1 = Vector256.Create(0x6ED9EBA1u);
        private readonly Vector256<uint> C2 = Vector256.Create(0x8F1BBCDCu);
        private readonly Vector256<uint> C3 = Vector256.Create(0xCA62C1D6u);

        private static readonly Vector256<uint>[] _dateCodes = new Vector256<uint>[36525];
        private static readonly Vector256<uint>[] _timeCodes = new Vector256<uint>[86400 / 8];

        static InitialSeedGenerator256()
        {
            {
                var i = 0;
                var container = new uint[8];
                for (uint hour = 0; hour < 24; ++hour)
                {
                    var h_code = hour / 10 << 28 | hour % 10 << 24;
                    if (hour >= 12) h_code |= 0x40000000;
                    for (uint minute = 0; minute < 60; ++minute)
                    {
                        var min_code = minute / 10 << 20 | minute % 10 << 16;
                        for (uint second = 0; second < 60; ++second)
                        {
                            container[i++] = h_code | min_code | second / 10 << 12 | second % 10 << 8;
                            if (i == 8)
                            {
                                _timeCodes[(second + minute * 60 + hour * 3600) / 8] = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(container.AsSpan()));
                                i = 0;
                            }
                        }
                    }
                }
            }

            {
                var i = 0;
                var month_ends = new uint[][] { new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, };
                for (uint year = 0; year < 100; year++)
                {
                    var month_end = year % 4 == 0 ? month_ends[1] : month_ends[0];

                    var y_code = year / 10 << 28 | year % 10 << 24;
                    var yy = 2000u + year - 1;
                    var day = (yy + yy / 4 - yy / 100 + yy / 400 + (13 * 13 + 8) / 5 + 1) % 7;
                    for (uint month = 1; month < 13; ++month)
                    {
                        var m_code = month / 10 << 20 | month % 10 << 16;
                        for (uint date = 1; date < month_end[month]; ++date)
                        {
                            var d_code = date / 10 << 12 | date % 10 << 8;

                            _dateCodes[i++] = Vector256.Create(y_code | m_code | d_code | day);

                            day++; if (day == 7) day = 0;
                        }
                    }
                }
            }
        }

        private readonly Vector256<uint>[] W = new Vector256<uint>[80];
        public InitialSeedGenerator256(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
        {
            W[0] = Vector256.Create(ReverseEndianness(nazo[0]));
            W[1] = Vector256.Create(ReverseEndianness(nazo[1]));
            W[2] = Vector256.Create(ReverseEndianness(nazo[2]));
            W[3] = Vector256.Create(ReverseEndianness(nazo[3]));
            W[4] = Vector256.Create(ReverseEndianness(nazo[4]));
            W[5] = Vector256.Create(ReverseEndianness(v << 16 | t0));
            W[6] = Vector256.Create(mac[4] << 8 | mac[5]);
            W[7] = Vector256.Create(ReverseEndianness(0x6000000 ^ frame ^ mac[3] << 24 | mac[2] << 16 | mac[1] << 8 | mac[0]));

            W[10] = Vector256<uint>.Zero;
            W[11] = Vector256<uint>.Zero;
            W[12] = Vector256.Create(0xFF2F0000);
            W[13] = Vector256.Create(0x80000000);
            W[14] = Vector256<uint>.Zero;
            W[15] = Vector256.Create(0x000001A0u);
        }

        public void GenerateMTSeed(int dateStart, int range, HashSet<uint> targets)
        {
            foreach (var dc in _dateCodes.AsSpan().Slice(dateStart, range))
            {
                W[8] = dc;
                foreach (var tc in _timeCodes)
                {
                    W[9] = tc;
                    for (int t = 16; t < W.Length; t++)
                    {
                        var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                        W[t] = Vector256.ShiftLeft(w, 1) ^ Vector256.ShiftRightLogical(w, 31);
                    }

                    var (A, B, C, D, E) = (H0, H1, H2, H3, H4);

                    foreach (var w in W.AsSpan()[0..20])
                    {
                        (A, B, C, D, E) =
                            ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B & C | ~B & D) + E + w + C0,
                            A,
                            Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }
                    foreach (var w in W.AsSpan()[20..40])
                    {
                        (A, B, C, D, E) =
                            ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C1,
                            A,
                            Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }
                    foreach (var w in W.AsSpan()[40..60])
                    {
                        (A, B, C, D, E) =
                            ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B & C | B & D | C & D) + E + w + C2,
                            A,
                            Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }
                    foreach (var w in W.AsSpan()[60..80])
                    {
                        (A, B, C, D, E) =
                            ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C3,
                            A,
                            Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                            C,
                            D);
                    }

                    var h32 = H1 + B;
                    var l32 = H0 + A;
                    for (int i = 0; i < 8; i++)
                    {
                        var lcgSeed = (ulong)ReverseEndianness(h32[i]) << 32 | ReverseEndianness(l32[i]);
                        var seed = (uint)(lcgSeed * 0x5D588B656C078965UL + 0x269EC3UL >> 32);
                        if (targets.Contains(seed))
                        {
                            Console.WriteLine($"{lcgSeed:X16} {seed:X8} {dc[i] >> 8:X6} {(tc[i] >> 8) & 0x3FFFFF:X6}");
                        }
                    }
                }
            }
        }

    }
}
