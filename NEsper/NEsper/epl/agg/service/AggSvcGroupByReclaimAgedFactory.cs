///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.annotation;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.variable;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedFactory : AggregationServiceFactoryBase
    {
        private const long DEFAULT_MAX_AGE_MSEC = 60000L;
    
        protected readonly AggregationAccessorSlotPair[] Accessors;
        protected readonly AggregationStateFactory[] AccessAggregations;
        protected readonly bool IsJoin;
    
        protected readonly AggSvcGroupByReclaimAgedEvalFuncFactory EvaluationFunctionMaxAge;
        protected readonly AggSvcGroupByReclaimAgedEvalFuncFactory EvaluationFunctionFrequency;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. sum(4*myNum))</param>
        /// <param name="prototypes">collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregationsaggregation states for each group</param>
        /// <param name="groupKeyBinding">The group key binding.</param>
        /// <param name="reclaimGroupAged">hint to reclaim</param>
        /// <param name="reclaimGroupFrequency">hint to reclaim</param>
        /// <param name="variableService">variables</param>
        /// <param name="accessors">accessor definitions</param>
        /// <param name="accessAggregations">access aggs</param>
        /// <param name="isJoin">true for join, false for single-stream</param>
        /// <param name="optionalContextName">Name of the optional context.</param>
        /// <exception cref="ExprValidationException">Required hint value for hint ' + HintEnum.RECLAIM_GROUP_AGED + ' has not been provided</exception>
        /// <throws><seealso cref="ExprValidationException" /> when validation fails</throws>
        public AggSvcGroupByReclaimAgedFactory(
            ExprEvaluator[] evaluators,
            AggregationMethodFactory[] prototypes,
            Object groupKeyBinding,
            HintAttribute reclaimGroupAged,
            HintAttribute reclaimGroupFrequency,
            VariableService variableService,
            AggregationAccessorSlotPair[] accessors,
            AggregationStateFactory[] accessAggregations,
            bool isJoin,
            String optionalContextName)
            : base(evaluators, prototypes, groupKeyBinding)
        {
            this.Accessors = accessors;
            this.AccessAggregations = accessAggregations;
            this.IsJoin = isJoin;
    
            String hintValueMaxAge = HintEnum.RECLAIM_GROUP_AGED.GetHintAssignedValue(reclaimGroupAged);
            if (hintValueMaxAge == null)
            {
                throw new ExprValidationException("Required hint value for hint '" + HintEnum.RECLAIM_GROUP_AGED + "' has not been provided");
            }
            EvaluationFunctionMaxAge = GetEvaluationFunction(variableService, hintValueMaxAge, optionalContextName);
    
            String hintValueFrequency = HintEnum.RECLAIM_GROUP_FREQ.GetHintAssignedValue(reclaimGroupAged);
            if ((reclaimGroupFrequency == null) || (hintValueFrequency == null))
            {
                EvaluationFunctionFrequency = EvaluationFunctionMaxAge;
            }
            else
            {
                EvaluationFunctionFrequency = GetEvaluationFunction(variableService, hintValueFrequency, optionalContextName);
            }
        }
    
        public override AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService)
        {
            AggSvcGroupByReclaimAgedEvalFunc max = EvaluationFunctionMaxAge.Make(agentInstanceContext);
            AggSvcGroupByReclaimAgedEvalFunc freq = EvaluationFunctionFrequency.Make(agentInstanceContext);
            return new AggSvcGroupByReclaimAgedImpl(Evaluators, Aggregators, GroupKeyBinding, Accessors, AccessAggregations, IsJoin, max, freq, methodResolutionService);
        }
    
        private AggSvcGroupByReclaimAgedEvalFuncFactory GetEvaluationFunction(VariableService variableService, String hintValue, String optionalContextName)
        {
            VariableMetaData variableMetaData = variableService.GetVariableMetaData(hintValue);
            if (variableMetaData != null)
            {
                if (!variableMetaData.VariableType.IsNumeric())
                {
                    throw new ExprValidationException("Variable type of variable '" + variableMetaData.VariableName + "' is not numeric");
                }
                String message = VariableServiceUtil.CheckVariableContextName(optionalContextName, variableMetaData);
                if (message != null) {
                    throw new ExprValidationException(message);
                }
                return new ProxyAggSvcGroupByReclaimAgedEvalFuncFactory
                {
                    ProcMake = agentInstanceContext =>
                    {
                        VariableReader reader = variableService.GetReader(
                            variableMetaData.VariableName, agentInstanceContext.AgentInstanceId);
                        return new AggSvcGroupByReclaimAgedEvalFuncVariable(reader);
                    }
                };
            }
            else
            {
                double valueDouble;
                try {
                    valueDouble = DoubleValue.ParseString(hintValue);
                }
                catch (Exception) {
                    throw new ExprValidationException("Failed to parse hint parameter value '" + hintValue + "' as a double-typed seconds value or variable name");
                }
                if (valueDouble <= 0) {
                    throw new ExprValidationException("Hint parameter value '" + hintValue + "' is an invalid value, expecting a double-typed seconds value or variable name");
                }
                return new ProxyAggSvcGroupByReclaimAgedEvalFuncFactory
                {
                    ProcMake = agentInstanceContext => new AggSvcGroupByReclaimAgedEvalFuncConstant(valueDouble)
                };
            }
        }
    }
}
