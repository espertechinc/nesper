///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     Sorted, reference-counting set based on an OrderedDictionary implementation that
    ///     stores keys and a reference counter for each unique key value. Each time the same
    ///     key is added, the reference counter increases. Each time a key is removed, the
    ///     reference counter decreases.
    /// </summary>
    public class SortedRefCountedSet<TK>
    {
        private readonly SortedList<TK, AtomicLong> _refSet;
        private long _countPoints;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public SortedRefCountedSet()
        {
            _countPoints = 0;
            _refSet = new SortedList<TK, AtomicLong>();
        }

        /// <summary>
        ///     Gets the number of data points.
        /// </summary>
        public long CountPoints {
            get => _countPoints;
            set => _countPoints = value;
        }

        /// <summary>
        ///     Gets the ref set.
        /// </summary>
        /// <value>The ref set.</value>
        public SortedList<TK, AtomicLong> RefSet => _refSet;

        /// <summary> Returns the largest key value, or null if the collection is empty.</summary>
        /// <returns>
        ///     largest key value, null if none
        /// </returns>

        public virtual TK MaxValue => _refSet.Count != 0 ? _refSet.Keys[RefSet.Count - 1] : default(TK);

        /// <summary> Returns the smallest key value, or null if the collection is empty.</summary>
        /// <returns>
        ///     smallest key value, default(K) if none
        /// </returns>

        public virtual TK MinValue => _refSet.Count != 0 ? _refSet.Keys[0] : default(TK);

        /// <summary>
        ///     Add a key to the set. Add with a reference count of one if the key didn't exist in the set.
        ///     Increase the reference count by one if the key already exists.
        /// </summary>
        /// <param name="key">
        ///     to add
        /// </param>
        public virtual void Add(TK key)
        {
            AtomicLong value;
            if (!_refSet.TryGetValue(key, out value)) {
                _refSet[key] = new AtomicLong(1);
                _countPoints++;
            }
            else {
                value.IncrementAndGet();
            }
        }

        /// <summary>
        ///     Add a key to the set with the given number of references.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="numReferences">The num references.</param>
        public void Add(
            TK key,
            int numReferences)
        {
            AtomicLong value;
            if (!_refSet.TryGetValue(key, out value)) {
                _refSet[key] = new AtomicLong(numReferences);
                return;
            }

            throw new ArgumentException("Value '" + key + "' already in collection");
        }

        /// <summary>
        ///     Clear out the collection.
        /// </summary>
        public void Clear()
        {
            _refSet.Clear();
            _countPoints = 0;
        }

        /// <summary>
        ///     Remove a key from the set. Removes the key if the reference count is one.
        ///     Decreases the reference count by one if the reference count is more then one.
        /// </summary>
        /// <param name="key">
        ///     to add
        /// </param>
        /// <throws>  IllegalStateException is a key is removed that wasn't added to the map </throws>
        public virtual void Remove(TK key)
        {
            AtomicLong value;

            if (!_refSet.TryGetValue(key, out value)) {
                // This could happen if a sort operation gets a remove stream that duplicates events.
                // Generally points to an invalid combination of data windows.
                // throw new IllegalStateException("Attempting to remove key from map that wasn't added");
                return;
            }

            --_countPoints;
            
            if (value.DecrementAndGet() == 0) {
                _refSet.Remove(key);
            }
        }
    }
}