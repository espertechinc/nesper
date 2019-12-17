///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace com.espertech.esper.compat.collections
{
    public class SynchronizedList<T> : IList<T>
    {
        private readonly IList<T> _subList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedList&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="_subList">The _sub list.</param>
        public SynchronizedList(IList<T> _subList)
        {
            this._subList = _subList;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int IndexOf(T item)
        {
            return _subList.IndexOf(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Insert(int index, T item)
        {
            _subList.Insert(index, item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveAt(int index)
        {
            _subList.RemoveAt(index);
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _subList[index]; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { _subList[index] = value; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Add(T item)
        {
            _subList.Add(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Clear()
        {
            _subList.Clear();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Contains(T item)
        {
            return _subList.Contains(item);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _subList.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Remove(T item)
        {
            return _subList.Remove(item);
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _subList.Count; }
        }

        public bool IsReadOnly
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _subList.IsReadOnly; }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerator<T> GetEnumerator()
        {
            return _subList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
