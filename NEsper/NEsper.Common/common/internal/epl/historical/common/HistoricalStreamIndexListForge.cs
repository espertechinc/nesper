///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
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
        private readonly int _historicalStreamNum;
        private readonly EventType[] _typesPerStream;
        private readonly QueryGraphForge _queryGraph;
        private readonly SortedSet<int> _pollingStreams;

        private IDictionary<HistoricalStreamIndexDesc, IList<int>> _indexesUsedByStreams;
        private PollResultIndexingStrategyForge _masterIndexingStrategy;

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
            this._historicalStreamNum = historicalStreamNum;
            this._typesPerStream = typesPerStream;
            this._queryGraph = queryGraph;
            this._pollingStreams = new SortedSet<int>();
        }

        /// <summary>
        /// Used during query plan phase to indicate that an index must be provided for use in lookup of historical events by using a
        /// stream's events.
        /// </summary>
        /// <param name="streamViewStreamNum">the stream providing lookup events</param>
        public void AddIndex(int streamViewStreamNum)
        {
            _pollingStreams.Add(streamViewStreamNum);
        }

        /// <summary>
        /// Get the strategies to use for polling from a given stream.
        /// </summary>
        /// <param name="streamViewStreamNum">the stream providing the polling events</param>
        /// <returns>looking and indexing strategy</returns>
        public JoinSetComposerPrototypeHistoricalDesc GetStrategy(
            int streamViewStreamNum,
            StatementRawInfo raw,
            SerdeCompileTimeResolver serdeResolver)
        {
            // If there is only a single polling stream, then build a single index
            if (_pollingStreams.Count == 1) {
                return JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                    _queryGraph,
                    _typesPerStream[_historicalStreamNum],
                    _typesPerStream[streamViewStreamNum],
                    _historicalStreamNum,
                    streamViewStreamNum,
                    raw,
                    serdeResolver);
            }

            // If there are multiple polling streams, determine if a single index is appropriate.
            // An index can be reused if:
            //  (a) indexed property names are the same
            //  (b) indexed property types are the same
            //  (c) key property types are the same (because of coercion)
            // A index lookup strategy is always specific to the providing stream.
            
            var additionalForgeables = new List<StmtClassForgeableFactory>();
            
            if (_indexesUsedByStreams == null) {
                _indexesUsedByStreams = new LinkedHashMap<HistoricalStreamIndexDesc, IList<int>>();
                foreach (var pollingStream in _pollingStreams) {
                    var queryGraphValue = _queryGraph.GetGraphValue(pollingStream, _historicalStreamNum);
                    var hashKeyProps = queryGraphValue.HashKeyProps;
                    var indexProperties = hashKeyProps.Indexed;

                    var keyTypes = GetPropertyTypes(hashKeyProps.Keys);
                    var indexTypes = GetPropertyTypes(_typesPerStream[_historicalStreamNum], indexProperties);

                    var desc = new HistoricalStreamIndexDesc(
                        indexProperties,
                        indexTypes,
                        keyTypes);
                    var usedByStreams = _indexesUsedByStreams.Get(desc);
                    if (usedByStreams == null) {
                        usedByStreams = new List<int>();
                        _indexesUsedByStreams.Put(desc, usedByStreams);
                    }

                    usedByStreams.Add(pollingStream);
                }

                // There are multiple indexes required:
                // Build a master indexing strategy that forms multiple indexes and numbers each.
                if (_indexesUsedByStreams.Count > 1) {
                    var numIndexes = _indexesUsedByStreams.Count;
                    var indexingStrategies =
                        new PollResultIndexingStrategyForge[numIndexes];

                    // create an indexing strategy for each index
                    var count = 0;
                    foreach (var desc in _indexesUsedByStreams) {
                        var sampleStreamViewStreamNum = desc.Value[0];
                        var indexingX = JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                            _queryGraph,
                            _typesPerStream[_historicalStreamNum],
                            _typesPerStream[sampleStreamViewStreamNum],
                            _historicalStreamNum,
                            sampleStreamViewStreamNum,
                            raw,
                            serdeResolver);
                        indexingStrategies[count] = indexingX.IndexingForge;
                        additionalForgeables.AddAll(indexingX.AdditionalForgeables);
                        count++;
                    }

                    // create a master indexing strategy that utilizes each indexing strategy to create a set of indexes
                    _masterIndexingStrategy = new PollResultIndexingStrategyMultiForge(
                        streamViewStreamNum,
                        indexingStrategies);
                }
            }

            // there is one type of index
            if (_indexesUsedByStreams.Count == 1) {
                return JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                    _queryGraph,
                    _typesPerStream[_historicalStreamNum],
                    _typesPerStream[streamViewStreamNum],
                    _historicalStreamNum,
                    streamViewStreamNum,
                    raw,
                    serdeResolver);
            }

            // determine which index number the polling stream must use
            var indexUsed = 0;
            var found = false;
            foreach (var desc in _indexesUsedByStreams.Values) {
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
            JoinSetComposerPrototypeHistoricalDesc indexing = JoinSetComposerPrototypeForgeFactory.DetermineIndexing(
                _queryGraph,
                _typesPerStream[_historicalStreamNum],
                _typesPerStream[streamViewStreamNum],
                _historicalStreamNum,
                streamViewStreamNum,
                raw,
                serdeResolver);
            HistoricalIndexLookupStrategyForge innerLookupStrategy = indexing.LookupForge;
            HistoricalIndexLookupStrategyForge lookupStrategy = new HistoricalIndexLookupStrategyMultiForge(indexUsed, innerLookupStrategy);
            additionalForgeables.AddAll(indexing.AdditionalForgeables);
            return new JoinSetComposerPrototypeHistoricalDesc(lookupStrategy, _masterIndexingStrategy, additionalForgeables);
        }

        private Type[] GetPropertyTypes(
            EventType eventType,
            string[] properties)
        {
            var types = new Type[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                types[i] = Boxing.GetBoxedType(eventType.GetPropertyType(properties[i]));
            }

            return types;
        }

        private Type[] GetPropertyTypes(IList<QueryGraphValueEntryHashKeyedForge> hashKeys)
        {
            var types = new Type[hashKeys.Count];
            for (var i = 0; i < hashKeys.Count; i++) {
                types[i] = Boxing.GetBoxedType(hashKeys[i].KeyExpr.Forge.EvaluationType);
            }

            return types;
        }
    }
} // end of namespace