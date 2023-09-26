///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary> Functions as a key value for Maps where keys need to be composite values.
    /// The class allows a Map that uses MultiKeyUntyped entries for key values to use multiple objects as keys.
    /// It calculates the hashCode from the key objects on construction and caches the hashCode.
    /// </summary>
    public sealed class MultiKeyObsolete<T> where T : class
    {
        /// <summary> Returns the key value array.</summary>
        /// <returns> key value array
        /// </returns>

        public T[] Array => _keys;

        private readonly T[] _keys;
        private readonly int _hashCode;

        /// <summary> Constructor for multiple keys supplied in an object array.</summary>
        /// <param name="keys">is an array of key objects
        /// </param>
        public MultiKeyObsolete(params T[] keys)
        {
            if (keys == null) {
                throw new ArgumentException("The array of keys must not be null");
            }

            var total = 0;
            for (var i = 0; i < keys.Length; i++) {
                if (keys[i] != null) {
                    total = (total * 397) ^ keys[i].GetHashCode();
                }
            }

            _hashCode = total;
            _keys = keys;
        }

        /// <summary> Returns the number of key objects.</summary>
        /// <returns> size of key object array
        /// </returns>

        public int Count => _keys.Length;

        /// <summary> Returns the key object at the specified position.</summary>
        /// <param name="index">is the array position
        /// </param>
        /// <returns> key object at position
        /// </returns>

        public T this[int index] => _keys[index];

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="other">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object other)
        {
            if (other == this) {
                return true;
            }

            if (other is MultiKeyObsolete<T> otherKeys) {
                return Arrays.AreEqual(_keys, otherKeys._keys);
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return "MultiKeyUntyped{{" + _keys.Render() + "}}";
        }
    }
}