///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
    /// <summary>
    /// Represents the Nth(...) and aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprNthAggNode : ExprAggregateNodeBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprNthAggNode(bool distinct)
            : base(distinct)
        {
        }
    
        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            const string message = "The nth aggregation function requires two parameters, an expression returning aggregation values and a numeric index constant";
            var positionalParams = PositionalParams;
            if (positionalParams.Length != 2) {
                throw new ExprValidationException(message);
            }
    
            ExprNode first = positionalParams[0];
            ExprNode second = positionalParams[1];
            if (!second.IsConstantResult) {
                throw new ExprValidationException(message);
            }

            var num = second.ExprEvaluator.Evaluate(new EvaluateParams(null, true, validationContext.ExprEvaluatorContext));
            var size = num.AsInt();

            var optionalFilter = OptionalFilter;
            if (optionalFilter != null)
            {
                PositionalParams = ExprNodeUtility.AddExpression(positionalParams, optionalFilter);
            }


            return validationContext.EngineImportService.AggregationFactoryFactory.MakeNth(validationContext.StatementExtensionSvcContext, this, first.ExprEvaluator.ReturnType, size);
        }

        public override string AggregationFunctionName => "nth";

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprNthAggNode;
        }

        protected override bool IsFilterExpressionAsLastParameter => false;
    }
}
