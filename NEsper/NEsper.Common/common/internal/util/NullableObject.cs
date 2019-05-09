///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// A generic class to hold an object that may itself be a null value versus an
    /// undefined (not existing) value.
    /// <para/>
    /// The presence of a reference indicates that a value exists, the absence of a reference
    /// to this object indicates that there is no value (similar to a Pair&lt;Object, bool&gt;).
    ///  </summary>
    [Serializable]
    public class NullableObject<T>
    {
        /// <summary>Ctor. </summary>
        /// <param name="value">the value to contain</param>
        public NullableObject(T value)
        {
            Value = value;
        }

        /// <summary>Returns the contained value. </summary>
        /// <returns>contained value</returns>
        public T Value { get; set; }
    }
}