using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    ///////////////////////////////////////////////////////////////////////////////////////
    // Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
    // http://esper.codehaus.org                                                          /
    // ---------------------------------------------------------------------------------- /
    // The software in this package is published under the terms of the GPL license       /
    // a copy of which has been included with this distribution in the license.txt file.  /
    ///////////////////////////////////////////////////////////////////////////////////////

    /*
     ***************************************************************************************
     *  Copyright (C) 2006 EsperTech, Inc. All rights reserved.                            *
     *  http://www.espertech.com/esper                                                     *
     *  http://www.espertech.com                                                           *
     *  ---------------------------------------------------------------------------------- *
     *  The software in this package is published under the terms of the GPL license       *
     *  a copy of which has been included with this distribution in the license.txt file.  *
     ***************************************************************************************
     */

    /// <summary>
    /// Factory for aggregation multi-function table readers
    /// </summary>
    public interface AggregationMultiFunctionTableReaderFactory
    {
        /// <summary>
        /// Returns a new table reader
        /// </summary>
        /// <param name="context">contextual information</param>
        /// <returns>table reader</returns>
        AggregationMultiFunctionTableReader NewReader(AggregationMultiFunctionTableReaderFactoryContext context);
    }
} // end of namespace