///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.support;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class SupportIndexTreeBuilderRunnable : IRunnable
    {
        protected static readonly Random random = new Random();

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventType eventType;
        private readonly FilterServiceGranularLockFactory lockFactory;
        private readonly IList<EventBean> matchedEvents;
        private readonly IList<FilterSpecActivatable> testFilterSpecs;

        private readonly FilterHandleSetNode topNode;
        private readonly IList<EventBean> unmatchedEvents;

        public SupportIndexTreeBuilderRunnable(
            EventType eventType,
            FilterHandleSetNode topNode,
            IList<FilterSpecActivatable> testFilterSpecs,
            IList<EventBean> matchedEvents,
            IList<EventBean> unmatchedEvents,
            IReaderWriterLockManager rwLockManager)
        {
            this.eventType = eventType;
            this.topNode = topNode;
            this.testFilterSpecs = testFilterSpecs;
            this.matchedEvents = matchedEvents;
            this.unmatchedEvents = unmatchedEvents;
            this.lockFactory = new FilterServiceGranularLockFactoryReentrant(rwLockManager);
        }

        public void Run()
        {
            long currentThreadId = Thread.CurrentThread.ManagedThreadId;

            // Choose one of filter specifications, randomly, then reserve to make sure no one else has the same
            FilterSpecActivatable filterSpec = null;
            EventBean unmatchedEvent = null;
            EventBean matchedEvent = null;

            var index = 0;
            do
            {
                index = random.Next(testFilterSpecs.Count);
                filterSpec = testFilterSpecs[index];
                unmatchedEvent = unmatchedEvents[index];
                matchedEvent = matchedEvents[index];
            } while (!ObjectReservationSingleton.GetInstance().Reserve(filterSpec));

            // Add expression
            var filterValues = filterSpec.GetValueSet(null, null, null, null);
            FilterHandle filterCallback = new SupportFilterHandle();
            IndexTreeBuilderAdd.Add(filterValues, filterCallback, topNode, lockFactory);

            // Fire a no-match
            IList<FilterHandle> matches = new List<FilterHandle>();
            topNode.MatchEvent(unmatchedEvent, matches);

            if (matches.Count != 0)
            {
                log.Error(
                    ".Run (" +
                    currentThreadId +
                    ") Got a match but expected no-match, matchCount=" +
                    matches.Count +
                    "  bean=" +
                    unmatchedEvent +
                    "  match=" +
                    matches[0].GetHashCode());
                Assert.IsFalse(true);
            }

            // Fire a match
            topNode.MatchEvent(matchedEvent, matches);

            if (matches.Count != 1)
            {
                log.Error(
                    ".Run (" +
                    currentThreadId +
                    ") Got zero or two or more match but expected a match, count=" +
                    matches.Count +
                    "  bean=" +
                    matchedEvent);
                Assert.IsFalse(true);
            }

            // Remove the same expression again
            IndexTreeBuilderRemove.Remove(eventType, filterCallback, filterValues[0], topNode);
            log.Debug(".Run (" + Thread.CurrentThread.ManagedThreadId + ")" + " Completed");

            ObjectReservationSingleton.GetInstance().Unreserve(filterSpec);
        }
    }
} // end of namespace
