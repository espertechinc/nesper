///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Outbound unit.
    /// </summary>
    public class OutboundUnitRunnable : IRunnable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly UniformPair<EventBean[]> events;
        private readonly StatementResultServiceImpl statementResultService;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="events">to dispatch</param>
        /// <param name="statementResultService">handles result indicate</param>
        public OutboundUnitRunnable(
            UniformPair<EventBean[]> events,
            StatementResultServiceImpl statementResultService)
        {
            this.events = events;
            this.statementResultService = statementResultService;
        }

        public void Run()
        {
            try {
                statementResultService.ProcessDispatch(events);
            }
            catch (Exception e) {
                log.Error("Unexpected error processing dispatch: " + e.Message, e);
            }
        }
    }
} // end of namespace