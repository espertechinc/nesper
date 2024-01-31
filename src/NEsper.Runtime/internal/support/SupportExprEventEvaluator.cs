///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.runtime.@internal.support
{
	public class SupportExprEventEvaluator : ExprEventEvaluator
	{
		private readonly EventPropertyValueGetter _getter;

		public SupportExprEventEvaluator(EventPropertyValueGetter getter)
		{
			_getter = getter;
		}

		public object Eval(
			EventBean @event,
			ExprEvaluatorContext ctx)
		{
			return _getter.Get(@event);
		}
	}
} // end of namespace
