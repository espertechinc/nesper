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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.firstlength
{
    /// <summary>
    ///     A length-first view takes the first N arriving events. Further arriving insert stream events are disregarded until
    ///     events are deleted.
    ///     <para />
    ///     Remove stream events delete from the data window.
    /// </summary>
    public class FirstLengthWindowView : ViewSupport,
        DataWindowView
    {
        protected internal readonly AgentInstanceContext agentInstanceContext;
        private readonly FirstLengthWindowViewFactory lengthFirstFactory;
        protected internal LinkedHashSet<EventBean> indexedEvents;

        public FirstLengthWindowView(
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            FirstLengthWindowViewFactory lengthFirstWindowViewFactory,
            int size)
        {
            if (size < 1) {
                throw new ArgumentException("Illegal argument for size of length window");
            }

            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            lengthFirstFactory = lengthFirstWindowViewFactory;
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

        public LinkedHashSet<EventBean> IndexedEvents => indexedEvents;

        public ViewFactory ViewFactory => lengthFirstFactory;

        public override EventType EventType => parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, lengthFirstFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(lengthFirstFactory, newData, oldData);

            OneEventCollection newDataToPost = null;
            OneEventCollection oldDataToPost = null;

            // add data points to the window as long as its not full, ignoring later events
            if (newData != null) {
                foreach (var aNewData in newData) {
                    if (indexedEvents.Count < Size) {
                        if (newDataToPost == null) {
                            newDataToPost = new OneEventCollection();
                        }

                        newDataToPost.Add(aNewData);
                        indexedEvents.Add(aNewData);
                    }
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    var removed = indexedEvents.Remove(anOldData);
                    if (removed) {
                        if (oldDataToPost == null) {
                            oldDataToPost = new OneEventCollection();
                        }

                        oldDataToPost.Add(anOldData);
                    }
                }
            }

            // If there are child views, call update method
            if (child != null && (newDataToPost != null || oldDataToPost != null)) {
                var nd = newDataToPost != null ? newDataToPost.ToArray() : null;
                var od = oldDataToPost != null ? oldDataToPost.ToArray() : null;
                agentInstanceContext.InstrumentationProvider.QViewIndicate(lengthFirstFactory, nd, od);
                child.Update(nd, od);
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
            viewDataVisitor.VisitPrimary(indexedEvents, true, lengthFirstFactory.ViewName, null);
        }

        public override string ToString()
        {
            return GetType().Name + " size=" + Size;
        }
    }
} // end of namespace