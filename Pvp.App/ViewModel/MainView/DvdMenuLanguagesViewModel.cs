using System.Collections.ObjectModel;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class DvdMenuLanguagesViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;

        public DvdMenuLanguagesViewModel(IMediaEngineFacade engine) : base(engine)
        {
            _engine = engine;
        }

        protected override void UpdateMenuCheckedStatus()
        {
        }

        protected override void UpdateMenu()
        {
            _dvdMenuLanguages.Clear();
            DvdMenuLanguagesMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                var nLang = _engine.MenuLangCount;

                if (nLang > 0)
                {
                    if (nLang > 10)
                        nLang = 10;

                    var command = new GenericRelayCommand<NumberedMenuItemData>(
                        data =>
                        {
                            if (data != null)
                                _engine.SetMenuLang(data.Number);
                        },
                        data =>
                        {
                            return data != null && _engine.MenuLangCount > 1;
                        });

                    for (int i = 0; i < nLang; i++)
                    {
                        _dvdMenuLanguages.Add(new LeafItem<NumberedMenuItemData>(_engine.GetMenuLangName(i), new NumberedMenuItemData(i), command));
                    }

                    DvdMenuLanguagesMenuVisible = true;
                }
            }
        }

        private bool _dvdMenuLanguagesMenuVisible;
        public bool DvdMenuLanguagesMenuVisible
        {
            get { return _dvdMenuLanguagesMenuVisible; }
            set
            {
                _dvdMenuLanguagesMenuVisible = value;
                RaisePropertyChanged("DvdMenuLanguagesMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _dvdMenuLanguages = new ObservableCollection<IHierarchicalItem>();
        public ObservableCollection<IHierarchicalItem> DvdMenuLanguages
        {
            get { return _dvdMenuLanguages; }
        }
    }
}