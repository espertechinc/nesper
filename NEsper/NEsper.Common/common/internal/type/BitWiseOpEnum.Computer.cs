///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.type
{
    public partial class BitWiseOpEnum
    {
        /// <summary>
        ///     Computer for relational op.
        /// </summary>
        public interface Computer
        {
            /// <summary>
            ///     Computes using the 2 numbers or boolean a result object.
            /// </summary>
            /// <param name="objOne">is the first number or boolean</param>
            /// <param name="objTwo">is the second number or boolean</param>
            /// <returns>result</returns>
            object Compute(
                object objOne,
                object objTwo);
        }
    }
}