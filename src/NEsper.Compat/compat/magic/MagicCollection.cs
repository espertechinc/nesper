///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.compat.magic
{
    public class MagicCollection<TV> : ICollection<object>
    {
        private readonly ICollection<TV> _realCollection;
        private readonly GenericTypeCaster<TV> _typeCaster;

        public MagicCollection(object opaqueCollection)
        {
            _realCollection = (ICollection<TV>) opaqueCollection;
            _typeCaster = obj => (TV) obj;
        }

        public MagicCollection(object opaqueCollection, GenericTypeCaster<TV> typeCaster)
        {
            _realCollection = (ICollection<TV>) opaqueCollection;
            _typeCaster = typeCaster;
        }

        public MagicCollection(ICollection<TV> realCollection, GenericTypeCaster<TV> typeCaster)
        {
            _realCollection = realCollection;
            _typeCaster = typeCaster;
        }

        public GenericTypeCaster<TV> TypeCaster => _typeCaster;

        public IEnumerator GetEnumerator()
        {
            return _realCollection.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            foreach (TV item in _realCollection)
            {
                yield return item;
            }
        }

        public void Add(object item)
        {
            _realCollection.Add((TV) item);
        }

        public void Clear()
        {
            _realCollection.Clear();
        }

        public bool Contains(object item)
        {
            if (item is TV v)
            {
                return _realCollection.Contains(v);
            }

            return _realCollection.Contains(TypeCaster.Invoke(item));
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            foreach (var item in this)
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
            if (item is TV v)
            {
                return _realCollection.Remove(v);
            }

            return _realCollection.Remove(TypeCaster.Invoke(item));
        }

        public int Count => _realCollection.Count;

        public bool IsReadOnly => _realCollection.IsReadOnly;
    }
}