///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.metric
{
    /// <summary>
    /// Represents a statement in the query API for statement metrics
    /// </summary>
    public class EPMetricsStatement
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="metric">metric</param>
        public EPMetricsStatement(StatementMetric metric)
        {
            Metric = metric;
        }

        /// <summary>
        /// Returns the metrics object
        /// </summary>
        /// <value>metrics</value>
        public StatementMetric Metric { get; }
    }
} // end of namespace