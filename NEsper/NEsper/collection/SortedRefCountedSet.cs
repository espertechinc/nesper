///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.collection
{
	/// <summary>
    /// Sorted, reference-counting set based on a SortedDictionary implementation that stores keys and a reference counter for
	/// each unique key value. Each time the same key is added, the reference counter increases.
	/// Each time a key is removed, the reference counter decreases.
	/// </summary>

    public class SortedRefCountedSet<K>
	{
    	private readonly SortedList<K, MutableInt> _refSet;
        private long _countPoints;

        /// <summary>
        /// Gets the number of data points.
        /// </summary>
	    public long CountPoints
	    {
	        get { return _countPoints; }
            set { _countPoints = value; }
	    }

        /// <summary>
        /// Gets the ref set.
        /// </summary>
        /// <value>The ref set.</value>
	    public SortedList<K, MutableInt> RefSet
	    {
	        get { return _refSet; }
	    }

	    /// <summary>
		///  Constructor.
		/// </summary>

		public SortedRefCountedSet()
		{
            _countPoints = 0;
            _refSet = new SortedList<K, MutableInt>();
		}

		/// <summary> Add a key to the set. Add with a reference count of one if the key didn't exist in the set.
		/// Increase the reference count by one if the key already exists.
		/// </summary>
		/// <param name="key">to add
		/// </param>

		public virtual void Add(K key)
		{
            MutableInt value;
            if (!_refSet.TryGetValue(key, out value))
            {
                _refSet.Add(key, new MutableInt());
                _countPoints++;
            }
            else
            {
                value.Value++;
            }
		}

        /// <summary>
        /// Add a key to the set with the given number of references.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="numReferences">The num references.</param>
        public void Add(K key, int numReferences)
        {
            MutableInt value;
            if (!_refSet.TryGetValue(key, out value))
            {
                _refSet[key] = new MutableInt(numReferences);
                return;
            }
            throw new ArgumentException("Value '" + key + "' already in collection");
        }

        /// <summary>
        /// Clear out the collection.
        /// </summary>
        public void Clear()
        {
            _refSet.Clear();
            _countPoints = 0;
        }

		/// <summary> Remove a key from the set. Removes the key if the reference count is one.
		/// Decreases the reference count by one if the reference count is more then one.
		/// </summary>
		/// <param name="key">to add
		/// </param>
		/// <throws>  IllegalStateException is a key is removed that wasn't added to the map </throws>

        public virtual void Remove(K key)
		{
            MutableInt value;

            if (!_refSet.TryGetValue(key, out value))
            {
                // This could happen if a sort operation gets a remove stream that duplicates events.
                // Generally points to an invalid combination of data windows.
                // throw new IllegalStateException("Attempting to remove key from map that wasn't added");
                return;
            }

            --_countPoints;
			if (value.Value == 1)
			{
				_refSet.Remove(key);
				return ;
			}

			value.Value--;
            //refSet[key] = value;
		}

		/// <summary> Returns the largest key value, or null if the collection is empty.</summary>
		/// <returns> largest key value, null if none
		/// </returns>

        public virtual K MaxValue
		{
        	get
        	{
        		return
        			( _refSet.Count != 0 ) ?
        			( _refSet.Keys[_refSet.Count - 1] ) :
        			( default(K) ) ;
        	}
		}

		/// <summary> Returns the smallest key value, or null if the collection is empty.</summary>
		/// <returns> smallest key value, default(K) if none
		/// </returns>

        public virtual K MinValue
		{
        	get
        	{
        		return
        			( _refSet.Count != 0 ) ?
        			( _refSet.Keys[0] ) :
        			( default(K) ) ;
        	}
		}

        public sealed class MutableInt : IComparable
        {
            public int Value = 1;

            public int CompareTo(object obj)
            {
                var other = obj as MutableInt;
                if (other == null)
                {
                    throw new ArgumentException("invalid argument to comparison");
                }

                return Value.CompareTo(other.Value);
            }

            public MutableInt() {}
            public MutableInt(int initialValue)
            {
                Value = initialValue;
            }
        }
	}
}
