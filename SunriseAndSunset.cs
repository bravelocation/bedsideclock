using System;

namespace com.bravelocation.SunriseCalculator
{
    /// <summary>
    /// Class to calculate sunrise and sunset on any day at any location
    /// </summary>
    public class SunriseAndSunset
    {
        /// <summary>
        /// Enumeration of types to calculate
        /// </summary>
        private enum TransitType
        {
            Sunrise,
            Sunset
        }

        /// <summary>
        /// Calculates sunrise today
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>Time of sunrise today</returns>
        public static DateTime Sunrise(double latitude, double longitude)
        {
            return SunriseAndSunset.TransitTime(latitude, longitude, DateTime.Now, TransitType.Sunrise);
        }

        /// <summary>
        /// Calculates sunrise on day of given time
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="currentTime">Current time</param>
        /// <returns>Time of sunrise on given day</returns>
        public static DateTime Sunrise(double latitude, double longitude, DateTime currentTime)
        {
            return SunriseAndSunset.TransitTime(latitude, longitude, currentTime, TransitType.Sunrise);
        }

        /// <summary>
        /// Calculates sunset today
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>Time of sunset today</returns>
        public static DateTime Sunset(double latitude, double longitude)
        {
            return SunriseAndSunset.TransitTime(latitude, longitude, DateTime.Now, TransitType.Sunset);
        }

        /// <summary>
        /// Calculates sunset on day of given time
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="currentTime">Current time</param>
        /// <returns>Time of sunset on given day</returns>
        public static DateTime Sunset(double latitude, double longitude, DateTime currentTime)
        {
            return SunriseAndSunset.TransitTime(latitude, longitude, currentTime, TransitType.Sunset);
        }

        /// <summary>
        /// Method that calculates the actual time of transit
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <param name="currentTime">Day of calculation</param>
        /// <param name="type">Transit type</param>
        /// <returns>Time of transit</returns>
        private static DateTime TransitTime(double latitude, double longitude, DateTime currentTime, TransitType type)
        {
            // See algorithm from http://williams.best.vwh.net/sunrise_sunset_algorithm.htm
 
            // 1. first calculate the day of the year
            int dayOfYear = currentTime.DayOfYear;

            // 2. convert the longitude to hour value and calculate an approximate time
	        int longitudeHour = (int) Math.Round(longitude / 15);
	
            int approxTime = 0;
            if (type == TransitType.Sunrise)
            {
                approxTime = dayOfYear + ((6 - longitudeHour) / 24);
            }
            else
            {
                approxTime = dayOfYear + ((18 - longitudeHour) / 24);
            }

            // 3. calculate the Sun's mean anomaly
            double meanAnomoly = (0.9856 * approxTime) - 3.289;

            // 4. calculate the Sun's true longitude
            double sunLongitude = meanAnomoly + (1.916 * Math.Sin(DegreesToRadians(meanAnomoly))) + (0.020 * Math.Sin(DegreesToRadians(2 * meanAnomoly))) + 282.634;
            sunLongitude = sunLongitude % 360.0;

            // 5a. calculate the Sun's right ascension
            double rightAscension = RadiansToDegrees(Math.Atan(0.91764 * Math.Tan(DegreesToRadians(sunLongitude)))) % 360.0;

            // 5b. right ascension value needs to be in the same quadrant as L
            double longitudeQuadrant = (Math.Floor(sunLongitude / 90)) * 90;
            double RAQuadrant = (Math.Floor(rightAscension / 90)) * 90;

            rightAscension = rightAscension + (longitudeQuadrant - RAQuadrant);

            // 5c. right ascension value needs to be converted into hours
            rightAscension = rightAscension / 15;

            // 6. calculate the Sun's declination
            double sinDec = 0.39782 * Math.Sin(DegreesToRadians(sunLongitude));
            double cosDec = Math.Cos(Math.Asin(sinDec));

            // 7a. calculate the Sun's local hour angle
            double zenith = 90.87;
            double cosH = (Math.Cos(DegreesToRadians(zenith)) - (sinDec * Math.Sin(DegreesToRadians(latitude)))) / (cosDec * Math.Cos(DegreesToRadians(latitude)));

            // handle where sun never sets or rises
            if (cosH > 1 || cosH < -1)
            {
                return ConvertOffsetToDateTime(currentTime, 0);
            }

            // 7b. finish calculating H and convert into hours
            double hours = 0.0;

            if (type == TransitType.Sunrise)
            {
                hours = 360.0 - RadiansToDegrees(Math.Acos(cosH));
            }
            else
            {
                hours = RadiansToDegrees(Math.Acos(cosH));
            }

            hours = hours / 15.0;

            // 8. calculate local mean time of rising/setting
            double time = hours + rightAscension - (0.06571 * approxTime) - 6.622;

            // 9. adjust back to UTC
            double universalTime = (time - longitudeHour) % 24.0;

            return ConvertOffsetToDateTime(currentTime, universalTime);
        }

        /// <summary>
        /// Calculates the offset to current date so time is correct
        /// </summary>
        /// <param name="original">original time</param>
        /// <param name="offsetHours">Offset hours</param>
        /// <returns>Converted time</returns>
        private static DateTime ConvertOffsetToDateTime(DateTime original, double offsetHours)
        {
            double hours = Math.Floor(offsetHours);

            double fractional = (offsetHours - hours) * 60;
            double minutes = Math.Floor(fractional);

            fractional = (fractional - minutes) * 60;
            double seconds = Math.Floor(fractional);

            if (hours < 0)
            {
                hours += 24.0;
            }

            
            DateTime thisDate = DateTime.SpecifyKind(new DateTime(original.Year, original.Month, original.Day, (int)hours, (int)minutes, (int)seconds), DateTimeKind.Utc);
            return thisDate.ToLocalTime();
        }

        /// <summary>
        /// Converts degress to radians
        /// </summary>
        /// <param name="degrees">Number of degrees</param>
        /// <returns>Converted value</returns>
        private static double DegreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="radians">Radians to convert</param>
        /// <returns>Converted value</returns>
        private static double RadiansToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }
    }
}
