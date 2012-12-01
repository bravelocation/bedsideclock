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

namespace com.bravelocation.bedsideClock
{
    public partial class MainPage : PhoneApplicationPage
    {
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

            this.SunTypeImage.Source = new BitmapImage(new Uri("moon/" + moonSource, UriKind.RelativeOrAbsolute));
        }

        /// <summary>
        /// Handles accelerometer event
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void accelerometer_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            // Dispatches event to UI thread
            Deployment.Current.Dispatcher.BeginInvoke(() => AccelerometerReadingChanged(e));
        }
        
        /// <summary>
        /// Handles accelerometer event on UI thread
        /// </summary>
        /// <param name="e">Event arguments</param>
        private void AccelerometerReadingChanged(SensorReadingEventArgs<AccelerometerReading> e)
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
            WebClient addressLookup = new WebClient();
            addressLookup.DownloadStringCompleted += new DownloadStringCompletedEventHandler(addressLookup_DownloadStringCompleted);
            addressLookup.DownloadStringAsync(YahooLocationServices.YahooLocationUri(this.currentLocation));
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
            
            if (e.Cancelled || e.Error != null)
            {
                this.LocationText.Text = StringResources.LocationNotFoundText;
                return;
            }

            this.LocationText.Text = YahooLocationServices.ParseAddress(e.Result);

            if (this.LocationText.Text != StringResources.LocationNotFoundText)
            {
                this.currentWoeId = YahooLocationServices.ParseWoeId(e.Result);
                this.LookupWeather(this.userSettings.ShowWeather);
            }
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
            WebClient weatherLookup = new WebClient();
            weatherLookup.DownloadStringCompleted += new DownloadStringCompletedEventHandler(weatherLookup_DownloadStringCompleted);
            weatherLookup.DownloadStringAsync(YahooWeather.YahooWeatherUri(this.currentWoeId));
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
            if (e.Cancelled || e.Error != null)
            {
                return;
            }

            int parsedWeather = YahooWeather.ParseWeather(e.Result, this.currentLocation);

            // Set the image URL
            string imageName = String.Format(CultureInfo.InvariantCulture, "weather/{0}.png", YahooWeather.MapWeatherCodeToImage(parsedWeather));
            this.WeatherImage.Source = new BitmapImage(new Uri(imageName, UriKind.RelativeOrAbsolute));
            this.WeatherImage.Visibility = System.Windows.Visibility.Visible;
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