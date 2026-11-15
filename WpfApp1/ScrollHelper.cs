using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public static class ScrollHelper
    {
        public static readonly DependencyProperty ScrollOffsetProperty =
            DependencyProperty.RegisterAttached("ScrollOffset", typeof(double), typeof(ScrollHelper),
                new PropertyMetadata(0.0, (d, e) => (d as ScrollViewer)?.ScrollToVerticalOffset((double)e.NewValue)));

        public static void SetScrollOffset(DependencyObject target, double value) => target.SetValue(ScrollOffsetProperty, value);
        public static double GetScrollOffset(DependencyObject target) => (double)target.GetValue(ScrollOffsetProperty);
    }
}
