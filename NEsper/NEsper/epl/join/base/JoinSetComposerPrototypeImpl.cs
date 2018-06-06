///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.@base
{
    public class JoinSetComposerPrototypeImpl : JoinSetComposerPrototype
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _statementName;
        private readonly int _statementId;
        private readonly OuterJoinDesc[] _outerJoinDescList;
        private readonly ExprNode _optionalFilterNode;
        private readonly EventType[] _streamTypes;
        private readonly string[] _streamNames;
        private readonly StreamJoinAnalysisResult _streamJoinAnalysisResult;
        private readonly Attribute[] _annotations;
        private readonly HistoricalViewableDesc _historicalViewableDesc;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly QueryPlanIndex[] _indexSpecs;
        private readonly QueryPlan _queryPlan;
        private readonly HistoricalStreamIndexList[] _historicalStreamIndexLists;
        private readonly bool _joinRemoveStream;
        private readonly bool _isOuterJoins;
        private readonly TableService _tableService;
        private readonly EventTableIndexService _eventTableIndexService;

        public JoinSetComposerPrototypeImpl(
            string statementName,
            int statementId,
            OuterJoinDesc[] outerJoinDescList,
            ExprNode optionalFilterNode,
            EventType[] streamTypes,
            string[] streamNames,
            StreamJoinAnalysisResult streamJoinAnalysisResult,
            Attribute[] annotations,
            HistoricalViewableDesc historicalViewableDesc,
            ExprEvaluatorContext exprEvaluatorContext,
            QueryPlanIndex[] indexSpecs,
            QueryPlan queryPlan,
            HistoricalStreamIndexList[] historicalStreamIndexLists,
            bool joinRemoveStream,
            bool isOuterJoins,
            TableService tableService,
            EventTableIndexService eventTableIndexService)
        {
            _statementName = statementName;
            _statementId = statementId;
            _outerJoinDescList = outerJoinDescList;
            _optionalFilterNode = optionalFilterNode;
            _streamTypes = streamTypes;
            _streamNames = streamNames;
            _streamJoinAnalysisResult = streamJoinAnalysisResult;
            _annotations = annotations;
            _historicalViewableDesc = historicalViewableDesc;
            _exprEvaluatorContext = exprEvaluatorContext;
            _indexSpecs = indexSpecs;
            _queryPlan = queryPlan;
            _historicalStreamIndexLists = historicalStreamIndexLists;
            _joinRemoveStream = joinRemoveStream;
            _isOuterJoins = isOuterJoins;
            _tableService = tableService;
            _eventTableIndexService = eventTableIndexService;
        }

        public JoinSetComposerDesc Create(Viewable[] streamViews, bool isFireAndForget, AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            // Build indexes
            var indexesPerStream = new IDictionary<TableLookupIndexReqKey, EventTable>[_indexSpecs.Length];
            var tableSecondaryIndexLocks = new ILockable[_indexSpecs.Length];
            var hasTable = false;
            for (var streamNo = 0; streamNo < _indexSpecs.Length; streamNo++)
            {
                if (_indexSpecs[streamNo] == null)
                {
                    continue;
                }

                var items = _indexSpecs[streamNo].Items;
                indexesPerStream[streamNo] = new LinkedHashMap<TableLookupIndexReqKey, EventTable>();

                if (_streamJoinAnalysisResult.TablesPerStream[streamNo] != null)
                {
                    // build for tables
                    var metadata = _streamJoinAnalysisResult.TablesPerStream[streamNo];
                    var state = _tableService.GetState(metadata.TableName, agentInstanceContext.AgentInstanceId);
                    foreach (var indexName in state.SecondaryIndexes)
                    { // add secondary indexes
                        indexesPerStream[streamNo].Put(new TableLookupIndexReqKey(indexName, metadata.TableName), state.GetIndex(indexName));
                    }
                    var index = state.GetIndex(metadata.TableName); // add primary index
                    indexesPerStream[streamNo].Put(new TableLookupIndexReqKey(metadata.TableName, metadata.TableName), index);
                    hasTable = true;
                    tableSecondaryIndexLocks[streamNo] = agentInstanceContext.StatementContext.IsWritesToTables ?
                            state.TableLevelRWLock.WriteLock : state.TableLevelRWLock.ReadLock;
                }
                else
                {
                    // build tables for implicit indexes
                    foreach (var entry in items)
                    {
                        EventTable index;
                        if (_streamJoinAnalysisResult.ViewExternal[streamNo] != null)
                        {
                            VirtualDWView view = _streamJoinAnalysisResult.ViewExternal[streamNo].Invoke(agentInstanceContext);
                            index = view.GetJoinIndexTable(items.Get(entry.Key));
                        }
                        else
                        {
                            index = EventTableUtil.BuildIndex(
                                agentInstanceContext, streamNo, items.Get(entry.Key), _streamTypes[streamNo], false,
                                entry.Value.IsUnique, null, null, isFireAndForget);
                        }
                        indexesPerStream[streamNo].Put(entry.Key, index);
                    }
                }
            }

            // obtain any external views
            var externalViewProviders = _streamJoinAnalysisResult.ViewExternal;
            var externalViews = new VirtualDWView[externalViewProviders.Length];
            for (var i = 0; i < externalViews.Length; i++)
            {
                if (externalViewProviders[i] != null)
                {
                    externalViews[i] = _streamJoinAnalysisResult.ViewExternal[i].Invoke(agentInstanceContext);
                }
            }

            // Build strategies
            var queryExecSpecs = _queryPlan.ExecNodeSpecs;
            var queryStrategies = new QueryStrategy[queryExecSpecs.Length];
            for (var i = 0; i < queryExecSpecs.Length; i++)
            {
                var planNode = queryExecSpecs[i];
                if (planNode == null)
                {
                    Log.Debug(".MakeComposer No execution node for stream " + i + " '" + _streamNames[i] + "'");
                    continue;
                }

                var executionNode = planNode.MakeExec(
                    _statementName, _statementId, _annotations, indexesPerStream, _streamTypes, streamViews,
                    _historicalStreamIndexLists, externalViews, tableSecondaryIndexLocks);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".MakeComposer Execution nodes for stream " + i + " '" + _streamNames[i] +
                        "' : \n" + ExecNode.Print(executionNode));
                }

                queryStrategies[i] = new ExecNodeQueryStrategy(i, _streamTypes.Length, executionNode);
            }

            // Remove indexes that are from tables as these are only available to query strategies
            if (hasTable)
            {
                indexesPerStream = RemoveTableIndexes(indexesPerStream, _streamJoinAnalysisResult.TablesPerStream);
            }

            // If this is not unidirectional and not a self-join (excluding self-outer-join)
            JoinSetComposerDesc joinSetComposerDesc;
            if ((!_streamJoinAnalysisResult.IsUnidirectional) &&
                (!_streamJoinAnalysisResult.IsPureSelfJoin || _outerJoinDescList.Length > 0))
            {
                JoinSetComposer composer;
                if (_historicalViewableDesc.HasHistorical)
                {
                    composer = new JoinSetComposerHistoricalImpl(_eventTableIndexService.AllowInitIndex(isRecoveringResilient), indexesPerStream, queryStrategies, streamViews, _exprEvaluatorContext);
                }
                else
                {
                    if (isFireAndForget)
                    {
                        composer = new JoinSetComposerFAFImpl(indexesPerStream, queryStrategies, _streamJoinAnalysisResult.IsPureSelfJoin, _exprEvaluatorContext, _joinRemoveStream, _isOuterJoins);
                    }
                    else
                    {
                        composer = new JoinSetComposerImpl(_eventTableIndexService.AllowInitIndex(isRecoveringResilient), indexesPerStream, queryStrategies, _streamJoinAnalysisResult.IsPureSelfJoin, _exprEvaluatorContext, _joinRemoveStream);
                    }
                }

                // rewrite the filter expression for all-inner joins in case "on"-clause outer join syntax was used to include those expressions
                var filterExpression = GetFilterExpressionInclOnClause(_optionalFilterNode, _outerJoinDescList);

                var postJoinEval = filterExpression == null ? null : filterExpression.ExprEvaluator;
                joinSetComposerDesc = new JoinSetComposerDesc(composer, postJoinEval);
            }
            else
            {
                ExprEvaluator postJoinEval = _optionalFilterNode == null ? null : _optionalFilterNode.ExprEvaluator;

                if (_streamJoinAnalysisResult.IsUnidirectionalAll)
                {
                    JoinSetComposer composer = new JoinSetComposerAllUnidirectionalOuter(queryStrategies);
                    joinSetComposerDesc = new JoinSetComposerDesc(composer, postJoinEval);
                }
                else
                {
                    QueryStrategy driver;
                    int unidirectionalStream;
                    if (_streamJoinAnalysisResult.IsUnidirectional)
                    {
                        unidirectionalStream = _streamJoinAnalysisResult.UnidirectionalStreamNumberFirst;
                        driver = queryStrategies[unidirectionalStream];
                    }
                    else
                    {
                        unidirectionalStream = 0;
                        driver = queryStrategies[0];
                    }

                    JoinSetComposer composer = new JoinSetComposerStreamToWinImpl(
                        _eventTableIndexService.AllowInitIndex(isRecoveringResilient), indexesPerStream, 
                        _streamJoinAnalysisResult.IsPureSelfJoin,
                        unidirectionalStream, driver, 
                        _streamJoinAnalysisResult.UnidirectionalNonDriving);
                    joinSetComposerDesc = new JoinSetComposerDesc(composer, postJoinEval);
                }
            }

            // init if the join-set-composer allows it
            if (joinSetComposerDesc.JoinSetComposer.AllowsInit)
            {

                // compile prior events per stream to preload any indexes
                var eventsPerStream = new EventBean[_streamNames.Length][];
                var events = new List<EventBean>();
                for (var i = 0; i < eventsPerStream.Length; i++)
                {
                    // For named windows and tables, we don't need to preload indexes from the iterators as this is always done already
                    if (_streamJoinAnalysisResult.NamedWindow[i] || _streamJoinAnalysisResult.TablesPerStream[i] != null)
                    {
                        continue;
                    }

                    IEnumerator<EventBean> it = null;
                    if (!(streamViews[i] is HistoricalEventViewable) && !(streamViews[i] is DerivedValueView))
                    {
                        try
                        {
                            it = streamViews[i].GetEnumerator();
                        }
                        catch (UnsupportedOperationException)
                        {
                            // Joins do not support the iterator
                        }
                    }

                    if (it != null)
                    {
                        while (it.MoveNext())
                        {
                            events.Add(it.Current);
                        }
                        eventsPerStream[i] = events.ToArray();
                        events.Clear();
                    }
                    else
                    {
                        eventsPerStream[i] = new EventBean[0];
                    }
                }

                // init
                joinSetComposerDesc.JoinSetComposer.Init(eventsPerStream, _exprEvaluatorContext);
            }

            return joinSetComposerDesc;
        }

        private IDictionary<TableLookupIndexReqKey, EventTable>[] RemoveTableIndexes(IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, TableMetadata[] tablesPerStream)
        {
            var result = new IDictionary<TableLookupIndexReqKey, EventTable>[indexesPerStream.Length];
            for (var i = 0; i < indexesPerStream.Length; i++)
            {
                if (tablesPerStream[i] == null)
                {
                    result[i] = indexesPerStream[i];
                    continue;
                }
                result[i] = Collections.GetEmptyMap<TableLookupIndexReqKey, EventTable>();
            }
            return result;
        }

        private ExprNode GetFilterExpressionInclOnClause(ExprNode optionalFilterNode, OuterJoinDesc[] outerJoinDescList)
        {
            if (optionalFilterNode == null)
            {   // no need to add as query planning is fully based on on-clause
                return null;
            }
            if (outerJoinDescList.Length == 0)
            {  // not an outer-join syntax
                return optionalFilterNode;
            }
            if (!OuterJoinDesc.ConsistsOfAllInnerJoins(outerJoinDescList))
            {    // all-inner joins
                return optionalFilterNode;
            }

            var hasOnClauses = OuterJoinDesc.HasOnClauses(outerJoinDescList);
            if (!hasOnClauses)
            {
                return optionalFilterNode;
            }

            var expressions = new List<ExprNode>();
            expressions.Add(optionalFilterNode);

            foreach (var outerJoinDesc in outerJoinDescList)
            {
                if (outerJoinDesc.OptLeftNode != null)
                {
                    expressions.Add(outerJoinDesc.MakeExprNode(null));
                }
            }

            ExprAndNode andNode = ExprNodeUtility.ConnectExpressionsByLogicalAnd(expressions);
            try
            {
                andNode.Validate(null);
            }
            catch (ExprValidationException ex)
            {
                throw new EPRuntimeException("Unexpected exception validating expression: " + ex.Message, ex);
            }

            return andNode;
        }
    }
} // end of namespace
