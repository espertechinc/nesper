///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelect : FAFQueryMethod
    {
        private Attribute[] annotations;
        private string contextName;
        private string contextModuleName;
        private ExprEvaluator whereClause;
        private ExprEvaluator[] consumerFilters;
        private ResultSetProcessorFactoryProvider resultSetProcessorFactoryProvider;
        private FireAndForgetProcessor[] processors;
        private JoinSetComposerPrototype joinSetComposerPrototype;
        private QueryGraph queryGraph;
        private IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses;
        private bool hasTableAccess;
        private EventPropertyValueGetter distinctKeyGetter;
        private IDictionary<int, SubSelectFactory> subselects;

        private FAFQueryMethodSelectExec selectExec;

        /// <summary>
        /// Returns the event type of the prepared statement.
        /// </summary>
        /// <value>event type</value>
        public EventType EventType => resultSetProcessorFactoryProvider.ResultEventType;

        public FAFQuerySessionUnprepared ReadyUnprepared(StatementContextRuntimeServices services)
        {
            Ready(false, services);
            return new FAFQueryMethodSelectSessionUnprepared(this);
        }

        public FAFQueryMethodSessionPrepared ReadyPrepared(StatementContextRuntimeServices services)
        {
            Ready(true, services);
            selectExec.Prepare(this);
            return new FAFQueryMethodSelectSessionPrepared(this);
        }

        private void Ready(
            bool prepared,
            StatementContextRuntimeServices svc)
        {
            var hasContext = false;
            for (var i = 0; i < processors.Length; i++) {
                hasContext |= processors[i].ContextName != null;
            }

            if (contextName == null) {
                if (processors.Length == 0) {
                    selectExec = new FAFQueryMethodSelectExecNoContextNoFromClause(svc);
                }
                else if (processors.Length == 1) {
                    if (processors[0] is FireAndForgetProcessorDB) {
                        if (!prepared) {
                            selectExec = new FAFQueryMethodSelectExecDBUnprepared(svc);
                        }
                        else {
                            selectExec = new FAFQueryMethodSelectExecDBPrepared(svc);
                        }
                    }
                    else if (!hasContext) {
                        selectExec = FAFQueryMethodSelectExecNoContextNoJoin.INSTANCE;
                    }
                    else {
                        selectExec = FAFQueryMethodSelectExecSomeContextNoJoin.INSTANCE;
                    }
                }
                else {
                    if (!hasContext) {
                        selectExec = FAFQueryMethodSelectExecNoContextJoin.INSTANCE;
                    }
                    else {
                        selectExec = FAFQueryMethodSelectExecSomeContextJoin.INSTANCE;
                    }
                }
            }
            else {
                if (processors.Length == 0) {
                    selectExec = new FAFQueryMethodSelectExecGivenContextNoFromClause(svc);
                }
                else {
                    if (processors.Length != 1) {
                        throw new UnsupportedOperationException("Context name is not supported in a join");
                    }

                    if (!hasContext) {
                        throw new UnsupportedOperationException("Query target is unpartitioned");
                    }

                    selectExec = FAFQueryMethodSelectExecGivenContextNoJoin.INSTANCE;
                }
            }

            if (!subselects.IsEmpty()) {
                InitializeSubselects(svc, annotations, subselects);
            }
        }

        public EPPreparedQueryResult Execute(
            AtomicBoolean serviceStatusProvider,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextPartitionSelector[] contextPartitionSelectors,
            ContextManagementService contextManagementService)
        {
            if (!serviceStatusProvider.Get()) {
                throw RuntimeDestroyed();
            }

            if (processors.Length > 0 &&
                contextPartitionSelectors != null &&
                contextPartitionSelectors.Length != processors.Length) {
                throw new ArgumentException(
                    "The number of context partition selectors does not match the number of named windows or tables in the from-clause");
            }

            try {
                return selectExec.Execute(this, contextPartitionSelectors, assignerSetter, contextManagementService);
            }
            finally {
                if (hasTableAccess) {
                    selectExec.ReleaseTableLocks(processors);
                }
            }
        }

        public Attribute[] Annotations {
            get => annotations;
            set => annotations = value;
        }

        public string ContextName {
            get => contextName;
            set => contextName = value;
        }

        public ExprEvaluator WhereClause {
            get => whereClause;
            set => whereClause = value;
        }

        public ExprEvaluator[] ConsumerFilters {
            get => consumerFilters;
            set => consumerFilters = value;
        }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider {
            get => resultSetProcessorFactoryProvider;
            set => resultSetProcessorFactoryProvider = value;
        }

        public FireAndForgetProcessor[] Processors {
            get => processors;
            set => processors = value;
        }

        public JoinSetComposerPrototype JoinSetComposerPrototype {
            get => joinSetComposerPrototype;
            set => joinSetComposerPrototype = value;
        }

        public QueryGraph QueryGraph {
            get => queryGraph;
            set => queryGraph = value;
        }

        public bool HasTableAccess {
            get => hasTableAccess;
            set => hasTableAccess = value;
        }

        public FAFQueryMethodSelectExec SelectExec => selectExec;

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses {
            get => tableAccesses;
            set => tableAccesses = value;
        }

        public EventPropertyValueGetter DistinctKeyGetter {
            get => distinctKeyGetter;
            set => distinctKeyGetter = value;
        }

        public IDictionary<int, SubSelectFactory> Subselects {
            get => subselects;
            set => subselects = value;
        }

        public string ContextModuleName {
            get => contextModuleName;
            set => contextModuleName = value;
        }
    }
} // end of namespace