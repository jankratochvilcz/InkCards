using InkCards.Models.Cards;
using InkCards.Services.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InkCards.Services
{
    public class CardOrderingService : ICardOrderingService
    {
        readonly ICardImpressionsStorageService cardImpressionsStorageService;

        public CardOrderingService(
            ICardImpressionsStorageService cardImpressionsStorageService)
        {
            this.cardImpressionsStorageService = cardImpressionsStorageService;
        }

        public async Task<IOrderedEnumerable<InkCard>> Order(IEnumerable<InkCard> cards, CardOrderingType orderType)
        {
            switch (orderType)
            {
                case CardOrderingType.OldestFirst:
                    return cards.OrderBy(x => x.Created);
                case CardOrderingType.MostUnsuccessfulFirst:
                    var orderedIdsByMostUnsuccessful = (await this.cardImpressionsStorageService.OrderByMostUnsuccessful(cards.Select(x => x.CardId), 10, CardSide.Back));
                    return cards.OrderBy(x => orderedIdsByMostUnsuccessful.IndexOf(x.CardId));
                case CardOrderingType.LongestUnseenFirst:
                    var orderedIdsByLongestUnseen = await this.cardImpressionsStorageService.OrderByLongestUnseen(cards.Select(x => x.CardId));
                    return cards.OrderBy(x => orderedIdsByLongestUnseen.IndexOf(x.CardId));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
