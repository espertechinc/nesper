///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.lastevent
{
    /// <summary>
    ///     This view is a very simple view presenting the last event posted by the parent view to any subviews.
    ///     Only the very last event object is kept by this view. The update method invoked by the parent view supplies
    ///     new data in an object array, of which the view keeps the very last instance as the 'last' or newest event.
    ///     The view always has the same schema as the parent view and attaches to anything, and accepts no parameters.
    ///     <para />
    ///     Thus if 5 pieces of new data arrive, the child view receives 5 elements of new data
    ///     and also 4 pieces of old data which is the first 4 elements of new data.
    ///     I.e. New data elements immediatly gets to be old data elements.
    ///     <para />
    ///     Old data received from parent is not handled, it is ignored.
    ///     We thus post old data as follows:
    ///     last event is not null +
    ///     new data from index zero to N-1, where N is the index of the last element in new data
    /// </summary>
    public class LastEventView : ViewSupport,
        DataWindowView
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly LastEventViewFactory _viewFactory;

        /// <summary>
        ///     The last new element posted from a parent view.
        /// </summary>
        private EventBean _lastEvent;

        public LastEventView(
            LastEventViewFactory viewFactory,
            AgentInstanceContext agentInstanceContext)
        {
            _viewFactory = viewFactory;
            _agentInstanceContext = agentInstanceContext;
        }

        public ViewFactory ViewFactory => _viewFactory;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            _agentInstanceContext.AuditProvider.View(newData, oldData, _agentInstanceContext, _viewFactory);
            _agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(_viewFactory, newData, oldData);

            OneEventCollection oldDataToPost = null;

            if (newData != null && newData.Length == 1 && (oldData == null || oldData.Length == 0)) {
                var currentLast = _lastEvent;
                _lastEvent = newData[0];
                if (child != null) {
                    var oldDataToPostHere = currentLast == null ? null : new[] { currentLast };
                    _agentInstanceContext.InstrumentationProvider.QViewIndicate(
                        _viewFactory,
                        newData,
                        oldDataToPostHere);
                    child.Update(newData, oldDataToPostHere);
                    _agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }
            else {
                if (newData != null && newData.Length != 0) {
                    if (_lastEvent != null) {
                        oldDataToPost = new OneEventCollection();
                        oldDataToPost.Add(_lastEvent);
                    }

                    if (newData.Length > 1) {
                        for (var i = 0; i < newData.Length - 1; i++) {
                            if (oldDataToPost == null) {
                                oldDataToPost = new OneEventCollection();
                            }

                            oldDataToPost.Add(newData[i]);
                        }
                    }

                    _lastEvent = newData[^1];
                }

                if (oldData != null) {
                    for (var i = 0; i < oldData.Length; i++) {
                        if (oldData[i] == _lastEvent) {
                            if (oldDataToPost == null) {
                                oldDataToPost = new OneEventCollection();
                            }

                            oldDataToPost.Add(oldData[i]);
                            _lastEvent = null;
                        }
                    }
                }

                // If there are child views, fireStatementStopped update method
                if (child != null) {
                    if (oldDataToPost != null && !oldDataToPost.IsEmpty()) {
                        var oldDataArray = oldDataToPost.ToArray();
                        _agentInstanceContext.InstrumentationProvider.QViewIndicate(
                            _viewFactory,
                            newData,
                            oldDataArray);
                        child.Update(newData, oldDataArray);
                        _agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                    else {
                        _agentInstanceContext.InstrumentationProvider.QViewIndicate(_viewFactory, newData, null);
                        child.Update(newData, null);
                        _agentInstanceContext.InstrumentationProvider.AViewIndicate();
                    }
                }
            }

            _agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.SingletonNullable(_lastEvent);
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_lastEvent, _viewFactory.ViewName);
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
} // end of namespace