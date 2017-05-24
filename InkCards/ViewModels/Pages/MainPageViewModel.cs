using GalaSoft.MvvmLight;
using InkCards.Models.Cards;
using InkCards.Services.Navigation;
using InkCards.Services.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using InkCards.ViewModels.Pages.Args;
using InkCards.Services.Bootstrap;

namespace InkCards.ViewModels.Pages
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly ICardStorageService cardStorageService;
        private readonly IFirstRunService firstRunService;

        private IEnumerable<CardCollection> selectedCollections;

        public ObservableCollection<CardCollectionGroup> CollectionGroups { get; } 
            = new ObservableCollection<CardCollectionGroup>();

        public IEnumerable<CardCollection> SelectedCollections
        {
            get { return this.selectedCollections ?? Enumerable.Empty<CardCollection>(); }
            set
            {
                if (this.selectedCollections == value) return;

                this.selectedCollections = value;

                this.CardsBrowseViewModel.Load(value);
                this.RaisePropertyChanged(nameof(this.IsEditAndDeleteAvailable));
            }
        }

        public bool IsEditAndDeleteAvailable => this.SelectedCollections.Count() == 1;

        public CardsBrowseViewModel CardsBrowseViewModel { get; set; }

        public event Action ClearSelectedCollections;

        public MainPageViewModel(
            INavigationService navigationService,
            ICardStorageService cardStorageService,
            CardsBrowseViewModel cardsBrowseViewModel,
            IFirstRunService firstRunService)
        {
            this.firstRunService = firstRunService;
            this.cardStorageService = cardStorageService;
            this.navigationService = navigationService;

            cardsBrowseViewModel.MaxCardCountToDisplay = 10;
            this.CardsBrowseViewModel = cardsBrowseViewModel;
        }

        public async Task Initialize()
        {
            await this.firstRunService.InitializeIfFirstRun();

            this.CollectionGroups.Clear();

            var collections = (await this.cardStorageService.GetCollections())
                .OrderByDescending(x => x.LastOpened);

            var newGroup = collections.Where(x => !x.LastOpened.HasValue);

            var todayGroup = collections
                .Except(newGroup)
                .Where(x => x.LastOpened.HasValue && x.LastOpened.Value.Date == DateTime.Now.Date);

            var lastSevenDaysGroup = collections
                .Except(newGroup)
                .Except(todayGroup)
                .Where(x => x.LastOpened.HasValue && x.LastOpened.Value.Date <= DateTime.Now.Date.AddDays(7));

            var olderGroup = collections
                .Except(newGroup)
                .Except(todayGroup)
                .Except(lastSevenDaysGroup);

            if (newGroup.Any()) this.CollectionGroups.Add(new CardCollectionGroup(newGroup, "New Collections"));
            if (todayGroup.Any()) this.CollectionGroups.Add(new CardCollectionGroup(todayGroup, "Today's Collections"));
            if (lastSevenDaysGroup.Any()) this.CollectionGroups.Add(new CardCollectionGroup(lastSevenDaysGroup, "Last 7 Days' Collections"));
            if (olderGroup.Any()) this.CollectionGroups.Add(new CardCollectionGroup(olderGroup, "Older Collections"));
        }

        public void NavigateToNewCardCollectionEdit()
            => this.navigationService.Navigate(PageType.CardDesignPage, null);

        public void NavigateToCardBrowsing()
            => this.navigationService.Navigate(PageType.CardsBrowse, this.SelectedCollections.Select(x => x.Id));

        public void NavigateToFeedback()
            => this.navigationService.Navigate(PageType.Feedback, null);

        public void BeginFlashCardTest()
            => this.navigationService.Navigate(PageType.FlashcardTestPage, new FlashcardTestArgs(CardSideToTest.Both, this.SelectedCollections.Select(x => x.Id) ));

        public void BeginFlashCardTestFront()
            => this.navigationService.Navigate(PageType.FlashcardTestPage, new FlashcardTestArgs(CardSideToTest.Front, this.SelectedCollections.Select(x => x.Id)));

        public void BeginFlashCardTestBack()
            => this.navigationService.Navigate(PageType.FlashcardTestPage, new FlashcardTestArgs(CardSideToTest.Back, this.SelectedCollections.Select(x => x.Id)));

        public void EditSelectedCollection()
            => this.navigationService.Navigate(PageType.CardDesignPage, this.SelectedCollections.First().Id);

        public async void DeleteSelectedCollection()
        {
            var collectionsToRemove = this.SelectedCollections;
            foreach(var collectionToRemove in this.SelectedCollections)
            {
                var affectedGroup = this.CollectionGroups.First(x => x.Any(y => y == collectionToRemove));
                affectedGroup.Remove(collectionToRemove);

                if (!affectedGroup.Any()) this.CollectionGroups.Remove(affectedGroup);
                await this.cardStorageService.DeleteCollection(collectionToRemove.Id);
            }
        }
    }

    public class CardCollectionGroup : List<CardCollection>, IGrouping<string, CardCollection>
    {
        public string Key { get; set; }

        public CardCollectionGroup(IEnumerable<CardCollection> cardCollections, string key)
        {
            this.Key = key;
            this.AddRange(cardCollections);
        }
    }
}