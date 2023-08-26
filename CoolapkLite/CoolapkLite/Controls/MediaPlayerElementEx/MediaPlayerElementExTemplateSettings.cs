using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using CoolapkLite.Models.Images;

namespace CoolapkLite.Controls
{
    public class MediaPlayerElementExTemplateSettings : DependencyObject
    {
        #region PosterSource

        /// <summary>
        /// Identifies the <see cref="PosterSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PosterSourceProperty =
            DependencyProperty.Register(
                nameof(PosterSource),
                typeof(ImageModel),
                typeof(MediaPlayerElementExTemplateSettings),
                null);

        public ImageModel PosterSource
        {
            get => (ImageModel)GetValue(PosterSourceProperty);
            set => SetValue(PosterSourceProperty, value);
        }

        #endregion
    }
}
