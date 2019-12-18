///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

namespace com.espertech.esper.compat.collections
{
    public sealed class ReferenceMap<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        /// <summary>
        /// Underlying dictionary must be opaque to match the semantics of
        /// the reference types.
        /// </summary>
        private readonly Dictionary<object, object> _dictionary;

        /// <summary>
        /// Defines the way that keys are maintained in the dictionary
        /// </summary>
        private ReferenceType _keyReferenceType;
        private readonly IReferenceAdapter<TKey> _keyAdapter;

        /// <summary>
        /// Defines the way that values are maintained in the dictionary
        /// </summary>
        private ReferenceType _valueReferenceType;
        private readonly IReferenceAdapter<TValue> _valueAdapter;

        /// <summary>
        /// List of dictionary keys that need to be removed
        /// </summary>
        private readonly List<object> _pruneList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMap{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="keyReferenceType">Type of the key reference.</param>
        /// <param name="valueReferenceType">Type of the value reference.</param>
        public ReferenceMap( ReferenceType keyReferenceType,
                                    ReferenceType valueReferenceType )
            : this( 101, keyReferenceType, valueReferenceType )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMap{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="keyReferenceType">Type of the key reference.</param>
        /// <param name="valueReferenceType">Type of the value reference.</param>
        public ReferenceMap( int capacity,
                                    ReferenceType keyReferenceType,
                                    ReferenceType valueReferenceType )
        {
            _pruneList = new List<object>();

            _keyReferenceType = keyReferenceType;
            _keyAdapter =
                keyReferenceType == ReferenceType.HARD
                    ? (IReferenceAdapter<TKey>) new HardReferenceAdapter<TKey>()
                    : (IReferenceAdapter<TKey>)new SoftReferenceAdapter<TKey>();

            _valueReferenceType = valueReferenceType;
            _valueAdapter =
                valueReferenceType == ReferenceType.HARD
                    ? (IReferenceAdapter<TValue>)new HardReferenceAdapter<TValue>()
                    : (IReferenceAdapter<TValue>)new SoftReferenceAdapter<TValue>();

            // Create the dictionary
            _dictionary = new Dictionary<object, object>(capacity, _keyAdapter);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <remarks>
        /// WARNING: The count returned here may include entries for which
        /// either the key or value objects have already been garbage
        /// collected. Call RemoveCollectedEntries to weed out collected
        /// entries and Update the count accordingly.
        /// </remarks>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        
        public int Count
        {
            get
            {
                Prune();  // Clear out known dead items
                return _dictionary.Count;
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            // Create the dictionary key
            Object dictKey = _keyAdapter.ReferenceToDictionary(key);
            // Create the dictionary value
            Object dictValue = _valueAdapter.ReferenceToDictionary(value);
            // Add them to the dictionary
            _dictionary.Add(dictKey, dictValue);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
        	Object tempKey = key ;
            Object tempOut;

        	if ( _dictionary.TryGetValue( tempKey, out tempOut ) )
        	{
        	    if (_valueAdapter.DictionaryToReference(tempOut, out value))
        	    {
        	        return true;
        	    }

        	    Prune(tempKey);
        	}

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetValue(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException("key");
            // Create the dictionary key
            Object dictKey = _keyAdapter.ReferenceToDictionary(key);
            // Create the dictionary value
            Object dictValue = _valueAdapter.ReferenceToDictionary(value);
            // Add them to the dictionary
            _dictionary[dictKey] = dictValue;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (KeyValuePair<object, object> entry in _dictionary)
            {
                TKey entryKey;
                TValue entryValue;

                if ((!_keyAdapter.DictionaryToReference(entry.Key, out entryKey)) ||
                    (!_valueAdapter.DictionaryToReference(entry.Value, out entryValue)))
                {
                    Prune(entry.Key);
                }
                else
                {
                    yield return new KeyValuePair<TKey, TValue>(entryKey, entryValue);
                }
            }
        }

        /// <summary>
        /// Adds the specified dictionary key to the prune list.
        /// </summary>
        /// <param name="dictKey">The dict key.</param>
        private void Prune( Object dictKey )
        {
            lock( _pruneList )
            {
                _pruneList.Add(dictKey);
            }
        }

        /// <summary>
        /// Removes all 'dead' references that have been added to the
        /// prune list.
        /// </summary>

        public void Prune()
        {
            foreach (Object item in _pruneList)
            {
                _dictionary.Remove(item);
            }
        }

        /// <summary>
        /// Removes the left-over weak references for entries in the dictionary
        /// whose key or value has already been reclaimed by the garbage
        /// collector. This will reduce the dictionary's Count by the number
        /// of dead key-value pairs that were eliminated.
        /// </summary>
        public void Purge()
        {
            // Iterate over the collection; this will cause entries that are dead to
            // be entered into the purgeList.
            foreach( KeyValuePair<TKey, TValue> entry in this ) {}
            // Prune the tree
            Prune();
        }

        /// <summary>
        /// Gets an enumerator that enumerates the keys.
        /// </summary>
        /// <value>The keys enum.</value>
        
        public IEnumerator<TKey> KeysEnum
        {
        	get 
        	{
                foreach (KeyValuePair<TKey, TValue> entry in this)
                {
                    yield return entry.Key;
                }
        	}
        }
        
        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that : <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TKey> Keys
        {
            get
            {
        		List<TKey> keyList = new List<TKey>() ;
                foreach (KeyValuePair<TKey, TValue> entry in this)
                {
                    keyList.Add(entry.Key);
                }

        		return keyList ;
        	}
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that : <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> valueList = new List<TValue>();
                foreach (KeyValuePair<TKey, TValue> entry in this)
                {
                    valueList.Add(entry.Value);
                }

                return valueList;
            }
        }

        /// <summary>
        /// Gets or sets the item with the specified key.
        /// </summary>
        /// <value></value>
        public TValue this[TKey key]
        {
            get
            {
            	TValue rvalue = null ;
            	if ( TryGetValue( key, out rvalue ) )
            	{
            	    return rvalue;
            	}
            	
            	throw new KeyNotFoundException( "Value '" + key + "' not found" ) ;
            }
            set => SetValue(key, value);
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TKey key = item.Key ;
        	TValue value ;
        	if ( TryGetValue( key, out value ) )
            {
                return Object.Equals( value, item.Value ) ;
        	}
            
            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, Starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>

        public bool IsReadOnly => false;

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then the defaultValue is
        /// returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TValue Get(TKey key, TValue defaultValue)
        {
            TValue returnValue = defaultValue;
            if (key != null)
            {
                if (!TryGetValue(key, out returnValue))
                {
                    returnValue = defaultValue;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then default(V) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue Get(TKey key)
        {
            return Get(key, default(TValue));
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to thenew value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Put(TKey key, TValue value)
        {
            this[key] = value;
        }

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to the new value.
        /// If a value was previously mapped it is returned.
        /// </summary>

        public TValue Push(TKey key, TValue value)
        {
            TValue temp;
            TryGetValue(key, out temp);
            this[key] = value;
            return temp;
        }

        /// <summary>
        /// Puts all values from the source dictionary into
        /// this dictionary.
        /// </summary>
        /// <param name="source"></param>
        public void PutAll(IDictionary<TKey, TValue> source)
        {
            foreach( KeyValuePair<TKey, TValue> entry in source )
            {
                this[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Returns the first value in the enumeration of values
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public TValue FirstValue
        {
            get
            {
                IEnumerator<KeyValuePair<TKey, TValue>> enumObj = GetEnumerator();
                return enumObj.MoveNext()
                           ? enumObj.Current.Value
                           : default(TValue);
            }
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.
        /// </summary>
        /// <param name="key">Search key into the dictionary</param>
        /// <param name="value">The value removed from the dictionary (if found).</param>
        /// <returns></returns>
        public bool Remove(TKey key, out TValue value)
        {
            if (!TryGetValue(key, out value))
            {
                return false;
            }

            return Remove(key);
        }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.  The item if found is returned; if not,
        /// default(V) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue RemoveAndReturn(TKey key)
        {
            TValue tempItem;

            return Remove(key, out tempItem)
                    ? tempItem
                    : default(TValue);
        }

        #endregion

        /// <summary>
        /// Converts items from their reference for to their dictionary form and
        /// vice-versa.
        /// </summary>
        /// <typeparam name="T"></typeparam>

        internal interface IReferenceAdapter<T> : IEqualityComparer<Object>
        {
            /// <summary>
            /// Converts the item from a reference item to a dictionary item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns></returns>
            Object ReferenceToDictionary(T item);

            /// <summary>
            /// Converts the item from a dictionary item to a reference item.
            /// Returns true if the dictionary item is still alive.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="refItem">The reference item.</param>
            /// <returns></returns>
            bool DictionaryToReference(Object item, out T refItem);
        }

        internal class HardReferenceAdapter<T> : IReferenceAdapter<T> where T : class
        {
            #region IReferenceAdapter<T> Members

            /// <summary>
            /// Converts the item from a reference item to a dictionary item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns></returns>
            public object ReferenceToDictionary(T item)
            {
                return item;
            }

            /// <summary>
            /// Converts the item from a dictionary item to a reference item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="refItem">The reference item.</param>
            /// <returns></returns>
            public bool DictionaryToReference(Object item, out T refItem)
            {
                refItem = item as T;
                return true;
            }

            #endregion

            #region IEqualityComparer<object> Members

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type T to compare.</param>
            /// <param name="y">The second object of type T to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public new bool Equals(object x, object y)
            {
                return
                    Object.ReferenceEquals(x, y) ||
                    Object.Equals(x, y);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified object.</returns>
            /// <exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        internal class SoftReferenceAdapter<T> : IReferenceAdapter<T> where T : class
        {
            #region IReferenceAdapter<T> Members

            /// <summary>
            /// Converts the item from a reference item to a dictionary item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <returns></returns>
            public object ReferenceToDictionary(T item)
            {
                return new WeakReference<T>(item);
            }

            /// <summary>
            /// Converts the item from a dictionary item to a reference item.
            /// </summary>
            /// <param name="item">The item.</param>
            /// <param name="refItem">The reference item.</param>
            /// <returns></returns>
            public bool DictionaryToReference(Object item, out T refItem)
            {
                WeakReference<T> reference = item as WeakReference<T>;
                if ((reference != null) && (reference.IsAlive))
                {
                    refItem = reference.Target as T;
                    return refItem != null;
                }

                refItem = null;
                return false;
            }
            #endregion

            #region IEqualityComparer<object> Members
            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
            /// </summary>
            /// <param name="x">The first object of type T to compare.</param>
            /// <param name="y">The second object of type T to compare.</param>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
            /// </returns>
            /// <remark>
            /// Note: There are actually 9 cases to handle here.
            /// Let Wa = Alive Weak Reference
            /// Let Wd = Dead Weak Reference
            /// Let S  = Strong Reference
            /// x  | y  | Equals(x,y)
            /// -------------------------------------------------
            /// Wa | Wa | comparer.Equals(x.Target, y.Target)
            /// Wa | Wd | false
            /// Wa | S  | comparer.Equals(x.Target, y)
            /// Wd | Wa | false
            /// Wd | Wd | x == y
            /// Wd | S  | false
            /// S  | Wa | comparer.Equals(x, y.Target)
            /// S  | Wd | false
            /// S  | S  | comparer.Equals(x, y)
            /// -------------------------------------------------
            /// </remark>
            public new bool Equals(object x, object y)
            {
                bool xIsDead, yIsDead;
                T first = GetTarget(x, out xIsDead);
                T second = GetTarget(y, out yIsDead);

                if (xIsDead)
                    return yIsDead ? x == y : false;

                if (yIsDead)
                    return false;

                return
                    Object.ReferenceEquals(first, second) ||
                    Object.Equals(first, second);
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
            /// <returns>A hash code for the specified object.</returns>
            /// <exception cref="T:System.ArgumentNullException">The type of obj is a reference type and obj is null.</exception>
            public int GetHashCode(object obj)
            {
                // Depending upon the path, the object that we are being passed could be
                // an object in reference form or dictionary form.  In reference form, it
                // is just a plain old T; in dictionary form, it would be a WeakReference<T>

                WeakReference<T> refT = obj as WeakReference<T>;
                return (refT != null)
                        ? (refT.GetHashCode())
                        : (obj.GetHashCode());
            }

            /// <summary>
            /// Gets the target of the object.  The target can only be a WeakReference of T or
            /// T itself.  This method distinguishes between the two and returns the actual
            /// target object.  Status of the target is returned through the out parameter.
            /// </summary>
            /// <param name="obj">The obj.</param>
            /// <param name="isDead">if set to <c>true</c> [is dead].</param>
            /// <returns></returns>
            private static T GetTarget(object obj, out bool isDead)
            {
                WeakReference<T> wref = obj as WeakReference<T>;
                T target;
                if (wref != null)
                {
                    target = wref.Target;
                    isDead = !wref.IsAlive;
                }
                else
                {
                    target = (T)obj;
                    isDead = false;
                }
                return target;
            }

            #endregion
        }
    }

    public enum ReferenceType
    {
        /// <summary>
        /// Hard references keep references to the object and prevent the
        /// garbage collector from collecting the item.
        /// </summary>
        HARD,
        /// <summary>
        /// Soft references allow the garbage collector to collect items
        /// that are not in use.
        /// </summary>
        SOFT
    }
}
