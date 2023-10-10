using CoolapkLite.Helpers;
using CoolapkLite.ViewModels.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.SettingsPages
{
    public class CachesViewModel : DataSourceBase<StorageFile>, IViewModel
    {
        public string Title { get; } = ResourceLoader.GetForViewIndependentUse("MainPage").GetString("Caches");

        private List<StorageFile> Images { get; set; }

        public CachesViewModel(CoreDispatcher dispatcher) => Dispatcher = dispatcher;

        protected override async Task<IList<StorageFile>> LoadItemsAsync(uint count)
        {
            if (_currentPage == 1)
            {
                try
                {
                    StorageFolder ImageCache = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
                    if (ImageCache != null)
                    {
                        IReadOnlyList<StorageFile> images = await ImageCache.GetFilesAsync();
                        Images = images.ToList();
                    }
                }
                catch (Exception ex)
                {
                    _ = Dispatcher.ShowMessageAsync(ex.ExceptionToMessage());
                }
            }

            if (Images.Count > count)
            {
                List<StorageFile> result = Images.GetRange(0, (int)count);
                Images.RemoveRange(0, (int)count);
                return result;
            }
            else
            {
                List<StorageFile> result = Images;
                Images.Clear();
                return result;
            }
        }

        public async Task Refresh(bool reset = false)
        {
            if (_busy) { return; }
            if (reset)
            {
                await Reset().ConfigureAwait(false);
            }
            else if (_hasMoreItems)
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is CachesViewModel model && IsEqual(model);

        public bool IsEqual(CachesViewModel other) => Dispatcher == null ? Equals(other) : Dispatcher == other.Dispatcher;

        public async Task RemoveImageAsync(StorageFile file)
        {
            await file.DeleteAsync();
            Remove(file);
        }
    }
}
