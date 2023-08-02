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
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.ToolPages
{
    public class FansAnalyzeViewModel : IViewModel
    {
        private readonly CoolapkListProvider Provider;

        public string ID { get; }
        public CoreDispatcher Dispatcher { get; }

        public string CachedSortedColumn { get; set; }
        public List<ContactModel> ContactModels { get; set; } = new List<ContactModel>();

        private string title = "粉丝分析";
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }


private ObservableCollection<ContactModel> filteredContactModel = new ObservableCollection<ContactModel>();
        public ObservableCollection<ContactModel> FilteredContactModel
        {
            get => filteredContactModel;
            set
            {
                if (filteredContactModel != value)
                {
                    filteredContactModel = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
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
                (o) => new Entity[] { new ContactModel(o) },
                "fuid");
        }

        public async Task Refresh(bool reset)
        {
            UIHelper.ShowProgressBar();
            if (reset)
            {
                Title = (await NetworkHelper.GetUserInfoByNameAsync(ID)).UserName + "的粉丝分析";
                await GetContactModels();
            }
            FilteredContactModel = new ObservableCollection<ContactModel>(ContactModels);
            UIHelper.HideProgressBar();
        }

        private async Task GetContactModels()
        {
            int page = 1;
            ContactModels.Clear();
            while (true)
            {
                int temp = ContactModels.Count;
                await Provider.GetEntity(ContactModels, page);
                if (ContactModels.Count <= 0 || ContactModels.Count <= temp) { break; }
                page++;
            }
        }

        public async Task SortData(string sortBy, bool ascending)
        {
            try
            {
                UIHelper.ShowProgressBar();
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
                UIHelper.HideProgressBar();
            }
        }

        public bool IsEqual(IViewModel other)
        {
            throw new NotImplementedException();
        }
    }
}
