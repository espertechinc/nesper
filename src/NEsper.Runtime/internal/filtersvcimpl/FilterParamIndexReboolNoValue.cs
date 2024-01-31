///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
	/// <summary>
	/// Index for filter parameter constants to match using the equals (=) operator.
	/// The implementation is based on a regular HashMap.
	/// </summary>
	public class FilterParamIndexReboolNoValue : FilterParamIndexLookupableBase
	{
		protected EventEvaluator eventEvaluator;
		protected readonly IReaderWriterLock constantsMapRWLock;

		internal FilterParamIndexReboolNoValue(
			ExprFilterSpecLookupable lookupable,
			IReaderWriterLock readWriteLock)
			: base(FilterOperator.REBOOL, lookupable)
		{
			constantsMapRWLock = readWriteLock;
		}

		public override EventEvaluator Get(object filterConstant)
		{
			return eventEvaluator;
		}

		public override void Put(
			object filterConstant,
			EventEvaluator evaluator)
		{
			this.eventEvaluator = evaluator;
		}

		public override void Remove(object filterConstant)
		{
			this.eventEvaluator = null;
		}

		public override int CountExpensive {
			get { return eventEvaluator == null ? 0 : 1; }
		}

		public override bool IsEmpty {
			get { return eventEvaluator == null; }
		}

		public override IReaderWriterLock ReadWriteLock {
			get { return constantsMapRWLock; }
		}

		public override void GetTraverseStatement(
			EventTypeIndexTraverse traverse,
			ICollection<int> statementIds,
			ArrayDeque<FilterItem> evaluatorStack)
		{
			if (eventEvaluator != null) {
				evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, null, this));
				eventEvaluator.GetTraverseStatement(traverse, statementIds, evaluatorStack);
				evaluatorStack.RemoveLast();
			}
		}

		public override void MatchEvent(
			EventBean theEvent,
			ICollection<FilterHandle> matches,
			ExprEvaluatorContext ctx)
		{
			if (eventEvaluator == null) {
				return;
			}

			EventBean[] events = new EventBean[] {theEvent};
			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().QFilterReverseIndex(this, null);
			}

			var result = Lookupable.Expr.Evaluate(events, true, ctx).AsBoxedBoolean();
			if (result != null && result.Value) {
				eventEvaluator.MatchEvent(theEvent, matches, ctx);
			}

			if (InstrumentationHelper.ENABLED) {
				InstrumentationHelper.Get().AFilterReverseIndex(result);
			}
		}
	}
} // end of namespace
