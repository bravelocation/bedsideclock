using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace com.bravelocation.bedsideClock
{
    public class MoonPhase
    {
        public enum Phase
        {
            NewMoon = 0,
            WaxingCrescentMoon = 1,
            QuarterMoon = 2,
            WaxingGibbousMoon = 3,
            FullMoon = 4,
            WaningGibbousMoon = 5,
            LastQuarterMoon = 6,
            WaningCrescentMoon = 7,
        }

        /// <summary>
        /// Calculates current phase of the moon using algorithm in http://www.voidware.com/moon_phase.htm
        /// </summary>
        /// <param name="currentDate">Current date</param>
        /// <returns>Phase of moon</returns>
        public static Phase CalculateMoonPhase(DateTime currentDate)
        {
            int currentYear = currentDate.Year;
            int currentMonth = currentDate.Month;
            int currentDay = currentDate.Day;

            if (currentMonth < 3)
            {
                currentYear--;
                currentMonth += 12;
            }

            ++currentMonth;
            int c = (int) (365.25 * currentYear);
            int e = (int) (30.6 * currentMonth);
            double jd = c + e + currentDay - 694039.09;  // jd is total days elapsed
            jd /= 29.53;                                 // divide by the moon cycle (29.53 days)
            int b = (int) jd;		                     // int(jd) -> b, take integer part of jd
            jd -= b;		                             // subtract integer part to leave fractional part of original jd
            b = (int) ((jd * 8) + 0.5);	                 // scale fraction from 0-8 and round by adding 0.5
            b = b % 8;		                             // 0 and 8 are the same so turn 8 into 0
            return (Phase) b;
        }
    }
}
