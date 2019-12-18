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
using System.Linq;

namespace com.espertech.esper.compat.collections
{
    [Serializable]
    public class EmptySet<T> : ISet<T>
    {
        public static readonly EmptySet<T> Instance = new EmptySet<T>(); 

        private static readonly T[] EmptyItem = new T[0];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return EmptyItem.Cast<T>().GetEnumerator();
        }

        public void Clear()
        {
            throw new UnsupportedOperationException();
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new UnsupportedOperationException();
        }

        public bool Remove(T item)
        {
            throw new UnsupportedOperationException();
        }

        public int Count => 0;

        public bool IsReadOnly => true;

        public void UnionWith(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool Add(T item)
        {
            throw new UnsupportedOperationException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new UnsupportedOperationException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new UnsupportedOperationException();
        }
    }
}
