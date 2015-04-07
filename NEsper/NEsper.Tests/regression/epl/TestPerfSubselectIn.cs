///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfSubselectIn 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportBean));
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
            config.AddEventType("S3", typeof(SupportBean_S3));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestPerformanceInKeywordAsPartOfSubquery()
        {
            String eplSingleIndex = "select (select p00 from S0.win:keepall() as s0 where s0.p01 in (s1.p10, s1.p11)) as c0 from S1 as s1";
            EPStatement stmtSingleIdx = _epService.EPAdministrator.CreateEPL(eplSingleIndex);
            stmtSingleIdx.Events += _listener.Update;

            RunAssertionPerformanceInKeywordAsPartOfSubquery();
            stmtSingleIdx.Dispose();

            String eplMultiIdx = "select (select p00 from S0.win:keepall() as s0 where s1.p11 in (s0.p00, s0.p01)) as c0 from S1 as s1";
            EPStatement stmtMultiIdx = _epService.EPAdministrator.CreateEPL(eplMultiIdx);
            stmtMultiIdx.Events += _listener.Update;

            RunAssertionPerformanceInKeywordAsPartOfSubquery();
        }

        private void RunAssertionPerformanceInKeywordAsPartOfSubquery()
        {
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(i, "v" + i, "p00_" + i));
            }

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 2000; i++)
                    {
                        int index = 5000 + i % 1000;
                        _epService.EPRuntime.SendEvent(new SupportBean_S1(index, "x", "p00_" + index));
                        Assert.AreEqual("v" + index, _listener.AssertOneGetNewAndReset().Get("c0"));
                    }
                });

            Assert.That(delta, Is.LessThan(500));
        }

        [Test]
        public void TestPerformanceWhereClauseCoercion()
        {
            String stmtText = "select IntPrimitive from MyEvent(TheString='A') as s0 where IntPrimitive in (" +
                                "select LongBoxed from MyEvent(TheString='B').win:length(10000) where s0.IntPrimitive = LongBoxed)";
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++)
            {
                SupportBean bean = new SupportBean();
                bean.TheString = "B";
                bean.LongBoxed = (long)i;
                _epService.EPRuntime.SendEvent(bean);
            }

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        int index = 5000 + i%1000;
                        SupportBean bean = new SupportBean();
                        bean.TheString = "A";
                        bean.IntPrimitive = index;
                        _epService.EPRuntime.SendEvent(bean);
                        Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
                    }
                });

            Assert.That(delta, Is.LessThan(2000));
        }
    
        [Test]
        public void TestPerformanceWhereClause()
        {
            String stmtText = "select id from S0 as s0 where p00 in (" +
                                "select p10 from S1.win:length(10000) where s0.P00 = p10)";
            TryPerformanceOneCriteria(stmtText);
        }
    
        private void TryPerformanceOneCriteria(String stmtText)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // preload with 10k events
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean_S1(i, Convert.ToString(i)));
            }

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        int index = 5000 + i%1000;
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(index, Convert.ToString(index)));
                        Assert.AreEqual(index, _listener.AssertOneGetNewAndReset().Get("id"));
                    }
                });

            Assert.That(delta, Is.LessThan(1000));
        }
    }
}
