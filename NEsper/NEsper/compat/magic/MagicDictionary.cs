///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    public class MagicDictionary<K,V> : IDictionary<object, object>
    {
        private readonly IDictionary<K, V> _realDictionary;
        private GenericTypeCaster<K> _typeKeyCaster;

        public MagicDictionary(Object opaqueDictionary)
        {
            _realDictionary = (IDictionary<K, V>)opaqueDictionary;
            _typeKeyCaster = null;
        }

        public MagicDictionary(IDictionary<K, V> realDictionary)
        {
            _realDictionary = realDictionary;
        }

        public GenericTypeCaster<K> TypeKeyCaster
        {
            get
            {
                if (_typeKeyCaster == null)
                    _typeKeyCaster = CastHelper.GetCastConverter<K>();
                return _typeKeyCaster;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            foreach( var entry in _realDictionary ) {
                yield return new KeyValuePair<object, object>(entry.Key, entry.Value);
            }
        }

        public void Add(KeyValuePair<object, object> item)
        {
            _realDictionary.Add(new KeyValuePair<K, V>((K) item.Key, (V)item.Value));
        }

        public void Clear()
        {
            _realDictionary.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return _realDictionary.Contains(new KeyValuePair<K, V>((K) item.Key, (V)item.Value));
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            return _realDictionary.Remove(new KeyValuePair<K, V>((K) item.Key, (V)item.Value));
        }

        public int Count
        {
            get { return _realDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return _realDictionary.IsReadOnly; }
        }

        public bool ContainsKey(object key)
        {
            if (key is K)
            {
                return _realDictionary.ContainsKey((K) key);
            }
            else
            {
                return _realDictionary.ContainsKey(TypeKeyCaster.Invoke(key));
            }
        }

        public void Add(object key, object value)
        {
            _realDictionary.Add((K) key, (V) value);
        }

        public bool Remove(object key)
        {
            return _realDictionary.Remove((K) key);
        }

        public bool TryGetValue(object key, out object value)
        {
            V item;
            if (_realDictionary.TryGetValue((K) key, out item)) {
                value = item;
                return true;
            }

            value = null;
            return false;
        }

        public object this[object key]
        {
            get { return _realDictionary[(K) key]; }
            set { _realDictionary[(K) key] = (V) value; }
        }

        public ICollection<object> Keys
        {
            get { return new MagicCollection<K>(_realDictionary.Keys); }
        }

        public ICollection<object> Values
        {
            get { return new MagicCollection<V>(_realDictionary.Values); }
        }
    }
}
