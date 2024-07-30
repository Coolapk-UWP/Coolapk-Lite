using CoolapkLite.Helpers;
using System;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    [Flags]
    public enum BackdropType
    {
        Default = -0x1,
        Mica = 0x7,
        Acrylic = 0x3,
        FakeMica = 0x5,
        Blur = 0x1,
        Solid = 0x0
    }

    public class CustomBrushResource : ResourceDictionary
    {
        public CustomBrushResource() => AddResource();

        private void AddResource()
        {
            switch (SettingsHelper.Get<BackdropType>(SettingsHelper.SelectedBackdrop))
            {
                case BackdropType.Mica when ApiInfoHelper.IsTryCreateBlurredWallpaperBackdropBrushSupported:
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS3.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.21H2.xaml");
                    break;
                case BackdropType.Acrylic when ApiInfoHelper.IsAcrylicBrushSupported:
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS3.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS3.xaml");
                    break;
                case BackdropType.FakeMica when ApiInfoHelper.IsAcrylicBrushSupported:
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS3.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.Fake.xaml");
                    break;
                case BackdropType.Blur when ApiInfoHelper.IsXamlCompositionBrushBaseSupported:
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS2.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS2.xaml");
                    break;
                case BackdropType.Mica:
                case BackdropType.Acrylic:
                case BackdropType.Blur:
                case BackdropType.Solid:
                    AddResourceDictionary("ms-appx:///Styles/Brushes/Acrylic/AcrylicBrush.RS1.xaml");
                    AddResourceDictionary("ms-appx:///Styles/Brushes/ThemeResources.RS1.xaml");
                    break;
                case BackdropType.Default:
                default:
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
                    break;
            }

            void AddResourceDictionary(string Source)
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(Source) });
            }
        }
    }
}
