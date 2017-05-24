using InkCards.Infrastructure.Extensions;
using InkCards.Models.Cards;
using InkCards.Models.Testing;
using InkCards.Services.Storage.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InkCards.Services.Storage
{
    public class SqliteCardImpressionStorageService : ICardImpressionsStorageService
    {
        public async Task AddImpression(CardImpression impression)
        {
            using (var context = new MainDatabaseContext())
            {
                await context.CardImpressions.AddAsync(impression);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CardImpression>> GetImpressions(IEnumerable<Guid> cardIds)
        {
            using (var context = new MainDatabaseContext())
            {
                return await context.CardImpressions
                    .Where(x => cardIds.Any(y => y == x.CardId))
                    .AsNoTracking()
                    .ToListAsync();
            }
        }

        public async Task<List<Guid>> OrderByLongestUnseen(IEnumerable<Guid> cards)
        {
            using (var context = new MainDatabaseContext())
            {
                var orderedImpressions = await context.CardImpressions
                    .Where(x => cards.Any(y => y == x.CardId))
                    .GroupBy(x => x.CardId)
                    .OrderBy(x => x.Max(y => y.Date))
                    .Select(x => x.Key)
                    .ToListAsync();

                var newCards = cards.Except(orderedImpressions);
                return newCards.Union(orderedImpressions).ToList();
            }
        }

        public async Task<List<Guid>> OrderByLongestImpression(
            IEnumerable<Guid> cards, 
            int historyDepth,
            CardSide side)
        {
            using (var context = new MainDatabaseContext())
            {
                var accountedForImpressions = await context.CardImpressions
                    .Where(x => cards.Any(y => y == x.CardId))
                    .Where(x => x.TestedSide == side)
                    .GroupBy(x => x.CardId)
                    .ToDictionaryAsync(
                        x => x.Key,
                        x => x.OrderByDescending(y => y.Date).Take(historyDepth));

                var impressionsOrderedBySpentMedian = accountedForImpressions
                    .OrderByDescending(x => x.Value.Select(y => side == CardSide.Front
                        ? y.FrontMillisecondsSpent
                        : y.BackMillisecondsSpent)
                        .Median())
                    .Select(x => x.Key);

                var newCards = cards.Except(impressionsOrderedBySpentMedian);
                return newCards.Union(impressionsOrderedBySpentMedian).ToList();
            }
        }

        public async Task<List<Guid>> OrderByMostUnsuccessful(
            IEnumerable<Guid> cardIds,
            int historyDepth,
            CardSide side)
        {
            using (var context = new MainDatabaseContext())
            {
                var accountedForImpressions = await context.CardImpressions
                    .Where(x => cardIds.Any(y => y == x.CardId))
                    .Where(x => x.TestedSide == side)
                    .GroupBy(x => x.CardId)
                    .ToDictionaryAsync(
                        x => x.Key,
                        x => x.OrderByDescending(y => y.Date).Take(historyDepth));

                var impressionsOrderedBySuccessAverage = accountedForImpressions
                    .OrderBy(x => x.Value.Select(y => y.GuessedCorrectly ? 10 : 0)
                        .Average())
                    .Select(x => x.Key);

                var newCards = cardIds.Except(impressionsOrderedBySuccessAverage);
                return newCards.Union(impressionsOrderedBySuccessAverage).ToList();
            }
        }
    }
}
