using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (var result in groups)
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
