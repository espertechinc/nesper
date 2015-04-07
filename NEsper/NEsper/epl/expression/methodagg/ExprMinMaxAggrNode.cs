///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.methodagg
{
	/// <summary>
	/// Represents the min/max(distinct? ...) aggregate function is an expression tree.
	/// </summary>
	[Serializable]
    public class ExprMinMaxAggrNode : ExprAggregateNodeBase
	{
	    private readonly MinMaxTypeEnum _minMaxTypeEnum;

	    private readonly bool _hasFilter;
	    private readonly bool _isEver;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="distinct">indicator whether distinct values of all values min/max</param>
        /// <param name="minMaxTypeEnum">enum for whether to minimum or maximum compute</param>
        /// <param name="hasFilter">if set to <c>true</c> [has filter].</param>
        /// <param name="isEver">if set to <c>true</c> [is ever].</param>
	    public ExprMinMaxAggrNode(bool distinct, MinMaxTypeEnum minMaxTypeEnum, bool hasFilter, bool isEver)
            : base(distinct)
	    {
	        _minMaxTypeEnum = minMaxTypeEnum;
	        _hasFilter = hasFilter;
	        _isEver = isEver;
	    }

	    public override AggregationMethodFactory ValidateAggregationChild(ExprValidationContext validationContext)
	    {
            if (PositionalParams.Length == 0 || PositionalParams.Length > 2)
            {
	            throw new ExprValidationException(_minMaxTypeEnum.ToString() + " node must have either 1 or 2 parameters");
	        }

            ExprNode child = PositionalParams[0];
	        bool hasDataWindows;
	        if (_isEver) {
	            hasDataWindows = false;
	        }
	        else {
	            if (validationContext.ExprEvaluatorContext.StatementType == StatementType.CREATE_TABLE) {
	                hasDataWindows = true;
	            }
	            else {
	                hasDataWindows = ExprNodeUtility.HasRemoveStreamForAggregations(child, validationContext.StreamTypeService, validationContext.IsResettingAggregations);
	            }
	        }

	        if (_hasFilter) {
                if (PositionalParams.Length < 2) {
	                throw new ExprValidationException(_minMaxTypeEnum.ToString() + "-filtered aggregation function must have a filter expression as a second parameter");
	            }
                base.ValidateFilter(PositionalParams[1].ExprEvaluator);
	        }
	        return new ExprMinMaxAggrNodeFactory(this, child.ExprEvaluator.ReturnType, hasDataWindows);
	    }

	    protected override bool EqualsNodeAggregateMethodOnly(ExprAggregateNode node)
	    {
	        if (!(node is ExprMinMaxAggrNode))
	        {
	            return false;
	        }

	        ExprMinMaxAggrNode other = (ExprMinMaxAggrNode) node;
	        return other._minMaxTypeEnum == _minMaxTypeEnum && other._isEver == _isEver;
	    }

	    /// <summary>
	    /// Returns the indicator for minimum or maximum.
	    /// </summary>
	    /// <value>min/max indicator</value>
	    public MinMaxTypeEnum MinMaxTypeEnum
	    {
	        get { return _minMaxTypeEnum; }
	    }

	    public bool HasFilter
	    {
	        get { return _hasFilter; }
	    }

	    public override string AggregationFunctionName
	    {
	        get { return _minMaxTypeEnum.GetExpressionText(); }
	    }

	    public bool IsEver
	    {
	        get { return _isEver; }
	    }
	}
} // end of namespace
