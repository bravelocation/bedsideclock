//-----------------------------------------------------------------------
// <copyright file="UserSettings.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
    using System;
    using System.IO.IsolatedStorage;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Ink;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;

    /// <summary>
    /// Class that manages the user settings
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// Amount of brightness the +/- buttons change it by
        /// </summary>
        public const byte BrightnessChangeAmount = 32;
        
        /// <summary>
        /// Minimum brightness setting
        /// </summary>
        public const byte MinimumBrightness = 63;

        /// <summary>
        /// Maximum brightness setting
        /// </summary>
        public const byte MaximumBrightness = 255;

        /// <summary>
        /// Current brightness setting key
        /// </summary>
        private const string BrightnessKey = "brightness";

        /// <summary>
        /// Current show location setting key
        /// </summary>
        private const string LocationKey = "showlocation";

        /// <summary>
        /// Current show weather setting key
        /// </summary>
        private const string WeatherKey = "showweather";

        /// <summary>
        /// Current show moon setting key
        /// </summary>
        private const string MoonKey = "showmoon";

        /// <summary>
        /// User settings
        /// </summary>
        private IsolatedStorageSettings userSettings = IsolatedStorageSettings.ApplicationSettings;

        /// <summary>
        /// Gets or sets the current brightness property
        /// </summary>
        public byte CurrentBrightness
        {
            get
            {
                if (this.userSettings.Contains(UserSettings.BrightnessKey))
                {
                    return (byte)this.userSettings[UserSettings.BrightnessKey];
                }

                return UserSettings.MaximumBrightness;
            }

            set
            {
                this.userSettings[UserSettings.BrightnessKey] = value;
                this.userSettings.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the clock should be dimmed
        /// </summary>
        public bool Dimmed
        {
            get
            {
                return this.CurrentBrightness != UserSettings.MaximumBrightness;
            }

            set
            {
                if (value)
                {
                    this.CurrentBrightness = UserSettings.MinimumBrightness;
                }
                else
                {
                    this.CurrentBrightness = UserSettings.MaximumBrightness;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the location should be shown
        /// </summary>
        public bool ShowLocation
        {
            get
            {
                if (this.userSettings.Contains(UserSettings.LocationKey))
                {
                    return (bool)this.userSettings[UserSettings.LocationKey];
                }

                return true;
            }

            set
            {
                this.userSettings[UserSettings.LocationKey] = value;
                this.userSettings.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the weather should be shown
        /// </summary>
        public bool ShowWeather
        {
            get
            {
                if (this.userSettings.Contains(UserSettings.WeatherKey))
                {
                    return (bool)this.userSettings[UserSettings.WeatherKey];
                }

                return true;
            }

            set
            {
                this.userSettings[UserSettings.WeatherKey] = value;
                this.userSettings.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the moon should be shown
        /// </summary>
        public bool ShowMoon
        {
            get
            {
                if (this.userSettings.Contains(UserSettings.MoonKey))
                {
                    return (bool)this.userSettings[UserSettings.MoonKey];
                }

                return true;
            }

            set
            {
                this.userSettings[UserSettings.MoonKey] = value;
                this.userSettings.Save();
            }
        }
    }
}
