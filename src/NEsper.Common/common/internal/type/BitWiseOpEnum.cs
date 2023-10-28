///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    ///     Enum representing relational types of operation.
    /// </summary>
    [Serializable]
    public enum BitWiseOpEnum
    {
        /// <summary>
        ///     Bitwise and.
        /// </summary>
        BAND,

        /// <summary>
        ///     Bitwise or.
        /// </summary>
        BOR,

        /// <summary>
        ///     Bitwise xor.
        /// </summary>
        BXOR
    }
} // end of namespace