///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.compat.magic
{
    public class MagicStringDictionary<TV> : IDictionary<string, object>
    {
        private readonly IDictionary<string, TV> _realDictionary;

        public MagicStringDictionary(Object opaqueDictionary)
        {
            _realDictionary = (IDictionary<string, TV>) opaqueDictionary;
        }

        public MagicStringDictionary(IDictionary<string, TV> realDictionary)
        {
            _realDictionary = realDictionary;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach( var entry in _realDictionary ) {
                yield return new KeyValuePair<string, object>(entry.Key, entry.Value);
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _realDictionary.Add(new KeyValuePair<string, TV>(item.Key, (TV) item.Value));
        }

        public void Clear()
        {
            _realDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _realDictionary.Contains(new KeyValuePair<string, TV>(item.Key, (TV) item.Value));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _realDictionary.Remove(new KeyValuePair<string, TV>(item.Key, (TV) item.Value));
        }

        public int Count => _realDictionary.Count;

        public bool IsReadOnly => _realDictionary.IsReadOnly;

        public bool ContainsKey(string key)
        {
            return _realDictionary.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _realDictionary.Add(key, (TV) value);
        }

        public bool Remove(string key)
        {
            return _realDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            if (_realDictionary.TryGetValue(key, out var item)) {
                value = item;
                return true;
            }

            value = null;
            return false;
        }

        public object this[string key]
        {
            get => _realDictionary[key];
            set => _realDictionary[key] = (TV) value;
        }

        public ICollection<string> Keys => _realDictionary.Keys;

        public ICollection<object> Values => new MagicCollection<TV>(
            _realDictionary.Values, 
            v => throw new NotSupportedException()); // cast conversion is only used in mutable functions
    }
}
