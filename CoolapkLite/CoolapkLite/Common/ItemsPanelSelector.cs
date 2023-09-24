using CoolapkLite.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Common
{
    public class ItemsPanelSelector : DependencyObject
    {
        private static readonly WeakEvent<bool> IsVirtualizingChanged = new WeakEvent<bool>();

        private static bool isVirtualizing = SettingsHelper.Get<bool>(SettingsHelper.IsUseVirtualizing);
        public static bool IsVirtualizing
        {
            get => isVirtualizing;
            set
            {
                if (isVirtualizing != value)
                {
                    isVirtualizing = value;
                    IsVirtualizingChanged?.Invoke(value);
                }
            }
        }

        public ItemsPanelSelector()
        {
            IsVirtualizingChanged.Add(OnIsVirtualizingChanged);
        }

        ~ItemsPanelSelector()
        {
            IsVirtualizingChanged.Remove(OnIsVirtualizingChanged);
        }

        #region VirtualizingTemplate

        /// <summary>
        /// Identifies the <see cref="VirtualizingTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VirtualizingTemplateProperty =
            DependencyProperty.Register(
                nameof(VirtualizingTemplate),
                typeof(ItemsPanelTemplate),
                typeof(ItemsPanelSelector),
                new PropertyMetadata(null, OnTemplatePropertyChanged));

        public ItemsPanelTemplate VirtualizingTemplate
        {
            get => (ItemsPanelTemplate)GetValue(VirtualizingTemplateProperty);
            set => SetValue(VirtualizingTemplateProperty, value);
        }

        #endregion

        #region NonVirtualizingTemplate

        /// <summary>
        /// Identifies the <see cref="NonVirtualizingTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NonVirtualizingTemplateProperty =
            DependencyProperty.Register(
                nameof(NonVirtualizingTemplate),
                typeof(ItemsPanelTemplate),
                typeof(ItemsPanelSelector),
                new PropertyMetadata(null, OnTemplatePropertyChanged));

        public ItemsPanelTemplate NonVirtualizingTemplate
        {
            get => (ItemsPanelTemplate)GetValue(NonVirtualizingTemplateProperty);
            set => SetValue(NonVirtualizingTemplateProperty, value);
        }

        #endregion

        #region Template

        /// <summary>
        /// Identifies the <see cref="Template"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.Register(
                nameof(Template),
                typeof(ItemsPanelTemplate),
                typeof(ItemsPanelSelector),
                null);

        public ItemsPanelTemplate Template
        {
            get => (ItemsPanelTemplate)GetValue(TemplateProperty);
            protected set => SetValue(TemplateProperty, value);
        }

        #endregion

        private static void OnTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ItemsPanelSelector)d).UpdateTemplate();
        }

        private async void OnIsVirtualizingChanged(bool value)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            Template = value ? VirtualizingTemplate : NonVirtualizingTemplate;
        }

        private void UpdateTemplate()
        {
            OnIsVirtualizingChanged(isVirtualizing);
        }
    }
}
