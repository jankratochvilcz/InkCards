using InkCards.ViewModels.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using InkCards.Models.Cards;
using System.ComponentModel;
using Windows.System;

namespace InkCards.Views.Pages
{
    public sealed partial class CardDesignPage : Page, INotifyPropertyChanged
    {
        private CardDesignPageViewModel viewModel;

        private Visibility unableToSave = Visibility.Collapsed;

        public CardDesignPageViewModel ViewModel
        {
            get
            {
                if (this.viewModel == null)
                {
                    this.viewModel = ((App)Application.Current).DependencyResolver.Resolve<CardDesignPageViewModel>();
                    this.viewModel.StrokesForCurrentlyEditedCardLoaded += ViewModel_StrokesForCurrentlyEditedCardLoaded;
                }

                return this.viewModel;
            }
        }

        public Visibility UnableToSave
        {
            get { return this.unableToSave; }
            set
            {
                if (this.unableToSave == value) return;

                this.unableToSave = value;
                this.OnPropertyChanged(nameof(this.UnableToSave));
            }
        }
        
        public CardDesignPage()
        {
            this.InitializeComponent();
        }

        private async void ViewModel_StrokesForCurrentlyEditedCardLoaded()
        => await Task.WhenAll(
            this.CardFrontEditor.LoadStrokesFromStream(this.ViewModel.CurrentlyEditedCard.CardFrontInk),
            this.CardBackEditor.LoadStrokesFromStream(this.ViewModel.CurrentlyEditedCard.CardBackInk));

        private async void SaveCurrentCardAsync()
        {
            if (!this.ViewModel.CanSaveCurrenlyEditedCard) return;

            var cardToSave = this.ViewModel.CurrentlyEditedCard;
            var streams = await Task.WhenAll(
                this.CardFrontEditor.GetStrokesAsStream(),
                this.CardBackEditor.GetStrokesAsStream());

            if (streams[0].Size == 0 || streams[1].Size == 0)
                return;

            await this.ViewModel.SaveCard(cardToSave, streams[0], streams[1]);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await this.ViewModel.Initialize(e.Parameter is Guid ? (Guid)e.Parameter : Guid.NewGuid());
        }

        private void InkCardPreviewBorder_RightTapped(object sender, RightTappedRoutedEventArgs e)
            => FlyoutBase.GetAttachedFlyout((Border)sender).ShowAt((Border)sender);

        private void InkCardDeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
            => this.ViewModel.DeleteCard((InkCard)((FrameworkElement)sender).Tag);

        private void CardCollectionTitleTextBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter) VisualStateManager.GoToState(this, nameof(this.CardCollectionTitleViewState), true);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
