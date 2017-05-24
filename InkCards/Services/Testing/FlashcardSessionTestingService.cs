using InkCards.Models.Cards;
using InkCards.Services.Storage;
using InkCards.ViewModels.Pages.Args;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InkCards.Services.Testing
{
    public class FlashcardSessionTestingService : IFlashcardSessionTestingService
    {
        private readonly ICardStorageService cardStorageService;
        private readonly ICardImpressionsStorageService impressionStorageService;

        private Dictionary<Guid, List<Guid>> Cards { get; set; }
        public Guid LastCardId { get; set; }

        Random random = new Random();
        private CardSideToTest sideToTest;

        public FlashcardSessionTestingService(
            ICardStorageService cardStorageService,
            ICardImpressionsStorageService impressionStorageService)
        {
            this.cardStorageService = cardStorageService;
            this.impressionStorageService = impressionStorageService;
        }

        public async Task Initialize(IEnumerable<Guid> collectionIds, CardSideToTest sideToTest)
        {
            this.sideToTest = sideToTest;
            this.Cards = (await Task.WhenAll(collectionIds
                .Select(async x => new
                {
                    CollectionId = x,
                    CardIds = await this.cardStorageService.GetCards(x)
                })))
                .ToDictionary(x => x.CollectionId, x => x.CardIds.Select(y => y.CardId).ToList());
        }

        public async Task<NextCardToTest> GetNextFlashcard()
        {
            CardSide cardSideToTest = this.GetCardSideToTest();

            var orderByUnseenTime = await this.impressionStorageService.OrderByLongestUnseen(this.Cards.SelectMany(x => x.Value));
            var orderByImpressionLength = await this.impressionStorageService.OrderByLongestImpression(this.Cards.SelectMany(x => x.Value), 3, cardSideToTest);
            var orderByUnsuccess = await this.impressionStorageService.OrderByMostUnsuccessful(this.Cards.SelectMany(x => x.Value), 5, cardSideToTest);

            var rankings = this.Cards.SelectMany(x => x.Value).Select(card =>
            {
                var unseenOrder = orderByUnseenTime.IndexOf(card);
                var impressionLengthOrder = orderByImpressionLength.IndexOf(card);
                var unsuccessOrder = orderByUnsuccess.IndexOf(card);

                var rank = 
                    unsuccessOrder * 55 + 
                    impressionLengthOrder * 10 + 
                    unseenOrder * 20 + 
                    random.Next(this.Cards.Count - 1) * 15;

                return new
                {
                    CardId = card,
                    Rank = rank
                };
            })
            .OrderBy(x => x.Rank)
            .ToList();

            var cardId = this.LastCardId != rankings.First().CardId
                ? rankings.First().CardId
                : rankings.Skip(1).First().CardId;

            this.LastCardId = cardId;

            return new NextCardToTest(cardId, cardSideToTest, this.Cards.First(x => x.Value.Any(y => y == cardId)).Key);
        }

        private CardSide GetCardSideToTest()
        {
            CardSide cardSideToTest = CardSide.Front;
            switch (this.sideToTest)
            {
                case CardSideToTest.Both:
                    cardSideToTest = this.random.Next(100) > 50
                        ? CardSide.Back
                        : CardSide.Front;
                    break;
                case CardSideToTest.Front:
                    cardSideToTest = CardSide.Front;
                    break;
                case CardSideToTest.Back:
                    cardSideToTest = CardSide.Back;
                    break;
            }

            return cardSideToTest;
        }

        public Task EvaluateResult(Guid cardId, bool guessedCorrectly)
        {
            return Task.CompletedTask;
        }
    }
}
