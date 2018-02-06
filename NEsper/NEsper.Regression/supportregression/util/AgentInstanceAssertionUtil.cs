///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.stmt;
using com.espertech.esper.core.service;
using NUnit.Framework;

namespace com.espertech.esper.supportregression.util
{
    public class AgentInstanceAssertionUtil {
    
        public static void AssertInstanceCounts(StatementContext context, int numAggregations) {
            AssertInstanceCounts(context, numAggregations, 0, 0, 0);
        }
    
        public static void AssertInstanceCounts(StatementContext context, int numAggregations, int numSubselect, int numPrev, int numPrior) {
            StatementAIResourceRegistry registry = context.StatementAgentInstanceRegistry;
            Assert.AreEqual(numAggregations, registry.AgentInstanceAggregationService.InstanceCount);
            Assert.AreEqual(numSubselect, registry.AgentInstanceExprService.SubselectAgentInstanceCount);
            Assert.AreEqual(numPrev, registry.AgentInstanceExprService.PreviousAgentInstanceCount);
            Assert.AreEqual(numPrior, registry.AgentInstanceExprService.PriorAgentInstanceCount);
        }
    }
}
