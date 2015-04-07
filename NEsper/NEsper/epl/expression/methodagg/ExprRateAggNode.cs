///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
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
        /// <summary>Ctor. </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprRateAggNode(bool distinct)
            : base(distinct)
        {
        }
    
        public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (PositionalParams.Length == 0) {
                throw new ExprValidationException("The rate aggregation function minimally requires a numeric constant or expression as a parameter.");
            }

            ExprNode first = PositionalParams[0];
            if (first.IsConstantResult) {
                const string message = "The rate aggregation function requires a numeric constant or time period as the first parameter in the constant-value notation";
                long intervalMSec;
                if (first is ExprTimePeriod)
                {
                    var secInterval = ((ExprTimePeriod)first).EvaluateAsSeconds(
                        null, true, validationContext.ExprEvaluatorContext);
                    intervalMSec = (long) Math.Round(secInterval * 1000d);
                }
                else if (ExprNodeUtility.IsConstantValueExpr(first)) {
                    if (!first.ExprEvaluator.ReturnType.IsNumeric()) {
                        throw new ExprValidationException(message);
                    }
                    var num = first.ExprEvaluator.Evaluate(
                        new EvaluateParams(null, true, validationContext.ExprEvaluatorContext));
                    intervalMSec = (long) Math.Round(num.AsDouble() * 1000d);
                }
                else {
                    throw new ExprValidationException(message);
                }
    
                return new ExprRateAggNodeFactory(this, true, intervalMSec);
            }
            else {
                const string message = "The rate aggregation function requires a property or expression returning a non-constant long-type value as the first parameter in the timestamp-property notation";
                var boxedParamOne = first.ExprEvaluator.ReturnType.GetBoxedType();
                if (boxedParamOne != typeof(long?)) {
                    throw new ExprValidationException(message);
                }
                if (first.IsConstantResult) {
                    throw new ExprValidationException(message);
                }
                if (first is ExprTimestampNode) {
                    throw new ExprValidationException("The rate aggregation function does not allow the current engine timestamp as a parameter");
                }
                if (PositionalParams.Length > 1) {
                    if (!PositionalParams[1].ExprEvaluator.ReturnType.IsNumeric()) {
                        throw new ExprValidationException("The rate aggregation function accepts an expression returning a numeric value to accumulate as an optional second parameter");
                    }
                }
                bool hasDataWindows = ExprNodeUtility.HasRemoveStreamForAggregations(first, validationContext.StreamTypeService, validationContext.IsResettingAggregations);
                if (!hasDataWindows) {
                    throw new ExprValidationException("The rate aggregation function in the timestamp-property notation requires data windows");
                }
                return new ExprRateAggNodeFactory(this, false, -1);
            }
        }

        public override string AggregationFunctionName
        {
            get { return "rate"; }
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprRateAggNode;
        }
    }
}
