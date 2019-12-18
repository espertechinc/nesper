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

namespace com.espertech.esper.compat.collections
{
    [Serializable]
    public class EmptyList<T> : IList<T>
    {
        public static readonly EmptyList<T> Instance = new EmptyList<T>(); 

        private static readonly IList<T> EmptyItem = new T[0];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return EmptyItem.GetEnumerator();
        }

        public void Clear()
        {
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        }

        public bool Remove(T item)
        {
            throw new UnsupportedOperationException();
        }

        public int Count => 0;

        public bool IsReadOnly => true;

        public void Add(T item)
        {
            throw new UnsupportedOperationException();
        }

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new UnsupportedOperationException();
        }

        public void RemoveAt(int index)
        {
            throw new UnsupportedOperationException();
        }

        public T this[int index]
        {
            get => throw new UnsupportedOperationException();
            set { throw new UnsupportedOperationException(); }
        }
    }
}
