///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.strategy;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerPrototypeGeneral : JoinSetComposerPrototypeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private EventTableIndexService eventTableIndexService;
        private bool hasHistorical;
        private bool joinRemoveStream;
        private QueryPlan queryPlan;

        private StreamJoinAnalysisResultRuntime streamJoinAnalysisResult;
        private string[] streamNames;

        public StreamJoinAnalysisResultRuntime StreamJoinAnalysisResult {
            set => streamJoinAnalysisResult = value;
        }

        public string[] StreamNames {
            set => streamNames = value;
        }

        public QueryPlan QueryPlan {
            set => queryPlan = value;
        }

        public bool JoinRemoveStream {
            set => joinRemoveStream = value;
        }

        public EventTableIndexService EventTableIndexService {
            set => eventTableIndexService = value;
        }

        public bool HasHistorical {
            set => hasHistorical = value;
        }

        public override JoinSetComposerDesc Create(
            Viewable[] streamViews,
            bool isFireAndForget,
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            // Build indexes
            var indexSpecs = queryPlan.IndexSpecs;
            var indexesPerStream =
                new IDictionary<TableLookupIndexReqKey, EventTable>[indexSpecs.Length];
            var tableSecondaryIndexLocks = new ILockable[indexSpecs.Length];
            var hasTable = false;
            for (var streamNo = 0; streamNo < indexSpecs.Length; streamNo++) {
                if (indexSpecs[streamNo] == null) {
                    continue;
                }

                var items = indexSpecs[streamNo].Items;
                indexesPerStream[streamNo] = new LinkedHashMap<TableLookupIndexReqKey, EventTable>();

                if (streamJoinAnalysisResult.Tables[streamNo] != null) {
                    // build for tables
                    Table table = streamJoinAnalysisResult.Tables[streamNo];
                    var state = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
                    foreach (string indexName in state.IndexRepository.ExplicitIndexNames) { // add secondary indexes
                        var indexInner = state.GetIndex(indexName);
                        indexesPerStream[streamNo].Put(
                            new TableLookupIndexReqKey(indexName, null, table.Name), indexInner);
                    }

                    var index = state.GetIndex(table.Name); // add primary index
                    indexesPerStream[streamNo].Put(new TableLookupIndexReqKey(table.Name, null, table.Name), index);
                    hasTable = true;
                    tableSecondaryIndexLocks[streamNo] =
                        agentInstanceContext.StatementContext.StatementInformationals.IsWritesToTables
                            ? state.TableLevelRWLock.WriteLock
                            : state.TableLevelRWLock.ReadLock;
                }
                else {
                    // build tables for implicit indexes
                    foreach (var entry in items) {
                        EventTable index;

                        var virtualDWView = GetNamedWindowVirtualDataWindow(
                            streamNo, streamJoinAnalysisResult, agentInstanceContext);
                        if (virtualDWView != null) {
                            index = VirtualDWQueryPlanUtil.GetJoinIndexTable(items.Get(entry.Key));
                        }
                        else {
                            index = EventTableUtil.BuildIndex(
                                agentInstanceContext, streamNo, items.Get(entry.Key), streamTypes[streamNo], false,
                                entry.Value.IsUnique, null, null, isFireAndForget);
                        }

                        indexesPerStream[streamNo].Put(entry.Key, index);
                    }
                }
            }

            // obtain any external views
            var externalViews = new VirtualDWView[indexSpecs.Length];
            for (var i = 0; i < externalViews.Length; i++) {
                externalViews[i] = GetNamedWindowVirtualDataWindow(i, streamJoinAnalysisResult, agentInstanceContext);
            }

            // Build strategies
            var queryExecSpecs = queryPlan.ExecNodeSpecs;
            var queryStrategies = new QueryStrategy[queryExecSpecs.Length];
            for (var i = 0; i < queryExecSpecs.Length; i++) {
                var planNode = queryExecSpecs[i];
                if (planNode == null) {
                    Log.Debug(".makeComposer No execution node for stream " + i + " '" + streamNames[i] + "'");
                    continue;
                }

                var executionNode = planNode.MakeExec(
                    agentInstanceContext, indexesPerStream, streamTypes, streamViews, externalViews,
                    tableSecondaryIndexLocks);

                if (Log.IsDebugEnabled) {
                    Log.Debug(
                        ".makeComposer Execution nodes for stream " + i + " '" + streamNames[i] +
                        "' : \n" + ExecNode.Print(executionNode));
                }

                queryStrategies[i] = new ExecNodeQueryStrategy(i, streamTypes.Length, executionNode);
            }

            // Remove indexes that are from tables as these are only available to query strategies
            if (hasTable) {
                indexesPerStream = RemoveTableIndexes(indexesPerStream, streamJoinAnalysisResult.Tables);
            }

            // If this is not unidirectional and not a self-join (excluding self-outer-join)
            JoinSetComposerDesc joinSetComposerDesc;
            if (JoinSetComposerUtil.IsNonUnidirectionalNonSelf(
                isOuterJoins, streamJoinAnalysisResult.IsUnidirectional, streamJoinAnalysisResult.IsPureSelfJoin)) {
                JoinSetComposer composer;
                if (hasHistorical) {
                    composer = new JoinSetComposerHistoricalImpl(
                        eventTableIndexService.AllowInitIndex(isRecoveringResilient), indexesPerStream, queryStrategies,
                        streamViews, agentInstanceContext);
                }
                else {
                    if (isFireAndForget) {
                        composer = new JoinSetComposerFAFImpl(
                            indexesPerStream, queryStrategies, streamJoinAnalysisResult.IsPureSelfJoin,
                            agentInstanceContext, joinRemoveStream, isOuterJoins);
                    }
                    else {
                        composer = new JoinSetComposerImpl(
                            eventTableIndexService.AllowInitIndex(isRecoveringResilient), indexesPerStream,
                            queryStrategies, streamJoinAnalysisResult.IsPureSelfJoin, agentInstanceContext,
                            joinRemoveStream);
                    }
                }

                // rewrite the filter expression for all-inner joins in case "on"-clause outer join syntax was used to include those expressions
                joinSetComposerDesc = new JoinSetComposerDesc(composer, postJoinFilterEvaluator);
            }
            else {
                if (streamJoinAnalysisResult.IsUnidirectionalAll) {
                    JoinSetComposer composer = new JoinSetComposerAllUnidirectionalOuter(queryStrategies);
                    joinSetComposerDesc = new JoinSetComposerDesc(composer, postJoinFilterEvaluator);
                }
                else {
                    QueryStrategy driver;
                    int unidirectionalStream;
                    if (streamJoinAnalysisResult.IsUnidirectional) {
                        unidirectionalStream = streamJoinAnalysisResult.UnidirectionalStreamNumberFirst;
                        driver = queryStrategies[unidirectionalStream];
                    }
                    else {
                        unidirectionalStream = 0;
                        driver = queryStrategies[0];
                    }

                    JoinSetComposer composer = new JoinSetComposerStreamToWinImpl(
                        !isRecoveringResilient, indexesPerStream, streamJoinAnalysisResult.IsPureSelfJoin,
                        unidirectionalStream, driver, streamJoinAnalysisResult.UnidirectionalNonDriving);
                    joinSetComposerDesc = new JoinSetComposerDesc(composer, postJoinFilterEvaluator);
                }
            }

            // init if the join-set-composer allows it
            if (joinSetComposerDesc.JoinSetComposer.AllowsInit()) {
                // compile prior events per stream to preload any indexes
                var eventsPerStream = new EventBean[streamNames.Length][];
                var events = new List<EventBean>();
                for (var i = 0; i < eventsPerStream.Length; i++) {
                    // For named windows and tables, we don't need to preload indexes from the iterators as this is always done already
                    if (streamJoinAnalysisResult.NamedWindows[i] != null ||
                        streamJoinAnalysisResult.Tables[i] != null) {
                        continue;
                    }

                    IEnumerator<EventBean> it = null;
                    if (!(streamViews[i] is HistoricalEventViewable) && !(streamViews[i] is DerivedValueView)) {
                        try {
                            it = streamViews[i].GetEnumerator();
                        }
                        catch (UnsupportedOperationException ex) {
                            // Joins do not support the iterator
                        }
                    }

                    if (it != null) {
                        for (; it.MoveNext();) {
                            events.Add(it.Current);
                        }

                        eventsPerStream[i] = events.ToArray();
                        events.Clear();
                    }
                    else {
                        eventsPerStream[i] = new EventBean[0];
                    }
                }

                // init
                joinSetComposerDesc.JoinSetComposer.Init(eventsPerStream, agentInstanceContext);
            }

            return joinSetComposerDesc;
        }

        private VirtualDWView GetNamedWindowVirtualDataWindow(
            int streamNo,
            StreamJoinAnalysisResultRuntime streamJoinAnalysisResult,
            AgentInstanceContext agentInstanceContext)
        {
            NamedWindow namedWindow = streamJoinAnalysisResult.NamedWindows[streamNo];
            if (namedWindow == null) {
                return null;
            }

            if (!namedWindow.RootView.IsVirtualDataWindow) {
                return null;
            }

            var instance = namedWindow.GetNamedWindowInstance(agentInstanceContext);
            return instance.RootViewInstance.VirtualDataWindow;
        }

        private IDictionary<TableLookupIndexReqKey, EventTable>[] RemoveTableIndexes(
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            Table[] tablesPerStream)
        {
            var result = new IDictionary<TableLookupIndexReqKey, EventTable>[indexesPerStream.Length];
            for (var i = 0; i < indexesPerStream.Length; i++) {
                if (tablesPerStream[i] == null) {
                    result[i] = indexesPerStream[i];
                    continue;
                }

                result[i] = Collections.GetEmptyMap<TableLookupIndexReqKey, EventTable>();
            }

            return result;
        }
    }
} // end of namespace