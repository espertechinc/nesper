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

using com.espertech.esper.common.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ArrayWrappingCollection : ICollection<EventBean>
    {
        public ArrayWrappingCollection(Array array)
        {
            if (array == null) {
                throw new ArgumentException("Null array provided");
            }

            if (!array.GetType().IsArray) {
                throw new ArgumentException(
                    "Non-array value provided to collection, expected array type but received type " +
                    array.GetType().Name);
            }

            Array = array;
        }

        public Array Array { get; }

        public int Count => Array.Length;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            for (var ii = 0; ii < Array.Length; ii++) {
                yield return (EventBean) Array.GetValue(ii);
            }
        }

        public void Add(EventBean item)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public bool Remove(EventBean item)
        {
            throw new UnsupportedOperationException("Read-only implementation");
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(EventBean item)
        {
            if (Array == null) {
                return false;
            }

            var len = Array.Length;
            for (var i = 0; i < len; i++) {
                var other = Array.GetValue(i);
                if (other != null && other.Equals(item)) {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(
            EventBean[] array,
            int arrayIndex)
        {
            Array.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly => true;

        public bool IsEmpty()
        {
            return Count == 0;
        }

        public object[] ToArray()
        {
            return (object[]) Array;
        }
    }
} // end of namespace