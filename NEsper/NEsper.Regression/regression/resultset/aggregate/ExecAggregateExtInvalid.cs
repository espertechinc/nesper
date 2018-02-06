///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateExtInvalid : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Expression.IsExtendedAggregation = false;
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryInvalid(epService, "select rate(10) from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'rate(10)': Unknown single-row function, aggregation function or mapped or indexed property named 'rate' could not be resolved [select rate(10) from SupportBean]");
        }
    }
} // end of namespace
