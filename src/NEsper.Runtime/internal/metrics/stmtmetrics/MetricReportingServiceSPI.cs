///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;

namespace com.espertech.esper.runtime.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     SPI for metrics activity.
    /// </summary>
    public interface MetricReportingServiceSPI : MetricReportingService
    {
        /// <summary>
        ///     Returns output hooks.
        /// </summary>
        /// <returns>hooks.</returns>
        ICollection<MetricsStatementResultListener> StatementOutputHooks { get; }

        /// <summary>
        ///     Add stmt result listener.
        /// </summary>
        /// <param name="listener">to add</param>
        void AddStatementResultListener(MetricsStatementResultListener listener);

        /// <summary>
        ///     Remove stmt result listener.
        /// </summary>
        /// <param name="listener">to remove</param>
        void RemoveStatementResultListener(MetricsStatementResultListener listener);
    }
} // end of namespace