///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.leaving;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    /// Represents the leaving() aggregate function is an expression tree.
    /// </summary>
    public class ExprLeavingAggNode : ExprAggregateNodeBase
    {
        public ExprLeavingAggNode(bool distinct)
            : base(distinct)
        {
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (optionalFilter == null && positionalParams.Length > 0) {
                throw MakeExceptionExpectedParamNum(0, 0);
            }

            return new AggregationFactoryMethodLeaving(this);
        }

        public override string AggregationFunctionName {
            get => "leaving";
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprLeavingAggNode;
        }

        public override bool IsFilterExpressionAsLastParameter => true;
    }
} // end of namespace