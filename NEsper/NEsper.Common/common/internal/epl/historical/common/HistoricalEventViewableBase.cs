///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    ///     Implements a poller viewable that uses a polling strategy, a cache and
    ///     some input parameters extracted from event streams to perform the polling.
    /// </summary>
    public abstract class HistoricalEventViewableBase : Viewable,
        HistoricalEventViewable
    {
        protected internal static readonly EventBean[][] NULL_ROWS;

        private static readonly PollResultIndexingStrategy ITERATOR_INDEXING_STRATEGY =
            new ProxyPollResultIndexingStrategy {
                ProcIndex = (
                    pollResult,
                    _,
                    __) => new EventTable[] {
                    new UnindexedEventTableList(pollResult, -1)
                },
#if false
            ProcToQueryPlan = () =>  {
	            return this.GetType().SimpleName + " unindexed";
	        },
#endif
            };

        protected internal readonly AgentInstanceContext agentInstanceContext;
        protected internal readonly HistoricalEventViewableFactoryBase factory;
        protected internal readonly PollExecStrategy pollExecStrategy;
        protected internal View child;
        protected internal HistoricalDataCache dataCache;

        static HistoricalEventViewableBase()
        {
            NULL_ROWS = new EventBean[1][];
            NULL_ROWS[0] = new EventBean[1];
        }

        public HistoricalEventViewableBase(
            HistoricalEventViewableFactoryBase factory,
            PollExecStrategy pollExecStrategy,
            AgentInstanceContext agentInstanceContext)
        {
            this.factory = factory;
            this.pollExecStrategy = pollExecStrategy;
            this.agentInstanceContext = agentInstanceContext;
        }

        public void Stop(AgentInstanceStopServices services)
        {
            pollExecStrategy.Destroy();
            dataCache.Destroy();
        }

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
                var lookupValue = factory.evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);

                EventTable[] result = null;

                // try the threadlocal iteration cache, if set
                if (localDataCache != null) {
                    result = localDataCache.GetCached(lookupValue);
                }

                // try the connection cache
                if (result == null) {
                    var multi = dataCache.GetCached(lookupValue);
                    if (multi != null) {
                        result = multi;
                        localDataCache?.Put(lookupValue, multi);
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
                        IList<EventBean> pollResult = pollExecStrategy.Poll(lookupValue, agentInstanceContext);

                        // index the result, if required, using an indexing strategy
                        var indexTable = indexingStrategy.Index(pollResult, dataCache.IsActive, agentInstanceContext);

                        // assign to row
                        resultPerInputRow[row] = indexTable;

                        // save in cache
                        dataCache.Put(lookupValue, indexTable);

                        if (localDataCache != null) {
                            localDataCache.Put(lookupValue, indexTable);
                        }
                    }
                    catch (EPException) {
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

        public IThreadLocal<HistoricalDataCache> DataCacheThreadLocal => factory.DataCacheThreadLocal;

        public bool HasRequiredStreams => factory.HasRequiredStreams;

        public View Child {
            get => child;
            set => child = value;
        }

        public EventType EventType => factory.EventType;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var tablesPerRow = Poll(NULL_ROWS, ITERATOR_INDEXING_STRATEGY, agentInstanceContext);
            return new IterablesArrayEnumerator(tablesPerRow);
        }
    }
} // end of namespace