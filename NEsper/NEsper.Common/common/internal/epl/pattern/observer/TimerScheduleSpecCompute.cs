///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
	public interface TimerScheduleSpecCompute
	{
	    TimerScheduleSpec Compute(
	        MatchedEventConvertor optionalConvertor,
	        MatchedEventMap beginState,
	        ExprEvaluatorContext exprEvaluatorContext,
	        TimeZoneInfo timeZone,
	        TimeAbacus timeAbacus);
	}
} // end of namespace