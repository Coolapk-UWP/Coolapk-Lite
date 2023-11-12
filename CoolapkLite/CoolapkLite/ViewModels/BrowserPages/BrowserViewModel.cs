using CoolapkLite.Common;
using CoolapkLite.Helpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.BrowserPages
{
    public class BrowserViewModel : IViewModel
    {
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");

        public CoreDispatcher Dispatcher { get; } = UIHelper.TryGetForCurrentCoreDispatcher();

        public bool IsChangeBrowserUA => SettingsHelper.Get<bool>(SettingsHelper.IsChangeBrowserUA);

        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private Uri uri;
        public Uri Uri
        {
            get => uri;
            set => SetProperty(ref uri, value);
        }

        private bool isLoginPage;
        public bool IsLoginPage
        {
            get => isLoginPage;
            set => SetProperty(ref isLoginPage, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
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

        public BrowserViewModel(string url, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            if (!url.Contains("://")) { url = $"https://{url}"; }
            Uri = url.TryGetUri();
            IsLoginPage = url == UriHelper.LoginUri;
            Title = _loader.GetString("Title");
        }

        public Task Refresh(bool reset) => Task.CompletedTask;

        bool IViewModel.IsEqual(IViewModel other) => other is BrowserViewModel model && IsEqual(model);
        public bool IsEqual(BrowserViewModel other) => Uri == other.Uri;
    }
}
