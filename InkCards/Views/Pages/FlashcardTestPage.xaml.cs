using InkCards.ViewModels.Pages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Unity;
using Windows.UI.Xaml.Navigation;
using System;
using InkCards.ViewModels.Pages.Args;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.ViewManagement;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace InkCards.Views.Pages
{
    public sealed partial class FlashcardTestPage : Page, INotifyPropertyChanged
    {
        private FlashcardTestViewModel viewModel;
        private bool isInCompactOverlayMode;

        public FlashcardTestViewModel ViewModel => this.viewModel ??
            (this.viewModel = ((App)Application.Current).DependencyResolver.Resolve<FlashcardTestViewModel>());

        public bool IsInCompactOverlayMode
        {
            get { return this.isInCompactOverlayMode; }
            set
            {
                if (this.isInCompactOverlayMode == value) return;

                this.isInCompactOverlayMode = value;
                this.OnPropertyChanged(nameof(this.IsInCompactOverlayMode));
            }
        }

        public FlashcardTestPage()
        {
            this.InitializeComponent();

            Observable
                .FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(x => this.SizeChanged += x, x => this.SizeChanged -= x)
                .Throttle(new TimeSpan(0, 0, 0, 0, 400))
                .ObserveOnDispatcher()
                .Subscribe(x => this.OnPageSizeChanged(x.EventArgs));
        }

        #region Page Lifecycle

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = (FlashcardTestArgs)e.Parameter;
            await this.ViewModel.Initialize(args);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.Teardown();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.ViewModel.CardUpdated += () => this.CardPreview.Render();

            var view = ApplicationView.GetForCurrentView();
            if (view.IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                this.CompactOverlayModeButton.Visibility = Visibility.Visible;
        }

        #endregion

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ViewModel.InMovieMode)) ((App)Application.Current).ApplyDefaultViewChromeStyle();

            if(e.PropertyName == nameof(this.ViewModel.IsRevealed) && this.ViewModel.IsRevealed)
            {
                var revealStoryboard = (Storyboard)this.Resources["RevealCardStoryboard"];
                //Storyboard.SetTarget()
                //revealStoryboard.Begin();
            }

            if (e.PropertyName == nameof(this.ViewModel.IsRevealed) && !this.ViewModel.IsRevealed)
            {
                var revealStoryboard = (Storyboard)this.Resources["RevealCardStoryboard"];
                revealStoryboard.ToString();
            }
        }

        private async void ToggleCompactOverlayMode()
        {
            if (this.IsInCompactOverlayMode) this.LeaveCompactOverlayMode();
            else this.EnterCompactOverlayMode();
        }

        private async Task EnterCompactOverlayMode()
        {
            var view = ApplicationView.GetForCurrentView();
            await view.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);

            ((App)Application.Current).ApplyCompactOverlayChromeStyle();

            this.IsInCompactOverlayMode = true;
        }

        private async Task LeaveCompactOverlayMode()
        {
            var view = ApplicationView.GetForCurrentView();
            await view.TryEnterViewModeAsync(ApplicationViewMode.Default);

            ((App)Application.Current).ApplyDefaultViewChromeStyle();

            this.IsInCompactOverlayMode = false;
        }

        private void OnPageSizeChanged(SizeChangedEventArgs e)
        {
            if(this.ViewModeStates.CurrentState == this.CompactOverlayState)
            {
                // Determines whether the card will be displayed with vertical or horizontal paddings
                // The 60px accounts for the bottom menu bar in compact state
                var sizePreviewByWidth = e.NewSize.Width < (e.NewSize.Height * 1.8) - 60;

                var newWidth = sizePreviewByWidth
                    ? e.NewSize.Width
                    : e.NewSize.Height * 1.8;

                var newHeight = sizePreviewByWidth
                    ? (10 / (double)18) * e.NewSize.Width
                    : e.NewSize.Height - 60;

                this.CardPreview.Width = newWidth;
                this.CardPreview.Height = newHeight;
            }
            else
            {
                this.CardPreview.Width = (double)App.Current.Resources["InkCardWidth"];
                this.CardPreview.Height = (double)App.Current.Resources["InkCardHeight"];
            }

            this.CardPreview.Render();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
