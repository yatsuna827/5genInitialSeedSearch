using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5genInitialSeedSearch
{
    class Date
    {
        public uint YY { get; private set; }
        public uint MM { get; private set; }
        public uint DD { get; private set; }
        public uint Day { get; private set; }

        private static readonly uint[] days = new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 };
        private uint LastDay => (MM == 2 && YY % 4 == 0) ? days[MM] + 1 : days[MM]; // yyは2000 ~ 2099なので, 閏年は4の倍数かどうかだけの判定でよい.
        public bool Advance()
        {
            DD++;
            Day++; if (Day == 7) Day = 0;
            if (DD == LastDay)
            {
                DD = 1;
                MM++;
                if (MM == 13)
                {
                    MM = 1;
                    return true;
                }
            }
            return false;
        }
        public uint GetCode()
        {
            return ((YY / 10) << 28) | ((YY % 10) << 24) | ((MM / 10) << 20) | ((MM % 10) << 16) | ((DD / 10) << 12) | ((DD % 10) << 8) | Day;
        }
        public string GetDate()
        {
            return $"20{YY:d2}/{MM:d2}/{DD:d2}";
        }
        public Date(uint yy, uint mm, uint dd)
        {
            YY = yy;
            MM = mm;
            DD = dd;

            var year = yy + 2000;
            if (mm < 3) { year--; mm += 12; }
            Day = (year + (year / 4) - (year / 100) + (year / 400) + ((13 * mm + 8) / 5) + dd) % 7;
        }
    }
}
