///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanbuild
{
    /// <summary>
    /// Builds a query plan for the simple 2-stream scenario.
    /// </summary>
    public class TwoStreamQueryPlanBuilder
    {
        /// <summary>
        /// Build query plan.
        /// </summary>
        /// <param name="queryGraph">navigability info</param>
        /// <param name="optionalOuterJoinType">outer join type, null if not an outer join</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="streamJoinAnalysisResult">stream join analysis</param>
        /// <returns>query plan</returns>
        public static QueryPlanForgeDesc Build(
            EventType[] typesPerStream,
            QueryGraphForge queryGraph,
            OuterJoinType? optionalOuterJoinType,
            StreamJoinAnalysisResultCompileTime streamJoinAnalysisResult,
            StatementRawInfo raw)
        {
            var uniqueIndexProps = streamJoinAnalysisResult.UniqueKeys;
            var tablesPerStream = streamJoinAnalysisResult.TablesPerStream;
            var additionalForgeable = new List<StmtClassForgeableFactory>();

            var indexSpecs = QueryPlanIndexBuilder.BuildIndexSpec(
                queryGraph,
                typesPerStream,
                uniqueIndexProps);

            var execNodeSpecs = new QueryPlanNodeForge[2];
            var lookupPlans = new TableLookupPlanForge[2];

            // plan lookup from 1 to zero
            var plan1to0 = NStreamQueryPlanBuilder.CreateLookupPlan(
                queryGraph,
                1,
                0,
                streamJoinAnalysisResult.IsVirtualDW(0),
                indexSpecs[0],
                typesPerStream,
                tablesPerStream[0],
                raw,
                SerdeCompileTimeResolverNonHA.INSTANCE);
            lookupPlans[1] = plan1to0.Forge;
            additionalForgeable.AddAll(plan1to0.AdditionalForgeables);

            // plan lookup from zero to 1
            var plan0to1 = NStreamQueryPlanBuilder.CreateLookupPlan(
                queryGraph,
                0,
                1,
                streamJoinAnalysisResult.IsVirtualDW(1),
                indexSpecs[1],
                typesPerStream,
                tablesPerStream[1],
                raw,
                SerdeCompileTimeResolverNonHA.INSTANCE);
            lookupPlans[0] = plan0to1.Forge;
            additionalForgeable.AddAll(plan0to1.AdditionalForgeables);

            execNodeSpecs[0] = new TableLookupNodeForge(lookupPlans[0]);
            execNodeSpecs[1] = new TableLookupNodeForge(lookupPlans[1]);

            if (optionalOuterJoinType != null) {
                if (optionalOuterJoinType.Equals(OuterJoinType.LEFT) ||
                    optionalOuterJoinType.Equals(OuterJoinType.FULL)) {
                    execNodeSpecs[0] = new TableOuterLookupNodeForge(lookupPlans[0]);
                }

                if (optionalOuterJoinType.Equals(OuterJoinType.RIGHT) ||
                    optionalOuterJoinType.Equals(OuterJoinType.FULL)) {
                    execNodeSpecs[1] = new TableOuterLookupNodeForge(lookupPlans[1]);
                }
            }

            var forge = new QueryPlanForge(indexSpecs, execNodeSpecs);
            return new QueryPlanForgeDesc(forge, additionalForgeable);
        }
    }
} // end of namespace