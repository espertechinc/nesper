///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public class SynchronizedSet<T> : SynchronizedCollection<T>, ISet<T>
    {
        private readonly ISet<T> _facadeSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedSet{T}" /> class.
        /// </summary>
        /// <param name="facadeSet">The facade set.</param>
        public SynchronizedSet(ISet<T> facadeSet) : base(facadeSet)
        {
            _facadeSet = facadeSet;
        }

        bool ISet<T>.Add(T item)
        {
            lock (this)
            {
                return _facadeSet.Add(item);
            }
        }

        /// <summary>
        /// Unions with another set.
        /// </summary>
        /// <param name="other">The other.</param>
        public void UnionWith(IEnumerable<T> other)
        {
            lock (this)
            {
                _facadeSet.UnionWith(other);
            }
        }

        /// <summary>
        /// Intersects with another set.
        /// </summary>
        /// <param name="other">The other.</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            lock (this)
            {
                _facadeSet.IntersectWith(other);
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            lock (this)
            {
                _facadeSet.ExceptWith(other);
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            lock (this)
            {
                _facadeSet.SymmetricExceptWith(other);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (this)
            {
                return _facadeSet.IsSubsetOf(other);
            }
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (this)
            {
                return _facadeSet.IsSupersetOf(other);
            }
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (this)
            {
                return _facadeSet.IsProperSupersetOf(other);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (this)
            {
                return _facadeSet.IsProperSubsetOf(other);
            }
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            lock (this)
            {
                return _facadeSet.Overlaps(other);
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            lock (this)
            {
                return _facadeSet.SetEquals(other);
            }
        }
    }
}
