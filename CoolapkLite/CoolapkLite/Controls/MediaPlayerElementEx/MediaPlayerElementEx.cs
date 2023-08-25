using CoolapkLite.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation.Metadata;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static QRCoder.PayloadGenerator;

namespace CoolapkLite.Controls
{
    [TemplatePart(Name = MediaElementName, Type = typeof(FrameworkElement))]
    public class MediaPlayerElementEx : Control
    {
        private const string MediaElementName = "PART_MediaElement";
        private readonly bool IsSupportedMediaPlayerElement = ApiInformation.IsTypePresent("Windows.UI.Xaml.Controls.MediaPlayerElement");

        private Uri _mediaUri;
        private AdaptiveMediaSource _mediaSource;
        private FrameworkElement MediaElement;

        #region Source

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(string),
                typeof(MediaPlayerElementEx),
                new PropertyMetadata(null, OnSourcePropertyChanged));

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaPlayerElementEx)d).UpdateSource();
        }

        #endregion

        #region PosterSource

        /// <summary>
        /// Identifies the <see cref="PosterSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PosterSourceProperty =
            DependencyProperty.Register(
                nameof(PosterSource),
                typeof(ImageSource),
                typeof(MediaPlayerElementEx),
                null);

        public ImageSource PosterSource
        {
            get => (ImageSource)GetValue(PosterSourceProperty);
            set => SetValue(PosterSourceProperty, value);
        }

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="MediaPlayerElementEx"/> class.
        /// </summary>
        public MediaPlayerElementEx() => DefaultStyleKey = typeof(MediaPlayerElementEx);

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MediaElement = GetTemplateChild(MediaElementName) as FrameworkElement;
            InitializeAdaptiveMediaSource();
        }

        private void UpdateSource()
        {
            if (!string.IsNullOrWhiteSpace(Source))
            {
                ParseLink(Source);
            }
        }

        private async void ParseLink(string url)
        {
            if (url.Contains("b23.tv") || url.Contains("t.cn"))
            {
                url = (await url.TryGetUri().ExpandShortUrlAsync()).ToString();
            }

            if (url.Contains("weibo"))
            {
                Match match = Regex.Match(url, @"[^/]+(?!.*/)");
                if (match.Success)
                {
                    match = Regex.Match(match.Value, @"^\w+");
                    if (match.Success)
                    {
                        GetWeiboVideo(match.Value);
                    }
                }
            }
        }

        private async void GetWeiboVideo(string id)
        {
            Uri uri = new Uri($"https://m.weibo.cn/status/{id}");
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

        private async void InitializeAdaptiveMediaSource(Uri uri)
        {
            _mediaUri = uri;
            AdaptiveMediaSourceCreationResult result = await AdaptiveMediaSource.CreateFromUriAsync(uri);

            if (result.Status == AdaptiveMediaSourceCreationStatus.Success)
            {
                _mediaSource = result.MediaSource;
                if (MediaElement is null) { return; }
                if (MediaElement is MediaElement mediaElement)
                {
                    mediaElement.SetMediaStreamSource(_mediaSource);
                }
                else if (IsSupportedMediaPlayerElement && MediaElement is MediaPlayerElement mediaPlayerElement)
                {
                    mediaPlayerElement.SetMediaPlayer(new MediaPlayer());
                    mediaPlayerElement.MediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(_mediaSource);
                }
                _mediaSource.InitialBitrate = _mediaSource.AvailableBitrates.Max();
            }
            else
            {
                if (MediaElement is null) { return; }
                if (MediaElement is MediaElement mediaElement)
                {
                    mediaElement.Source = uri;
                }
                else if (IsSupportedMediaPlayerElement && MediaElement is MediaPlayerElement mediaPlayerElement)
                {
                    mediaPlayerElement.Source = MediaSource.CreateFromUri(uri);
                }
            }
        }

        private void InitializeAdaptiveMediaSource()
        {
            if (MediaElement is null) { return; }
            if (MediaElement is MediaElement mediaElement)
            {
                if (_mediaSource != null)
                {
                    mediaElement.SetMediaStreamSource(_mediaSource);
                    _mediaSource.InitialBitrate = _mediaSource.AvailableBitrates.Max();
                }
                else if (_mediaUri != null)
                {
                    mediaElement.Source = _mediaUri;
                }
            }
            else if (IsSupportedMediaPlayerElement && MediaElement is MediaPlayerElement mediaPlayerElement)
            {
                if (_mediaSource != null)
                {
                    mediaPlayerElement.SetMediaPlayer(new MediaPlayer());
                    mediaPlayerElement.MediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(_mediaSource);
                }
                else if (_mediaUri != null)
                {
                    mediaPlayerElement.Source = MediaSource.CreateFromUri(_mediaUri);
                }
            }
        }
    }
}
