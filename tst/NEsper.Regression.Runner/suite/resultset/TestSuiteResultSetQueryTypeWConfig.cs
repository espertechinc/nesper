///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.resultset.querytype;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetQueryTypeWConfig : AbstractTestBase
    {
        public static void Configure(Configuration configuration)
        {
            configuration.Common.AddEventType(typeof(SupportBean));
            configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
        }
        
        [Test, RunInApplicationDomain]
        public void TestResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution()
        {
            RegressionRunner.Run(_session, new ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution(5000000));
        }
    }
} // end of namespace