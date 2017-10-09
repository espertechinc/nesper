///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Type widner that coerces from string to char if required.
    /// </summary>
    public class TypeWidenerObjectArrayToCollectionCoercer
    {
        public static object Widen(Object input)
        {
            return input.Unwrap<object>(true);
        }
    }
} // end of namespace
