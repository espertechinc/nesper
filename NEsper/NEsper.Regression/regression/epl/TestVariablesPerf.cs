///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestVariablesPerf 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestConstantPerformance() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("create const variable String MYCONST = 'E331'");
    
            for (int i = 0; i < 10000; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i * -1));
            }
    
            // test join
            EPStatement stmtJoin = _epService.EPAdministrator.CreateEPL("select * from SupportBean_S0 s0 unidirectional, MyWindow sb where TheString = MYCONST");
            stmtJoin.Events += _listener.Update;

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(i, "E" + i));
                        EPAssertionUtil.AssertProps(
                            _listener.AssertOneGetNewAndReset(), "sb.TheString,sb.IntPrimitive".Split(','), new Object[]
                            {
                                "E331",
                                -331
                            });
                    }
                });

            Assert.That(delta, Is.LessThan(500), "delta=" + delta);
            stmtJoin.Dispose();
    
            // test subquery
            EPStatement stmtSubquery = _epService.EPAdministrator.CreateEPL("select * from SupportBean_S0 where exists (select * from MyWindow where TheString = MYCONST)");
            stmtSubquery.Events += _listener.Update;

            delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(i, "E" + i));
                        Assert.IsTrue(_listener.GetAndClearIsInvoked());
                    }
                });

            Assert.That(delta, Is.LessThan(500), "delta=" + delta);
        }
    }
}
