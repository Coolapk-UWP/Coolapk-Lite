using Windows.Foundation;

namespace CoolapkLite.Controls
{
    public class DisplayRegionHelperInfo
    {
        private const int c_maxRegions = 2;

        public TwoPaneViewMode Mode { get; set; } = TwoPaneViewMode.SinglePane;
        public Rect[] Regions { get; set; } = new Rect[c_maxRegions];
    }
}
