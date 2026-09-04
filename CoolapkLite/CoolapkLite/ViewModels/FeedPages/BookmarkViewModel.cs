using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
