///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.methodagg
{
	/// <summary>
	/// Represents the "lastever" aggregate function is an expression tree.
	/// </summary>
    [Serializable]
    public class ExprLastEverNode : ExprAggregateNodeBase
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="distinct">flag indicating unique or non-unique value aggregation</param>
	    public ExprLastEverNode(bool distinct)
            : base(distinct)
	    {
	    }

	    protected override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
	    {
            if (PositionalParams.Length == 0 || PositionalParams.Length > 2)
	        {
                throw MakeExceptionExpectedParamNum(0, 2);
	        }
            if (PositionalParams.Length == 2)
            {
                base.ValidateFilter(PositionalParams[1].ExprEvaluator);
	        }
            return validationContext.EngineImportService.AggregationFactoryFactory.MakeLastEver(validationContext.StatementExtensionSvcContext, this, PositionalParams[0].ExprEvaluator.ReturnType);
	    }

	    public bool HasFilter => PositionalParams.Length == 2;

	    public override string AggregationFunctionName => "lastever";

	    protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
	    {
	        return node is ExprLastEverNode;
	    }

	    protected override bool IsFilterExpressionAsLastParameter => true;
	}
} // end of namespace
