///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.firstlastever;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.core;

using System;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    ///     Represents the "firstever" and "lastever: aggregate function is an expression tree.
    /// </summary>
    public class ExprFirstLastEverNode : ExprAggregateNodeBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="first">true if first</param>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprFirstLastEverNode(
            bool distinct,
            bool first)
            : base(distinct)
        {
            IsFirst = first;
        }

        public bool HasFilter => positionalParams.Length == 2;

        public override string AggregationFunctionName => IsFirst ? "firstever" : "lastever";

        public bool IsFirst { get; }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (positionalParams.Length > 2) {
                throw MakeExceptionExpectedParamNum(0, 2);
            }

            if (positionalParams.Length == 2) {
                ValidateFilter(positionalParams[1]);
                optionalFilter = positionalParams[1];
            }

            Type resultType;
            var isWildcard = positionalParams.Length == 0 ||
                             positionalParams.Length > 0 && positionalParams[0] is ExprWildcard;
            if (isWildcard) {
                resultType = validationContext.StreamTypeService.EventTypes[0].UnderlyingType;
            }
            else {
                resultType = positionalParams[0].Forge.EvaluationType;
            }

            var serde = validationContext.SerdeResolver.SerdeForAggregation(resultType, validationContext.StatementRawInfo);
            return new AggregationForgeFactoryFirstLastEver(this, resultType, serde);
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprFirstLastEverNode)) {
                return false;
            }

            var other = (ExprFirstLastEverNode) node;
            return other.IsFirst == IsFirst;
        }

        public override bool IsFilterExpressionAsLastParameter => true;
    }
} // end of namespace