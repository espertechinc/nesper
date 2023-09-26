///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    ///     Functions as a key value for Maps where keys need to be composite values.
    ///     The class allows a Map that uses HashableMultiKey entries for key values to use multiple objects as keys.
    ///     It calculates the hashCode from the key objects on construction and caches the hashCode.
    /// </summary>
    public class MultiKeyArrayOfKeys<T>
    {
        private readonly int _hashCode;

        /// <summary>
        ///     Constructor for multiple keys supplied in an object array.
        /// </summary>
        /// <param name="keys">is an array of key objects</param>
        public MultiKeyArrayOfKeys(T[] keys)
        {
            if (keys == null) {
                throw new ArgumentException("The array of keys must not be null");
            }

            _hashCode = CompatExtensions.DeepHash(keys);
            Array = keys;
        }

        /// <summary>
        ///     Returns the number of key objects.
        /// </summary>
        /// <value>size of key object array</value>
        public int Count => Array.Length;

        /// <summary>
        ///     Returns the key value array.
        /// </summary>
        /// <value>key value array</value>
        public T[] Array { get; }

        /// <summary>
        ///     Returns the key object at the specified position.
        /// </summary>
        /// <param name="index">is the array position</param>
        /// <returns>key object at position</returns>
        public T Get(int index)
        {
            return Array[index];
        }

        public T this[int index] {
            get => Array[index];
            set => Array[index] = value;
        }

        public override bool Equals(object other)
        {
            if (other == this) {
                return true;
            }

            if (other is MultiKeyArrayOfKeys<T> otherKeys) {
                return Arrays.DeepEquals(Array, otherKeys.Array);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return GetType().Name + Arrays.AsList(Array);
        }
    }
} // end of namespace