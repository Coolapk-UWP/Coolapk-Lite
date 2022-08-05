using CoolapkLite.Controls;
using CoolapkLite.Models.Images;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoolapkLite.ViewModels
{
    public class ShowImageViewModel : INotifyPropertyChanged, IViewModel
    {
        public double[] VerticalOffsets { get; set; } = new double[1];

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int index;
        public int Index
        {
            get => index;
            set
            {
                index = value;
                Title = GetTitle(Images[value].Uri);
                RaisePropertyChangedEvent();
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

        private string GetTitle(string url)
        {
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            return $"{(regex.IsMatch(url) ? regex.Match(url).Value : "查看图片")} ({Index + 1}/{Images.Count})";
        }
    }
}
