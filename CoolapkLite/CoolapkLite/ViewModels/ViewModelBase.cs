using CoolapkLite.Common;
using CoolapkLite.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace CoolapkLite.ViewModels
{
    public abstract class ViewModelBase : IViewModel
    {
        public CoreDispatcher Dispatcher { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void PropertyChangedInvoke([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected virtual async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChangedInvoke(name);
            }
        }

        protected void SetProperty<TProperty>(ref TProperty property, TProperty value, [CallerMemberName] string name = null)
        {
            if (!property?.Equals(value) ?? (value != null))
            {
                property = value;
                RaisePropertyChangedEvent(name);
            }
        }

        public ViewModelBase(CoreDispatcher dispatcher) => Dispatcher = dispatcher ?? UIHelper.TryGetForCurrentCoreDispatcher();

        public virtual Task Refresh(bool reset) => Task.CompletedTask;
    }

    public abstract class CachedViewModelBase<TSelf> : ViewModelBase where TSelf : CachedViewModelBase<TSelf>
    {
        public static Dictionary<CoreDispatcher, TSelf> Caches { get; } = new Dictionary<CoreDispatcher, TSelf>();

        protected override async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                foreach (KeyValuePair<CoreDispatcher, TSelf> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    cache.Value.PropertyChangedInvoke(name);
                }
            }
        }

        protected static async void RaisePropertyChangedEvent(params string[] names)
        {
            if (names?.Length > 0)
            {
                foreach (KeyValuePair<CoreDispatcher, TSelf> cache in Caches)
                {
                    await cache.Key.ResumeForegroundAsync();
                    names.ForEach(cache.Value.PropertyChangedInvoke);
                }
            }
        }

        public CachedViewModelBase(CoreDispatcher dispatcher) : base(dispatcher) => Caches[Dispatcher] = this as TSelf;
    }

    public abstract class CachedListViewModelBase<TSelf, TItem> : CachedViewModelBase<TSelf>, IListViewModel<TItem> where TSelf : CachedListViewModelBase<TSelf, TItem>
    {
        private const string IndexerName = "Item[]";
        protected static readonly List<TItem> _items = new List<TItem>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected async void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
        {
            foreach (KeyValuePair<CoreDispatcher, TSelf> cache in Caches)
            {
                await cache.Key.ResumeForegroundAsync();
                cache.Value.PropertyChangedInvoke(IndexerName);
                cache.Value.CollectionChanged?.Invoke(cache.Value, e);
            }
        }

        public CachedListViewModelBase(CoreDispatcher dispatcher) : base(dispatcher) { }

        #region IList<TItem> Members

        public int Count => _items.Count;

        bool ICollection<TItem>.IsReadOnly => ((ICollection<TItem>)_items).IsReadOnly;

        public TItem this[int index]
        {
            get => _items[index];
            set => SetIndex(index, value);
        }

        protected abstract void SetIndex(int index, TItem item);

        protected virtual void Replace(int index, TItem old, TItem item)
        {
            _items[index] = item;
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    item,
                    old,
                    index));
        }

        public virtual ReplaceStatus AddOrReplace(TItem item)
        {
            int index = _items.Count;
            _items.Add(item);
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
            return ReplaceStatus.Added;
        }

        void ICollection<TItem>.Add(TItem item) => _ = AddOrReplace(item);

        protected void AddRange(IEnumerable<TItem> collection)
        {
            int index = _items.Count;
            _items.AddRange(collection);
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    collection is IList list ? list : _items.GetRange(index, _items.Count - index),
                    index));
        }

        public virtual void Insert(int index, TItem item)
        {
            _items.Insert(index, item);
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    item,
                    index));
        }

        public void CopyTo(TItem[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        protected virtual void Remove(int index, TItem item)
        {
            _items.RemoveAt(index);
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    item,
                    index));
        }

        public bool Remove(TItem item)
        {
            int index = _items.IndexOf(item);
            if (index >= 0)
            {
                Remove(index, _items[index]);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            TItem old = _items[index];
            Remove(index, old);
        }

        protected void Clear()
        {
            _items.Clear();
            RaisePropertyChangedEvent(nameof(Count));
            RaiseCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset));
        }

        void ICollection<TItem>.Clear() => Clear();

        public bool Contains(TItem item) => _items.Contains(item);

        public int IndexOf(TItem item) => _items.IndexOf(item);

        public IEnumerator<TItem> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

        #endregion

        #region IList Members

        bool IList.IsFixedSize => ((IList)_items).IsFixedSize;

        bool IList.IsReadOnly => ((IList)_items).IsReadOnly;

        bool ICollection.IsSynchronized => ((ICollection)_items).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

        object IList.this[int index]
        {
            get => ((IList)_items)[index];
            set
            {
                if (value is TItem item)
                {
                    this[index] = item;
                }
                else
                {
                    ThrowWrongValueTypeArgumentException(value, typeof(TItem));
                }
            }
        }

        private static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType) =>
            throw new ArgumentException($"The value \"{value}\" is not of type \"{targetType}\" and cannot be used in this collection.", nameof(value));

        int IList.Add(object value)
        {
            if (value is TItem item)
            {
                _ = AddOrReplace(item);
            }
            else
            {
                ThrowWrongValueTypeArgumentException(value, typeof(TItem));
            }
            return Count - 1;
        }

        void IList.Insert(int index, object value)
        {
            if (value is TItem item)
            {
                Insert(index, item);
            }
            else
            {
                ThrowWrongValueTypeArgumentException(value, typeof(TItem));
            }
        }

        void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

        void IList.Remove(object value)
        {
            if (value is TItem item)
            {
                Remove(item);
            }
        }

        void IList.Clear() => Clear();

        bool IList.Contains(object value) => value is TItem item && Contains(item);

        int IList.IndexOf(object value) => value is TItem item ? IndexOf(item) : -1;

        #endregion
    }

    public enum ReplaceStatus
    {
        Duplicated = -1,
        Replaced,
        Added
    }
}
