///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;


namespace com.espertech.esper.epl.metric
{
    /// <summary>Execution context for metrics reporting executions. </summary>
    public class MetricExecutionContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="epServicesContext">services context</param>
        /// <param name="runtime">for routing events</param>
        /// <param name="statementMetricRepository">for getting statement data</param>
        public MetricExecutionContext(EPServicesContext epServicesContext, EPRuntime runtime, StatementMetricRepository statementMetricRepository)
        {
            Services = epServicesContext;
            Runtime = runtime;
            StatementMetricRepository = statementMetricRepository;
        }

        /// <summary>Returns services. </summary>
        /// <value>services</value>
        public EPServicesContext Services { get; private set; }

        /// <summary>Returns runtime </summary>
        /// <value>runtime</value>
        public EPRuntime Runtime { get; private set; }

        /// <summary>Returns statement metric holder </summary>
        /// <value>holder for metrics</value>
        public StatementMetricRepository StatementMetricRepository { get; private set; }
    }
}
