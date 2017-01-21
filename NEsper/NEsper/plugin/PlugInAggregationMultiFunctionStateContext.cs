///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Context for use with <seealso cref="PlugInAggregationMultiFunctionStateFactory"/> 
    /// provides contextual information at the time an aggregation state is allocated.
    /// </summary>
    public class PlugInAggregationMultiFunctionStateContext
    {
        private readonly int _agentInstanceId;
        private readonly Object _groupKey;
    
        /// <summary>Ctor. </summary>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="groupKey">group key, or null if there are no group-by criteria</param>
        public PlugInAggregationMultiFunctionStateContext(int agentInstanceId, Object groupKey) 
        {
            _agentInstanceId = agentInstanceId;
            _groupKey = groupKey;
        }

        /// <summary>Returns the agent instance id. </summary>
        /// <value>context partition id</value>
        public int AgentInstanceId
        {
            get { return _agentInstanceId; }
        }

        /// <summary>Returns the group key or null if no group-by criteria are defined </summary>
        /// <value>group key</value>
        public object GroupKey
        {
            get { return _groupKey; }
        }
    }
}
