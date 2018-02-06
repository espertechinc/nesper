///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestPerfNamedWindowSubquery
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }

        #endregion

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void RunConstantValueAssertion(bool indexShare, bool buildIndex)
        {
            String createEpl = "create window MyWindow#keepall as select * from SupportBean";
            if (indexShare)
            {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            _epService.EPAdministrator.CreateEPL(createEpl);

            if (buildIndex)
            {
                _epService.EPAdministrator.CreateEPL("create index idx1 on MyWindow(TheString hash, IntPrimitive btree)");
            }
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

            // preload
            for (int i = 0; i < 10000; i++)
            {
                var bean = new SupportBean("E" + i, i);
                bean.DoublePrimitive = i;
                _epService.EPRuntime.SendEvent(bean);
            }

            // single-field compare
            String[] fields = "val".Split(',');
            String eplSingle =
                "select (select IntPrimitive from MyWindow where TheString = 'E9734') as val from SupportBeanRange sbr";
            EPStatement stmtSingle = _epService.EPAdministrator.CreateEPL(eplSingle);
            stmtSingle.Events += _listener.Update;

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "", -1, -1));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {9734});
            }
            long delta = PerformanceObserver.MilliTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            stmtSingle.Dispose();

            // two-field compare
            String eplTwoHash =
                "select (select IntPrimitive from MyWindow where TheString = 'E9736' and IntPrimitive = 9736) as val from SupportBeanRange sbr";
            EPStatement stmtTwoHash = _epService.EPAdministrator.CreateEPL(eplTwoHash);
            stmtTwoHash.Events += _listener.Update;

            startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "", -1, -1));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {9736});
            }
            delta = PerformanceObserver.MilliTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            stmtTwoHash.Dispose();

            // range compare single
            if (buildIndex)
            {
                _epService.EPAdministrator.CreateEPL("create index idx2 on MyWindow(IntPrimitive btree)");
            }
            String eplSingleBTree =
                "select (select IntPrimitive from MyWindow where IntPrimitive between 9735 and 9735) as val from SupportBeanRange sbr";
            EPStatement stmtSingleBtree = _epService.EPAdministrator.CreateEPL(eplSingleBTree);
            stmtSingleBtree.Events += _listener.Update;

            startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "", -1, -1));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {9735});
            }
            delta = PerformanceObserver.MilliTime - startTime;
            Assert.That(delta, Is.LessThan(500));
            stmtSingleBtree.Dispose();

            // range compare composite
            String eplComposite =
                "select (select IntPrimitive from MyWindow where TheString = 'E9738' and IntPrimitive between 9738 and 9738) as val from SupportBeanRange sbr";
            EPStatement stmtComposite = _epService.EPAdministrator.CreateEPL(eplComposite);
            stmtComposite.Events += _listener.Update;

            startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R", "", -1, -1));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {9738});
            }
            delta = PerformanceObserver.MilliTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            stmtComposite.Dispose();

            // destroy all
            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunKeyAndRangeAssertion(bool indexShare, bool buildIndex)
        {
            string createEpl = "create window MyWindow#keepall as select * from SupportBean";
            if (indexShare)
            {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }

            _epService.EPAdministrator.CreateEPL(createEpl);

            if (buildIndex)
            {
                _epService.EPAdministrator.CreateEPL("create index idx1 on MyWindow(TheString hash, IntPrimitive btree)");
            }

            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean(i < 5000 ? "A" : "B", i));
            }

            string[] fields = "cols.mini,cols.maxi".Split(',');
            string queryEpl =
                "select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow " +
                "where TheString = sbr.key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(queryEpl);
            stmt.Events += _listener.Update;

            long delta = PerformanceObserver.TimeMillis(
                delegate
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBeanRange("R1", "A", 300, 312));
                        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {300, 312});
                    }
                });

            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunRangeAssertion(bool indexShare, bool buildIndex)
        {
            String createEpl = "create window MyWindow#keepall as select * from SupportBean";
            if (indexShare)
            {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            _epService.EPAdministrator.CreateEPL(createEpl);

            if (buildIndex)
            {
                _epService.EPAdministrator.CreateEPL("create index idx1 on MyWindow(IntPrimitive btree)");
            }
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", i));
            }

            String[] fields = "cols.mini,cols.maxi".Split(',');
            String queryEpl =
                "select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow where IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(queryEpl);
            stmt.Events += _listener.Update;

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 1000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R1", "K", 300, 312));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {300, 312});
            }
            long delta = PerformanceObserver.MilliTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertion(bool enableIndexShareCreate, bool disableIndexShareConsumer, bool createExplicitIndex)
        {
            _epService.EPAdministrator.CreateEPL("create schema EventSchema(e0 string, e1 int, e2 string)");

            String createEpl = "create window MyWindow#keepall as select * from SupportBean";
            if (enableIndexShareCreate)
            {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
            _epService.EPAdministrator.CreateEPL(createEpl);
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

            if (createExplicitIndex)
            {
                _epService.EPAdministrator.CreateEPL("create index MyIndex on MyWindow (TheString)");
            }

            String consumeEpl =
                "select e0, (select TheString from MyWindow where IntPrimitive = es.e1 and TheString = es.e2) as val from EventSchema as es";
            if (disableIndexShareConsumer)
            {
                consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
            }
            EPStatement consumeStmt = _epService.EPAdministrator.CreateEPL(consumeEpl);
            consumeStmt.Events += _listener.Update;

            String[] fields = "e0,val".Split(',');

            // test once
            _epService.EPRuntime.SendEvent(new SupportBean("WX", 10));
            SendEvent("E1", 10, "WX");
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", "WX"});

            // preload
            for (int i = 0; i < 10000; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("W" + i, i));
            }

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 5000; i++)
            {
                SendEvent("E" + i, i, "W" + i);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E" + i, "W" + i});
            }
            long endTime = PerformanceObserver.MilliTime;
            long delta = endTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void SendEvent(String e0, int e1, String e2)
        {
            var theEvent = new LinkedHashMap<String, Object>();

            theEvent["e0"] = e0;
            theEvent["e1"] = e1;
            theEvent["e2"] = e2;

            if (EventRepresentationChoiceExtensions.GetEngineDefault(_epService).IsObjectArrayEvent())
            {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "EventSchema");
            }
            else
            {
                _epService.EPRuntime.SendEvent(theEvent, "EventSchema");
            }
        }

        [Test]
        public void TestConstantValue()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof (SupportBeanRange));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof (SupportBean));

            RunConstantValueAssertion(false, false);
            RunConstantValueAssertion(true, false);
            RunConstantValueAssertion(true, true);
        }

        [Test]
        public void TestDisableShare()
        {
            RunAssertion(true, true, false);
        }

        [Test]
        public void TestDisableShareCreate()
        {
            RunAssertion(true, true, true);
        }

        [Test]
        public void TestKeyAndRange()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanRange>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            RunKeyAndRangeAssertion(false, false);
            RunKeyAndRangeAssertion(true, false);
            RunKeyAndRangeAssertion(true, true);
        }

        [Test]
        public void TestKeyedRange()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof (SupportBeanRange));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof (SupportBean));

            String createEpl = "create window MyWindow#keepall as select * from SupportBean";
            _epService.EPAdministrator.CreateEPL(createEpl);
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");

            // preload
            for (int i = 0; i < 10000; i++)
            {
                String key = i < 5000 ? "A" : "B";
                _epService.EPRuntime.SendEvent(new SupportBean(key, i));
            }

            String[] fields = "cols.mini,cols.maxi".Split(',');
            String queryEpl = "select (select min(IntPrimitive) as mini, max(IntPrimitive) as maxi from MyWindow " +
                              "where TheString = sbr.key and IntPrimitive between sbr.rangeStart and sbr.rangeEnd) as cols from SupportBeanRange sbr";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(queryEpl);
            stmt.Events += _listener.Update;

            long startTime = PerformanceObserver.MilliTime;
            for (int i = 0; i < 500; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBeanRange("R1", "A", 299, 313));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {299, 313});

                _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "B", 7500, 7510));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {7500, 7510});
            }
            long delta = PerformanceObserver.MilliTime - startTime;
            Assert.IsTrue(delta < 500, "delta=" + delta);
            Log.Info("delta=" + delta);
        }

        [Test]
        public void TestNoShare()
        {
            RunAssertion(false, false, false);
        }

        [Test]
        public void TestRange()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof (SupportBeanRange));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof (SupportBean));

            RunRangeAssertion(false, false);
            RunRangeAssertion(true, false);
            RunRangeAssertion(true, true);
        }

        [Test]
        public void TestShare()
        {
            RunAssertion(true, false, false);
        }

        [Test]
        public void TestShareCreate()
        {
            RunAssertion(true, false, true);
        }
    }
}