using InkCards.Models.Preferences;

namespace InkCards.Services.Storage
{
    public interface IPreferencesService
    {
        InkToolbarPreferences GetInkToolbarPreferences(string toolbarId);
        void SetInkToolbarPreferences(string toolbarId, InkToolbarPreferences preferences);

        bool IsFirstRun { get; set; }
    }
}