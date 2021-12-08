using CoolapkLite.Core.Exceptions;
using CoolapkLite.Core.Helpers;
using CoolapkLite.Core.Models;
using CoolapkLite.Core.Providers;
using CoolapkLite.Models;
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
    internal class FansAnalyzeViewModel : IViewModel
    {
        public string Title { get; protected set; }
        public ObservableCollection<Entity> FanList { get; set; }
        public double[] VerticalOffsets { get; set; } = new double[1];
        public ObservableCollection<ChartValueItem> FanNumListByDate { get; set; }

        public delegate void FanNumListByDateChanged();
        public event FanNumListByDateChanged OnFanNumListByDateChanged;

        private readonly string _id;
        private readonly CoolapkListProvider Provider;
        private readonly string LiteDBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "FanLists.db");

        internal FansAnalyzeViewModel(string id)
        {
            _id = id;
            LoadFanList();
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) => UriHelper.GetUri(UriType.GetUserFollows, "fansList", _id, p),
                GetEntities,
                "id");
        }

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
            int page = 1;
            FanList.Clear();
            while (true)
            {
                int temp = FanList.Count;
                await Provider.GetEntity(FanList, page);
                if (FanList.Count <= 0 || FanList.Count <= temp) { break; }
                page++;
            }
            using (LiteDatabase db = new(LiteDBPath))
            {
                try
                {
                    ILiteCollection<Entity> DateBaseResults = db.GetCollection<Entity>(NumToLetter(_id));
                    foreach (Entity entity in FanList)
                    {
                        _ = DateBaseResults.Upsert(entity.EntityId, entity);
                    }
                }
                catch (IOException e)
                {
                    throw new CoolapkMessageException(e.Message);
                }
            }
            OrderFanList();
        }

        private void LoadFanList()
        {
            using (LiteDatabase db = new(LiteDBPath))
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
        }

        private void OrderFanList()
        {
            FanNumListByDate = FanNumListByDate ?? new ObservableCollection<ChartValueItem>();
            if(FanList.Count > 0) { FanNumListByDate.Clear(); }
            ObservableCollection<Entity> FanListByDate = new ObservableCollection<Entity>(FanList.OrderBy(item => (item as ContactModel).DateLine));
            int temp = (FanListByDate.First() as ContactModel).DateLine, num = 0;
            foreach (ContactModel contact in FanListByDate)
            {
                if (temp != contact.DateLine)
                {
                    FanNumListByDate.Add(new ChartValueItem(temp, num));
                    temp = contact.DateLine;
                }
                num++;
            }
            FanNumListByDate.Add(new ChartValueItem(temp, num));
            OnFanNumListByDateChanged?.Invoke();
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

    internal class ChartValueItem : INotifyPropertyChanged
    {
        private int _valueX;
        public int ValueX
        {
            get
            {
                return _valueX;
            }
            set
            {
                if (_valueX != value)
                {
                    _valueX = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int _valueY;
        public int ValueY
        {
            get
            {
                return _valueY;
            }
            set
            {
                if (_valueY != value)
                {
                    _valueY = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ChartValueItem(int X, int Y)
        {
            ValueX = X;
            ValueY = Y;
        }
    }
}
