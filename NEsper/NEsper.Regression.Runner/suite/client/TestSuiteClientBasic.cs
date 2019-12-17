///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.suite.client.basic;
using com.espertech.esper.regressionrun.Runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientBasic
    {
        private RegressionSession _session;

        [SetUp]
        public void SetUp()
        {
            _session = RegressionRunner.Session();
            _session.Configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
        }

        [TearDown]
        public void TearDown()
        {
            _session.Destroy();
            _session = null;
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicSelect()
        {
            RegressionRunner.Run(_session, new ClientBasicSelect());
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicFilter()
        {
            RegressionRunner.Run(_session, new ClientBasicFilter());
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicSelectClause()
        {
            RegressionRunner.Run(_session, new ClientBasicSelectClause());
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicAggregation()
        {
            RegressionRunner.Run(_session, new ClientBasicAggregation());
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicLengthWindow()
        {
            RegressionRunner.Run(_session, new ClientBasicLengthWindow());
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicPattern()
        {
            RegressionRunner.Run(_session, new ClientBasicPattern());
        }

        [Test, RunInApplicationDomain]
        public void TestClientBasicAnnotation()
        {
            RegressionRunner.Run(_session, new ClientBasicAnnotation());
        }
    }
} // end of namespace