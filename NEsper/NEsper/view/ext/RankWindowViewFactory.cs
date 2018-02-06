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
    /// <summary>Factory for rank window views.</summary>
    public class RankWindowViewFactory 
        : DataWindowViewFactory
        , DataWindowViewWithPrevious
    {
        private const string NAME = "Rank";

        /// <summary>The unique-by expressions.</summary>
        private ExprNode[] _uniqueCriteriaExpressions;
        /// <summary>The sort-by expressions.</summary>
        private ExprNode[] _sortCriteriaExpressions;
        /// <summary>The flags defining the ascending or descending sort order.</summary>
        private bool[] _isDescendingValues;
        private ExprEvaluator[] _uniqueEvals;
        private ExprEvaluator[] _sortEvals;
        /// <summary>The sort window size.</summary>
        private ExprEvaluator _sizeEvaluator;
        private bool _useCollatorSort;
        private IList<ExprNode> _viewParameters;
        private EventType _eventType;
    
        public void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParams)
        {
            _viewParameters = viewParams;
        }
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
            const string message = NAME + " view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys";
            if (_viewParameters.Count < 3) {
                throw new ViewParameterException(message);
            }
    
            // validate
            ExprNode[] validated = ViewFactorySupport.Validate(NAME, parentEventType, statementContext, _viewParameters, true);
    
            // find size-parameter index
            int indexNumericSize = -1;
            for (int i = 0; i < validated.Length; i++) {
                if (validated[i] is ExprConstantNode || validated[i] is ExprContextPropertyNode) {
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
            for (int i = 0; i < indexNumericSize; i++) {
                ViewFactorySupport.AssertReturnsNonConstant(NAME, validated[i], i);
            }
            for (int i = indexNumericSize + 1; i < validated.Length; i++) {
                ViewFactorySupport.AssertReturnsNonConstant(NAME, validated[i], i);
            }
    
            // get sort size
            ViewFactorySupport.ValidateNoProperties(ViewName, validated[indexNumericSize], indexNumericSize);
            _sizeEvaluator = ViewFactorySupport.ValidateSizeParam(ViewName, statementContext, validated[indexNumericSize], indexNumericSize);
    
            // compile unique expressions
            _uniqueCriteriaExpressions = new ExprNode[indexNumericSize];
            Array.Copy(validated, 0, _uniqueCriteriaExpressions, 0, indexNumericSize);
    
            // compile sort expressions
            _sortCriteriaExpressions = new ExprNode[validated.Length - indexNumericSize - 1];
            _isDescendingValues = new bool[_sortCriteriaExpressions.Length];
    
            int count = 0;
            for (int i = indexNumericSize + 1; i < validated.Length; i++) {
                if (validated[i] is ExprOrderedExpr) {
                    _isDescendingValues[count] = ((ExprOrderedExpr) validated[i]).IsDescending;
                    _sortCriteriaExpressions[count] = validated[i].ChildNodes[0];
                } else {
                    _sortCriteriaExpressions[count] = validated[i];
                }
                count++;
            }
    
            _uniqueEvals = ExprNodeUtility.GetEvaluators(_uniqueCriteriaExpressions);
            _sortEvals = ExprNodeUtility.GetEvaluators(_sortCriteriaExpressions);
    
            if (statementContext.ConfigSnapshot != null) {
                _useCollatorSort = statementContext.ConfigSnapshot.EngineDefaults.Language.IsSortUsingCollator;
            }
        }
    
        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext) {
            int sortWindowSize = ViewFactorySupport.EvaluateSizeParam(ViewName, _sizeEvaluator, agentInstanceViewFactoryContext.AgentInstanceContext);
            IStreamSortRankRandomAccess rankedRandomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new RankWindowView(this, _uniqueCriteriaExpressions, _uniqueEvals, _sortCriteriaExpressions, _sortEvals, _isDescendingValues, sortWindowSize, rankedRandomAccess, _useCollatorSort, agentInstanceViewFactoryContext);
        }
    
        public Object MakePreviousGetter() {
            return new RandomAccessByIndexGetter();
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            if (!(view is SortWindowView)) {
                return false;
            }
    
            SortWindowView other = (SortWindowView) view;
            int sortWindowSize = ViewFactorySupport.EvaluateSizeParam(ViewName, _sizeEvaluator, agentInstanceContext);
            if ((other.SortWindowSize != sortWindowSize) ||
                    (!Compare(other.IsDescendingValues, _isDescendingValues)) ||
                    (!ExprNodeUtility.DeepEquals(other.SortCriteriaExpressions, _sortCriteriaExpressions, false))) {
                return false;
            }
    
            return other.IsEmpty();
        }
    
        private bool Compare(bool[] one, bool[] two) {
            if (one.Length != two.Length) {
                return false;
            }
    
            for (int i = 0; i < one.Length; i++) {
                if (one[i] != two[i]) {
                    return false;
                }
            }
    
            return true;
        }

        public string ViewName
        {
            get { return NAME; }
        }

        public bool[] IsDescendingValues
        {
            get { return _isDescendingValues; }
        }

        public ExprEvaluator[] UniqueEvals
        {
            get { return _uniqueEvals; }
        }

        public ExprEvaluator[] SortEvals
        {
            get { return _sortEvals; }
        }

        public bool UseCollatorSort
        {
            get { return _useCollatorSort; }
        }
    }
} // end of namespace
