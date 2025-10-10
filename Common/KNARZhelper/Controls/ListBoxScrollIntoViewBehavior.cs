using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Controls;

namespace KNARZhelper.Controls
{
    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject.SelectedItem != null)
            {
                var listBox = sender as ListBox;

                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            listBox.UpdateLayout();

                            if (listBox.SelectedItem != null)
                            {
                                listBox.ScrollIntoView(listBox.SelectedItem);
                            }
                        }));
                }
            }
        }
    }
}
