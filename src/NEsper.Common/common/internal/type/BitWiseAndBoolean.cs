///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Computer for type-specific arith. operations.
    /// </summary>
    /// <summary>
    ///     Bit Wise And.
    /// </summary>
    public class BitWiseAndBoolean : BitWiseComputer
    {
        public object Compute(
            object objOne,
            object objTwo)
        {
            var b1 = (bool)objOne;
            var b2 = (bool)objTwo;
            return b1 & b2;
        }
    }
}