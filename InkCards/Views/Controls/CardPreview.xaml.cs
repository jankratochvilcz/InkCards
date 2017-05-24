using InkCards.Models.Cards;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace InkCards.Views.Controls
{
    public sealed partial class CardPreview : UserControl, INotifyPropertyChanged
    {
        private InkCard card;
        private bool isFlipped;
        private bool isRendered;

        public double Scale { get; set; }

        public bool RenderManually { get; set; }

        public bool AllowFlipOnTapped { get; set; }

        public InkCard Card
        {
            get { return this.card; }
            set
            {
                if (this.card == value) return;

                if (this.card != null) this.card.PropertyChanged -= this.CardOnPropertyChanged;
                if (value != null) value.PropertyChanged += this.CardOnPropertyChanged;

                this.card = value;
                if (!this.RenderManually) this.Render();
            }
        }

        public bool IsFlipped
        {
            get { return this.isFlipped; }
            set
            {
                if (this.isFlipped == value) return;

                this.isFlipped = value;
                if (!this.RenderManually) this.Render();
            }
        }

        public bool IsRendered
        {
            get { return this.isRendered; }
            set
            {
                if (this.isRendered == value) return;

                this.isRendered = value;
                this.OnPropertyChanged(nameof(this.IsRendered));
                if (value) this.FinishedRendering?.Invoke();
            }
        }

        public event Action FinishedRendering;

        public CardPreview()
        {
            this.InitializeComponent();

            this.PreviewImage.InkPresenter.IsInputEnabled = false;
            this.PreviewImage.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.None;

            this.RegisterPropertyChangedCallback(CardPreview.RequestedThemeProperty, (x, _) => this.Render());
        }

        private void CardOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(this.Card.CardFrontInk) && !this.IsFlipped) ||
                (e.PropertyName == nameof(this.Card.CardBackInk) && this.IsFlipped))
                this.Render();
        }

        public async void Render()
        {
            this.IsRendered = false;
            this.PreviewImage.Opacity = 0;

            var stream = !this.IsFlipped
                ? this.Card?.CardFrontInk
                : this.Card?.CardBackInk;

            try
            {
                if (stream == null || stream.Size == 0)
                {
                    this.PreviewImage.InkPresenter.StrokeContainer.Clear();
                    this.IsRendered = true;
                    return;
                }

                this.PreviewImage.InkPresenter.StrokeContainer.Clear();

                using (var inputStream = stream.GetInputStreamAt(0))
                {
                    await this.PreviewImage.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                }

                var originalWidth = (double)Application.Current.Resources["InkCardWidth"];
                var originalHeight = (double)Application.Current.Resources["InkCardHeight"];
                var widthTransformRatio = this.Width / originalWidth;
                var heightTransformRatio = this.Height / originalHeight;

                var strokes = this.PreviewImage.InkPresenter.StrokeContainer.GetStrokes().ToList();
                
                double leftOffsetAdjustment = this.GetAdjustment(
                    originalWidth,
                    widthTransformRatio,
                    strokes,
                    x => x.BoundingRect.Left,
                    x => x.BoundingRect.Right);

                double topOffsetAdjustment = this.GetAdjustment(
                    originalHeight,
                    heightTransformRatio,
                    strokes,
                    x => x.BoundingRect.Top,
                    x => x.BoundingRect.Bottom);

                foreach (var stroke in strokes)
                    this.TransformStroke(
                        widthTransformRatio,
                        heightTransformRatio,
                        stroke,
                        leftOffsetAdjustment,
                        topOffsetAdjustment);

                //PreviewImage.Visibility = Visibility.Visible;
                this.IsRendered = true;
                this.PreviewImage.Opacity = 1;
            }
            catch (Exception)
            {
                this.IsRendered = true;
            }

            var streamx = new InMemoryRandomAccessStream();
            var bitmap = new BitmapImage();
            await this.PreviewImage.InkPresenter.StrokeContainer.SaveAsync(streamx);
        }

        private double GetAdjustment(
            double originalLength,
            double transformationRatio,
            List<InkStroke> strokes,
            Func<InkStroke, double> beginBoundary,
            Func<InkStroke, double> endBoundary)
        {
            var originalBeginMargin = strokes.Min(beginBoundary);
            var originalEndMargin = originalLength - strokes.Max(endBoundary);
            var targetEqualMargin = (originalBeginMargin + originalEndMargin) / 2;
            var adjustment = (targetEqualMargin - originalBeginMargin) * transformationRatio;
            return adjustment;
        }

        private void TransformStroke(
            double widthTransformRatio,
            double heightTransformRatio,
            InkStroke stroke,
            double leftOffsetAdjustment,
            double topOffsetAdjustment)
        {
            var transform = Matrix3x2.CreateScale(
                (float)widthTransformRatio,
                (float)heightTransformRatio);

            transform.Translation = new Vector2(
                (float)leftOffsetAdjustment,
                (float)topOffsetAdjustment);

            stroke.PointTransform = transform;


            // Scales the ink to not be to thick when shrunk down
            // Have to do the get, set business, otherwise doesn't work (unsure why)
            // https://docs.microsoft.com/en-us/uwp/api/Windows.UI.Input.Inking.InkDrawingAttributes#Windows_UI_Input_Inking_InkDrawingAttributes_Size
            var attributes = stroke.DrawingAttributes;
            var strokeSize = new Size(stroke.DrawingAttributes.Size.Width * widthTransformRatio, stroke.DrawingAttributes.Size.Height * heightTransformRatio);
            attributes.Size = strokeSize;

            if (this.RequestedTheme == ElementTheme.Dark) this.InvertStrokeColor(stroke, attributes);

            stroke.DrawingAttributes = attributes;
        }

        private void InvertStrokeColor(InkStroke stroke, InkDrawingAttributes attributes)
            => attributes.Color = Color.FromArgb(
                stroke.DrawingAttributes.Color.A,
                (byte)(stroke.DrawingAttributes.Color.R ^ 0xff),
                (byte)(stroke.DrawingAttributes.Color.G ^ 0xff),
                (byte)(stroke.DrawingAttributes.Color.B ^ 0xff));

        private void UserControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!this.AllowFlipOnTapped) return;

            this.IsFlipped = !this.IsFlipped;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}
