using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using NetworkHelper = Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper;

namespace CoolapkLite.ViewModels
{
    public sealed class ShowImageViewModel : ViewModelBase
    {
        private string ImageName => index != -1 && Images?.Count > 0 ? Images[Index].Title : string.Empty;

        public string Title
        {
            get
            {
                if (index == -1 || !(Images?.Count > 0)) { return "查看图片"; }
                string name = ImageName;
                return $"{(string.IsNullOrWhiteSpace(name) ? "查看图片" : name)} ({Index + 1}/{Images.Count})";
            }
        }

        private IList<ImageModel> images;
        public IList<ImageModel> Images
        {
            get => images;
            private set => SetProperty(ref images, value);
        }

        private int index = -1;
        public int Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    index = value;
                    RaisePropertyChangedEvent();
                    RaisePropertyChangedEvent(nameof(Title));
                }
            }
        }

        public ShowImageViewModel(ImageModel image, CoreDispatcher dispatcher) : base(dispatcher)
        {
            if (image.Dispatcher != dispatcher)
            {
                image = image.Clone(dispatcher);
            }
            if (image.ContextArray.Length > 0)
            {
                Images = image.ContextArray;
                Index = Images.IndexOf(image);
            }
            else
            {
                Images = new[] { image };
                Index = 0;
            }
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                foreach (ImageModel Image in Images)
                {
                    Image.Type &= (ImageType)0xFE;
                }
            }
        }

        public override Task Refresh(bool reset = true) => Images[Index].Refresh(Dispatcher);

        public bool IsEqual(ShowImageViewModel other) => Images == other.Images;
    }
}
