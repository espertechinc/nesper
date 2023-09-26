///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.historical.execstrategy;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.threadlocal;


namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    /// Implements a poller viewable that uses a polling strategy, a cache and
    /// some input parameters extracted from event streams to perform the polling.
    /// </summary>
    public abstract class HistoricalEventViewableBase : Viewable,
        HistoricalEventViewable
    {
        private readonly HistoricalEventViewableFactoryBase factory;
        private readonly PollExecStrategy pollExecStrategy;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        protected HistoricalDataCache dataCache;
        private View child;

        private static readonly EventBean[][] NULL_ROWS;

        static HistoricalEventViewableBase()
        {
            NULL_ROWS = new EventBean[1][];
            NULL_ROWS[0] = new EventBean[1];
        }

        public HistoricalEventViewableBase(
            HistoricalEventViewableFactoryBase factory,
            PollExecStrategy pollExecStrategy,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            this.factory = factory;
            this.pollExecStrategy = pollExecStrategy;
            this.exprEvaluatorContext = exprEvaluatorContext;
        }

        public void Stop(AgentInstanceStopServices services)
        {
            pollExecStrategy.Dispose();
            dataCache.Destroy();
        }

        public View Child {
            get => child;
            set => child = value;
        }

        private static readonly PollResultIndexingStrategy ITERATOR_INDEXING_STRATEGY =
            new ProxyPollResultIndexingStrategy() {
                ProcIndex = (
                    pollResult,
                    isActiveCache,
                    exprEvaluatorContext) => {
                    return new EventTable[] { new UnindexedEventTableList(pollResult, -1) };
                }
            };

        public EventTable[][] Poll(
            EventBean[][] lookupEventsPerStream,
            PollResultIndexingStrategy indexingStrategy,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var localDataCache = factory.DataCacheThreadLocal.GetOrCreate();
            var strategyStarted = false;

            var resultPerInputRow = new EventTable[lookupEventsPerStream.Length][];

            // Get input parameters for each row
            EventBean[] eventsPerStream;
            for (var row = 0; row < lookupEventsPerStream.Length; row++) {
                // Build lookup keys
                eventsPerStream = lookupEventsPerStream[row];
                var lookupValue = factory.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);

                EventTable[] result = null;

                // try the threadlocal iteration cache, if set
                object cacheMultiKey = null;
                if (localDataCache != null || dataCache.IsActive) {
                    cacheMultiKey = factory.LookupValueToMultiKey.Invoke(lookupValue);
                }

                if (localDataCache != null) {
                    var tables = localDataCache.GetCached(cacheMultiKey);
                    result = tables;
                }

                // try the connection cache
                if (result == null) {
                    var multi = dataCache.GetCached(cacheMultiKey);
                    if (multi != null) {
                        result = multi;
                        if (localDataCache != null) {
                            localDataCache.Put(cacheMultiKey, multi);
                        }
                    }
                }

                // use the result from cache
                if (result != null) {
                    // found in cache
                    resultPerInputRow[row] = result;
                }
                else {
                    // not found in cache, get from actual polling (db query)
                    try {
                        if (!strategyStarted) {
                            pollExecStrategy.Start();
                            strategyStarted = true;
                        }

                        // Poll using the polling execution strategy and lookup values
                        var pollResult = pollExecStrategy.Poll(lookupValue, this.exprEvaluatorContext);

                        // index the result, if required, using an indexing strategy
                        var indexTable = indexingStrategy.Index(
                            pollResult,
                            dataCache.IsActive,
                            this.exprEvaluatorContext);

                        // assign to row
                        resultPerInputRow[row] = indexTable;

                        // save in cache
                        dataCache.Put(cacheMultiKey, indexTable);

                        if (localDataCache != null) {
                            localDataCache.Put(cacheMultiKey, indexTable);
                        }
                    }
                    catch (EPException ex) {
                        if (strategyStarted) {
                            pollExecStrategy.Done();
                        }

                        throw;
                    }
                }
            }

            if (strategyStarted) {
                pollExecStrategy.Done();
            }

            return resultPerInputRow;
        }

        public EventType EventType => factory.EventType;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var tablesPerRow = Poll(NULL_ROWS, ITERATOR_INDEXING_STRATEGY, exprEvaluatorContext);
            return new IterablesArrayEnumerator(tablesPerRow);
        }

        public IThreadLocal<HistoricalDataCache> DataCacheThreadLocal => factory.DataCacheThreadLocal;

        public bool HasRequiredStreams => factory.HasRequiredStreams;

        public HistoricalDataCache OptionalDataCache => dataCache;
    }
} // end of namespace