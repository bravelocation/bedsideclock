using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace com.bravelocation.bedsideClock
{
    public partial class Settings : PhoneApplicationPage
    {
        /// <summary>
        /// User settings
        /// </summary>
        private UserSettings userSettings = new UserSettings();
        
        public Settings()
        {
            InitializeComponent();

            this.DimmedSwitch.IsChecked = this.userSettings.Dimmed;
            this.DimmedSwitch.Checked += new EventHandler<RoutedEventArgs>(DimmedSwitch_Changed);
            this.DimmedSwitch.Unchecked += new EventHandler<RoutedEventArgs>(DimmedSwitch_Changed);

            this.LocationSwitch.IsChecked = userSettings.ShowLocation;
            this.LocationSwitch.Checked += new EventHandler<RoutedEventArgs>(LocationSwitch_Changed);
            this.LocationSwitch.Unchecked += new EventHandler<RoutedEventArgs>(LocationSwitch_Changed);

            this.WeatherSwitch.IsChecked = userSettings.ShowWeather;
            this.WeatherSwitch.Checked += new EventHandler<RoutedEventArgs>(WeatherSwitch_Changed);
            this.WeatherSwitch.Unchecked += new EventHandler<RoutedEventArgs>(WeatherSwitch_Changed);

            this.MoonSwitch.IsChecked = userSettings.ShowMoon;
            this.MoonSwitch.Checked += new EventHandler<RoutedEventArgs>(MoonSwitch_Changed);
            this.MoonSwitch.Unchecked += new EventHandler<RoutedEventArgs>(MoonSwitch_Changed);
        }

        /// <summary>
        /// Overrides the default back button behaviour to ensure new settings are picked up
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // Force a navigation reload of the main screen, to ensure new settings are picked up
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));  
            e.Cancel = true;  //Cancels the default behavior.
        }

        private void feedbackButton_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();
            task.To = "feedback@bravelocation.com";
            task.Subject = "Bedside Clock Feedback";
            task.Show();
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://www.bravelocation.com");
            task.Show();
        }

        private void privacyButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://info.yahoo.com/privacy/us/yahoo/devel/details.html");
            task.Show();
        }

        private void yahooWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://weather.yahoo.com");
            task.Show();
        }

        private void elliottButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://www.gavinelliott.co.uk/2010/04/free-weather-icons/");
            task.Show();
        }

        private void LocationSwitch_Changed(object sender, RoutedEventArgs e)
        {
            userSettings.ShowLocation = this.LocationSwitch.IsChecked.Value;
        }

        private void WeatherSwitch_Changed(object sender, RoutedEventArgs e)
        {
            userSettings.ShowWeather = this.WeatherSwitch.IsChecked.Value;
        }

        private void MoonSwitch_Changed(object sender, RoutedEventArgs e)
        {
            userSettings.ShowMoon = this.MoonSwitch.IsChecked.Value;
        }

        private void DimmedSwitch_Changed(object sender, RoutedEventArgs e)
        {
            this.userSettings.Dimmed = this.DimmedSwitch.IsChecked.Value;
        }
    }
}