using System.Collections.Generic;
using System.Linq;

namespace Pvp.App.ViewModel.HierarchicalMenu
{
    internal static class Extensions
    {
        public static IEnumerable<ContentItem<T>> AsContentItems<T>(this IEnumerable<IHierarchicalItem> items)
        {
            return items.OfType<ContentItem<T>>();
        }

        public static IEnumerable<ContentItem<T>> AsContentItemsRecursive<T>(this IEnumerable<IHierarchicalItem> items)
        {
            var list = new List<ContentItem<T>>();

            var hierarchicalItems = items as IList<IHierarchicalItem> ?? items.ToList();
            foreach (var item in hierarchicalItems)
            {
                var contentItem = item as ContentItem<T>;
                if (contentItem != null)
                {
                    list.Add(contentItem);
                }

                if (item.HierarchicalItemType == HierarchicalItemType.Parent)
                {
                    list.AddRange(((ParentItem)item).SubItems.AsContentItemsRecursive<T>());
                }
            }

            return list;
        }
    }
}