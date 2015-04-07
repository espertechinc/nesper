///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Factory for aggregation service instances.
    /// <para />
    /// Consolidates aggregation nodes such that result futures point to a single instance
    /// and no re-evaluation of the same result occurs.
    /// </summary>
    public interface AggregationServiceMatchRecognizeFactory
    {
        AggregationServiceMatchRecognize MakeService(AgentInstanceContext agentInstanceContext);
    }
}
