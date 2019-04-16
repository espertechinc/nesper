///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.avedev;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the avedev(...) aggregate function is an expression tree.
    /// </summary>
    public class ExprAvedevNode : ExprAggregateNodeBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprAvedevNode(bool distinct)
            : base(distinct)
        {
        }

        public override string AggregationFunctionName => "avedev";

        public bool HasFilter { get; private set; }

        internal override bool IsFilterExpressionAsLastParameter => true;

        internal override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            HasFilter = positionalParams.Length > 1;
            if (HasFilter) {
                optionalFilter = positionalParams[1];
            }

            var childType = ValidateNumericChildAllowFilter(HasFilter);
            return new AggregationFactoryMethodAvedev(this, childType, positionalParams);
        }

        internal override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprAvedevNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace