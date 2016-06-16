///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.view.std
{
    /// <summary>
    /// The merge view works together with a group view that splits the data in a stream to 
    /// multiple subviews, based on a key index. Every group view requires a merge view to 
    /// merge the many subviews back into a single view. Typically the last view in a chain 
    /// containing a group view is a merge view. The merge view has no other responsibility 
    /// then becoming the single last instance in the chain to which external listeners for 
    /// updates can be attached to receive updates for the many subviews that have this merge 
    /// view as common child views. The parent view of this view is generally the AddPropertyValueView 
    /// that adds the grouped-by information back into the data. 
    /// </summary>
    public sealed class MergeView : ViewSupport, CloneableView, MergeViewMarker
    {
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceContext;
        private readonly ICollection<View> _parentViews;
        private readonly ExprNode[] _groupFieldNames;
        private readonly EventType _eventType;
        private readonly bool _removable;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="groupCriteria">is the fields from which to pull the value to group by</param>
        /// <param name="resultEventType">is passed by the factory as the factory adds the merged fields to an event type</param>
        /// <param name="removable">if set to <c>true</c> [removable].</param>
        public MergeView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            ExprNode[] groupCriteria,
            EventType resultEventType,
            bool removable)
        {
            _removable = removable;
            if (!removable)
            {
                _parentViews = new LinkedList<View>();
            }
            else
            {
                _parentViews = new HashSet<View>();
            }
            _agentInstanceContext = agentInstanceContext;
            _groupFieldNames = groupCriteria;
            _eventType = resultEventType;
        }

        public View CloneView()
        {
            return new MergeView(_agentInstanceContext, _groupFieldNames, _eventType, _removable);
        }

        /// <summary>Returns the field name that contains the values to group by. </summary>
        /// <value>field name providing group key value</value>
        public ExprNode[] GroupFieldNames
        {
            get { return _groupFieldNames; }
        }

        /// <summary>Add a parent data merge view. </summary>
        /// <param name="parentView">is the parent data merge view to add</param>
        public void AddParentView(AddPropertyValueView parentView)
        {
            _parentViews.Add(parentView);
        }

        public override EventType EventType
        {
            get
            {
                // The schema is the parent view's type, or the type plus the added Field(s)
                return _eventType;
            }
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            UpdateChildren(newData, oldData);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            // The merge data view has multiple parent views which are AddPropertyValueView
            var iterables = new LinkedList<IEnumerable<EventBean>>();

            foreach (View dataView in _parentViews)
            {
                iterables.AddLast(dataView);
            }

            return iterables.SelectMany(parentEnum => parentEnum).GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName + " groupFieldName=" + _groupFieldNames.Render();
        }

        public void RemoveParentView(View view)
        {
            _parentViews.Remove(view);
        }
    }
}
