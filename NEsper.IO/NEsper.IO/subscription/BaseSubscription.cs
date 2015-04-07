using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esperio.subscription
{
    /// <summary>
    /// Subscription is a concept for selecting events for processing out of all events 
    /// available from an engine instance.
    /// </summary>

    public abstract class BaseSubscription
        : Subscription
        , FilterHandleCallback
    {
        /// <summary>
        /// The output adapter to which the subscription applies.
        /// </summary>
        protected OutputAdapter adapter;

        /// <summary>Ctor, assigns default name.</summary>
        protected BaseSubscription()
        {
            SubscriptionName = "default";
        }

        abstract public string StatementId { get; }

        /// <summary>
        /// Gets or sets the subscription name.
        /// </summary>
        /// <value></value>
        /// <returns>subscription name</returns>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Gets the type name of the event type we are looking for.
        /// </summary>
        /// <value></value>
        /// <returns>event type alias</returns>
        public string EventTypeName { get; set; }

        /// <summary>
        /// Gets or sets the output adapter this subscription is associated with.
        /// </summary>
        /// <value></value>
        /// <returns>output adapter</returns>
        public OutputAdapter Adapter
        {
            get { return adapter; }
            set
            {
                this.adapter = value;

                var epService = ((AdapterSPI)adapter).EPServiceProvider;
                if (!(epService is EPServiceProviderSPI))
                {
                    throw new ArgumentException("Invalid type of EPServiceProvider");
                }

                var spi = (EPServiceProviderSPI)epService;
                var eventType = spi.EventAdapterService.GetEventTypeByName(EventTypeName);
                var fvs = new FilterSpecCompiled(eventType, null, new List<FilterSpecParam>(), null).GetValueSet(null);

                var name = "subscription:" + SubscriptionName;
                var metricsHandle = spi.MetricReportingService.GetStatementHandle(name, name);
                var lockImpl = LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                var statementHandle = new EPStatementHandle(name, lockImpl, name, false, metricsHandle, 0, false, new StatementFilterVersion());
                var registerHandle = new EPStatementHandleCallback(statementHandle, this);
                spi.FilterService.Add(fvs, registerHandle);
            }
        }

        #region FilterHandleCallback Members

        public abstract void MatchFound(EventBean @event);

        public abstract bool IsSubSelect { get; }

        #endregion
    }
}
