using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.compat.collections
{
    public class ArrayDictionary<TK, TV> : IDictionary<TK, TV>
    {
        private KeyValuePair<TK, TV>[] _keyValuePairs;

        public ArrayDictionary(params KeyValuePair<TK, TV>[] values)
        {
            _keyValuePairs = values;
        }

        public TV this[TK key]
        {
            get
            {
                for (int ii = 0; ii < _keyValuePairs.Length; ii++)
                {
                    if (Equals(key, _keyValuePairs[ii].Key))
                    {
                        return _keyValuePairs[ii].Value;
                    }
                }

                throw new KeyNotFoundException();
            }
            set => throw new NotSupportedException();
        }

        public ICollection<TK> Keys => _keyValuePairs.Select(kv => kv.Key).ToArray();

        public ICollection<TV> Values => _keyValuePairs.Select(kv => kv.Value).ToArray();

        public int Count => _keyValuePairs.Length;

        public bool IsReadOnly => true;

        public void Add(TK key, TV value)
        {
            throw new NotSupportedException();
        }

        public void Add(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<TK, TV> item)
        {
            for (int ii = 0; ii < _keyValuePairs.Length; ii++)
            {
                if (Equals(item, _keyValuePairs[ii]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsKey(TK key)
        {
            for (int ii = 0; ii < _keyValuePairs.Length; ii++)
            {
                if (Equals(key, _keyValuePairs[ii].Key))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
        {
            for (int ii = 0; ii < _keyValuePairs.Length; ii++)
            {
                yield return _keyValuePairs[ii];
            }
        }

        public bool Remove(TK key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TK, TV> item)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TK key, out TV value)
        {
            for (int ii = 0; ii < _keyValuePairs.Length; ii++)
            {
                if (Equals(key, _keyValuePairs[ii].Key))
                {
                    value = _keyValuePairs[ii].Value;
                    return true;
                }
            }

            value = default(TV);
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int ii = 0; ii < _keyValuePairs.Length; ii++)
            {
                yield return _keyValuePairs[ii];
            }
        }
    }
}
