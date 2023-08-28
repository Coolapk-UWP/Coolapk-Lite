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
        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        public string Title { get; } = ResourceLoader.GetForCurrentView("MainPage").GetString("Setting");

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

        public async Task Refresh(bool reset)
        {
            Dispatcher.ShowProgressBar();
            await ThreadSwitcher.ResumeBackgroundAsync();
            try
            {
                StorageFolder ImageCache = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
                if (ImageCache != null)
                {
                    IReadOnlyList<StorageFile> images = await ImageCache.GetFilesAsync();
                    Images = new ObservableCollection<StorageFile>(images);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.ShowMessage(ex.ExceptionToMessage());
            }
            Dispatcher.HideProgressBar();
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
