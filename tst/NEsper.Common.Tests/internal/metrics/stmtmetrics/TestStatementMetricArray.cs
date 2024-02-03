///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.container;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    [TestFixture]
    public class TestStatementMetricArray : AbstractCommonTest
    {
        [Test]
        public void TestFlowReportActive()
        {
            var rep = new StatementMetricArray("uri", "name", 3, false, container.RWLockManager());

            var d001 = new DeploymentIdNamePair("A", "001");
            var d002 = new DeploymentIdNamePair("A", "002");
            var d003 = new DeploymentIdNamePair("A", "003");
            var d004 = new DeploymentIdNamePair("A", "004");
            var d005 = new DeploymentIdNamePair("A", "005");
            var d006 = new DeploymentIdNamePair("A", "006");
            var d007 = new DeploymentIdNamePair("A", "007");
            var d008 = new DeploymentIdNamePair("A", "008");
            var d009 = new DeploymentIdNamePair("A", "009");

            ClassicAssert.AreEqual(0, rep.SizeLastElement());

            ClassicAssert.AreEqual(0, rep.AddStatementGetIndex(d001));
            ClassicAssert.AreEqual(1, rep.SizeLastElement());

            ClassicAssert.AreEqual(1, rep.AddStatementGetIndex(d002));
            ClassicAssert.AreEqual(2, rep.AddStatementGetIndex(d003));
            ClassicAssert.AreEqual(3, rep.SizeLastElement());

            rep.RemoveStatement(d002);

            ClassicAssert.AreEqual(3, rep.AddStatementGetIndex(d004));
            ClassicAssert.AreEqual(4, rep.AddStatementGetIndex(d005));

            rep.RemoveStatement(d005);
            ClassicAssert.AreEqual(5, rep.AddStatementGetIndex(d006));

            var metrics = new StatementMetric[6];
            for (var i = 0; i < 6; i++)
            {
                metrics[i] = rep.GetAddMetric(i);
            }

            var flushed = rep.FlushMetrics();
            EPAssertionUtil.AssertSameExactOrder(metrics, flushed);

            ClassicAssert.AreEqual(1, rep.AddStatementGetIndex(d007));
            ClassicAssert.AreEqual(4, rep.AddStatementGetIndex(d008));

            rep.RemoveStatement(d001);
            rep.RemoveStatement(d003);
            rep.RemoveStatement(d004);
            rep.RemoveStatement(d006);
            rep.RemoveStatement(d007);
            ClassicAssert.AreEqual(6, rep.SizeLastElement());
            rep.RemoveStatement(d008);
            ClassicAssert.AreEqual(6, rep.SizeLastElement());

            flushed = rep.FlushMetrics();
            ClassicAssert.AreEqual(6, flushed.Length);
            ClassicAssert.AreEqual(0, rep.SizeLastElement());

            flushed = rep.FlushMetrics();
            ClassicAssert.IsNull(flushed);
            ClassicAssert.AreEqual(0, rep.SizeLastElement());

            ClassicAssert.AreEqual(0, rep.AddStatementGetIndex(d009));
            ClassicAssert.AreEqual(1, rep.SizeLastElement());

            flushed = rep.FlushMetrics();
            ClassicAssert.AreEqual(6, flushed.Length);
            for (var i = 0; i < flushed.Length; i++)
            {
                ClassicAssert.IsNull(flushed[i]);
            }
            ClassicAssert.AreEqual(1, rep.SizeLastElement());
        }

        [Test]
        public void TestFlowReportInactive()
        {
            var rep = new StatementMetricArray("uri", "name", 3, true, container.RWLockManager());

            ClassicAssert.AreEqual(0, rep.AddStatementGetIndex(new DeploymentIdNamePair("A", "001")));
            ClassicAssert.AreEqual(1, rep.AddStatementGetIndex(new DeploymentIdNamePair("A", "002")));
            ClassicAssert.AreEqual(2, rep.AddStatementGetIndex(new DeploymentIdNamePair("A", "003")));
            rep.RemoveStatement(new DeploymentIdNamePair("A", "002"));

            var flushed = rep.FlushMetrics();
            for (var i = 0; i < 3; i++)
            {
                ClassicAssert.IsNotNull(flushed[i]);
            }
        }
    }
} // end of namespace
