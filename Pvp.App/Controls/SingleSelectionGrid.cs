using System;
using System.Linq;
using System.Windows.Controls;

namespace Pvp.App.Controls
{
    internal class SingleSelectionGrid : DataGrid
    {
        public SingleSelectionGrid()
        {
            CanSelectMultipleItems = false;
        }
    }
}
