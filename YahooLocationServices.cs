//-----------------------------------------------------------------------
// <copyright file="YahooLocationServices.cs" company="Brave Location">
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
    /// Static class to handle using Yahoo location service
    /// </summary>
    public static class YahooLocationServices
    {
        /// <summary>
        /// API Key to use for Yahoo Location Services
        /// N.B. If reusing this code, please use your own API key from http://developer.apps.yahoo.com/
        /// </summary>
        private static string yahooApiKey = "1auJbv6k";

        /// <summary>
        /// Comma splitter
        /// </summary>
        private static char[] commaSplitter = { ',' };

        /// <summary>
        /// String to use to format Uri using WOE ID
        /// </summary>
        private static string yahooLocationFormattedUri = "http://where.yahooapis.com/geocode?q={0:0.00000000000000},+{1:0.00000000000000}&gflags=R&flags=G&appid={2}";

        /// <summary>
        /// Formats the correct Url to fetch address using current location
        /// </summary>
        /// <param name="currentLocation">Current location</param>
        /// <returns>Uri to call to fetch weather</returns>
        public static Uri YahooLocationUri(GeoCoordinate currentLocation)
        {
            string locationUrl = string.Format(CultureInfo.InvariantCulture, YahooLocationServices.yahooLocationFormattedUri, currentLocation.Latitude, currentLocation.Longitude, YahooLocationServices.yahooApiKey);

            return new Uri(locationUrl);
        }

        /// <summary>
        /// Parses the address from the XML returned from Yahoo!
        /// </summary>
        /// <param name="locationXml">Location XML to parse</param>
        /// <returns>The address in the XML, or empty string if not found or on an error</returns>
        public static string ParseAddress(string locationXml)
        {
            // Parse the Yahoo XML returned
            // See documentation at http://developer.yahoo.com/geo/placefinder/guide/
            XElement rootElement;
            try
            {
                rootElement = XElement.Parse(locationXml);
            }
            catch (XmlException)
            {
                return StringResources.LocationNotFoundText;
            }

            if (rootElement == null)
            {
                return StringResources.LocationNotFoundText;
            }

            // Check for error code
            XElement errorCode = rootElement.Element("Error");
            if (errorCode == null || errorCode.Value != "0")
            {
                return StringResources.LocationNotFoundText;
            }

            XElement result = rootElement.Element("Result");
            if (result != null)
            {
                XElement line1 = result.Element("line1");
                XElement line2 = result.Element("line2");
                XElement line3 = result.Element("line3");
                XElement line4 = result.Element("line4");

                StringBuilder address = new StringBuilder();
                address.Append(YahooLocationServices.ParseLocationElement(line1));
                address.Append(" " + YahooLocationServices.ParseLocationElement(line2));
                address.Append(" " + YahooLocationServices.ParseLocationElement(line3));
                address.Append(" " + YahooLocationServices.ParseLocationElement(line4));

                return address.ToString().Trim();
            }

            return StringResources.LocationNotFoundText;
        }

        /// <summary>
        /// Parse the WOE ID from the XML returned from Yahoo!
        /// </summary>
        /// <param name="locationXml">Location XML to parse</param>
        /// <returns>WOE ID, or empty string if not found or on an error</returns>
        public static string ParseWoeId(string locationXml)
        {
            XElement rootElement;
            try
            {
                rootElement = XElement.Parse(locationXml);
            }
            catch (XmlException)
            {
                return string.Empty;
            }

            if (rootElement == null)
            {
                return string.Empty;
            }

            // Check for error code
            XElement errorCode = rootElement.Element("Error");
            if (errorCode == null || errorCode.Value != "0")
            {
                return string.Empty;
            }

            XElement result = rootElement.Element("Result");
            if (result != null)
            {
                // Pick up the WOE ID and lookup weather
                XElement woeid = result.Element("woeid");
                return YahooLocationServices.ParseLocationElement(woeid);
            }

            return string.Empty;
        }

        /// <summary>
        /// Parses a location element for valid values
        /// </summary>
        /// <param name="line">Line element</param>
        /// <returns>String or empty</returns>
        private static string ParseLocationElement(XElement line)
        {
            if (line == null)
            {
                return string.Empty;
            }

            // Don't show if values are actually lat/long values
            double parsedLine = double.MinValue;
            string[] parts = line.Value.Split(YahooLocationServices.commaSplitter, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && double.TryParse(parts[0], out parsedLine) && double.TryParse(parts[1], out parsedLine))
            {
                return string.Empty;
            }

            return line.Value;
        }
    }
}
