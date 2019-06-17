///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>Outbound unit.</summary>
    public class OutboundUnitRunnable : IRunnable
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly UniformPair<EventBean[]> _events;
        private readonly StatementResultServiceImpl _statementResultService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="events">to dispatch</param>
        /// <param name="statementResultService">handles result indicate</param>
        public OutboundUnitRunnable(UniformPair<EventBean[]> events, StatementResultServiceImpl statementResultService)
        {
            _events = events;
            _statementResultService = statementResultService;
        }

        public void Run()
        {
            try
            {
                _statementResultService.ProcessDispatch(_events);
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing dispatch: " + e.Message, e);
            }
        }
    }
} // end of namespace