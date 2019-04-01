///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.sum;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the sum(...) aggregate function is an expression tree.
    /// </summary>
    public class ExprSumNode : ExprAggregateNodeBase
    {
        public ExprSumNode(bool distinct) : base(distinct)
        {
        }

        public override string AggregationFunctionName => "sum";

        public bool IsFilter { get; private set; }

        internal override bool IsFilterExpressionAsLastParameter => true;

        internal override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            IsFilter = positionalParams.Length > 1;
            if (IsFilter)
            {
                optionalFilter = positionalParams[1];
            }

            var childType = ValidateNumericChildAllowFilter(IsFilter);
            return new AggregationFactoryMethodSum(this, childType);
        }

        internal override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprSumNode;
        }
    }
} // end of namespace