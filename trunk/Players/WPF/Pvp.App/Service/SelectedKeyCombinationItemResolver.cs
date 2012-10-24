using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Pvp.App.ViewModel.Settings;

namespace Pvp.App.Service
{
    internal class SelectedKeyCombinationItemResolver : ISelectedKeyCombinationItemResolver
    {
        public KeyCombinationItem Resolve(EventArgs args)
        {
            var selectionEventArgs = args as SelectionChangedEventArgs;
            if (selectionEventArgs != null)
                return Resolve(selectionEventArgs);

            var mouseEventArgs = args as MouseButtonEventArgs;
            if (mouseEventArgs != null)
                return Resolve(mouseEventArgs);

            return null;
        }

        private KeyCombinationItem Resolve(SelectionChangedEventArgs args)
        {
            return args.AddedItems.OfType<KeyCombinationItem>().FirstOrDefault();
        }

        private KeyCombinationItem Resolve(MouseButtonEventArgs args)
        {
            KeyCombinationItem selectedItem = null;

            var listBox = args.Source as ListBox;
            if (listBox != null)
            {
                selectedItem = listBox.SelectedItem as KeyCombinationItem;
            }

            return selectedItem;
        }
    }
}