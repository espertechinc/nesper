///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.expr.filter;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.expr
{
    [TestFixture]
    public class TestSuiteExprFilterWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestExprFilterLargeThreading()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.AddEventType(typeof(SupportTradeEvent));
            session.Configuration.Common.Execution.ThreadingProfile = ThreadingProfile.LARGE;
            RegressionRunner.Run(session, new ExprFilterLargeThreading());
            session.Destroy();
        }
    }
} // end of namespace