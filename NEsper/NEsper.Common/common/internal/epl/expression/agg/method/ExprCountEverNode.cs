///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.count;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the "countever" aggregate function is an expression tree.
    /// </summary>
    public class ExprCountEverNode : ExprAggregateNodeBase
    {
        public ExprCountEverNode(bool distinct)
            : base(distinct)
        {
        }

        public bool HasFilter => positionalParams.Length == 2;

        public override string AggregationFunctionName => "countever";

        internal override bool IsFilterExpressionAsLastParameter => true;

        internal override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (positionalParams.Length > 2) {
                throw MakeExceptionExpectedParamNum(0, 2);
            }

            if (isDistinct) {
                throw new ExprValidationException(
                    "Aggregation function '" + AggregationFunctionName + "' does now allow distinct");
            }

            var ignoreNulls = false;
            if (positionalParams.Length == 0) {
                // no parameters is allowed
            }
            else {
                ignoreNulls = !(positionalParams[0] is ExprWildcard);
                if (positionalParams.Length == 2) {
                    ValidateFilter(positionalParams[1]);
                    optionalFilter = positionalParams[1];
                }
            }

            return new AggregationFactoryMethodCountEver(this, ignoreNulls);
        }

        internal override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprCountEverNode;
        }
    }
} // end of namespace