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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.length
{
    /// <summary>
    ///     This view is a moving window extending the specified number of elements into the past,
    ///     allowing in addition to remove events efficiently for remove-stream events received by the view.
    /// </summary>
    public class LengthWindowViewRStream : ViewSupport,
        DataWindowView
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly LengthWindowViewFactory lengthWindowViewFactory;
        private readonly LinkedHashSet<EventBean> indexedEvents;

        /// <summary>
        ///     Constructor creates a moving window extending the specified number of elements into the past.
        /// </summary>
        /// <param name="size">is the specified number of elements into the past</param>
        /// <param name="lengthWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="agentInstanceContext">context</param>
        public LengthWindowViewRStream(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            LengthWindowViewFactory lengthWindowViewFactory,
            int size)
        {
            if (size < 1) {
                throw new ArgumentException("Illegal argument for size of length window");
            }

            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.lengthWindowViewFactory = lengthWindowViewFactory;
            Size = size;
            indexedEvents = new LinkedHashSet<EventBean>();
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty => indexedEvents.IsEmpty();

        /// <summary>
        ///     Returns the size of the length window.
        /// </summary>
        /// <returns>size of length window</returns>
        public int Size { get; }

        public ViewFactory ViewFactory => lengthWindowViewFactory;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, lengthWindowViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(
                lengthWindowViewFactory,
                newData,
                oldData);

            EventBean[] expiredArr = null;
            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    indexedEvents.Remove(anOldData);
                    InternalHandleRemoved(anOldData);
                }

                expiredArr = oldData;
            }

            // add data points to the window
            // we don't care about removed data from a prior view
            if (newData != null) {
                foreach (var newEvent in newData) {
                    indexedEvents.Add(newEvent);
                    InternalHandleAdded(newEvent);
                }
            }

            // Check for any events that get pushed out of the window
            var expiredCount = indexedEvents.Count - Size;
            if (expiredCount > 0) {
                expiredArr = new EventBean[expiredCount];
                var it = indexedEvents.GetEnumerator();
                for (var i = 0; i < expiredCount; i++) {
                    expiredArr[i] = it.Current;
                }

                foreach (var anExpired in expiredArr) {
                    indexedEvents.Remove(anExpired);
                    InternalHandleExpired(anExpired);
                }
            }

            // If there are child views, call update method
            if (child != null) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(
                    lengthWindowViewFactory,
                    newData,
                    expiredArr);
                child.Update(newData, expiredArr);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return indexedEvents.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(indexedEvents, true, lengthWindowViewFactory.ViewName, null);
        }

        public void InternalHandleExpired(EventBean oldData)
        {
            // no action required
        }

        public void InternalHandleRemoved(EventBean expiredData)
        {
            // no action required
        }

        public void InternalHandleAdded(EventBean newData)
        {
            // no action required
        }

        public override string ToString()
        {
            return GetType().Name + " size=" + Size;
        }
    }
} // end of namespace