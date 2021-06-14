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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    /// <summary>
    ///     Represents a consumer of a named window that selects from a named window via a from-clause.
    ///     <para />
    ///     The view simply dispatches directly to child views, and keeps the last new event for iteration.
    /// </summary>
    public class NamedWindowConsumerView : ViewSupport,
        AgentInstanceMgmtCallback
    {
        private readonly bool audit;
        private readonly EventType eventType;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly FlushedEventBuffer optPropertyContainedBuffer;
        private readonly PropertyEvaluator optPropertyEvaluator;

        public NamedWindowConsumerView(
            int namedWindowConsumerId,
            ExprEvaluator filter,
            PropertyEvaluator optPropertyEvaluator,
            EventType eventType,
            NamedWindowConsumerCallback consumerCallback,
            ExprEvaluatorContext exprEvaluatorContext,
            bool audit)
        {
            NamedWindowConsumerId = namedWindowConsumerId;
            Filter = filter;
            this.optPropertyEvaluator = optPropertyEvaluator;
            if (optPropertyEvaluator != null) {
                optPropertyContainedBuffer = new FlushedEventBuffer();
            }
            else {
                optPropertyContainedBuffer = null;
            }

            this.eventType = eventType;
            ConsumerCallback = consumerCallback;
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.audit = audit;
        }

        public int NamedWindowConsumerId { get; }

        public NamedWindowConsumerCallback ConsumerCallback { get; }

        public ExprEvaluator Filter { get; }

        public void Stop(AgentInstanceStopServices services)
        {
            ConsumerCallback.Stopped(this);
        }
        
        public void Transfer(AgentInstanceTransferServices services)
        {
            // no action required
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (audit) {
                exprEvaluatorContext.AuditProvider.StreamMulti(newData, oldData, exprEvaluatorContext, eventType.Name);
            }

            // if we have a filter for the named window,
            if (Filter != null) {
                var eventPerStream = new EventBean[1];
                newData = PassFilter(newData, true, exprEvaluatorContext, eventPerStream);
                oldData = PassFilter(oldData, false, exprEvaluatorContext, eventPerStream);
            }

            if (optPropertyEvaluator != null) {
                newData = GetUnpacked(newData);
                oldData = GetUnpacked(oldData);
            }

            if (newData != null || oldData != null) {
                Child?.Update(newData, oldData);
            }
        }

        private EventBean[] GetUnpacked(EventBean[] data)
        {
            if (data == null) {
                return null;
            }

            if (data.Length == 0) {
                return data;
            }

            for (var i = 0; i < data.Length; i++) {
                var unpacked = optPropertyEvaluator.GetProperty(data[i], exprEvaluatorContext);
                optPropertyContainedBuffer.Add(unpacked);
            }

            return optPropertyContainedBuffer.GetAndFlush();
        }

        private EventBean[] PassFilter(
            EventBean[] eventData,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext,
            EventBean[] eventPerStream)
        {
            if (eventData == null || eventData.Length == 0) {
                return null;
            }

            if (eventData.Length == 1) {
                eventPerStream[0] = eventData[0];
                var result = Filter.Evaluate(eventPerStream, isNewData, exprEvaluatorContext);
                return result != null && true.Equals(result) ? eventData : null;
            }

            OneEventCollection filtered = null;
            foreach (var theEvent in eventData) {
                eventPerStream[0] = theEvent;
                var result = Filter.Evaluate(eventPerStream, isNewData, exprEvaluatorContext);
                if (result == null || false.Equals(result)) {
                    continue;
                }

                if (filtered == null) {
                    filtered = new OneEventCollection();
                }

                filtered.Add(theEvent);
            }

            return filtered?.ToArray();
        }

        public override EventType EventType {
            get {
                if (optPropertyEvaluator != null) {
                    return optPropertyEvaluator.FragmentEventType;
                }

                return eventType;
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (Filter == null) {
                return ConsumerCallback.GetEnumerator();
            }

            return FilteredEventEnumerator.For(Filter, ConsumerCallback.GetEnumerator(), exprEvaluatorContext);
        }
    }
} // end of namespace