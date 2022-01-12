using CoolapkLite.Core.Models;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CoolapkLite.Models.Pages
{
    internal abstract class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListDetailBase(JObject o) : base(o)
        {
            EntityFixed = true;
        }
    }

    internal class UserDetail : FeedListDetailBase
    {
        public string UserName { get; private set; }

        internal UserDetail(JObject o) : base(o)
        {
            if (o.TryGetValue("username", out JToken username))
            {
                UserName = username.ToString();
            }
        }
    }
}
