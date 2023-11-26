using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _5genInitialSeedSearch
{
    class Time
    {
        public uint Hour { get; private set; }
        public uint Minute { get; private set; }
        public uint Second { get; private set; }
        
        private readonly uint minHour;
        private readonly uint maxHour;
        private readonly uint minMinute;
        private readonly uint maxMinute;
        private readonly uint minSecond;
        private readonly uint maxSecond;

        public bool Advance()
        {
            Second++;
            if(Second == maxSecond)
            {
                Second = minSecond;
                Minute++;
                if(Minute == maxMinute)
                {
                    Minute = minMinute;
                    Hour++;
                    if(Hour == maxHour)
                    {
                        Hour = minHour;
                        return true;
                    }
                }
            }
            return false;
        }
        public uint GetCode()
        {
            return ((12 <= Hour ? 1u : 0) << 30) | ((Hour / 10) << 28) | ((Hour % 10) << 24) | ((Minute / 10) << 20) | ((Minute % 10) << 16) | ((Second / 10) << 12) | ((Second % 10) << 8);
        }
        public string GetTime()
        {
            return $"{Hour}:{Minute}.{Second}";
        }

        public Time(uint h, uint m, uint s, uint minH = 0, uint maxH = 23, uint minM = 0, uint maxM = 59, uint minS = 0, uint maxS = 59)
        {
            if (h > 23) h = 23;
            if (m > 59) m = 59;
            if (s > 59) s = 59;

            Hour = h;
            Minute = m;
            Second = s;

            minHour = minH;
            maxHour = maxH+1;
            minMinute = minM;
            maxMinute = maxM+1;
            minSecond = minS;
            maxSecond = maxS+1;
        }
    }
}
