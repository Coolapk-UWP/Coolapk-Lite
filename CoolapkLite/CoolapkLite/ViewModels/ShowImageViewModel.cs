using CoolapkLite.Models.Images;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels
{
    public class ShowImageViewModel : INotifyPropertyChanged, IViewModel
    {
        public string Title { get; protected set; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        private double index;
        public double Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    index = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private IList<ImageModel> images;
        public IList<ImageModel> Images
        {
            get => images;
            set
            {
                if (images != value)
                {
                    images = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ShowImageViewModel(ImageModel image)
        {
            Images = image.ContextArray;
            Index = image.ContextArray.IndexOf(image);
        }

        public Task Refresh(bool reset = false)
        {
            throw new NotImplementedException();
        }
    }
}
