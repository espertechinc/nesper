///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.collection
{
    public class ArrayWrap<T> : IList<T>
    {
        private T[] _handles;

        public ArrayWrap(int currentSize)
        {
            _handles = new T[currentSize];
        }

        public void Expand(int size)
        {
            var newSize = _handles.Length + size;
            var newHandles = new T[newSize];
            System.Array.Copy(_handles, 0, newHandles, 0, _handles.Length);
            _handles = newHandles;
        }

        public T this[int index]
        {
            get { return _handles[index]; }
            set { _handles[index] = value; }
        }

        public T[] Array
        {
            get { return _handles; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>) _handles).GetEnumerator();
        }

        public void Add(T item)
        {
            ((IList<T>) _handles).Add(item);
        }

        public void Clear()
        {
            ((IList<T>) _handles).Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)_handles).Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ((IList<T>) _handles).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((IList<T>)_handles).Remove(item);
        }

        public int Count
        {
            get { return ((IList<T>) _handles).Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList<T>)_handles).IsReadOnly; }
        }

        public int IndexOf(T item)
        {
            return ((IList<T>) _handles).IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            ((IList<T>)_handles).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)_handles).RemoveAt(index);
        }
    }
}
