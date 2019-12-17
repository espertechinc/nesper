///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Factory for aggregation multi-function agents
    /// </summary>
    public interface AggregationMultiFunctionAgentFactory
    {
        /// <summary>
        /// Returns a new agent
        /// </summary>
        /// <param name="ctx">contextual information</param>
        /// <returns>agent</returns>
        AggregationMultiFunctionAgent NewAgent(AggregationMultiFunctionAgentFactoryContext ctx);
    }
} // end of namespace