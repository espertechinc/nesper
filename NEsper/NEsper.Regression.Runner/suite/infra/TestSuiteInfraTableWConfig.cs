///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.infra.tbl;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.infra
{
    // see INFRA suite for additional Table tests
    [TestFixture]
    public class TestSuiteInfraTableWConfig
    {
        [Test, RunInApplicationDomain]
        public void TestInfraTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd()
        {
            RegressionSession session = RegressionRunner.Session();
            session.Configuration.Runtime.Execution.IsFairlock = true;
            session.Configuration.Common.AddEventType(typeof(SupportTopGroupSubGroupEvent));
            session.Configuration.Common.AddEventType(typeof(SupportBean));
            session.Configuration.Common.AddEventType(typeof(SupportBean_S0));
            RegressionRunner.Run(session, new InfraTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd());
            session.Destroy();
        }
    }
} // end of namespace