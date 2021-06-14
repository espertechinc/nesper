///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public interface AuditProviderPattern
    {
        void PatternTrue(
            EvalFactoryNode factoryNode,
            object from,
            MatchedEventMapMinimal matchEvent,
            bool isQuitted,
            AgentInstanceContext agentInstanceContext);

        void PatternFalse(
            EvalFactoryNode factoryNode,
            object from,
            AgentInstanceContext agentInstanceContext);
    }
} // end of namespace