using System;
using System.Linq;

namespace Pvp.App.ViewModel
{
    internal class TitleChapterMenuItemData
    {
        public TitleChapterMenuItemData(int title, int chapter)
        {
            Title = title;
            Chapter = chapter;
        }

        public int Title { get; private set; }
        public int Chapter { get; private set; }
    }
}
