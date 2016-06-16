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
using com.espertech.esper.core.service;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfNamedWindow
    {
        private EPServiceProviderSPI _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        [Test]
        public void TestOnSelectInKeywordPerformance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S1>();

            // create window
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");

            int maxRows = 10000;   // for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(i, "p00_" + i));
            }

            String eplSingleIdx = "on SupportBean_S1 select sum(mw.id) as sumi from MyWindow mw where p00 in (p10, p11)";
            RunOnDemandAssertion(eplSingleIdx, 1, new SupportBean_S1(0, "x", "p00_6523"), 6523);

            String eplMultiIndex = "on SupportBean_S1 select sum(mw.id) as sumi from MyWindow mw where p10 in (p00, p01)";
            RunOnDemandAssertion(eplMultiIndex, 2, new SupportBean_S1(0, "p00_6524"), 6524);
        }

        [Test]
        public void TestOnSelectEqualsAndRangePerformance()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            // create window one
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            // insert X rows
            int maxRows = 10000;   //for performance testing change to int maxRows = 100000;
            for (int i = 0; i < maxRows; i++) {
                SupportBean bean = new SupportBean((i < 5000) ? "A" : "B", i);
                bean.LongPrimitive = i;
                bean.LongBoxed = ((long) i + 1);
                _epService.EPRuntime.SendEvent(bean);
            }
            _epService.EPRuntime.SendEvent(new SupportBean("B", 100));
    
            String eplIdx1One = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow where IntPrimitive = sbr.rangeStart";
            RunOnDemandAssertion(eplIdx1One, 1, new SupportBeanRange("R", 5501, 0), 5501);
    
            String eplIdx1Two = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow where IntPrimitive between sbr.rangeStart and sbr.rangeEnd";
            RunOnDemandAssertion(eplIdx1Two, 1, new SupportBeanRange("R", 5501, 5503), 5501 + 5502 + 5503);
    
            String eplIdx1Three = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow where TheString = key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd";
            RunOnDemandAssertion(eplIdx1Three, 1, new SupportBeanRange("R", "A", 4998, 5503), 4998 + 4999);
    
            String eplIdx1Four = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow " +
                    "where TheString = key and LongPrimitive = rangeStart and IntPrimitive between rangeStart and rangeEnd " +
                    "and LongBoxed between rangeStart and rangeEnd";
            RunOnDemandAssertion(eplIdx1Four, 1, new SupportBeanRange("R", "A", 4998, 5503), 4998);
    
            String eplIdx1Five = "on SupportBeanRange sbr select sum(IntPrimitive) as sumi from MyWindow " +
                    "where IntPrimitive between rangeStart and rangeEnd " +
                    "and LongBoxed between rangeStart and rangeEnd";
            RunOnDemandAssertion(eplIdx1Five, 1, new SupportBeanRange("R", "A", 4998, 5001), 4998 + 4999 + 5000);
        }

        private void RunOnDemandAssertion(String epl, int numIndexes, Object theEvent, int expected)
        {
            Assert.AreEqual(0, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;
            Assert.AreEqual(numIndexes, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
    
            long start = Environment.TickCount;
            int loops = 1000;
    
            for (int i = 0; i < loops; i++) {
                _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(expected, _listener.AssertOneGetNewAndReset().Get("sumi"));
            }
            long end = Environment.TickCount;
            long delta = end - start;
            Assert.IsTrue(delta < 1000,"delta=" + delta);
    
            stmt.Dispose();
            Assert.AreEqual(0, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyWindow").Length);
        }
    
        [Test]
        public void TestDeletePerformance() {
            // create window
            String stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt
            String stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow where id = a";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            String stmtTextInsertOne = "insert into MyWindow select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            for (int i = 0; i < 50000; i++) {
                SendSupportBean("S" + i, i);
            }
    
            // delete rows
            stmtCreate.Events += _listener.Update;
            long startTime = Environment.TickCount;
            for (int i = 0; i < 10000; i++) {
                SendSupportBean_A("S" + i);
            }
            long endTime = Environment.TickCount;
            long delta = endTime - startTime;
            Assert.IsTrue( delta < 500,"Delta=" + delta);
    
            // assert they are deleted
            Assert.AreEqual(50000 - 10000, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
            Assert.AreEqual(10000, _listener.OldDataList.Count);
        }
    
        [Test]
        public void TestDeletePerformanceCoercion() {
            // create window
            String stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt
            String stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where b = Price";
            _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            String stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            for (int i = 0; i < 50000; i++) {
                SendSupportBean("S" + i, (long) i);
            }
    
            // delete rows
            stmtCreate.Events += _listener.Update;
            long startTime = Environment.TickCount;
            for (int i = 0; i < 10000; i++) {
                SendMarketBean("S" + i, i);
            }
            long endTime = Environment.TickCount;
            long delta = endTime - startTime;
            Assert.IsTrue( delta < 500,"Delta=" + delta);
    
            // assert they are deleted
            Assert.AreEqual(50000 - 10000, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
            Assert.AreEqual(10000, _listener.OldDataList.Count);
        }
    
        [Test]
        public void TestDeletePerformanceTwoDeleters() {
            // create window
            String stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt one
            String stmtTextDeleteOne = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where b = Price";
            _epService.EPAdministrator.CreateEPL(stmtTextDeleteOne);
    
            // create delete stmt two
            String stmtTextDeleteTwo = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow where id = a";
            _epService.EPAdministrator.CreateEPL(stmtTextDeleteTwo);
    
            // create insert into
            String stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            for (int i = 0; i < 20000; i++) {
                SendSupportBean("S" + i, (long) i);
            }
    
            // delete all rows
            stmtCreate.Events += _listener.Update;
            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        SendMarketBean("S" + i, i);
                        SendSupportBean_A("S" + (i + 10000));
                    }
                });

            Assert.That(delta, Is.LessThan(1500));
    
            // assert they are all deleted
            Assert.AreEqual(0, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
            Assert.AreEqual(20000, _listener.OldDataList.Count);
        }
    
        [Test]
        public void TestDeletePerformanceIndexReuse() {
            // create window
            String stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create delete stmt
            EPStatement[] statements = new EPStatement[50];
            for (int i = 0; i < statements.Length; i++) {
                String stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where b = Price";
                statements[i] = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
            }
    
            // create insert into
            String stmtTextInsertOne = "insert into MyWindow select TheString as a, LongPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // load window
            long startTime = Environment.TickCount;
            for (int i = 0; i < 10000; i++) {
                SendSupportBean("S" + i, (long) i);
            }
            long endTime = Environment.TickCount;
            long delta = endTime - startTime;
            Assert.IsTrue( delta < 1000,"Delta=" + delta);
            Assert.AreEqual(10000, EPAssertionUtil.EnumeratorCount(stmtCreate.GetEnumerator()));
    
            // destroy all
            for (int i = 0; i < statements.Length; i++) {
                statements[i].Dispose();
            }
        }
    
        private SupportBean_A SendSupportBean_A(String id) {
            SupportBean_A bean = new SupportBean_A(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportMarketDataBean SendMarketBean(String symbol, double price) {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(String stringValue, long longPrimitive) {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(String stringValue, int intPrimitive) {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
