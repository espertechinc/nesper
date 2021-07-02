///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	public class FAFQueryMethodSelectExecGivenContextNoFromClause : FAFQueryMethodSelectExec
	{
		private readonly StatementContextRuntimeServices _svc;
		private FAFQueryMethodSelectNoFromExprEvaluatorContext _exprEvaluatorContext;

		public FAFQueryMethodSelectExecGivenContextNoFromClause(StatementContextRuntimeServices svc)
		{
			_svc = svc;
		}

		public EPPreparedQueryResult Execute(
			FAFQueryMethodSelect select,
			ContextPartitionSelector[] contextPartitionSelectors,
			FAFQueryMethodAssignerSetter assignerSetter,
			ContextManagementService contextManagementService)
		{
			if (contextPartitionSelectors != null && contextPartitionSelectors.Length > 1) {
				throw new ArgumentException("Fire-and-forget queries without a from-clause allow only a single context partition selector");
			}

			var contextDeploymentId = ContextDeployTimeResolver.ResolveContextDeploymentId(
				select.ContextModuleName,
				NameAccessModifier.PUBLIC,
				select.ContextName,
				null,
				_svc.ContextPathRegistry);
			var contextManager = contextManagementService.GetContextManager(contextDeploymentId, select.ContextName);
			if (contextManager == null) {
				throw new EPException("Failed to find context manager for context '" + select.ContextName + "'");
			}

			var singleSelector = contextPartitionSelectors != null && contextPartitionSelectors.Length > 0
				? contextPartitionSelectors[0]
				: ContextPartitionSelectorAll.INSTANCE;
			var agentInstanceIds = contextManager.Realization.GetAgentInstanceIds(singleSelector);

			_exprEvaluatorContext = new FAFQueryMethodSelectNoFromExprEvaluatorContext(_svc, select);
			var resultSetProcessor = ProcessorWithAssign(
				select.ResultSetProcessorFactoryProvider,
				_exprEvaluatorContext,
				null,
				assignerSetter,
				select.TableAccesses,
				select.Subselects);

			var events = new ArrayDeque<EventBean>();
			var input = new EventBean[] {null};
			foreach (int agentInstanceId in agentInstanceIds) {
				_exprEvaluatorContext.ContextProperties = contextManager.GetContextPropertiesEvent(agentInstanceId);

				if (select.WhereClause != null) {
					var resultInner = select.WhereClause.Evaluate(CollectionUtil.EVENTBEANARRAY_EMPTY, true, _exprEvaluatorContext);
					if (resultInner != null || false.Equals(resultInner)) {
						continue;
					}
				}

				var results = resultSetProcessor.ProcessViewResult(input, null, true);
				if (results.First != null && results.First.Length > 0) {
					events.Add(results.First[0]);
				}
			}

			var result = events.ToArray();
			var distinct = EventBeanUtility.GetDistinctByProp(result, select.DistinctKeyGetter);
			return new EPPreparedQueryResult(resultSetProcessor.ResultEventType, distinct);
		}

		public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
		{
			_exprEvaluatorContext?.TableExprEvaluatorContext.ReleaseAcquiredLocks();
			_exprEvaluatorContext = null;
		}
	}
} // end of namespace
