///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Manages index-building and sharing for historical streams by collecting required 
    /// indexes during the query planning phase, and by providing the right lookup strategy 
    /// and indexing strategy during query execution node creation.
    /// </summary>
    public class HistoricalStreamIndexList
    {
        private readonly int _historicalStreamNum;
        private readonly EventType[] _typesPerStream;
        private readonly QueryGraph _queryGraph;
        private readonly SortedSet<int> _pollingStreams;
    
        private IDictionary<HistoricalStreamIndexDesc, IList<int>> _indexesUsedByStreams;
        private PollResultIndexingStrategy _masterIndexingStrategy;
    
        /// <summary>Ctor. </summary>
        /// <param name="historicalStreamNum">number of the historical stream</param>
        /// <param name="typesPerStream">event types for each stream</param>
        /// <param name="queryGraph">relationship between key and index properties</param>
        public HistoricalStreamIndexList(int historicalStreamNum, EventType[] typesPerStream, QueryGraph queryGraph)
        {
            _historicalStreamNum = historicalStreamNum;
            _typesPerStream = typesPerStream;
            _queryGraph = queryGraph;
            _pollingStreams = new SortedSet<int>();
        }
    
        /// <summary>Used during query plan phase to indicate that an index must be provided for use in lookup of historical events by using a stream's events. </summary>
        /// <param name="streamViewStreamNum">the stream providing lookup events</param>
        public void AddIndex(int streamViewStreamNum)
        {
            _pollingStreams.Add(streamViewStreamNum);
        }
    
        /// <summary>Get the strategies to use for polling from a given stream. </summary>
        /// <param name="streamViewStreamNum">the stream providing the polling events</param>
        /// <returns>looking and indexing strategy</returns>
        public Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> GetStrategy(int streamViewStreamNum)
        {
            // If there is only a single polling stream, then build a single index
            if (_pollingStreams.Count == 1)
            {
                return JoinSetComposerPrototypeFactory.DetermineIndexing(_queryGraph, _typesPerStream[_historicalStreamNum], _typesPerStream[streamViewStreamNum], _historicalStreamNum, streamViewStreamNum);
            }
    
            // If there are multiple polling streams, determine if a single index is appropriate.
            // An index can be reused if:
            //  (a) indexed property names are the same
            //  (b) indexed property types are the same
            //  (c) key property types are the same (because of coercion)
            // A index lookup strategy is always specific to the providing stream.
            if (_indexesUsedByStreams == null)
            {
                _indexesUsedByStreams = new LinkedHashMap<HistoricalStreamIndexDesc, IList<int>>();
                foreach (var pollingStream in _pollingStreams)
                {
                    var queryGraphValue = _queryGraph.GetGraphValue(pollingStream, _historicalStreamNum);
                    var hashKeyProps = queryGraphValue.HashKeyProps;
                    var indexProperties = hashKeyProps.Indexed;
    
                    var keyTypes = GetPropertyTypes(hashKeyProps.Keys);
                    var indexTypes = GetPropertyTypes(_typesPerStream[_historicalStreamNum], indexProperties);
    
                    var desc = new HistoricalStreamIndexDesc(indexProperties, indexTypes, keyTypes);
                    var usedByStreams = _indexesUsedByStreams.Get(desc);
                    if (usedByStreams == null)
                    {
                        usedByStreams = new List<int>();
                        _indexesUsedByStreams.Put(desc, usedByStreams);
                    }
                    usedByStreams.Add(pollingStream);
                }
    
                // There are multiple indexes required:
                // Build a master indexing strategy that forms multiple indexes and numbers each.
                if (_indexesUsedByStreams.Count > 1)
                {
                    var numIndexes = _indexesUsedByStreams.Count;
                    var indexingStrategies = new PollResultIndexingStrategy[numIndexes];
    
                    // create an indexing strategy for each index
                    var count = 0;
                    foreach (var desc in _indexesUsedByStreams)
                    {
                        var sampleStreamViewStreamNum = desc.Value[0];
                        indexingStrategies[count] = JoinSetComposerPrototypeFactory.DetermineIndexing(_queryGraph, _typesPerStream[_historicalStreamNum], _typesPerStream[sampleStreamViewStreamNum], _historicalStreamNum, sampleStreamViewStreamNum).Second;
                        count++;
                    }
    
                    // create a master indexing strategy that utilizes each indexing strategy to create a set of indexes
                    var streamNum = streamViewStreamNum;
                    _masterIndexingStrategy = new ProxyPollResultIndexingStrategy
                    {
                        ProcIndex = (pollResult, isActiveCache) =>
                        {
                            var tables = new EventTable[numIndexes];
                            for (var i = 0; i < numIndexes; i++)
                            {
                                tables[i] = indexingStrategies[i].Index(pollResult, isActiveCache)[0];
                            }
    
                            var organization = new EventTableOrganization(null, false, false, streamNum, null, EventTableOrganization.EventTableOrganizationType.MULTIINDEX);
                            return new EventTable[]
                            {
                                new MultiIndexEventTable(tables, organization)
                            };
                        },
                        ProcToQueryPlan = () =>
                        {
                            var writer = new StringWriter();
                            var delimiter = "";
                            foreach (var strategy in indexingStrategies) {
                                writer.Write(delimiter);
                                writer.Write(strategy.ToQueryPlan());
                                delimiter = ", ";
                            }
                            return GetType().FullName + " " + writer;
                        }
                    };
                }
            }
    
            // there is one type of index
            if (_indexesUsedByStreams.Count == 1)
            {
                return JoinSetComposerPrototypeFactory.DetermineIndexing(
                    _queryGraph, _typesPerStream[_historicalStreamNum], _typesPerStream[streamViewStreamNum], _historicalStreamNum, streamViewStreamNum);
            }
    
            // determine which index number the polling stream must use
            var indexUsed = 0;
            var found = false;
            foreach (var desc in _indexesUsedByStreams.Values)
            {
                if (desc.Contains(streamViewStreamNum))
                {
                    found = true;
                    break;
                }
                indexUsed++;
            }
            if (!found) {
                throw new IllegalStateException("MapIndex not found for use by stream " + streamViewStreamNum);
            }
    
            // Use one of the indexes built by the master index and a lookup strategy
            var indexNumber = indexUsed;
            HistoricalIndexLookupStrategy innerLookupStrategy = JoinSetComposerPrototypeFactory.DetermineIndexing(_queryGraph, _typesPerStream[_historicalStreamNum], _typesPerStream[streamViewStreamNum], _historicalStreamNum, streamViewStreamNum).First;
    
            var lookupStrategy = new ProxyHistoricalIndexLookupStrategy
            {
                ProcLookup = (lookupEvent, index, context) =>
                {
                    var multiIndex = (MultiIndexEventTable) index[0];
                    var indexToUse = multiIndex.Tables[indexNumber];
                    return innerLookupStrategy.Lookup(lookupEvent, new EventTable[] { indexToUse }, context);
                },
                ProcToQueryPlan = () => GetType().FullName + " inner: " + innerLookupStrategy.ToQueryPlan()
            };
    
            return new Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> (lookupStrategy, _masterIndexingStrategy);
        }
    
        private Type[] GetPropertyTypes(EventType eventType, IList<string> properties)
        {
            var types = new Type[properties.Count];
            for (var i = 0; i < properties.Count; i++)
            {
                types[i] = eventType.GetPropertyType(properties[i]).GetBoxedType();
            }
            return types;
        }
    
        private Type[] GetPropertyTypes(IList<QueryGraphValueEntryHashKeyed> hashKeys)
        {
            var types = new Type[hashKeys.Count];
            for (var i = 0; i < hashKeys.Count; i++)
            {
                types[i] = hashKeys[i].KeyExpr.ExprEvaluator.ReturnType.GetBoxedType();
            }
            return types;
        }
    
    }
}
