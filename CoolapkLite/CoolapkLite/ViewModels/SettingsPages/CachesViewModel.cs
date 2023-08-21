using CoolapkLite.Common;
using CoolapkLite.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public class CachesViewModel : IViewModel
    {
        public CoreDispatcher Dispatcher { get; }

        private ObservableCollection<StorageFile> images = new ObservableCollection<StorageFile>();
        public ObservableCollection<StorageFile> Images
        {
            get => images;
            private set
            {
                if (images != value)
                {
                    images = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public string Title => ResourceLoader.GetForCurrentView("MainPage").GetString("Setting");

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
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

        public CachesViewModel(CoreDispatcher dispatcher) => Dispatcher = dispatcher;

        public async Task Refresh(bool reset)
        {
            UIHelper.ShowProgressBar();
            await ThreadSwitcher.ResumeBackgroundAsync();
            try
            {
                StorageFolder ImageCache = await ApplicationData.Current.TemporaryFolder.GetFolderAsync("ImageCache");
                if (ImageCache != null)
                {
                    IReadOnlyList<StorageFile> images = await ImageCache.GetFilesAsync();
                    Images = new ObservableCollection<StorageFile>(images);
                }
            }
            catch (Exception ex)
            {
                UIHelper.ShowMessage(ex.ExceptionToMessage());
            }
            UIHelper.HideProgressBar();
        }

        bool IViewModel.IsEqual(IViewModel other) => other is CachesViewModel model && IsEqual(model);

        public bool IsEqual(CachesViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;
        
        public async Task RemoveImage(StorageFile file)
        {
            await file.DeleteAsync();
            Images.Remove(file);
        }
    }
}
