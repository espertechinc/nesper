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
using System.Linq;

namespace com.espertech.esper.compat.magic
{
    public class MagicCollection<V> : ICollection<Object>
    {
        private readonly ICollection<V> _realCollection;
        private GenericTypeCaster<V> _typeCaster;

        public MagicCollection(object opaqueCollection)
        {
            _realCollection = (ICollection<V>) opaqueCollection;
            _typeCaster = null;
        }

        public MagicCollection(ICollection<V> realCollection)
        {
            _realCollection = realCollection;
        }

        public GenericTypeCaster<V> TypeCaster
        {
            get
            {
                if (_typeCaster == null)
                    _typeCaster = CastHelper.GetCastConverter<V>();
                return _typeCaster;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _realCollection.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            foreach (V item in _realCollection )
                yield return item;
        }

        public void Add(object item)
        {
            _realCollection.Add((V)item);
        }

        public void Clear()
        {
            _realCollection.Clear();
        }

        public bool Contains(object item)
        {
            if (item is V) {
                return _realCollection.Contains((V) item);
            } else {
                return _realCollection.Contains(TypeCaster.Invoke(item));
            }
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            foreach(var item in this)
            {
                if (arrayIndex >= array.Length)
                {
                    break;
                }

                array[arrayIndex++] = item;
            }
        }

        public bool Remove(object item)
        {
            if (item is V) {
                return _realCollection.Remove((V) item);
            } else {
                return _realCollection.Remove(TypeCaster.Invoke(item));
            }
        }

        public int Count
        {
            get { return _realCollection.Count(); }
        }

        public bool IsReadOnly
        {
            get { return _realCollection.IsReadOnly; }
        }
    }
}
