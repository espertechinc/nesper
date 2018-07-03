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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// A length-first view takes the first Count arriving events. Further arriving insert stream 
    /// events are disregarded until events are deleted. <para/>Remove stream events delete 
    /// from the data window.
    /// </summary>
    public class FirstLengthWindowView : ViewSupport, DataWindowView, CloneableView
    {
        protected readonly AgentInstanceViewFactoryChainContext AgentInstanceViewFactoryContext;
        private readonly FirstLengthWindowViewFactory _lengthFirstFactory;
        private readonly LinkedHashSet<EventBean> _indexedEvents;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceViewFactoryContext">The agent instance view factory context.</param>
        /// <param name="lengthFirstWindowViewFactory">for copying this view in a group-by</param>
        /// <param name="size">the first Count events to consider</param>
        public FirstLengthWindowView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext, FirstLengthWindowViewFactory lengthFirstWindowViewFactory, int size)
        {
            if (size < 1)
            {
                throw new ArgumentException("Illegal argument for size of length window");
            }

            AgentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            _lengthFirstFactory = lengthFirstWindowViewFactory;
            Size = size;
            _indexedEvents = new LinkedHashSet<EventBean>();
        }

        public View CloneView()
        {
            return _lengthFirstFactory.MakeView(AgentInstanceViewFactoryContext);
        }

        /// <summary>Returns true if the window is empty, or false if not empty. </summary>
        /// <returns>true if empty</returns>
        public bool IsEmpty()
        {
            return _indexedEvents.IsEmpty;
        }

        /// <summary>Returns the size of the length window. </summary>
        /// <value>size of length window</value>
        public int Size { get; private set; }

        public override EventType EventType => Parent.EventType;

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewProcessIRStream(this, _lengthFirstFactory.ViewName, newData, oldData); }

            OneEventCollection newDataToPost = null;
            OneEventCollection oldDataToPost = null;

            // add data points to the window as long as its not full, ignoring later events
            if (newData != null)
            {
                foreach (EventBean aNewData in newData)
                {
                    if (_indexedEvents.Count < Size)
                    {
                        if (newDataToPost == null)
                        {
                            newDataToPost = new OneEventCollection();
                        }
                        newDataToPost.Add(aNewData);
                        _indexedEvents.Add(aNewData);
                        InternalHandleAdded(aNewData);
                    }
                }
            }

            if (oldData != null)
            {
                foreach (EventBean anOldData in oldData)
                {
                    bool removed = _indexedEvents.Remove(anOldData);
                    if (removed)
                    {
                        if (oldDataToPost == null)
                        {
                            oldDataToPost = new OneEventCollection();
                        }
                        oldDataToPost.Add(anOldData);
                        InternalHandleRemoved(anOldData);
                    }
                }
            }

            // If there are child views, call Update method
            if (HasViews && ((newDataToPost != null) || (oldDataToPost != null)))
            {
                EventBean[] nd = (newDataToPost != null) ? newDataToPost.ToArray() : null;
                EventBean[] od = (oldDataToPost != null) ? oldDataToPost.ToArray() : null;
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QViewIndicate(this, _lengthFirstFactory.ViewName, nd, od); }
                UpdateChildren(nd, od);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewIndicate(); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AViewProcessIRStream(); }
        }

        public void InternalHandleRemoved(EventBean anOldData)
        {
            // no action required
        }

        public void InternalHandleAdded(EventBean aNewData)
        {
            // no action required
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _indexedEvents.GetEnumerator();
        }

        public override String ToString()
        {
            return GetType().FullName + " size=" + Size;
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(_indexedEvents, true, _lengthFirstFactory.ViewName, null);
        }

        public LinkedHashSet<EventBean> IndexedEvents => _indexedEvents;

        public ViewFactory ViewFactory => _lengthFirstFactory;
    }
}
