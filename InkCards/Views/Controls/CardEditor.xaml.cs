using InkCards.ViewModels.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Microsoft.Practices.Unity;
using InkCards.Models.Preferences;

namespace InkCards.Views.Controls
{
    public sealed partial class CardEditor : UserControl, INotifyPropertyChanged
    {
        private const int GridCellSize = 25;

        private CardEditorViewModel viewModel;
        private CardEditorViewModel ViewModel => this.viewModel ?? (this.viewModel = ((App)Application.Current).DependencyResolver.Resolve<CardEditorViewModel>());

        private string title;
        private bool inkToolBarInitialized;

        public bool ContainsAnyStrokes
        {
            get { return (bool)GetValue(ContainsAnyStrokesProperty); }
            set { SetValue(ContainsAnyStrokesProperty, value); }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title == value) return;

                this.title = value;
                this.OnPropertyChanged(nameof(this.Title));
            }
        }

        public string ToolbarId { get; set; }

        public InkCanvas Canvas => this.CardCanvas;

        public static readonly DependencyProperty ContainsAnyStrokesProperty =
            DependencyProperty.Register("ContainsAnyStrokes", typeof(bool), typeof(CardEditor), new PropertyMetadata(default(bool)));

        public event Action StrokesEdited;

        public CardEditor()
        {
            this.InitializeComponent();

            this.CardCanvas.InkPresenter.StrokesCollected += (x, _) => this.OnStrokesEdited();
            this.CardCanvas.InkPresenter.StrokesErased += (x, _) => this.OnStrokesEdited();
        }

        public void ClearStrokes() => this.CardCanvas.InkPresenter.StrokeContainer.Clear();

        public async Task<IRandomAccessStream> GetStrokesAsStream()
        {
            var stream = new InMemoryRandomAccessStream();
            await this.CardCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);

            return stream;
        }

        public async Task LoadStrokesFromStream(IRandomAccessStream stream)
        {
            if (stream == null || stream.Size == 0)
            {
                this.ClearStrokes();
                this.UpdateContainsAnyStrokes();
                return;
            }

            using (var inputStream = stream.GetInputStreamAt(0))
            {
                await this.CardCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
            }

            this.UpdateContainsAnyStrokes();
        }

        private void OnStrokesEdited()
        {
            this.UpdateContainsAnyStrokes();
            this.StrokesEdited?.Invoke();
        }

        private void UpdateContainsAnyStrokes() => this.ContainsAnyStrokes = this.CardCanvas.InkPresenter.StrokeContainer.GetStrokes().Any();

        private void RenderGridLines()
        {
            var columnsCount = ((double)Application.Current.Resources["InkCardWidth"] / GridCellSize) + 1;
            var rowsCount = ((double)Application.Current.Resources["InkCardHeight"] / GridCellSize) + 1;
            var brush = (Brush)Application.Current.Resources["GridLineBrush"];

            this.GridLinesGrid.ColumnDefinitions.Clear();
            this.GridLinesGrid.RowDefinitions.Clear();

            for (int i = 0; i < columnsCount; i++)
                this.GridLinesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridCellSize) });
            for (int i = 0; i < rowsCount; i++)
                this.GridLinesGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridCellSize) });

            for (int i = 0; i < columnsCount; i++)
            {
                var line = new Rectangle
                {
                    Width = 1,
                    Fill = brush,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                this.GridLinesGrid.Children.Add(line);
                Grid.SetColumn(line, i);
                Grid.SetRowSpan(line, (int)rowsCount);
            }

            for (int i = 0; i < rowsCount; i++)
            {
                var line = new Rectangle
                {
                    Height = 1,
                    Fill = brush,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                this.GridLinesGrid.Children.Add(line);
                Grid.SetRow(line, i);
                Grid.SetColumnSpan(line, (int)columnsCount);
            }
        }

        private void SaveInkToolbarPreferences(InkToolbar sender)
        {
            if (!this.inkToolBarInitialized) return;

            var pencilButton = (InkToolbarPenButton)this.InkToolbar.GetToolButton(InkToolbarTool.Pencil);
            this.ViewModel.InkToolbarPreferences.PencilWidth = pencilButton.SelectedStrokeWidth;
            this.ViewModel.InkToolbarPreferences.PencilBrushIndex = pencilButton.SelectedBrushIndex;

            var penButton = (InkToolbarPenButton)this.InkToolbar.GetToolButton(InkToolbarTool.BallpointPen);
            this.ViewModel.InkToolbarPreferences.PenWidth = penButton.SelectedStrokeWidth;
            this.ViewModel.InkToolbarPreferences.PenBrushIndex = penButton.SelectedBrushIndex;

            var highlighterButton = (InkToolbarPenButton)this.InkToolbar.GetToolButton(InkToolbarTool.Highlighter);
            this.ViewModel.InkToolbarPreferences.HighlighterWidth = highlighterButton.SelectedStrokeWidth;
            this.ViewModel.InkToolbarPreferences.HighlighterBrushIndex = highlighterButton.SelectedBrushIndex;
        }

        private void LoadInkToolbarPreferences(InkToolbarPreferences preferences)
        {
            var pencilButton = (InkToolbarPenButton)this.InkToolbar.GetToolButton(InkToolbarTool.Pencil);
            pencilButton.SelectedStrokeWidth = preferences.PencilWidth;
            pencilButton.SelectedBrushIndex = preferences.PencilBrushIndex;

            var penButton = (InkToolbarPenButton)this.InkToolbar.GetToolButton(InkToolbarTool.BallpointPen);
            penButton.SelectedStrokeWidth = preferences.PenWidth;
            penButton.SelectedBrushIndex = preferences.PenBrushIndex;

            var highlighterButton = (InkToolbarPenButton)this.InkToolbar.GetToolButton(InkToolbarTool.Highlighter);
            highlighterButton.SelectedStrokeWidth = preferences.HighlighterWidth;
            highlighterButton.SelectedBrushIndex = preferences.HighlighterBrushIndex;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.RenderGridLines();
            this.ViewModel.Initialize(this.ToolbarId);
        }

        private void InkToolbar_InkDrawingAttributesChanged(InkToolbar sender, object args)
        {
            if (!this.inkToolBarInitialized && sender.InkDrawingAttributes != null)
            {
                this.LoadInkToolbarPreferences(this.ViewModel.InkToolbarPreferences);
                this.inkToolBarInitialized = true;

                return;
            }

            this.SaveInkToolbarPreferences(sender);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
