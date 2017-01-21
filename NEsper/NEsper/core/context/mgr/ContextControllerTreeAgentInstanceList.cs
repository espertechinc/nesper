///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.context;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerTreeAgentInstanceList {
    
        private readonly long _filterVersionAfterAllocation;
        private readonly Object _initPartitionKey;
        private readonly IDictionary<String, Object> _initContextProperties;
        private readonly IList<AgentInstance> _agentInstances;
        private ContextPartitionState _state;

        public ContextControllerTreeAgentInstanceList(long filterVersionAfterAllocation, Object initPartitionKey, IDictionary<string, object> initContextProperties, IList<AgentInstance> agentInstances, ContextPartitionState state)
        {
            _filterVersionAfterAllocation = filterVersionAfterAllocation;
            _initPartitionKey = initPartitionKey;
            _initContextProperties = initContextProperties;
            _agentInstances = agentInstances;
            _state = state;
        }

        public long FilterVersionAfterAllocation
        {
            get { return _filterVersionAfterAllocation; }
        }

        public object InitPartitionKey
        {
            get { return _initPartitionKey; }
        }

        public IDictionary<string, object> InitContextProperties
        {
            get { return _initContextProperties; }
        }

        public IList<AgentInstance> AgentInstances
        {
            get { return _agentInstances; }
        }

        public ContextPartitionState State
        {
            get { return _state; }
            set { _state = value; }
        }

        public void ClearAgentInstances() {
            _agentInstances.Clear();
        }
    }
}
