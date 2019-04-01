///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.quadtree
{
    public abstract class EventAdvancedIndexFactoryQuadTree : EventAdvancedIndexFactory {
    
        public AdvancedIndexConfigContextPartition ConfigureContextPartition(
            EventType eventType, 
            AdvancedIndexDesc indexDesc, 
            ExprEvaluator[] parameters, 
            ExprEvaluatorContext exprEvaluatorContext, 
            EventTableOrganization organization, 
            EventAdvancedIndexConfigStatement advancedIndexConfigStatement)
        {
            return AdvancedIndexFactoryProviderQuadTree.ConfigureQuadTree(organization.IndexName, parameters, exprEvaluatorContext);
        }
    
        public SubordTableLookupStrategyFactoryQuadTree GetSubordinateLookupStrategy(
            string operationName, IDictionary<int, ExprNode> positionalExpressions, bool isNWOnTrigger, int numOuterstreams)
        {
            var x = positionalExpressions[0].ExprEvaluator;
            var y = positionalExpressions[1].ExprEvaluator;
            var width = positionalExpressions[2].ExprEvaluator;
            var height = positionalExpressions[3].ExprEvaluator;
            var expressions = new string[positionalExpressions.Count];
            foreach (var entry in positionalExpressions) {
                expressions[entry.Key] = entry.Value.ToExpressionStringMinPrecedenceSafe();
            }
            var lookupStrategyDesc = new LookupStrategyDesc(LookupStrategyType.ADVANCED, expressions);
            return new SubordTableLookupStrategyFactoryQuadTree(x, y, width, height, isNWOnTrigger, numOuterstreams, lookupStrategyDesc);
        }

        public abstract EventTable Make(EventAdvancedIndexConfigStatement configStatement,
            AdvancedIndexConfigContextPartition configContextPartition, EventTableOrganization organization);

        public abstract bool ProvidesIndexForOperation(string operationName, IDictionary<int, ExprNode> expressions);
    }
} // end of namespace
