///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.nth;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    /// Represents the nth(...) and aggregate function is an expression tree.
    /// </summary>
    public class ExprNthAggNode : ExprAggregateNodeBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprNthAggNode(bool distinct)
            : base(distinct)
        {
        }

        internal override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            string message =
                "The nth aggregation function requires two parameters, an expression returning aggregation values and a numeric index constant";
            if (this.positionalParams.Length != 2) {
                throw new ExprValidationException(message);
            }

            ExprNode first = this.positionalParams[0];
            ExprNode second = this.positionalParams[1];
            if (!second.Forge.ForgeConstantType.IsCompileTimeConstant) {
                throw new ExprValidationException(message);
            }

            var num = second.Forge.ExprEvaluator.Evaluate(null, true, null);
            int size = num.AsInt();

            if (optionalFilter != null) {
                this.positionalParams = ExprNodeUtilityMake.AddExpression(positionalParams, optionalFilter);
            }

            return new AggregationFactoryMethodNth(this, first.Forge.EvaluationType, size);
        }

        public override string AggregationFunctionName {
            get => "nth";
        }

        internal override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprNthAggNode;
        }

        internal override bool IsFilterExpressionAsLastParameter => false;
    }
} // end of namespace