///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Mapping of event type to a tree-like structure containing filter parameter constants
    /// in indexes <seealso cref="FilterParamIndexBase" /> and filter callbacks in
    /// <seealso cref="FilterHandleSetNode" />.
    /// <para/>
    /// This class evaluates events for the purpose of filtering by (1) looking up the event's
    /// <seealso cref="EventType" /> and (2) asking the subtree for this event type to evaluate
    /// the event.
    /// <para/>
    /// The class performs all the locking required for multithreaded access.
    /// </summary>
    public class EventTypeIndex : EventEvaluator
    {
        private readonly IDictionary<EventType, FilterHandleSetNode> _eventTypes;

        private readonly IReaderWriterLock _eventTypesLock;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventTypeIndex(FilterServiceGranularLockFactory lockFactory)
        {
            _eventTypes = new Dictionary<EventType, FilterHandleSetNode>();
            _eventTypesLock = lockFactory.ObtainNew();
        }

        /// <summary>
        /// Dispose the service.
        /// </summary>
        public void Dispose()
        {
            _eventTypes.Clear();
        }
    
        /// <summary>
        /// Add a new event type to the index and use the specified node for the root node of 
        /// its subtree. If the event type already existed, the method will throw an 
        /// IllegalStateException. 
        /// </summary>
        /// <param name="eventType">is the event type to be added to the index</param>
        /// <param name="rootNode">is the root node of the subtree for filter constant indizes and callbacks</param>
        public void Add(EventType eventType, FilterHandleSetNode rootNode)
        {
            using(_eventTypesLock.AcquireWriteLock())
            {
                if (_eventTypes.ContainsKey(eventType))
                {
                    throw new IllegalStateException("Event type already in index, add not performed, type=" + eventType);
                }
                _eventTypes.Put(eventType, rootNode);
            }
        }
    
    
        public void RemoveType(EventType type) 
        {
            using (_eventTypesLock.AcquireWriteLock())
            {
                _eventTypes.Remove(type);
            }
        }
    
        /// <summary>Returns the root node for the given event type, or null if this event type has not been seen before. </summary>
        /// <param name="eventType">is an event type</param>
        /// <returns>the subtree's root node</returns>
        public FilterHandleSetNode Get(EventType eventType)
        {
            using (_eventTypesLock.AcquireReadLock())
            {
                FilterHandleSetNode result = _eventTypes.Get(eventType);
                return result;
            }
        }
    
        public void MatchEvent(EventBean theTheEvent, ICollection<FilterHandle> matches)
        {
            EventType eventType = theTheEvent.EventType;
    
            // Attempt to match exact type
            MatchType(eventType, theTheEvent, matches);
    
            // No supertype means we are done
            if (eventType.SuperTypes == null)
            {
                return;
            }

            var deepSuperTypes = eventType.DeepSuperTypes;
            var deepSuperTypesLength = deepSuperTypes.Length;
            for (int ii = 0; ii < deepSuperTypesLength; ii++)
            {
                MatchType(deepSuperTypes[ii], theTheEvent, matches);
            }
        }

        /// <summary>Returns the current size of the known event types. </summary>
        /// <value>collection size</value>
        internal int Count
        {
            get { return _eventTypes.Count; }
        }

        internal int FilterCountApprox
        {
            get
            {
                using (_eventTypesLock.AcquireReadLock())
                {
                    int count = 0;
                    foreach (var entry in _eventTypes)
                    {
                        count += entry.Value.FilterCallbackCount;
                        count += entry.Value.Indizes.Sum(index => index.Count);
                    }
                    return count;
                }
            }
        }

        private void MatchType(EventType eventType, EventBean eventBean, ICollection<FilterHandle> matches)
        {
            FilterHandleSetNode rootNode;

            using (_eventTypesLock.AcquireWriteLock())
            {
                rootNode = _eventTypes.Get(eventType);
            }

            // If the top class node is null, no filters have yet been registered for this event type.
            // In this case, log a message and done.
            if (rootNode != null)
            {
                rootNode.MatchEvent(eventBean, matches);
            }
        }
    }
}
