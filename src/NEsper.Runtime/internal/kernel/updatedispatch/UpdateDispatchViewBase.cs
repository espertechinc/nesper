///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.kernel.updatedispatch
{
    /// <summary>
    ///     Convenience view for dispatching view updates received from a parent view to update listeners
    ///     via the dispatch service.
    /// </summary>
    public abstract class UpdateDispatchViewBase : ViewSupport,
        Dispatchable,
        UpdateDispatchView
    {
        /// <summary>
        ///     Dispatches events to listeners.
        /// </summary>
        protected readonly DispatchService dispatchService;

        protected readonly EventType eventType;

        /// <summary>
        ///     Handles result delivery
        /// </summary>
        protected readonly StatementResultService statementResultService;

        /// <summary>
        ///     For iteration with patterns.
        /// </summary>
        protected EventBean lastIterableEvent;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="statementResultServiceImpl">handles result delivery</param>
        /// <param name="eventType">event type</param>
        public UpdateDispatchViewBase(
            EventType eventType,
            StatementResultService statementResultServiceImpl,
            DispatchService dispatchService)
        {
            this.dispatchService = dispatchService;
            statementResultService = statementResultServiceImpl;
            this.eventType = eventType;
        }

        public StatementResultService StatementResultService => statementResultService;

        public void Execute()
        {
            var dispatchTLEntry = statementResultService.DispatchTL.GetOrCreate();
            dispatchTLEntry.IsDispatchWaiting = false;
            statementResultService.Execute(dispatchTLEntry);
        }

        public override EventType EventType => eventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException();
        }

        /// <summary>
        ///     Remove event reference to last event.
        /// </summary>
        public void Clear()
        {
            lastIterableEvent = null;
        }

        public virtual UpdateDispatchView View => this;

        public virtual void NewResult(UniformPair<EventBean[]> result)
        {
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
        }

        public virtual void Cancelled()
        {
            Clear();
        }
    }
} // end of namespace