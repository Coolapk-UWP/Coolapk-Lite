using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace CoolapkLite.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string TileUrl = "TileUrl";
        public const string IsFirstRun = "IsFirstRun";
        public const string IsDarkMode = "IsDarkMode";
        public const string IsNoPicsMode = "IsNoPicsMode";
        public const string ShowOtherException = "ShowOtherException";
        public const string IsDisplayOriginPicture = "IsDisplayOriginPicture";
        public const string CheckUpdateWhenLuanching = "CheckUpdateWhenLuanching";
        public const string IsBackgroundColorFollowSystem = "IsBackgroundColorFollowSystem";

        public static Type Get<Type>(string key) => (Type)LocalSettings.Values[key];

        public static void Set(string key, object value) => LocalSettings.Values[key] = value;

        public static void SetDefaultSettings()
        {
            if (!LocalSettings.Values.ContainsKey(TileUrl))
            {
                LocalSettings.Values.Add(TileUrl, "https://www.wpxap.com/");
            }
            if (!LocalSettings.Values.ContainsKey(IsFirstRun))
            {
                LocalSettings.Values.Add(IsFirstRun, true);
            }
            if (!LocalSettings.Values.ContainsKey(IsDarkMode))
            {
                LocalSettings.Values.Add(IsDarkMode, false);
            }
            if (!LocalSettings.Values.ContainsKey(IsNoPicsMode))
            {
                LocalSettings.Values.Add(IsNoPicsMode, false);
            }
            if (!LocalSettings.Values.ContainsKey(ShowOtherException))
            {
                LocalSettings.Values.Add(ShowOtherException, true);
            }
            if (!LocalSettings.Values.ContainsKey(IsDisplayOriginPicture))
            {
                LocalSettings.Values.Add(IsDisplayOriginPicture, false);
            }
            if (!LocalSettings.Values.ContainsKey(CheckUpdateWhenLuanching))
            {
                LocalSettings.Values.Add(CheckUpdateWhenLuanching, true);
            }
            if (!LocalSettings.Values.ContainsKey(IsBackgroundColorFollowSystem))
            {
                LocalSettings.Values.Add(IsBackgroundColorFollowSystem, true);
            }
        }
    }

    public enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }

    internal static partial class SettingsHelper
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        public static readonly MetroLog.ILogManager LogManager = MetroLog.LogManagerFactory.CreateLogManager();
        public static readonly UISettings UISettings = new UISettings();
        public static ElementTheme Theme => Get<bool>("IsBackgroundColorFollowSystem") ? ElementTheme.Default : (Get<bool>("IsDarkMode") ? ElementTheme.Dark : ElementTheme.Light);
        public static Core.WeakEvent<UISettingChangedType> UISettingChanged { get; } = new Core.WeakEvent<UISettingChangedType>();

        static SettingsHelper()
        {
            SetDefaultSettings();
            SetBackgroundTheme(UISettings, null);
            UISettings.ColorValuesChanged += SetBackgroundTheme;
            UIHelper.CheckTheme();
        }

        private static void SetBackgroundTheme(UISettings o, object _)
        {
            if (Get<bool>(IsBackgroundColorFollowSystem))
            {
                bool value = o.GetColorValue(UIColorType.Background) == Windows.UI.Colors.Black;
                Set(IsDarkMode, value);
                UISettingChanged.Invoke(value ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
            }
        }
    }
}
