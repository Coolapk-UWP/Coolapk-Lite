using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using NetworkHelper = Microsoft.Toolkit.Uwp.Connectivity.NetworkHelper;

namespace CoolapkLite.ViewModels
{
    public class ShowImageViewModel : IViewModel
    {
        private string ImageName => index != -1 && Images?.Count > 0 ? Images[Index].Title : string.Empty;

        public CoreDispatcher Dispatcher { get; }

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

        public ShowImageViewModel(ImageModel image, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
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

        public Task Refresh(bool reset = false) => Images[Index].Refresh(Dispatcher);

        bool IViewModel.IsEqual(IViewModel other) => other is ShowImageViewModel model && IsEqual(model);
        public bool IsEqual(ShowImageViewModel other) => Images == other.Images;
    }
}
