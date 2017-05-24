using InkCards.ViewModels.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Unity;
using System.Linq;
using InkCards.Views.Controls;
using Windows.ApplicationModel.Core;
using InkCards.Models.Cards;

namespace InkCards.Views.Pages
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel viewModel;

        private MainPageViewModel ViewModel => this.viewModel ?? (this.viewModel = ((App)Application.Current).DependencyResolver.Resolve<MainPageViewModel>());

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;

            ((App)Application.Current).ApplyDefaultViewChromeStyle();

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.Initialize();

            this.ViewModel.CardsBrowseViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ViewModel.CardsBrowseViewModel.IsLoading) &&
                this.ViewModel.CardsBrowseViewModel.IsLoading)
                VisualStateManager.GoToState(this, nameof(this.RightPaneHidden), true);
        }

        private void CardPreview_FinishedRendering()
        {
            if (this.CollectionDetailCardsGridView.Items
                    .Select(x => this.CollectionDetailCardsGridView.ContainerFromItem(x))
                    .Cast<GridViewItem>()
                    .Where(x => x != null)
                    .Select(x => x.ContentTemplateRoot)
                    .Cast<CardPreview>()
                    .All(x => x.IsRendered) && !this.ViewModel.CardsBrowseViewModel.IsLoading)
                VisualStateManager.GoToState(this, nameof(this.RightPaneVisible), true);
        }

        private void CollectionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.SelectedCollections = this.CollectionsListView.SelectedItems.Cast<CardCollection>();
        }

        private void DeleteCollection(object sender, RoutedEventArgs e)
        {
            this.DeleteCollectionFlyout.Hide();
            this.ViewModel.DeleteSelectedCollection();
        }
    }
}
