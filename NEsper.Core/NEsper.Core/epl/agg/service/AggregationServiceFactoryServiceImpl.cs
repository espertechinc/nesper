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
    public class AggregationServiceFactoryServiceImpl : AggregationServiceFactoryService
    {
        public static readonly AggregationServiceFactoryService DEFAULT_FACTORY =
            new AggregationServiceFactoryServiceImpl();

        public AggregationServiceFactory GetNullAggregationService()
        {
            return AggregationServiceNullFactory.AGGREGATION_SERVICE_NULL_FACTORY;
        }

        public AggregationServiceFactory GetNoGroupNoAccess(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupAllNoAccessFactory(evaluatorsArr, aggregatorsArr);
        }

        public AggregationServiceFactory GetNoGroupAccessOnly(
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggSpecs,
            bool join,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupAllAccessOnlyFactory(pairs, accessAggSpecs, join);
        }

        public AggregationServiceFactory GetNoGroupAccessMixed(
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupAllMixedAccessFactory(
                evaluatorsArr, aggregatorsArr, pairs, accessAggregations, join);
        }

        public AggregationServiceFactory GetGroupedNoReclaimNoAccess(
            ExprNode[] groupByNodes,
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByNoAccessFactory(evaluatorsArr, aggregatorsArr);
        }

        public AggregationServiceFactory GetGroupNoReclaimAccessOnly(
            ExprNode[] groupByNodes,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggSpecs,
            bool @join,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByAccessOnlyFactory(pairs, accessAggSpecs, join);
        }

        public AggregationServiceFactory GetGroupNoReclaimMixed(
            ExprNode[] groupByNodes,
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool @join,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByMixedAccessFactory(
                evaluatorsArr, aggregatorsArr, pairs, accessAggregations, join);
        }

        public AggregationServiceFactory GetGroupReclaimAged(
            ExprNode[] groupByNodes,
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            HintAttribute reclaimGroupAged,
            HintAttribute reclaimGroupFrequency,
            VariableService variableService,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool @join,
            string optionalContextName,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByReclaimAgedFactory(
                evaluatorsArr, aggregatorsArr, reclaimGroupAged, reclaimGroupFrequency, variableService,
                pairs, accessAggregations, join, optionalContextName);
        }

        public AggregationServiceFactory GetGroupReclaimNoAccess(
            ExprNode[] groupByNodes,
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool @join,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByRefcountedNoAccessFactory(evaluatorsArr, aggregatorsArr);
        }

        public AggregationServiceFactory GetGroupReclaimMixable(
            ExprNode[] groupByNodes,
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool @join,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByRefcountedWAccessFactory(
                evaluatorsArr, aggregatorsArr, pairs, accessAggregations, join);
        }

        public AggregationServiceFactory GetGroupReclaimMixableRollup(
            ExprNode[] groupByNodes,
            AggregationGroupByRollupDesc byRollupDesc,
            ExprEvaluator[] evaluatorsArr,
            AggregationMethodFactory[] aggregatorsArr,
            AggregationAccessorSlotPair[] pairs,
            AggregationStateFactory[] accessAggregations,
            bool join,
            AggregationGroupByRollupDesc groupByRollupDesc,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByRefcountedWAccessRollupFactory(
                evaluatorsArr, aggregatorsArr, pairs, accessAggregations, join, groupByRollupDesc);
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
            return new AggSvcGroupByWTableFactory(
                tableMetadata, methodPairs, accessorPairs, join, targetStates, accessStateExpr, agents,
                groupByRollupDesc);
        }

        public AggregationServiceFactory GetNoGroupWBinding(
            AggregationAccessorSlotPair[] accessors,
            bool join,
            TableColumnMethodPair[] methodPairs,
            string tableName,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents)
        {
            return new AggSvcGroupAllMixedAccessWTableFactory(
                accessors, join, methodPairs, tableName, targetStates, accessStateExpr, agents);
        }

        public AggregationServiceFactory GetNoGroupLocalGroupBy(
            bool @join,
            AggregationLocalGroupByPlan localGroupByPlan,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupAllLocalGroupByFactory(join, localGroupByPlan);
        }

        public AggregationServiceFactory GetGroupLocalGroupBy(
            bool @join,
            AggregationLocalGroupByPlan localGroupByPlan,
            bool isUnidirectional,
            bool isFireAndForget,
            bool isOnSelect)
        {
            return new AggSvcGroupByLocalGroupByFactory(join, localGroupByPlan);
        }
    }
} // end of namespace
