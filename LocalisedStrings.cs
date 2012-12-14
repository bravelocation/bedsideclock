//-----------------------------------------------------------------------
// <copyright file="LocalisedStrings.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
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
        /// Gets a property that exposes resources class
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