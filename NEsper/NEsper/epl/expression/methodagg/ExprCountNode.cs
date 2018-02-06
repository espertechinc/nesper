///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
    /// <summary>
    /// Represents the Count(...) and Count(*) and Count(distinct ...) aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprCountNode : ExprAggregateNodeBase
    {
        private bool _hasFilter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprCountNode(bool distinct) 
            : base(distinct)
        {
        }
    
        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (PositionalParams.Length > 2 || PositionalParams.Length == 0)
            {
                throw MakeExceptionExpectedParamNum(1, 2);
            }
    
            Type childType = null;
            bool ignoreNulls = false;

            if (PositionalParams.Length == 1 && PositionalParams[0] is ExprWildcard)
            {
                ValidateNotDistinct();
                // defaults
            }
            else if (PositionalParams.Length == 1) {
                childType = PositionalParams[0].ExprEvaluator.ReturnType;
                ignoreNulls = true;
            }
            else if (PositionalParams.Length == 2) {
                _hasFilter = true;
                if (!(PositionalParams[0] is ExprWildcard)) {
                    childType = PositionalParams[0].ExprEvaluator.ReturnType;
                    ignoreNulls = true;
                }
                else {
                    ValidateNotDistinct();
                }
                base.ValidateFilter(PositionalParams[1].ExprEvaluator);
            }

            return validationContext.EngineImportService.AggregationFactoryFactory.MakeCount(validationContext.StatementExtensionSvcContext, this, ignoreNulls, childType);
        }

        public override string AggregationFunctionName => "count";

        public bool HasFilter => _hasFilter;

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprCountNode;
        }

        private void ValidateNotDistinct()
        {
            if (base.IsDistinct)
            {
                throw new ExprValidationException("Invalid use of the 'distinct' keyword with count and wildcard");
            }
        }

        protected override bool IsFilterExpressionAsLastParameter => true;
    }
}
