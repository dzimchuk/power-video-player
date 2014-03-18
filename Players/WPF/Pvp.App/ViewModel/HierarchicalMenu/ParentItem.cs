using System.Collections.Generic;
using System.Linq;

namespace Pvp.App.ViewModel.HierarchicalMenu
{
    internal class ParentItem : ContentItem
    {
        public ParentItem(string title) : base(title)
        {
            SubItems = new List<IHierarchicalItem>();
        }

        public override HierarchicalItemType HierarchicalItemType
        {
            get { return HierarchicalItemType.Parent; }
        }

        public ICollection<IHierarchicalItem> SubItems { get; private set; }
    }
}