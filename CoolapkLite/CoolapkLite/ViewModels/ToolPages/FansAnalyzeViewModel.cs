using CoolapkLite.Core.Exceptions;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models.Users;
using LiteDB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CoolapkLite.ViewModels.ToolPages
{
    internal partial class FansAnalyzeViewModel : IViewModel, INotifyPropertyChanged
    {
        public delegate void LoadMoreStarted();
        public delegate void LoadMoreCompleted();
        public delegate void LoadMoreProgressChanged(double value);

        public event LoadMoreStarted OnLoadMoreStarted;
        public event LoadMoreCompleted OnLoadMoreCompleted;
        public event LoadMoreProgressChanged OnLoadMoreProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        private void InvokeProgressChanged<T>(T item, IList<T> items) => OnLoadMoreProgressChanged?.Invoke((double)(items.IndexOf(item) + 1) / items.Count);

        internal FansAnalyzeViewModel(string id)
        {
            _id = id;
            LoadFanList();
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFollows, "fansList", _id, p),
                GetEntities,
                "id");
        }
    }

    internal partial class FansAnalyzeViewModel : IViewModel, INotifyPropertyChanged
    {
        public string Title { get; protected set; }
        public ObservableCollection<Entity> FanList { get; set; }
        public double[] VerticalOffsets { get; set; } = new double[1];

        private readonly string _id;
        private readonly CoolapkListProvider Provider;
        private readonly string LiteDBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "FanLists.db");

        private IEnumerable<Entity> GetEntities(JObject jo)
        {
            yield return jo.Value<string>("entityType") == "contacts" ? new ContactModel(jo) : null;
        }

        public async Task Refresh(int p)
        {
            await GetFanList();
        }

        private async Task GetFanList()
        {
            OnLoadMoreStarted?.Invoke();
            int page = 1;
            FanList.Clear();
            while (true)
            {
                int temp = FanList.Count;
                await Provider.GetEntity(FanList, page);
                if (FanList.Count <= 0 || FanList.Count <= temp) { break; }
                page++;
            }
            using (LiteDatabase db = new LiteDatabase(LiteDBPath))
            {
                try
                {
                    ILiteCollection<Entity> DateBaseResults = db.GetCollection<Entity>(NumToLetter(_id));
                    foreach (Entity entity in FanList)
                    {
                        _ = DateBaseResults.Upsert(entity.EntityId, entity);
                        InvokeProgressChanged(entity, FanList);
                    }
                }
                catch (IOException e)
                {
                    throw new CoolapkMessageException(e.Message);
                }
            }
            OrderFanList();
            SortFanListByLevel();
            OnLoadMoreCompleted?.Invoke();
        }

        private void LoadFanList()
        {
            OnLoadMoreStarted?.Invoke();
            using (LiteDatabase db = new LiteDatabase(LiteDBPath))
            {
                try
                {
                    ILiteCollection<Entity> DateBaseResults = db.GetCollection<Entity>(NumToLetter(_id));
                    FanList = new ObservableCollection<Entity>(DateBaseResults.FindAll());
                }
                catch (IOException e)
                {
                    throw new CoolapkMessageException(e.Message);
                }
                catch (LiteException)
                {
                    FanList = new ObservableCollection<Entity>();
                }
            }
            OnLoadMoreCompleted?.Invoke();
        }

        private string NumToLetter(string nums)
        {
            string letter = string.Empty;
            foreach (char c in nums)
            {
                int number = Convert.ToInt32(c.ToString());
                if (0 <= number && 35 >= number)
                {
                    int num = number + 97;
                    ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                    byte[] btNumber = new byte[] { (byte)num };
                    letter += asciiEncoding.GetString(btNumber);
                }
            }
            return letter;
        }
    }

    internal partial class FansAnalyzeViewModel : IViewModel, INotifyPropertyChanged
    {
        public List<DateData> FanNumListByDate { get; set; }

        private string _dateLabel = "长按选择";
        public string DateLabel
        {
            get => _dateLabel;
            set
            {
                if (_dateLabel != value)
                {
                    _dateLabel = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private double _fansValue;
        public double FansValue
        {
            get => _fansValue;
            set
            {
                if (_fansValue != value)
                {
                    _fansValue = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        //private ChartDataContext fanNumListByDateTrack;
        //public ChartDataContext FanNumListByDateTrack
        //{
        //    get => fanNumListByDateTrack;
        //    set
        //    {
        //        if (fanNumListByDateTrack != value)
        //        {
        //            fanNumListByDateTrack = value;
        //            RaisePropertyChangedEvent();
        //            FanNumListByDateUpdate(fanNumListByDateTrack);
        //        }
        //    }
        //}

        private void OrderFanList()
        {
            OnLoadMoreStarted?.Invoke();
            FanNumListByDate = FanNumListByDate ?? new List<DateData>();
            if (FanList.Count > 0) { FanNumListByDate.Clear(); }
            ObservableCollection<Entity> FanListByDate = new ObservableCollection<Entity>(FanList.OrderBy(item => (item as ContactModel).DateLine));
            int temp = (FanListByDate.First() as ContactModel).DateLine, num = 0;
            foreach (ContactModel contact in FanListByDate)
            {
                if (temp != contact.DateLine)
                {
                    FanNumListByDate.Add(new DateData() { Date = Convert.ToDouble(temp).ConvertUnixTimeStampToDateTime(), Value = num });
                    temp = contact.DateLine;
                }
                num++;
                InvokeProgressChanged(contact, FanListByDate);
            }
            FanNumListByDate.Add(new DateData() { Date = Convert.ToDouble(temp).ConvertUnixTimeStampToDateTime(), Value = num });
            OnLoadMoreCompleted?.Invoke();
        }

        //private void FanNumListByDateUpdate(ChartDataContext fanNumListByDateTrack)
        //{
        //    DateData item = fanNumListByDateTrack.ClosestDataPoint.DataPoint.DataItem as DateData;
        //    DateLabel = item.Date.ToString("yyyy.MM.dd");
        //    FansValue = item.Value;
        //}
    }

    internal partial class FansAnalyzeViewModel : IViewModel, INotifyPropertyChanged
    {
        public List<NumData> FanSortListByLevel { get; set; }

        private void SortFanListByLevel()
        {
            OnLoadMoreStarted?.Invoke();
            FanSortListByLevel = FanSortListByLevel ?? new List<NumData>();
            if (FanList.Count > 0) { FanSortListByLevel.Clear(); }
            ObservableCollection<Entity> FanListByLevel = new ObservableCollection<Entity>(FanList.OrderBy(item => (item as ContactModel).UserInfo.Level));
            int temp = (FanListByLevel.First() as ContactModel).UserInfo.Level, num = 0;
            foreach (ContactModel contact in FanListByLevel)
            {
                if (temp != contact.UserInfo.Level)
                {
                    FanSortListByLevel.Add(new NumData() { Num = temp, Value = num });
                    temp = contact.UserInfo.Level;
                    num = 0;
                }
                num++;
                InvokeProgressChanged(contact, FanListByLevel);
            }
            FanSortListByLevel.Add(new NumData() { Num = temp, Value = num });
            OnLoadMoreCompleted?.Invoke();
        }
    }

    public class NumData : INotifyPropertyChanged
    {
        private int _num;
        public int Num
        {
            get => _num;
            set
            {
                if (_num == value)
                {
                    return;
                }
                _num = value;
                RaisePropertyChangedEvent();
            }
        }

        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }
                _value = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }

    public class DateData : INotifyPropertyChanged
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value)
                {
                    return;
                }
                _date = value;
                RaisePropertyChangedEvent();
            }
        }

        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }
                _value = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
