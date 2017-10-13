///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.stream
{
    /// <summary>
    /// Service implementation to reuse or not reuse event streams and existing filters 
    /// depending on the type of statement.
    /// <para/>
    /// For non-join statements, the class manages the reuse of event streams when filters 
    /// match, and thus when an event stream is reused such can be the views under the stream. 
    /// For joins however, this can lead to problems in multithread-safety since the statement 
    /// resource lock would then have to be multiple locks, i.e. the reused statement's resource 
    /// lock and the join statement's own lock, at a minimum. <para/>For join statements, always 
    /// creating a new event stream and therefore not reusing view resources, for use with joins.
    /// <para/>
    /// This can be very effective in that if a client applications creates a large number of very 
    /// similar statements in terms of filters and views used then these resources are all re-used 
    /// across statements. 
    /// <para/>T
    /// The re-use is multithread-safe in that
    ///  (A) statement start/stop is locked against other engine processing
    ///  (B) the first statement supplies the lock for shared filters and views, protecting multiple 
    ///      threads from entering into the same view. 
    ///  (C) joins statements do not participate in filter and view reuse
    /// </summary>
    public class StreamFactorySvcImpl : StreamFactoryService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // Using identify hash map - ignoring the equals semantics on filter specs
        // Thus two filter specs objects are always separate entries in the map
        private readonly IdentityDictionary<Object, StreamEntry> _eventStreamsIdentity;

        // Using a reference-counted map for non-join statements
        private readonly RefCountedMap<FilterSpecCompiled, StreamEntry> _eventStreamsRefCounted;

        private readonly String _engineURI;
        private readonly bool _isReuseViews;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="isReuseViews">indicator on whether stream and view resources are to be reused between statements</param>
        public StreamFactorySvcImpl(String engineURI, bool isReuseViews)
        {
            _engineURI = engineURI;
            _eventStreamsRefCounted = new RefCountedMap<FilterSpecCompiled, StreamEntry>();
            _eventStreamsIdentity = new IdentityDictionary<Object, StreamEntry>();
            _isReuseViews = isReuseViews;
        }

        public void Destroy()
        {
            _eventStreamsRefCounted.Clear();
            _eventStreamsIdentity.Clear();
        }

        /// <summary>
        /// See the method of the same name in <seealso cref="com.espertech.esper.view.stream.StreamFactoryService" />.
        /// Always attempts to reuse an existing event stream. May thus return a new event stream or an existing event
        /// stream depending on whether filter criteria match.
        /// </summary>
        /// <param name="statementId">the statement id</param>
        /// <param name="filterSpec">is the filter definition</param>
        /// <param name="filterService">filter service to activate filter if not already active</param>
        /// <param name="epStatementAgentInstanceHandle">is the statement resource lock</param>
        /// <param name="isJoin">is indicatng whether the stream will participate in a join statement, information necessary for stream reuse and multithreading concerns</param>
        /// <param name="agentInstanceContext"></param>
        /// <param name="hasOrderBy">if the consumer has order-by</param>
        /// <param name="filterWithSameTypeSubselect">if set to <c>true</c> [filter with same type subselect].</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="stateless">if set to <c>true</c> [stateless].</param>
        /// <param name="streamNum">The stream num.</param>
        /// <param name="isCanIterateUnbound">if set to <c>true</c> [is can iterate unbound].</param>
        /// <returns>
        /// newly createdStatement event stream, not reusing existing instances
        /// </returns>
        /// <exception cref="IllegalStateException">Filter spec object already found in collection</exception>
        public Pair<EventStream, IReaderWriterLock> CreateStream(
            int statementId,
            FilterSpecCompiled filterSpec,
            FilterService filterService,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            bool isJoin,
            AgentInstanceContext agentInstanceContext,
            bool hasOrderBy,
            bool filterWithSameTypeSubselect,
            Attribute[] annotations,
            bool stateless,
            int streamNum,
            bool isCanIterateUnbound)
        {
            EventStream eventStream;

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".createStream hashCode=" + filterSpec.GetHashCode() + " filter=" + filterSpec);
            }

            // Check if a stream for this filter already exists
            StreamEntry entry;
            var forceNewStream = isJoin || (!_isReuseViews) || hasOrderBy || filterWithSameTypeSubselect || stateless;
            if (forceNewStream)
            {
                entry = _eventStreamsIdentity.Get(filterSpec);
            }
            else
            {
                entry = _eventStreamsRefCounted[filterSpec];
            }

            // If pair exists, either reference count or illegal state
            if (entry != null)
            {
                if (forceNewStream)
                {
                    throw new IllegalStateException("Filter spec object already found in collection");
                }
                else
                {
                    Log.Debug(".createStream filter already found");
                    _eventStreamsRefCounted.Reference(filterSpec);

                    // audit proxy
                    eventStream = EventStreamProxy.GetAuditProxy(
                        _engineURI, epStatementAgentInstanceHandle.StatementHandle.StatementName, annotations,
                        filterSpec, entry.EventStream);

                    // We return the lock of the statement first establishing the stream to use that as the new statement's lock
                    return new Pair<EventStream, IReaderWriterLock>(
                        eventStream, entry.Callback.AgentInstanceHandle.StatementAgentInstanceLock);
                }
            }

            // New event stream
            var resultEventType = filterSpec.ResultEventType;
            var zeroDepthStream = isCanIterateUnbound
                ? (EventStream)new ZeroDepthStreamIterable(resultEventType)
                : (EventStream)new ZeroDepthStreamNoIterate(resultEventType);

            // audit proxy
            var inputStream = EventStreamProxy.GetAuditProxy(
                _engineURI, epStatementAgentInstanceHandle.StatementHandle.StatementName, annotations, filterSpec,
                zeroDepthStream);

            eventStream = inputStream;
            FilterHandleCallback filterCallback;
            if (filterSpec.OptionalPropertyEvaluator != null)
            {
                filterCallback = new ProxyFilterHandleCallback()
                {
                    ProcStatementId = () => statementId,
                    ProcMatchFound = (theEvent, allStmtMatches) =>
                    {
                        var result = filterSpec.OptionalPropertyEvaluator.GetProperty(theEvent, agentInstanceContext);
                        if (result != null)
                        {
                            eventStream.Insert(result);
                        }
                    },
                    ProcIsSubselect = () => false
                };
            }
            else
            {
                filterCallback = new ProxyFilterHandleCallback()
                {
                    ProcStatementId = () => statementId,
                    ProcMatchFound = (theEvent, allStmtMatches) =>
                    {
                        if (InstrumentationHelper.ENABLED) InstrumentationHelper.Get().QFilterActivationStream(theEvent.EventType.Name, streamNum);
                        eventStream.Insert(theEvent);
                        if (InstrumentationHelper.ENABLED) InstrumentationHelper.Get().AFilterActivationStream();
                    },
                    ProcIsSubselect = () => false
                };
            }

            var handle = new EPStatementHandleCallback(epStatementAgentInstanceHandle, filterCallback);

            // Store stream for reuse
            entry = new StreamEntry(eventStream, handle);
            if (forceNewStream)
            {
                _eventStreamsIdentity.Put(filterSpec, entry);
            }
            else
            {
                _eventStreamsRefCounted[filterSpec] = entry;
            }

            // Activate filter
            var filterValues = filterSpec.GetValueSet(null, agentInstanceContext, null);
            var filterServiceEntry = filterService.Add(filterValues, handle);
            entry.FilterServiceEntry = filterServiceEntry;

            return new Pair<EventStream, IReaderWriterLock>(inputStream, null);
        }

        /// <summary>
        /// See the method of the same name in <seealso cref="com.espertech.esper.view.stream.StreamFactoryService"/>.
        /// </summary>
        /// <param name="filterSpec">is the filter definition</param>
        /// <param name="filterService">to be used to deactivate filter when the last event stream is dropped</param>
        /// <param name="isJoin">is indicatng whether the stream will participate in a join statement, informationnecessary for stream reuse and multithreading concerns</param>
        /// <param name="hasOrderBy">if the consumer has an order-by clause</param>
        /// <param name="filterWithSameTypeSubselect"></param>
        /// <param name="stateless"></param>
        public void DropStream(
            FilterSpecCompiled filterSpec,
            FilterService filterService,
            bool isJoin,
            bool hasOrderBy,
            bool filterWithSameTypeSubselect,
            bool stateless)
        {
            StreamEntry entry;
            var forceNewStream = isJoin || (!_isReuseViews) || hasOrderBy || filterWithSameTypeSubselect || stateless;

            if (forceNewStream)
            {
                entry = _eventStreamsIdentity.Get(filterSpec);
                if (entry == null)
                {
                    throw new IllegalStateException("Filter spec object not in collection");
                }
                _eventStreamsIdentity.Remove(filterSpec);
                filterService.Remove(entry.Callback, entry.FilterServiceEntry);
            }
            else
            {
                entry = _eventStreamsRefCounted[filterSpec];
                var isLast = _eventStreamsRefCounted.Dereference(filterSpec);
                if (isLast)
                {
                    filterService.Remove(entry.Callback, entry.FilterServiceEntry);
                }
            }
        }

        public sealed class StreamEntry
        {
            public StreamEntry(EventStream eventStream, EPStatementHandleCallback callback)
            {
                EventStream = eventStream;
                Callback = callback;
            }

            public EventStream EventStream { get; private set; }

            public EPStatementHandleCallback Callback { get; private set; }

            public FilterServiceEntry FilterServiceEntry { get; set; }
        }
    }
}
