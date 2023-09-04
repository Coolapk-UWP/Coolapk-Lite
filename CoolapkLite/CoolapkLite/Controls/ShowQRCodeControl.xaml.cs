using CoolapkLite.Helpers;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkLite.Controls
{
    public sealed partial class ShowQRCodeControl : UserControl
    {
        public static readonly DependencyProperty QRCodeTextProperty =
            DependencyProperty.Register(
                nameof(QRCodeText),
                typeof(string),
                typeof(ShowQRCodeControl),
                new PropertyMetadata("https://www.coolapk.com", OnQRCodeTextChanged));

        public string QRCodeText
        {
            get => (string)GetValue(QRCodeTextProperty);
            set => SetValue(QRCodeTextProperty, value);
        }

        private static void OnQRCodeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ShowQRCodeControl).QRCodeText = e.NewValue as string ?? "https://www.coolapk.com";
        }

        public ShowQRCodeControl() => InitializeComponent();

        private void ShowUIButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();

            Uri shareLinkString = QRCodeText.TryGetUri();
            if (shareLinkString != null)
            {
                dataPackage.SetWebLink(shareLinkString);
                dataPackage.Properties.Title = "动态分享";
                dataPackage.Properties.Description = QRCodeText;
            }
            else
            {
                dataPackage.SetText(QRCodeText);
                dataPackage.Properties.Title = "内容分享";
                dataPackage.Properties.Description = "内含文本";
            }

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (obj, args) => { args.Request.Data = dataPackage; };
            DataTransferManager.ShowShareUI();
        }
    }
}