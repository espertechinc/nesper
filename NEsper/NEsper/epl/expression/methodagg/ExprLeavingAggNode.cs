///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Represents the Leaving() aggregate function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprLeavingAggNode : ExprAggregateNodeBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
        public ExprLeavingAggNode(bool distinct)
            : base(distinct)
        {
        }

        public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
        {
            if (PositionalParams.Length > 0)
            {
                throw MakeExceptionExpectedParamNum(0, 0);
            }
    
            return new ExprLeavingAggNodeFactory(this);
        }

        public override string AggregationFunctionName
        {
            get { return "leaving"; }
        }

        protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
        {
            return node is ExprLeavingAggNode;
        }
    }
}
