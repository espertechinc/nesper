///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compiler.@internal.util;
using com.espertech.esper.regressionlib.suite.client.basic;
using com.espertech.esper.regressionrun.runner;

using NUnit.Framework;

namespace com.espertech.esper.regressionrun.suite.client
{
    [TestFixture]
    public class TestSuiteClientBasic
    {
        private RegressionSession session;

        [SetUp]
        public void SetUp()
        {
            session = RegressionRunner.Session();
            session.Configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
        }

        [TearDown]
        public void TearDown()
        {
            session.Destroy();
            session = null;
        }

        [Test]
        public void TestClientBasicSelect()
        {
            RegressionRunner.Run(session, new ClientBasicSelect());
        }

        [Test]
        public void TestClientBasicFilter()
        {
            RegressionRunner.Run(session, new ClientBasicFilter());
        }

        [Test]
        public void TestClientBasicSelectClause()
        {
            RegressionRunner.Run(session, new ClientBasicSelectClause());
        }

        [Test]
        public void TestClientBasicAggregation()
        {
            RegressionRunner.Run(session, new ClientBasicAggregation());
        }

        [Test]
        public void TestClientBasicLengthWindow()
        {
            RegressionRunner.Run(session, new ClientBasicLengthWindow());
        }

        [Test]
        public void TestClientBasicPattern()
        {
            RegressionRunner.Run(session, new ClientBasicPattern());
        }

        [Test]
        public void TestClientBasicAnnotation()
        {
            RegressionRunner.Run(session, new ClientBasicAnnotation());
        }
    }
} // end of namespace