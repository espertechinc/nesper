///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.exttimedbatch
{
    /// <summary>
    ///     Batch window based on timestamp of arriving events.
    /// </summary>
    public class ExternallyTimedBatchView : ViewSupport,
        DataWindowView
    {
        private readonly EventBean[] eventsPerStream = new EventBean[1];
        private readonly ExternallyTimedBatchViewFactory factory;
        private readonly TimePeriodProvide timePeriodProvide;

        protected internal readonly ISet<EventBean> window = new LinkedHashSet<EventBean>();
        protected internal AgentInstanceContext agentInstanceContext;
        protected internal EventBean[] lastBatch;
        protected internal long? oldestTimestamp;
        protected internal long? referenceTimestamp;

        protected ViewUpdatedCollection viewUpdatedCollection;

        public ExternallyTimedBatchView(
            ExternallyTimedBatchViewFactory factory,
            ViewUpdatedCollection viewUpdatedCollection,
            AgentInstanceViewFactoryChainContext agentInstanceContext,
            TimePeriodProvide timePeriodProvide)
        {
            this.factory = factory;
            this.viewUpdatedCollection = viewUpdatedCollection;
            this.agentInstanceContext = agentInstanceContext.AgentInstanceContext;
            referenceTimestamp = factory.optionalReferencePoint;
            this.timePeriodProvide = timePeriodProvide;
        }

        public bool IsEmpty => window.IsEmpty();

        public ViewFactory ViewFactory => factory;

        public override EventType EventType => Parent.EventType;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            agentInstanceContext.AuditProvider.View(newData, oldData, agentInstanceContext, factory);
            agentInstanceContext.InstrumentationProvider.QViewProcessIRStream(factory, newData, oldData);

            // remove points from data window
            if (oldData != null && oldData.Length != 0) {
                foreach (var anOldData in oldData) {
                    window.Remove(anOldData);
                    HandleInternalRemovedEvent(anOldData);
                }

                DetermineOldestTimestamp();
            }

            // add data points to the window
            EventBean[] batchNewData = null;
            if (newData != null) {
                foreach (var newEvent in newData) {
                    var timestamp = GetLongValue(newEvent);
                    if (referenceTimestamp == null) {
                        referenceTimestamp = timestamp;
                    }

                    if (oldestTimestamp == null) {
                        oldestTimestamp = timestamp;
                    }
                    else {
                        var delta = timePeriodProvide.DeltaAddWReference(
                            oldestTimestamp.Value,
                            referenceTimestamp.Value,
                            null,
                            true,
                            agentInstanceContext);
                        referenceTimestamp = delta.LastReference;
                        if (timestamp - oldestTimestamp >= delta.Delta) {
                            if (batchNewData == null) {
                                batchNewData = window.ToArray();
                            }
                            else {
                                batchNewData = EventBeanUtility.AddToArray(batchNewData, window);
                            }

                            window.Clear();
                            oldestTimestamp = null;
                        }
                    }

                    window.Add(newEvent);
                    HandleInternalAddEvent(newEvent, batchNewData != null);
                }
            }

            if (batchNewData != null) {
                HandleInternalPostBatch(window, batchNewData);
                viewUpdatedCollection?.Update(batchNewData, lastBatch);

                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, batchNewData, lastBatch);
                child.Update(batchNewData, lastBatch);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();

                lastBatch = batchNewData;
                DetermineOldestTimestamp();
            }

            if (oldData != null && oldData.Length > 0) {
                viewUpdatedCollection?.Update(null, oldData);

                agentInstanceContext.InstrumentationProvider.QViewIndicate(factory, null, oldData);
                child.Update(null, oldData);
                agentInstanceContext.InstrumentationProvider.AViewIndicate();
            }

            agentInstanceContext.InstrumentationProvider.AViewProcessIRStream();
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return window.GetEnumerator();
        }

        public void VisitView(ViewDataVisitor viewDataVisitor)
        {
            viewDataVisitor.VisitPrimary(window, true, factory.ViewName, null);
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        protected void DetermineOldestTimestamp()
        {
            if (window.IsEmpty()) {
                oldestTimestamp = null;
            }
            else {
                oldestTimestamp = GetLongValue(window.First());
            }
        }

        protected void HandleInternalPostBatch(
            ISet<EventBean> window,
            EventBean[] batchNewData)
        {
            // no action require
        }

        protected void HandleInternalRemovedEvent(EventBean anOldData)
        {
            // no action require
        }

        protected void HandleInternalAddEvent(
            EventBean anNewData,
            bool isNextBatch)
        {
            // no action require
        }

        private long GetLongValue(EventBean obj)
        {
            eventsPerStream[0] = obj;
            var num = factory.timestampEval.Evaluate(eventsPerStream, true, agentInstanceContext);
            return num.AsInt64();
        }
    }
} // end of namespace