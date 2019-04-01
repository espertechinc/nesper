///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    public class MergeView : ViewSupport
    {
        private readonly GroupByView groupByView;
        private readonly ICollection<View> parentViews;

        public MergeView(GroupByView groupByView, EventType eventType)
        {
            this.groupByView = groupByView;
            EventType = eventType;
            parentViews = new List<View>(4);
        }

        public override EventType EventType { get; }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            groupByView.Child.Update(newData, oldData);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            // The merge data view has multiple parent views which are AddPropertyValueView
            ArrayDeque<IEnumerable<EventBean>> iterables = new ArrayDeque<IEnumerable<EventBean>>();

            foreach (var dataView in parentViews) {
                iterables.Add(dataView);
            }

            return new IterablesListIterator(iterables.GetEnumerator());
        }

        public void RemoveParentView(View parentView)
        {
            parentViews.Remove(parentView);
        }

        public void AddParentView(View parentView)
        {
            parentViews.Add(parentView);
        }
    }
} // end of namespace