///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     Sorted, reference-counting set based on an OrderedDictionary implementation that
    ///     stores keys and a reference counter for each unique key value. Each time the same
    ///     key is added, the reference counter increases. Each time a key is removed, the
    ///     reference counter decreases.
    /// </summary>
    public class SortedRefCountedSet<K>
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public SortedRefCountedSet()
        {
            CountPoints = 0;
            RefSet = new SortedList<K, MutableInt>();
        }

        /// <summary>
        ///     Gets the number of data points.
        /// </summary>
        public long CountPoints { get; set; }

        /// <summary>
        ///     Gets the ref set.
        /// </summary>
        /// <value>The ref set.</value>
        public SortedList<K, MutableInt> RefSet { get; }

        /// <summary> Returns the largest key value, or null if the collection is empty.</summary>
        /// <returns>
        ///     largest key value, null if none
        /// </returns>

        public virtual K MaxValue => RefSet.Count != 0 ? RefSet.Keys[RefSet.Count - 1] : default(K);

        /// <summary> Returns the smallest key value, or null if the collection is empty.</summary>
        /// <returns>
        ///     smallest key value, default(K) if none
        /// </returns>

        public virtual K MinValue => RefSet.Count != 0 ? RefSet.Keys[0] : default(K);

        /// <summary>
        ///     Add a key to the set. Add with a reference count of one if the key didn't exist in the set.
        ///     Increase the reference count by one if the key already exists.
        /// </summary>
        /// <param name="key">
        ///     to add
        /// </param>
        public virtual void Add(K key)
        {
            MutableInt value;
            if (!RefSet.TryGetValue(key, out value)) {
                RefSet.Add(key, new MutableInt());
                CountPoints++;
            }
            else {
                value.Value++;
            }
        }

        /// <summary>
        ///     Add a key to the set with the given number of references.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="numReferences">The num references.</param>
        public void Add(
            K key,
            int numReferences)
        {
            MutableInt value;
            if (!RefSet.TryGetValue(key, out value)) {
                RefSet[key] = new MutableInt(numReferences);
                return;
            }

            throw new ArgumentException("Value '" + key + "' already in collection");
        }

        /// <summary>
        ///     Clear out the collection.
        /// </summary>
        public void Clear()
        {
            RefSet.Clear();
            CountPoints = 0;
        }

        /// <summary>
        ///     Remove a key from the set. Removes the key if the reference count is one.
        ///     Decreases the reference count by one if the reference count is more then one.
        /// </summary>
        /// <param name="key">
        ///     to add
        /// </param>
        /// <throws>  IllegalStateException is a key is removed that wasn't added to the map </throws>
        public virtual void Remove(K key)
        {
            MutableInt value;

            if (!RefSet.TryGetValue(key, out value)) {
                // This could happen if a sort operation gets a remove stream that duplicates events.
                // Generally points to an invalid combination of data windows.
                // throw new IllegalStateException("Attempting to remove key from map that wasn't added");
                return;
            }

            --CountPoints;
            if (value.Value == 1) {
                RefSet.Remove(key);
                return;
            }

            value.Value--;
            //refSet[key] = value;
        }

        public sealed class MutableInt : IComparable
        {
            public int Value = 1;

            public MutableInt()
            {
            }

            public MutableInt(int initialValue)
            {
                Value = initialValue;
            }

            public int CompareTo(object obj)
            {
                var other = obj as MutableInt;
                if (other == null) {
                    throw new ArgumentException("invalid argument to comparison");
                }

                return Value.CompareTo(other.Value);
            }
        }
    }
}