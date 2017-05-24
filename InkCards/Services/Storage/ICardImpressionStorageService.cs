using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InkCards.Models.Testing;
using InkCards.Models.Cards;

namespace InkCards.Services.Storage
{
    public interface ICardImpressionsStorageService
    {
        Task AddImpression(CardImpression impression);

        Task<IEnumerable<CardImpression>> GetImpressions(IEnumerable<Guid> cardIds);

        Task<List<Guid>> OrderByLongestUnseen(IEnumerable<Guid> cardIds);

        Task<List<Guid>> OrderByLongestImpression(
            IEnumerable<Guid> cards,
            int historyDepth,
            CardSide side);

        Task<List<Guid>> OrderByMostUnsuccessful(
            IEnumerable<Guid> cardIds,
            int historyDepth,
            CardSide side);
    }
}