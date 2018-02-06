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
    /// Represents the Median(...) aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprMedianNode : ExprAggregateNodeBase
    {
        private bool _hasFilter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprMedianNode(bool distinct)
            : base(distinct)
        {
        }
    
        protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            _hasFilter = PositionalParams.Length > 1;
            var childType = base.ValidateNumericChildAllowFilter(_hasFilter);
            return validationContext.EngineImportService.AggregationFactoryFactory.MakeMedian(validationContext.StatementExtensionSvcContext, this, childType);
        }

        public override string AggregationFunctionName => "median";

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            if (!(node is ExprMedianNode))
            {
                return false;
            }
    
            return true;
        }

        public bool HasFilter => _hasFilter;

        protected override bool IsFilterExpressionAsLastParameter => true;
    }
}
