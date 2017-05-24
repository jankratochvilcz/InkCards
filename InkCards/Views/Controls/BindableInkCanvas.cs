using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkCards.Views.Controls
{
    public class BindableInkCanvas : InkCanvas
    {
        public static readonly DependencyProperty StrokesProperty = DependencyProperty.RegisterAttached(
             "Strokes",
             typeof(InkStrokeContainer),
             typeof(BindableInkCanvas),
             new PropertyMetadata(null, StrokesChanged)
           );

        private static void StrokesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = d as BindableInkCanvas;
            if (instance != null)
            {
                instance.InkPresenter.StrokeContainer = instance.Strokes;
            }
        }

        public InkStrokeContainer Strokes
        {
            get { return (InkStrokeContainer)GetValue(StrokesProperty); }
            set { SetValue(StrokesProperty, value); }
        }
    }
}
