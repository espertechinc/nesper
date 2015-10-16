///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.rowregex;

namespace com.espertech.esper.core.start
{
    public class EPStatementStartMethodHelperAssignExpr
    {
        public static void AssignExpressionStrategies(
            EPStatementStartMethodSelectDesc selectDesc,
            AggregationService aggregationService,
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategyInstances,
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorStrategyInstances,
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousStrategyInstances,
            ICollection<ExprPreviousMatchRecognizeNode> matchRecognizeNodes,
            RegexExprPreviousEvalStrategy matchRecognizePrevEvalStrategy,
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategyInstances)
        {
            // initialize aggregation expression nodes
            if (selectDesc.ResultSetProcessorPrototypeDesc.AggregationServiceFactoryDesc != null && aggregationService != null)
            {
                EPStatementStartMethodHelperAssignExpr.AssignAggregations(aggregationService, selectDesc.ResultSetProcessorPrototypeDesc.AggregationServiceFactoryDesc.Expressions);
            }
    
            // assign subquery nodes
            AssignSubqueryStrategies(selectDesc.SubSelectStrategyCollection, subselectStrategyInstances);
    
            // assign prior nodes
            AssignPriorStrategies(priorStrategyInstances);
    
            // assign previous nodes
            AssignPreviousStrategies(previousStrategyInstances);
    
            // assign match-recognize previous nodes
            AssignMatchRecognizePreviousStrategies(matchRecognizeNodes, matchRecognizePrevEvalStrategy);

            // assign table access nodes
            AssignTableAccessStrategies(tableAccessStrategyInstances);
        }
    
        public static void AssignTableAccessStrategies(IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategies)
        {
            foreach (var pair in tableAccessStrategies)
            {
                pair.Key.Strategy = pair.Value;
            }
        }

        public static void AssignMatchRecognizePreviousStrategies(IEnumerable<ExprPreviousMatchRecognizeNode> matchRecognizeNodes, RegexExprPreviousEvalStrategy strategy) {
            if (matchRecognizeNodes != null && strategy != null) {
                foreach (var node in matchRecognizeNodes) {
                    node.Strategy = strategy;
                }
            }
        }
    
        public static void AssignAggregations(AggregationResultFuture aggregationService, IList<AggregationServiceAggExpressionDesc> aggregationExpressions) {
            foreach (var aggregation in aggregationExpressions) {
                aggregation.AssignFuture(aggregationService);
            }
        }
    
        public static void AssignPreviousStrategies(IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousStrategyInstances) {
            foreach (var pair in previousStrategyInstances) {
                pair.Key.Evaluator = pair.Value;
            }
        }
    
        public static void AssignPriorStrategies(IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorStrategyInstances) {
            foreach (var pair in priorStrategyInstances) {
                pair.Key.PriorStrategy = pair.Value;
            }
        }
    
        public static ResultSetProcessor GetAssignResultSetProcessor(AgentInstanceContext agentInstanceContext, ResultSetProcessorFactoryDesc resultSetProcessorPrototype) {
            AggregationService aggregationService = null;
            if (resultSetProcessorPrototype.AggregationServiceFactoryDesc != null) {
                aggregationService = resultSetProcessorPrototype.AggregationServiceFactoryDesc.AggregationServiceFactory.MakeService(agentInstanceContext, agentInstanceContext.StatementContext.MethodResolutionService);
            }
    
            OrderByProcessor orderByProcessor = null;
            if (resultSetProcessorPrototype.OrderByProcessorFactory != null) {
                orderByProcessor = resultSetProcessorPrototype.OrderByProcessorFactory.Instantiate(
                    aggregationService, agentInstanceContext);
            }
    
            var processor = resultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(orderByProcessor, aggregationService, agentInstanceContext);
    
            // initialize aggregation expression nodes
            if (resultSetProcessorPrototype.AggregationServiceFactoryDesc != null) {
                foreach (var aggregation in resultSetProcessorPrototype.AggregationServiceFactoryDesc.Expressions) {
                    aggregation.AssignFuture(aggregationService);
                }
            }
    
            return processor;
        }
    
        public static void AssignSubqueryStrategies(SubSelectStrategyCollection subSelectStrategyCollection, IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategyInstances)
        {
            // initialize subselects expression nodes (strategy assignment)
            foreach (var subselectEntry in subselectStrategyInstances)
            {
                var subselectNode = subselectEntry.Key;
                var strategyInstance = subselectEntry.Value;
    
                subselectNode.Strategy = strategyInstance.Stategy;
                subselectNode.SubselectAggregationService = strategyInstance.SubselectAggregationService;
    
                // initialize aggregations in the subselect
                var factoryDesc = subSelectStrategyCollection.Subqueries.Get(subselectNode);
                if (factoryDesc.AggregationServiceFactoryDesc != null) {
                    foreach (var aggExpressionDesc in factoryDesc.AggregationServiceFactoryDesc.Expressions) {
                        aggExpressionDesc.AssignFuture(subselectEntry.Value.SubselectAggregationService);
                    }
                    if (factoryDesc.AggregationServiceFactoryDesc.GroupKeyExpressions != null) {
                        foreach (var groupKeyExpr in factoryDesc.AggregationServiceFactoryDesc.GroupKeyExpressions) {
                            groupKeyExpr.AssignFuture(subselectEntry.Value.SubselectAggregationService);
                        }
                    }
                }
    
                // initialize "prior" nodes in the subselect
                if (strategyInstance.PriorStrategies != null) {
                    foreach (var entry in strategyInstance.PriorStrategies) {
                        entry.Key.PriorStrategy = entry.Value;
                    }
                }
    
                // initialize "prev" nodes in the subselect
                if (strategyInstance.PreviousNodeStrategies != null) {
                    foreach (var entry in strategyInstance.PreviousNodeStrategies) {
                        entry.Key.Evaluator = entry.Value;
                    }
                }
            }
        }
    }
}
