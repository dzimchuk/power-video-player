using System.Collections.ObjectModel;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class DvdChaptersMenuViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;

        public DvdChaptersMenuViewModel(IMediaEngineFacade engine) : base(engine)
        {
            _engine = engine;
        }

        protected override void UpdateMenuCheckedStatus()
        {
            var title = _engine.CurrentTitle;
            var chapter = _engine.CurrentChapter;
            foreach (var item in _dvdChapters)
            {
                CheckTitleChaperMenuItem(item, title, chapter);
            }
        }

        private static void CheckTitleChaperMenuItem(IHierarchicalItem item, int currentTitle, int currentChapter)
        {
            var parent = item as ParentDataItem<TitleChapterMenuItemData>;
            if (parent != null)
            {
                parent.IsChecked = parent.Data.Title == currentTitle;
                foreach (var subItem in parent.SubItems)
                {
                    CheckTitleChaperMenuItem(subItem, currentTitle, currentChapter);
                }
            }
            else
            {
                var leaf = (LeafItem<TitleChapterMenuItemData>)item;
                leaf.IsChecked = leaf.Data.Title == currentTitle && leaf.Data.Chapter == currentChapter;
            }
        }

        protected override void UpdateMenu()
        {
            _dvdChapters.Clear();
            DvdChaptersMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int ulTitles = _engine.NumberOfTitles;

                if (ulTitles > 0)
                {
                    var command = new GenericRelayCommand<TitleChapterMenuItemData>(
                        data =>
                        {
                            if (data != null)
                                _engine.GoTo(data.Title, data.Chapter);
                        },
                        data =>
                        {
                            return data != null ?
                                    (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Play_Chapter) == 0
                                    :
                                    false;
                        });

                    if (ulTitles == 1)
                    {
                        int nChapters = _engine.GetNumChapters(1);
                        for (int i = 1; i <= nChapters; i++)
                        {
                            _dvdChapters.Add(new LeafItem<TitleChapterMenuItemData>(string.Format(Resources.Resources.mi_chapter_format, i),
                                new TitleChapterMenuItemData(1, i), command));
                        }
                    }
                    else
                    {
                        for (int title = 1; title <= ulTitles; title++)
                        {
                            var titleItem = new ParentDataItem<TitleChapterMenuItemData>(string.Format(Resources.Resources.mi_title_format, title),
                                new TitleChapterMenuItemData(title, 0));

                            int nChapters = _engine.GetNumChapters(title);
                            for (int i = 1; i <= nChapters; i++)
                            {
                                titleItem.SubItems.Add(new LeafItem<TitleChapterMenuItemData>(string.Format(Resources.Resources.mi_chapter_format, i),
                                    new TitleChapterMenuItemData(title, i), command));
                            }

                            _dvdChapters.Add(titleItem);
                        }
                    }

                    DvdChaptersMenuVisible = true;
                }
            }
        }

        private bool _dvdChaptersMenuVisible;
        public bool DvdChaptersMenuVisible
        {
            get { return _dvdChaptersMenuVisible; }
            set
            {
                _dvdChaptersMenuVisible = value;
                RaisePropertyChanged("DvdChaptersMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _dvdChapters = new ObservableCollection<IHierarchicalItem>();

        public ObservableCollection<IHierarchicalItem> DvdChapters
        {
            get { return _dvdChapters; }
        }
    }
}