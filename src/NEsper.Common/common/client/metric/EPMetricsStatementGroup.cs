///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.client.metric
{
    /// <summary>
    /// Represents a statement group in the query API for statement metrics
    /// </summary>
    public class EPMetricsStatementGroup
    {
        private readonly StatementMetricArray array;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="array">statements</param>
        public EPMetricsStatementGroup(StatementMetricArray array)
        {
            this.array = array;
        }

        /// <summary>
        /// Returns the group name
        /// </summary>
        /// <value>group name</value>
        public string Name => array.Name;

        /// <summary>
        /// Returns an indicator whether to report inactive statements
        /// </summary>
        /// <value>indicator</value>
        public bool IsReportInactive => array.IsReportInactive;

        /// <summary>
        /// Iterate statements of the group.
        /// <para />This obtains a read-lock on the list of statements belonging to that group, for the duration of the call.
        /// </summary>
        /// <param name="consumer">receives the statement metrics</param>
        public void IterateStatements(Consumer<EPMetricsStatement> consumer)
        {
            array.Enumerate(consumer);
        }
    }
} // end of namespace