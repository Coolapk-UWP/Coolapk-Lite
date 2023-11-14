using CoolapkLite.Controls;
using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Models.Message;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnicodeStyle;
using UnicodeStyle.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private AppBarToggleButton BoldButton;
        private AppBarToggleButton ItalicButton;
        private AppBarToggleButton UnderLineButton;
        private AppBarToggleButton StrikethroughButton;

        private readonly string[] NormalEmojis = EmojiHelper.Normal;
        private readonly string[] CoolCoinsEmojis = EmojiHelper.CoolCoins;
        private readonly string[] FunnyEmojis = EmojiHelper.Funny;
        private readonly string[] DogeEmojis = EmojiHelper.Doge;
        private readonly string[] TraditionEmojis = EmojiHelper.Tradition;
        private readonly string[] ClassicEmojis = EmojiHelper.Classic;

        #region Provider

        /// <summary>
        /// Identifies the <see cref="Provider"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ProviderProperty =
            DependencyProperty.Register(
                nameof(Provider),
                typeof(ChatViewModel),
                typeof(ChatPage),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Page"/>.
        /// </summary>
        public ChatViewModel Provider
        {
            get => (ChatViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        public ChatPage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ChatViewModel ViewModel
                && Provider?.IsEqual(ViewModel) != true)
            {
                Provider = ViewModel;
            }

            Provider.LoadMoreStarted += OnLoadMoreStarted;
            Provider.LoadMoreCompleted += OnLoadMoreCompleted;

            if (!Provider.Any)
            {
                _ = Refresh(true);
            }

            Clipboard.ContentChanged += Clipboard_ContentChanged;
            Clipboard_ContentChanged(null, null);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Provider.LoadMoreStarted -= OnLoadMoreStarted;
            Provider.LoadMoreCompleted -= OnLoadMoreCompleted;
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "SendButton":
                    SendDataContent();
                    break;
                case "ImageButton":
                    SendImageContent(await Provider.PickImageAsync());
                    break;
                default:
                    break;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case nameof(PastePic):
                    SendImageContent(await Provider.DropFileAsync(Clipboard.GetContent()));
                    break;
                default:
                    break;
            }
        }

        private async void SendDataContent()
        {
            _ = this.ShowProgressBarAsync();
            try
            {
                InputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
                contentText = contentText.Replace("\r", Environment.NewLine);
                if (string.IsNullOrWhiteSpace(contentText)) { return; }
                string id = Provider.ID.Substring(Provider.ID.IndexOf('_') + 1);
                if (string.IsNullOrWhiteSpace(id)) { return; }
                post:
                try
                {
                    using (MultipartFormDataContent content = new MultipartFormDataContent())
                    {
                        using (StringContent message = new StringContent(contentText))
                        {
                            content.Add(message, "message");
                            (bool isSucceed, JToken results) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.SendMessage, id), content);
                            if (isSucceed)
                            {
                                MessageModel messageModel = new MessageModel(results.First as JObject);
                                Provider.Add(messageModel);
                                InputBox.Document.SetText(TextSetOptions.None, string.Empty);
                            }
                        }
                    }
                }
                catch (CoolapkMessageException cex)
                {
                    _ = this.ShowMessageAsync(cex.Message);
                    if (cex.IsRequestCaptcha)
                    {
                        if (cex.IsRequestCaptcha)
                        {
                            captcha:
                            CaptchaDialog captchaDialog = new CaptchaDialog();
                            if (await captchaDialog.ShowAsync() == ContentDialogResult.Primary)
                            {
                                if (await captchaDialog.RequestValidateAsync())
                                {
                                    goto post;
                                }
                                else
                                {
                                    goto captcha;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                _ = this.HideProgressBarAsync();
            }
        }

        private async void SendImageContent(WriteableBitmap writeableBitmap)
        {
            if (writeableBitmap == null) { return; }
            _ = this.ShowProgressBarAsync();
            try
            {
                string id = Provider.ID.Substring(Provider.ID.IndexOf('_') + 1);
                if (string.IsNullOrWhiteSpace(id)) { return; }
                string pic = await Provider.UploadPicAsync(writeableBitmap);
                if (string.IsNullOrWhiteSpace(pic))
                {
                    _ = this.ShowMessageAsync("图片上传失败");
                    return;
                }
                int index = pic.IndexOf("/message/");
                if (index != -1) { pic = pic.Substring(index); }
                try
                {
                    using (MultipartFormDataContent content = new MultipartFormDataContent())
                    {
                        using (StringContent message_pic = new StringContent(pic))
                        {
                            content.Add(message_pic, "message_pic");
                            (bool isSucceed, JToken results) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.SendMessage, id), content);
                            if (isSucceed)
                            {
                                MessageModel messageModel = new MessageModel(results.First as JObject);
                                Provider.Add(messageModel);
                            }
                        }
                    }
                }
                catch (CoolapkMessageException cex)
                {
                    _ = this.ShowMessageAsync(cex.Message);
                    if (cex.IsRequestCaptcha)
                    {
                        //CaptchaDialog dialog = new CaptchaDialog();
                        //_ = await dialog.ShowAsync();
                    }
                }
            }
            finally
            {
                _ = this.HideProgressBarAsync();
            }
        }

        private void InputBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInfoHelper.IsCommandBarFlyoutSupported)
            {
                InputBox.ContextFlyout.Opening += Menu_Opening;
                InputBox.ContextFlyout.Closing += Menu_Closing;
            }

            if (ApiInfoHelper.IsSelectionFlyoutSupported)
            {
                InputBox.SelectionFlyout.Opening += Menu_Opening;
                InputBox.SelectionFlyout.Closing += Menu_Closing;
            }
        }

        private void Menu_Opening(object sender, object e)
        {
            if (sender is CommandBarFlyout Flyout && Flyout.Target == InputBox)
            {
                Flyout.PrimaryCommands.Clear();

                BoldButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE8DD",
                        FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]
                    }
                };
                BoldButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(BoldButton);

                ItalicButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE8DB",
                        FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]
                    }
                };
                ItalicButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(ItalicButton);

                Flyout.PrimaryCommands.Add(new AppBarSeparator());

                UnderLineButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\uE8DC",
                        FontFamily = (FontFamily)Application.Current.Resources["SymbolThemeFontFamily"]
                    }
                };
                UnderLineButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(UnderLineButton);

                StrikethroughButton = new AppBarToggleButton
                {
                    Icon = new FontIcon
                    {
                        Glyph = "\u0335a\u0335b\u0335c\u0335",
                        FontFamily = (FontFamily)Application.Current.Resources["ContentControlThemeFontFamily"],
                        Margin = new Thickness(0, -5, 0, 0)
                    }
                };
                StrikethroughButton.Click += StyleButton_Click;
                Flyout.PrimaryCommands.Add(StrikethroughButton);
            }
        }

        private void Menu_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            if (BoldButton != null)
            {
                BoldButton.Click -= StyleButton_Click;
                BoldButton = null;
            }

            if (ItalicButton != null)
            {
                ItalicButton.Click -= StyleButton_Click;
                ItalicButton = null;
            }

            if (UnderLineButton != null)
            {
                UnderLineButton.Click -= StyleButton_Click;
                UnderLineButton = null;
            }

            if (StrikethroughButton != null)
            {
                StrikethroughButton.Click -= StyleButton_Click;
                StrikethroughButton = null;
            }
        }

        private void StyleButton_Click(object sender, RoutedEventArgs e)
        {
            if (BoldButton != null && ItalicButton != null && UnderLineButton != null && StrikethroughButton != null)
            {
                InputBox.Document.Selection.GetText(TextGetOptions.UseObjectText, out string SelectionText);

                SelectionText = UnderLineButton.IsChecked == true
                    ? UnicodeLine.AddLine(SelectionText, true, UnicodeLines.Underline)
                    : UnicodeLine.RemoveLine(SelectionText, UnicodeLines.Underline);

                SelectionText = StrikethroughButton.IsChecked == true
                    ? UnicodeLine.AddLine(SelectionText, true, UnicodeLines.LongStrokeOverlay)
                    : UnicodeLine.RemoveLine(SelectionText, UnicodeLines.LongStrokeOverlay);

                UnicodeStyles Style = BoldButton.IsChecked == true
                    ? ItalicButton.IsChecked == true
                        ? UnicodeStyles.BoldItalic
                        : UnicodeStyles.Bold
                    : ItalicButton.IsChecked == true
                        ? UnicodeStyles.Italic
                        : UnicodeStyles.Regular;

                using (UnicodeStyle.UnicodeStyle UnicodeStyle = new UnicodeStyle.UnicodeStyle())
                {
                    SelectionText = UnicodeStyle.StyleConvert(SelectionText, Style);
                }

                InputBox.Document.Selection.Text = SelectionText;
            }
        }

        private async void InsertEmoji(string data)
        {
            using (IRandomAccessStreamWithContentType randomAccessStream = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/Emoji/{data}.png"))).OpenReadAsync())
            {
                string name = data[0] == '(' ? $"#{data}" : data;
                InputBox.Document.Selection.InsertImage(
                    24, 24, 0,
                    VerticalCharacterAlignment.Top,
                    name,
                    randomAccessStream);
                _ = InputBox.Document.Selection.MoveRight(TextRangeUnit.Character, 1, false);
            }
        }

        private void InputBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is RichEditBox richEditBox)) { return; }
            richEditBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            Provider.IsTextEmpty = string.IsNullOrEmpty(contentText);
        }

        #region 搜索框

        private void EmojiAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !string.IsNullOrWhiteSpace(sender.Text))
            {
                sender.ItemsSource = EmojiHelper.Emojis.Where(x => (x[0] == '(' ? $"#{x}" : x).Contains(sender.Text));
            }
        }

        private void EmojiAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            InsertEmoji(args.SelectedItem.ToString());
            sender.Text = string.Empty;
        }

        #endregion

        private void OnLoadMoreStarted() => _ = this.ShowProgressBarAsync();

        private void OnLoadMoreCompleted() => _ = this.HideProgressBarAsync();

        public Task Refresh(bool reset = false) => Provider.Refresh(reset);

        private void TitleBar_RefreshEvent(TitleBar sender, object e) => _ = Refresh(true);

        private void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e) => InsertEmoji(e.ClickedItem.ToString());

        private void Clipboard_ContentChanged(object sender, object e) => _ = Provider.CheckDataAsync(Clipboard.GetContent()).ContinueWith(x => PastePic.SetValueAsync(IsEnabledProperty, x.Result)).Unwrap();

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer ScrollViewer = ListView.FindDescendant<ScrollViewer>();
            ItemsStackPanel ItemsStackPanel = ListView.FindDescendant<ItemsStackPanel>();

            if (ItemsStackPanel != null && ApiInfoHelper.IsKeepLastItemInViewSupported)
            {
                ItemsStackPanel.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
            }

            if (ScrollViewer != null)
            {
                ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!(sender is ScrollViewer scrollViewer)) { return; }
            if (scrollViewer.VerticalOffset == 0)
            {
                _ = Refresh();
            }
        }
    }
}
