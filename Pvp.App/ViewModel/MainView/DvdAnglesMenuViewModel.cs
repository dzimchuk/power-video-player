using System.Collections.ObjectModel;
using Pvp.App.Util;
using Pvp.App.ViewModel.HierarchicalMenu;
using Pvp.Core.DirectShow;
using Pvp.Core.MediaEngine;

namespace Pvp.App.ViewModel.MainView
{
    internal class DvdAnglesMenuViewModel : MenuViewModelBase
    {
        private readonly IMediaEngineFacade _engine;

        public DvdAnglesMenuViewModel(IMediaEngineFacade engine) : base(engine)
        {
            _engine = engine;
        }

        protected override void UpdateMenuCheckedStatus()
        {
            var dvdAngle = _engine.CurrentAngle;
            foreach (var item in _dvdAngles.AsContentItems<NumberedMenuItemData>())
            {
                item.IsChecked = item.Data.Number == dvdAngle;
            }
        }

        protected override void UpdateMenu()
        {
            _dvdAngles.Clear();
            DvdAnglesMenuVisible = false;

            if (_engine.GraphState != GraphState.Reset)
            {
                int ulAngles = _engine.AnglesAvailable;

                if (ulAngles > 1)
                {
                    var command = new GenericRelayCommand<NumberedMenuItemData>(
                        data =>
                        {
                            if (data != null)
                                _engine.CurrentAngle = data.Number;
                        },
                        data =>
                        {
                            return data != null && (_engine.UOPS & VALID_UOP_FLAG.UOP_FLAG_Select_Angle) == 0;
                        });

                    for (int i = 0; i < ulAngles; i++)
                    {
                        _dvdAngles.Add(new LeafItem<NumberedMenuItemData>(string.Format(Resources.Resources.mi_angle_format, i + 1), new NumberedMenuItemData(i + 1), command));
                    }

                    DvdAnglesMenuVisible = true;
                }
            }
        }

        private bool _dvdAnglesMenuVisible;
        public bool DvdAnglesMenuVisible
        {
            get { return _dvdAnglesMenuVisible; }
            set
            {
                _dvdAnglesMenuVisible = value;
                RaisePropertyChanged("DvdAnglesMenuVisible");
            }
        }

        private readonly ObservableCollection<IHierarchicalItem> _dvdAngles = new ObservableCollection<IHierarchicalItem>();
        public ObservableCollection<IHierarchicalItem> DvdAngles
        {
            get { return _dvdAngles; }
        }
    }
}