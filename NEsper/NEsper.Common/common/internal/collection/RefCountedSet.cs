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
    /// <summary> reference-counting set based on a HashMap implementation that stores keys and a reference counter for
    /// each unique key value. Each time the same key is added, the reference counter increases.
    /// Each time a key is removed, the reference counter decreases.
    /// </summary>
    public class RefCountedSet<TK>
    {
        private bool _hasNullEntry;
        private int _nullEntry;
        private readonly IDictionary<TK, int> _refSet;
        private int _numValues;

        /// <summary>
        /// Constructor.
        /// </summary>
        public RefCountedSet()
        {
            _refSet = new Dictionary<TK, Int32>();
        }

        public RefCountedSet(
            IDictionary<TK, int> refSet,
            int numValues)
        {
            _refSet = refSet;
            _numValues = numValues;
        }

        /// <summary>
        /// Adds a key to the set, but the key is null.  It behaves the same, but has its own
        /// variables that need to be incremented.
        /// </summary>
        private bool AddNull()
        {
            if (!_hasNullEntry) {
                _hasNullEntry = true;
                _numValues++;
                _nullEntry = 0;
                return true;
            }

            _numValues++;
            _nullEntry++;

            return false;
        }

        /// <summary> Add a key to the set. Add with a reference count of one if the key didn't exist in the set.
        /// Increase the reference count by one if the key already exists.
        /// Return true if this is the first time the key was encountered, or false if key is already in set.
        /// </summary>
        /// <param name="key">to add
        /// </param>
        /// <returns> true if the key is not in the set already, false if the key is already in the set
        /// </returns>
        public virtual bool Add(TK key)
        {
            if (ReferenceEquals(key, null)) {
                return AddNull();
            }

            int value;
            if (!_refSet.TryGetValue(key, out value)) {
                _refSet[key] = 1;
                _numValues++;
                return true;
            }

            value++;
            _numValues++;
            _refSet[key] = value;
            return false;
        }

        /// <summary>
        /// Removes the null key
        /// </summary>
        private bool RemoveNull()
        {
            if (_nullEntry == 1) {
                _hasNullEntry = false;
                _nullEntry--;
                return true;
            }

            _nullEntry--;
            _numValues--;

            return false;
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="numReferences">The num references.</param>
        public void Add(
            TK key,
            int numReferences)
        {
            int value;
            if (!_refSet.TryGetValue(key, out value)) {
                _refSet[key] = numReferences;
                _numValues += numReferences;
                return;
            }

            throw new ArgumentException("Value '" + key + "' already in collection");
        }

        /// <summary> Removed a key to the set. Removes the key if the reference count is one.
        /// Decreases the reference count by one if the reference count is more then one.
        /// Return true if the reference count was one and the key thus removed, or false if key is stays in set.
        /// </summary>
        /// <param name="key">to add
        /// </param>
        /// <returns> true if the key is removed, false if it stays in the set
        /// </returns>
        /// <throws>  IllegalStateException is a key is removed that wasn't added to the map </throws>
        public virtual bool Remove(TK key)
        {
            if (ReferenceEquals(key, null)) {
                return RemoveNull();
            }

            int value;
            if (!_refSet.TryGetValue(key, out value)) {
                return true; // ignore duplcate removals
            }

            if (value == 1) {
                _refSet.Remove(key);
                _numValues--;
                return true;
            }

            value--;
            _refSet[key] = value;
            _numValues--;
            return false;
        }

        /// <summary>
        /// Remove a key from the set regardless of the number of references.
        /// </summary>
        /// <param name="key">to add</param>
        /// <returns>
        /// true if the key is removed, false if the key was not found
        /// </returns>
        /// <throws>IllegalStateException if a key is removed that wasn't added to the map</throws>
        public bool RemoveAll(TK key)
        {
            return _refSet.Remove(key);
        }

        /// <summary> Returns an iterator over the entry set.</summary>
        /// <returns> entry set iterator
        /// </returns>
        public IEnumerator<KeyValuePair<TK, int>> GetEnumerator()
        {
            if (_hasNullEntry) {
                yield return new KeyValuePair<TK, int>(default(TK), _nullEntry);
            }

            foreach (KeyValuePair<TK, int> value in _refSet) {
                yield return value;
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<TK> Keys {
            get { return _refSet.Keys; }
        }

        /// <summary> Returns the number of values in the collection.</summary>
        /// <returns> size
        /// </returns>

        public virtual int Count {
            get { return _numValues; }
        }

        /// <summary>
        /// Clear out the collection.
        /// </summary>
        public virtual void Clear()
        {
            _refSet.Clear();
            _numValues = 0;
        }

        public IDictionary<TK, int> RefSet {
            get { return _refSet; }
        }

        public int NumValues {
            get { return _numValues; }
            set { _numValues = value; }
        }
    }
}