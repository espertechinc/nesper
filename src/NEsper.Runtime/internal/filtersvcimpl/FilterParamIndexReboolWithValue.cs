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
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
	public sealed class FilterParamIndexReboolWithValue : FilterParamIndexEqualsBase
	{
		internal FilterParamIndexReboolWithValue(
			ExprFilterSpecLookupable lookupable,
			IReaderWriterLock readWriteLock)
			: base(lookupable, readWriteLock, FilterOperator.REBOOL)
		{
		}

		public override void MatchEvent(
			EventBean theEvent,
			ICollection<FilterHandle> matches,
			ExprEvaluatorContext ctx)
		{
			EventBean[] events = new EventBean[] {theEvent};
			foreach (KeyValuePair<object, EventEvaluator> entry in ConstantsMap) {
				ctx.FilterReboolConstant = entry.Key;
				if (InstrumentationHelper.ENABLED) {
					InstrumentationHelper.Get().QFilterReverseIndex(this, entry.Key);
				}

				var result = Lookupable.Expr.Evaluate(events, true, ctx).AsBoxedBoolean();
				if (result != null && result.Value) {
					entry.Value.MatchEvent(theEvent, matches, ctx);
				}

				if (InstrumentationHelper.ENABLED) {
					InstrumentationHelper.Get().AFilterReverseIndex(result);
				}
			}
		}
	}
} // end of namespace
