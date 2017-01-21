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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.filter;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.support.filter
{
    public class IndexTreeBuilderRunnable
    {
        protected internal static Random Random = new Random(Environment.TickCount);
    
        private readonly FilterHandleSetNode _topNode;
        private readonly List<FilterSpecCompiled> _testFilterSpecs;
        private readonly List<EventBean> _matchedEvents;
        private readonly List<EventBean> _unmatchedEvents;
        private readonly EventType _eventType;
        private FilterServiceGranularLockFactory _lockFactory = 
            new FilterServiceGranularLockFactoryReentrant();

        public IndexTreeBuilderRunnable(EventType eventType, FilterHandleSetNode topNode, List<FilterSpecCompiled> testFilterSpecs, List<EventBean> matchedEvents, List<EventBean> unmatchedEvents)
        {
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
            do
            {
                index = Random.Next(_testFilterSpecs.Count);
                filterSpec = _testFilterSpecs[index];
                unmatchedEvent = _unmatchedEvents[index];
                matchedEvent = _matchedEvents[index];
            }
            while(!ObjectReservationSingleton.Instance.Reserve(filterSpec));
    
            // Add expression
            var filterValues = filterSpec.GetValueSet(null, null, null);
            FilterHandle filterCallback = new SupportFilterHandle();
            var pathAddedTo = IndexTreeBuilder.Add(filterValues, filterCallback, _topNode, _lockFactory);
    
            // Fire a no-match
            IList<FilterHandle> matches = new List<FilterHandle>();
            _topNode.MatchEvent(unmatchedEvent, matches);
    
            if (matches.Count != 0)
            {
                Log.Fatal(".run (" + currentThreadId + ") Got a match but expected no-match, matchCount=" + matches.Count + "  bean=" + unmatchedEvent +
                          "  match=" + matches[0].GetHashCode());
                Assert.IsFalse(true);
            }
    
            // Fire a match
            _topNode.MatchEvent(matchedEvent, matches);
    
            if (matches.Count != 1)
            {
                Log.Fatal(".run (" + currentThreadId + ") Got zero or two or more match but expected a match, count=" + matches.Count +
                        "  bean=" + matchedEvent);
                Assert.IsFalse(true);
            }
    
            // Remove the same expression again
            IndexTreeBuilder.Remove(_eventType, filterCallback, pathAddedTo[0].ToArray(), _topNode);
            Log.Debug(".run (" + Thread.CurrentThread.ManagedThreadId + ")" + " Completed");
    
            ObjectReservationSingleton.Instance.Unreserve(filterSpec);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
