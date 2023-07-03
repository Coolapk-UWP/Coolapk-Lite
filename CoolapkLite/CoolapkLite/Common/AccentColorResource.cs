using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace CoolapkLite.Common
{
    public class AccentColorResource : ResourceDictionary
    {
        private const string AccentDark1Key = "SystemAccentColorDark1";
        private const string AccentDark2Key = "SystemAccentColorDark2";
        private const string AccentDark3Key = "SystemAccentColorDark3";
        private const string AccentLight1Key = "SystemAccentColorLight1";
        private const string AccentLight2Key = "SystemAccentColorLight2";
        private const string AccentLight3Key = "SystemAccentColorLight3";

        private readonly UISettings _uiSettings = new UISettings();

        public AccentColorResource()
        {
            if (!Application.Current.Resources.ContainsKey(AccentDark1Key))
            {
                UpdateSystemAccentColors(_uiSettings);
                _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            }
        }

        ~AccentColorResource()
        {
            _uiSettings.ColorValuesChanged -= OnColorValuesChanged;
        }

        public void UpdateSystemAccentColors(UISettings sender)
        {
            this[AccentDark1Key] = sender.GetColorValue(UIColorType.AccentDark1);
            this[AccentDark2Key] = sender.GetColorValue(UIColorType.AccentDark2);
            this[AccentDark3Key] = sender.GetColorValue(UIColorType.AccentDark3);
            this[AccentLight1Key] = sender.GetColorValue(UIColorType.AccentLight1);
            this[AccentLight2Key] = sender.GetColorValue(UIColorType.AccentLight2);
            this[AccentLight3Key] = sender.GetColorValue(UIColorType.AccentLight3);
        }

        private async void OnColorValuesChanged(UISettings sender, object args)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            UpdateSystemAccentColors(sender);
        }
    }
}
