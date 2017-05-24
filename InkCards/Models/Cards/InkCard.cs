using System;
using System.ComponentModel;
using Windows.Storage.Streams;

namespace InkCards.Models.Cards
{
    public class InkCard : INotifyPropertyChanged
    {
        private IRandomAccessStream cardBack;
        private IRandomAccessStream cardFront;

        public Guid CardId { get; set; }

        public Guid CardCollectionId { get; set; }

        public DateTime Created { get; set; }

        public IRandomAccessStream CardFrontInk
        {
            get { return this.cardFront; }
            set
            {
                if (this.cardFront == value) return;

                this.cardFront = value;
                this.OnPropertyChanged(nameof(this.CardFrontInk));
            }
        }

        public IRandomAccessStream CardBackInk
        {
            get { return this.cardBack; }
            set
            {
                if (this.cardBack == value) return;

                this.cardBack = value;
                this.OnPropertyChanged(nameof(this.CardBackInk));
            }
        }


        private Uri cardFrontUri;

        public Uri CardFrontUri
        {
            get { return this.cardFrontUri; }
            set
            {
                this.cardFrontUri = value;
                this.OnPropertyChanged(nameof(this.CardFrontUri));
            }
        }


        private Uri cardBackUri;

        public Uri CardBackUri
        {
            get { return this.cardBackUri; }
            set
            {
                this.cardBackUri = value;
                this.OnPropertyChanged(nameof(this.CardBackUri));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
