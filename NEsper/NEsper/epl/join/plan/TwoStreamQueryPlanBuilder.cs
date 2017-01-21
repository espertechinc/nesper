///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.join.plan
{
	/// <summary>
	/// Builds a query plan for the simple 2-stream scenario.
	/// </summary>
	public class TwoStreamQueryPlanBuilder
	{
        /// <summary>
        /// Build query plan.
        /// </summary>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="queryGraph">navigability info</param>
        /// <param name="optionalOuterJoinType">outer join type, null if not an outer join</param>
        /// <param name="uniqueIndexProps">The unique index props.</param>
        /// <param name="tablesPerStream">The tables per stream.</param>
        /// <returns>
        /// query plan
        /// </returns>
	    public static QueryPlan Build(EventType[] typesPerStream, QueryGraph queryGraph, OuterJoinType? optionalOuterJoinType, string[][][] uniqueIndexProps, TableMetadata[] tablesPerStream)
	    {
	        var indexSpecs = QueryPlanIndexBuilder.BuildIndexSpec(queryGraph, typesPerStream, uniqueIndexProps);

	        var execNodeSpecs = new QueryPlanNode[2];
	        var lookupPlans= new TableLookupPlan[2];

	        // plan lookup from 1 to zero
	        lookupPlans[1] = NStreamQueryPlanBuilder.CreateLookupPlan(queryGraph, 1, 0, indexSpecs[0], typesPerStream, tablesPerStream[0]);

	        // plan lookup from zero to 1
	        lookupPlans[0] = NStreamQueryPlanBuilder.CreateLookupPlan(queryGraph, 0, 1, indexSpecs[1], typesPerStream, tablesPerStream[1]);
	        execNodeSpecs[0] = new TableLookupNode(lookupPlans[0]);
	        execNodeSpecs[1] = new TableLookupNode(lookupPlans[1]);

	        if (optionalOuterJoinType != null)
	        {
	            if ( (optionalOuterJoinType.Equals(OuterJoinType.LEFT)) ||
	                 (optionalOuterJoinType.Equals(OuterJoinType.FULL)) )
	            {
	                execNodeSpecs[0] = new TableOuterLookupNode(lookupPlans[0]);
	            }
	            if ( (optionalOuterJoinType.Equals(OuterJoinType.RIGHT)) ||
	                 (optionalOuterJoinType.Equals(OuterJoinType.FULL)) )
	            {
	                execNodeSpecs[1] = new TableOuterLookupNode(lookupPlans[1]);
	            }
	        }

	        return new QueryPlan(indexSpecs, execNodeSpecs);
	    }
	}
} // end of namespace
