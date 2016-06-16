///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// Functions as a key value for Maps where keys need to be composite values. The class allows a 
    /// Map that uses MultiKeyUntyped entries for key values to use multiple objects as keys. It 
    /// calculates the hashCode from the key objects on construction and caches the hashCode. 
    /// </summary>
    [Serializable]
    public class MultiKeyUntyped : MetaDefItem
    {
        private readonly Object[] _keys;
        private readonly int _hashCode;

        /// <summary>
        /// Constructor for multiple keys supplied in an object array.
        /// </summary>
        /// <param name="keys">is an array of key objects</param>
        public MultiKeyUntyped(Object[] keys)
        {
            if (keys == null)
            {
                throw new ArgumentException("The array of keys must not be null");
            }

            unchecked
            {
                int total = 0;
                int length = keys.Length;

                for (int ii = 0; ii < length; ii++)
                {
                    if (keys[ii] != null)
                    {
                        total *= 397;
                        total ^= keys[ii].GetHashCode();
                    }
                }

                _hashCode = total;
                _keys = keys;
            }
        }

        /// <summary>Constructor for a single key object. </summary>
        /// <param name="key">is the single key object</param>
        public MultiKeyUntyped(Object key)
            : this(new [] { key })
        {
        }

        /// <summary>Constructor for a pair of key objects. </summary>
        /// <param name="key1">is the first key object</param>
        /// <param name="key2">is the second key object</param>
        public MultiKeyUntyped(Object key1, Object key2)
            : this(new [] { key1, key2 })
        {
        }

        /// <summary>Constructor for three key objects. </summary>
        /// <param name="key1">is the first key object</param>
        /// <param name="key2">is the second key object</param>
        /// <param name="key3">is the third key object</param>
        public MultiKeyUntyped(Object key1, Object key2, Object key3)
            : this(new [] { key1, key2, key3 })
        {
        }

        /// <summary>Constructor for four key objects. </summary>
        /// <param name="key1">is the first key object</param>
        /// <param name="key2">is the second key object</param>
        /// <param name="key3">is the third key object</param>
        /// <param name="key4">is the fourth key object</param>
        public MultiKeyUntyped(Object key1, Object key2, Object key3, Object key4)
            : this(new [] { key1, key2, key3, key4 })
        {
        }

        /// <summary>Returns the number of key objects. </summary>
        /// <value>size of key object array</value>
        public int Count
        {
            get { return _keys.Length; }
        }

        /// <summary>Returns the key object at the specified position. </summary>
        /// <param name="index">is the array position</param>
        /// <returns>key object at position</returns>
        public Object Get(int index)
        {
            return _keys[index];
        }

        public override bool Equals(Object other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (other is MultiKeyUntyped)
            {
                var otherKeys = (MultiKeyUntyped)other;
                return Collections.AreEqual(_keys, otherKeys._keys);
            }
            return false;
        }

        /// <summary>Returns keys. </summary>
        /// <value>keys object array</value>
        public object[] Keys
        {
            get { return _keys; }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override String ToString()
        {
            return "MultiKeyUntyped" + _keys.Render();
        }
    }
}
