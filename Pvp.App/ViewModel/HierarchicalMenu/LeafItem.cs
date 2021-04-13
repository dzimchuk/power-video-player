using System.Windows.Input;

namespace Pvp.App.ViewModel.HierarchicalMenu
{
    internal class LeafItem<T> : ContentItem<T>
    {
        public LeafItem(string title, T data, ICommand command) : base(title, data)
        {
            Command = command;
        }

        public override HierarchicalItemType HierarchicalItemType
        {
            get { return HierarchicalItemType.Leaf; }
        }

        public ICommand Command { get; private set; }
    }
}