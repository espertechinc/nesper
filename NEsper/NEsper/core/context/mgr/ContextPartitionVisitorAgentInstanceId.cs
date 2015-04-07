///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextPartitionVisitorAgentInstanceId : ContextPartitionVisitor
    {
        private readonly List<int> _agentInstanceIds = new List<int>();
        private readonly int _maxNestingLevel;

        public ContextPartitionVisitorAgentInstanceId(int maxNestingLevel)
        {
            _maxNestingLevel = maxNestingLevel;
        }

        public IList<int> AgentInstanceIds
        {
            get { return _agentInstanceIds; }
        }

        public void Visit(
            int nestingLevel,
            int pathId,
            ContextStatePathValueBinding binding,
            Object payload,
            ContextController contextController,
            ContextControllerInstanceHandle instanceHandle)
        {
            if (nestingLevel == _maxNestingLevel)
            {
                _agentInstanceIds.Add(instanceHandle.ContextPartitionOrPathId);
            }
        }
    }
}