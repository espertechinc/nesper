///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Casts an object to another type, typically for numeric types.
    /// <para/>
    /// May performs a compatibility check and returns null if not compatible.
    /// </summary>
    /// <param name="value">to cast</param>
    /// <returns>casted or transformed object, possibly the same, or null if the cast cannot be made</returns>
    public delegate Object SimpleTypeCaster(Object value);
}
