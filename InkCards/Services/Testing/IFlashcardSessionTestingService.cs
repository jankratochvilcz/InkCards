using InkCards.ViewModels.Pages.Args;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InkCards.Services.Testing
{
    public interface IFlashcardSessionTestingService
    {
        Task EvaluateResult(Guid cardId, bool guessedCorrectly);
        Task<NextCardToTest> GetNextFlashcard();
        Task Initialize(IEnumerable<Guid> collectionIds, CardSideToTest sideToTest);
    }
}