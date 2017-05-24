using InkCards.ViewModels.Pages;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Unity;

namespace InkCards.Views.Pages
{
    public sealed partial class CardsBrowsePage : Page
    {
        private CardsBrowseViewModel viewModel;

        public CardsBrowseViewModel ViewModel => this.viewModel ??
            (this.viewModel = ((App)Application.Current).DependencyResolver.Resolve<CardsBrowseViewModel>());

        public CardsBrowsePage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var collectionIds = (IEnumerable<Guid>)e.Parameter;

            await this.ViewModel.Load(collectionIds);
        }
    }
}
