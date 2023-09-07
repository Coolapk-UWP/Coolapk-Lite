using CoolapkLite.Helpers;
using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public class CustomBrushResource : ResourceDictionary
    {
        public CustomBrushResource() => AddResource();

        private void AddResource()
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsUseBlurBrush))
            {
                if (ApiInfoHelper.IsTryCreateBlurredWallpaperBackdropBrushSupported)
                {
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS3.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.21H2.xaml");
                }
                else if (ApiInfoHelper.IsAcrylicBrushSupported)
                {
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS3.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS3.xaml");
                }
                else if (ApiInfoHelper.IsXamlCompositionBrushBaseSupported)
                {
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS2.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS2.xaml");
                }
                else
                {
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS1.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS1.xaml");
                }
            }
            else
            {
                AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS1.xaml");
                AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS1.xaml");
            }

            void AddResourceDictionary(string Source)
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(Source) });
            }
        }
    }
}
