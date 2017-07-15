///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.util
{
    /// <summary>Interface for number coercion.</summary>
    public interface SimpleNumberCoercer {
        /// <summary>
        /// Coerce the given number to a previously determined type, assuming the type is a Boxed type. Allows coerce to lower resultion number.
        /// Does't coerce to primitive types.
        /// </summary>
        /// <param name="numToCoerce">is the number to coerce to the given type</param>
        /// <returns>the numToCoerce as a value in the given result type</returns>
        public Number CoerceBoxed(Number numToCoerce);
    
        public Type GetReturnType();
    }
} // end of namespace
