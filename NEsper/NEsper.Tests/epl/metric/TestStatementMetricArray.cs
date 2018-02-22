///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client.metric;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.epl.metric
{
    [TestFixture]
    public class TestStatementMetricArray
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestFlowReportActive()
        {
            var rep = new StatementMetricArray("uri", "name", 3, false, _container.RWLockManager());

            Assert.AreEqual(0, rep.SizeLastElement);

            Assert.AreEqual(0, rep.AddStatementGetIndex("001"));
            Assert.AreEqual(1, rep.SizeLastElement);

            Assert.AreEqual(1, rep.AddStatementGetIndex("002"));
            Assert.AreEqual(2, rep.AddStatementGetIndex("003"));
            Assert.AreEqual(3, rep.SizeLastElement);

            rep.RemoveStatement("002");

            Assert.AreEqual(3, rep.AddStatementGetIndex("004"));
            Assert.AreEqual(4, rep.AddStatementGetIndex("005"));

            rep.RemoveStatement("005");
            Assert.AreEqual(5, rep.AddStatementGetIndex("006"));

            var metrics = new StatementMetric[6];
            for (int i = 0; i < 6; i++) {
                metrics[i] = rep.GetAddMetric(i);
            }

            StatementMetric[] flushed = rep.FlushMetrics();
            ArrayAssertionUtil.AssertSameExactOrder(metrics, flushed);

            Assert.AreEqual(1, rep.AddStatementGetIndex("007"));
            Assert.AreEqual(4, rep.AddStatementGetIndex("008"));

            rep.RemoveStatement("001");
            rep.RemoveStatement("003");
            rep.RemoveStatement("004");
            rep.RemoveStatement("006");
            rep.RemoveStatement("007");
            Assert.AreEqual(6, rep.SizeLastElement);
            rep.RemoveStatement("008");
            Assert.AreEqual(6, rep.SizeLastElement);

            flushed = rep.FlushMetrics();
            Assert.AreEqual(6, flushed.Length);
            Assert.AreEqual(0, rep.SizeLastElement);

            flushed = rep.FlushMetrics();
            Assert.IsNull(flushed);
            Assert.AreEqual(0, rep.SizeLastElement);

            Assert.AreEqual(0, rep.AddStatementGetIndex("009"));
            Assert.AreEqual(1, rep.SizeLastElement);

            flushed = rep.FlushMetrics();
            Assert.AreEqual(6, flushed.Length);
            for (int i = 0; i < flushed.Length; i++) {
                Assert.IsNull(flushed[i]);
            }
            Assert.AreEqual(1, rep.SizeLastElement);
        }

        [Test]
        public void TestFlowReportInactive()
        {
            var rep = new StatementMetricArray("uri", "name", 3, true, _container.RWLockManager());

            Assert.AreEqual(0, rep.AddStatementGetIndex("001"));
            Assert.AreEqual(1, rep.AddStatementGetIndex("002"));
            Assert.AreEqual(2, rep.AddStatementGetIndex("003"));
            rep.RemoveStatement("002");

            StatementMetric[] flushed = rep.FlushMetrics();
            for (int i = 0; i < 3; i++) {
                Assert.IsNotNull(flushed[i]);
            }
        }
    }
}
