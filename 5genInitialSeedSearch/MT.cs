using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _5genInitialSeedSearch
{
    class MT
    {
        public static uint GetBWIVsCode(uint seed)
        {
            var table = new uint[403];
            table[0] = seed;
            for (uint i = 1; i < 403; i++)
                table[i] = 0x6C078965u * (table[i - 1] ^ (table[i - 1] >> 30)) + i;

            uint ivsCode = 0;
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

            return ivsCode;
        }
        public static uint GetMERARUBAIVsCode(uint seed)
        {
            var table = new uint[404];
            table[0] = seed;
            for (uint i = 1; i < 404; i++)
                table[i] = 0x6C078965u * (table[i - 1] ^ (table[i - 1] >> 30)) + i;

            uint ivsCode = 0;
            for (var i = 1; i < 7; i++)
            {
                var temp = (table[i] & 0x80000000) | (table[i + 1] & 0x7FFFFFFF);
                var val = table[i + 397] ^ (temp >> 1);
                if ((temp & 1) == 1) val ^= 0x9908b0df;

                val ^= (val >> 11);
                val ^= (val << 7) & 0x9d2c5680;
                val ^= (val << 15) & 0xefc60000;
                val ^= (val >> 18);

                val >>= 27;

                ivsCode |= val << (5 * (i-1));
            }

            return ivsCode;
        }
        public static uint GetBW2IVsCode(uint seed)
        {
            var table = new uint[405];
            table[0] = seed;
            for (uint i = 1; i < 405; i++)
                table[i] = 0x6C078965u * (table[i - 1] ^ (table[i - 1] >> 30)) + i;

            uint ivsCode = 0;
            for (var i = 2; i < 8; i++)
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

            return ivsCode;
        }
    }
}
