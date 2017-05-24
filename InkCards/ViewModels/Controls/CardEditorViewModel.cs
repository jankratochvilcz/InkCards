using GalaSoft.MvvmLight;
using InkCards.Models.Preferences;
using InkCards.Services.Storage;
using System.ComponentModel;

namespace InkCards.ViewModels.Controls
{
    public class CardEditorViewModel : ViewModelBase
    {
        private InkToolbarPreferences inkToolbarPreferences;

        public InkToolbarPreferences InkToolbarPreferences
        {
            get { return this.inkToolbarPreferences; }
            set
            {
                if (this.inkToolbarPreferences == value) return;

                if(this.inkToolbarPreferences != null)
                    this.inkToolbarPreferences.PropertyChanged -= InkToolbarPreferencesChanged;
                if (value != null)
                    value.PropertyChanged += InkToolbarPreferencesChanged;

                this.inkToolbarPreferences = value;
                this.RaisePropertyChanged(nameof(this.InkToolbarPreferences));
            }
        }

        readonly IPreferencesService preferencesService;
        private string toolbarId;

        public CardEditorViewModel(
            IPreferencesService preferencesService)
        {
            this.preferencesService = preferencesService;
        }

        public void Initialize(string toolbarId)
        {
            this.toolbarId = toolbarId;
            this.InkToolbarPreferences = this.preferencesService.GetInkToolbarPreferences(toolbarId);
        }

        private void InkToolbarPreferencesChanged(object sender, PropertyChangedEventArgs e)
            => this.SaveToolbarPreferences();

        private void SaveToolbarPreferences()
            => this.preferencesService.SetInkToolbarPreferences(this.toolbarId, this.InkToolbarPreferences);
    }
}
