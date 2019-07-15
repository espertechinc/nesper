///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateExtInvalid : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            TryInvalidCompile(
                env,
                "select rate(10) from SupportBean",
                "Failed to valIdate select-clause expression 'rate(10)': Unknown single-row function, aggregation function or mapped or indexed property named 'rate' could not be resolved [select rate(10) from SupportBean]");
        }
    }
} // end of namespace