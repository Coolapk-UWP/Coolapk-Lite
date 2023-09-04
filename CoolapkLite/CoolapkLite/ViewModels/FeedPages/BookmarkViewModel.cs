using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

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

        protected static async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, BookmarkViewModel> cache in Caches)
                {
                    if (cache.Key?.HasThreadAccess == false)
                    {
                        await cache.Key.ResumeForegroundAsync();
                    }
                    cache.Value.PropertyChanged?.Invoke(cache.Value, new PropertyChangedEventArgs(name));
                }
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
                await SettingsHelper.SetFile(SettingsHelper.Bookmark, Bookmarks.ToArray());
            }
            if (reset)
            {
                Bookmarks = new ObservableCollection<Bookmark>(await SettingsHelper.GetFile<Bookmark[]>(SettingsHelper.Bookmark));
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is BookmarkViewModel model && IsEqual(model);

        public bool IsEqual(BookmarkViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
    }
}
