using System.Runtime.Intrinsics;
using static System.Console;

static class SearchMTSeed
{
    public static void Execute()
    {
        var targets = new Dictionary<uint, string>
        {
            [0x3FFFFFFF] = "6V",
            [0x3FFFFC1F] = "5VA0",
            [0x01FFFFFF] = "5VS0",
            [0x3E0F83E0] = "逆ベンツ",
        };

        if (Vector256.IsHardwareAccelerated)
            Search_SIMD256(targets);
        else if (Vector128.IsHardwareAccelerated)
            Search_SIMD128(targets);
        else
            Search(targets);
    }

    static void Search(IReadOnlyDictionary<uint, string> targets)
    {
        WriteLine($"{DateTime.Now} 開始");
        Parallel.For(0, 0xFF, n =>
        {
            var h8 = (uint)n << 24;
            var table = new uint[403];
            for (var l24 = 0x0u; l24 < 0x100_0000; l24++)
            {
                table[0] = h8 | l24;
                for (var i = 1u; i < table.Length; i++)
                    table[i] = 0x6C078965u * (table[i - 1] ^ (table[i - 1] >> 30)) + i;

                var ivsCode = 0u;
                for (var i = 0; i < 6; i++)
                {
                    var temp = (table[i] & 0x80000000) | (table[i + 1] & 0x7FFFFFFF);
                    var val = table[i + 397] ^ (temp >> 1);
                    if ((temp & 1) == 1) val ^= 0x9908b0df;

                    val ^= (val >> 11);
                    val ^= (val << 7) & 0x9d2c5680;
                    val ^= (val << 15) & 0xefc60000;
                    val ^= (val >> 18);

                    val >>= 27;

                    ivsCode |= val << (5 * i);
                }
                if (targets.ContainsKey(ivsCode)) WriteLine($"{targets[ivsCode]} {h8 | l24:X8}");

            }
        });
        WriteLine($"{DateTime.Now} 終了");
    }
    static void Search_SIMD128(IReadOnlyDictionary<uint, string> targets)
    {
        Vector128<uint> ONE = Vector128.Create(1u);

        Vector128<uint> MATRIX_A = Vector128.Create(0x9908b0dfu);
        Vector128<uint> UPPER_MASK = Vector128.Create(0x80000000u);
        Vector128<uint> LOWER_MASK = Vector128.Create(0x7fffffffu);

        Vector128<uint> MATRIX_TEMPER_1 = Vector128.Create(0x9d2c5680u);
        Vector128<uint> MATRIX_TEMPER_2 = Vector128.Create(0xefc60000u);

        Vector128<uint> under = Vector128.Create(0u, 1, 2, 3);
        Vector128<uint> ADD = Vector128.Create(4u);

        WriteLine($"{DateTime.Now} 開始");
        Parallel.For(0, 0xFF, h8 =>
        {
            var seed = Vector128.Create((uint)h8 << 24);
            var _stateVector = new Vector128<uint>[403];
            for (var _ = 0; _ < 0x100_0000 / 4; _++, seed += ADD)
            {
                _stateVector[0] = seed | under;
                for (uint i = 1; i < _stateVector.Length; i++)
                    _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector128.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector128.Create(i);

                var ivsCodes = Vector128<uint>.Zero;
                for (var k = 0; k < 6; k++)
                {
                    var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
                    var val = _stateVector[k + 397] ^ Vector128.ShiftRightLogical(temp, 1)
                        ^ (MATRIX_A * (temp & ONE));

                    val ^= Vector128.ShiftRightLogical(val, 11);
                    val ^= Vector128.ShiftLeft(val, 7) & MATRIX_TEMPER_1;
                    val ^= Vector128.ShiftLeft(val, 15) & MATRIX_TEMPER_2;
                    val ^= Vector128.ShiftRightLogical(val, 18);

                    ivsCodes |= Vector128.ShiftLeft(Vector128.ShiftRightLogical(val, 27), 5 * k);
                }

                for (int k = 0; k < Vector128<uint>.Count; k++)
                    if (targets.ContainsKey(ivsCodes[k])) WriteLine($"{targets[ivsCodes[k]]} {seed[k]:X8}");
            }
        });
        WriteLine($"{DateTime.Now} 終了");
    }
    static void Search_SIMD256(IReadOnlyDictionary<uint, string> targets)
    {
        Vector256<uint> ONE = Vector256.Create(1u);

        Vector256<uint> MATRIX_A = Vector256.Create(0x9908b0dfu);
        Vector256<uint> UPPER_MASK = Vector256.Create(0x80000000u);
        Vector256<uint> LOWER_MASK = Vector256.Create(0x7fffffffu);

        Vector256<uint> MATRIX_TEMPER_1 = Vector256.Create(0x9d2c5680u);
        Vector256<uint> MATRIX_TEMPER_2 = Vector256.Create(0xefc60000u);

        Vector256<uint> under = Vector256.Create(0u, 1, 2, 3, 4, 5, 6, 7);
        Vector256<uint> ADD = Vector256.Create(8u);

        WriteLine($"{DateTime.Now} 開始");
        Parallel.For(0, 0xFF, h8 =>
        {
            var seed = Vector256.Create((uint)h8 << 24);
            var _stateVector = new Vector256<uint>[403];
            for (var _ = 0; _ < 0x100_0000 / 8; _++, seed += ADD)
            {
                _stateVector[0] = seed | under;
                for (uint i = 1; i < _stateVector.Length; i++)
                    _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector256.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector256.Create(i);

                var ivsCodes = Vector256<uint>.Zero;
                for (var k = 0; k < 6; k++)
                {
                    var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
                    var val = _stateVector[k + 397] ^ Vector256.ShiftRightLogical(temp, 1)
                        ^ (MATRIX_A * (temp & ONE));

                    val ^= Vector256.ShiftRightLogical(val, 11);
                    val ^= Vector256.ShiftLeft(val, 7) & MATRIX_TEMPER_1;
                    val ^= Vector256.ShiftLeft(val, 15) & MATRIX_TEMPER_2;
                    val ^= Vector256.ShiftRightLogical(val, 18);

                    ivsCodes |= Vector256.ShiftLeft(Vector256.ShiftRightLogical(val, 27), 5 * k);
                }

                for (int k = 0; k < Vector256<uint>.Count; k++)
                    if (targets.ContainsKey(ivsCodes[k])) WriteLine($"{targets[ivsCodes[k]]} {seed[k]:X8}");
            }
        });
        WriteLine($"{DateTime.Now} 終了");
    }

}
