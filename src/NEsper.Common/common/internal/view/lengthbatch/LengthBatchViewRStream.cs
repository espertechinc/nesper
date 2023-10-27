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

namespace com.espertech.esper.common.@internal.view.lengthbatch
{
    /// <summary>
    ///     Same as the <seealso cref="LengthBatchView" />, this view also supports fast-remove from the batch for remove
    ///     stream events.
    /// </summary>
    public class LengthBatchViewRStream : ViewSupport,
        DataWindowView
    {
        // View parameters
        protected internal readonly AgentInstanceContext agentInstanceContext;
        private readonly LengthBatchViewFactory lengthBatchViewFactory;
        protected internal LinkedHashSet<EventBean> currentBatch = new LinkedHashSet<EventBean>();

        // Current running windows
        protected internal LinkedHashSet<EventBean> lastBatch;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="size">is the number of events to batch</param>
        /// <param name="lengthBatchViewFactory">for copying this view in a group-by</param>
        /// <param name="agentInstanceViewFactoryContext">context</param>
        public LengthBatchViewRStream(
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
            LengthBatchViewFactory lengthBatchViewFactory,
            int size)
        {
            this.lengthBatchViewFactory = lengthBatchViewFactory;
            Size = size;
            agentInstanceContext = agentInstanceViewFactoryContext.AgentInstanceContext;

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

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, lengthBatchViewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(lengthBatchViewFactory, newData, oldData);

            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    if (currentBatch.Remove(oldData[i])) {
                        InternalHandleRemoved(oldData[i]);
                    }
                }
            }

            // we don't care about removed data from a prior view
            if (newData == null || newData.Length == 0) {
                agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
                return;
            }

            // add data points to the current batch
            foreach (var newEvent in newData) {
                currentBatch.Add(newEvent);
            }

            // check if we reached the minimum size
            if (currentBatch.Count < Size) {
                // done if no overflow
                agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
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

        public void InternalHandleRemoved(EventBean oldData)
        {
            // no action required
        }

        /// <summary>
        ///     This method updates child views and clears the batch of events.
        /// </summary>
        protected void SendBatch()
        {
            // If there are child views and the batch was filled, fireStatementStopped update method
            if (Child != null) {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (!currentBatch.IsEmpty()) {
                    newData = currentBatch.ToArray();
                }

                if (lastBatch != null && !lastBatch.IsEmpty()) {
                    oldData = lastBatch.ToArray();
                }

                // Post new data (current batch) and old data (prior batch)
                if (newData != null || oldData != null) {
                    agentInstanceContext.InstrumentationProvider.QViewIndicate(
                        lengthBatchViewFactory,
                        newData,
                        oldData);
                    Child.Update(newData, oldData);
                    agentInstanceContext.InstrumentationProvider.AViewIndicate();
                }
            }

            lastBatch = currentBatch;
            currentBatch = new LinkedHashSet<EventBean>();
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