///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class MagicDictionary<TK1,TV1> : IDictionary<object, object>
    {
        private readonly IDictionary<TK1, TV1> _realDictionary;
        private readonly GenericTypeCaster<TK1> _typeKeyCaster;

        public MagicDictionary(Object opaqueDictionary, GenericTypeCaster<TK1> typeKeyCaster)
        {
            _realDictionary = (IDictionary<TK1, TV1>)opaqueDictionary;
            _typeKeyCaster = typeKeyCaster;
        }

        public MagicDictionary(IDictionary<TK1, TV1> realDictionary, GenericTypeCaster<TK1> typeKeyCaster)
        {
            _realDictionary = realDictionary;
            _typeKeyCaster = typeKeyCaster;
        }

        public GenericTypeCaster<TK1> TypeKeyCaster => _typeKeyCaster;

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
            _realDictionary.Add(new KeyValuePair<TK1, TV1>((TK1) item.Key, (TV1)item.Value));
        }

        public void Clear()
        {
            _realDictionary.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return _realDictionary.Contains(new KeyValuePair<TK1, TV1>((TK1) item.Key, (TV1)item.Value));
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            return _realDictionary.Remove(new KeyValuePair<TK1, TV1>((TK1) item.Key, (TV1)item.Value));
        }

        public int Count => _realDictionary.Count;

        public bool IsReadOnly => _realDictionary.IsReadOnly;

        public bool ContainsKey(object key)
        {
            if (key is TK1 k1)
            {
                return _realDictionary.ContainsKey(k1);
            }

            return _realDictionary.ContainsKey(TypeKeyCaster.Invoke(key));
        }

        public void Add(object key, object value)
        {
            _realDictionary.Add((TK1) key, (TV1) value);
        }

        public bool Remove(object key)
        {
            return _realDictionary.Remove((TK1) key);
        }

        public bool TryGetValue(object key, out object value)
        {
            if (_realDictionary.TryGetValue((TK1) key, out var item)) {
                value = item;
                return true;
            }

            value = null;
            return false;
        }

        public object this[object key]
        {
            get => _realDictionary[(TK1) key];
            set => _realDictionary[(TK1) key] = (TV1) value;
        }

        public ICollection<object> Keys => new MagicCollection<TK1>(
            _realDictionary.Keys,
            v => throw new NotSupportedException());

        public ICollection<object> Values => new MagicCollection<TV1>(
            _realDictionary.Values,
            v => throw new NotSupportedException());
    }
}
