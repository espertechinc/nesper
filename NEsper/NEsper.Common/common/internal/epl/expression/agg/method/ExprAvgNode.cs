///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.avg;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the avg(...) aggregate function is an expression tree.
    /// </summary>
    public class ExprAvgNode : ExprAggregateNodeBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprAvgNode(bool distinct)
            : base(distinct)
        {
        }

        public override string AggregationFunctionName => "avg";

        public bool HasFilter { get; private set; }

        public override bool IsFilterExpressionAsLastParameter => true;

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            HasFilter = positionalParams.Length > 1;
            if (HasFilter) {
                optionalFilter = positionalParams[1];
            }

            var childType = base.ValidateNumericChildAllowFilter(HasFilter);
            var distinctSerde = isDistinct ? validationContext.SerdeResolver.SerdeForAggregationDistinct(childType, validationContext.StatementRawInfo) : null;
            return new AggregationForgeFactoryAvg(this, childType, distinctSerde, validationContext.ImportService.DefaultMathContext);
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprAvgNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace