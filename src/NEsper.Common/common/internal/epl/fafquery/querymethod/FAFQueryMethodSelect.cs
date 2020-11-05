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

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodSelect : FAFQueryMethod
    {
        public Attribute[] Annotations { get; set; }

        public string ContextName { get; set; }

        public ExprEvaluator WhereClause { get; set; }

        public ExprEvaluator[] ConsumerFilters { get; set; }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider { get; set; }

        public FireAndForgetProcessor[] Processors { get; set; }

        public JoinSetComposerPrototype JoinSetComposerPrototype { get; set; }

        public QueryGraph QueryGraph { get; set; }

        public bool HasTableAccess { get; set; }

        public FAFQueryMethodSelectExec SelectExec { get; private set; }

        public IDictionary<int, ExprTableEvalStrategyFactory> TableAccesses { get; set; }
        
        public bool IsDistinct { get; set; }

        public EventPropertyValueGetter DistinctKeyGetter { get; set; }
        
        public IDictionary<int, SubSelectFactory> Subselects { get; set; }

        /// <summary>
        ///     Returns the event type of the prepared statement.
        /// </summary>
        /// <returns>event type</returns>
        public EventType EventType => ResultSetProcessorFactoryProvider.ResultEventType;

        public void Ready(StatementContextRuntimeServices services)
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
            
            if (!Subselects.IsEmpty()) {
                FAFQueryMethodUtil.InitializeSubselects(services, Annotations, Subselects);
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
                if (HasTableAccess) {
                    Processors[0].StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                }
            }
        }
    }
} // end of namespace