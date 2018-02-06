///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.filter;

namespace com.espertech.esper.adapter
{
    /// <summary>
    /// Subscription is a concept for selecting events for processing out of all 
    /// events available from an engine instance.
    /// </summary>
    public abstract class BaseSubscription : Subscription, FilterHandleCallback
    {
        public IReaderWriterLockManager RWLockManager { get; set; }

        /// <summary>The event type of the events we are subscribing for. </summary>
        public String EventTypeName { get; set; }
    
        /// <summary>The name of the subscription. </summary>
        public String SubscriptionName { get; set; }

        public OutputAdapter Adapter { get; private set; }

        public abstract int StatementId { get; }

        public abstract bool IsSubSelect { get; }

        public abstract void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches);

        /// <summary>Ctor, assigns default name. </summary>
        protected BaseSubscription(IReaderWriterLockManager rwLockManager)
        {
            SubscriptionName = "default";
        }

        public void RegisterAdapter(OutputAdapter adapter)
        {
            Adapter = adapter;
            RegisterAdapter(((AdapterSPI) adapter).EPServiceProvider);
        }
    
        /// <summary>Register an adapter. </summary>
        /// <param name="epService">engine</param>
        public void RegisterAdapter(EPServiceProvider epService)
        {
            var spi = (EPServiceProviderSPI) epService;
            var eventType = spi.EventAdapterService.GetEventTypeByName(EventTypeName);
            var fvs = new FilterSpecCompiled(eventType, null, new IList<FilterSpecParam>[0], null).GetValueSet(null, null, null);
    
            var name = "subscription:" + SubscriptionName;
            var metricsHandle = spi.MetricReportingService.GetStatementHandle(-1, name);
            var statementHandle = new EPStatementHandle(-1, name, name, StatementType.ESPERIO, name, false, metricsHandle, 0, false, false, spi.ServicesContext.MultiMatchHandlerFactory.GetDefaultHandler());
            var agentHandle = new EPStatementAgentInstanceHandle(statementHandle, RWLockManager.CreateDefaultLock(), -1, new StatementAgentInstanceFilterVersion(), null);
            var registerHandle = new EPStatementHandleCallback(agentHandle, this);
            spi.FilterService.Add(fvs, registerHandle);
        }
    }
}
