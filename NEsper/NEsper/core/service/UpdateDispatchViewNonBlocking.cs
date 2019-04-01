///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.dispatch;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Convenience view for dispatching view updates received from a parent view to Update
    /// listeners via the dispatch service.
    /// </summary>
    public class UpdateDispatchViewNonBlocking : UpdateDispatchViewBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementResultServiceImpl">handles result delivery</param>
        /// <param name="dispatchService">for performing the dispatch</param>
        /// <param name="threadLocalManager">The thread local manager.</param>
        public UpdateDispatchViewNonBlocking(
            StatementResultService statementResultServiceImpl,
            DispatchService dispatchService,
            IThreadLocalManager threadLocalManager)
            : base(statementResultServiceImpl, dispatchService, threadLocalManager)
        {
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            NewResult(new UniformPair<EventBean[]>(newData, oldData));
        }

        public override void NewResult(UniformPair<EventBean[]> results)
        {
            StatementResultService.Indicate(results);
            if (!IsDispatchWaiting.Value)
            {
                DispatchService.AddExternal(this);
                IsDispatchWaiting.Value = true;
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
