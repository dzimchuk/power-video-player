using System.Collections.ObjectModel;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class AudioMenuViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;

        public AudioMenuViewModel(IMediaEngineFacade engine) : base(engine)
        {
            _engine = engine;
        }

        protected override void UpdateMenuCheckedStatus()
        {
            var audioStream = _engine.CurrentAudioStream;
            foreach (var item in _audioStreams.AsContentItems<NumberedMenuItemData>())
            {
                item.IsChecked = item.Data.Number == audioStream;
            }
        }

        protected override void UpdateMenu()
        {
            _audioStreams.Clear();
            AudioStreamsMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int nStreams = _engine.AudioStreams;

                if (nStreams > 0)
                {
                    var command = new GenericRelayCommand<NumberedMenuItemData>(
                        data =>
                        {
                            if (data != null)
                                _engine.CurrentAudioStream = data.Number;
                        },
                        data =>
                        {
                            return data != null &&
                                   (_engine.SourceType != SourceType.Dvd ||
                                    ((_engine.IsAudioStreamEnabled(data.Number)) && (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_Audio_Stream) == 0));
                        });

                    if (nStreams > 8)
                        nStreams = 8;

                    for (int i = 0; i < nStreams; i++)
                    {
                        _audioStreams.Add(new LeafItem<NumberedMenuItemData>(_engine.GetAudioStreamName(i), new NumberedMenuItemData(i), command));
                    }

                    AudioStreamsMenuVisible = true;
                }
            }
        }

        private bool _audioStreamsMenuVisible;
        public bool AudioStreamsMenuVisible
        {
            get { return _audioStreamsMenuVisible; }
            set
            {
                _audioStreamsMenuVisible = value;
                RaisePropertyChanged("AudioStreamsMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _audioStreams = new ObservableCollection<IHierarchicalItem>();
        public ObservableCollection<IHierarchicalItem> AudioStreams
        {
            get { return _audioStreams; }
        }
    }
}