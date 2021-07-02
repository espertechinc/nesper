///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	public class FAFQueryMethodSelectExecNoContextNoFromClause : FAFQueryMethodSelectExec
	{
		private readonly StatementContextRuntimeServices _svc;
		private ExprEvaluatorContext _exprEvaluatorContext;

		public FAFQueryMethodSelectExecNoContextNoFromClause(StatementContextRuntimeServices svc)
		{
			_svc = svc;
		}

		public EPPreparedQueryResult Execute(
			FAFQueryMethodSelect select,
			ContextPartitionSelector[] contextPartitionSelectors,
			FAFQueryMethodAssignerSetter assignerSetter,
			ContextManagementService contextManagementService)
		{
			if (select.WhereClause != null) {
				var result = select.WhereClause.Evaluate(CollectionUtil.EVENTBEANARRAY_EMPTY, true, _exprEvaluatorContext);
				if (result == null || false.Equals(result)) {
					return EPPreparedQueryResult.Empty(select.ResultSetProcessorFactoryProvider.ResultEventType);
				}
			}

			_exprEvaluatorContext = new FAFQueryMethodSelectNoFromExprEvaluatorContext(_svc, select);
			
			var resultSetProcessor = ProcessorWithAssign(
				select.ResultSetProcessorFactoryProvider,
				_exprEvaluatorContext,
				null,
				assignerSetter,
				select.TableAccesses,
				select.Subselects);
			
			return ProcessedNonJoin(resultSetProcessor, new EventBean[] {null}, select.DistinctKeyGetter);
		}

		public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
		{
			_exprEvaluatorContext?.TableExprEvaluatorContext.ReleaseAcquiredLocks();
			_exprEvaluatorContext = null;
		}
	}
} // end of namespace
