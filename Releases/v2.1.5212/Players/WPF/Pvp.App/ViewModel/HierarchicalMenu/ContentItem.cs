using System.ComponentModel;
using System.Threading;

namespace Pvp.App.ViewModel.HierarchicalMenu
{
    internal abstract class ContentItem : IHierarchicalItem, INotifyPropertyChanged
    {
        private readonly string _title;

        private bool _isChecked;

        protected ContentItem(string title)
        {
            _title = title;
        }

        public abstract HierarchicalItemType HierarchicalItemType { get; }

        public string Title { get { return _title; } }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                RaiseOnPropertyChanged("IsChecked");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseOnPropertyChanged(string propertyName)
        {
            var handler = Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal abstract class ContentItem<T> : ContentItem
    {
        private readonly T _data;

        protected ContentItem(string title, T data) : base(title)
        {
            _data = data;
        }
        
        public T Data { get { return _data; } }
    }
}