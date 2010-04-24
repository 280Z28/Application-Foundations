namespace Tvl.UI.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using ICollection = System.Collections.ICollection;
    using IList = System.Collections.IList;

    public class ObservableConcatCollection<T> : ConcatCollection<T>, IList<T>, IList, INotifyCollectionChanged
    {
        public ObservableConcatCollection(params ICollection[] collections)
            : base(Array.ConvertAll<ICollection, ICollection>(collections, SnapshotIfNotObservable))
        {
            foreach (var observable in base.Sources.OfType<INotifyCollectionChanged>())
                observable.CollectionChanged += HandleChildCollectionChanged;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public T this[int index]
        {
            get
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");

                foreach (var source in Sources)
                {
                    if (index < source.Count)
                    {
                        return source.Cast<T>().ElementAt(index);
                    }

                    index -= source.Count;
                }

                throw new ArgumentOutOfRangeException("index");
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public int IndexOf(T item)
        {
            int offset = 0;
            int index = 0;
            foreach (var source in Sources)
            {
                IList list = source as IList;
                if (list != null)
                    index = list.IndexOf(item);
                else
                    index = source.Cast<T>().TakeWhile(i => !EqualityComparer<T>.Default.Equals(i, item)).Count();

                if (index >= 0 && index < source.Count)
                    return index + offset;

                offset += source.Count;
            }

            return -1;
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var t = CollectionChanged;
            if (t != null)
                t(this, e);
        }

        private void HandleChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ICollection sendingCollection = sender as ICollection;
            if (sendingCollection == null || e == null)
                return;

            int sendingSourceIndex = Sources.IndexOf(sendingCollection);
            if (sendingSourceIndex < 0)
                return;

            int offset = Sources.Take(sendingSourceIndex).Sum(i => i.Count);

            NotifyCollectionChangedEventArgs args = null;
            switch (e.Action)
            {
            case NotifyCollectionChangedAction.Add:
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, e.OldStartingIndex + offset);
                break;

            case NotifyCollectionChangedAction.Move:
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.OldItems, e.NewStartingIndex, e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, e.OldItems, e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Replace:
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, e.OldStartingIndex);
                break;

            case NotifyCollectionChangedAction.Reset:
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                break;

            default:
                return;
            }

            OnCollectionChanged(args);
        }

        private static ICollection SnapshotIfNotObservable(ICollection collection)
        {
            if (!(collection is INotifyCollectionChanged))
                return collection.Cast<object>().ToArray();

            return collection;
        }

        #region IList Members

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            if (value != null && !(value is T))
                return false;

            value = value ?? default(T);
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            if (value != null && !(value is T))
                return -1;

            value = value ?? default(T);
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }
}
