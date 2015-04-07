///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>Outbound unit. </summary>
    public class OutboundUnitRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly UniformPair<EventBean[]> _events;
        private readonly StatementResultServiceImpl _statementResultService;

        /// <summary>Ctor. </summary>
        /// <param name="events">to dispatch</param>
        /// <param name="statementResultService">handles result indicate</param>
        public OutboundUnitRunnable(UniformPair<EventBean[]> events,
                                    StatementResultServiceImpl statementResultService)
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
}