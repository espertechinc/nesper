///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Mapping of event type to a tree-like structure
    ///     containing filter parameter constants in indexes <seealso cref="FilterParamIndexBase" /> and filter callbacks in
    ///     <seealso cref="FilterHandleSetNode" />.
    ///     <para />
    ///     This class evaluates events for the purpose of filtering by (1) looking up the event's <seealso cref="EventType" />
    ///     and (2) asking the subtree for this event type to evaluate the event.
    ///     <para />
    ///     The class performs all the locking required for multithreaded access.
    /// </summary>
    public class EventTypeIndex : EventEvaluator
    {
        private readonly IDictionary<EventType, FilterHandleSetNode> eventTypes;
        private readonly IReaderWriterLock eventTypesRWLock;

        public EventTypeIndex(FilterServiceGranularLockFactory lockFactory)
        {
            eventTypes = new Dictionary<EventType, FilterHandleSetNode>();
            eventTypesRWLock = lockFactory.ObtainNew();
        }

        /// <summary>
        ///     Returns the current size of the known event types.
        /// </summary>
        /// <value>collection size</value>
        protected internal int Count => eventTypes.Count;

        protected internal int FilterCountApprox {
            get {
                var count = 0;
                using (eventTypesRWLock.ReadLock.Acquire()) {
                    foreach (var entry in eventTypes) {
                        count += entry.Value.FilterCallbackCount;
                        foreach (var index in entry.Value.Indizes) {
                            count += index.CountExpensive;
                        }
                    }
                }

                return count;
            }
        }

        public void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches)
        {
            var eventType = theEvent.EventType;

            // Attempt to match exact type
            MatchType(eventType, theEvent, matches);

            // No supertype means we are done
            if (eventType.SuperTypes == null) {
                return;
            }

            foreach (var superType in eventType.DeepSuperTypes) {
                MatchType(superType, theEvent, matches);
            }
        }

        /// <summary>
        ///     Destroy the service.
        /// </summary>
        public void Destroy()
        {
            eventTypes.Clear();
        }

        public IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> GetTraverseStatement(
            ICollection<int> statementIds)
        {
            var evaluatorStack = new ArrayDeque<FilterItem>();
            IDictionary<int, IList<FilterItem[]>> filters = new Dictionary<int, IList<FilterItem[]>>();
            IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> filtersPerType =
                new Dictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>>();

            EventTypeIndexTraverse traverse = (
                stack,
                filterHandle) => {
                var filterArray = stack.ToArray();
                var existing = filters.Get(filterHandle.StatementId);
                if (existing == null) {
                    existing = new List<FilterItem[]>();
                    filters.Put(filterHandle.StatementId, existing);
                }

                existing.Add(filterArray);
            };

            foreach (var entry in eventTypes) {
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                if (!filters.IsEmpty()) {
                    filtersPerType.Put(entry.Key.Metadata.EventTypeIdPair, new Dictionary<int, IList<FilterItem[]>>(filters));
                    filters.Clear();
                }
            }

            return filtersPerType;
        }

        public void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            throw new UnsupportedOperationException("Use GetTraverse instead");
        }

        /// <summary>
        ///     Add a new event type to the index and use the specified node for the root node of its subtree.
        ///     If the event type already existed, the method will throw an IllegalStateException.
        /// </summary>
        /// <param name="eventType">is the event type to be added to the index</param>
        /// <param name="rootNode">is the root node of the subtree for filter constant indizes and callbacks</param>
        public void Add(
            EventType eventType,
            FilterHandleSetNode rootNode)
        {
            using (eventTypesRWLock.WriteLock.Acquire()) {
                if (eventTypes.ContainsKey(eventType)) {
                    throw new IllegalStateException("Event type already in index, add not performed, type=" + eventType);
                }

                eventTypes.Put(eventType, rootNode);
            }
        }

        public void RemoveType(EventType type)
        {
            using (eventTypesRWLock.WriteLock.Acquire()) {
                eventTypes.Remove(type);
            }
        }

        /// <summary>
        ///     Returns the root node for the given event type, or null if this event type has not been seen before.
        /// </summary>
        /// <param name="eventType">is an event type</param>
        /// <returns>the subtree's root node</returns>
        public FilterHandleSetNode Get(EventType eventType)
        {
            using (eventTypesRWLock.ReadLock.Acquire()) {
                return eventTypes.Get(eventType);
            }
        }

        private void MatchType(
            EventType eventType,
            EventBean eventBean,
            ICollection<FilterHandle> matches)
        {
            FilterHandleSetNode rootNode = null;

            using (eventTypesRWLock.ReadLock.Acquire()) {
                rootNode = eventTypes.Get(eventType);
            }

            // If the top class node is null, no filters have yet been registered for this event type.
            // In this case, log a message and done.
            if (rootNode == null) {
                return;
            }

            rootNode.MatchEvent(eventBean, matches);
        }
    }
} // end of namespace