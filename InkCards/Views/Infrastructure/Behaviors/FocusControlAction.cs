using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace InkCards.Views.Infrastructure.Behaviors
{
    public class FocusControlAction : DependencyObject, IAction
    {
        public Control TargetElement
        {
            get { return (Control)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("TargetElement", typeof(Control), typeof(FocusControlAction), new PropertyMetadata(default(Control)));

        public object Execute(object sender, object parameter)
        {
            if (this.TargetElement == null) return false;

            this.TargetElement.Focus(FocusState.Programmatic);
            if (this.TargetElement is TextBox) ((TextBox)this.TargetElement).SelectAll();
            return true;
        }
    }
}
