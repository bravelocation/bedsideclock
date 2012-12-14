//-----------------------------------------------------------------------
// <copyright file="MoonPhase.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
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

    /// <summary>
    /// Class used to calculate the phase of the moon
    /// </summary>
    public class MoonPhase
    {
        /// <summary>
        /// Phases of the moon
        /// </summary>
        public enum Phase
        {
            /// <summary>New moon</summary>
            NewMoon = 0,

            /// <summary>Waxing crescent moon</summary>
            WaxingCrescentMoon = 1,

            /// <summary>Quarter moon</summary>
            QuarterMoon = 2,

            /// <summary>Waxing gibbous moon</summary>
            WaxingGibbousMoon = 3,

            /// <summary>Full moon</summary>
            FullMoon = 4,

            /// <summary>Waning gibbous moon</summary>
            WaningGibbousMoon = 5,

            /// <summary>Last quarter moon</summary>
            LastQuarterMoon = 6,

            /// <summary>Waning crescent moon</summary>
            WaningCrescentMoon = 7,
        }

        /// <summary>
        /// Calculates current phase of the moon
        /// </summary>
        /// <param name="currentDate">Current date</param>
        /// <returns>Phase of moon</returns>
        public static Phase CalculateMoonPhase(DateTime currentDate)
        {
            // Using algorithm in http://www.voidware.com/moon_phase.htm
            int currentYear = currentDate.Year;
            int currentMonth = currentDate.Month;
            int currentDay = currentDate.Day;

            if (currentMonth < 3)
            {
                currentYear--;
                currentMonth += 12;
            }

            ++currentMonth;
            int c = (int)(365.25 * currentYear);
            int e = (int)(30.6 * currentMonth);
            double jd = c + e + currentDay - 694039.09;  // jd is total days elapsed
            jd /= 29.53;                                 // divide by the moon cycle (29.53 days)
            int b = (int)jd;                             // int(jd) -> b, take integer part of jd
            jd -= b;                                     // subtract integer part to leave fractional part of original jd
            b = (int)((jd * 8) + 0.5);                   // scale fraction from 0-8 and round by adding 0.5
            b = b % 8;                                   // 0 and 8 are the same so turn 8 into 0
            return (Phase)b;
        }
    }
}
