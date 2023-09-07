using CoolapkLite.Helpers;
using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public class CustomControlResource : ResourceDictionary
    {
        public CustomControlResource() => AddResource();

        private void AddResource()
        {
            if (ApiInfoHelper.IsMediaPlayerElementSupported)
            {
                AddResourceDictionary("ms-appx:///Controls/MediaPlayerElementEx/MediaPlayerElementEx.ThemeResources.RS1.xaml");
            }
            else
            {
                AddResourceDictionary("ms-appx:///Controls/MediaPlayerElementEx/MediaPlayerElementEx.ThemeResources.TH1.xaml");
            }

            void AddResourceDictionary(string Source)
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(Source) });
            }
        }
    }
}
