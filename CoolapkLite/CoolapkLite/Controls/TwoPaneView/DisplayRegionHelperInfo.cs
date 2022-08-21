using Windows.Foundation;

namespace CoolapkLite.Controls
{
    public struct DisplayRegionHelperInfo
    {
        private const int c_maxRegions = 2;

        public TwoPaneViewMode Mode { get; set; }
        public Rect[] Regions { get; set; }

        public DisplayRegionHelperInfo()
        {
            Regions = new Rect[c_maxRegions];
        }
    }
}
