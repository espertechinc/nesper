///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dispatch;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Convenience view for dispatching view updates received from a parent view to Update listeners via the dispatch service.
    /// </summary>
    public abstract class UpdateDispatchViewBase
        : ViewSupport
        , Dispatchable
        , UpdateDispatchView
    {
        /// <summary>Handles result delivery </summary>
        public StatementResultService StatementResultService { get; protected internal set; }

        /// <summary>Dispatches events to listeners. </summary>
        protected readonly DispatchService DispatchService;

        /// <summary>For iteration with patterns. </summary>
        protected EventBean LastIterableEvent;

        private readonly IThreadLocal<Mutable<bool>> _isDispatchWaiting;
            

        /// <summary>Flag to indicate we have registered a dispatch.</summary>
        protected Mutable<bool> IsDispatchWaiting
        {
            get { return _isDispatchWaiting.GetOrCreate(); }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementResultService">handles result delivery</param>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="threadLocalManager">The thread local manager.</param>
        protected UpdateDispatchViewBase(
            StatementResultService statementResultService, 
            DispatchService dispatchService,
            IThreadLocalManager threadLocalManager)
        {
            _isDispatchWaiting = threadLocalManager.Create(() => new Mutable<bool>());
            DispatchService = dispatchService;
            StatementResultService = statementResultService;
        }

        public abstract void NewResult(UniformPair<EventBean[]> result);

        public override EventType EventType
        {
            get { return null; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException();
        }

        public void Execute()
        {
            IsDispatchWaiting.Value = false;
            StatementResultService.Execute();
        }

        /// <summary>Remove event reference to last event. </summary>
        public void Clear()
        {
            LastIterableEvent = null;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}