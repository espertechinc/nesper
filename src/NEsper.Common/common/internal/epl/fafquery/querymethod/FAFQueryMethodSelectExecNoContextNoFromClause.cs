///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecNoContextNoFromClause : FAFQueryMethodSelectExec
    {
        private readonly StatementContextRuntimeServices svc;
        private ExprEvaluatorContext exprEvaluatorContext;

        public FAFQueryMethodSelectExecNoContextNoFromClause(StatementContextRuntimeServices svc)
        {
            this.svc = svc;
        }

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            if (select.WhereClause != null) {
                var result = select.WhereClause.Evaluate(
                    CollectionUtil.EVENTBEANARRAY_EMPTY,
                    true,
                    exprEvaluatorContext);
                if (result == null || !(bool)result) {
                    return EPPreparedQueryResult.Empty(select.ResultSetProcessorFactoryProvider.ResultEventType);
                }
            }

            exprEvaluatorContext = new FAFQueryMethodSelectNoFromExprEvaluatorContext(svc, select);
            var resultSetProcessor = ProcessorWithAssign(
                select.ResultSetProcessorFactoryProvider,
                exprEvaluatorContext,
                null,
                assignerSetter,
                select.TableAccesses,
                select.Subselects);
            return ProcessedNonJoin(resultSetProcessor, new EventBean[] { null }, select.DistinctKeyGetter);
        }

        public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            if (exprEvaluatorContext != null) {
                exprEvaluatorContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
            }

            exprEvaluatorContext = null;
        }
    }
} // end of namespace