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

namespace com.bravelocation.bedsideClock
{
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
        /// Current brightness property
        /// </summary>
        public byte CurrentBrightness
        {
            get
            {
                if (userSettings.Contains(UserSettings.BrightnessKey))
                {
                    return (byte)userSettings[UserSettings.BrightnessKey];
                }

                return UserSettings.MaximumBrightness;
            }
            set
            {
                userSettings[UserSettings.BrightnessKey] = value;
                this.userSettings.Save();
            }
        }

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
        /// Show location property
        /// </summary>
        public bool ShowLocation
        {
            get
            {
                if (userSettings.Contains(UserSettings.LocationKey))
                {
                    return (bool)userSettings[UserSettings.LocationKey];
                }

                return true;
            }
            set
            {
                userSettings[UserSettings.LocationKey] = value;
                this.userSettings.Save();
            }
        }

        /// <summary>
        /// Show weather property
        /// </summary>
        public bool ShowWeather
        {
            get
            {
                if (userSettings.Contains(UserSettings.WeatherKey))
                {
                    return (bool)userSettings[UserSettings.WeatherKey];
                }

                return true;
            }
            set
            {
                userSettings[UserSettings.WeatherKey] = value;
                this.userSettings.Save();
            }
        }

        /// <summary>
        /// Show moon property
        /// </summary>
        public bool ShowMoon
        {
            get
            {
                if (userSettings.Contains(UserSettings.MoonKey))
                {
                    return (bool)userSettings[UserSettings.MoonKey];
                }

                return true;
            }
            set
            {
                userSettings[UserSettings.MoonKey] = value;
                this.userSettings.Save();
            }
        }
    }
}
