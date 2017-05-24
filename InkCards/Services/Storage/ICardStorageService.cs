using InkCards.Models.Cards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InkCards.Services.Storage
{
    public interface ICardStorageService
    {
        Task<IEnumerable<CardCollection>> GetCollections();
        Task<IEnumerable<CardCollection>> GetCollections(IEnumerable<Guid> collectionIds);
        Task<CardCollection> GetCollection(Guid collectionId);
        Task SaveCollection(CardCollection collection);
        Task DeleteCollection(Guid collectionId);

        Task<IEnumerable<InkCard>> GetCards(IEnumerable<Guid> collectionIds);
        Task<IEnumerable<InkCard>> GetCards(Guid cardCollectionId);
        Task<InkCard> GetCard(Guid cardCollectionId, Guid cardId);

        Task LoadCard(InkCard card);
        Task SaveCard(InkCard card);
        Task DeleteCard(InkCard card);
    }
}
