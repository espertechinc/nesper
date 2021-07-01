///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.lengthbatch
{
    /// <summary>
    ///     A data view that aggregates events in a stream and releases them in one batch when a maximum number of events has
    ///     been collected.
    ///     <para />
    ///     The view works similar to a length_window but is not continuous, and similar to a time_batch however is not
    ///     time-based
    ///     but reacts to the number of events.
    ///     <para />
    ///     The view releases the batched events, when a certain number of batched events has been reached or exceeded,
    ///     as new data to child views. The prior batch if
    ///     not empty is released as old data to any child views. The view doesn't release intervals with no old or new data.
    ///     It also does not collect old data published by a parent view.
    ///     <para />
    ///     If there are no events in the current and prior batch, the view will not invoke the update method of child views.
    /// </summary>
    public class LengthBatchView : ViewSupport,
        DataWindowView
    {
        protected internal readonly AgentInstanceContext agentInstanceContext;
        private readonly LengthBatchViewFactory lengthBatchViewFactory;
        private readonly ViewUpdatedCollection viewUpdatedCollection;
        protected internal ArrayDeque<EventBean> currentBatch = new ArrayDeque<EventBean>();

        // Current running windows
        protected internal ArrayDeque<EventBean> lastBatch;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="size">is the number of events to batch</param>
        /// <param name="viewUpdatedCollection">is a collection that the view must update when receiving events</param>
        /// <param name="lengthBatchViewFactory">for copying this view in a group-by</param>
        /// <param name="agentInstanceContext">context</param>
        public LengthBatchView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            LengthBatchViewFactory lengthBatchViewFactory,
            int size,
            ViewUpdatedCollection viewUpdatedCollection)
        {
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            this.lengthBatchViewFactory = lengthBatchViewFactory;
            Size = size;
            this.viewUpdatedCollection = viewUpdatedCollection;

            if (size <= 0) {
                throw new ArgumentException("Invalid size parameter, size=" + size);
            }
        }

        /// <summary>
        ///     Returns the number of events to batch (data window size).
        /// </summary>
        /// <returns>batch size</returns>
        public int Size { get; }

        public ViewFactory ViewFactory => lengthBatchViewFactory;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, lengthBatchViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(lengthBatchViewFactory, newData, oldData);

            // we don't care about removed data from a prior view
            if (newData == null || newData.Length == 0) {
                return;
            }

            // add data points to the current batch
            foreach (var newEvent in newData) {
                currentBatch.Add(newEvent);
            }

            // check if we reached the minimum size
            if (currentBatch.Count < Size) {
                // done if no overflow
                return;
            }

            SendBatch();

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return currentBatch.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(currentBatch, true, lengthBatchViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(lastBatch, true, lengthBatchViewFactory.ViewName, null);
        }

        /// <summary>
        ///     This method updates child views and clears the batch of events.
        /// </summary>
        protected void SendBatch()
        {
            // If there are child views and the batch was filled, fireStatementStopped update method
            if (child != null) {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (!currentBatch.IsEmpty()) {
                    newData = currentBatch.ToArray();
                }

                if (lastBatch != null && !lastBatch.IsEmpty()) {
                    oldData = lastBatch.ToArray();
                }

                // update view buffer to serve expressions require access to events held
                viewUpdatedCollection?.Update(newData, oldData);

                // Post new data (current batch) and old data (prior batch)
                if (newData != null || oldData != null) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(
                        lengthBatchViewFactory,
                        newData,
                        oldData);
                    child.Update(newData, oldData);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            lastBatch = currentBatch;
            currentBatch = new ArrayDeque<EventBean>();
        }

        /// <summary>
        ///     Returns true if the window is empty, or false if not empty.
        /// </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (lastBatch != null) {
                if (!lastBatch.IsEmpty()) {
                    return false;
                }
            }

            return currentBatch.IsEmpty();
        }

        public override string ToString()
        {
            return GetType().Name +
                   " size=" +
                   Size;
        }
    }
} // end of namespace