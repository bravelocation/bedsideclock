using System;
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
    /// <summary>
    /// Wrapper class for resource file
    /// </summary>
    public class LocalisedStrings
    {
        /// <summary>
        /// Private member to hold resources object
        /// </summary>
        private static StringResources localizedResources = new StringResources();

        /// <summary>
        /// Property that exposes resources class
        /// </summary>
        public StringResources LocalisedResources 
        { 
            get 
            { 
                return LocalisedStrings.localizedResources; 
            } 
        }
    }
}