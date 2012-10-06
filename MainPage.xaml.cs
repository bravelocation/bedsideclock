using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Info;

using System.Windows.Threading;
using Microsoft.Devices.Sensors;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework;

namespace com.bravelocation.bedsideClock
{
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Comma splitter
        /// </summary>
        private static char[] CommaSplitter = { ',' };

        /// <summary>
        /// User settings
        /// </summary>
        private UserSettings userSettings = new UserSettings();

        /// <summary>
        /// Number of seconds before fading
        /// </summary>
        private const int SecondsToFade = 5;

        /// <summary>
        /// The timer which drives the clock
        /// </summary>
        private DispatcherTimer clockTimer;

        /// <summary>
        /// Reference to device's accelerometer
        /// </summary>
        private Accelerometer accelerometer;

        /// <summary>
        /// Current x-value of accelerometer * 5
        /// </summary>
        private int currentX = 0;

        /// <summary>
        /// Current y-value of accelerometer * 5
        /// </summary>
        private int currentY = 0;

        /// <summary>
        /// Current z-value of accelerometer * 5
        /// </summary>
        private int currentZ = 0;

        /// <summary>
        /// Time of the last movement - used to calculate when to fade after move
        /// </summary>
        private DateTime lastMovementTime = DateTime.Now;

        /// <summary>
        /// Location watcher
        /// </summary>
        private GeoCoordinateWatcher watcher;

        /// <summary>
        /// Current minute
        /// </summary>
        private int currentMinute = -1;

        /// <summary>
        /// Current hour
        /// </summary>
        private int currentHour = -1;

        /// <summary>
        /// Current location
        /// </summary>
        private GeoCoordinate currentLocation = new GeoCoordinate(53.79415893554690, -1.54840183258057); // London
        // private GeoCoordinate currentLocation = new GeoCoordinate(47.61752586841285, -122.19486024230719); // Redmond

        /// <summary>
        /// Current WOE ID
        /// </summary>
        private string currentWoeId = String.Empty;

