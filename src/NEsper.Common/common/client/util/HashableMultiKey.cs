///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.util
{
    public sealed class HashableMultiKey
    {
        /// <summary>
        ///     Constructor for multiple keys supplied in an object array.
        /// </summary>
        /// <param name="keys">is an array of key objects</param>
        public HashableMultiKey(object[] keys)
        {
            Keys = keys ?? throw new ArgumentException("The array of keys must not be null");
        }

        /// <summary>
        ///     Constructor for a single key object.
        /// </summary>
        /// <param name="key">is the single key object</param>
        public HashableMultiKey(object key)
            : this(new[] { key })
        {
        }

        /// <summary>
        ///     Constructor for a pair of key objects.
        /// </summary>
        /// <param name="key1">is the first key object</param>
        /// <param name="key2">is the second key object</param>
        public HashableMultiKey(
            object key1,
            object key2)
            : this(new[] { key1, key2 })
        {
        }

        /// <summary>
        ///     Constructor for three key objects.
        /// </summary>
        /// <param name="key1">is the first key object</param>
        /// <param name="key2">is the second key object</param>
        /// <param name="key3">is the third key object</param>
        public HashableMultiKey(
            object key1,
            object key2,
            object key3)
            : this(new[] { key1, key2, key3 })
        {
        }

        /// <summary>
        ///     Constructor for four key objects.
        /// </summary>
        /// <param name="key1">is the first key object</param>
        /// <param name="key2">is the second key object</param>
        /// <param name="key3">is the third key object</param>
        /// <param name="key4">is the fourth key object</param>
        public HashableMultiKey(
            object key1,
            object key2,
            object key3,
            object key4)
            : this(new[] { key1, key2, key3, key4 })
        {
        }

        /// <summary>
        ///     Returns keys.
        /// </summary>
        /// <value>keys object array</value>
        public object[] Keys { get; }

        /// <summary>
        ///     Returns the number of key objects.
        /// </summary>
        /// <value>size of key object array</value>
        public int Count => Keys.Length;

        /// <summary>
        ///     Returns the key object at the specified position.
        /// </summary>
        /// <param name="index">is the array position</param>
        /// <returns>key object at position</returns>
        public object Get(int index)
        {
            return Keys[index];
        }

        public override bool Equals(object other)
        {
            if (other == this) {
                return true;
            }

            if (other is HashableMultiKey otherKeys) {
                return Keys.AreEqual(otherKeys.Keys);
            }

            return false;
        }

        public override int GetHashCode()
        {
            var total = 0;
            for (var i = 0; i < Keys.Length; i++) {
                if (Keys[i] != null) {
                    total *= 31;
                    total ^= Keys[i].GetHashCode();
                }
            }

            return total;
        }

        public override string ToString()
        {
            return "HashableMultiKey" + Keys.Render();
        }
    }
} // end of namespace