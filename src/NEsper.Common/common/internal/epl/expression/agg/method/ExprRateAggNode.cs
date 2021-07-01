///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.rate;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.node;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    /// Represents the rate(...) and aggregate function is an expression tree.
    /// </summary>
    public class ExprRateAggNode : ExprAggregateNodeBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprRateAggNode(bool distinct)
            : base(distinct)
        {
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (this.positionalParams.Length == 0) {
                throw new ExprValidationException(
                    "The rate aggregation function minimally requires a numeric constant or expression as a parameter.");
            }

            // handle "ever"
            ExprNode first = this.positionalParams[0];
            if (first.Forge.ForgeConstantType.IsCompileTimeConstant) {
                string messageX =
                    "The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation";
                long intervalTime;
                if (first is ExprTimePeriod) {
                    double secInterval = ((ExprTimePeriod) first).EvaluateAsSeconds(null, true, null);
                    intervalTime =
                        validationContext.ImportService.TimeAbacus.DeltaForSecondsDouble(secInterval);
                }
                else if (ExprNodeUtilityQuery.IsConstant(first)) {
                    if (!first.Forge.EvaluationType.IsNumeric()) {
                        throw new ExprValidationException(messageX);
                    }

                    var num = first.Forge.ExprEvaluator.Evaluate(null, true, null);
                    intervalTime = validationContext.ImportService.TimeAbacus.DeltaForSecondsNumber(num);
                }
                else {
                    throw new ExprValidationException(messageX);
                }

                if (optionalFilter == null) {
                    this.positionalParams = ExprNodeUtilityQuery.EMPTY_EXPR_ARRAY;
                }
                else {
                    this.positionalParams = new ExprNode[] {optionalFilter};
                }

                return new AggregationForgeFactoryRate(this, true, intervalTime, validationContext.ImportService.TimeAbacus);
            }

            string message =
                "The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation";
            Type boxedParamOne = first.Forge.EvaluationType.GetBoxedType();
            if (boxedParamOne != typeof(long?)) {
                throw new ExprValidationException(message);
            }

            if (first.Forge.ForgeConstantType.IsConstant) {
                throw new ExprValidationException(message);
            }

            if (first is ExprTimestampNode) {
                throw new ExprValidationException(
                    "The rate aggregation function does not allow the current runtime timestamp as a parameter");
            }

            if (this.positionalParams.Length > 1) {
                if (!this.positionalParams[1].Forge.EvaluationType.IsNumeric()) {
                    throw new ExprValidationException(
                        "The rate aggregation function accepts an expression returning a numeric value to accumulate as an optional second parameter");
                }
            }

            bool hasDataWindows = ExprNodeUtilityAggregation.HasRemoveStreamForAggregations(
                first,
                validationContext.StreamTypeService,
                validationContext.IsResettingAggregations);
            if (!hasDataWindows) {
                throw new ExprValidationException(
                    "The rate aggregation function in the timestamp-property notation requires data windows");
            }

            if (optionalFilter != null) {
                positionalParams = ExprNodeUtilityMake.AddExpression(positionalParams, optionalFilter);
            }

            return new AggregationForgeFactoryRate(this, false, -1, validationContext.ImportService.TimeAbacus);
        }

        public override string AggregationFunctionName {
            get => "rate";
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprRateAggNode;
        }

        public override bool IsFilterExpressionAsLastParameter => false;
    }
} // end of namespace