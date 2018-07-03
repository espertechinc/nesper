///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logger;
using com.espertech.esper.compat.logging;
using com.espertech.esper.filter;
using com.espertech.esper.supportunit.util;

using NUnit.Framework;

namespace com.espertech.esper.supportunit.filter
{
    public class IndexTreeBuilderRunnable
    {
        protected internal static Random Random = new Random(Environment.TickCount);

        private readonly EventType _eventType;
        private readonly FilterHandleSetNode _topNode;
        private readonly IList<FilterSpecCompiled> _testFilterSpecs;
        private readonly IList<EventBean> _matchedEvents;
        private readonly IList<EventBean> _unmatchedEvents;
        private readonly FilterServiceGranularLockFactory _lockFactory;

        public IndexTreeBuilderRunnable(EventType eventType, FilterHandleSetNode topNode, IList<FilterSpecCompiled> testFilterSpecs, IList<EventBean> matchedEvents, IList<EventBean> unmatchedEvents)
        {
            _lockFactory = new FilterServiceGranularLockFactoryReentrant(
                SupportContainer.Instance.RWLockManager());
            _eventType = eventType;
            _topNode = topNode;
            _testFilterSpecs = testFilterSpecs;
            _matchedEvents = matchedEvents;
            _unmatchedEvents = unmatchedEvents;
        }

        public void Run()
        {
            long currentThreadId = Thread.CurrentThread.ManagedThreadId;
    
            // Choose one of filter specifications, randomly, then reserve to make sure no one else has the same
            FilterSpecCompiled filterSpec = null;
            EventBean unmatchedEvent = null;
            EventBean matchedEvent = null;
    
            var index = 0;
            do {
                //index = (int) (atomic.IncrementAndGet() % _testFilterSpecs.Count);
                index = Random.Next(_testFilterSpecs.Count);
                filterSpec = _testFilterSpecs[index];
                unmatchedEvent = _unmatchedEvents[index];
                matchedEvent = _matchedEvents[index];
            }
            while(!ObjectReservationSingleton.Instance.Reserve(filterSpec));

#if DEBUG && DIAGNOSTIC
            Log.Info("Reserved: {0} = {1}", index, filterSpec);
#endif

            // Add expression
            var filterValues = filterSpec.GetValueSet(null, null, null);
            var filterCallback = new SupportFilterHandle();

#if DEBUG && DIAGNOSTIC
            Log.Info("TestMultithreaded: {0}", filterValues);
#endif

            var pathAddedTo = IndexTreeBuilder.Add(filterValues, filterCallback, _topNode, _lockFactory);
    
            // Fire a no-match
            IList<FilterHandle> matches = new List<FilterHandle>();
            _topNode.MatchEvent(unmatchedEvent, matches);
    
            if (matches.Count != 0)
            {
                Log.Fatal(".Run (" + currentThreadId + ") Got a match but expected no-match, matchCount=" + matches.Count + "  bean=" + unmatchedEvent +
                          "  match=" + matches[0].GetHashCode());
                Assert.IsFalse(true);
            }
    
            // Fire a match
            _topNode.MatchEvent(matchedEvent, matches);
    
            if (matches.Count != 1)
            {
                Log.Fatal(".Run (" + currentThreadId + ") Got zero or two or more match but expected a match, count=" + matches.Count +
                        "  bean=" + matchedEvent);
                foreach (var entry in LoggerNLog.MemoryTarget.Logs) {
                    System.Diagnostics.Debug.WriteLine(entry);
                }
                Assert.IsFalse(true);
            }
    
            // Remove the same expression again
            IndexTreeBuilder.Remove(_eventType, filterCallback, pathAddedTo[0].ToArray(), _topNode);
    
            ObjectReservationSingleton.Instance.Unreserve(filterSpec);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
