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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationServiceFactoryServiceImpl : AggregationServiceFactoryService
    {
        public static AggregationServiceFactoryService DEFAULT_FACTORY = new AggregationServiceFactoryServiceImpl();

        public AggregationServiceFactory GetNullAggregationService()
        {
            return AggregationServiceNullFactory.AGGREGATION_SERVICE_NULL_FACTORY;
        }

        public AggregationServiceFactory GetNoGroupNoAccess(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr)
        {
            return new AggSvcGroupAllNoAccessFactory(evaluatorsArr, aggregatorsArr, null);
        }

        public AggregationServiceFactory GetNoGroupAccessOnly(
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggSpecs,
            bool join)
        {
            return new AggSvcGroupAllAccessOnlyFactory(pairs, accessAggSpecs, join);
        }

        public AggregationServiceFactory GetNoGroupAccessMixed(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join)
        {
            return new AggSvcGroupAllMixedAccessFactory(
                evaluatorsArr, aggregatorsArr, null, pairs, accessAggregations, join);
        }

        public AggregationServiceFactory GetGroupedNoReclaimNoAccess(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            Object groupKeyBinding)
        {
            return new AggSvcGroupByNoAccessFactory(evaluatorsArr, aggregatorsArr, groupKeyBinding);
        }

        public AggregationServiceFactory GetGroupNoReclaimAccessOnly(
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggSpecs,
            Object groupKeyBinding,
            bool join)
        {
            return new AggSvcGroupByAccessOnlyFactory(pairs, accessAggSpecs, groupKeyBinding, join);
        }

        public AggregationServiceFactory GetGroupNoReclaimMixed(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join,
            Object groupKeyBinding)
        {
            return new AggSvcGroupByMixedAccessFactory(
                evaluatorsArr, aggregatorsArr, groupKeyBinding, pairs, accessAggregations, join);
        }

        public AggregationServiceFactory GetGroupReclaimAged(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            HintAttribute reclaimGroupAged,
            HintAttribute reclaimGroupFrequency,
            VariableService variableService,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join,
            Object groupKeyBinding,
            String optionalContextName)
        {
            return new AggSvcGroupByReclaimAgedFactory(
                evaluatorsArr, aggregatorsArr, groupKeyBinding, reclaimGroupAged, reclaimGroupFrequency, variableService,
                pairs, accessAggregations, join, optionalContextName);
        }

        public AggregationServiceFactory GetGroupReclaimNoAccess(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join,
            Object groupKeyBinding)
        {
            return new AggSvcGroupByRefcountedNoAccessFactory(evaluatorsArr, aggregatorsArr, groupKeyBinding);
        }

        public AggregationServiceFactory GetGroupReclaimMixable(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join,
            Object groupKeyBinding)
        {
            return new AggSvcGroupByRefcountedWAccessFactory(
                evaluatorsArr, aggregatorsArr, groupKeyBinding, pairs, accessAggregations, join);
        }

        public AggregationServiceFactory GetGroupReclaimMixableRollup(
            ExprEvaluator[] evaluatorsArr, 
            AggregationMethodFactory[] aggregatorsArr, 
            AggregationAccessorSlotPair[] pairs, 
            AggregationStateFactory[] accessAggregations, 
            bool join, 
            Object groupKeyBinding, 
            AggregationGroupByRollupDesc groupByRollupDesc)
        {
            return new AggSvcGroupByRefcountedWAccessRollupFactory(evaluatorsArr, aggregatorsArr, groupKeyBinding, pairs, accessAggregations, join, groupByRollupDesc);
        }

        public AggregationServiceFactory GetGroupWBinding(
            TableMetadata tableMetadata,
            TableColumnMethodPair[] methodPairs,
            AggregationAccessorSlotPair[] accessorPairs,
            bool join,
            IntoTableSpec bindings,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents,
            AggregationGroupByRollupDesc groupByRollupDesc)
        {
            return new AggSvcGroupByWTableFactory(tableMetadata, methodPairs, accessorPairs, join, targetStates, accessStateExpr, agents, groupByRollupDesc);
        }

        public AggregationServiceFactory GetNoGroupWBinding(
            AggregationAccessorSlotPair[] accessors,
            bool join,
            TableColumnMethodPair[] methodPairs,
            String tableName,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents)
        {
            return new AggSvcGroupAllMixedAccessWTableFactory(
                accessors, join, methodPairs, tableName, targetStates, accessStateExpr, agents);
        }

        public AggregationServiceFactory GetNoGroupLocalGroupBy(
            bool join,
            AggregationLocalGroupByPlan localGroupByPlan,
            Object groupKeyBinding)
        {
            return new AggSvcGroupAllLocalGroupByFactory(join, localGroupByPlan, groupKeyBinding);
        }

        public AggregationServiceFactory GetGroupLocalGroupBy(
            bool join,
            AggregationLocalGroupByPlan localGroupByPlan,
            Object groupKeyBinding)
        {
            return new AggSvcGroupByLocalGroupByFactory(join, localGroupByPlan, groupKeyBinding);
        }
    }
}