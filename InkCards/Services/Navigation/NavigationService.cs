using InkCards.Views.Pages;
using Microsoft.Services.Store.Engagement;
using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace InkCards.Services.Navigation
{
    class NavigationService : INavigationService
    {
        private readonly Frame frame;
        private readonly Dictionary<PageType, Action<object>> navigationPaths;

        public NavigationService(Frame frame)
        {
            this.frame = frame;

            this.navigationPaths = new Dictionary<PageType, Action<object>>
            {
                {PageType.MainPage, arg => this.frame.Navigate(typeof(MainPage), arg)},
                {PageType.CardDesignPage, arg => this.frame.Navigate(typeof(CardDesignPage), arg)},
                {PageType.FlashcardTestPage, arg => this.frame.Navigate(typeof(FlashcardTestPage), arg)},
                {PageType.CardsBrowse, arg => this.frame.Navigate(typeof(CardsBrowsePage), arg)},
                {PageType.Feedback, async arg =>
                {
                    var launcher = StoreServicesFeedbackLauncher.GetDefault();
                    await launcher.LaunchAsync();
                } }
            };
        }

        public void Navigate(PageType pageType, object arg)
            => this.navigationPaths[pageType](arg);
    }
}
