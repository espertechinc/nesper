///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    /// Manages index-building and sharing for historical streams by collecting required indexes during the
    /// query planning phase, and by providing the right lookup strategy and indexing strategy during
    /// query execution node creation.
    /// </summary>
    public class HistoricalStreamIndexListForge
    {
        private readonly int historicalStreamNum;
        private readonly EventType[] typesPerStream;
        private readonly QueryGraphForge queryGraph;
        private readonly SortedSet<int> pollingStreams;

        private IDictionary<HistoricalStreamIndexDesc, IList<int>> indexesUsedByStreams;
        private PollResultIndexingStrategyForge masterIndexingStrategy;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="historicalStreamNum">number of the historical stream</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="queryGraph">relationship between key and index properties</param>
        public HistoricalStreamIndexListForge(
            int historicalStreamNum,
            EventType[] typesPerStream,
            QueryGraphForge queryGraph)
        {
            this.historicalStreamNum = historicalStreamNum;
            this.typesPerStream = typesPerStream;
            this.queryGraph = queryGraph;
            this.pollingStreams = new SortedSet<int>();
        }

        /// <summary>
        /// Used during query plan phase to indicate that an index must be provided for use in lookup of historical events by using a
        /// stream's events.
        /// </summary>
        /// <param name="streamViewStreamNum">the stream providing lookup events</param>
        public void AddIndex(int streamViewStreamNum)
        {
            pollingStreams.Add(streamViewStreamNum);
        }

        /// <summary>
        /// Get the strategies to use for polling from a given stream.
        /// </summary>
        /// <param name="streamViewStreamNum">the stream providing the polling events</param>
        /// <returns>looking and indexing strategy</returns>
        public Pair<HistoricalIndexLookupStrategyForge, PollResultIndexingStrategyForge> GetStrategy(
            int streamViewStreamNum)
        {
            // If there is only a single polling stream, then build a single index
            if (pollingStreams.Count == 1) {
                return JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                    queryGraph,
                    typesPerStream[historicalStreamNum],
                    typesPerStream[streamViewStreamNum],
                    historicalStreamNum,
                    streamViewStreamNum);
            }

            // If there are multiple polling streams, determine if a single index is appropriate.
            // An index can be reused if:
            //  (a) indexed property names are the same
            //  (b) indexed property types are the same
            //  (c) key property types are the same (because of coercion)
            // A index lookup strategy is always specific to the providing stream.
            if (indexesUsedByStreams == null) {
                indexesUsedByStreams = new LinkedHashMap<HistoricalStreamIndexDesc, IList<int>>();
                foreach (int pollingStream in pollingStreams) {
                    QueryGraphValueForge queryGraphValue = queryGraph.GetGraphValue(pollingStream, historicalStreamNum);
                    QueryGraphValuePairHashKeyIndexForge hashKeyProps = queryGraphValue.HashKeyProps;
                    string[] indexProperties = hashKeyProps.Indexed;

                    Type[] keyTypes = GetPropertyTypes(hashKeyProps.Keys);
                    Type[] indexTypes = GetPropertyTypes(typesPerStream[historicalStreamNum], indexProperties);

                    HistoricalStreamIndexDesc desc = new HistoricalStreamIndexDesc(
                        indexProperties,
                        indexTypes,
                        keyTypes);
                    IList<int> usedByStreams = indexesUsedByStreams.Get(desc);
                    if (usedByStreams == null) {
                        usedByStreams = new List<int>();
                        indexesUsedByStreams.Put(desc, usedByStreams);
                    }

                    usedByStreams.Add(pollingStream);
                }

                // There are multiple indexes required:
                // Build a master indexing strategy that forms multiple indexes and numbers each.
                if (indexesUsedByStreams.Count > 1) {
                    int numIndexes = indexesUsedByStreams.Count;
                    PollResultIndexingStrategyForge[] indexingStrategies =
                        new PollResultIndexingStrategyForge[numIndexes];

                    // create an indexing strategy for each index
                    int count = 0;
                    foreach (KeyValuePair<HistoricalStreamIndexDesc, IList<int>> desc in indexesUsedByStreams) {
                        int sampleStreamViewStreamNum = desc.Value[0];
                        indexingStrategies[count] = JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                                queryGraph,
                                typesPerStream[historicalStreamNum],
                                typesPerStream[sampleStreamViewStreamNum],
                                historicalStreamNum,
                                sampleStreamViewStreamNum)
                            .Second;
                        count++;
                    }

                    // create a master indexing strategy that utilizes each indexing strategy to create a set of indexes
                    masterIndexingStrategy = new PollResultIndexingStrategyMultiForge(
                        streamViewStreamNum,
                        indexingStrategies);
                }
            }

            // there is one type of index
            if (indexesUsedByStreams.Count == 1) {
                return JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                    queryGraph,
                    typesPerStream[historicalStreamNum],
                    typesPerStream[streamViewStreamNum],
                    historicalStreamNum,
                    streamViewStreamNum);
            }

            // determine which index number the polling stream must use
            int indexUsed = 0;
            bool found = false;
            foreach (IList<int> desc in indexesUsedByStreams.Values) {
                if (desc.Contains(streamViewStreamNum)) {
                    found = true;
                    break;
                }

                indexUsed++;
            }

            if (!found) {
                throw new IllegalStateException("Index not found for use by stream " + streamViewStreamNum);
            }

            // Use one of the indexes built by the master index and a lookup strategy
            HistoricalIndexLookupStrategyForge innerLookupStrategy = JoinSetComposerPrototypeForgeFactory
                .DetermineIndexing(
                    queryGraph,
                    typesPerStream[historicalStreamNum],
                    typesPerStream[streamViewStreamNum],
                    historicalStreamNum,
                    streamViewStreamNum)
                .First;
            HistoricalIndexLookupStrategyForge lookupStrategy =
                new HistoricalIndexLookupStrategyMultiForge(indexUsed, innerLookupStrategy);
            return new Pair<HistoricalIndexLookupStrategyForge, PollResultIndexingStrategyForge>(
                lookupStrategy,
                masterIndexingStrategy);
        }

        private Type[] GetPropertyTypes(
            EventType eventType,
            string[] properties)
        {
            Type[] types = new Type[properties.Length];
            for (int i = 0; i < properties.Length; i++) {
                types[i] = Boxing.GetBoxedType(eventType.GetPropertyType(properties[i]));
            }

            return types;
        }

        private Type[] GetPropertyTypes(IList<QueryGraphValueEntryHashKeyedForge> hashKeys)
        {
            Type[] types = new Type[hashKeys.Count];
            for (int i = 0; i < hashKeys.Count; i++) {
                types[i] = Boxing.GetBoxedType(hashKeys[i].KeyExpr.Forge.EvaluationType);
            }

            return types;
        }
    }
} // end of namespace