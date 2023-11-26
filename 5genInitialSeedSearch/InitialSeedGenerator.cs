using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5genInitialSeedSearch
{
    class InitialSeedGenerator
    {
        private readonly uint[] W = new uint[80];
        public InitialSeedGenerator(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
        {
            W[0] = toLittleEndian(nazo[0]);
            W[1] = toLittleEndian(nazo[1]);
            W[2] = toLittleEndian(nazo[2]);
            W[3] = toLittleEndian(nazo[3]);
            W[4] = toLittleEndian(nazo[4]);

            W[5] = toLittleEndian((v << 16) | t0);
            W[6] = (mac[4] << 8) | mac[5];
            W[7] = toLittleEndian(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0]));

            W[10] = 0x00000000;
            W[11] = 0x00000000;
            W[12] = 0xFF2F0000;
            W[13] = 0x80000000;
            W[14] = 0x00000000;
            W[15] = 0x000001A0;
        }

        public uint GenerateMTSeed(uint dateCode, uint timeCode)
        {
            W[8] = dateCode;
            W[9] = timeCode;

            for (int t = 16; t < 80; t++)
            {
                var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                W[t] = (w << 1) | (w >> 31);
            }

            const uint H0 = 0x67452301;
            const uint H1 = 0xEFCDAB89;
            const uint H2 = 0x98BADCFE;
            const uint H3 = 0x10325476;
            const uint H4 = 0xC3D2E1F0;

            uint A, B, C, D, E;
            A = H0; B = H1; C = H2; D = H3; E = H4;

            for (int t = 0; t < 20; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + ((B & C) | ((~B) & D)) + E + W[t] + 0x5A827999;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }
            for (int t = 20; t < 40; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0x6ED9EBA1;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }
            for (int t = 40; t < 60; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + ((B & C) | (B & D) | (C & D)) + E + W[t] + 0x8F1BBCDC;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }
            for (int t = 60; t < 80; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0xCA62C1D6;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }

            return (uint)(((((ulong)toLittleEndian(H1 + B) << 32) | toLittleEndian(H0 + A)) * 0x5D588B656C078965UL + 0x269EC3UL) >> 32);
        }
        public ulong Generate(uint dateCode, uint timeCode)
        {
            W[8] = dateCode;
            W[9] = timeCode;

            uint t;
            for (t = 16; t < 80; t++)
            {
                var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                W[t] = (w << 1) | (w >> 31);
            }

            const uint H0 = 0x67452301;
            const uint H1 = 0xEFCDAB89;
            const uint H2 = 0x98BADCFE;
            const uint H3 = 0x10325476;
            const uint H4 = 0xC3D2E1F0;

            uint A, B, C, D, E;
            A = H0; B = H1; C = H2; D = H3; E = H4;

            for (t = 0; t < 20; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + ((B & C) | ((~B) & D)) + E + W[t] + 0x5A827999;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }
            for (; t < 40; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0x6ED9EBA1;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }
            for (; t < 60; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + ((B & C) | (B & D) | (C & D)) + E + W[t] + 0x8F1BBCDC;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }
            for (; t < 80; t++)
            {
                var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0xCA62C1D6;
                E = D;
                D = C;
                C = (B << 30) | (B >> 2);
                B = A;
                A = temp;
            }

            ulong seed = toLittleEndian(H1 + B);
            seed <<= 32;
            seed |= toLittleEndian(H0 + A);

            return seed * 0x5D588B656C078965UL + 0x269EC3UL;
        }
        
        private uint toLittleEndian(uint val) { return ((val & 0xff) << 24) | (((val >> 8) & 0xff) << 16) | (((val >> 16) & 0xff) << 8) | ((val >> 24) & 0xff); }
    }
}
