//-----------------------------------------------------------------------
// <copyright file="MoonPhaseUnitTests.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
    using System;
    using Xunit;

    /// <summary>
    /// Tests for the MoonPhase class
    /// </summary>
    public class MoonPhaseUnitTests
    {
        /// <summary>
        /// Check the new moon is calculated correctly
        /// </summary>
        [Fact]
        public void CheckNewMoonPhase()
        {
            MoonPhase.Phase expectedPhase = MoonPhase.Phase.NewMoon;
            MoonPhase.Phase actualPhase = MoonPhase.CalculateMoonPhase(new DateTime(2012, 12, 13));

            Assert.Equal<MoonPhase.Phase>(expectedPhase, actualPhase);
        }

        /// <summary>
        /// Check the full moon is calculated correctly
        /// </summary>
        [Fact]
        public void CheckFullMoonPhase()
        {
            MoonPhase.Phase expectedPhase = MoonPhase.Phase.FullMoon;
            MoonPhase.Phase actualPhase = MoonPhase.CalculateMoonPhase(new DateTime(2012, 12, 27));

            Assert.Equal<MoonPhase.Phase>(expectedPhase, actualPhase);
        }
    }
}
