using System;
using System.Windows;
using System.Windows.Controls;
using Pvp.App.ViewModel.HierarchicalMenu;

namespace Pvp.App.View
{
    internal class HierarchicalDataTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _parentTemplate;
        private DataTemplate _leafTemplate;
        private DataTemplate _separatorTemplate;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var hItem = item as IHierarchicalItem;
            if (hItem != null)
            {
                switch (hItem.HierarchicalItemType)
                {
                    case HierarchicalItemType.Parent:
                        return GetParentTemplate((FrameworkElement)container);
                    case HierarchicalItemType.Leaf:
                        return GetLeafTemplate((FrameworkElement)container);
                    case HierarchicalItemType.Separator:
                        return GetSeparatorTemplate((FrameworkElement)container);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return base.SelectTemplate(item, container);
            }
        }

        private DataTemplate GetParentTemplate(FrameworkElement element)
        {
            return _parentTemplate ?? (_parentTemplate = (DataTemplate)element.FindResource("HierarchicalParentTemplate"));
        }

        private DataTemplate GetLeafTemplate(FrameworkElement element)
        {
            return _leafTemplate ?? (_leafTemplate = (DataTemplate)element.FindResource("HierarchicalLeafTemplate"));
        }

        private DataTemplate GetSeparatorTemplate(FrameworkElement element)
        {
            return _separatorTemplate ?? (_separatorTemplate = (DataTemplate)element.FindResource("HierarchicalSeparatorTemplate"));
        }
    }
}