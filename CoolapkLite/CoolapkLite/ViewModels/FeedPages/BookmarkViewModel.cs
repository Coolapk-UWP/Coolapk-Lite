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
    public class BookmarkViewModel : IViewModel
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

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
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
            if (Bookmarks != null)
            {
                await SettingsHelper.SetAsync(SettingsHelper.Bookmark, Bookmarks.ToArray()).ConfigureAwait(false);
            }
            if (reset)
            {
                Bookmarks = await SettingsHelper.GetAsync<Bookmark[]>(SettingsHelper.Bookmark).ContinueWith(x => new ObservableCollection<Bookmark>(x.Result)).ConfigureAwait(false);
            }
            await UpdateJumpListAsync().ConfigureAwait(false);
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
