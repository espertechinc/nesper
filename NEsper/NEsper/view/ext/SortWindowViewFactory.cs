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
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.ext
{
    /// <summary>Factory for sort window views. </summary>
    public class SortWindowViewFactory : DataWindowViewFactory, DataWindowViewWithPrevious
    {
        private readonly static String NAME = "Sort";
    
        private IList<ExprNode> viewParameters;
    
        /// <summary>The sort-by expressions. </summary>
        protected ExprNode[] sortCriteriaExpressions;
    
        /// <summary>The flags defining the ascending or descending sort order. </summary>
        protected bool[] isDescendingValues;
    
        /// <summary>The sort window size. </summary>
        protected int sortWindowSize;
    
        private EventType eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParams)
        {
            this.viewParameters = viewParams;
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            eventType = parentEventType;
            var message = NAME + " window requires a numeric size parameter and a list of expressions providing sort keys";
            if (viewParameters.Count < 2)
            {
                throw new ViewParameterException(message);
            }
    
            var validated = ViewFactorySupport.Validate(NAME + " window", parentEventType, statementContext, viewParameters, true);
            for (var i = 1; i < validated.Length; i++)
            {
                ViewFactorySupport.AssertReturnsNonConstant(NAME + " window", validated[i], i);
            }
    
            var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
            var sortSize = ViewFactorySupport.EvaluateAssertNoProperties(NAME + " window", validated[0], 0, exprEvaluatorContext);
            if ((sortSize == null) || (!(sortSize.IsNumber())))
            {
                throw new ViewParameterException(message);
            }
            sortWindowSize = sortSize.AsInt();
    
            sortCriteriaExpressions = new ExprNode[validated.Length - 1];
            isDescendingValues = new bool[sortCriteriaExpressions.Length];
    
            for (var i = 1; i < validated.Length; i++)
            {
                if (validated[i] is ExprOrderedExpr)
                {
                    isDescendingValues[i - 1] = ((ExprOrderedExpr) validated[i]).IsDescending;
                    sortCriteriaExpressions[i - 1] = validated[i].ChildNodes[0];
                }
                else
                {
                    sortCriteriaExpressions[i - 1] = validated[i];
                }
            }
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var sortedRandomAccess = ViewServiceHelper.GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
    
            var useCollatorSort = false;
            if (agentInstanceViewFactoryContext.AgentInstanceContext.StatementContext.ConfigSnapshot != null)
            {
                useCollatorSort = agentInstanceViewFactoryContext.AgentInstanceContext.StatementContext.ConfigSnapshot.EngineDefaults.LanguageConfig.IsSortUsingCollator;
            }
    
            var childEvals = ExprNodeUtility.GetEvaluators(sortCriteriaExpressions);
            return new SortWindowView(this, sortCriteriaExpressions, childEvals, isDescendingValues, sortWindowSize, sortedRandomAccess, useCollatorSort, agentInstanceViewFactoryContext);
        }
    
        public Object MakePreviousGetter() {
            return new RandomAccessByIndexGetter();
        }

        public EventType EventType
        {
            get { return eventType; }
        }

        public bool CanReuse(View view)
        {
            if (!(view is SortWindowView))
            {
                return false;
            }
    
            var other = (SortWindowView) view;
            if ((other.SortWindowSize != sortWindowSize) ||
                (!Compare(other.IsDescendingValues, isDescendingValues)) ||
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
}
