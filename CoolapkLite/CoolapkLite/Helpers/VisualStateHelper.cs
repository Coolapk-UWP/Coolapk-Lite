using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers
{
    public static class VisualStateHelper
    {
        public static VisualStateGroup FindVisualStateGroupByName(this FrameworkElement element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            System.Collections.Generic.IList<VisualStateGroup> groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup result in groups)
            {
                if (result != null && name.Equals(result?.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }
            }

            return null;
        }
    }
}
