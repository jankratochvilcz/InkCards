using InkCards.Models.Preferences;
using InkCards.Services.Storage.Extensions;
using Windows.Storage;
using System.Collections.Generic;

namespace InkCards.Services.Storage
{
    public class LocalDataPreferencesService : IPreferencesService
    {
        private const string ToolbarPreferencesPrefix = "ToolbarPreferences-";

        private readonly ApplicationDataContainer localSettings;

        public LocalDataPreferencesService()
        {
            this.localSettings = ApplicationData.Current.LocalSettings;
        }

        public bool IsFirstRun
        {
            get => this.localSettings.Values.ContainsKey(nameof(this.IsFirstRun))
                ? (bool)this.localSettings.Values[nameof(this.IsFirstRun)]
                : true;

            set
            {
                if (this.localSettings.Values.ContainsKey(nameof(this.IsFirstRun)))
                    this.localSettings.Values[nameof(this.IsFirstRun)] = value;
                else
                    this.localSettings.Values.Add(new KeyValuePair<string, object>(nameof(this.IsFirstRun), value));
            }
        }

        public InkToolbarPreferences GetInkToolbarPreferences(string toolbarId)
        {
            if (!this.localSettings.Values.ContainsKey(ToolbarPreferencesPrefix + toolbarId))
                return new InkToolbarPreferences();

            return ((ApplicationDataCompositeValue)this.localSettings.Values[ToolbarPreferencesPrefix + toolbarId])
                .FromCompositeValue();
        }

        public void SetInkToolbarPreferences(string toolbarId, InkToolbarPreferences preferences)
            => this.localSettings.Values[ToolbarPreferencesPrefix + toolbarId] = preferences.ToCompositeValue();
    }
}
