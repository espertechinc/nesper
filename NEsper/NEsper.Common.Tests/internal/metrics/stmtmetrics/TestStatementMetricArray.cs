///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    [TestFixture]
    public class TestStatementMetricArray : CommonTest
    {
        [Test]
        public void TestFlowReportActive()
        {
            var rep = new StatementMetricArray("uri", "name", 3, false);

            var d001 = new DeploymentIdNamePair("A", "001");
            var d002 = new DeploymentIdNamePair("A", "002");
            var d003 = new DeploymentIdNamePair("A", "003");
            var d004 = new DeploymentIdNamePair("A", "004");
            var d005 = new DeploymentIdNamePair("A", "005");
            var d006 = new DeploymentIdNamePair("A", "006");
            var d007 = new DeploymentIdNamePair("A", "007");
            var d008 = new DeploymentIdNamePair("A", "008");
            var d009 = new DeploymentIdNamePair("A", "009");

            Assert.AreEqual(0, rep.SizeLastElement());

            Assert.AreEqual(0, rep.AddStatementGetIndex(d001));
            Assert.AreEqual(1, rep.SizeLastElement());

            Assert.AreEqual(1, rep.AddStatementGetIndex(d002));
            Assert.AreEqual(2, rep.AddStatementGetIndex(d003));
            Assert.AreEqual(3, rep.SizeLastElement());

            rep.RemoveStatement(d002);

            Assert.AreEqual(3, rep.AddStatementGetIndex(d004));
            Assert.AreEqual(4, rep.AddStatementGetIndex(d005));

            rep.RemoveStatement(d005);
            Assert.AreEqual(5, rep.AddStatementGetIndex(d006));

            var metrics = new StatementMetric[6];
            for (var i = 0; i < 6; i++)
            {
                metrics[i] = rep.GetAddMetric(i);
            }

            var flushed = rep.FlushMetrics();
            EPAssertionUtil.AssertSameExactOrder(metrics, flushed);

            Assert.AreEqual(1, rep.AddStatementGetIndex(d007));
            Assert.AreEqual(4, rep.AddStatementGetIndex(d008));

            rep.RemoveStatement(d001);
            rep.RemoveStatement(d003);
            rep.RemoveStatement(d004);
            rep.RemoveStatement(d006);
            rep.RemoveStatement(d007);
            Assert.AreEqual(6, rep.SizeLastElement());
            rep.RemoveStatement(d008);
            Assert.AreEqual(6, rep.SizeLastElement());

            flushed = rep.FlushMetrics();
            Assert.AreEqual(6, flushed.Length);
            Assert.AreEqual(0, rep.SizeLastElement());

            flushed = rep.FlushMetrics();
            Assert.IsNull(flushed);
            Assert.AreEqual(0, rep.SizeLastElement());

            Assert.AreEqual(0, rep.AddStatementGetIndex(d009));
            Assert.AreEqual(1, rep.SizeLastElement());

            flushed = rep.FlushMetrics();
            Assert.AreEqual(6, flushed.Length);
            for (var i = 0; i < flushed.Length; i++)
            {
                Assert.IsNull(flushed[i]);
            }
            Assert.AreEqual(1, rep.SizeLastElement());
        }

        [Test]
        public void TestFlowReportInactive()
        {
            var rep = new StatementMetricArray("uri", "name", 3, true);

            Assert.AreEqual(0, rep.AddStatementGetIndex(new DeploymentIdNamePair("A", "001")));
            Assert.AreEqual(1, rep.AddStatementGetIndex(new DeploymentIdNamePair("A", "002")));
            Assert.AreEqual(2, rep.AddStatementGetIndex(new DeploymentIdNamePair("A", "003")));
            rep.RemoveStatement(new DeploymentIdNamePair("A", "002"));

            var flushed = rep.FlushMetrics();
            for (var i = 0; i < 3; i++)
            {
                Assert.IsNotNull(flushed[i]);
            }
        }
    }
} // end of namespace