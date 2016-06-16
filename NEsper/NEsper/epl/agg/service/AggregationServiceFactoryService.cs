///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	    AggregationServiceFactory GetGroupedNoReclaimNoAccess(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, object groupKeyBinding, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupNoReclaimAccessOnly(AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggSpecs, object groupKeyBinding, bool join, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupNoReclaimMixed(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, object groupKeyBinding, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupReclaimAged(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, HintAttribute reclaimGroupAged, HintAttribute reclaimGroupFrequency, VariableService variableService, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, object groupKeyBinding, string optionalContextName, bool isUnidirectional, bool isFireAndForget, bool isOnSelect) ;
	    AggregationServiceFactory GetGroupReclaimNoAccess(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, object groupKeyBinding, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupReclaimMixable(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, object groupKeyBinding, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupReclaimMixableRollup(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, object groupKeyBinding, AggregationGroupByRollupDesc groupByRollupDesc, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupWBinding(TableMetadata tableMetadata, TableColumnMethodPair[] methodPairs, AggregationAccessorSlotPair[] accessorPairs, bool join, IntoTableSpec bindings, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents, AggregationGroupByRollupDesc groupByRollupDesc);
	    AggregationServiceFactory GetNoGroupWBinding(AggregationAccessorSlotPair[] accessors, bool join, TableColumnMethodPair[] methodPairs, string tableName, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents);
	    AggregationServiceFactory GetNoGroupLocalGroupBy(bool join, AggregationLocalGroupByPlan localGroupByPlan, object groupKeyBinding, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	    AggregationServiceFactory GetGroupLocalGroupBy(bool join, AggregationLocalGroupByPlan localGroupByPlan, object groupKeyBinding, bool isUnidirectional, bool isFireAndForget, bool isOnSelect);
	}
} // end of namespace
