///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;

namespace com.espertech.esper.epl.expression.visitor
{
	/// <summary>
	/// Visitor that collects event property identifier information under expression nodes.
	/// The visitor can be configued to not visit aggregation nodes thus ignoring
	/// properties under aggregation nodes such as sum, avg, min/max etc.
	/// </summary>
	public class ExprNodeStreamSelectVisitor : ExprNodeVisitor
	{
	    private readonly bool _isVisitAggregateNodes;
	    private bool _hasStreamSelect;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="visitAggregateNodes">true to indicate that the visitor should visit aggregate nodes, or falseif the visitor ignores aggregate nodes
	    /// </param>
	    public ExprNodeStreamSelectVisitor(bool visitAggregateNodes)
	    {
	        _isVisitAggregateNodes = visitAggregateNodes;
	    }

	    public bool IsVisit(ExprNode exprNode)
	    {
	        if (exprNode is ExprLambdaGoesNode) {
	            return false;
	        }

	        if (_isVisitAggregateNodes) {
	            return true;
	        }

	        return (!(exprNode is ExprAggregateNode));
	    }

	    public bool HasStreamSelect
	    {
	        get { return _hasStreamSelect; }
	    }

	    public void Visit(ExprNode exprNode)
	    {
	        if (exprNode is ExprStreamUnderlyingNode) {
	            _hasStreamSelect = true;
	        }

            var streamRef = exprNode as ExprDotNode;
	        if (streamRef != null && streamRef.StreamReferencedIfAny != null) {
                _hasStreamSelect = true;
	        }
	    }
	}
} // end of namespace
