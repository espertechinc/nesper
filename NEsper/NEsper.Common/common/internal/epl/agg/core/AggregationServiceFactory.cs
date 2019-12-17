///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    /// Factory for aggregation service instances.
    /// <para />Consolidates aggregation nodes such that result futures point to a single instance and
    /// no re-evaluation of the same result occurs.
    /// </summary>
    public interface AggregationServiceFactory
    {
        AggregationService MakeService(
            AgentInstanceContext agentInstanceContext,
            ImportServiceRuntime importService,
            bool isSubquery,
            int? subqueryNumber,
            int[] groupId);
    }
} // end of namespace