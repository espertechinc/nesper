///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    [TestFixture]
    public class TestMetricScheduleService : AbstractTestBase
    {
        private MetricScheduleService svc;
        private SupportMetricExecution[] execs;
        private IList<MetricExec> executions;

        [SetUp]
        public void SetUp()
        {
            svc = new MetricScheduleService();

            execs = new SupportMetricExecution[100];
            for (int i = 0; i < execs.Length; i++)
            {
                execs[i] = new SupportMetricExecution();
            }

            executions = new List<MetricExec>();
        }

        [Test]
        public void TestFlow()
        {
            svc.CurrentTime = 1000;
            Assert.IsNull(svc.NearestTime);

            svc.Add(2000, execs[0]);
            Assert.AreEqual(3000, svc.NearestTime.Value);

            svc.Add(2100, execs[1]);
            Assert.AreEqual(3000, svc.NearestTime.Value);

            svc.Add(2000, execs[2]);
            Assert.AreEqual(3000, svc.NearestTime.Value);

            svc.CurrentTime = 1100;
            svc.Add(100, execs[3]);
            Assert.AreEqual(1200, svc.NearestTime.Value);

            svc.CurrentTime = 1199;
            svc.Evaluate(executions);
            Assert.IsTrue(executions.IsEmpty());

            svc.CurrentTime = 1200;
            svc.Evaluate(executions);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { execs[3] }, executions.GetEnumerator());
            Assert.AreEqual(3000, svc.NearestTime.Value);

            executions.Clear();
            svc.CurrentTime = 2999;
            svc.Evaluate(executions);
            Assert.IsTrue(executions.IsEmpty());

            svc.CurrentTime = 3000;
            svc.Evaluate(executions);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] { execs[0], execs[2] }, executions.GetEnumerator());
            Assert.AreEqual(3100, svc.NearestTime.Value);

            svc.Clear();
            Assert.IsNull(svc.NearestTime);

            executions.Clear();
            svc.CurrentTime = Int64.MaxValue - 1;
            svc.Evaluate(executions);
            Assert.IsTrue(executions.IsEmpty());
            Assert.IsNull(svc.NearestTime);
        }
    }
} // end of namespace