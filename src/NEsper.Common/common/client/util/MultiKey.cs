///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Interface for use with multi-keys made up of multiple values and providing hashcode and equals semantics
    /// </summary>
    public interface MultiKey
    {
        /// <summary>
        ///     Returns the number of keys available
        /// </summary>
        /// <returns>number of keys</returns>
        int NumKeys { get; }

        /// <summary>
        ///     Returns the key value at the given index
        /// </summary>
        /// <param name="num">key number</param>
        /// <returns>key value</returns>
        object GetKey(int num);
    }
    
    public static class MultiKeyExtensions {
        /// <summary>
        ///     Convert the multi-key to an object array
        /// </summary>
        /// <param name="mk">to convert</param>
        /// <returns>object-array</returns>
        public static object[] ToObjectArray(this MultiKey mk)
        {
            var keys = new object[mk.NumKeys];
            for (var i = 0; i < keys.Length; i++) {
                keys[i] = mk.GetKey(i);
            }

            return keys;
        }
    }
}