///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Property getter for Objectarray-underlying events.
    /// </summary>
    public interface ObjectArrayEventPropertyGetter : EventPropertyGetterSPI
    {
        /// <summary>Returns a property of an event. </summary>
        /// <param name="array">to interrogate</param>
        /// <returns>property value</returns>
        /// <throws>com.espertech.esper.client.PropertyAccessException for property access errors</throws>
        Object GetObjectArray(Object[] array);
    
        /// <summary>Exists-function for properties in a object array-type event. </summary>
        /// <param name="array">to interrogate</param>
        /// <returns>indicator</returns>
        bool IsObjectArrayExistsProperty(Object[] array);
    }
}
