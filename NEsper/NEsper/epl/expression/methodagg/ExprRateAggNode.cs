///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.methodagg
{
    /// <summary>
    /// Represents the Rate(...) and aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprRateAggNode : ExprAggregateNodeBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">- flag indicating unique or non-unique value aggregation</param>
        public ExprRateAggNode(bool distinct)
            : base(distinct)
        {
        }

        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            var positionalParams = base.PositionalParams;
            if (positionalParams.Length == 0)
            {
                throw new ExprValidationException(
                    "The rate aggregation function minimally requires a numeric constant or expression as a parameter.");
            }

            var first = positionalParams[0];
            if (first.IsConstantResult)
            {
                const string message =
                    "The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation";
                long intervalTime;
                if (first is ExprTimePeriod period)
                {
                    double secInterval = period.EvaluateAsSeconds(
                        null, true, validationContext.ExprEvaluatorContext);
                    intervalTime = validationContext.EngineImportService.TimeAbacus.DeltaForSecondsDouble(secInterval);
                }
                else if (ExprNodeUtility.IsConstantValueExpr(first))
                {
                    if (!first.ExprEvaluator.ReturnType.IsNumeric())
                    {
                        throw new ExprValidationException(message);
                    }
                    var num =
                        first.ExprEvaluator.Evaluate(
                            new EvaluateParams(null, true, validationContext.ExprEvaluatorContext));
                    intervalTime = validationContext.EngineImportService.TimeAbacus.DeltaForSecondsNumber(num);
                }
                else
                {
                    throw new ExprValidationException(message);
                }

                var optionalFilter = this.OptionalFilter;
                if (optionalFilter == null)
                {
                    this.PositionalParams = ExprNodeUtility.EMPTY_EXPR_ARRAY;
                }
                else
                {
                    this.PositionalParams = new ExprNode[] { optionalFilter };
                }

                return
                    validationContext.EngineImportService.AggregationFactoryFactory.MakeRate(
                        validationContext.StatementExtensionSvcContext, this, true, intervalTime,
                        validationContext.TimeProvider, validationContext.EngineImportService.TimeAbacus);
            }

            const string messageX =
                "The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation";
            Type boxedParamOne = first.ExprEvaluator.ReturnType.GetBoxedType();
            if (boxedParamOne != typeof (long?))
            {
                throw new ExprValidationException(messageX);
            }
            if (first.IsConstantResult)
            {
                throw new ExprValidationException(messageX);
            }
            if (first is ExprTimestampNode)
            {
                throw new ExprValidationException(
                    "The rate aggregation function does not allow the current engine timestamp as a parameter");
            }
            if (((positionalParams.Length == 2) && (this.OptionalFilter == null)) ||
                ((positionalParams.Length > 2) && (this.OptionalFilter != null))) {
                if (!TypeHelper.IsNumeric(positionalParams[1].ExprEvaluator.ReturnType)) {
                    throw new ExprValidationException(
                        "The rate aggregation function accepts an expression returning a numeric value to accumulate as an optional second parameter");
                }
            }
            bool hasDataWindows = ExprNodeUtility.HasRemoveStreamForAggregations(
                first, validationContext.StreamTypeService, validationContext.IsResettingAggregations);
            if (!hasDataWindows)
            {
                throw new ExprValidationException(
                    "The rate aggregation function in the timestamp-property notation requires data windows");
            }
            return
                validationContext.EngineImportService.AggregationFactoryFactory.MakeRate(
                    validationContext.StatementExtensionSvcContext, this, false, -1, validationContext.TimeProvider,
                    validationContext.EngineImportService.TimeAbacus);
        }

        protected override int MaxPositionalParams => 2;

        public override string AggregationFunctionName => "rate";

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprRateAggNode;
        }

        protected override bool IsFilterExpressionAsLastParameter => true;
    }
} // end of namespace
