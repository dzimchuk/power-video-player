using System;

namespace Pvp.App.ViewModel.HierarchicalMenu
{
    internal class ParentDataItem<T> : ParentItem
    {
        private readonly T _data;

        public ParentDataItem(string title, T data) : base(title)
        {
            _data = data;
        }

        public T Data { get { return _data; } }
    }
}