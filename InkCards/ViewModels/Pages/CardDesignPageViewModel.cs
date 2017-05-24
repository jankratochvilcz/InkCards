using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using InkCards.Infrastructure.Extensions;
using InkCards.Models.Cards;
using InkCards.Services.Navigation;
using InkCards.Services.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace InkCards.ViewModels.Pages
{
    public class CardDesignPageViewModel : ViewModelBase
    {
        private readonly ICardStorageService cardStorageService;
        private readonly INavigationService navigationService;

        private Guid currentCardCollectionId;
        private string currentCardCollectionName;
        private InkCard currentlyEditedCard;
        private bool currentlyEditedCardFrontContainsStrokes;
        private bool currentlyEditedCardBackContainsStrokes;

        private RelayCommand addCardCommand;

        public string CurrentCardCollectionName
        {
            get { return this.currentCardCollectionName; }
            set
            {
                if (this.currentCardCollectionName == value) return;

                this.currentCardCollectionName = value;
                this.RaisePropertyChanged(nameof(this.CurrentCardCollectionName));

                this.UpdateCardCollection(new CardCollection
                {
                    Id = this.currentCardCollectionId,
                    Name = value
                });
            }
        }

        public ObservableCollection<InkCard> Cards { get; } = new ObservableCollection<InkCard>();

        public InkCard CurrentlyEditedCard
        {
            get { return this.currentlyEditedCard; }
            set
            {
                if (this.currentlyEditedCard == value) return;

                this.currentlyEditedCard = value;

                this.RaisePropertyChanged(nameof(this.CurrentlyEditedCard));

                if (value != null)
                    this.LoadStrokeSteams(value);
            }
        }

        public bool CurrentlyEditedCardFrontContainsStrokes
        {
            get { return currentlyEditedCardFrontContainsStrokes; }
            set
            {
                currentlyEditedCardFrontContainsStrokes = value;
                this.RaisePropertyChanged(nameof(this.CanSaveCurrenlyEditedCard));
                this.AddCardCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CurrentlyEditedCardBackContainsStrokes
        {
            get { return currentlyEditedCardBackContainsStrokes; }
            set
            {
                currentlyEditedCardBackContainsStrokes = value;
                this.RaisePropertyChanged(nameof(this.CanSaveCurrenlyEditedCard));
                this.AddCardCommand.RaiseCanExecuteChanged();
            }
        }

        public bool CanSaveCurrenlyEditedCard => this.CurrentlyEditedCardFrontContainsStrokes && this.CurrentlyEditedCardBackContainsStrokes;

        public RelayCommand AddCardCommand => this.addCardCommand ??
            (this.addCardCommand = new RelayCommand(this.AddCard, () => this.CurrentlyEditedCard == null || this.CanSaveCurrenlyEditedCard));

        public event Action StrokesForCurrentlyEditedCardLoaded;

        public CardDesignPageViewModel(
            ICardStorageService cardStorageService,
            INavigationService navigationService)
        {
            this.navigationService = navigationService;
            this.cardStorageService = cardStorageService;
        }

        public async Task Initialize(Guid cardCollectionId)
        {
            this.Cards.Clear();

            var cardCollection = await this.cardStorageService.GetCollection(cardCollectionId);
            if (cardCollection == null) await this.InitializeCardCollection(cardCollectionId);
            else
            {
                this.currentCardCollectionId = cardCollection.Id;
                this.CurrentCardCollectionName = cardCollection.Name;
            }


            var cards = await this.cardStorageService.GetCards(cardCollectionId);

            foreach (var existingCard in cards)
            {
                this.Cards.Add(existingCard);
            }

            if (!this.Cards.Any()) this.AddCard();
            else this.CurrentlyEditedCard = this.Cards.First();
        }

        public void AddCard()
        {
            var newCard = new InkCard
            {
                CardId = Guid.NewGuid(),
                CardCollectionId = this.currentCardCollectionId
            };

            this.Cards.Insert(0, newCard);
            this.CurrentlyEditedCard = newCard;
        }

        public async Task SaveCard(
            InkCard card,
            IRandomAccessStream cardFront,
            IRandomAccessStream cardBack)
        {
            this.CurrentlyEditedCard.CardFrontInk = cardFront;
            this.CurrentlyEditedCard.CardBackInk = cardBack;

            await this.cardStorageService.SaveCard(card);
        }

        public async void GoToMainPage()
        {
            if (!this.Cards.Any()) await this.cardStorageService.DeleteCollection(this.currentCardCollectionId);
            this.navigationService.Navigate(PageType.MainPage, null);
        }

        public void DeleteCard(InkCard card)
        {
            var currentlyEditedCardIndex = this.Cards.IndexOf(card);

            this.Cards.Remove(card);
            this.cardStorageService.DeleteCard(card);

            this.CurrentlyEditedCard = this.Cards.GetPreviousItemToSelect(currentlyEditedCardIndex);
        }

        private async void LoadStrokeSteams(InkCard card)
        {
            await this.cardStorageService.LoadCard(card);
            this.StrokesForCurrentlyEditedCardLoaded?.Invoke();
        }

        private async void UpdateCardCollection(CardCollection collection)
            => await this.cardStorageService.SaveCollection(collection);

        private async Task InitializeCardCollection(Guid collectionId)
        {
            if (collectionId == Guid.Empty) collectionId = Guid.NewGuid();

            var cardCollection = new CardCollection
            {
                Id = collectionId,
                Name = "Untitled"
            };

            this.currentCardCollectionId = collectionId;
            this.currentCardCollectionName = cardCollection.Name;
            this.RaisePropertyChanged(nameof(this.CurrentCardCollectionName));

            await this.cardStorageService.SaveCollection(cardCollection);
        }
    }
}
