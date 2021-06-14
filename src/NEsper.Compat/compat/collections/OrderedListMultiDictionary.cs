using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class OrderedListMultiDictionary<TK, TV> : OrderedListDictionary<TK, ICollection<TV>>
    {
        internal OrderedListMultiDictionary(
            List<KeyValuePair<TK, ICollection<TV>>> itemList,
            KeyValuePairComparer comparer) : base(itemList, comparer)
        {
        }

        public OrderedListMultiDictionary(IComparer<TK> keyComparer) : base(keyComparer)
        {
        }

        public OrderedListMultiDictionary()
        {
        }

        public void Put(
            TK key,
            TV value)
        {
            if (!TryGetValue(key, out var valueCollection)) {
                valueCollection = new ArrayDeque<TV>();
                valueCollection.Add(value);
                Add(key, valueCollection);
            }
        }
    }
}