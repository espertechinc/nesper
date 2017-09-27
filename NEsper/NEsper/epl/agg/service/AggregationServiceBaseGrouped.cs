///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// All aggregation services require evaluation nodes which supply the value to be 
    /// aggregated (summed, averaged, etc.) and aggregation state factories to make new 
    /// aggregation states.
    /// </summary>
    public abstract class AggregationServiceBaseGrouped : AggregationService
    {
        /// <summary>Evaluation nodes under. </summary>
        protected internal ExprEvaluator[] Evaluators;

        /// <summary>Aggregation states and factories. </summary>
        protected internal AggregationMethodFactory[] Aggregators;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">are the child node of each aggregation function used for computing the value to be aggregated</param>
        /// <param name="aggregators">aggregation states/factories</param>
        protected AggregationServiceBaseGrouped(ExprEvaluator[] evaluators, AggregationMethodFactory[] aggregators)
        {
            Evaluators = evaluators;
            Aggregators = aggregators;

            if (evaluators.Length != aggregators.Length)
            {
                throw new ArgumentException("Expected the same number of evaluates as computer prototypes");
            }
        }

        public void Stop()
        {
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return this;
        }

        public abstract object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams);
        public abstract ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams);
        public abstract ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams);
        public abstract EventBean GetEventBean(int column, EvaluateParams evaluateParams);
        public abstract object GetGroupKey(int agentInstanceId);
        public abstract ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext);
        public abstract void ApplyEnter(EventBean[] eventsPerStream, object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext);
        public abstract void ApplyLeave(EventBean[] eventsPerStream, object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext);
        public abstract void SetCurrentAccess(object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel);
        public abstract void ClearResults(ExprEvaluatorContext exprEvaluatorContext);
        public abstract void SetRemovedCallback(AggregationRowRemovedCallback callback);
        public abstract void Accept(AggregationServiceVisitor visitor);
        public abstract void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor);
        public abstract bool IsGrouped { get; }
    }
}
