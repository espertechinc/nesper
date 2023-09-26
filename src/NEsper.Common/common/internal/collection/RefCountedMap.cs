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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Reference-counting map based on a HashMap implementation that stores as a value a pair of value and reference counter.
    /// The class provides a reference method that takes a key
    /// and increments the reference count for the key. It also provides a de-reference method that takes a key and
    /// decrements the reference count for the key, and removes the key if the reference count reached zero.
    /// Null values are not allowed as keys.
    /// </summary>
    public class RefCountedMap<TK, TV>
    {
        private readonly IDictionary<TK, Pair<TV, int>> _refMap;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RefCountedMap()
        {
            _refMap = new Dictionary<TK, Pair<TV, int>>();
        }


        /// <summary>
        /// Gets or sets the item with the specified key.
        /// </summary>
        /// <value></value>
        public virtual TV this[TK key] {
            get {
                if (!_refMap.TryGetValue(key, out var refValue)) {
                    return default;
                }

                return refValue.First;
            }

            set {
                if (key == null) {
                    throw new ArgumentException("Collection does not allow null key values");
                }

                if (_refMap.ContainsKey(key)) {
                    throw new IllegalStateException("Value value already in collection");
                }

                var refValue = new Pair<TV, int>(value, 1);
                _refMap[key] = refValue;

                //return val;
            }
        }

        public bool TryGetValue(
            TK key,
            out TV value)
        {
            if (!_refMap.TryGetValue(key, out var refValue)) {
                value = default;
                return false;
            }

            value = refValue.First;
            return true;
        }

        public bool Contains(TK key)
        {
            return _refMap.ContainsKey(key);
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to thenew value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Put(
            TK key,
            TV value)
        {
            this[key] = value;
        }

        /// <summary> Increase the reference count for a given key by one.
        /// Throws an ArgumentException if the key was not found.
        /// </summary>
        /// <param name="key">is the key to increase the ref count for
        /// </param>
        public void Reference(TK key)
        {
            Pair<TV, int> refValue;
            if (!_refMap.TryGetValue(key, out refValue)) {
                throw new IllegalStateException("Value value not found in collection");
            }

            refValue.Second = refValue.Second + 1;
        }

        /// <summary> Decreases the reference count for a given key by one. Returns true if the reference count reaches zero.
        /// Removes the key from the collection when the reference count reaches zero.
        /// Throw an ArgumentException if the key is not found.
        /// </summary>
        /// <param name="key">to de-reference
        /// </param>
        /// <returns> true to indicate the reference count reached zero, false to indicate more references to the key exist.
        /// </returns>
        public virtual bool Dereference(TK key)
        {
            Pair<TV, int> refValue;
            if (!_refMap.TryGetValue(key, out refValue)) {
                throw new IllegalStateException("Value value not found in collection");
            }

            var refCounter = refValue.Second;
            if (refCounter < 1) {
                throw new IllegalStateException(
                    "Unexpected reference counter value " + refValue.Second + " encountered for key " + key);
            }

            // Remove key on dereference of last reference
            if (refCounter == 1) {
                _refMap.Remove(key);
                return true;
            }

            refValue.Second = refCounter - 1;
            return false;
        }

        /// <summary>
        /// Clear out the collection.
        /// </summary>
        public virtual void Clear()
        {
            _refMap.Clear();
        }
    }
}