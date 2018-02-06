///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.view.window;

namespace com.espertech.esper.view.ext
{
    /// <summary>Factory for sort window views.</summary>
    public class SortWindowViewFactory
        : DataWindowViewFactory,
            DataWindowViewWithPrevious
    {
        private const string NAME = "Sort";

        /// <summary>The sort-by expressions.</summary>
        private ExprNode[] _sortCriteriaExpressions;

        private ExprEvaluator[] _sortCriteriaEvaluators;

        /// <summary>The flags defining the ascending or descending sort order.</summary>
        private bool[] _isDescendingValues;

        /// <summary>The sort window size.</summary>
        private ExprEvaluator _sizeEvaluator;

        private IList<ExprNode> _viewParameters;
        private EventType _eventType;
        private bool _useCollatorSort = false;

        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParams)
        {
            _viewParameters = viewParams;
        }

        public void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
            const string message =
                NAME + " window requires a numeric size parameter and a list of expressions providing sort keys";
            if (_viewParameters.Count < 2)
            {
                throw new ViewParameterException(message);
            }

            var validated = ViewFactorySupport.Validate(
                NAME + " window", parentEventType, statementContext, _viewParameters, true);
            for (var i = 1; i < validated.Length; i++)
            {
                ViewFactorySupport.AssertReturnsNonConstant(NAME + " window", validated[i], i);
            }

            ViewFactorySupport.ValidateNoProperties(ViewName, validated[0], 0);
            _sizeEvaluator = ViewFactorySupport.ValidateSizeParam(ViewName, statementContext, validated[0], 0);

            _sortCriteriaExpressions = new ExprNode[validated.Length - 1];
            _isDescendingValues = new bool[_sortCriteriaExpressions.Length];

            for (var i = 1; i < validated.Length; i++)
            {
                if (validated[i] is ExprOrderedExpr)
                {
                    _isDescendingValues[i - 1] = ((ExprOrderedExpr) validated[i]).IsDescending;
                    _sortCriteriaExpressions[i - 1] = validated[i].ChildNodes[0];
                }
                else
                {
                    _sortCriteriaExpressions[i - 1] = validated[i];
                }
            }
            _sortCriteriaEvaluators = ExprNodeUtility.GetEvaluators(_sortCriteriaExpressions);

            if (statementContext.ConfigSnapshot != null)
            {
                _useCollatorSort = statementContext.ConfigSnapshot.EngineDefaults.Language.IsSortUsingCollator;
            }
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var sortWindowSize = ViewFactorySupport.EvaluateSizeParam(
                ViewName, _sizeEvaluator, agentInstanceViewFactoryContext.AgentInstanceContext);
            var sortedRandomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new SortWindowView(
                this, _sortCriteriaExpressions, _sortCriteriaEvaluators, _isDescendingValues, sortWindowSize,
                sortedRandomAccess, _useCollatorSort, agentInstanceViewFactoryContext);
        }

        public Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is SortWindowView))
            {
                return false;
            }

            var other = (SortWindowView) view;
            var sortWindowSize = ViewFactorySupport.EvaluateSizeParam(ViewName, _sizeEvaluator, agentInstanceContext);
            if ((other.SortWindowSize != sortWindowSize) ||
                (!Compare(other.IsDescendingValues, _isDescendingValues)) ||
                (!ExprNodeUtility.DeepEquals(other.SortCriteriaExpressions, _sortCriteriaExpressions, false)))
            {
                return false;
            }

            return other.IsEmpty();
        }

        public string ViewName
        {
            get { return NAME; }
        }

        public ExprEvaluator[] SortCriteriaEvaluators
        {
            get { return _sortCriteriaEvaluators; }
        }

        public bool[] IsDescendingValues
        {
            get { return _isDescendingValues; }
        }

        public bool UseCollatorSort
        {
            get { return _useCollatorSort; }
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
} // end of namespace
