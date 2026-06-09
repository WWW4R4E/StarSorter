using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace StarSorter.Views
{
    public sealed partial class TopicsRepeater : UserControl
    {
        public static readonly DependencyProperty TopicsProperty =
            DependencyProperty.Register(
                nameof(Topics),
                typeof(IEnumerable<string>),
                typeof(TopicsRepeater),
                new PropertyMetadata(null, OnTopicsChanged));

        public IEnumerable<string> Topics
        {
            get => (IEnumerable<string>)GetValue(TopicsProperty);
            set => SetValue(TopicsProperty, value);
        }

        public TopicsRepeater()
        {
            this.InitializeComponent();
        }

        private static void OnTopicsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TopicsRepeater control)
            {
                control.repeater.ItemsSource = e.NewValue as IEnumerable<string>;
            }
        }
    }
}