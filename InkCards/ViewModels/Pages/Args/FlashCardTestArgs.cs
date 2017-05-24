using System;
using System.Collections.Generic;

namespace InkCards.ViewModels.Pages.Args
{
    public class FlashcardTestArgs
    {
        public CardSideToTest CardSideToTest { get; }
        public IEnumerable<Guid> CollectionIds { get; }

        public FlashcardTestArgs(
            CardSideToTest cardSideToTest,
            IEnumerable<Guid> collectionIdsToTest)
        {
            CollectionIds = collectionIdsToTest;
            CardSideToTest = cardSideToTest;
        }
    }
}
