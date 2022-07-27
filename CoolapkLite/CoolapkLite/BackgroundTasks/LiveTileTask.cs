using CoolapkLite.Helpers;
using CoolapkLite.Models.Feeds;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace CoolapkLite.BackgroundTasks
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            UpdateTile();
            deferral.Complete();
        }

        public static async void UpdateTile()
        {
            Uri uri = new Uri(SettingsHelper.Get<string>(SettingsHelper.TileUrl));
            try { await GetData(uri); } catch { }
        }

        private static async Task GetData(Uri uri)
        {
            (bool isSucceed, JToken result) = await NetworkHelper.GetDataAsync(uri, true);
            if (isSucceed)
            {
                JArray array = (JArray)result;
                if (array.Count < 1) { return; }
                int i = 0;
                foreach (JObject item in array)
                {
                    if (i >= 5) { break; }
                    if (item.TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            i++;
                            UpdateTitle(GetFeedTitle(item));
                        }
                    }
                }
            }
        }

        private static void UpdateTitle(TileContent tileContent)
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }
            catch { }
        }

        private static TileContent GetFeedTitle(JObject token)
        {
            FeedModel FeedDetail = new FeedModel(token);
            string Message = FeedDetail.Message.CSStoString();
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = FeedDetail.Info,
                    Arguments = FeedDetail.Url,

                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                },

                                new AdaptiveText()
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 3
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = FeedDetail.UserInfo.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = FeedDetail.UserInfo.UserName,
                                                    HintStyle = AdaptiveTextStyle.Caption,
                                                },

                                                new AdaptiveText()
                                                {
                                                    Text = Message,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    HintMaxLines = 3
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage()
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Base,
                                },

                                new AdaptiveText()
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
