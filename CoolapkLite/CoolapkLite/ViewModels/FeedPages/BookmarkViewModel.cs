using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.StartScreen;

namespace CoolapkLite.ViewModels.FeedPages
{
    public sealed class BookmarkViewModel : IViewModel
    {
        public static Dictionary<CoreDispatcher, BookmarkViewModel> Caches { get; } = new Dictionary<CoreDispatcher, BookmarkViewModel>();

        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Bookmark");

        private ObservableCollection<Bookmark> _bookmarks;
        public ObservableCollection<Bookmark> Bookmarks
        {
            get => _bookmarks;
            set => SetProperty(ref _bookmarks, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (property == null ? value != null : !property.Equals(value))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public BookmarkViewModel(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            Caches[dispatcher] = this;
        }

        public async Task Refresh(bool reset)
        {
            if (_bookmarks != null)
            {
                await SettingsHelper.SetAsync(SettingsHelper.Bookmark, _bookmarks.ToArray()).ConfigureAwait(false);
            }
            if (reset)
            {
                await ResetAsync().ConfigureAwait(false);
            }
            await UpdateJumpListAsync().ConfigureAwait(false);
            RefreshOthers();
        }

        public async Task ImportAsync()
        {
            try
            {
                FileOpenPicker fileOpenPicker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    ViewMode = PickerViewMode.List
                };
                fileOpenPicker.FileTypeFilter.Add(".json");

                StorageFile file = await fileOpenPicker.PickSingleFileAsync();
                if (file != null)
                {
                    string content = await FileIO.ReadTextAsync(file);
                    List<Bookmark> bookmarks = JsonConvert.DeserializeObject<List<Bookmark>>(content, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
                    if (bookmarks?.Count > 0)
                    {
                        if (_bookmarks.Count > 0)
                        {
                            int count = 0;
                            foreach (Bookmark bookmark in bookmarks)
                            {
                                if (!_bookmarks.Contains(bookmark))
                                {
                                    _bookmarks.Add(bookmark);
                                    count++;
                                }
                            }
                            _ = Dispatcher.ShowMessageAsync($"成功导入 {count} 个收藏夹");
                        }
                        else
                        {
                            _bookmarks.AddRange(bookmarks);
                            _ = Dispatcher.ShowMessageAsync($"成功导入 {bookmarks.Count} 个收藏夹");
                        }
                        await Refresh(false);
                    }
                    else
                    {
                        _ = Dispatcher.ShowMessageAsync("导入的文件中没有有效的收藏夹信息");
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(BookmarkViewModel)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        public async Task ExportAsync()
        {
            try
            {
                string content = JsonConvert.SerializeObject(_bookmarks, _bookmarks.GetType(), Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

                string fileName = Title;
                int index = fileName.LastIndexOf('.');
                FileSavePicker fileSavePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = $"Coolapk-Bookmarks_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                    FileTypeChoices = { { "json 文件", new[] { ".json" } } }
                };

                StorageFile file = await fileSavePicker.PickSaveFileAsync();
                if (file != null)
                {
                    await FileIO.WriteTextAsync(file, content);
                    _ = Dispatcher.ShowMessageAsync($"收藏夹已导出到 {file.Path}");
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(BookmarkViewModel)).Error(ex.ExceptionToMessage(), ex);
            }
        }

        private async Task ResetAsync() => Bookmarks = await SettingsHelper.GetAsync<Bookmark[]>(SettingsHelper.Bookmark).ContinueWith(x => new ObservableCollection<Bookmark>(x.Result)).ConfigureAwait(false);

        private void RefreshOthers()
        {
            foreach (KeyValuePair<CoreDispatcher, BookmarkViewModel> cache in Caches)
            {
                if (cache.Key != Dispatcher)
                {
                    _ = cache.Value.ResetAsync();
                }
            }
        }

        private async Task UpdateJumpListAsync()
        {
            if (ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported())
            {
                JumpList JumpList = await JumpList.LoadCurrentAsync();
                JumpList.SystemGroupKind = JumpListSystemGroupKind.None;

                _ = JumpList.Items.RemoveAll(x => x.GroupName == "收藏");
                JumpList.Items.AddRange(_bookmarks.Take(4).Select(x => JumpListItem.CreateWithArguments(x.Url, x.Title).AddGroupNameAndLogo("收藏", new Uri("ms-appx:///Assets/Icons/KnowledgeArticle.png"))));

                await JumpList.SaveAsync();
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is BookmarkViewModel model && IsEqual(model);

        public bool IsEqual(BookmarkViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
