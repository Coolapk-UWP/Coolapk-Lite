using CoolapkLite.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace CoolapkLite.ViewModels.DataSource
{
    /// <summary>
    /// A incremental loading class base on the data binding sample on
    /// <see cref="MSDN" href="https://code.msdn.microsoft.com/windowsapps/Data-Binding-7b1d67b5/"/>
    /// , but using ObservableCollection to contain data and notify changes. <br/>
    /// If you want to use incremental loading in MVVM pattern, you can use this as a collection,
    /// and add a constructor with a delegate to load data,
    /// so that you can load different data in your view model, refer this blog for detail
    /// <see href="http://blogs.msdn.com/b/devosaure/archive/2012/10/15/isupportincrementalloading-loading-a-subsets-of-data.aspx"/>
    /// </summary>
    public abstract class IncrementalLoadingBase<T> : ObservableCollection<T>, ISupportIncrementalLoading
    {
        #region ISupportIncrementalLoading

        public bool HasMoreItems => HasMoreItemsOverride();

        /// <summary>
        /// Load more items, this is invoked by Controls like ListView.
        /// </summary>
        /// <param name="count">How many new items want to load.</param>
        /// <returns>Item count actually loaded.</returns>
        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (_busy)
            {
                return Task.Run(() => new LoadMoreItemsResult { Count = 0 }).AsAsyncOperation();
            }

            _busy = true;

            // We need to use AsyncInfo.Run to invoke async operation, as this method cannot return a Task.
            return AsyncInfo.Run((c) => LoadMoreItemsAsync(c, count));
        }

        #endregion

        public CoreDispatcher Dispatcher { get; protected set; }

        private bool any = false;
        public bool Any
        {
            get => any;
            set => SetProperty(ref any, value);
        }

        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set => SetProperty(ref isLoading, value);
        }

        protected override event PropertyChangedEventHandler PropertyChanged;

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

        /// <summary>
        /// We use this method to load data and add to self.
        /// </summary>
        /// <param name="c">Cancellation Token</param>
        /// <param name="count">How many want to load.</param>
        /// <returns>Item count actually loaded.</returns>
        protected async Task<LoadMoreItemsResult> LoadMoreItemsAsync(CancellationToken c, uint count)
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();

                // We are going to load more.
                IsLoading = true;
                LoadMoreStarted?.Invoke();

                // Data loading will different for sub-class.
                IList<T> items = await LoadMoreItemsOverrideAsync(c, count);

                await AddItemsAsync(items);

                // We finished loading operation.
                IsLoading = false;
                LoadMoreCompleted?.Invoke();

                return new LoadMoreItemsResult { Count = items == null ? 0 : (uint)items.Count };
            }
            finally
            {
                _busy = false;
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            Any = this.Any();
        }

        public delegate void EventHandler();
        public delegate void EventHandler<TEventArgs>(TEventArgs e);

        public event EventHandler LoadMoreStarted;
        public event EventHandler LoadMoreCompleted;

        #region Overridable methods

        /// <summary>
        /// Append items to list.
        /// </summary>
        protected virtual async Task AddItemsAsync(IList<T> items)
        {
            if (items != null)
            {
                foreach (T item in items)
                {
                    await AddAsync(item).ConfigureAwait(false);
                }
            }
        }

        public virtual async Task AddAsync(T item)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            Add(item);
        }

        public virtual async Task RemoveAsync(T item)
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            Remove(item);
        }

        public virtual async Task ClearAsync()
        {
            if (Dispatcher?.HasThreadAccess == false)
            {
                await Dispatcher.ResumeForegroundAsync();
            }
            Clear();
        }

        protected abstract Task<IList<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count);

        protected abstract bool HasMoreItemsOverride();

        #endregion

        protected bool _busy = false;
    }
}
