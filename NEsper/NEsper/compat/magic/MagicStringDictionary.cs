///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class MagicStringDictionary<V> : IDictionary<string, object>
    {
        private readonly IDictionary<string, V> _realDictionary;

        public MagicStringDictionary(Object opaqueDictionary)
        {
            _realDictionary = (IDictionary<string, V>) opaqueDictionary;
        }

        public MagicStringDictionary(IDictionary<string, V> realDictionary)
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
            _realDictionary.Add(new KeyValuePair<string, V>(item.Key, (V) item.Value));
        }

        public void Clear()
        {
            _realDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _realDictionary.Contains(new KeyValuePair<string, V>(item.Key, (V) item.Value));
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _realDictionary.Remove(new KeyValuePair<string, V>(item.Key, (V) item.Value));
        }

        public int Count
        {
            get { return _realDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _realDictionary.IsReadOnly; }
        }

        public bool ContainsKey(string key)
        {
            return _realDictionary.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _realDictionary.Add(key, (V) value);
        }

        public bool Remove(string key)
        {
            return _realDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            V item;
            if (_realDictionary.TryGetValue(key, out item)) {
                value = item;
                return true;
            }

            value = null;
            return false;
        }

        public object this[string key]
        {
            get { return _realDictionary[key]; }
            set { _realDictionary[key] = (V) value; }
        }

        public ICollection<string> Keys
        {
            get { return _realDictionary.Keys; }
        }

        public ICollection<object> Values
        {
            get { return new MagicCollection<V>(_realDictionary.Values); }
        }
    }
}
