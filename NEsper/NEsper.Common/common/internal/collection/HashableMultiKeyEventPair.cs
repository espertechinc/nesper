///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.collection
{
    /// <summary>
    /// Functions as a key value for Maps where keys need to be composite values, and includes an <seealso cref="EventBean" /> handle
    /// The class allows a Map that uses MultiKeyUntyped entries for key values to use multiple objects as keys.
    /// It calculates the hashCode from the key objects on construction and caches the hashCode.
    /// </summary>
    [Serializable]
    public sealed class HashableMultiKeyEventPair
    {
        [NonSerialized] private readonly object[] keys;
        [NonSerialized] private EventBean eventBean;
	    private readonly int hashCode;

	    /// <summary>
	    /// Constructor for multiple keys supplied in an object array.
	    /// </summary>
	    /// <param name="keys">is an array of key objects</param>
	    /// <param name="eventBean">for pair</param>
	    public HashableMultiKeyEventPair(object[] keys, EventBean eventBean) {
	        if (keys == null) {
	            throw new ArgumentException("The array of keys must not be null");
	        }

	        int total = 0;
	        for (int i = 0; i < keys.Length; i++) {
	            if (keys[i] != null) {
	                total *= 31;
	                total ^= keys[i].GetHashCode();
	            }
	        }

	        this.hashCode = total;
	        this.keys = keys;
	        this.eventBean = eventBean;
	    }

	    /// <summary>
	    /// Returns the event.
	    /// </summary>
	    /// <returns>event</returns>
	    public EventBean EventBean {
	        get => eventBean;	    }

	    /// <summary>
	    /// Returns the number of key objects.
	    /// </summary>
	    /// <returns>size of key object array</returns>
	    public int Size() {
	        return keys.Length;
	    }

	    /// <summary>
	    /// Returns the key object at the specified position.
	    /// </summary>
	    /// <param name="index">is the array position</param>
	    /// <returns>key object at position</returns>
	    public object Get(int index) {
	        return keys[index];
	    }

	    public override bool Equals(object other) {
	        if (other == this) {
	            return true;
	        }
	        if (other is HashableMultiKeyEventPair) {
	            HashableMultiKeyEventPair otherKeys = (HashableMultiKeyEventPair) other;
	            return Arrays.Equals(keys, otherKeys.keys);
	        }
	        return false;
	    }

	    /// <summary>
	    /// Returns keys.
	    /// </summary>
	    /// <returns>keys object array</returns>
	    public object[] GetKeys() {
	        return keys;
	    }

	    public override int GetHashCode() {
	        return hashCode;
	    }

	    public override string ToString() {
	        return "MultiKeyUntyped" + Arrays.AsList(keys).ToString();
	    }

	}
} // end of namespace