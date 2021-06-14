///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public interface EventAdvancedIndexFactory
    {
        EventAdvancedIndexFactoryForge Forge { get; }

        AdvancedIndexConfigContextPartition ConfigureContextPartition(
            AgentInstanceContext agentInstanceContext,
            EventType eventType,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc,
            EventTableOrganization organization);

        EventTable Make(
            EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configContextPartition,
            EventTableOrganization organization);

        EventAdvancedIndexConfigStatementForge ToConfigStatement(ExprNode[] indexedExpr);
    }
} // end of namespace