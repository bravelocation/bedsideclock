using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace com.bravelocation.bedsideClock
{
    /// <summary>
    /// Static class to handle using Yahoo location service
    /// </summary>
    public static class YahooLocationServices
    {
        /// <summary>
        /// Comma splitter
        /// </summary>
        private static char[] CommaSplitter = { ',' };

        /// <summary>
        /// String to use to format Uri using WOE ID
        /// </summary>
        private static string YahooLocationFormattedUri = "http://where.yahooapis.com/geocode?q={0:0.00000000000000},+{1:0.00000000000000}&gflags=R&flags=G&appid=1auJbv6k";

        /// <summary>
        /// Formats the correct Url to fetch address using current location
        /// </summary>
        /// <param name="currentLocation">Current location</param>
        /// <returns>Uri to call to fetch weather</returns>
        public static Uri YahooLocationUri(GeoCoordinate currentLocation)
        {
            string locationUrl = String.Format(CultureInfo.InvariantCulture, YahooLocationServices.YahooLocationFormattedUri, currentLocation.Latitude, currentLocation.Longitude);

            return new Uri(locationUrl);
        }

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

        public static string ParseWoeId(string locationXml)
        {
            XElement rootElement;
            try
            {
                rootElement = XElement.Parse(locationXml);
            }
            catch (XmlException)
            {
                return String.Empty;
            }

            if (rootElement == null)
            {
                return String.Empty;
            }

            // Check for error code
            XElement errorCode = rootElement.Element("Error");
            if (errorCode == null || errorCode.Value != "0")
            {
                return String.Empty;
            }

            XElement result = rootElement.Element("Result");
            if (result != null)
            {
                // Pick up the WOE ID and lookup weather
                XElement woeid = result.Element("woeid");
                return YahooLocationServices.ParseLocationElement(woeid);
            }

            return String.Empty;
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
                return String.Empty;
            }

            // Don't show if values are actually lat/long values
            double parsedLine = Double.MinValue;
            string[] parts = line.Value.Split(YahooLocationServices.CommaSplitter, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && Double.TryParse(parts[0], out parsedLine) && Double.TryParse(parts[1], out parsedLine))
            {
                return String.Empty;
            }

            return line.Value;
        }

    }
}
