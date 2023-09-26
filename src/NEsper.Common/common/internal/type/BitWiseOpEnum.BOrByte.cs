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
        ///     Bit Wise Or.
        /// </summary>
        public class BOrByte : Computer
        {
            public object Compute(
                object objOne,
                object objTwo)
            {
                var n1 = (byte)objOne;
                var n2 = (byte)objTwo;
                return (byte)(n1 | n2);
            }
        }
    }
}