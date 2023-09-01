using CoolapkLite.Models.Images;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace CoolapkLite.ViewModels
{
    public interface IComboBoxChangeSelectedIndex
    {
        List<string> ItemSource { get; }
        int ComboBoxSelectedIndex { get; }
        void SetComboBoxSelectedIndex(int value);
    }

    public interface IToggleChangeSelectedIndex
    {
        bool ToggleIsOn { get; }
    }

    public interface ISharePic
    {
        void CopyPic(ImageModel image);
        void SharePic(ImageModel image);
        void SavePic(ImageModel imageModel);
        Task<DataPackage> GetImageDataPackageAsync(ImageModel image, string title);
        Task GetImageDataPackageAsync(DataPackage dataPackage, ImageModel image, string title);
    }

    public interface IViewModel : INotifyPropertyChanged
    {
        string Title { get; }
        Task Refresh(bool reset);
        bool IsEqual(IViewModel other);
    }
}
