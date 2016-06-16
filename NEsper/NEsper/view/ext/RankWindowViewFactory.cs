///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
	/// Factory for rank window views.
	/// </summary>
	public class RankWindowViewFactory
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
	{
	    private const string NAME = "Rank";

	    private IList<ExprNode> _viewParameters;

	    /// <summary>
	    /// The unique-by expressions.
	    /// </summary>
	    protected ExprNode[] uniqueCriteriaExpressions;

	    /// <summary>
	    /// The sort-by expressions.
	    /// </summary>
	    protected ExprNode[] sortCriteriaExpressions;

	    public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParams)
	    {
	        _viewParameters = viewParams;
	    }

	    public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
	    {
	        EventType = parentEventType;
	        var message = NAME + " view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys";
	        if (_viewParameters.Count < 3)
	        {
	            throw new ViewParameterException(message);
	        }

	        // validate
	        var validated = ViewFactorySupport.Validate(NAME, parentEventType, statementContext, _viewParameters, true);

	        // find size-parameter index
	        var indexNumericSize = -1;
	        for (var i = 0; i < validated.Length; i++) {
	            if (validated[i] is ExprConstantNode) {
	                indexNumericSize = i;
	                break;
	            }
	        }
	        if (indexNumericSize == -1) {
	            throw new ViewParameterException("Failed to find constant value for the numeric size parameter");
	        }
	        if (indexNumericSize == 0) {
	            throw new ViewParameterException("Failed to find unique value expressions that are expected to occur before the numeric size parameter");
	        }
	        if (indexNumericSize == validated.Length - 1) {
	            throw new ViewParameterException("Failed to find sort key expressions after the numeric size parameter");
	        }

	        // validate non-constant for unique-keys and sort-keys
	        for (var i = 0; i < indexNumericSize; i++)
	        {
	            ViewFactorySupport.AssertReturnsNonConstant(NAME, validated[i], i);
	        }
	        for (var i = indexNumericSize+1; i < validated.Length; i++)
	        {
	            ViewFactorySupport.AssertReturnsNonConstant(NAME, validated[i], i);
	        }

	        // get sort size
	        var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
	        var sortSize = ViewFactorySupport.EvaluateAssertNoProperties(NAME, validated[indexNumericSize], indexNumericSize, exprEvaluatorContext);
	        if ((sortSize == null) || (!sortSize.IsNumber()))
	        {
	            throw new ViewParameterException(message);
	        }
	        SortWindowSize = sortSize.AsInt();

	        // compile unique expressions
	        uniqueCriteriaExpressions = new ExprNode[indexNumericSize];
	        Array.Copy(validated, 0, uniqueCriteriaExpressions, 0, indexNumericSize);

	        // compile sort expressions
	        sortCriteriaExpressions = new ExprNode[validated.Length - indexNumericSize - 1];
	        IsDescendingValues = new bool[sortCriteriaExpressions.Length];

	        var count = 0;
	        for (var i = indexNumericSize + 1; i < validated.Length; i++)
	        {
	            if (validated[i] is ExprOrderedExpr)
	            {
	                IsDescendingValues[count] = ((ExprOrderedExpr) validated[i]).IsDescending;
	                sortCriteriaExpressions[count] = validated[i].ChildNodes[0];
	            }
	            else
	            {
	                sortCriteriaExpressions[count] = validated[i];
	            }
	            count++;
	        }

	        UniqueEvals = ExprNodeUtility.GetEvaluators(uniqueCriteriaExpressions);
	        SortEvals = ExprNodeUtility.GetEvaluators(sortCriteriaExpressions);

	        if (statementContext.ConfigSnapshot != null) {
	            IsUseCollatorSort = statementContext.ConfigSnapshot.EngineDefaults.LanguageConfig.IsSortUsingCollator;
	        }
	    }

	    public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
	    {
	        var rankedRandomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
	        return new RankWindowView(this, uniqueCriteriaExpressions, UniqueEvals, sortCriteriaExpressions, SortEvals, IsDescendingValues, SortWindowSize, rankedRandomAccess, IsUseCollatorSort, agentInstanceViewFactoryContext);
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

	    public string ViewName
	    {
	        get { return NAME; }
	    }

	    public bool[] IsDescendingValues { get; protected set; }

	    public ExprEvaluator[] UniqueEvals { get; protected set; }

	    public ExprEvaluator[] SortEvals { get; protected set; }

	    public int SortWindowSize { get; protected set; }

	    public bool IsUseCollatorSort { get; protected set; }
	}
} // end of namespace
