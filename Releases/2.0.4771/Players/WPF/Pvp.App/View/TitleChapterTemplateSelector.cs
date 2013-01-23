using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pvp.App.ViewModel;

namespace Pvp.App.View
{
    internal class TitleChapterTemplateSelector : DataTemplateSelector
    {
        private DataTemplate _parentTemplate = null;
        private DataTemplate _leafTemplate = null;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var tc = item as TitleChapterCommand;
            if (tc != null)
            {
                var element = (FrameworkElement)container;
                if (_parentTemplate == null)
                    _parentTemplate = (DataTemplate)element.FindResource("BoundTitleCommandTemplate");

                if (_leafTemplate == null)
                    _leafTemplate = (DataTemplate)element.FindResource("BoundChapterCommandTemplate");


                return tc.SubItems.Any() ? _parentTemplate : _leafTemplate;
            }
            else
                return base.SelectTemplate(item, container);
        }
    }
}
