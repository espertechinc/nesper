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
    public class MagicDictionary<K1,V1> : IDictionary<object, object>
    {
        private readonly IDictionary<K1, V1> _realDictionary;
        private GenericTypeCaster<K1> _typeKeyCaster;

        public MagicDictionary(Object opaqueDictionary)
        {
            if (!(opaqueDictionary is IDictionary<K1, V1>))
                System.Diagnostics.Debug.WriteLine("stop");
            _realDictionary = (IDictionary<K1, V1>)opaqueDictionary;
            _typeKeyCaster = null;
        }

        public MagicDictionary(IDictionary<K1, V1> realDictionary)
        {
            _realDictionary = realDictionary;
        }

        public GenericTypeCaster<K1> TypeKeyCaster
        {
            get
            {
                if (_typeKeyCaster == null)
                    _typeKeyCaster = CastHelper.GetCastConverter<K1>();
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
            _realDictionary.Add(new KeyValuePair<K1, V1>((K1) item.Key, (V1)item.Value));
        }

        public void Clear()
        {
            _realDictionary.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return _realDictionary.Contains(new KeyValuePair<K1, V1>((K1) item.Key, (V1)item.Value));
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            return _realDictionary.Remove(new KeyValuePair<K1, V1>((K1) item.Key, (V1)item.Value));
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
            if (key is K1)
            {
                return _realDictionary.ContainsKey((K1) key);
            }
            else
            {
                return _realDictionary.ContainsKey(TypeKeyCaster.Invoke(key));
            }
        }

        public void Add(object key, object value)
        {
            _realDictionary.Add((K1) key, (V1) value);
        }

        public bool Remove(object key)
        {
            return _realDictionary.Remove((K1) key);
        }

        public bool TryGetValue(object key, out object value)
        {
            V1 item;
            if (_realDictionary.TryGetValue((K1) key, out item)) {
                value = item;
                return true;
            }

            value = null;
            return false;
        }

        public object this[object key]
        {
            get { return _realDictionary[(K1) key]; }
            set { _realDictionary[(K1) key] = (V1) value; }
        }

        public ICollection<object> Keys
        {
            get { return new MagicCollection<K1>(_realDictionary.Keys); }
        }

        public ICollection<object> Values
        {
            get { return new MagicCollection<V1>(_realDictionary.Values); }
        }
    }
}
