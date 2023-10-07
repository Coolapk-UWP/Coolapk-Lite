using CoolapkLite.Common;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.Models.Users;
using CoolapkLite.ViewModels.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.ToolsPages
{
    public class FansAnalyzeViewModel : IViewModel
    {
        private readonly CoolapkListProvider Provider;

        public string ID { get; }
        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        public string CachedSortedColumn { get; set; }
        public List<ContactModel> ContactModels { get; set; } = new List<ContactModel>();

        private string title = "粉丝分析";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private List<Point> orderedPointList = new List<Point>();
        public List<Point> OrderedPointList
        {
            get => orderedPointList;
            set => SetProperty(ref orderedPointList, value);
        }

        private ObservableCollection<ContactModel> filteredContactModel = new ObservableCollection<ContactModel>();
        public ObservableCollection<ContactModel> FilteredContactModel
        {
            get => filteredContactModel;
            set => SetProperty(ref filteredContactModel, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
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

        public FansAnalyzeViewModel(string uid, CoreDispatcher dispatcher)
        {
            ID = uid;
            Dispatcher = dispatcher;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetUserList,
                    "fansList",
                    uid,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                o => new ContactModel(o).AsEnumerable(),
                "fuid");
        }

        bool IViewModel.IsEqual(IViewModel other) => other is FansAnalyzeViewModel model && IsEqual(model);

        public bool IsEqual(FansAnalyzeViewModel other) => !string.IsNullOrWhiteSpace(ID) ? ID == other.ID : Provider == other.Provider;

        public async Task Refresh(bool reset)
        {
            Dispatcher.ShowProgressBar();
            if (reset)
            {
                OrderedPointList.Clear();
                Title = await NetworkHelper.GetUserInfoByNameAsync(ID).ContinueWith(x => $"{x.Result.UserName}的粉丝分析");
                await GetContactModelsAsync();
                await GetOrderedPointListAsync();
            }
            FilteredContactModel = new ObservableCollection<ContactModel>(ContactModels);
            Dispatcher.HideProgressBar();
        }

        private async Task GetContactModelsAsync()
        {
            int page = 1;
            ContactModels.Clear();
            while (true)
            {
                int temp = ContactModels.Count;
                await Provider.GetEntityAsync(ContactModels, page);
                if (ContactModels.Count <= 0 || ContactModels.Count <= temp) { break; }
                page++;
            }
        }

        private async Task GetOrderedPointListAsync()
        {
            await ThreadSwitcher.ResumeBackgroundAsync();
            OrderedPointList.Clear();
            IOrderedEnumerable<ContactModel> FanListByDate = ContactModels.OrderBy(item => item.DateLine);
            int temp = FanListByDate.FirstOrDefault().DateLine, num = 0;
            foreach (ContactModel contact in FanListByDate)
            {
                if (temp != contact.DateLine)
                {
                    OrderedPointList.Add(new Point { X = temp, Y = num });
                    temp = contact.DateLine;
                }
                num++;
            }
            OrderedPointList.Add(new Point { X = temp, Y = num });
        }

        public async Task SortDataAsync(string sortBy, bool ascending)
        {
            try
            {
                Dispatcher.ShowProgressBar();
                await ThreadSwitcher.ResumeBackgroundAsync();
                CachedSortedColumn = sortBy;
                switch (sortBy)
                {
                    case "IsFriend":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.IsFriend))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.IsFriend));
                        break;
                    case "UID":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.UserInfo.UID))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.UserInfo.UID));
                        break;
                    case "UserName":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.UserInfo.UserName))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.UserInfo.UserName));
                        break;
                    case "Level":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.UserInfo.Experience))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.UserInfo.Experience));
                        break;
                    case "FansNum":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => (item.UserInfo as IUserModel).FansNum))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => (item.UserInfo as IUserModel).FansNum));
                        break;
                    case "FollowNum":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => (item.UserInfo as IUserModel).FollowNum))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => (item.UserInfo as IUserModel).FollowNum));
                        break;
                    case "DateLine":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.DateLine))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.DateLine));
                        break;
                    case "LoginTime":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.UserInfo.LoginTime))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.UserInfo.LoginTime));
                        break;
                    case "RegDate":
                        FilteredContactModel = ascending
                            ? new ObservableCollection<ContactModel>(filteredContactModel.OrderBy(item => item.UserInfo.RegDate))
                            : new ObservableCollection<ContactModel>(filteredContactModel.OrderByDescending(item => item.UserInfo.RegDate));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.GetLogger(nameof(FansAnalyzeViewModel)).Error(ex.ExceptionToMessage());
            }
            finally
            {
                Dispatcher.HideProgressBar();
            }
        }
    }
}
