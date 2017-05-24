using InkCards.Services.Bootstrap;
using InkCards.Services.Storage.Sqlite;
using InkCards.Views.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.HockeyApp;
using Microsoft.Practices.Unity;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace InkCards
{
    sealed partial class App : Application
    {
        public IUnityContainer DependencyResolver { get; private set; }
        public Frame Frame { get; private set; }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            using (var db = new MainDatabaseContext())
            {
                db.Database.Migrate();
            }

            HockeyClient.Current.Configure("c72c77abf0814629a4e5bb469d91a1ae");
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootFrame;
            }

            this.Frame = rootFrame;
            this.InitializeDependencyResolver();

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        private void InitializeDependencyResolver()
        {
            var container = new UnityContainer();
            new UnityConfig().ConfigureContainer(container, this);
            this.DependencyResolver = container;
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        public void ApplyDefaultViewChromeStyle()
        {
            var applicationViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            applicationViewTitleBar.BackgroundColor =
            applicationViewTitleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["VeryLightGrayBrush"]).Color;

            applicationViewTitleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)Application.Current.Resources["DarkGrayBrush"]).Color;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(1200, 650));

            var titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.ExtendViewIntoTitleBar = false;
        }

        public void ApplyCompactOverlayChromeStyle()
        {
            var view = ApplicationView.GetForCurrentView();
            view.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            view.TitleBar.ButtonForegroundColor = ((SolidColorBrush)((ResourceDictionary)App.Current.Resources.ThemeDictionaries["Dark"])["DarkGrayBrush"]).Color;
            var titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.ExtendViewIntoTitleBar = true;
        }
    }
}
