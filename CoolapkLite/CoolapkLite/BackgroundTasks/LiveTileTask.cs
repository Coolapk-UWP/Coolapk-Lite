using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Feeds;
using CoolapkLite.Models.Users;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using mtuc = Microsoft.Toolkit.Uwp.Connectivity;

namespace CoolapkLite.BackgroundTasks
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public static LiveTileTask Instance = new LiveTileTask();

        public LiveTileTask() => Instance = Instance ?? this;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            await UpdateTile();
            deferral.Complete();
        }

        public async Task UpdateTile()
        {
            if (mtuc.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                Uri uri = new Uri(SettingsHelper.Get<string>(SettingsHelper.TileUrl));
                try
                {
                    await GetData(uri);
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.GetLogger(nameof(LiveTileTask)).Error(ex.ExceptionToMessage(), ex);
                }
            }
        }

        private async Task GetData(Uri uri)
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(uri, true);
            if (isSucceed)
            {
                JArray array = (JArray)result;
                if (array.Count < 1) { return; }
                foreach (JToken item in array.Take(5))
                {
                    if (((JObject)item).TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            UpdateTile(GetFeedTile((JObject)item));
                        }
                    }
                }
            }
        }

        private void UpdateTile(TileContent tileContent)
        {
            try
            {
                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.EnableNotificationQueue(true);
                TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                tileUpdater.Update(tileNotification);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(LiveTileTask)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public static async Task<bool> PinSecondaryTile(string tileId, string displayName, string arguments)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse();
            bool isPinned = SecondaryTile.Exists(tileId);
            if (isPinned)
            {
                UIHelper.ShowMessage(loader.GetString("AlreadyPinnedTile"));
            }
            else
            {
                // Initialize the tile with required arguments
                SecondaryTile tile = new SecondaryTile(
                    tileId,
                    displayName,
                    arguments,
                    new Uri("ms-appx:///Assets/Square150x150Logo.png"),
                    Windows.UI.StartScreen.TileSize.Default);

                // Enable wide and large tile sizes
                tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");

                // Add a small size logo for better looking small tile
                tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/SmallTile.png");

                // Add a unique corner logo for the secondary tile
                tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");

                // Show the display name on all sizes
                tile.VisualElements.ShowNameOnSquare150x150Logo = true;
                tile.VisualElements.ShowNameOnWide310x150Logo = true;
                tile.VisualElements.ShowNameOnSquare310x310Logo = true;

                // Pin the tile
                isPinned = await tile.RequestCreateAsync();

                if (isPinned) { UIHelper.ShowMessage(loader.GetString("PinnedTileSucceeded")); }
            }
            return isPinned;
        }

        public static void UpdateTile(string tileId, TileContent tileContent)
        {
            try
            {
                TileUpdater tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);
                tileUpdater.Clear();
                tileUpdater.EnableNotificationQueue(true);
                TileNotification tileNotification = new TileNotification(tileContent.GetXml());
                tileUpdater.Update(tileNotification);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(LiveTileTask)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public static TileContent GetFeedTile(JObject token)
        {
            FeedModel FeedDetail = new FeedModel(token);
            return GetFeedTile(FeedDetail);
        }

        public static TileContent GetFeedTile(FeedModelBase FeedDetail)
        {
            string Message = FeedDetail.Message.CSStoString();
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = FeedDetail.Info,

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 3
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = FeedDetail.UserInfo.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = FeedDetail.UserInfo.UserName,
                                                    HintStyle = AdaptiveTextStyle.Caption
                                                },

                                                new AdaptiveText
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

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = FeedDetail.Pic == null ? null : new TileBackgroundImage
                            {
                                Source = FeedDetail.Pic.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = FeedDetail.UserInfo.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                },
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                    }
                                },

                                new AdaptiveText
                                {
                                    Text = FeedDetail.UserInfo.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = Message,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 3
                                }
                            }
                        }
                    }
                }
            };
        }

        public static TileContent GetUserTile(JObject token)
        {
            UserModel UserDetail = new UserModel(token);
            return GetUserTile(UserDetail);
        }

        public static TileContent GetUserTile(IUserModel UserDetail)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = UserDetail.UserName,

                    TileSmall = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveImage
                                {
                                    Source = UserDetail.UserAvatar.Uri,
                                    HintCrop = AdaptiveImageCrop.Circle
                                }
                            }
                        }
                    },

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            PeekImage = new TilePeekImage
                            {
                                Source = UserDetail.UserAvatar.Uri,
                                HintCrop = TilePeekImageCrop.Circle
                            },

                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = UserDetail.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.FollowNum}{loader.GetString("Follow")} {UserDetail.FansNum}{loader.GetString("Fan")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.LoginText}{loader.GetString("Active")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText
                                {
                                    Text = UserDetail.Bio,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 2
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = UserDetail.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = UserDetail.UserName,
                                                    HintStyle = AdaptiveTextStyle.Caption
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = $"{UserDetail.FollowNum}{loader.GetString("Follow")} {UserDetail.FansNum}{loader.GetString("Fan")}",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = $"{UserDetail.LoginText}{loader.GetString("Active")}",
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = UserDetail.Bio,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true,
                                                    HintMaxLines = 2
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    },

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            BackgroundImage = new TileBackgroundImage
                            {
                                Source = UserDetail.Cover.Uri,
                                HintOverlay = 70
                            },

                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = UserDetail.UserAvatar.Uri,
                                                    HintCrop = AdaptiveImageCrop.Circle
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                    }
                                },

                                new AdaptiveText
                                {
                                    Text = UserDetail.UserName,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.FollowNum}{loader.GetString("Follow")} {UserDetail.FansNum}{loader.GetString("Fan")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText
                                {
                                    Text = $"{UserDetail.LoginText}{loader.GetString("Active")}",
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },

                                new AdaptiveText
                                {
                                    Text = UserDetail.Bio,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    }
                }
            };
        }

        public static TileContent GetListTile(IHasDescription ListDetail)
        {
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = ListDetail.Title,

                    TileSmall = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveImage
                                {
                                    Source = ListDetail.Pic.Uri
                                }
                            }
                        }
                    },

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            PeekImage = new TilePeekImage
                            {
                                Source = ListDetail.Pic.Uri
                            },

                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = ListDetail.Title,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = ListDetail.Pic.Uri
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = ListDetail.Title,
                                                    HintStyle = AdaptiveTextStyle.Caption
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = ListDetail.Description,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    },

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = ListDetail.Pic.Uri
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                    }
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.Title,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    }
                }
            };
        }
        public static TileContent GetListTile(IHasSubtitle ListDetail)
        {
            return new TileContent
            {
                Visual = new TileVisual
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = ListDetail.Title,

                    TileSmall = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveImage
                                {
                                    Source = ListDetail.Pic.Uri
                                }
                            }
                        }
                    },

                    TileMedium = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            PeekImage = new TilePeekImage
                            {
                                Source = ListDetail.Pic.Uri
                            },

                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = ListDetail.Title,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.SubTitle,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.Description,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = ListDetail.Pic.Uri
                                                }
                                            },
                                        },
                                        new AdaptiveSubgroup
                                        {
                                            Children =
                                            {
                                                new AdaptiveText
                                                {
                                                    Text = ListDetail.Title,
                                                    HintStyle = AdaptiveTextStyle.Caption
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = ListDetail.SubTitle,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true
                                                },

                                                new AdaptiveText
                                                {
                                                    Text = ListDetail.Description,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true
                                                }
                                            },
                                        }
                                    }
                                },
                            }
                        }
                    },

                    TileLarge = new TileBinding
                    {
                        Content = new TileBindingContentAdaptive
                        {
                            Children =
                            {
                                new AdaptiveGroup
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage
                                                {
                                                    Source = ListDetail.Pic.Uri
                                                }
                                            }
                                        },

                                        new AdaptiveSubgroup()
                                    }
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.Title,
                                    HintStyle = AdaptiveTextStyle.Caption
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.SubTitle,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true
                                },

                                new AdaptiveText
                                {
                                    Text = ListDetail.Description,
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
