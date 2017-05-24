using GalaSoft.MvvmLight;
using InkCards.Models.Cards;
using InkCards.Services;
using InkCards.Services.Navigation;
using InkCards.Services.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace InkCards.ViewModels.Pages
{
    public class CardsBrowseViewModel : ViewModelBase
    {
        private readonly ICardStorageService cardStorageService;
        private readonly INavigationService navigationService;
        private readonly ICardOrderingService cardOrderingService;

        private IEnumerable<CardCollection> collections;
        private int cardsCount;
        private bool isLoading;

        private IEnumerable<CardCollection> Collections
        {
            get { return this.collections ?? Enumerable.Empty<CardCollection>(); }
            set
            {
                if (this.collections == value) return;

                this.collections = value;

                this.RaisePropertyChanged(nameof(this.CollectionTitle));
            }
        }

        public string CollectionTitle => this.Collections.Count() == 1
            ? this.Collections.First().Name
            : $"{this.Collections.Count()} collections";

        public ObservableCollection<InkCard> Cards { get; } = new ObservableCollection<InkCard>();

        public int CardsCount
        {
            get { return this.cardsCount; }
            private set
            {
                if (this.cardsCount == value) return;

                this.cardsCount = value;
                this.RaisePropertyChanged(nameof(this.CardsCount));
            }
        }

        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                if (this.isLoading == value) return;

                this.isLoading = value;
                this.RaisePropertyChanged(nameof(this.IsLoading));
            }
        }


        private int orderType;

        public int OrderType
        {
            get { return this.orderType; }
            set
            {
                if (this.orderType == value) return;

                this.orderType = value;
                this.Load(this.Collections);
            }
        }

        public int MaxCardCountToDisplay { get; set; } = int.MaxValue;

        public CardsBrowseViewModel(
            ICardStorageService cardStorageService,
            INavigationService navigationService,
            ICardOrderingService cardOrderingService)
        {
            this.cardOrderingService = cardOrderingService;
            this.navigationService = navigationService;
            this.cardStorageService = cardStorageService;
        }

        public async Task Load(IEnumerable<CardCollection> collections)
        {
            this.IsLoading = true;
            this.Collections = collections;
            this.Cards.Clear();

            if (collections == null || !collections.Any()) return;
            var cards = await this.cardStorageService.GetCards(collections.Select(x => x.Id));
            var orderedCards = await this.cardOrderingService.Order(cards, (CardOrderingType)this.OrderType);

            foreach (var card in orderedCards.Take(this.MaxCardCountToDisplay)) this.Cards.Add(card);

            this.CardsCount = cards.Count();
            this.IsLoading = false;
        }

        public async Task Load(IEnumerable<Guid> collectionIds)
        {
            var loadedCollections = await this.cardStorageService.GetCollections(collectionIds);
            await this.Load(loadedCollections);
        }

        public void GoToMainPage() 
            => this.navigationService.Navigate(PageType.MainPage, null);
    }
}
