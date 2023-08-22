using CoolapkLite.Helpers;
using CoolapkLite.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class BookmarkViewModel : IViewModel
    {
        public string Title { get; } = ResourceLoader.GetForCurrentView("MainPage").GetString("Bookmark");

        private ObservableCollection<Bookmark> _bookmarks;
        public ObservableCollection<Bookmark> Bookmarks
        {
            get => _bookmarks;
            set
            {
                if (_bookmarks != value)
                {
                    _bookmarks = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public bool IsEqual(IViewModel other)
        {
            throw new NotImplementedException();
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
    }
}
