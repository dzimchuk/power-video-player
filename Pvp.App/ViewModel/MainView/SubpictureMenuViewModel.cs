using System.Collections.ObjectModel;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class SubpictureMenuViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;

        public SubpictureMenuViewModel(IMediaEngineFacade engine) : base(engine)
        {
            _engine = engine;
        }

        protected override void UpdateMenuCheckedStatus()
        {
            var subpictureStream = _engine.CurrentSubpictureStream;
            foreach (var item in _subpictureStreams.AsContentItems<NumberedMenuItemData>())
            {
                bool toBeChecked;
                if (item.Data.Number == -1)
                {
                    toBeChecked = _engine.IsSubpictureEnabled();
                }
                else
                {
                    toBeChecked = item.Data.Number == subpictureStream;
                }

                item.IsChecked = toBeChecked;
            }
        }

        protected override void UpdateMenu()
        {
            _subpictureStreams.Clear();
            SubpictureStreamsMenuVisible = false;

            if (IsInPlayingMode)
            {
                int nStreams = _engine.NumberOfSubpictureStreams;

                if (nStreams > 0)
                {
                    var command = new GenericRelayCommand<NumberedMenuItemData>(
                        data =>
                        {
                            if (data != null)
                            {
                                if (data.Number == -1)
                                {
                                    _engine.EnableSubpicture(!_engine.IsSubpictureEnabled());
                                }
                                else
                                {
                                    _engine.CurrentSubpictureStream = data.Number;
                                }
                            }
                        },
                        data =>
                        {
                            var enabled = false;
                            if (data != null)
                            {
                                if (data.Number == -1)
                                {
                                    enabled = (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_SubPic_Stream) == 0;
                                }
                                else
                                {
                                    enabled = _engine.SourceType != SourceType.Dvd ?
                                        _engine.IsSubpictureStreamEnabled(data.Number)
                                        :
                                        (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_SubPic_Stream) == 0 &&
                                        _engine.IsSubpictureStreamEnabled(data.Number);
                                }
                            }

                            return enabled;
                        });

                    if (_engine.SourceType == SourceType.Dvd)
                    {
                        _subpictureStreams.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_display_subpictures,
                            new NumberedMenuItemData(-1), command));
                    }

                    for (int i = 0; i < nStreams; i++)
                    {
                        _subpictureStreams.Add(new LeafItem<NumberedMenuItemData>(_engine.GetSubpictureStreamName(i),
                            new NumberedMenuItemData(i), command));
                    }

                    SubpictureStreamsMenuVisible = true;
                }
            }
        }

        private bool _subpictureStreamsMenuVisible;
        public bool SubpictureStreamsMenuVisible
        {
            get { return _subpictureStreamsMenuVisible; }
            set
            {
                _subpictureStreamsMenuVisible = value;
                RaisePropertyChanged("SubpictureStreamsMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _subpictureStreams = new ObservableCollection<IHierarchicalItem>();
        public ObservableCollection<IHierarchicalItem> SubpictureStreams
        {
            get { return _subpictureStreams; }
        }
    }
}