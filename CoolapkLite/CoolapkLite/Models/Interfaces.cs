using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Models
{
    internal interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }

    internal interface ICanChangeReplyNum
    {
        string ReplyNum { get; set; }
    }

    internal interface ICanChangeLikeModel
    {
        string ID { get; }
        bool Liked { get; set; }
        string LikeNum { get; set; }
    }
}
