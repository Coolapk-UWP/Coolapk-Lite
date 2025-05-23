﻿using CoolapkLite.Controls.Dialogs;
using CoolapkLite.Helpers;
using CoolapkLite.Helpers.Converters;
using CoolapkLite.Models;
using CoolapkLite.Models.Exceptions;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.FeedPages;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnicodeStyle;
using UnicodeStyle.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class CreateFeedControl : Picker
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
                typeof(CreateFeedViewModel),
                typeof(CreateFeedControl),
                null);

        /// <summary>
        /// Get the <see cref="ViewModels.IViewModel"/> of current <see cref="Picker"/>.
        /// </summary>
        public CreateFeedViewModel Provider
        {
            get => (CreateFeedViewModel)GetValue(ProviderProperty);
            private set => SetValue(ProviderProperty, value);
        }

        #endregion

        #region FeedType

        public static readonly DependencyProperty FeedTypeProperty =
            DependencyProperty.Register(
                nameof(FeedType),
                typeof(CreateFeedType),
                typeof(CreateFeedControl),
                new PropertyMetadata(CreateFeedType.Feed, OnFeedPropertyChanged));

        public CreateFeedType FeedType
        {
            get => (CreateFeedType)GetValue(FeedTypeProperty);
            set => SetValue(FeedTypeProperty, value);
        }

        #endregion

        #region ReplyID

        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register(
                nameof(ReplyID),
                typeof(int),
                typeof(CreateFeedControl),
                new PropertyMetadata(0, OnFeedPropertyChanged));

        public int ReplyID
        {
            get => (int)GetValue(ReplyIDProperty);
            set => SetValue(ReplyIDProperty, value);
        }

        #endregion

        private static void OnFeedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CreateFeedControl)d).UpdateTitle();
        }

        public CreateFeedControl()
        {
            InitializeComponent();
            Provider = new CreateFeedViewModel(Dispatcher);
            if (ApiInfoHelper.IsUniversalApiContract14Present)
            { CommandBar.DefaultLabelPosition = CommandBarDefaultLabelPosition.Collapsed; }
        }

        private void Picker_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            Clipboard.ContentChanged += Clipboard_ContentChanged;
            Clipboard_ContentChanged(null, null);
            UpdateTitle();
        }

        private void Picker_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
        }

        private void UpdateTitle()
        {
            if (Provider != null)
            {
                switch (FeedType)
                {
                    case CreateFeedType.Feed:
                        Provider.Title = "写动态";
                        break;
                    case CreateFeedType.Reply:
                        Provider.Title = $"回复动态 ID {ReplyID}";
                        break;
                    case CreateFeedType.ReplyReply:
                        Provider.Title = $"回复评论 ID {ReplyID}";
                        break;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "CloseButton":
                    Hide();
                    break;
                default:
                    break;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Send":
                    CreateDataContent();
                    break;
                case "AddPic":
                    Provider.PickImage();
                    break;
                default:
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) { return; }
            switch (element.Name)
            {
                case nameof(PastePic):
                    _ = Provider.DropFileAsync(Clipboard.GetContent());
                    break;
                case "DeletePic":
                    Provider.Pictures.Remove(element.Tag as WriteableBitmap);
                    break;
                default:
                    break;
            }
        }

        private async void CreateDataContent()
        {
            _ = this.ShowProgressBarAsync();
            try
            {
                InputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
                contentText = contentText.Replace("\r", Environment.NewLine);
                if (string.IsNullOrWhiteSpace(contentText)) { return; }
                if (FeedType == CreateFeedType.Feed) { await CreateFeedContentAsync(contentText); }
                else { await CreateReplyContentAsync(contentText); }
            }
            finally
            {
                _ = this.HideProgressBarAsync();
            }
        }

        private async Task CreateFeedContentAsync(string contentText)
        {
            IList<string> pics = Array.Empty<string>();
            if (Provider.Pictures.Count > 0)
            {
                pics = await Provider.UploadPicAsync();
                if (pics.Count != Provider.Pictures.Count)
                {
                    _ = this.ShowMessageAsync("图片上传失败");
                    return;
                }
            }
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent message = new StringContent(contentText))
                using (StringContent type = new StringContent("feed"))
                using (StringContent is_html_article = new StringContent("0"))
                using (StringContent pic = new StringContent(string.Join(",", pics)))
                {
                    content.Add(message, "message");
                    content.Add(type, "type");
                    content.Add(is_html_article, "is_html_article");
                    content.Add(pic, "pic");
                    await SendContentAsync(content);
                }
            }
        }

        private async Task CreateReplyContentAsync(string contentText)
        {
            IList<string> pics = Array.Empty<string>();
            if (Provider.Pictures.Count > 0)
            {
                pics = await Provider.UploadPicAsync();
                if (pics.Count != Provider.Pictures.Count)
                {
                    _ = this.ShowMessageAsync("图片上传失败");
                    return;
                }
            }
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent message = new StringContent(contentText))
                using (StringContent pic = new StringContent(string.Join(",", pics)))
                {
                    content.Add(message, "message");
                    content.Add(pic, "pic");
                    await SendContentAsync(content);
                }
            }
        }

        private async Task SendContentAsync(HttpContent content)
        {
            string type = null;
            switch (FeedType)
            {
                case CreateFeedType.Feed:
                    type = UriType.CreateFeed;
                    break;
                case CreateFeedType.Reply:
                    type = UriType.CreateFeedReply;
                    break;
                case CreateFeedType.ReplyReply:
                    type = UriType.CreateReplyReply;
                    break;
            }

        post:
            try
            {
                (bool isSucceed, JToken token) = await (type == UriType.CreateFeed
                    ? RequestHelper.PostDataAsync(UriHelper.GetUri(type), content)
                    : RequestHelper.PostDataAsync(UriHelper.GetUri(type, ReplyID), content));
                if (isSucceed)
                {
                    SendSuccessful();
                }
                else if (token != null)
                {
                    throw new CoolapkMessageException(token as JObject);
                }
            }
            catch (CoolapkMessageException cex)
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

        private void SendSuccessful()
        {
            _ = this.ShowMessageAsync(ResourceLoader.GetForViewIndependentUse("CreateFeedControl").GetString("SendSuccessed"));
            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
            Provider.Pictures.Clear();
            Hide();
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

        private void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            InsertEmoji(e.ClickedItem.ToString());
            EmojiFlyout.Hide();
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

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            LinkFlyout.Hide();
            if (e.ClickedItem is UserModel UserModel)
            {
                InputBox.Document.Selection.TypeText($"@{UserModel.UserName} ");
            }
            else if (e.ClickedItem is TopicModel TopicModel)
            {
                InputBox.Document.Selection.TypeText($" #{TopicModel.Title}# ");
            }
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            DragOperationDeferral deferral = e.GetDeferral();
            e.AcceptedOperation = await Provider.CheckDataAsync(e.DataView) ? DataPackageOperation.Copy : DataPackageOperation.None;
            e.Handled = true;
            deferral.Complete();
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            DragOperationDeferral deferral = e.GetDeferral();
            await Provider.DropFileAsync(e.DataView);
            e.Handled = true;
            deferral.Complete();
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            if (args.Handled) { return; }
            if (args.EventType == CoreAcceleratorKeyEventType.KeyDown || args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.V when PastePic.IsEnabled && CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down):
                        _ = Provider.DropFileAsync(Clipboard.GetContent());
                        args.Handled = true;
                        break;
                    case VirtualKey.Escape:
                        Hide();
                        args.Handled = true;
                        break;
                }
            }
        }

        private void Clipboard_ContentChanged(object sender, object e) => _ = Provider.CheckDataAsync(Clipboard.GetContent()).ContinueWith(x => PastePic.SetValueAsync(IsEnabledProperty, x.Result)).Unwrap();

        #region 搜索框

        private void UserAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Provider.CreateUserItemSource.Keyword = args.QueryText;
            _ = Provider.CreateUserItemSource.Refresh(true);
        }

        private void TopicAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Provider.CreateTopicItemSource.Keyword = args.QueryText;
            _ = Provider.CreateTopicItemSource.Refresh(true);
        }

        private void EmojiAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !string.IsNullOrWhiteSpace(sender.Text))
            {
                sender.ItemsSource = EmojiHelper.Emojis.Where(x => (x[0] == '(' ? $"#{x}" : x).Contains(sender.Text));
            }
        }

        private void EmojiAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            EmojiFlyout.Hide();
            InsertEmoji(args.SelectedItem.ToString());
            sender.Text = string.Empty;
        }

        #endregion
    }

    public class StringToEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object result = null;
            string item = value.ToString();
            if (EmojiHelper.Emojis.Contains(item))
            {
                result = new BitmapImage(new Uri($"ms-appx:///Assets/Emoji/{item}.png"));
            }
            result = result ?? ImageCacheHelper.GetNoPic(UIHelper.TryGetForCurrentCoreDispatcher());
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ConverterTools.Convert(value, targetType);
    }

    public class EmojiNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string data = value.ToString();
            string result = data[0] == '(' ? $"#{data}" : data;
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string data = value.ToString();
            string result = data[0] == '#' ? data.Substring(1) : data;
            return ConverterTools.Convert(result, targetType);
        }
    }

    [Flags]
    public enum CreateFeedType
    {
        Feed = 0x01,
        Reply = 0x02,
        ReplyReply = Feed | Reply
    }
}
