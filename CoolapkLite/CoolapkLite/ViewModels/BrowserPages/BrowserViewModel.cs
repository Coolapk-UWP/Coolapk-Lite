using CoolapkLite.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels.BrowserPages
{
    public sealed class BrowserViewModel : ViewModelBase
    {
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");

        public bool IsChangeBrowserUA { get; } = SettingsHelper.Get<bool>(SettingsHelper.IsChangeBrowserUA);

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

        public BrowserViewModel(string url, CoreDispatcher dispatcher) : base(dispatcher)
        {
            if (!url.Contains("://")) { url = $"https://{url}"; }
            Uri = url.TryGetUri();
            IsLoginPage = url == UriHelper.LoginUri;
            Title = _loader.GetString("Title");
        }

        public bool IsEqual(BrowserViewModel other) => Uri == other.Uri;
    }
}
