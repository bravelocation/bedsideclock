//-----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Brave Location">
//     Copyright (c) Brave Location Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Com.BraveLocation.BedsideClock
{
    using System;
    using System.Collections.Generic;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;

    /// <summary>
    /// Main application class
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// User settings
        /// </summary>
        private IsolatedStorageSettings userSettings = IsolatedStorageSettings.ApplicationSettings;

        /// <summary>
        /// Avoid double-initialization
        /// </summary>
        private bool phoneApplicationInitialized = false;
       
        /// <summary>
        /// Initializes a new instance of the App class.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            this.UnhandledException += this.Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                // Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                // Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            this.InitializeComponent();

            // Phone-specific initialization
            this.InitializePhoneApplication();

            // Disable the user idle detection mode - so screen continues to display
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
        }

        /// <summary>
        /// Gets easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Code to execute when the application is launching (e.g., from Start)
        /// This code will not execute when the application is reactivated
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        /// <summary>
        /// Code to execute when the application is activated (brought to foreground)
        /// This code will not execute when the application is first launched
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        /// <summary>
        /// Code to execute when the application is deactivated (sent to background)
        /// This code will not execute when the application is closing
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            this.userSettings.Save();
        }

        /// <summary>
        /// Code to execute when the application is closing (e.g., user hit Back)
        /// This code will not execute when the application is deactivated
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            this.userSettings.Save();
        }

        /// <summary>
        /// Code to execute if a navigation fails
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Code to execute on Unhandled Exceptions
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        /// <summary>
        /// Do not add any additional code to this method 
        /// </summary>
        private void InitializePhoneApplication()
        {
            if (this.phoneApplicationInitialized)
            {
                return;
            }

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            this.RootFrame = new PhoneApplicationFrame();
            this.RootFrame.Navigated += this.CompleteInitializePhoneApplication;

            // Handle navigation failures
            this.RootFrame.NavigationFailed += this.RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            this.phoneApplicationInitialized = true;
        }

        /// <summary>
        /// Do not add any additional code to this method 
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event args</param>
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (this.RootVisual != this.RootFrame)
            {
                this.RootVisual = this.RootFrame;
            }

            // Remove this handler since it is no longer needed
            this.RootFrame.Navigated -= this.CompleteInitializePhoneApplication;
        }

        #endregion
    }
}