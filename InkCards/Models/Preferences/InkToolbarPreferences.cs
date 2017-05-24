using GalaSoft.MvvmLight;

namespace InkCards.Models.Preferences
{
    public class InkToolbarPreferences : ObservableObject
    {

        private bool showGridLines;

        public bool ShowGridLines
        {
            get { return this.showGridLines; }
            set
            {
                if (this.showGridLines == value) return;

                this.showGridLines = value;
                this.RaisePropertyChanged(nameof(this.ShowGridLines));
            }
        }

        #region Pencil Settings

        private double pencilWidth;

        public double PencilWidth
        {
            get { return this.pencilWidth; }
            set
            {
                if (this.pencilWidth == value) return;

                this.pencilWidth = value;
                this.RaisePropertyChanged(nameof(this.PencilWidth));
            }
        }

        private int pencilBrushIndex;

        public int PencilBrushIndex
        {
            get { return this.pencilBrushIndex; }
            set
            {
                if (this.pencilBrushIndex == value) return;

                this.pencilBrushIndex = value;
                this.RaisePropertyChanged(nameof(this.PencilBrushIndex));
            }
        }

        #endregion

        #region Pen Settings

        private double penWidth;

        public double PenWidth
        {
            get { return this.penWidth; }
            set
            {
                if (this.penWidth == value) return;

                this.penWidth = value;
                this.RaisePropertyChanged(nameof(this.PenWidth));
            }
        }

        private int penBrushIndex;

        public int PenBrushIndex
        {
            get { return this.penBrushIndex; }
            set
            {
                if (this.penBrushIndex == value) return;

                this.penBrushIndex = value;
                this.RaisePropertyChanged(nameof(this.PenBrushIndex));
            }
        }

        #endregion

        #region Highlighter Settings

        private double highlighterWidth;

        public double HighlighterWidth
        {
            get { return this.highlighterWidth; }
            set
            {
                if (this.highlighterWidth == value) return;

                this.highlighterWidth = value;
                this.RaisePropertyChanged(nameof(this.HighlighterWidth));
            }
        }

        private int highlighterBrushIndex;

        public int HighlighterBrushIndex
        {
            get { return this.highlighterBrushIndex; }
            set
            {
                if (this.highlighterBrushIndex == value) return;

                this.highlighterBrushIndex = value;
                this.RaisePropertyChanged(nameof(this.HighlighterBrushIndex));
            }
        }

        #endregion
    }
}
