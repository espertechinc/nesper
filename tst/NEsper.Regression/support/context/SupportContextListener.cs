///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.support.context
{
    public class SupportContextListener : ContextStateListener,
        ContextPartitionStateListener
    {
        private readonly RegressionEnvironment env;
        private IList<ContextStateEvent> events = new List<ContextStateEvent>();

        public SupportContextListener(RegressionEnvironment env)
        {
            this.env = env;
        }

        public void OnContextPartitionAllocated(ContextStateEventContextPartitionAllocated @event)
        {
            events.Add(@event);
            Assert.IsNotNull(
                env.Runtime.ContextPartitionService.GetContextProperties(
                    @event.ContextDeploymentId,
                    @event.ContextName,
                    @event.Id));
        }

        public void OnContextPartitionDeallocated(ContextStateEventContextPartitionDeallocated @event)
        {
            events.Add(@event);
        }

        public void OnContextActivated(ContextStateEventContextActivated @event)
        {
            events.Add(@event);
        }

        public void OnContextDeactivated(ContextStateEventContextDeactivated @event)
        {
            events.Add(@event);
        }

        public void OnContextStatementAdded(ContextStateEventContextStatementAdded @event)
        {
            events.Add(@event);
        }

        public void OnContextStatementRemoved(ContextStateEventContextStatementRemoved @event)
        {
            events.Add(@event);
        }

        public void OnContextCreated(ContextStateEventContextCreated @event)
        {
            events.Add(@event);
            env.Runtime.ContextPartitionService.AddContextPartitionStateListener(
                @event.ContextDeploymentId,
                @event.ContextName,
                this);
        }

        public void OnContextDestroyed(ContextStateEventContextDestroyed @event)
        {
            events.Add(@event);
        }

        public IList<ContextStateEvent> GetAndReset()
        {
            var current = events;
            events = new List<ContextStateEvent>();
            return current;
        }

        public void AssertAndReset(params Consumer<ContextStateEvent>[] consumers)
        {
            var events = GetAndReset();
            Assert.AreEqual(consumers.Length, events.Count);
            var count = 0;
            foreach (var consumer in consumers) {
                consumer.Invoke(events[count++]);
            }
        }

        public IList<ContextStateEventContextPartitionAllocated> GetAllocatedEvents()
        {
            IList<ContextStateEventContextPartitionAllocated> allocateds =
                new List<ContextStateEventContextPartitionAllocated>();
            foreach (var @event in events) {
                if (@event is ContextStateEventContextPartitionAllocated) {
                    allocateds.Add((ContextStateEventContextPartitionAllocated) @event);
                }
            }

            return allocateds;
        }

        public void AssertNotInvoked()
        {
            Assert.IsTrue(events.IsEmpty());
        }
    }
} // end of namespace