///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.epl.metric
{
    [TestFixture]
    public class TestMetricScheduleService 
    {
        private MetricScheduleService _svc;
        private SupportMetricExecution[] _execs;
        private IList<MetricExec> _executions;
    
        [SetUp]
        public void SetUp()
        {
            _svc = new MetricScheduleService();
    
            _execs = new SupportMetricExecution[100];
            for (int i = 0; i < _execs.Length; i++)
            {
                _execs[i] = new SupportMetricExecution();
            }
    
            _executions = new List<MetricExec>();
        }
    
        [Test]
        public void TestFlow()
        {
            _svc.CurrentTime = 1000;
            Assert.IsNull(_svc.NearestTime);
    
            _svc.Add(2000, _execs[0]);
            Assert.AreEqual(3000, (long) _svc.NearestTime);
    
            _svc.Add(2100, _execs[1]);
            Assert.AreEqual(3000, (long) _svc.NearestTime);
    
            _svc.Add(2000, _execs[2]);
            Assert.AreEqual(3000, (long) _svc.NearestTime);

            _svc.CurrentTime = 1100;
            _svc.Add(100, _execs[3]);
            Assert.AreEqual(1200, (long) _svc.NearestTime);

            _svc.CurrentTime = 1199;
            _svc.Evaluate(_executions);
            Assert.IsTrue(_executions.IsEmpty());

            _svc.CurrentTime = 1200;
            _svc.Evaluate(_executions);
            EPAssertionUtil.AssertEqualsExactOrder(_executions, new MetricExec[] { _execs[3] });
            Assert.AreEqual(3000, (long) _svc.NearestTime);
    
            _executions.Clear();
            _svc.CurrentTime = 2999;
            _svc.Evaluate(_executions);
            Assert.IsTrue(_executions.IsEmpty());

            _svc.CurrentTime = 3000;
            _svc.Evaluate(_executions);
            EPAssertionUtil.AssertEqualsExactOrder(_executions, new MetricExec[] { _execs[0], _execs[2] });
            Assert.AreEqual(3100, (long) _svc.NearestTime);
    
            _svc.Clear();
            Assert.IsNull(_svc.NearestTime);
    
            _executions.Clear();
            _svc.CurrentTime = long.MaxValue - 1;
            _svc.Evaluate(_executions);
            Assert.IsTrue(_executions.IsEmpty());
            Assert.IsNull(_svc.NearestTime);
        }
    }
}
