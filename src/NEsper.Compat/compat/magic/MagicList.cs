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
using System.Linq;

namespace com.espertech.esper.compat.magic
{
    public class MagicList<TV> : IList<Object>
    {
        private readonly IList<TV> _realList;
        private readonly GenericTypeCaster<TV> _typeCaster;

        public MagicList(object opaqueCollection, GenericTypeCaster<TV> typeCaster)
        {
            _realList = (IList<TV>) opaqueCollection;
            _typeCaster = null;
        }

        public MagicList(IList<TV> realCollection, GenericTypeCaster<TV> typeCaster)
        {
            _realList = realCollection;
        }

        public GenericTypeCaster<TV> TypeCaster => _typeCaster;

        #region Implementation of IList<object>

        public int IndexOf(object item)
        {
            if (item is TV v)
            {
                return _realList.IndexOf(v);
            }

            return _realList.IndexOf(_typeCaster.Invoke(item));
        }

        public void Insert(int index, object item)
        {
            if (item is TV v)
            {
                _realList.Insert(index, v);
            }
            else
            {
                _realList.Insert(index, _typeCaster.Invoke(item));
            }
        }

        public void RemoveAt(int index)
        {
            _realList.RemoveAt(index);
        }

        public object this[int index]
        {
            get => _realList[index];
            set
            {
                if (value is TV v) {
                    _realList[index] = v;
                }
                else {
                    _realList[index] = TypeCaster.Invoke(value);
                }
            }
        }

        #endregion

        public IEnumerator GetEnumerator()
        {
            return _realList.GetEnumerator();
        }

        IEnumerator<Object> IEnumerable<Object>.GetEnumerator()
        {
            foreach (TV item in _realList )
                yield return item;
        }

        public void Add(object item)
        {
            _realList.Add((TV)item);
        }

        public void Clear()
        {
            _realList.Clear();
        }

        public bool Contains(object item)
        {
            if (item is TV v) {
                return _realList.Contains(v);
            }

            return _realList.Contains(TypeCaster.Invoke(item));
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(object item)
        {
            if (item is TV v) {
                return _realList.Remove(v);
            }

            return _realList.Remove(TypeCaster.Invoke(item));
        }

        public int Count => _realList.Count();

        public bool IsReadOnly => _realList.IsReadOnly;
    }
}
