using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.SettingsPages;
using System;
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

        public string Title { get; } = ResourceLoader.GetForCurrentView("MainPage").GetString("Bookmark");

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
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
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

        public BookmarkViewModel() => Caches[Dispatcher] = this;

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
