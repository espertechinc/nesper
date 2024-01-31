///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.context
{
    public class AgentInstanceAssertionUtil
    {
        public static void AssertInstanceCounts(
            RegressionEnvironment env,
            string statementName,
            int numAggregations)
        {
            var stmt = (EPStatementSPI) env.Statement(statementName);
            if (stmt == null) {
                Assert.Fail("Statement not found '" + statementName + "'");
            }

            AssertInstanceCounts(env, stmt.StatementContext, numAggregations, null, null, null);
        }

        public static void AssertInstanceCounts(
            RegressionEnvironment env,
            string statementName,
            int? numAggregations,
            int? numSubselect,
            int? numPrev,
            int? numPrior)
        {
            var stmt = (EPStatementSPI) env.Statement(statementName);
            if (stmt == null) {
                Assert.Fail("Statement not found '" + statementName + "'");
            }

            AssertInstanceCounts(env, stmt.StatementContext, numAggregations, numSubselect, numPrev, numPrior);
        }

        private static void AssertInstanceCounts(
            RegressionEnvironment env,
            StatementContext context,
            int? numAggregations,
            int? numSubselect,
            int? numPrev,
            int? numPrior)
        {
            if (env.IsHA_Releasing) {
                return;
            }

            var registry = context.StatementAIResourceRegistry;
            if (numAggregations != null) {
                ClassicAssert.AreEqual((int) numAggregations, registry.AgentInstanceAggregationService.InstanceCount);
            }
            else {
                ClassicAssert.IsNull(registry.AgentInstanceAggregationService);
            }

            if (numSubselect != null) {
                ClassicAssert.AreEqual((int) numSubselect, registry.AgentInstanceSubselects[0].LookupStrategies.InstanceCount);
            }
            else {
                ClassicAssert.IsNull(registry.AgentInstanceSubselects);
            }

            if (numPrev != null) {
                ClassicAssert.AreEqual((int) numPrev, registry.AgentInstancePreviousGetterStrategies[0].InstanceCount);
            }
            else {
                ClassicAssert.IsNull(registry.AgentInstancePreviousGetterStrategies);
            }

            if (numPrior != null) {
                ClassicAssert.AreEqual((int) numPrior, registry.AgentInstancePriorEvalStrategies[0].InstanceCount);
            }
            else {
                ClassicAssert.IsNull(registry.AgentInstancePriorEvalStrategies);
            }
        }
    }
} // end of namespace