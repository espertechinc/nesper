///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.resultset.core;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public abstract class FAFQueryMethodSelectExecDBBase : FAFQueryMethodSelectExec
    {
        protected readonly StatementContextRuntimeServices services;

        public FAFQueryMethodSelectExecDBBase(StatementContextRuntimeServices services)
        {
            this.services = services;
        }

        protected abstract ICollection<EventBean> ExecuteInternal(
            ExprEvaluatorContext exprEvaluatorContext,
            FAFQueryMethodSelect select);

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            ExprEvaluatorContext exprEvaluatorContext =
                new FAFQueryMethodSelectNoFromExprEvaluatorContext(services, select);
            var resultSetProcessor = ProcessorWithAssign(
                select.ResultSetProcessorFactoryProvider,
                exprEvaluatorContext,
                null,
                assignerSetter,
                select.TableAccesses,
                select.Subselects);
            var rows = ExecuteInternal(exprEvaluatorContext, select);
            if (select.WhereClause != null) {
                rows = Filtered(rows, select.WhereClause, exprEvaluatorContext);
            }

            return ProcessedNonJoin(resultSetProcessor, rows, select.DistinctKeyGetter);
        }

        public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            services.TableExprEvaluatorContext.ReleaseAcquiredLocks();
        }
    }
} // end of namespace