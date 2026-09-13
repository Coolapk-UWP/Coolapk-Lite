using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.StartScreen;

namespace CoolapkLite.ViewModels.FeedPages
{
    public sealed class BookmarkViewModel : CachedListViewModelBase<BookmarkViewModel, Bookmark>
    {
        private static readonly AsyncLock locker = new AsyncLock();

        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Bookmark");

        public BookmarkViewModel(CoreDispatcher dispatcher) : base(dispatcher) { }

        #region IList<Bookmark> Members

        protected override void SetIndex(int index, Bookmark value)
        {
            Bookmark old = _items[index];
            if (old != value && !_items.Contains(value))
            {
                Replace(index, old, value);
            }
        }

        protected override void Replace(int index, Bookmark old, Bookmark item)
        {
            _ = SaveBookmarks();
            base.Replace(index, old, item);
        }

        public override ReplaceStatus AddOrReplace(Bookmark item)
        {
            if (_items.Contains(item))
            {
                return ReplaceStatus.Duplicated;
            }
            else
            {
                _ = SaveBookmarks();
                return base.AddOrReplace(item);
            }
        }

        public override void Insert(int index, Bookmark item)
        {
            if (!_items.Contains(item))
            {
                _ = SaveBookmarks();
                base.Insert(index, item);
            }
        }

        protected override void Remove(int index, Bookmark item)
        {
            _ = SaveBookmarks();
            base.Remove(index, item);
        }

        #endregion

        public override async Task Refresh(bool reset)
        {
            IEnumerable<Bookmark> bookmarks = await SettingsHelper.GetAsync<IEnumerable<Bookmark>>(SettingsHelper.Bookmark).ConfigureAwait(false);
            Clear(); AddRange(bookmarks);
            await UpdateJumpListAsync().ConfigureAwait(false);
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
                        if (_items.Count > 0)
                        {
                            int count = 0;
                            foreach (Bookmark bookmark in bookmarks)
                            {
                                if (!_items.Contains(bookmark))
                                {
                                    _items.Add(bookmark);
                                    count++;
                                }
                            }
                            _ = Dispatcher.ShowMessageAsync($"成功导入 {count} 个收藏夹");
                        }
                        else
                        {
                            _items.AddRange(bookmarks);
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
                string content = JsonConvert.SerializeObject(_items, _items.GetType(), Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

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

        private async Task SaveBookmarks()
        {
            using (await locker.LockAsync())
            {
                await Task.WhenAll(
                    UpdateJumpListAsync(),
                    SettingsHelper.SetAsync(SettingsHelper.Bookmark, _items)).ConfigureAwait(false);
            }
        }

        private async Task UpdateJumpListAsync()
        {
            if (ApiInfoHelper.IsJumpListSupported && JumpList.IsSupported())
            {
                JumpList list = await JumpList.LoadCurrentAsync();

                if (list.Items.Count <= 0) { App.AddJumpList(list); }
                _ = list.Items.RemoveAll(x => x.GroupName == "收藏");
                list.Items.AddRange(_items.Take(4).Select(x => JumpListItem.CreateWithArguments(x.Url, x.Title).AddGroupNameAndLogo("收藏", new Uri("ms-appx:///Assets/Icons/KnowledgeArticle.png"))));

                await list.SaveAsync();
            }
        }

        public bool IsEqual(BookmarkViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
