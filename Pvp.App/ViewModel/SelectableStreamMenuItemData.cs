namespace Pvp.App.ViewModel
{
    public class SelectableStreamMenuItemData
    {
        public SelectableStreamMenuItemData(string filterName, int streamIndex)
        {
            FilterName = filterName;
            StreamIndex = streamIndex;
        }

        public string FilterName { get; private set; }
        public int StreamIndex { get; private set; }
    }
}