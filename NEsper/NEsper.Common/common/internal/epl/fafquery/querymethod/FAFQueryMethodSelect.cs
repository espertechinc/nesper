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
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.@join.@base;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelect : FAFQueryMethod
    {
        private bool isDistinct;

        public Attribute[] Annotations { get; private set; }

        public string ContextName { get; private set; }

        public ExprEvaluator WhereClause { get; private set; }

        public ExprEvaluator[] ConsumerFilters { get; private set; }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider { get; private set; }

        public FireAndForgetProcessor[] Processors { get; private set; }

        public EventBeanReader EventBeanReaderDistinct { get; private set; }

        public JoinSetComposerPrototype JoinSetComposerPrototype { get; private set; }

        public QueryGraph QueryGraph { get; private set; }

        public bool IsTableAccess { get; private set; }

        public FAFQueryMethodSelectExec SelectExec { get; private set; }

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses { get; private set; }

        /// <summary>
        ///     Returns the event type of the prepared statement.
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType => ResultSetProcessorFactoryProvider.ResultEventType;

        public void Ready()
        {
            var hasContext = false;
            for (var i = 0; i < Processors.Length; i++) {
                hasContext |= Processors[i].ContextName != null;
            }

            if (ContextName == null) {
                if (Processors.Length == 1) {
                    if (!hasContext) {
                        SelectExec = FAFQueryMethodSelectExecNoContextNoJoin.INSTANCE;
                    }
                    else {
                        SelectExec = FAFQueryMethodSelectExecSomeContextNoJoin.INSTANCE;
                    }
                }
                else {
                    if (!hasContext) {
                        SelectExec = FAFQueryMethodSelectExecNoContextJoin.INSTANCE;
                    }
                    else {
                        SelectExec = FAFQueryMethodSelectExecSomeContextJoin.INSTANCE;
                    }
                }
            }
            else {
                if (Processors.Length != 1) {
                    throw new UnsupportedOperationException("Context name is not supported in a join");
                }

                if (!hasContext) {
                    throw new UnsupportedOperationException("Query target is unpartitioned");
                }

                SelectExec = FAFQueryMethodSelectExecGivenContextNoJoin.INSTANCE;
            }
        }

        public EPPreparedQueryResult Execute(
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextPartitionSelector[] contextPartitionSelectors,
            ContextManagementService contextManagementService)
        {
            if (!serviceStatusProvider.Get()) {
                throw FAFQueryMethodUtil.RuntimeDestroyed();
            }

            if (contextPartitionSelectors != null && contextPartitionSelectors.Length != Processors.Length) {
                throw new ArgumentException(
                    "The number of context partition selectors does not match the number of named windows or tables in the from-clause");
            }

            try {
                return SelectExec.Execute(this, contextPartitionSelectors, assignerSetter, contextManagementService);
            }
            finally {
                if (IsTableAccess) {
                    Processors[0].StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }

        public void SetAnnotations(Attribute[] annotations)
        {
            Annotations = annotations;
        }

        public void SetProcessors(FireAndForgetProcessor[] processors)
        {
            Processors = processors;
        }

        public void SetResultSetProcessorFactoryProvider(
            ResultSetProcessorFactoryProvider resultSetProcessorFactoryProvider)
        {
            ResultSetProcessorFactoryProvider = resultSetProcessorFactoryProvider;
        }

        public void SetWhereClause(ExprEvaluator whereClause)
        {
            WhereClause = whereClause;
        }

        public void SetJoinSetComposerPrototype(JoinSetComposerPrototype joinSetComposerPrototype)
        {
            JoinSetComposerPrototype = joinSetComposerPrototype;
        }

        public void SetConsumerFilters(ExprEvaluator[] consumerFilters)
        {
            ConsumerFilters = consumerFilters;
        }

        public void SetQueryGraph(QueryGraph queryGraph)
        {
            QueryGraph = queryGraph;
        }

        public void SetContextName(string contextName)
        {
            ContextName = contextName;
        }

        public void SetTableAccesses(IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses)
        {
            TableAccesses = tableAccesses;
        }

        public void SetHasTableAccess(bool hasTableAccess)
        {
            IsTableAccess = hasTableAccess;
        }

        public void SetDistinct(bool distinct)
        {
            isDistinct = distinct;
            if (isDistinct) {
                var resultEventType = ResultSetProcessorFactoryProvider.ResultEventType;
                if (resultEventType is EventTypeSPI) {
                    EventBeanReaderDistinct = ((EventTypeSPI) resultEventType).Reader;
                }

                if (EventBeanReaderDistinct == null) {
                    EventBeanReaderDistinct = new EventBeanReaderDefaultImpl(resultEventType);
                }
            }
        }
    }
} // end of namespace