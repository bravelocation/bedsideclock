//-----------------------------------------------------------------------
// <copyright file="SunriseAndSunsetUnitTests.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
    using System;
    using Com.BraveLocation.SunriseCalculator;
    using Xunit;

    /// <summary>
    /// Tests for sunrise and sunset calculations
    /// </summary>
    public class SunriseAndSunsetUnitTests
    {
        /// <summary>
        /// Checks the degrees to radians conversion
        /// </summary>
        [Fact]
        public void SimpleDegreesToRadiansCheck()
        {
            double expected = Math.PI / 6.0;
            double actual = SunriseAndSunset.DegreesToRadians(30.0);

            Assert.Equal<double>(expected, actual);
        }

        /// <summary>
        /// Checks the radians to degrees conversion
        /// </summary>
        [Fact]
        public void SimpleRadiansToDegreesCheck()
        {
            double expected = 90.0;
            double actual = SunriseAndSunset.RadiansToDegrees(Math.PI / 2.0);

            Assert.Equal<double>(expected, actual);
        }

        /// <summary>
        /// Checks the radians to degrees conversion and back
        /// </summary>
        [Fact]
        public void RadiansToDegreesAndBack()
        {
            double expected = Math.PI / 2.0;
            double actual = SunriseAndSunset.DegreesToRadians(SunriseAndSunset.RadiansToDegrees(Math.PI / 2.0));

            Assert.Equal<double>(expected, actual);
        }

        /// <summary>
        /// Checks simple calculation of date time offset
        /// </summary>
        [Fact]
        public void ConvertOffsetToDateTimeSimple()
        {
            DateTime thisDay = new DateTime(2012, 12, 13);

            DateTime expected = new DateTime(2012, 12, 13, 10, 30, 0, DateTimeKind.Utc);
            DateTime actual = SunriseAndSunset.ConvertOffsetToDateTime(thisDay, 10.5);

            Assert.Equal<DateTime>(expected, actual);
        }

        /// <summary>
        /// Checks calculation of date time negative offset
        /// </summary>
        [Fact]
        public void ConvertOffsetToDateTimeNegativeOffset()
        {
            DateTime thisDay = new DateTime(2012, 12, 13);

            DateTime expected = new DateTime(2012, 12, 13, 18, 45, 0, DateTimeKind.Utc);
            DateTime actual = SunriseAndSunset.ConvertOffsetToDateTime(thisDay, -5.25);

            Assert.Equal<DateTime>(expected, actual);
        }

        /// <summary>
        /// Checks calculation of sunrise in London in winter
        /// </summary>
        [Fact]
        public void SunriseLondonWinter()
        {
            DateTime thisDay = new DateTime(2012, 12, 13);

            DateTime expected = new DateTime(2012, 12, 13, 08, 09, 42, DateTimeKind.Utc);
            DateTime actual = SunriseAndSunset.Sunrise(53.79415893554690, -1.54840183258057, thisDay); // London in winter!

            Assert.Equal<DateTime>(expected, actual);
        }

        /// <summary>
        /// Checks calculation of sunrise in London in summer
        /// </summary>
        [Fact]
        public void SunriseLondonSummer()
        {
            DateTime thisDay = new DateTime(2012, 6, 13);

            DateTime expected = new DateTime(2012, 6, 13, 04, 28, 24, DateTimeKind.Utc);
            DateTime actual = SunriseAndSunset.Sunrise(53.79415893554690, -1.54840183258057, thisDay); // London in summer!

            Assert.Equal<DateTime>(expected, actual);
        }

        /// <summary>
        /// Checks calculation of sunset in London in winter
        /// </summary>
        [Fact]
        public void SunsetLondonWinter()
        {
            DateTime thisDay = new DateTime(2012, 12, 13);

            DateTime expected = new DateTime(2012, 12, 13, 15, 38, 57, DateTimeKind.Utc);
            DateTime actual = SunriseAndSunset.Sunset(53.79415893554690, -1.54840183258057, thisDay); // London in winter!

            Assert.Equal<DateTime>(expected, actual);
        }

        /// <summary>
        /// Checks calculation of sunrise in London in summer
        /// </summary>
        [Fact]
        public void SunsetLondonSummer()
        {
            DateTime thisDay = new DateTime(2012, 6, 13);

            DateTime expected = new DateTime(2012, 6, 13, 21, 31, 43, DateTimeKind.Utc);
            DateTime actual = SunriseAndSunset.Sunset(53.79415893554690, -1.54840183258057, thisDay); // London in summer!

            Assert.Equal<DateTime>(expected, actual);
        }
    }
}
