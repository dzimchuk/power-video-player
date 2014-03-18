namespace Pvp.App.ViewModel.HierarchicalMenu
{
    internal class SeparatorItem : IHierarchicalItem
    {
        public HierarchicalItemType HierarchicalItemType { get { return HierarchicalItemType.Separator; } }
    }
}