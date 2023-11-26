using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5genInitialSeedSearch
{
    enum ROM
    {
        B1JP, W1JP, B2JP, W2JP,
        B1US, W1US, B2US, W2US,
        B1FR, W1FR, B2FR, W2FR,
        B1GE, W1GE, B2GE, W2GE,
        B1SP, W1SP, B2SP, W2SP,
        B1IT, W1IT, B2IT, W2IT,
        B1KO, W1KO, B2KO, W2KO,
    }
    static class ConstantParameters
    {
        public static uint[] VCount = new uint[]
        {
            0x60, 0x5f, 0x82, 0x82
        };
        public static uint[][] timer0 = new uint[][]
        {
            new uint[] { 0xc79, 0xc7a },
            new uint[] { 0xc68, 0xc69 },
            new uint[] { 0x1102, 0x1103, 0x1104, 0x1105, 0x1106, 0x1107, 0x1108 },
            new uint[] { 0x10F4, 0x10F5, 0x10F6, 0x10F7, 0x10F8, 0x10F9, 0x10FA, 0x10FB }
        };
        public static readonly uint[][] nazo = new uint[][]
        {
            // JP
            new uint[] { 0x2215f10, 0x221600C, 0x221600C, 0x2216058, 0x2216058 },
            new uint[] { 0x2215f30, 0x221602C, 0x221602C, 0x2216078, 0x2216078 },
            new uint[] { 0x209A8DC, 0x2039AC9, 0x21FF9B0, 0x21FFA04, 0x21FFA04 },
            new uint[] { 0x209A8FC, 0x2039AF5, 0x21FF9D0, 0x21FFA24, 0x21FFA24 },

            // US
            new uint[] { 0x22160B0, 0x22161AC, 0x22161AC, 0x22161F8, 0x22161F8 }, // ちゃんと加算する
            new uint[] { 0x22160D0, 0x22161CC, 0x22161CC, 0x2216218, 0x2216218 },
            new uint[] { 0x209AEE8, 0x2039DE9, 0x2200010, 0x2200064, 0x2200064 },
            new uint[] { 0x209AF28, 0x2039E15, 0x2200050, 0x22000A4, 0x22000A4 },

            // FR
            new uint[] { 0x2216030, 0x221612C, 0x221612C, 0x2216178, 0x2216178 },
            new uint[] { 0x2216050, 0x221614C, 0x221614C, 0x2216198, 0x2216198 },
            new uint[] { 0x209AF08, 0x2039DF9, 0x2200030, 0x2200084, 0x2200084 },
            new uint[] { 0x209AF28, 0x2039E25, 0x2200050, 0x22000A4, 0x22000A4 },

            // GE
            new uint[] { 0x2215FF0, 0x22160EC, 0x22160EC, 0x2216138, 0x2216138 },
            new uint[] { 0x2216010, 0x221610C, 0x221610C, 0x2216158, 0x2216158 },
            new uint[] { 0x209AE28, 0x2039D69, 0x21FFF50, 0x21FFFA4, 0x21FFFA4 },
            new uint[] { 0x209AE48, 0x2039D95, 0x21FFF70, 0x21FFFC4, 0x21FFFC4 },

            // SP
            new uint[] { 0x2216050, 0x221614C, 0x221614C, 0x2216198, 0x2216198 },
            new uint[] { 0x2216070, 0x221616C, 0x221616C, 0x22161B8, 0x22161B8 },
            new uint[] { 0x209AEA8, 0x2039DB9, 0x21FFFD0, 0x2200024, 0x2200024 },
            new uint[] { 0x209AEC8, 0x2039DE5, 0x21FFFF0, 0x2200044, 0x2200044 },

            // IT
            new uint[] { 0x2215FB0, 0x22160AC, 0x22160AC, 0x22160F8, 0x22160F8 },
            new uint[] { 0x2215FD0, 0x22160CC, 0x22160CC, 0x2216118, 0x2216118 },
            new uint[] { 0x209ADE8, 0x2039D69, 0x21FFF10, 0x21FFF64, 0x21FFF64 },
            new uint[] { 0x209AE28, 0x2039D95, 0x21FFF50, 0x21FFFA4, 0x21FFFA4 },

            // KO
            new uint[] { 0x22167B0, 0x22168AC, 0x22168AC, 0x22168F8, 0x22168F8 },
            new uint[] { 0x22167B0, 0x22168AC, 0x22168AC, 0x22168F8, 0x22168F8 },
            new uint[] { 0x209B60C, 0x203A4D5, 0x2200750, 0x22007A4, 0x22007A4 },
            new uint[] { 0x209B62C, 0x203A501, 0x2200770, 0x22007C4, 0x22007C4 },
        };
    }
}
