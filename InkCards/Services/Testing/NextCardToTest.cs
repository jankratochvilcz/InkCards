using InkCards.Models.Cards;
using System;

namespace InkCards.Services.Testing
{
    public class NextCardToTest
    {
        public Guid CardId { get; }

        public CardSide CardSideToTest { get; }

        public Guid CollectionId { get; set; }

        public NextCardToTest(
            Guid cardId,
            CardSide sideToTest,
            Guid collectionId)
        {
            this.CardSideToTest = sideToTest;
            this.CardId = cardId;
            this.CollectionId = collectionId;
        }
    }
}