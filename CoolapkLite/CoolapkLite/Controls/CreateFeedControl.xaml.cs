using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Resources;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkLite.Controls
{
    public sealed partial class CreateFeedControl : UserControl
    {
        public static readonly DependencyProperty FeedTypeProperty =
            DependencyProperty.Register(
                nameof(FeedType),
                typeof(CreateFeedType),
                typeof(CreateFeedControl),
                new PropertyMetadata(CreateFeedType.Feed));

        public CreateFeedType FeedType
        {
            get => (CreateFeedType)GetValue(FeedTypeProperty);
            set => SetValue(FeedTypeProperty, value);
        }

        public static readonly DependencyProperty ReplyIDProperty =
            DependencyProperty.Register(
                nameof(ReplyID),
                typeof(int),
                typeof(CreateFeedControl),
                null);

        public int ReplyID
        {
            get => (int)GetValue(ReplyIDProperty);
            set => SetValue(ReplyIDProperty, value);
        }

        internal readonly ObservableCollection<WriteableBitmap> Pictures = new ObservableCollection<WriteableBitmap>();

        public CreateFeedControl() => InitializeComponent();

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Send":
                    CreateDataContent();
                    break;
                case "AddPic":
                    PickImage();
                    break;
                default:
                    break;
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Name)
            {
                case "Delete":
                    Pictures.Remove((sender as FrameworkElement).Tag as WriteableBitmap);
                    break;
                default:
                    break;
            }
        }

        private void CreateDataContent()
        {
            UIHelper.ShowProgressBar();
            InputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            contentText = contentText.Replace("\r", "\r\n");
            if (string.IsNullOrWhiteSpace(contentText)) { return; }
            if (FeedType == CreateFeedType.Feed) { CreateFeedContent(contentText); }
            else { CreateReplyContent(contentText); }
        }

        private async void CreateFeedContent(string contentText)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent message = new StringContent(contentText))
                using (StringContent type = new StringContent("feed"))
                using (StringContent is_html_article = new StringContent("0"))
                using (StringContent pic = new StringContent(string.Join(",", await UploadPic())))
                {
                    content.Add(message, "message");
                    content.Add(type, "type");
                    content.Add(is_html_article, "is_html_article");
                    content.Add(pic, "pic");
                    await SendContent(content);
                }
            }
        }

        private async void CreateReplyContent(string contentText)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent message = new StringContent(contentText))
                using (StringContent pic = new StringContent(string.Join(",", await UploadPic())))
                {
                    content.Add(message, "message");
                    content.Add(pic, "pic");
                    await SendContent(content);
                }
            }
        }

        private async Task SendContent(HttpContent content)
        {
            UriType type = (UriType)(-1);
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

            try
            {
                object[] arg = Array.Empty<object>();
                if (type != UriType.CreateFeed)
                {
                    arg = new object[] { ReplyID };
                }
                (bool isSucceed, JToken _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, arg), content);
                if (isSucceed)
                {
                    SendSuccessful();
                }
            }
            catch (CoolapkMessageException cex)
            {
                UIHelper.ShowMessage(cex.Message);
                if (cex.MessageStatus == CoolapkMessageException.RequestCaptcha)
                {
                    //CaptchaDialog dialog = new CaptchaDialog();
                    //_ = await dialog.ShowAsync();
                }
            }
            UIHelper.HideProgressBar();
        }

        private void SendSuccessful()
        {
            UIHelper.ShowMessage(ResourceLoader.GetForViewIndependentUse("CreateFeedControl").GetString("SendSuccessed"));
            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
            Pictures.Clear();
        }

        public async void PickImage()
        {
            FileOpenPicker FileOpen = new FileOpenPicker();
            FileOpen.FileTypeFilter.Add(".jpg");
            FileOpen.FileTypeFilter.Add(".jpeg");
            FileOpen.FileTypeFilter.Add(".png");
            FileOpen.FileTypeFilter.Add(".bmp");
            FileOpen.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            StorageFile file = await FileOpen.PickSingleFileAsync();
            if (file != null)
            {
                using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                {
                    BitmapDecoder ImageDecoder = await BitmapDecoder.CreateAsync(stream);
                    SoftwareBitmap SoftwareImage = await ImageDecoder.GetSoftwareBitmapAsync();
                    try
                    {
                        WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                        await WriteableImage.SetSourceAsync(stream);
                        Pictures.Add(WriteableImage);
                    }
                    catch
                    {
                        try
                        {
                            using (InMemoryRandomAccessStream random = new InMemoryRandomAccessStream())
                            {
                                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, random);
                                encoder.SetSoftwareBitmap(SoftwareImage);
                                await encoder.FlushAsync();
                                WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                                await WriteableImage.SetSourceAsync(random);
                                Pictures.Add(WriteableImage);
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        private async Task<List<string>> UploadPic()
        {
            int i = 0;
            UIHelper.ShowMessage("上传图片");
            List<string> results = new List<string>();
            foreach (WriteableBitmap pic in Pictures)
            {
                i++;
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    Stream pixelStream = pic.PixelBuffer.AsStream();
                    byte[] pixels = new byte[pixelStream.Length];
                    await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                    encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                        (uint)pic.PixelWidth,
                        (uint)pic.PixelHeight,
                        96.0,
                        96.0,
                        pixels);

                    await encoder.FlushAsync();

                    byte[] bytes = stream.GetBytes();
                    (bool isSucceed, string result) = await RequestHelper.UploadImage(bytes, "pic");
                    if (isSucceed) { results.Add(result); }
                }
                UIHelper.ShowMessage($"已上传 ({i}/{Pictures.Count})");
            }
            return results;
        }
    }

    public enum CreateFeedType
    {
        Feed,
        Reply,
        ReplyReply
    }
}
