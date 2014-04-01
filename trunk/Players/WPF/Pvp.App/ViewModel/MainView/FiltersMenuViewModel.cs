using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class FiltersMenuViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private readonly IWindowHandleProvider _windowHandleProvider;

        public FiltersMenuViewModel(IMediaEngineFacade engine, IWindowHandleProvider windowHandleProvider) : base(engine)
        {
            _engine = engine;
            _windowHandleProvider = windowHandleProvider;
        }

        protected override void UpdateMenuCheckedStatus()
        {
            foreach (var item in _filters.AsContentItemsRecursive<SelectableStreamMenuItemData>())
            {
                item.IsChecked = _engine.IsStreamSelected(item.Data.FilterName, item.Data.StreamIndex);
            }
        }

        protected override void UpdateMenu()
        {
            _filters.Clear();

            if (_engine.GraphState == GraphState.Reset)
            {
                FiltersMenuVisible = false;
            }
            else
            {
                var last = _engine.FilterCount;
                if (last > 15)
                    last = 15;

                var displayPropPageCommand = new GenericRelayCommand<NumberedMenuItemData>(
                    data =>
                    {
                        if (data != null)
                            _engine.DisplayFilterPropPage(_windowHandleProvider.Handle, data.Number, true);
                    },
                    data =>
                    {
                        return data != null && _engine.DisplayFilterPropPage(_windowHandleProvider.Handle, data.Number, false);
                    });

                var selectStreamCommand = new GenericRelayCommand<SelectableStreamMenuItemData>(
                    data =>
                    {
                        if (data != null)
                            _engine.SelectStream(data.FilterName, data.StreamIndex);
                    });

                for (var i = 0; i < last; i++)
                {
                    var filterName = _engine.GetFilterName(i);

                    var selectableStreams = _engine.GetSelectableStreams(filterName);
                    var streams = selectableStreams as IList<SelectableStream> ?? selectableStreams.ToList();
                    if (streams.Any())
                    {
                        var parentItem = new ParentDataItem<NumberedMenuItemData>(filterName, new NumberedMenuItemData(i));
                        parentItem.SubItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_properties, new NumberedMenuItemData(i),
                            displayPropPageCommand));

                        var grouppedStreams = streams.GroupBy(s => s.MajorType);
                        foreach (var group in grouppedStreams)
                        {
                            parentItem.SubItems.Add(new SeparatorItem());
                            foreach (var stream in group)
                            {
                                var leafItem = new LeafItem<SelectableStreamMenuItemData>(stream.Name,
                                    new SelectableStreamMenuItemData(filterName, stream.Index), selectStreamCommand);
                                leafItem.IsChecked = stream.Enabled;
                                parentItem.SubItems.Add(leafItem);
                            }
                        }

                        _filters.Add(parentItem);
                    }
                    else
                    {
                        _filters.Add(new LeafItem<NumberedMenuItemData>(filterName, new NumberedMenuItemData(i), displayPropPageCommand));
                    }
                }

                FiltersMenuVisible = true;
            }
        }

        private bool _filtersMenuVisible;
        public bool FiltersMenuVisible
        {
            get { return _filtersMenuVisible; }
            set
            {
                _filtersMenuVisible = value;
                RaisePropertyChanged("FiltersMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _filters = new ObservableCollection<IHierarchicalItem>();
        public ObservableCollection<IHierarchicalItem> Filters
        {
            get { return _filters; }
        }
    }
}