///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOutputLimitRowForAll 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private const String CATEGORY = "Fully-Aggregated and Un-grouped";

        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MarketData", typeof(SupportMarketDataBean));
            config.AddEventType("SupportBean", typeof(SupportBean));
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
        public void Test1NoneNoHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec)";
            RunAssertion12(stmtText, "none");
        }
    
        [Test]
        public void Test2NoneNoHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol";
            RunAssertion12(stmtText, "none");
        }
    
        [Test]
        public void Test3NoneHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                " having sum(Price) > 100";
            RunAssertion34(stmtText, "none");
        }
    
        [Test]
        public void Test4NoneHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol " +
                                " having sum(Price) > 100";
            RunAssertion34(stmtText, "none");
        }
    
        [Test]
        public void Test5DefaultNoHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output every 1 seconds";
            RunAssertion56(stmtText, "default");
        }
    
        [Test]
        public void Test6DefaultNoHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol " +
                                "output every 1 seconds";
            RunAssertion56(stmtText, "default");
        }
    
        [Test]
        public void Test7DefaultHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) \n" +
                                "having sum(Price) > 100" +
                                "output every 1 seconds";
            RunAssertion78(stmtText, "default");
        }
    
        [Test]
        public void Test8DefaultHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol " +
                                "having sum(Price) > 100" +
                                "output every 1 seconds";
            RunAssertion78(stmtText, "default");
        }
    
        [Test]
        public void Test9AllNoHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test9AllNoHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec) " +
                    "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test10AllNoHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol " +
                                "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test10AllNoHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }
    
        [Test]
        public void Test11AllHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "having sum(Price) > 100" +
                                "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test11AllHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec) " +
                    "having sum(Price) > 100" +
                    "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }
    
        [Test]
        public void Test12AllHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol " +
                                "having sum(Price) > 100" +
                                "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test12AllHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "having sum(Price) > 100" +
                    "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }
    
        [Test]
        public void Test13LastNoHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec)" +
                                "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test13LastNoHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec)" +
                    "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }
    
        [Test]
        public void Test14LastNoHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test14LastNoHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }
    
        [Test]
        public void Test15LastHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec)" +
                                "having sum(Price) > 100 " +
                                "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test15LastHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec)" +
                    "having sum(Price) > 100 " +
                    "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }
    
        [Test]
        public void Test16LastHavingJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=symbol " +
                                "having sum(Price) > 100 " +
                                "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test16LastHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "having sum(Price) > 100 " +
                    "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }
    
        [Test]
        public void Test17FirstNoHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output first every 1 seconds";
            RunAssertion17(stmtText, "first");
        }
    
        [Test]
        public void Test18SnapshotNoHavingNoJoin()
        {
            var stmtText = "select sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output snapshot every 1 seconds";
            RunAssertion18(stmtText, "first");
        }

        [Test]
        public void TestOutputLastWithInsertInto()
        {
            //RunAssertionOuputLastWithInsertInto(false);
            RunAssertionOuputLastWithInsertInto(true);
        }

        private void RunAssertionOuputLastWithInsertInto(bool hinted)
        {
            var hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";
            var eplInsert = (
                hint +
                "insert into MyStream select sum(IntPrimitive) as thesum from SupportBean.win:keepall() " +
                "output last every 2 events"
                );
            var stmtInsert = _epService.EPAdministrator.CreateEPL(eplInsert);

            var stmtListen = _epService.EPAdministrator.CreateEPL("select * from MyStream");
            stmtListen.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "thesum".Split(','), new Object[] { 30 });

            stmtInsert.Dispose();
            stmtListen.Dispose();
        }
    
        private void RunAssertion12(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(200, 1, new Object[][] { new Object[] {25d}}, new Object[][] { new Object[] {null}});
            expected.AddResultInsRem(800, 1, new Object[][] { new Object[] {34d}}, new Object[][] { new Object[] {25d}});
            expected.AddResultInsRem(1500, 1, new Object[][] { new Object[] {58d}}, new Object[][] { new Object[] {34d}});
            expected.AddResultInsRem(1500, 2, new Object[][] { new Object[] {59d}}, new Object[][] { new Object[] {58d}});
            expected.AddResultInsRem(2100, 1, new Object[][] { new Object[] {85d}}, new Object[][] { new Object[] {59d}});
            expected.AddResultInsRem(3500, 1, new Object[][] { new Object[] {87d}}, new Object[][] { new Object[] {85d}});
            expected.AddResultInsRem(4300, 1, new Object[][] { new Object[] {109d}}, new Object[][] { new Object[] {87d}});
            expected.AddResultInsRem(4900, 1, new Object[][] { new Object[] {112d}}, new Object[][] { new Object[] {109d}});
            expected.AddResultInsRem(5700, 0, new Object[][] { new Object[] {87d}}, new Object[][] { new Object[] {112d}});
            expected.AddResultInsRem(5900, 1, new Object[][] { new Object[] {88d}}, new Object[][] { new Object[] {87d}});
            expected.AddResultInsRem(6300, 0, new Object[][] { new Object[] {79d}}, new Object[][] { new Object[] {88d}});
            expected.AddResultInsRem(7000, 0, new Object[][] { new Object[] {54d}}, new Object[][] { new Object[] {79d}});
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion34(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(4300, 1, new Object[][] { new Object[] {109d}}, null);
            expected.AddResultInsRem(4900, 1, new Object[][] { new Object[] {112d}}, new Object[][] { new Object[] {109d}});
            expected.AddResultInsRem(5700, 0, null, new Object[][] { new Object[] {112d}});
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion13_14(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new Object[][] { new Object[] {34d}}, new Object[][] { new Object[] {null}});
            expected.AddResultInsRem(2200, 0, new Object[][] { new Object[] {85d}}, new Object[][] { new Object[] {34d}});
            expected.AddResultInsRem(3200, 0, new Object[][] { new Object[] {85d}}, new Object[][] { new Object[] {85d}});
            expected.AddResultInsRem(4200, 0, new Object[][] { new Object[] {87d}}, new Object[][] { new Object[] {85d}});
            expected.AddResultInsRem(5200, 0, new Object[][] { new Object[] {112d}}, new Object[][] { new Object[] {87d}});
            expected.AddResultInsRem(6200, 0, new Object[][] { new Object[] {88d}}, new Object[][] { new Object[] {112d}});
            expected.AddResultInsRem(7200, 0, new Object[][] { new Object[] {54d}}, new Object[][] { new Object[] {88d}});
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion15_16(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new Object[][] { new Object[] {112d}}, new Object[][] { new Object[] {109d}});
            expected.AddResultInsRem(6200, 0, null, new Object[][] { new Object[] {112d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion78(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new Object[][] { new Object[] { 109d }, new Object[] { 112d } }, new Object[][] { new Object[] { 109d } });
            expected.AddResultInsRem(6200, 0, null, new Object[][] { new Object[] {112d}});
            expected.AddResultInsRem(7200, 0, null, null);
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion56(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new Object[][] { new Object[] { 25d }, new Object[] { 34d } }, new Object[][] { new Object[] { null }, new Object[] { 25d } });
            expected.AddResultInsRem(2200, 0, new Object[][] { new Object[] { 58d }, new Object[] { 59d }, new Object[] { 85d } }, new Object[][] { new Object[] { 34d }, new Object[] { 58d }, new Object[] { 59d } });
            expected.AddResultInsRem(3200, 0, new Object[][] { new Object[] {85d}}, new Object[][] { new Object[] {85d}});
            expected.AddResultInsRem(4200, 0, new Object[][] { new Object[] {87d}}, new Object[][] { new Object[] {85d}});
            expected.AddResultInsRem(5200, 0, new Object[][] { new Object[] { 109d }, new Object[] { 112d } }, new Object[][] { new Object[] { 87d }, new Object[] { 109d } });
            expected.AddResultInsRem(6200, 0, new Object[][] { new Object[] { 87d }, new Object[] { 88d } }, new Object[][] { new Object[] { 112d }, new Object[] { 87d } });
            expected.AddResultInsRem(7200, 0, new Object[][] { new Object[] { 79d }, new Object[] { 54d } }, new Object[][] { new Object[] { 88d }, new Object[] { 79d } });
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion17(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(200, 1, new Object[][] { new Object[] {25d}}, new Object[][] { new Object[] {null}});
            expected.AddResultInsRem(1500, 1, new Object[][] { new Object[] {58d}}, new Object[][] { new Object[] {34d}});
            expected.AddResultInsRem(3500, 1, new Object[][] { new Object[] {87d}}, new Object[][] { new Object[] {85d}});
            expected.AddResultInsRem(4300, 1, new Object[][] { new Object[] {109d}}, new Object[][] { new Object[] {87d}});
            expected.AddResultInsRem(5700, 0, new Object[][] { new Object[] {87d}}, new Object[][] { new Object[] {112d}});
            expected.AddResultInsRem(6300, 0, new Object[][] { new Object[] {79d}}, new Object[][] { new Object[] {88d}});
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        private void RunAssertion18(String stmtText, String outputLimit)
        {
            SendTimer(0);
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            var fields = new String[] {"sum(Price)"};
            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, new Object[][] { new Object[] {34d}}, null);
            expected.AddResultInsRem(2200, 0, new Object[][] { new Object[] {85d}}, null);
            expected.AddResultInsRem(3200, 0, new Object[][] { new Object[] {85d}}, null);
            expected.AddResultInsRem(4200, 0, new Object[][] { new Object[] {87d}}, null);
            expected.AddResultInsRem(5200, 0, new Object[][] { new Object[] {112d}}, null);
            expected.AddResultInsRem(6200, 0, new Object[][] { new Object[] {88d}}, null);
            expected.AddResultInsRem(7200, 0, new Object[][] { new Object[] {54d}}, null);
    
            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }
    
        [Test]
        public void TestAggAllHaving()
        {
            var stmtText = "select sum(Volume) as result " +
                                "from " + typeof(SupportMarketDataBean).FullName + ".win:length(10) as two " +
                                "having sum(Volume) > 0 " +
                                "output every 5 events";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            var fields = new String[] {"result"};
    
            SendMDEvent(20);
            SendMDEvent(-100);
            SendMDEvent(0);
            SendMDEvent(0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendMDEvent(0);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {20L}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {20L}});
            _listener.Reset();
        }
    
        [Test]
        public void TestAggAllHavingJoin()
        {
            var stmtText = "select sum(Volume) as result " +
                                "from " + typeof(SupportMarketDataBean).FullName + ".win:length(10) as one," +
                                typeof(SupportBean).FullName + ".win:length(10) as two " +
                                "where one.Symbol=two.TheString " +
                                "having sum(Volume) > 0 " +
                                "output every 5 events";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            var fields = new String[] {"result"};
            _epService.EPRuntime.SendEvent(new SupportBean("S0", 0));
    
            SendMDEvent(20);
            SendMDEvent(-100);
            SendMDEvent(0);
            SendMDEvent(0);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendMDEvent(0);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {20L}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] {20L}});
            _listener.Reset();
        }
    
        [Test]
        public void TestJoinSortWindow()
        {
            SendTimer(0);
    
            var viewExpr = "select irstream max(Price) as maxVol" +
                              " from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(1,Volume desc) as s0, " +
                              typeof(SupportBean).FullName + ".win:keepall() as s1 where s1.TheString=s0.Symbol " +
                              "output every 1.0d seconds";
            var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));
    
            SendEvent("JOIN_KEY", 1d);
            SendEvent("JOIN_KEY", 2d);
            _listener.Reset();
    
            // moves all events out of the window,
            SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            var result = _listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
            Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
            Assert.AreEqual(2, result.Second.Length);
            Assert.AreEqual(null, result.Second[0].Get("maxVol"));
            Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));
            
            // statement object model test
            var model = _epService.EPAdministrator.CompileEPL(viewExpr);
            SerializableObjectCopier.Copy(model);
            Assert.AreEqual(viewExpr, model.ToEPL());
        }
    
        [Test]
        public void TestMaxTimeWindow()
        {
            SendTimer(0);
    
            var viewExpr = "select irstream max(Price) as maxVol" +
                              " from " + typeof(SupportMarketDataBean).FullName + ".win:time(1.1 sec) " +
                              "output every 1 seconds";
            var stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;
    
            SendEvent("SYM1", 1d);
            SendEvent("SYM1", 2d);
            _listener.Reset();
    
            // moves all events out of the window,
            SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            var result = _listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
            Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
            Assert.AreEqual(2, result.Second.Length);
            Assert.AreEqual(null, result.Second[0].Get("maxVol"));
            Assert.AreEqual(1.0, result.Second[1].Get("maxVol"));
        }
    
        [Test]
        public void TestTimeWindowOutputCountLast()
        {
            var stmtText = "select count(*) as cnt from " + typeof(SupportBean).FullName + ".win:time(10 seconds) output every 10 seconds";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(0);
            SendTimer(10000);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(20000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent("e1");
            SendTimer(30000);
            var newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual(1L, newEvents[0].Get("cnt"));
            Assert.AreEqual(0L, newEvents[1].Get("cnt"));
    
            SendTimer(31000);
    
            SendEvent("e2");
            SendEvent("e3");
            SendTimer(40000);
            newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual(1L, newEvents[0].Get("cnt"));
            Assert.AreEqual(2L, newEvents[1].Get("cnt"));
        }
    
        [Test]
        public void TestTimeBatchOutputCount()
        {
            var stmtText = "select count(*) as cnt from " + typeof(SupportBean).FullName + ".win:time_batch(10 seconds) output every 10 seconds";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(0);
            SendTimer(10000);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(20000);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent("e1");
            SendTimer(30000);
            Assert.IsFalse(listener.IsInvoked);
            SendTimer(40000);
            var newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            // output limiting starts 10 seconds after, therefore the old batch was posted already and the cnt is zero
            Assert.AreEqual(1L, newEvents[0].Get("cnt"));
            Assert.AreEqual(0L, newEvents[1].Get("cnt"));
    
            SendTimer(50000);
            var newData = listener.LastNewData;
            Assert.AreEqual(0L, newData[0].Get("cnt"));
            listener.Reset();
    
            SendEvent("e2");
            SendEvent("e3");
            SendTimer(60000);
            newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(1, newEvents.Length);
            Assert.AreEqual(2L, newEvents[0].Get("cnt"));
        }
    
        [Test]
        public void TestLimitSnapshot()
        {
            var listener = new SupportUpdateListener();
    
            SendTimer(0);
            var selectStmt = "select count(*) as cnt from " + typeof(SupportBean).FullName + ".win:time(10 seconds) where IntPrimitive > 0 output snapshot every 1 seconds";
    
            var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += listener.Update;
            SendEvent("s0", 1);
    
            SendTimer(500);
            SendEvent("s1", 1);
            SendEvent("s2", -1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(1000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {2L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(1500);
            SendEvent("s4", 2);
            SendEvent("s5", 3);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(2000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {4L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendEvent("s5", 4);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(9000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {5L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {4L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(10999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {3L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
        }
    
        [Test]
        public void TestLimitSnapshotJoin()
        {
            var listener = new SupportUpdateListener();
    
            SendTimer(0);
            var selectStmt = "select count(*) as cnt from " +
                    typeof(SupportBean).FullName + ".win:time(10 seconds) as s, " +
                    typeof(SupportMarketDataBean).FullName + ".win:keepall() as m where m.Symbol = s.TheString and IntPrimitive > 0 output snapshot every 1 seconds";
    
            var stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("s0", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("s1", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("s2", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("s4", 0, 0L, ""));
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("s5", 0, 0L, ""));
    
            SendEvent("s0", 1);
    
            SendTimer(500);
            SendEvent("s1", 1);
            SendEvent("s2", -1);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(1000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {2L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(1500);
            SendEvent("s4", 2);
            SendEvent("s5", 3);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(2000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {4L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendEvent("s5", 4);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(9000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {5L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            // The execution of the join is after the snapshot, as joins are internal dispatch
            SendTimer(10000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {5L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
    
            SendTimer(10999);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new String[] {"cnt"}, new Object[][] { new Object[] {3L}});
            Assert.IsNull(listener.LastOldData);
            listener.Reset();
        }

        [Test]
        public void TestOutputSnapshotGetValue()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // disabled for this test

            _epService.EPAdministrator.Configuration.AddPlugInAggregationFunctionFactory("customagg", typeof(MyContextAggFuncFactory));
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();

            RunAssertionOutputSnapshotGetValue(true);
            RunAssertionOutputSnapshotGetValue(false);
        }

        private void RunAssertionOutputSnapshotGetValue(bool join)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "select customagg(IntPrimitive) as c0 from SupportBean" +
                    (join ? ".win:keepall(), SupportBean_S0.std:lastevent()" : "") +
                    " output snapshot every 3 events");
            stmt.AddListener(_listener);
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));

            MyContextAggFunc.ResetGetValueInvocationCount();

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.AreEqual(0, MyContextAggFunc.GetValueInvocationCount);

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 30));
            Assert.AreEqual(60, _listener.AssertOneGetNewAndReset().Get("c0"));
            Assert.AreEqual(1, MyContextAggFunc.GetValueInvocationCount);

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 40));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 50));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 60));
            Assert.AreEqual(210, _listener.AssertOneGetNewAndReset().Get("c0"));
            Assert.AreEqual(2, MyContextAggFunc.GetValueInvocationCount);

            stmt.Dispose();
        }
    
        private void SendEvent(String s)
    	{
    	    var bean = new SupportBean();
    	    bean.TheString = s;
    	    bean.DoubleBoxed = 0.0;
    	    bean.IntPrimitive = 0;
    	    bean.IntBoxed = 0;
    	    _epService.EPRuntime.SendEvent(bean);
    	}
        
        private void SendEvent(String s, int intPrimitive)
    	{
    	    var bean = new SupportBean();
    	    bean.TheString = s;
    	    bean.IntPrimitive = intPrimitive;
    	    _epService.EPRuntime.SendEvent(bean);
    	}
    
        private void SendTimer(long time)
        {
            var theEvent = new CurrentTimeEvent(time);
            var runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent(String symbol, double price)
    	{
    	    var bean = new SupportMarketDataBean(symbol, price, 0L, null);
    	    _epService.EPRuntime.SendEvent(bean);
    	}
    
        private void SendMDEvent(long volume)
    	{
    	    var bean = new SupportMarketDataBean("S0", 0, volume, null);
    	    _epService.EPRuntime.SendEvent(bean);
    	}

        public class MyContextAggFuncFactory : AggregationFunctionFactory
        {
            public string FunctionName { get; set; }

            public void Validate(AggregationValidationContext validationContext)
            {
            }

            public AggregationMethod NewAggregator()
            {
                return new MyContextAggFunc();
            }

            public Type ValueType {
                get { return typeof (int); }
            }
        }

        public class MyContextAggFunc : AggregationMethod
        {
            private static long _getValueInvocationCount = 0;

            public static long GetValueInvocationCount
            {
                get { return _getValueInvocationCount; }
            }

            public static void ResetGetValueInvocationCount()
            {
                _getValueInvocationCount = 0;
            }

            private int _sum;

            public void Enter(Object value)
            {
                _sum += value.AsInt();
            }

            public void Leave(Object value)
            {
            }

            public object Value
            {
                get
                {
                    _getValueInvocationCount++;
                    return _sum;
                }
            }

            public Type ValueType
            {
                get { return typeof (int); }
            }

            public void Clear()
            {
            }
        }
    }
}
