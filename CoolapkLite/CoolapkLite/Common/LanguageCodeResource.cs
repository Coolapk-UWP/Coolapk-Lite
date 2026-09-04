using CoolapkLite.Helpers;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public sealed class LanguageCodeResource : ResourceDictionary
    {
        public LanguageCodeResource() => AddResource();

        private void AddResource()
        {
            this["LanguageCodeResource"] = LanguageHelper.GetPrimaryLanguage();
        }
    }
}
