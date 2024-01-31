///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.common.@internal.view.sort
{
    /// <summary>
    ///     Factory for sort window views.
    /// </summary>
    public class SortWindowViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        private EventType _eventType;

        public EventType EventType {
            get => _eventType;
            set => _eventType = value;
        }

        public ExprEvaluator[] SortCriteriaEvaluators { get; set; }

        public ExprEvaluator SizeEvaluator { get; set; }

        public IComparer<object> IComparer { get; set; }

        public bool[] IsDescendingValues { get; set; }

        public bool IsUseCollatorSort {
            get => UseCollatorSort;
            set => UseCollatorSort = value;
        }

        public Type[] SortCriteriaTypes { get; set; }

        public bool UseCollatorSort { get; set; }

        public DataInputOutputSerde[] SortSerdes { get; set; }


        public string ViewName => ViewEnum.SORT_WINDOW.GetViewName();

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            IComparer = ExprNodeUtilityMake.GetComparatorHashableMultiKeys(
                SortCriteriaTypes,
                UseCollatorSort,
                IsDescendingValues); // hashable-key comparator since we may remove sort keys
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var sortWindowSize = ViewFactoryUtil.EvaluateSizeParam(
                ViewName,
                SizeEvaluator,
                agentInstanceViewFactoryContext.AgentInstanceContext);
            var sortedRandomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprSortedRankedAccess(agentInstanceViewFactoryContext);
            return new SortWindowView(this, sortWindowSize, sortedRandomAccess, agentInstanceViewFactoryContext);
        }

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        private bool Compare(
            bool[] one,
            bool[] two)
        {
            if (one.Length != two.Length) {
                return false;
            }

            for (var i = 0; i < one.Length; i++) {
                if (one[i] != two[i]) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace