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
        protected internal IComparer<object> comparator;
        protected internal EventType eventType;
        protected internal bool[] isDescendingValues;
        protected internal ExprEvaluator size;
        protected internal ExprEvaluator[] sortCriteriaEvaluators;
        protected internal Type[] sortCriteriaTypes;
        protected internal bool useCollatorSort;

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public ExprEvaluator[] SortCriteriaEvaluators {
            get => sortCriteriaEvaluators;
            set => sortCriteriaEvaluators = value;
        }

        public ExprEvaluator Size {
            get => size;
            set => size = value;
        }

        public IComparer<object> IComparer => comparator;

        public bool[] IsDescendingValues {
            get => isDescendingValues;
            set => isDescendingValues = value;
        }

        public bool IsUseCollatorSort => useCollatorSort;

        public Type[] SortCriteriaTypes {
            set => sortCriteriaTypes = value;
        }

        public bool UseCollatorSort {
            set => useCollatorSort = value;
        }

        public string ViewName => ViewEnum.SORT_WINDOW.GetViewName();

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            comparator = ExprNodeUtilityMake.GetComparatorHashableMultiKeys(
                sortCriteriaTypes, useCollatorSort,
                isDescendingValues); // hashable-key comparator since we may remove sort keys
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var sortWindowSize = ViewFactoryUtil.EvaluateSizeParam(
                ViewName, size, agentInstanceViewFactoryContext.AgentInstanceContext);
            IStreamSortRankRandomAccess sortedRandomAccess =
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