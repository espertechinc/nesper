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
    /// Represents the "countever" aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprCountEverNode : ExprAggregateNodeBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprCountEverNode(bool distinct)
            : base(distinct)
        {
        }
    
        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext) 
        {
            if (PositionalParams.Length > 2)
            {
                throw MakeExceptionExpectedParamNum(0, 2);
            }
            if (base.IsDistinct) {
                throw new ExprValidationException("Aggregation function '" + AggregationFunctionName + "' does now allow distinct");
            }
    
            bool ignoreNulls = false;
            if (PositionalParams.Length == 0) {
                // no parameters is allowed
            }
            else {
                ignoreNulls = !(PositionalParams[0] is ExprWildcard);
                if (PositionalParams.Length == 2)
                {
                    base.ValidateFilter(PositionalParams[1].ExprEvaluator);
                }
            }

            return validationContext.EngineImportService.AggregationFactoryFactory.MakeCountEver(validationContext.StatementExtensionSvcContext, this, ignoreNulls);
        }

        public bool HasFilter => PositionalParams.Length == 2;

        public override string AggregationFunctionName => "countever";

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprCountEverNode;
        }

        protected override bool IsFilterExpressionAsLastParameter => true;
    }
}
