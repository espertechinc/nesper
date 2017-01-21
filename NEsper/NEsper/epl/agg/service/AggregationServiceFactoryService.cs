///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.annotation;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.agg.service
{
	public interface AggregationServiceFactoryService
    {
	    AggregationServiceFactory GetNullAggregationService();
	    AggregationServiceFactory GetNoGroupNoAccess(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetNoGroupAccessOnly(AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggSpecs, bool join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetNoGroupAccessMixed(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupedNoReclaimNoAccess(ExprNode[] groupByNodes, ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupNoReclaimAccessOnly(ExprNode[] groupByNodes, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggSpecs, bool @join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupNoReclaimMixed(ExprNode[] groupByNodes, ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool @join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupReclaimAged(ExprNode[] groupByNodes, ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, HintAttribute reclaimGroupAged, HintAttribute reclaimGroupFrequency, VariableService variableService, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool @join, string optionalContextName, bool isUnidirectional, bool isFireAndForget, bool isOnSelect) ;
	    AggregationServiceFactory GetGroupReclaimNoAccess(ExprNode[] groupByNodes, ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool @join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupReclaimMixable(ExprNode[] groupByNodes, ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool @join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
        AggregationServiceFactory GetGroupReclaimMixableRollup(ExprNode[] groupByNodes, AggregationGroupByRollupDesc byRollupDesc, ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, AggregationGroupByRollupDesc groupByRollupDesc, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupWBinding(TableMetadata tableMetadata, TableColumnMethodPair[] methodPairs, AggregationAccessorSlotPair[] accessorPairs, bool join, IntoTableSpec bindings, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents, AggregationGroupByRollupDesc groupByRollupDesc);
	    AggregationServiceFactory GetNoGroupWBinding(AggregationAccessorSlotPair[] accessors, bool join, TableColumnMethodPair[] methodPairs, string tableName, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents);
	    AggregationServiceFactory GetNoGroupLocalGroupBy(bool @join, AggregationLocalGroupByPlan localGroupByPlan, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupLocalGroupBy(bool @join, AggregationLocalGroupByPlan localGroupByPlan, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	}
} // end of namespace
