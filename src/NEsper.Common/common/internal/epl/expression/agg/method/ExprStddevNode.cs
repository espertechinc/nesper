///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.stddev;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the stddev(...) aggregate function is an expression tree.
    /// </summary>
    public class ExprStddevNode : ExprAggregateNodeBase
    {
        public ExprStddevNode(bool distinct)
            : base(distinct)
        {
        }

        public bool HasFilter { get; private set; }

        public override string AggregationFunctionName => "stddev";

        public override bool IsFilterExpressionAsLastParameter => true;

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            HasFilter = positionalParams.Length > 1;
            if (HasFilter) {
                optionalFilter = positionalParams[1];
            }

            var childType = ValidateNumericChildAllowFilter(HasFilter);
            var distinctSerde = isDistinct
                ? validationContext.SerdeResolver.SerdeForAggregationDistinct(
                    childType,
                    validationContext.StatementRawInfo)
                : null;
            return new AggregationForgeFactoryStddev(this, childType, distinctSerde);
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprStddevNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace