using System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public class CustomControlResource : ResourceDictionary
    {
        public CustomControlResource() => AddResource();

        private void AddResource()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane"))
            {
                AddResourceDictionary("ms-appx:///Styles/SettingsFlyout.xaml");
            }

            void AddResourceDictionary(string Source)
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(Source) });
            }
        }
    }
}
