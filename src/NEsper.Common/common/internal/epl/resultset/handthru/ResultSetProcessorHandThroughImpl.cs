///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
	public class ResultSetProcessorHandThroughImpl : ResultSetProcessor
	{
		private readonly ResultSetProcessorHandThroughFactory _factory;
		private ExprEvaluatorContext _exprEvaluatorContext;

		public ResultSetProcessorHandThroughImpl(
			ResultSetProcessorHandThroughFactory factory,
			ExprEvaluatorContext exprEvaluatorContext)
		{
			_factory = factory;
			_exprEvaluatorContext = exprEvaluatorContext;
		}

		public EventType ResultEventType => _factory.ResultEventType;

		public UniformPair<EventBean[]> ProcessViewResult(
			EventBean[] newData,
			EventBean[] oldData,
			bool isSynthesize)
		{
			EventBean[] selectOldEvents = null;
			if (_factory.IsRstream) {
				selectOldEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruView(
					_factory.SelectExprProcessor,
					oldData,
					false,
					isSynthesize,
					_exprEvaluatorContext);
			}

			var selectNewEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruView(
				_factory.SelectExprProcessor,
				newData,
				true,
				isSynthesize,
				_exprEvaluatorContext);
			return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
		}

		public UniformPair<EventBean[]> ProcessJoinResult(
			ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
			ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
			bool isSynthesize)
		{
			EventBean[] selectOldEvents = null;
			if (_factory.IsRstream) {
				selectOldEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruJoin(
					_factory.SelectExprProcessor,
					oldEvents,
					false,
					isSynthesize,
					_exprEvaluatorContext);
			}

			var selectNewEvents = ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruJoin(
				_factory.SelectExprProcessor,
				newEvents,
				true,
				isSynthesize,
				_exprEvaluatorContext);
			return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
		}

		public IEnumerator<EventBean> GetEnumerator(Viewable viewable)
		{
			return new TransformEventEnumerator(viewable.GetEnumerator(), new ResultSetProcessorHandtruTransform(this));
		}

		public IEnumerator<EventBean> GetEnumerator(ISet<MultiKeyArrayOfKeys<EventBean>> joinSet)
		{
			var result = ProcessJoinResult(joinSet, EmptySet<MultiKeyArrayOfKeys<EventBean>>.Instance, true);
			return new ArrayEventEnumerator(result.First);
		}

		public void Clear()
		{
		}

		public void Stop()
		{
		}

		public UniformPair<EventBean[]> ProcessOutputLimitedJoin(
			IList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> joinEventsSet,
			bool generateSynthetic)
		{
			throw new UnsupportedOperationException();
		}

		public UniformPair<EventBean[]> ProcessOutputLimitedView(
			IList<UniformPair<EventBean[]>> viewEventsList,
			bool generateSynthetic)
		{
			throw new UnsupportedOperationException();
		}

		public ExprEvaluatorContext ExprEvaluatorContext {
			get => _exprEvaluatorContext;
			set => _exprEvaluatorContext = value;
		}

		public void SetExprEvaluatorContext(ExprEvaluatorContext value)
		{
			_exprEvaluatorContext = value;
		}
		
		public void ApplyViewResult(
			EventBean[] newData,
			EventBean[] oldData)
		{
			// not implemented
		}

		public void ApplyJoinResult(
			ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
			ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
		{
			// not implemented
		}

		public void ProcessOutputLimitedLastAllNonBufferedView(
			EventBean[] newData,
			EventBean[] oldData,
			bool isSynthesize)
		{
			// not implemented
		}

		public void ProcessOutputLimitedLastAllNonBufferedJoin(
			ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
			ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
			bool isGenerateSynthetic)
		{
			// not implemented
		}

		public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize)
		{
			throw new UnsupportedOperationException();
		}

		public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize)
		{
			throw new UnsupportedOperationException();
		}

		public void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
		{
		}
	}
} // end of namespace
