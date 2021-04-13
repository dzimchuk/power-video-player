using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class DvdMenuViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;
        private bool _engineReady;

        public DvdMenuViewModel(IMediaEngineFacade engine) : base(engine)
        {
            _engine = engine;
        }

        protected override void UpdateMenuCheckedStatus()
        {
        }

        protected override void UpdateMenu()
        {
            _engineReady = true;

            _dvdMenuItems.Clear();
            DvdMenuVisible = false;

            if (_engine.SourceType == SourceType.Dvd)
            {
                var command = new GenericRelayCommand<NumberedMenuItemData>(
                    data =>
                    {
                        if (data != null)
                        {
                            _engine.ShowMenu((DVD_MENU_ID)data.Number);
                        }
                    },
                    data =>
                    {
                        var enabled = false;

                        if (data != null)
                        {
                            VALID_UOP_FLAG uops = _engine.UOPS;
                            DVD_MENU_ID id = (DVD_MENU_ID)data.Number;
                            switch (id)
                            {
                                case DVD_MENU_ID.DVD_MENU_Title:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Title) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Root:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Root) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Subpicture:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_SubPic) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Audio:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Audio) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Angle:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Angle) == 0;
                                    break;
                                case DVD_MENU_ID.DVD_MENU_Chapter:
                                    enabled = (uops & VALID_UOP_FLAG.UOP_FLAG_ShowMenu_Chapter) == 0;
                                    break;
                            }
                        }

                        return enabled;
                    });

                _dvdMenuItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_title_menu,
                    new NumberedMenuItemData((int)DVD_MENU_ID.DVD_MENU_Title), command));

                _dvdMenuItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_root_menu,
                    new NumberedMenuItemData((int)DVD_MENU_ID.DVD_MENU_Root), command));

                _dvdMenuItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_subpicture_menu,
                    new NumberedMenuItemData((int)DVD_MENU_ID.DVD_MENU_Subpicture), command));

                _dvdMenuItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_audio_menu,
                    new NumberedMenuItemData((int)DVD_MENU_ID.DVD_MENU_Audio), command));

                _dvdMenuItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_angle_menu,
                    new NumberedMenuItemData((int)DVD_MENU_ID.DVD_MENU_Angle), command));

                _dvdMenuItems.Add(new LeafItem<NumberedMenuItemData>(Resources.Resources.mi_chapter_menu,
                    new NumberedMenuItemData((int)DVD_MENU_ID.DVD_MENU_Chapter), command));

                DvdMenuVisible = true;
            }
        }

        private bool _dvdMenuVisible;
        public bool DvdMenuVisible
        {
            get { return _dvdMenuVisible; }
            set
            {
                _dvdMenuVisible = value;
                RaisePropertyChanged("DvdMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _dvdMenuItems = new ObservableCollection<IHierarchicalItem>();
        public ObservableCollection<IHierarchicalItem> DvdMenuItems
        {
            get { return _dvdMenuItems; }
        }

        private ICommand _dvdResumeCommand;
        public ICommand DvdResumeCommand
        {
            get
            {
                if (_dvdResumeCommand == null)
                {
                    _dvdResumeCommand = new RelayCommand(
                        () =>
                        {
                            _engine.ResumeDVD();
                        },
                        () =>
                        {
                            return _engineReady && _engine.IsResumeDVDEnabled();
                        });
                }

                return _dvdResumeCommand;
            }
        }
    }
}