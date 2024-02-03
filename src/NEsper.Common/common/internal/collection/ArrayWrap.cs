///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.collection
{
    public class ArrayWrap<T> : IList<T>
    {
        public ArrayWrap(int currentSize)
        {
            Array = new T[currentSize];
        }

        public T[] Array { get; private set; }

        public T this[int index] {
            get => Array[index];
            set => Array[index] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)Array).GetEnumerator();
        }

        public void Add(T item)
        {
            ((IList<T>)Array).Add(item);
        }

        public void Clear()
        {
            ((IList<T>)Array).Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)Array).Contains(item);
        }

        public void CopyTo(
            T[] array,
            int arrayIndex)
        {
            ((IList<T>)Array).CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return ((IList<T>)Array).Remove(item);
        }

        public int Count => ((IList<T>)Array).Count;

        public bool IsReadOnly => ((IList<T>)Array).IsReadOnly;

        public int IndexOf(T item)
        {
            return ((IList<T>)Array).IndexOf(item);
        }

        public void Insert(
            int index,
            T item)
        {
            ((IList<T>)Array).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<T>)Array).RemoveAt(index);
        }

        public void Expand(int size)
        {
            var newSize = Array.Length + size;
            var newHandles = new T[newSize];
            System.Array.Copy(Array, 0, newHandles, 0, Array.Length);
            Array = newHandles;
        }
    }
}