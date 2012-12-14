//-----------------------------------------------------------------------
// <copyright file="Settings.xaml.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
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

    /// <summary>
    /// Settings page
    /// </summary>
    public partial class Settings : PhoneApplicationPage
    {
        /// <summary>
        /// User settings
        /// </summary>
        private UserSettings userSettings = new UserSettings();
        
        /// <summary>
        /// Initializes a new instance of the Settings class
        /// </summary>
        public Settings()
        {
            this.InitializeComponent();

            this.DimmedSwitch.IsChecked = this.userSettings.Dimmed;
            this.DimmedSwitch.Checked += new EventHandler<RoutedEventArgs>(this.DimmedSwitch_Changed);
            this.DimmedSwitch.Unchecked += new EventHandler<RoutedEventArgs>(this.DimmedSwitch_Changed);

            this.LocationSwitch.IsChecked = this.userSettings.ShowLocation;
            this.LocationSwitch.Checked += new EventHandler<RoutedEventArgs>(this.LocationSwitch_Changed);
            this.LocationSwitch.Unchecked += new EventHandler<RoutedEventArgs>(this.LocationSwitch_Changed);

            this.WeatherSwitch.IsChecked = this.userSettings.ShowWeather;
            this.WeatherSwitch.Checked += new EventHandler<RoutedEventArgs>(this.WeatherSwitch_Changed);
            this.WeatherSwitch.Unchecked += new EventHandler<RoutedEventArgs>(this.WeatherSwitch_Changed);

            this.MoonSwitch.IsChecked = this.userSettings.ShowMoon;
            this.MoonSwitch.Checked += new EventHandler<RoutedEventArgs>(this.MoonSwitch_Changed);
            this.MoonSwitch.Unchecked += new EventHandler<RoutedEventArgs>(this.MoonSwitch_Changed);
        }

        /// <summary>
        /// Overrides the default back button behavior to ensure new settings are picked up
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            // Force a navigation reload of the main screen, to ensure new settings are picked up
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));  
            e.Cancel = true;  // Cancels the default behavior.
        }

        /// <summary>
        /// Event handler of click on the feedback button
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();
            task.To = "feedback@BraveLocation.com";
            task.Subject = "Bedside Clock Feedback";
            task.Show();
        }

        /// <summary>
        /// Event handler for click on about button
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://www.BraveLocation.com");
            task.Show();
        }

        /// <summary>
        /// Event handler for click on privacy button
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void PrivacyButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://info.yahoo.com/privacy/us/yahoo/devel/details.html");
            task.Show();
        }

        /// <summary>
        /// Event handler for click on Yahoo Weather button
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void YahooWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://weather.yahoo.com");
            task.Show();
        }

        /// <summary>
        /// Event handler for click on "Elliot" button
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void ElliottButton_Click(object sender, RoutedEventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://www.gavinelliott.co.uk/2010/04/free-weather-icons/");
            task.Show();
        }

        /// <summary>
        /// Event handler for when location switch changes
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void LocationSwitch_Changed(object sender, RoutedEventArgs e)
        {
            this.userSettings.ShowLocation = this.LocationSwitch.IsChecked.Value;
        }

        /// <summary>
        /// Event handler for when weather switch changes
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void WeatherSwitch_Changed(object sender, RoutedEventArgs e)
        {
            this.userSettings.ShowWeather = this.WeatherSwitch.IsChecked.Value;
        }

        /// <summary>
        /// Event handler for when moon switch changes
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void MoonSwitch_Changed(object sender, RoutedEventArgs e)
        {
            this.userSettings.ShowMoon = this.MoonSwitch.IsChecked.Value;
        }

        /// <summary>
        /// Event handler for when dimmed switch changes
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguments</param>
        private void DimmedSwitch_Changed(object sender, RoutedEventArgs e)
        {
            this.userSettings.Dimmed = this.DimmedSwitch.IsChecked.Value;
        }
    }
}