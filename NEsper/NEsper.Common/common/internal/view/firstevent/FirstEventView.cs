///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.view.firstevent
{
    /// <summary>
    /// View retaining the very first event. Any subsequent events received are simply discarded and not
    /// entered into either insert or remove stream. Only the very first event received is entered into the remove stream.
    /// <para />The view thus never posts a remove stream unless explicitly deleted from when used with a named window.
    /// </summary>
    public class FirstEventView : ViewSupport,
        DataWindowView
    {
        /// <summary>
        /// The first new element posted from a parent view.
        /// </summary>
        private readonly FirstEventViewFactory viewFactory;

        private readonly AgentInstanceContext agentInstanceContext;
        protected internal EventBean firstEvent;

        public FirstEventView(
            FirstEventViewFactory viewFactory,
            AgentInstanceContext agentInstanceContext)
        {
            this.viewFactory = viewFactory;
            this.agentInstanceContext = agentInstanceContext;
        }

        public override EventType EventType {
            get {
                // The schema is the parent view's schema
                return parent.EventType;
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, viewFactory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(viewFactory, newData, oldData);

            EventBean[] newDataToPost = null;
            EventBean[] oldDataToPost = null;

            if (oldData != null) {
                for (int i = 0; i < oldData.Length; i++) {
                    if (oldData[i] == firstEvent) {
                        oldDataToPost = new EventBean[] {firstEvent};
                        firstEvent = null;
                    }
                }
            }

            if ((newData != null) && (newData.Length != 0)) {
                if (firstEvent == null) {
                    firstEvent = newData[0];
                    newDataToPost = new EventBean[] {firstEvent};
                }
            }

            if ((child != null) && ((newDataToPost != null) || (oldDataToPost != null))) {
                agentInstanceContext.InstrumentationProvider.QViewIndicate(viewFactory, newDataToPost, oldDataToPost);
                child.Update(newDataToPost, oldDataToPost);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            yield return firstEvent;
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }

        public EventBean FirstEvent {
            get => this.firstEvent;
            set => this.firstEvent = value;
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(firstEvent, FirstEventViewFactory.NAME);
        }

        public ViewFactory ViewFactory {
            get => viewFactory;
        }
    }
} // end of namespace