        /// <summary>
        /// Current brightness property
        /// </summary>
        public byte CurrentBrightness
        {
            get 
            {
                return userSettings.CurrentBrightness; 
            }
            set
            {
                userSettings.CurrentBrightness = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
  
            this.LocationText.Visibility = System.Windows.Visibility.Collapsed;

            // Setup orientation-changed handler
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(MainPage_OrientationChanged);

            // Setup the clock timer
            this.currentMinute = DateTime.Now.Minute;
            this.currentHour = DateTime.Now.Hour;

            this.clockTimer = new DispatcherTimer();
            this.clockTimer.Interval = new TimeSpan(0, 0, 1);
            this.clockTimer.Tick += new EventHandler(clockTimer_Tick);
            this.clockTimer.Start();

            // Setup accelerometer
            this.accelerometer = new Accelerometer();
            this.accelerometer.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<AccelerometerReading>>(accelerometer_CurrentValueChanged);
            this.accelerometer.Start();

            // Setup do a location and weather lookup 
            this.GeocoordinateLookup();

            // Setup the colors and icon
            this.SetTimeColorsAndIcon();

            // Set the bottom row to be the correct size
            this.SetBottomRowSize(this.Orientation);

            // Show power warning if appropriate
            this.ShowPowerWarning(this.LocationText);
            DeviceStatus.PowerSourceChanged += new EventHandler(DeviceStatus_PowerSourceChanged);

            // Do a location lookup if required
            this.ShowLocation(userSettings.ShowLocation);

            // Show the time
            this.FadeDisplay();
        }

        /// <summary>
        /// Event handler for power source changing
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void DeviceStatus_PowerSourceChanged(object sender, EventArgs e)
        {
            this.LocationText.Dispatcher.BeginInvoke(() => this.ShowPowerWarning(this.LocationText)); 
        }

        /// <summary>
        /// Shows the power warning
        /// </summary>
        private void ShowPowerWarning(TextBlock textBlock)
        {
            if (DeviceStatus.PowerSource == PowerSource.Battery)
            {
                textBlock.Text = StringResources.PowerOnCheckText;
                textBlock.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                textBlock.Text = String.Empty;
                textBlock.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Fades the display
        /// </summary>
        private void FadeDisplay()
        {
            // Set the opacity of the settings elements
            double settingsOpacity = ((double)this.CurrentBrightness) / (UserSettings.MaximumBrightness);
            if (settingsOpacity < 0.5) 
            { 
                settingsOpacity = 0.5; 
            }

            this.CurrentTime.Opacity = settingsOpacity;
            this.CurrentDate.Opacity = settingsOpacity;
            this.LocationText.Opacity = settingsOpacity;
            this.SettingsButton.Opacity = settingsOpacity;
        }

        /// <summary>
        /// Unfades the display
        /// </summary>
        private void UnfadeDisplay()
        {
            this.lastMovementTime = DateTime.Now;

            this.CurrentTime.Opacity = UserSettings.MaximumBrightness;
            this.CurrentDate.Opacity = UserSettings.MaximumBrightness;
            this.LocationText.Opacity = UserSettings.MaximumBrightness;
            this.SettingsButton.Opacity = UserSettings.MaximumBrightness;
        }


        /// <summary>
        /// Handles orientation changed event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void MainPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            // Set the bottom row to be the correct size
            this.SetBottomRowSize(e.Orientation);

            this.UnfadeDisplay();
        }

        /// <summary>
        /// Sets the bottom row size based on page orientation
        /// </summary>
        /// <param name="o">Page orientation</param>
        private void SetBottomRowSize(PageOrientation o)
        {
            if (o == PageOrientation.Landscape || o == PageOrientation.LandscapeLeft || o == PageOrientation.LandscapeRight)
            {
                this.BottomRow.Height = new GridLength(80);
            }
            else
            {
                this.BottomRow.Height = new GridLength(120);
            }
        }

        /// <summary> 
        /// Handles the clock tick - updates the text correctly
        /// </summary>
        /// <param name="sender">Event handler sender</param>
        /// <param name="e">Event handler arguments</param>
        private void clockTimer_Tick(object sender, EventArgs e)
        {
            DateTime current = DateTime.Now;

            this.CurrentTime.Text = current.ToString("HH:mm");

            // Do we need to fade the display
            if (current.AddSeconds(MainPage.SecondsToFade * -1.0) > this.lastMovementTime)
            {
                this.FadeDisplay();
            }

            // If the minute has changed, reset the colors and icon
            if (this.currentMinute != current.Minute)
            {
                this.SetTimeColorsAndIcon();
                this.currentMinute = current.Minute;
            }

            // If the hour has changed, reset the weather if necessary
            if (this.currentHour != current.Hour)
            {
                this.LookupWeather(this.userSettings.ShowWeather);
                this.currentHour = current.Hour;
            }
        }

        /// <summary>
        /// Sets the colors of the clock and sets the icon depending on time
        /// </summary>
        private void SetTimeColorsAndIcon()
        {
            DateTime timeNow = DateTime.Now;
            this.CurrentDate.Text = timeNow.ToString("dddd dd MMMM");

            // First set the color of the text
            System.Windows.Media.Color currentColor = Colors.Green;

            switch (timeNow.Hour)
            {
                case 0: currentColor = Colors.Blue; break;
                case 1: currentColor = Colors.Blue; break;
                case 2: currentColor = Colors.Blue; break;
                case 3: currentColor = Colors.Purple; break;
                case 4: currentColor = Colors.Purple; break;
                case 5: currentColor = Colors.Red; break;
                case 6: currentColor = Colors.Orange; break;
                case 7: currentColor = Colors.Yellow; break;
                default: currentColor = Colors.Green; break;
            }

            this.CurrentTime.Foreground = new SolidColorBrush(currentColor);

            // Now decide whether it's before sunrise of after sunset
            DateTime sunrise = SunriseCalculator.SunriseAndSunset.Sunrise(this.currentLocation.Latitude, this.currentLocation.Longitude, timeNow);
            DateTime sunset = SunriseCalculator.SunriseAndSunset.Sunset(this.currentLocation.Latitude, this.currentLocation.Longitude, timeNow);

            // Make sure all on same day
            if (sunrise.DayOfYear != timeNow.DayOfYear)
            {
                if (sunrise.DayOfYear < timeNow.DayOfYear)
                {
                    sunrise = sunrise.AddDays(1);
                }
                else
                {
                    sunrise = sunrise.AddDays(-1);
                }
            }

            if (sunset.DayOfYear != timeNow.DayOfYear)
            {
                if (sunset.DayOfYear < timeNow.DayOfYear)
                {
                    sunset = sunset.AddDays(1);
                }
                else
                {
                    sunset = sunset.AddDays(-1);
                }
            }

            // Set icon as appropriate
            if (this.userSettings.ShowMoon)
            {
                this.SunTypeImage.Visibility = System.Windows.Visibility.Visible;

                if (timeNow < sunrise.AddMinutes(-30))
                {
                    this.ShowMoonIcon(timeNow);
                }
                else if (timeNow < sunrise.AddMinutes(30))
                {
                    this.SunTypeImage.Source = new BitmapImage(new Uri("dawndusk.png", UriKind.RelativeOrAbsolute));
                }
                else if (timeNow < sunset.AddMinutes(-30))
                {
                    // Not showing sun any more
                    this.SunTypeImage.Source = new BitmapImage(new Uri("blank.png", UriKind.RelativeOrAbsolute));
                }
                else if (timeNow < sunset.AddMinutes(30))
                {
                    this.SunTypeImage.Source = new BitmapImage(new Uri("dawndusk.png", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    this.ShowMoonIcon(timeNow);
                }
            }
            else
            {
                this.SunTypeImage.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Show correct moon icon
        /// </summary>
        /// <param name="timeNow">Time now</param>
        private void ShowMoonIcon(DateTime timeNow)
        {
            // Which moon icon?
            MoonPhase.Phase currentMoonPhase = MoonPhase.CalculateMoonPhase(timeNow);
            string moonSource = "quartermoon.png";

            switch (currentMoonPhase)
            {
                case MoonPhase.Phase.NewMoon: moonSource = "newmoon.png"; break;
                case MoonPhase.Phase.WaxingCrescentMoon: moonSource = "crescentmoon.png"; break;
                case MoonPhase.Phase.QuarterMoon: moonSource = "quartermoon.png"; break;
                case MoonPhase.Phase.WaxingGibbousMoon: moonSource = "gibbousmoon.png"; break;
                case MoonPhase.Phase.FullMoon: moonSource = "fullmoon.png"; break;
                case MoonPhase.Phase.WaningGibbousMoon: moonSource = "gibbousmoon.png"; break;
                case MoonPhase.Phase.LastQuarterMoon: moonSource = "quartermoon.png"; break;
                case MoonPhase.Phase.WaningCrescentMoon: moonSource = "crescentmoon.png"; break;
                default: moonSource = "quartermoon.png"; break;
            }

            this.SunTypeImage.Source = new BitmapImage(new Uri(moonSource, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Handles accelerometer event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void accelerometer_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            // Dispatches event to UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() => MyReadingChanged(e));
        }
        
        /// <summary>
        /// Handles accelerometer event on UI thread
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void MyReadingChanged(SensorReadingEventArgs<AccelerometerReading> e)
        {
            int latestX = Convert.ToInt32(e.SensorReading.Acceleration.X * 5.0);
            int latestY = Convert.ToInt32(e.SensorReading.Acceleration.Y * 5.0);
            int latestZ = Convert.ToInt32(e.SensorReading.Acceleration.Z * 5.0);

            if (latestX != this.currentX || latestY != currentY || latestZ != currentZ)
            {
                this.currentX = latestX;
                this.currentY = latestY;
                this.currentZ = latestZ;

                this.UnfadeDisplay();
            }
        }

        /// <summary>
        /// Show the location information
        /// </summary>
        private void ShowLocation(bool showLocation)
        {
            // Uncomment this section when running in emulator
            // this.LookupAddress();

            // Then toggle the visibility of the text
            if (showLocation)
            {
                // Reset the text
                this.LocationText.Text = StringResources.InitialLocationText;

                // Set the progress bar
                this.LocationProgressBar.Visibility = System.Windows.Visibility.Visible;
                this.LocationProgressBar.IsEnabled = true;

                // Try updating the location
                this.GeocoordinateLookup();

                // Make the text and visible
                this.LocationText.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.LocationText.Visibility = System.Windows.Visibility.Collapsed;
                this.LocationProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                this.LocationProgressBar.IsEnabled = false;
            }
       }

        /// <summary>
        /// Looks up current geo-coordinates
        /// </summary>
        private void GeocoordinateLookup()
        {
            // The watcher variable was previously declared as type GeoCoordinateWatcher. 
            if (this.watcher == null)
            {
                this.watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // Use high accuracy.
                this.watcher.MovementThreshold = 20; // Use MovementThreshold to ignore noise in the signal.
                this.watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            }

            watcher.Start();
        }

        /// <summary>
        /// Event handler for geo-coordinate watcher changed event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
                // Use the Position property of the GeoCoordinateWatcher object to get the current location.
                this.currentLocation = watcher.Position.Location;

                //Stop the Location Service to conserve battery power.
                watcher.Stop();
                
                // Reset the sunset location just in case
                this.SetTimeColorsAndIcon();

                // Try looking up the address
                this.LookupAddress();
            }
            else if (e.Status == GeoPositionStatus.NoData)
            {
                watcher.Stop();
                this.LocationText.Text = StringResources.LocationNotFoundText;
                this.LocationProgressBar.IsEnabled = false;
                this.LocationProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (e.Status == GeoPositionStatus.Disabled)
            {
                watcher.Stop();
                this.LocationText.Text = StringResources.LocationServiceDisabledText;
                this.LocationProgressBar.IsEnabled = false;
                this.LocationProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Lookup the address using the current coordinates
        /// </summary>
        private void LookupAddress()
        {
            // Use location service
            string locationUrl = String.Format(CultureInfo.InvariantCulture, "http://where.yahooapis.com/geocode?q={0:0.00000000000000},+{1:0.00000000000000}&gflags=R&flags=G&appid=1auJbv6k", this.currentLocation.Latitude, this.currentLocation.Longitude);

            WebClient addressLookup = new WebClient();
            addressLookup.DownloadStringCompleted += new DownloadStringCompletedEventHandler(addressLookup_DownloadStringCompleted);
            addressLookup.DownloadStringAsync(new Uri(locationUrl));
        }

        /// <summary>
        /// Event handler for address lookup call
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void addressLookup_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            this.LocationProgressBar.IsEnabled = false;
            this.LocationProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            
            if (e.Error != null)
            {
                this.LocationText.Text = StringResources.LocationNotFoundText;
                return;
            }

            // Parse the Yahoo XML returned
            // See documentation at http://developer.yahoo.com/geo/placefinder/guide/
            XElement rootElement;
            try
            {
                rootElement = XElement.Parse(e.Result);
            }
            catch (XmlException)
            {
                this.LocationText.Text = StringResources.LocationNotFoundText;
                return;
            }

            if (rootElement == null)
            {
                this.LocationText.Text = StringResources.LocationNotFoundText;
                return;
            }

            // Check for error code
            XElement errorCode = rootElement.Element("Error");
            if (errorCode == null || errorCode.Value != "0")
            {
                this.LocationText.Text = StringResources.LocationNotFoundText;
                return;
            }

            XElement result = rootElement.Element("Result");
            if (result != null)
            {
                XElement line1 = result.Element("line1");
                XElement line2 = result.Element("line2");
                XElement line3 = result.Element("line3");
                XElement line4 = result.Element("line4");

                StringBuilder address = new StringBuilder();
                address.Append(this.ParseLocationElement(line1));
                address.Append(" " + this.ParseLocationElement(line2));
                address.Append(" " + this.ParseLocationElement(line3));
                address.Append(" " + this.ParseLocationElement(line4));

                this.LocationText.Text = address.ToString().Trim();

                // Pick up the WOE ID and lookup weather
                XElement woeid = result.Element("woeid");
                this.currentWoeId = this.ParseLocationElement(woeid);
                this.LookupWeather(this.userSettings.ShowWeather);

                return;
            }

            // Something went wrong if we got here
            this.LocationText.Text = StringResources.LocationNotFoundText;
        }

        /// <summary>
        /// Lookup the weather using the current WOE ID
        /// </summary>
        /// <param name="showWeather">Show weather</param>
        private void LookupWeather(bool showWeather)
        {
            if (!showWeather || String.IsNullOrEmpty(this.currentWoeId))
            {
                return;
            }

            // Use weather service
            string weatherUrl = String.Format(CultureInfo.InvariantCulture, "http://weather.yahooapis.com/forecastrss?w={0}", this.currentWoeId);

            WebClient weatherLookup = new WebClient();
            weatherLookup.DownloadStringCompleted += new DownloadStringCompletedEventHandler(weatherLookup_DownloadStringCompleted);
            weatherLookup.DownloadStringAsync(new Uri(weatherUrl));
        }

        /// <summary>
        /// Event handler for address lookup call
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void weatherLookup_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // Clear weather icon
            this.WeatherImage.Source = new BitmapImage(new Uri("blank.png", UriKind.RelativeOrAbsolute));
            this.WeatherImage.Visibility = System.Windows.Visibility.Collapsed;

            // Parse the Yahoo XML returned
            // See documentation at http://developer.yahoo.com/weather/
            XElement rootElement;
            try
            {
                rootElement = XElement.Parse(e.Result);
            }
            catch (XmlException)
            {
                return;
            }

            string weatherForecastDay = this.WeatherForecastDay();

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
                            if (forecast.Name.LocalName == "forecast")
                            {
                                // If correctly today or tomorrow, depending on time, set image as code
                                XAttribute forecastDay = forecast.Attribute("day");
                                if (forecastDay != null && forecastDay.Value == weatherForecastDay)
                                {
                                    // Use the code to set the image
                                    XAttribute forecastCode = forecast.Attribute("code");
                                    if (forecastCode != null)
                                    {
                                        // Set the image URL
                                        string imageName = String.Format(CultureInfo.InvariantCulture, "weather/{0}.png", this.MapWeatherCodeToImage(forecastCode.Value));
                                        this.WeatherImage.Source = new BitmapImage(new Uri(imageName, UriKind.RelativeOrAbsolute));
                                        this.WeatherImage.Visibility = System.Windows.Visibility.Visible;

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Turns weather code into logical image
        /// </summary>
        /// <param name="code">Yahoo! Weather code</param>
        /// <returns>String referencing image to use for weather</returns>
        private string MapWeatherCodeToImage(string code)
        {
            switch (code)
            {
                case "9":
                case "11":
                case "12":
                case "40":
                    return "cloudy-with-rain";
                case "23":
                case "24":
                case "29":
                case "30":
                case "33":
                case "34":
                case "44":
                    return "cloudy-with-sunshine";
                case "20":
                case "22":
                case "26":
                case "27":
                case "28":
                    return "cloudy";
                case "5":
                case "6":
                case "7":
                case "8":
                case "13":
                case "14":
                case "18":
                case "46":
                    return "light-snow";
                case "10":
                case "15":
                case "16":
                case "43":
                    return "heavy-snow";
                case "0": 
                case "1":
                case "2":
                case "3":
                case "4":
                case "17":
                case "19":
                case "35":
                case "37":
                case "38":
                case "39":
                case "45":
                case "47":
                    return "storm";
                case "21":
                case "25":
                case "31":
                case "32":
                case "36":
                    return "sunny";
                default:
                    return "blank";
            }
        }

        /// <summary>
        /// Calculates the day of the week to use for the forecast
        /// </summary>
        /// <returns>String matching values from http://developer.yahoo.com/weather/ </returns>
        private string WeatherForecastDay()
        {
            DayOfWeek forecastDayOfWeek = DateTime.Now.DayOfWeek;

            // If after sunset, show tomorrow's weather
            DateTime sunset = SunriseCalculator.SunriseAndSunset.Sunset(this.currentLocation.Latitude, this.currentLocation.Longitude, DateTime.Now);

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
            return String.Empty;
        }

        /// <summary>
        /// Parses a location element for valid values
        /// </summary>
        /// <param name="line">Line element</param>
        /// <returns>String or empty</returns>
        private string ParseLocationElement(XElement line)
        {
            if (line == null)
            {
                return String.Empty;
            }

            // Don't show if values are actually lat/long values
            double parsedLine = Double.MinValue;
            string[] parts = line.Value.Split(MainPage.CommaSplitter, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2 && Double.TryParse(parts[0], out parsedLine) && Double.TryParse(parts[1], out parsedLine))
            {
                return String.Empty;
            }

            return line.Value;
        }

        /// <summary>
        /// settings button event handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));  
        }
    }
}