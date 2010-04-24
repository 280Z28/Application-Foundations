namespace Tvl.UI.ViewModel
{
    using System.Collections.Specialized;
    using System.Linq;
    using ICollection = System.Collections.ICollection;

    public static class ConcatCollection
    {
        public static ConcatCollection<T> Create<T>(params ICollection[] collections)
        {
            if (collections.OfType<INotifyCollectionChanged>().Any())
                return new ObservableConcatCollection<T>(collections);

            return new ConcatCollection<T>(collections);
        }
    }
}
