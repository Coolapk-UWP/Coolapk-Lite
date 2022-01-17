using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Models
{
    internal interface IPic
    {
        string Uri { get; }
        ImageType Type { get; }
        BitmapImage Pic { get; }
    }

    internal interface IList
    {
        string Url { get; }
        string Title { get; }
        string Description { get; }
        ImageModelWithColor Pic { get; }
    }

    internal interface IHasUriAndTitle
    {
        string Url { get; }
        string Title { get; }
    }

    internal interface IListWithSubtitle
    {
        string Url { get; }
        string Title { get; }
        ImageModel Pic { get; }
        string SubTitle { get; }
        string Description { get; }
    }

    internal interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }

    internal interface ICanChangeReplyNum
    {
        int ID { get; }
        int ReplyNum { get; set; }
    }

    internal interface ICanChangeLikeModel
    {
        int ID { get; }
        bool Liked { get; set; }
        int LikeNum { get; set; }
    }

    internal interface ICanChangeStarModel
    {
        int ID { get; }
        bool Stared { get; set; }
        int StarNum { get; set; }
    }
}
