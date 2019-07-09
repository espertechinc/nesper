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
    /// Marker interface for use with multi-function aggregation to indicate whether
    /// aggregation functions share state
    /// </summary>
    public interface AggregationMultiFunctionStateKey
    {
    }

    public class ProxyAggregationMultiFunctionStateKey : AggregationMultiFunctionStateKey
    {
    }
} // end of namespace