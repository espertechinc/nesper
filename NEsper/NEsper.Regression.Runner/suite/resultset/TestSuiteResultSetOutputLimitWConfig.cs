///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.resultset.outputlimit;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetOutputLimitWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitChangeSetOpt()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Compiler.ViewResources.OutputLimitOpt = false;
            RegressionRunner.Run(session, new ResultSetOutputLimitChangeSetOpt(false));
            session.Destroy();
        }

        [Test, RunInApplicationDomain]
        public void TestResultSetOutputLimitMicrosecondResolution()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
            RegressionRunner.Run(session,
                new ResultSetOutputLimitMicrosecondResolution(0, "1", 1000000, 1000000));
            RegressionRunner.Run(session,
                new ResultSetOutputLimitMicrosecondResolution(789123456789L, "0.1", 789123456789L + 100000, 100000));
            session.Destroy();
        }
    }
} // end of namespace