namespace Tvl.UI.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using ICollection = System.Collections.ICollection;
    using IEnumerable = System.Collections.IEnumerable;
    using IEnumerator = System.Collections.IEnumerator;

    public class ConcatCollection<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private readonly ICollection[] _collections;

        public ConcatCollection(params ICollection[] collections)
        {
            if (collections == null)
                throw new ArgumentNullException("collections");
            Contract.EndContractBlock();

            this._collections = collections;
        }

        protected IList<ICollection> Sources
        {
            get
            {
                return _collections;
            }
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var source in Sources)
            {
                source.CopyTo(array, arrayIndex);
                arrayIndex += source.Count;
            }
        }

        public int Count
        {
            get
            {
                return Sources.Sum(i => i.Count);
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Sources.SelectMany(i => i.Cast<T>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var source in Sources)
            {
                source.CopyTo(array, index);
                index += source.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}
