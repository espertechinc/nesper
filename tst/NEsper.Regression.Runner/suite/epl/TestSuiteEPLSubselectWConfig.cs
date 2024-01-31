///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.epl.subselect;
using com.espertech.esper.regressionrun.runner;
using com.espertech.esper.regressionrun.suite.core;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.epl
{
    [TestFixture]
    public class TestSuiteEPLSubselectWConfig : AbstractTestContainer
    {
        [Test, RunInApplicationDomain]
        public void TestEPLSubselectCorrelatedAggregationPerformance()
        {
            using var session = RegressionRunner.Session(Container);
            session.Configuration.Runtime.Expression.IsSelfSubselectPreeval = false;
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            RegressionRunner.Run(session, new EPLSubselectOrderOfEvalNoPreeval());
        }
    }
} // end of namespace