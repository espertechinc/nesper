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
    public class Singleton<T> : ISet<T>
    {
        private readonly T _item;

        public Singleton(T item)
        {
            _item = item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return _item;
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            return Equals(item, _item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array[arrayIndex] = _item;
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => 1;

        public bool IsReadOnly => true;

        public bool Add(T item)
        {
            throw new NotSupportedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }
    }
}