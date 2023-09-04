using CoolapkLite.Helpers;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public class LanguageCodeResource : ResourceDictionary
    {
        public LanguageCodeResource() => AddResource();

        private void AddResource()
        {
            this["LanguageCodeResource"] = LanguageHelper.GetPrimaryLanguage();
        }
    }
}
