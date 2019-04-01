///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.util;

namespace com.espertech.esper.collection
{
    /// <summary>
    /// Functions as a key value for Maps where keys need to be composite values, and includes 
    /// an <seealso cref="com.espertech.esper.client.EventBean" /> handle.
    /// The class allows a Map that uses MultiKeyUntyped entries for key values to use multiple objects 
    /// as keys. It calculates the hashCode from the key objects on construction and caches the hashCode.
    /// </summary>
    [Serializable]
    public sealed class MultiKeyUntypedEventPair : MetaDefItem
    {
        [NonSerialized] private readonly Object[] _keys;
        [NonSerialized] private readonly EventBean _eventBean = null;
        private readonly int _hashCode;

        /// <summary>Constructor for multiple keys supplied in an object array. </summary>
        /// <param name="keys">is an array of key objects</param>
        /// <param name="eventBean">for pair</param>
        public MultiKeyUntypedEventPair(Object[] keys, EventBean eventBean)
        {
            if (keys == null)
            {
                throw new ArgumentException("The array of keys must not be null");
            }

            int total = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i] != null)
                {
                    total *= 31;
                    total ^= keys[i].GetHashCode();
                }
            }

            _hashCode = total;
            _keys = keys;
            _eventBean = eventBean;
        }

        /// <summary>Returns the event. </summary>
        /// <value>event</value>
        public EventBean EventBean
        {
            get { return _eventBean; }
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
            if (other == this)
            {
                return true;
            }
            if (other is MultiKeyUntypedEventPair)
            {
                MultiKeyUntypedEventPair otherKeys = (MultiKeyUntypedEventPair)other;
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
