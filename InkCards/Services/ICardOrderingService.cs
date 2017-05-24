using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InkCards.Models.Cards;

namespace InkCards.Services
{
    public interface ICardOrderingService
    {
        Task<IOrderedEnumerable<InkCard>> Order(IEnumerable<InkCard> cards, CardOrderingType orderType);
    }
}