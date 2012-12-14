//-----------------------------------------------------------------------
// <copyright file="YahooWeather.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
    using System;
    using System.Collections.Generic;
    using System.Device.Location;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Handles the fetching and parsing of data from Yahoo Weather
    /// </summary>
    public class YahooWeather
    {
        /// <summary>
        /// String to use to format Uri using WOE ID
        /// </summary>
        private static string yahooWeatherWOEIDFormattedUri = "http://weather.yahooapis.com/forecastrss?w={0}";

        /// <summary>
        /// Formats the correct Url to fetch weather using WOE ID
        /// </summary>
        /// <param name="currentWoeId">Current WOE ID</param>
        /// <returns>Uri to call to fetch weather</returns>
        public static Uri YahooWeatherUri(string currentWoeId)
        {
            string weatherUrl = string.Format(CultureInfo.InvariantCulture, YahooWeather.yahooWeatherWOEIDFormattedUri, currentWoeId);

            return new Uri(weatherUrl);
        }

        /// <summary>
        /// Parses the weather forecast code from the Yahoo! weather XML
        /// </summary>
        /// <param name="weatherXml">XML to be parsed</param>
        /// <param name="currentLocation">Current location</param>
        /// <returns>Weather code</returns>
        public static int ParseWeather(string weatherXml, GeoCoordinate currentLocation)
        {
            XElement rootElement;
            try
            {
                rootElement = XElement.Parse(weatherXml);
            }
            catch (XmlException)
            {
                return -1;
            }

            string weatherForecastDay = YahooWeather.WeatherForecastDay(currentLocation);

            if (rootElement != null)
            {
                XElement channel = rootElement.Element("channel");
                if (channel != null)
                {
                    XElement item = channel.Element("item");
                    if (item != null)
                    {
                        // Find the correct forecast node depending on time
                        foreach (XElement forecast in item.Descendants())
                        {
                            if (forecast.Name != null && forecast.Name.LocalName == "forecast")
                            {
                                // If correctly today or tomorrow, depending on time, set image as code
                                XAttribute forecastDay = forecast.Attribute("day");
                                if (forecastDay != null && forecastDay.Value == weatherForecastDay)
                                {
                                    // Use the code to set the image
                                    XAttribute forecastCode = forecast.Attribute("code");
                                    if (forecastCode != null)
                                    {
                                        return int.Parse(forecastCode.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Turns weather code into logical image
        /// </summary>
        /// <param name="code">Yahoo! Weather code</param>
        /// <returns>String referencing image to use for weather</returns>
        public static string MapWeatherCodeToImage(int code)
        {
            switch (code)
            {
                case 9:
                case 11:
                case 12:
                case 40:
                    return "cloudy-with-rain";
                case 23:
                case 24:
                case 29:
                case 30:
                case 33:
                case 34:
                case 44:
                    return "cloudy-with-sunshine";
                case 20:
                case 22:
                case 26:
                case 27:
                case 28:
                    return "cloudy";
                case 5:
                case 6:
                case 7:
                case 8:
                case 13:
                case 14:
                case 18:
                case 46:
                    return "light-snow";
                case 10:
                case 15:
                case 16:
                case 43:
                    return "heavy-snow";
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 17:
                case 19:
                case 35:
                case 37:
                case 38:
                case 39:
                case 45:
                case 47:
                    return "storm";
                case 21:
                case 25:
                case 31:
                case 32:
                case 36:
                    return "sunny";
                default:
                    return "blank";
            }
        }

        /// <summary>
        /// Calculates the day of the week to use for the forecast
        /// </summary>
        /// <param name="currentLocation">Current location</param>
        /// <returns>String matching values from http://developer.yahoo.com/weather/ </returns>
        private static string WeatherForecastDay(GeoCoordinate currentLocation)
        {
            DayOfWeek forecastDayOfWeek = DateTime.Now.DayOfWeek;

            // If after sunset, show tomorrow's weather
            DateTime sunset = SunriseCalculator.SunriseAndSunset.Sunset(currentLocation.Latitude, currentLocation.Longitude, DateTime.Now);

            if (DateTime.Now.Hour >= sunset.Hour)
            {
                if (forecastDayOfWeek == DayOfWeek.Saturday)
                {
                    forecastDayOfWeek = DayOfWeek.Sunday;
                }
                else
                {
                    forecastDayOfWeek = forecastDayOfWeek + 1;
                }
            }

            // Now convert to Yahoo string like {Mon Tue Wed Thu Fri Sat Sun}
            switch (forecastDayOfWeek)
            {
                case DayOfWeek.Monday: return "Mon";
                case DayOfWeek.Tuesday: return "Tue";
                case DayOfWeek.Wednesday: return "Wed";
                case DayOfWeek.Thursday: return "Thu";
                case DayOfWeek.Friday: return "Fri";
                case DayOfWeek.Saturday: return "Sat";
                case DayOfWeek.Sunday: return "Sun";
            }

            // Logically cannot get here!
            return string.Empty;
        }
    }
}
