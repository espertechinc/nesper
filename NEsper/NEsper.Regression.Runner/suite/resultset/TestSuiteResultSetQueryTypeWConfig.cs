///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.suite.resultset.querytype;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.resultset
{
    [TestFixture]
    public class TestSuiteResultSetQueryTypeWConfig
    {
        [Test]
        public void TestResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.TimeSource.TimeUnit = TimeUnit.MICROSECONDS;
            RegressionRunner.Run(session, new ResultSetQueryTypeRowPerGroupReclaimMicrosecondResolution(5000000));
            session.Destroy();
        }
    }
} // end of namespace