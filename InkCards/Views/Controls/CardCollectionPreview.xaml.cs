using InkCards.Models.Cards;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace InkCards.Views.Controls
{
    public sealed partial class CardCollectionPreview : UserControl, INotifyPropertyChanged
    {
        private CardCollection collection;

        public CardCollection Collection
        {
            get { return this.collection; }
            set
            {
                if (this.collection == value) return;

                this.collection = value;
                this.OnPropertyChanged(nameof(this.Collection));
            }
        }

        public CardCollectionPreview()
        {
            this.InitializeComponent();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
