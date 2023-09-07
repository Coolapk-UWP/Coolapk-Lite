using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = MediaElementName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = MediaElementBorderName, Type = typeof(FrameworkElement))]
    public class MediaPlayerElementEx : Control
    {
        private const string MediaElementName = "PART_MediaElement";
        private const string MediaElementBorderName = "PART_MediaElementBorder";
        public static bool IsMediaPlayerElementSupported { get; } = ApiInfoHelper.IsMediaPlayerElementSupported;

        private Uri _mediaUri;
        private FrameworkElement MediaElementBorder;

        public FrameworkElement MediaElement;

        private AdaptiveMediaSource _mediaSource;
        private AdaptiveMediaSource AdaptiveMediaSource
        {
            get => _mediaSource;
            set
            {
                if (_mediaSource != value)
                {
                    _mediaSource?.Dispose();
                    _mediaSource = value;
                }
            }
        }

        private HttpRandomAccessStream _mediaStream;
        private HttpRandomAccessStream HttpRandomAccessStream
        {
            get => _mediaStream;
            set
            {
                if (_mediaStream != value)
                {
                    _mediaStream?.Dispose();
                    _mediaStream = value;
                }
            }
        }

        #region AreTransportControlsEnabled 

        /// <summary>
        /// Identifies the <see cref="AreTransportControlsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AreTransportControlsEnabledProperty =
            DependencyProperty.Register(
                nameof(AreTransportControlsEnabled),
                typeof(bool),
                typeof(MediaPlayerElementEx),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that determines whether the standard transport controls are enabled.
        /// </summary>
        public bool AreTransportControlsEnabled
        {
            get => (bool)GetValue(AreTransportControlsEnabledProperty);
            set => SetValue(AreTransportControlsEnabledProperty, value);
        }

        #endregion

        #region AutoPlay 

        /// <summary>
        /// Identifies the <see cref="AutoPlay"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register(
                nameof(AutoPlay),
                typeof(bool),
                typeof(MediaPlayerElementEx),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether media will begin playback automatically when the <see cref="MediaInfo"/> property is set.
        /// </summary>
        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }

        #endregion

        #region MediaInfo

        /// <summary>
        /// Identifies the <see cref="MediaInfo"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MediaInfoProperty =
            DependencyProperty.Register(
                nameof(MediaInfo),
                typeof(JObject),
                typeof(MediaPlayerElementEx),
                new PropertyMetadata(null, OnMediaInfoPropertyChanged));

        public JObject MediaInfo
        {
            get => (JObject)GetValue(MediaInfoProperty);
            set => SetValue(MediaInfoProperty, value);
        }

        private static void OnMediaInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaPlayerElementEx)d).UpdateMediaInfo();
        }

        #endregion

        #region Stretch 

        /// <summary>
        /// Identifies the <see cref="Stretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(
                nameof(Stretch),
                typeof(Stretch),
                typeof(MediaPlayerElementEx),
                new PropertyMetadata(Stretch.Uniform));

        /// <summary>
        /// Gets or sets a value that describes how an <see cref="MediaPlayerElementEx"/> should be stretched to fill the destination rectangle.
        /// </summary>
        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(MediaPlayerElementExTemplateSettings),
                typeof(MediaPlayerElementEx),
                null);

        public MediaPlayerElementExTemplateSettings TemplateSettings
        {
            get => (MediaPlayerElementExTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="MediaPlayerElementEx"/> class.
        /// </summary>
        public MediaPlayerElementEx()
        {
            DefaultStyleKey = typeof(MediaPlayerElementEx);
            SetValue(TemplateSettingsProperty, new MediaPlayerElementExTemplateSettings());
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (MediaElementBorder != null)
            {
                MediaElementBorder.SizeChanged -= MediaElement_SizeChanged;
            }

            MediaElement = GetTemplateChild(MediaElementName) as FrameworkElement;
            MediaElementBorder = GetTemplateChild(MediaElementBorderName) as FrameworkElement;

            if (MediaElementBorder != null)
            {
                MediaElementBorder.SizeChanged += MediaElement_SizeChanged;
            }

            InitializeAdaptiveMediaSource();
        }

        private void MediaElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = e.NewSize.Width * 9 / 16;

            if (MediaElementBorder != null)
            {
                MediaElementBorder.Height = height;
            }
        }

        private void UpdateMediaInfo()
        {
            if (MediaInfo != null)
            {
                ParseMediaInfo(MediaInfo);
            }
        }

        private void ParseMediaInfo(JObject json)
        {
            MediaPlayerElementExTemplateSettings templateSettings = TemplateSettings;

            if (json.TryGetValue("name", out JToken name))
            {
                templateSettings.Title = name.ToString();
            }

            if (MediaInfo.TryGetValue("artistName", out JToken artistName))
            {
                templateSettings.Artist = artistName.ToString();
            }

            if (json.TryGetValue("cover", out JToken cover))
            {
                templateSettings.PosterSource = new ImageModel(cover.ToString(), ImageType.OriginImage, Dispatcher);
            }

            if (json.TryGetValue("requestParams", out JToken v))
            {
                if (JObject.Parse(v.ToString())?.First?.First is JObject request
                    && request.TryGetValue("fromType", out JToken fromType))
                {
                    switch (fromType.ToString())
                    {
                        case "weiboDirect":
                            if (request.TryGetValue("0", out JToken url))
                            {
                                GetWeiboVideo(url.ToString());
                            }
                            break;
                        case "biliBiliNormal2":
                            if (json.TryGetValue("playHeaders", out JToken playHeaders))
                            {
                                if (request.TryGetValue("0", out JToken avid))
                                {
                                    if (request.TryGetValue("1", out JToken cid))
                                    {
                                        GetBilibiliVideo(avid.ToString(), cid.ToString(), playHeaders.ToString());
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private async void GetWeiboVideo(string url)
        {
            if (url.TryGetUri(out Uri uri))
            {
                (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri);
                if (isSucceed)
                {
                    Match match = Regex.Match(result, @"var \$render_data = \[((?:.|\n)*?)\]\[0\] \|\| \{\};");
                    if (match.Success && match.Groups.Count >= 2)
                    {
                        JObject json = JObject.Parse(match.Groups[1].Value);
                        if (json.TryGetValue("status", out JToken v1))
                        {
                            JObject status = (JObject)v1;
                            if (status.TryGetValue("page_info", out JToken v2))
                            {
                                JObject page_info = (JObject)v2;
                                if (page_info.TryGetValue("media_info", out JToken v3))
                                {
                                    JObject media_info = (JObject)v3;
                                    if (media_info.TryGetValue("stream_url_hd", out JToken stream_url_hd))
                                    {
                                        if (stream_url_hd.ToString().TryGetUri(out Uri stream_uri_hd))
                                        {
                                            InitializeAdaptiveMediaSource(stream_uri_hd);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void GetBilibiliVideo(string avid, string cid, string playHeaders)
        {
            Uri uri = new Uri($"https://api.bilibili.com/x/player/playurl?avid={avid}&cid={cid}&qn=64");
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri);
            if (isSucceed)
            {
                JObject json = JObject.Parse(result);
                if (json.TryGetValue("data", out JToken v1))
                {
                    JObject data = (JObject)v1;
                    if (data.TryGetValue("durl", out JToken durl))
                    {
                        JObject d = durl.First as JObject;
                        if (d.TryGetValue("url", out JToken url))
                        {
                            if (url.ToString().TryGetUri(out uri))
                            {
                                HttpClient httpClient = new HttpClient();
                                if (!string.IsNullOrEmpty(playHeaders))
                                {
                                    json = JObject.Parse(playHeaders);
                                    if (json.TryGetValue("Referer", out JToken Referer))
                                    {
                                        httpClient.DefaultRequestHeaders.Referer = Referer.ToString().TryGetUri();
                                    }
                                    if (json.TryGetValue("User-Agent", out JToken User_Agent))
                                    {
                                        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(User_Agent.ToString());
                                    }
                                }
                                InitializeAdaptiveMediaSource(uri, httpClient);
                            }
                        }
                    }
                }
            }
        }

        private async void InitializeAdaptiveMediaSource(Uri uri)
        {
            _mediaUri = uri;
            AdaptiveMediaSourceCreationResult result = await AdaptiveMediaSource.CreateFromUriAsync(uri);
            if (result.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                AdaptiveMediaSource = result.MediaSource;
                if (MediaElement is null) { return; }
                if (MediaElement is MediaElement mediaElement)
                {
                    mediaElement.SetMediaStreamSource(AdaptiveMediaSource);
                }
                else if (IsMediaPlayerElementSupported && MediaElement is MediaPlayerElement mediaPlayerElement)
                {
                    mediaPlayerElement.SetMediaPlayer(new MediaPlayer());
                    mediaPlayerElement.MediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(_mediaSource);
                }
                AdaptiveMediaSource.InitialBitrate = AdaptiveMediaSource.AvailableBitrates.Max();
            }
            else
            {
                AdaptiveMediaSource = null;
                if (MediaElement is null) { return; }
                if (MediaElement is MediaElement mediaElement)
                {
                    mediaElement.Source = uri;
                }
                else if (IsMediaPlayerElementSupported && MediaElement is MediaPlayerElement mediaPlayerElement)
                {
                    mediaPlayerElement.Source = MediaSource.CreateFromUri(uri);
                }
            }
            HttpRandomAccessStream = null;
        }

        private async void InitializeAdaptiveMediaSource(Uri uri, HttpClient httpClient)
        {
            _mediaUri = uri;
            AdaptiveMediaSourceCreationResult result = await AdaptiveMediaSource.CreateFromUriAsync(uri, httpClient);
            if (result.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                HttpRandomAccessStream = null;
                AdaptiveMediaSource = result.MediaSource;
                if (MediaElement is null) { return; }
                if (MediaElement is MediaElement mediaElement)
                {
                    mediaElement.SetMediaStreamSource(AdaptiveMediaSource);
                }
                else if (IsMediaPlayerElementSupported && MediaElement is MediaPlayerElement mediaPlayerElement)
                {
                    mediaPlayerElement.SetMediaPlayer(new MediaPlayer());
                    mediaPlayerElement.MediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(AdaptiveMediaSource);
                }
                AdaptiveMediaSource.InitialBitrate = AdaptiveMediaSource.AvailableBitrates.Max();
            }
            else
            {
                AdaptiveMediaSource = null;
                HttpRandomAccessStream = await HttpRandomAccessStream.CreateAsync(httpClient, uri);
                if (MediaElement is null) { return; }
                if (MediaElement is MediaElement mediaElement)
                {
                    mediaElement.SetSource(HttpRandomAccessStream, HttpRandomAccessStream.ContentType);
                }
                else if (IsMediaPlayerElementSupported && MediaElement is MediaPlayerElement mediaPlayerElement)
                {
                    mediaPlayerElement.Source = MediaSource.CreateFromStream(HttpRandomAccessStream, HttpRandomAccessStream.ContentType);
                }
            }
        }

        private void InitializeAdaptiveMediaSource()
        {
            if (MediaElement is null) { return; }
            if (MediaElement is MediaElement mediaElement)
            {
                if (AdaptiveMediaSource != null)
                {
                    mediaElement.SetMediaStreamSource(AdaptiveMediaSource);
                    AdaptiveMediaSource.InitialBitrate = AdaptiveMediaSource.AvailableBitrates.Max();
                }
                else if (HttpRandomAccessStream != null)
                {
                    mediaElement.SetSource(HttpRandomAccessStream, HttpRandomAccessStream.ContentType);
                }
                else if (_mediaUri != null)
                {
                    mediaElement.Source = _mediaUri;
                }
            }
            else if (IsMediaPlayerElementSupported && MediaElement is MediaPlayerElement mediaPlayerElement)
            {
                if (AdaptiveMediaSource != null)
                {
                    mediaPlayerElement.SetMediaPlayer(new MediaPlayer());
                    mediaPlayerElement.MediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(AdaptiveMediaSource);
                }
                else if (HttpRandomAccessStream != null)
                {
                    mediaPlayerElement.Source = MediaSource.CreateFromStream(HttpRandomAccessStream, HttpRandomAccessStream.ContentType);
                }
                else if (_mediaUri != null)
                {
                    mediaPlayerElement.Source = MediaSource.CreateFromUri(_mediaUri);
                }
            }
        }
    }
}
