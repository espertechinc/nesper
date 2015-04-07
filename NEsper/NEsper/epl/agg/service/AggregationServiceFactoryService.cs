///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.annotation;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.agg.service
{
    public interface AggregationServiceFactoryService
    {
        AggregationServiceFactory GetNullAggregationService();
        AggregationServiceFactory GetNoGroupNoAccess(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr);
        AggregationServiceFactory GetNoGroupAccessOnly(AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggSpecs, bool join);
        AggregationServiceFactory GetNoGroupAccessMixed(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join);
        AggregationServiceFactory GetGroupedNoReclaimNoAccess(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, Object groupKeyBinding);
        AggregationServiceFactory GetGroupNoReclaimAccessOnly(AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggSpecs, Object groupKeyBinding, bool join);
        AggregationServiceFactory GetGroupNoReclaimMixed(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, Object groupKeyBinding);
        AggregationServiceFactory GetGroupReclaimAged(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, HintAttribute reclaimGroupAged, HintAttribute reclaimGroupFrequency, VariableService variableService, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, Object groupKeyBinding, String optionalContextName);
        AggregationServiceFactory GetGroupReclaimNoAccess(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, Object groupKeyBinding);
        AggregationServiceFactory GetGroupReclaimMixable(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, Object groupKeyBinding);
        AggregationServiceFactory GetGroupReclaimMixableRollup(ExprEvaluator[] evaluatorsArr, AggregationMethodFactory[] aggregatorsArr, AggregationAccessorSlotPair[] pairs, AggregationStateFactory[] accessAggregations, bool join, Object groupKeyBinding, AggregationGroupByRollupDesc groupByRollupDesc);
        AggregationServiceFactory GetGroupWBinding(TableMetadata tableMetadata, TableColumnMethodPair[] methodPairs, AggregationAccessorSlotPair[] accessorPairs, bool join, IntoTableSpec bindings, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents, AggregationGroupByRollupDesc groupByRollupDesc);
        AggregationServiceFactory GetNoGroupWBinding(AggregationAccessorSlotPair[] accessors, bool join, TableColumnMethodPair[] methodPairs, String tableName, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents);
        AggregationServiceFactory GetNoGroupLocalGroupBy(bool join, AggregationLocalGroupByPlan localGroupByPlan, Object groupKeyBinding);
        AggregationServiceFactory GetGroupLocalGroupBy(bool join, AggregationLocalGroupByPlan localGroupByPlan, Object groupKeyBinding);
    }
}
