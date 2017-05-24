using GalaSoft.MvvmLight;
using InkCards.Models.Cards;
using InkCards.Models.Testing;
using InkCards.Services.Navigation;
using InkCards.Services.Storage;
using InkCards.Services.Testing;
using InkCards.ViewModels.Pages.Args;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InkCards.ViewModels.Pages
{
    public class FlashcardTestViewModel : ViewModelBase
    {
        public long MaxImpressionLength { get; } = 15000;

        private readonly ICardStorageService cardStorageService;
        private readonly ICardImpressionsStorageService cardImpressionsStorageService;
        private readonly IFlashcardSessionTestingService flashcardSessionTestingService;

        private InkCard card;
        private bool isFlipped;
        private IEnumerable<CardCollection> collection;

        private CardImpression currentImpression;
        private Stopwatch impressionStopwatch = new Stopwatch();
        private Timer impressionElapsedUpdater;
        private readonly INavigationService navigationService;

        public InkCard Card
        {
            get { return this.card; }
            set
            {
                if (this.card == value) return;

                this.card = value;
                this.RaisePropertyChanged(nameof(this.Card));
            }
        }

        public IEnumerable<CardCollection> Collections
        {
            get { return this.collection; }
            set
            {
                if (this.collection == value) return;

                this.collection = value;
                this.RaisePropertyChanged(nameof(this.Collections));
                this.RaisePropertyChanged(nameof(this.Title));
            }
        }

        public string Title => this.Collections != null && this.Collections.Count() == 1
            ? this.Collections.First().Name
            : $"{this.Collections?.Count()} collections";

        public bool IsFlipped
        {
            get { return this.isFlipped; }
            set
            {
                if (this.isFlipped == value) return;

                this.isFlipped = value;
                this.RaisePropertyChanged(nameof(this.IsFlipped));
            }
        }


        private bool isRevealed;

        public bool IsRevealed
        {
            get { return this.isRevealed; }
            set
            {
                if (this.isRevealed == value) return;

                this.isRevealed = value;
                this.RaisePropertyChanged(nameof(this.IsRevealed));
            }
        }

        private bool inMovieMode;

        public bool InMovieMode
        {
            get { return this.inMovieMode; }
            set
            {
                if (this.inMovieMode == value) return;

                this.inMovieMode = value;
                this.RaisePropertyChanged(nameof(this.InMovieMode));
            }
        }

        public long ImpressionMillisecondsElapsed => this.GetMillisecondsElapsed();

        public event Action CardUpdated;

        public FlashcardTestViewModel(
            ICardStorageService cardStorageService,
            ICardImpressionsStorageService cardImpressionsStorageService,
            IFlashcardSessionTestingService flashcardSessionTestingService,
            INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.cardStorageService = cardStorageService;
            this.cardImpressionsStorageService = cardImpressionsStorageService;
            this.flashcardSessionTestingService = flashcardSessionTestingService;

            this.impressionElapsedUpdater = new Timer(_ => this.UpdateImpressionElapsed(), null, 0, 16); // corresponds to 60fps
        }

        public async Task Initialize(FlashcardTestArgs args)
        {
            this.Collections = await this.cardStorageService.GetCollections(args.CollectionIds);

            await this.flashcardSessionTestingService.Initialize(args.CollectionIds, args.CardSideToTest);

            await this.BeginCardImpression();

            foreach (var collection in this.Collections)
            {
                collection.LastOpened = DateTime.Now;
                await this.cardStorageService.SaveCollection(collection);
            }
            
        }

        public void Teardown()
        {
            this.impressionElapsedUpdater.Dispose();
            this.impressionStopwatch.Stop();
        }

        public void Reveal()
        {
            impressionStopwatch.Stop();
            this.currentImpression.FrontMillisecondsSpent = GetMillisecondsElapsed();

            this.IsFlipped = !this.IsFlipped;
            this.IsRevealed = true;

            impressionStopwatch.Restart();

            this.CardUpdated?.Invoke();
        }

        public void FlipIfRevealed()
        {
            if (!this.isRevealed) return;

            this.IsFlipped = !this.IsFlipped;

            this.CardUpdated?.Invoke();
        }

        public async void GuessedCorrectly()
        {
            await this.EndCardImpression(true);
            await this.BeginCardImpression();
        }

        public async void GuessedIncorrectly()
        {
            await this.EndCardImpression(false);
            await this.BeginCardImpression();
        }

        public void End() => this.navigationService.Navigate(PageType.MainPage, null);

        public void ToggleMovieMode() => this.InMovieMode = !this.InMovieMode;

        private async Task BeginCardImpression()
        {
            var nextCardToTest = await this.flashcardSessionTestingService.GetNextFlashcard();

            this.IsFlipped = nextCardToTest.CardSideToTest == CardSide.Front;
            this.IsRevealed = false;

            this.Card = await this.cardStorageService.GetCard(nextCardToTest.CollectionId, nextCardToTest.CardId);
            this.currentImpression = new CardImpression
            {
                CardId = nextCardToTest.CardId,
                Date = DateTime.Now,
                TestedSide = nextCardToTest.CardSideToTest
            };

            impressionStopwatch.Restart();

            this.CardUpdated?.Invoke();
        }

        private async Task EndCardImpression(bool guessedCorrectly)
        {
            impressionStopwatch.Stop();

            this.currentImpression.BackMillisecondsSpent = this.GetMillisecondsElapsed();
            this.currentImpression.GuessedCorrectly = guessedCorrectly;

            await this.cardImpressionsStorageService.AddImpression(currentImpression);
        }

        private long GetMillisecondsElapsed()
        {
            return impressionStopwatch.ElapsedMilliseconds < this.MaxImpressionLength
                ? impressionStopwatch.ElapsedMilliseconds
                : this.MaxImpressionLength;
        }

        private async void UpdateImpressionElapsed()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => this.RaisePropertyChanged(nameof(this.ImpressionMillisecondsElapsed)));
        }
    }
}
