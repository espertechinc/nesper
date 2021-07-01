///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.rank
{
    /// <summary>
    ///     Factory for rank window views.
    /// </summary>
    public class RankWindowViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        public EventType EventType { get; set; }

        public bool[] IsDescendingValues { get; set; }

        public ExprEvaluator CriteriaEval { get; set; }

        public ExprEvaluator[] SortCriteriaEvaluators { get; set; }

        public bool IsUseCollatorSort { get; set; }

        public ExprEvaluator SizeEvaluator { get; set; }

        public IComparer<object> Comparer { get; private set; }

        public Type[] SortCriteriaTypes { get; set; }

        public Type[] CriteriaTypes { get; set; }

        public DataInputOutputSerde KeySerde { get; set; }
        
        public DataInputOutputSerde[] SortSerdes { get; set; }
        
        public string ViewName => ViewEnum.RANK_WINDOW.GetViewName();

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            Comparer = ExprNodeUtilityMake.GetComparatorHashableMultiKeys(
                SortCriteriaTypes,
                IsUseCollatorSort,
                IsDescendingValues); // hashable-key comparator since we may remove sort keys
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            int sortWindowSize = ViewFactoryUtil.EvaluateSizeParam(
                ViewName,
                SizeEvaluator,
                agentInstanceViewFactoryContext.AgentInstanceContext);
            IStreamSortRankRandomAccess rankedRandomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new RankWindowView(this, sortWindowSize, rankedRandomAccess, agentInstanceViewFactoryContext);
        }

        PreviousGetterStrategy DataWindowViewWithPrevious.MakePreviousGetter()
        {
            return MakePreviousGetter();
        }

        public RandomAccessByIndexGetter MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    }
} // end of namespace