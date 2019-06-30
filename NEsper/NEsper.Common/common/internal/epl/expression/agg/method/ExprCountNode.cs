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
using System;

namespace com.espertech.esper.common.@internal.epl.expression.agg.method
{
    /// <summary>
    /// Represents the count(...) and count(*) and count(distinct ...) aggregate function is an expression tree.
    /// </summary>
    public class ExprCountNode : ExprAggregateNodeBase
    {
        private bool hasFilter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprCountNode(bool distinct)
            : base(distinct)
        {
        }

        public override AggregationForgeFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (positionalParams.Length > 2 || positionalParams.Length == 0) {
                throw MakeExceptionExpectedParamNum(1, 2);
            }

            Type childType = null;
            bool ignoreNulls = false;

            if (positionalParams.Length == 1 && positionalParams[0] is ExprWildcard) {
                ValidateNotDistinct();
                // defaults
            }
            else if (positionalParams.Length == 1) {
                childType = positionalParams[0].Forge.EvaluationType;
                ignoreNulls = true;
            }
            else if (positionalParams.Length == 2) {
                hasFilter = true;
                if (!(positionalParams[0] is ExprWildcard)) {
                    childType = positionalParams[0].Forge.EvaluationType;
                    ignoreNulls = true;
                }
                else {
                    ValidateNotDistinct();
                }

                base.ValidateFilter(positionalParams[1]);
                optionalFilter = positionalParams[1];
            }

            return new AggregationFactoryMethodCount(this, ignoreNulls, childType);
        }

        public override string AggregationFunctionName {
            get => "count";
        }

        public bool HasFilter {
            get => hasFilter;
        }

        public override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprCountNode)) {
                return false;
            }

            return true;
        }

        public override bool IsFilterExpressionAsLastParameter {
            get => true;
        }

        private void ValidateNotDistinct()
        {
            if (base.IsDistinct) {
                throw new ExprValidationException("Invalid use of the 'distinct' keyword with count and wildcard");
            }
        }
    }
} // end of namespace