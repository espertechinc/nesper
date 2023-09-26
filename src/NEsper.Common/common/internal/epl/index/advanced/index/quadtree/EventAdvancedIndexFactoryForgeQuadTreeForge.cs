///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public abstract class EventAdvancedIndexFactoryForgeQuadTreeForge : EventAdvancedIndexFactoryForge
    {
        public AdvancedIndexConfigContextPartition ConfigureContextPartition(
            EventType eventType,
            AdvancedIndexDescWExpr indexDesc,
            ExprEvaluator[] parameters,
            ExprEvaluatorContext exprEvaluatorContext,
            EventTableOrganization organization,
            EventAdvancedIndexConfigStatementForge advancedIndexConfigStatement)
        {
            return AdvancedIndexFactoryProviderQuadTree.ConfigureQuadTree(
                organization.IndexName,
                parameters,
                exprEvaluatorContext);
        }

        public SubordTableLookupStrategyFactoryQuadTreeForge GetSubordinateLookupStrategy(
            string operationName,
            IDictionary<int, ExprNode> positionalExpressions,
            bool isNWOnTrigger,
            int numOuterstreams)
        {
            var x = positionalExpressions.Get(0).Forge;
            var y = positionalExpressions.Get(1).Forge;
            var width = positionalExpressions.Get(2).Forge;
            var height = positionalExpressions.Get(3).Forge;
            var expressions = new string[positionalExpressions.Count];
            foreach (var entry in positionalExpressions) {
                expressions[entry.Key] = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(entry.Value);
            }

            var lookupStrategyDesc = new LookupStrategyDesc(LookupStrategyType.ADVANCED, expressions);
            return new SubordTableLookupStrategyFactoryQuadTreeForge(
                x,
                y,
                width,
                height,
                isNWOnTrigger,
                numOuterstreams,
                lookupStrategyDesc);
        }

        public abstract EventAdvancedIndexFactory RuntimeFactory { get; }

        public abstract bool ProvidesIndexForOperation(string operationName);

        public abstract CodegenExpression CodegenMake(
            CodegenMethodScope parent,
            CodegenClassScope classScope);
    }
} // end of namespace