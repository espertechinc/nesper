///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Factory for aggregation multi-function accessors.
    /// </summary>
    public interface AggregationMultiFunctionAccessorFactory
    {
        /// <summary>
        /// Returns the accessor
        /// </summary>
        /// <param name="ctx">contextual information</param>
        /// <returns>accessor</returns>
        AggregationMultiFunctionAccessor NewAccessor(AggregationMultiFunctionAccessorFactoryContext ctx);
    }
} // end of namespace