///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.advanced.index.service;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public abstract class EventAdvancedIndexFactoryForgeQuadTreeFactory : EventAdvancedIndexFactory
    {
        public AdvancedIndexConfigContextPartition ConfigureContextPartition(
            ExprEvaluatorContext exprEvaluatorContext,
            EventType eventType,
            EventAdvancedIndexProvisionRuntime advancedIndexProvisionDesc,
            EventTableOrganization organization)
        {
            return AdvancedIndexFactoryProviderQuadTree.ConfigureQuadTree(
                organization.IndexName,
                advancedIndexProvisionDesc.ParameterEvaluators,
                exprEvaluatorContext);
        }

        public abstract EventAdvancedIndexFactoryForge Forge { get; }

        public abstract EventTable Make(
            EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configContextPartition,
            EventTableOrganization organization);

        public abstract EventAdvancedIndexConfigStatementForge ToConfigStatement(ExprNode[] indexedExpr);
    }
} // end of namespace