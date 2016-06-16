///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.ext
{
	/// <summary>
	/// Factory for sort window views.
	/// </summary>
	public class SortWindowViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
	{
	    private const string NAME = "Sort";

	    private IList<ExprNode> _viewParameters;

	    /// <summary>
	    /// The sort-by expressions.
	    /// </summary>
	    protected ExprNode[] sortCriteriaExpressions;

	    public SortWindowViewFactory()
	    {
	        IsUseCollatorSort = false;
	    }

	    public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParams)
	    {
	        _viewParameters = viewParams;
	    }

	    public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
	    {
	        EventType = parentEventType;
	        var message = NAME + " window requires a numeric size parameter and a list of expressions providing sort keys";
	        if (_viewParameters.Count < 2)
	        {
	            throw new ViewParameterException(message);
	        }

	        var validated = ViewFactorySupport.Validate(NAME + " window", parentEventType, statementContext, _viewParameters, true);
	        for (var i = 1; i < validated.Length; i++)
	        {
	            ViewFactorySupport.AssertReturnsNonConstant(NAME + " window", validated[i], i);
	        }

	        var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
	        var sortSize = ViewFactorySupport.EvaluateAssertNoProperties(NAME + " window", validated[0], 0, exprEvaluatorContext);
	        if ((sortSize == null) || (!sortSize.IsNumber()))
	        {
	            throw new ViewParameterException(message);
	        }
	        SortWindowSize = sortSize.AsInt();

	        sortCriteriaExpressions = new ExprNode[validated.Length - 1];
	        IsDescendingValues = new bool[sortCriteriaExpressions.Length];

	        for (var i = 1; i < validated.Length; i++)
	        {
	            if (validated[i] is ExprOrderedExpr)
	            {
	                IsDescendingValues[i - 1] = ((ExprOrderedExpr) validated[i]).IsDescending;
	                sortCriteriaExpressions[i - 1] = validated[i].ChildNodes[0];
	            }
	            else
	            {
	                sortCriteriaExpressions[i - 1] = validated[i];
	            }
	        }
	        SortCriteriaEvaluators = ExprNodeUtility.GetEvaluators(sortCriteriaExpressions);

	        if (statementContext.ConfigSnapshot != null) {
	            IsUseCollatorSort = statementContext.ConfigSnapshot.EngineDefaults.LanguageConfig.IsSortUsingCollator;
	        }
	    }

	    public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
	    {
	        var sortedRandomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);

	        return new SortWindowView(this, sortCriteriaExpressions, SortCriteriaEvaluators, IsDescendingValues, SortWindowSize, sortedRandomAccess, IsUseCollatorSort, agentInstanceViewFactoryContext);
	    }

	    public object MakePreviousGetter()
        {
	        return new RandomAccessByIndexGetter();
	    }

	    public EventType EventType { get; private set; }

	    public bool CanReuse(View view)
	    {
	        if (!(view is SortWindowView))
	        {
	            return false;
	        }

	        var other = (SortWindowView) view;
	        if ((other.SortWindowSize != SortWindowSize) ||
	            (!Compare(other.IsDescendingValues, IsDescendingValues)) ||
	            (!ExprNodeUtility.DeepEquals(other.SortCriteriaExpressions, sortCriteriaExpressions)) )
	        {
	            return false;
	        }

	        return other.IsEmpty();
	    }

	    public string ViewName
	    {
	        get { return NAME; }
	    }

	    public ExprEvaluator[] SortCriteriaEvaluators { get; protected set; }

	    public bool[] IsDescendingValues { get; protected set; }

	    public int SortWindowSize { get; protected set; }

	    public bool IsUseCollatorSort { get; private set; }

	    private bool Compare(bool[] one, bool[] two)
	    {
	        if (one.Length != two.Length)
	        {
	            return false;
	        }

	        for (var i = 0; i < one.Length; i++)
	        {
	            if (one[i] != two[i])
	            {
	                return false;
	            }
	        }

	        return true;
	    }
	}
} // end of namespace
