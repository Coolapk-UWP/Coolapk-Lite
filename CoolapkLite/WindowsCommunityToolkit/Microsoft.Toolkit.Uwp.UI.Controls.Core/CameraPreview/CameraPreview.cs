// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// Camera Control to preview video. Can subscribe to video frames, software bitmap when they arrive.
    /// </summary>
    [TemplatePart(Name = Preview_MediaPlayerElementControl, Type = typeof(MediaPlayerElement))]
    [TemplatePart(Name = Preview_FrameSourceGroupButton, Type = typeof(Button))]
    public partial class CameraPreview : Control
    {
        private MediaPlayer _mediaPlayer;
        private MediaPlayerElement _mediaPlayerElementControl;
        private Button _frameSourceGroupButton;

        private IReadOnlyList<MediaFrameSourceGroup> _frameSourceGroups;

        private bool IsFrameSourceGroupButtonAvailable => _frameSourceGroups != null && _frameSourceGroups.Count > 1;

        /// <summary>
        /// Gets Camera Helper
        /// </summary>
        public CameraHelper CameraHelper { get; private set; }

        /// <summary>
        /// Initialize control with a default CameraHelper instance for video preview and frame capture.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync()
        {
            await StartAsync(new CameraHelper());
        }

        /// <summary>
        /// Initialize control with a CameraHelper instance for video preview and frame capture.
        /// </summary>
        /// <param name="cameraHelper"><see cref="CameraHelper"/></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync(CameraHelper cameraHelper)
        {
            if (cameraHelper == null)
            {
                cameraHelper = new CameraHelper();
            }

            CameraHelper = cameraHelper;
            _frameSourceGroups = await CameraHelper.GetFrameSourceGroupsAsync();

            // UI controls exist and are initialized
            if (_mediaPlayerElementControl != null)
            {
                await InitializeAsync();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraPreview"/> class.
        /// </summary>
        public CameraPreview()
        {
            this.DefaultStyleKey = typeof(CameraPreview);
        }

        /// <inheritdoc/>
        protected override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_frameSourceGroupButton != null)
            {
                _frameSourceGroupButton.Click -= FrameSourceGroupButton_ClickAsync;
            }

            _mediaPlayerElementControl = (MediaPlayerElement)GetTemplateChild(Preview_MediaPlayerElementControl);
            _frameSourceGroupButton = (Button)GetTemplateChild(Preview_FrameSourceGroupButton);

            if (_frameSourceGroupButton != null)
            {
                _frameSourceGroupButton.Click += FrameSourceGroupButton_ClickAsync;
                _frameSourceGroupButton.IsEnabled = false;
                _frameSourceGroupButton.Visibility = Visibility.Collapsed;
            }

            if (CameraHelper != null)
            {
                await InitializeAsync();
            }
        }

        private async Task InitializeAsync()
        {
            CameraHelperResult result = await CameraHelper.InitializeAndStartCaptureAsync();
            if (result != CameraHelperResult.Success)
            {
                InvokePreviewFailed(result.ToString());
            }

            SetUIControls(result);
        }

        private async void FrameSourceGroupButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            MediaFrameSourceGroup oldGroup = CameraHelper.FrameSourceGroup;
            int currentIndex = _frameSourceGroups.Select((grp, index) => new { grp, index }).First(v => v.grp.Id == oldGroup.Id).index;
            int newIndex = currentIndex < (_frameSourceGroups.Count - 1) ? currentIndex + 1 : 0;
            MediaFrameSourceGroup group = _frameSourceGroups[newIndex];
            _frameSourceGroupButton.IsEnabled = false;
            CameraHelper.FrameSourceGroup = group;
            await InitializeAsync();
        }

        private void InvokePreviewFailed(string error)
        {
            EventHandler<PreviewFailedEventArgs> handler = PreviewFailed;
            handler?.Invoke(this, new PreviewFailedEventArgs { Error = error });
        }

        private void SetMediaPlayerSource()
        {
            try
            {
                MediaFrameSource frameSource = CameraHelper?.PreviewFrameSource;
                if (frameSource != null)
                {
                    if (_mediaPlayer == null)
                    {
                        _mediaPlayer = new MediaPlayer
                        {
                            AutoPlay = true,
                            RealTimePlayback = true
                        };
                    }

                    _mediaPlayer.Source = MediaSource.CreateFromMediaFrameSource(frameSource);
                    _mediaPlayerElementControl.SetMediaPlayer(_mediaPlayer);
                }
            }
            catch (Exception ex)
            {
                InvokePreviewFailed(ex.Message);
            }
        }

        private void SetUIControls(CameraHelperResult result)
        {
            bool success = result == CameraHelperResult.Success;
            if (success)
            {
                SetMediaPlayerSource();
            }
            else
            {
                _mediaPlayerElementControl.SetMediaPlayer(null);
            }

            _frameSourceGroupButton.IsEnabled = IsFrameSourceGroupButtonAvailable;
            SetFrameSourceGroupButtonVisibility();
        }

        private void SetFrameSourceGroupButtonVisibility()
        {
            _frameSourceGroupButton.Visibility = IsFrameSourceGroupButtonAvailable && IsFrameSourceGroupButtonVisible
                ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Stops preview.
        /// </summary>
        public void Stop()
        {
            _mediaPlayerElementControl?.SetMediaPlayer(null);

            if (_mediaPlayer != null)
            {
                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }
        }
    }
}