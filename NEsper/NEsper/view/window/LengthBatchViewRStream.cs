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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Same as the <seealso cref="LengthBatchView"/>, this view also supports fast-remove 
    /// from the batch for remove stream events.
    /// </summary>
    public class LengthBatchViewRStream : ViewSupport, CloneableView, DataWindowView
    {
        // View parameters
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;
        private readonly LengthBatchViewFactory _lengthBatchViewFactory;
        private readonly int _size;
    
        // Current running windows
        protected LinkedHashSet<EventBean> LastBatch = null;
        protected LinkedHashSet<EventBean> CurrentBatch = new LinkedHashSet<EventBean>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="lengthBatchViewFactory">for copying this view in a group-by</param>
        /// <param name="size">is the number of events to batch</param>
        public LengthBatchViewRStream(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext,
                                      LengthBatchViewFactory lengthBatchViewFactory,
                                      int size)
        {
            _lengthBatchViewFactory = lengthBatchViewFactory;
            _size = size;
            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
    
            if (size <= 0)
            {
                throw new ArgumentException("Invalid size parameter, size=" + size);
            }
        }
    
        public View CloneView()
        {
            return _lengthBatchViewFactory.MakeView(AgentInstanceViewFactoryContext);
        }

        /// <summary>Returns the number of events to batch (data window size). </summary>
        /// <value>batch size</value>
        public int Size => _size;

        public override EventType EventType => Parent.EventType;

        public void InternalHandleRemoved(EventBean oldData)
        {
            // no action required
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _lengthBatchViewFactory.ViewName, newData, oldData);}
    
            if (oldData != null)
            {
                for (int i = 0; i < oldData.Length; i++)
                {
                    if (CurrentBatch.Remove(oldData[i])) {
                        InternalHandleRemoved(oldData[i]);
                    }
                }
            }
    
            // we don't care about removed data from a prior view
            if ((newData == null) || (newData.Length == 0))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
                return;
            }
    
            // add data points to the current batch
            foreach (EventBean newEvent in newData) {
                CurrentBatch.Add(newEvent);
            }
    
            // check if we reached the minimum size
            if (CurrentBatch.Count < _size)
            {
                // done if no overflow
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
                return;
            }
    
            SendBatch();
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream();}
        }
    
        /// <summary>This method updates child views and clears the batch of events. </summary>
        protected void SendBatch()
        {
            // If there are child views and the batch was filled, fireStatementStopped Update method
            if (HasViews)
            {
                // Convert to object arrays
                EventBean[] newData = null;
                EventBean[] oldData = null;
                if (CurrentBatch.IsNotEmpty())
                {
                    newData = CurrentBatch.ToArray();
                }
                if ((LastBatch != null) && (LastBatch.IsNotEmpty()))
                {
                    oldData = LastBatch.ToArray();
                }
    
                // Post new data (current batch) and old data (prior batch)
                if ((newData != null) || (oldData != null))
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _lengthBatchViewFactory.ViewName, newData, oldData);}
                    UpdateChildren(newData, oldData);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate();}
                }
            }
    
            LastBatch = CurrentBatch;
            CurrentBatch = new LinkedHashSet<EventBean>();
        }
    
        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            if (LastBatch != null)
            {
                if (LastBatch.IsNotEmpty())
                {
                    return false;
                }
            }
            return CurrentBatch.IsEmpty;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CurrentBatch.GetEnumerator();
        }
    
        public override String ToString()
        {
            return GetType().FullName +
                    " size=" + _size;
        }
    
        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(CurrentBatch, true, _lengthBatchViewFactory.ViewName, null);
            viewDataVisitor.VisitPrimary(LastBatch, true, _lengthBatchViewFactory.ViewName, null);
        }

        public ViewFactory ViewFactory => _lengthBatchViewFactory;
    }
}
