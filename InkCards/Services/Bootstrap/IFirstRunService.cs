using System.Threading.Tasks;

namespace InkCards.Services.Bootstrap
{
    public interface IFirstRunService
    {
        Task InitializeIfFirstRun();
    }
}