///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public interface EPServicesEvaluation {
		FilterService FilterService { get; }

		IReaderWriterLock EventProcessingRWLock { get; }

		MetricReportingService MetricReportingService { get; }

		SchedulingService SchedulingService { get; }

		VariableManagementService VariableManagementService { get; }

		ExceptionHandlingService ExceptionHandlingService { get; }

		TableExprEvaluatorContext TableExprEvaluatorContext { get; }

		InternalEventRouteDest InternalEventRouteDest { get; }

		EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory { get; }
	}
} // end of namespace
