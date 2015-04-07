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
    public class MagicList<V> : IList<Object>
    {
        private readonly IList<V> realList;
        private GenericTypeCaster<V> typeCaster;

        public MagicList(object opaqueCollection)
        {
            this.realList = (IList<V>)opaqueCollection;
            this.typeCaster = null;
        }

        public MagicList(IList<V> realCollection)
        {
            this.realList = realCollection;
        }

        public GenericTypeCaster<V> TypeCaster
        {
            get
            {
                if (typeCaster == null)
                    typeCaster = CastHelper.GetCastConverter<V>();
                return typeCaster;
            }
        }

        #region Implementation of IList<object>

        public int IndexOf(object item)
        {
            if (item is V)
            {
                return realList.IndexOf((V)item);
            }
            else
            {
                return realList.IndexOf(typeCaster.Invoke(item));
            }
        }

        public void Insert(int index, object item)
        {
            if (item is V)
            {
                realList.Insert(index, (V) item);
            }
            else
            {
                realList.Insert(index, typeCaster.Invoke(item));
            }
        }

        public void RemoveAt(int index)
        {
            realList.RemoveAt(index);
        }

        public object this[int index]
        {
            get { return realList[index]; }
            set
            {
                if (value is V) {
                    realList[index] = (V) value;
                }
                else {
                    realList[index] = TypeCaster.Invoke(value);
                }
            }
        }

        #endregion

        public IEnumerator GetEnumerator()
        {
            return realList.GetEnumerator();
        }

        IEnumerator<Object> IEnumerable<Object>.GetEnumerator()
        {
            foreach (V item in realList )
                yield return item;
        }

        public void Add(object item)
        {
            realList.Add((V)item);
        }

        public void Clear()
        {
            realList.Clear();
        }

        public bool Contains(object item)
        {
            if (item is V) {
                return realList.Contains((V) item);
            } else {
                return realList.Contains(TypeCaster.Invoke(item));
            }
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(object item)
        {
            if (item is V) {
                return realList.Remove((V) item);
            } else {
                return realList.Remove(TypeCaster.Invoke(item));
            }
        }

        public int Count
        {
            get { return realList.Count(); }
        }

        public bool IsReadOnly
        {
            get { return realList.IsReadOnly; }
        }


    }
}
