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
using System.IO;
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
}
