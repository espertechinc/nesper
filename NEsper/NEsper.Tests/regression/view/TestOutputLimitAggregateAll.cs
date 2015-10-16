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
using com.espertech.esper.client.time;
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOutputLimitAggregateAll
    {
        private static readonly String EVENT_NAME = typeof(SupportMarketDataBean).FullName;

        private const String JOIN_KEY = "KEY";
        private const String CATEGORY = "Aggregated and Un-grouped";

        private SupportUpdateListener _listener;
        private EPServiceProvider _epService;
        private long _currentTime;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MarketData", typeof(SupportMarketDataBean));
            config.AddEventType("SupportBean", typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void Test1NoneNoHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec)";
            RunAssertion12(stmtText, "none");
        }

        [Test]
        public void Test2NoneNoHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol";
            RunAssertion12(stmtText, "none");
        }

        [Test]
        public void Test3NoneHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                " having sum(Price) > 100";
            RunAssertion34(stmtText, "none");
        }

        [Test]
        public void Test4NoneHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                " having sum(Price) > 100";
            RunAssertion34(stmtText, "none");
        }

        [Test]
        public void Test5DefaultNoHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output every 1 seconds";
            RunAssertion56(stmtText, "default");
        }

        [Test]
        public void Test6DefaultNoHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "output every 1 seconds";
            RunAssertion56(stmtText, "default");
        }

        [Test]
        public void Test7DefaultHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) \n" +
                                "having sum(Price) > 100" +
                                "output every 1 seconds";
            RunAssertion78(stmtText, "default");
        }

        [Test]
        public void Test8DefaultHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "having sum(Price) > 100" +
                                "output every 1 seconds";
            RunAssertion78(stmtText, "default");
        }

        [Test]
        public void Test9AllNoHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test9AllNoHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec) " +
                    "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test10AllNoHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test10AllNoHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "output all every 1 seconds";
            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test11AllHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "having sum(Price) > 100" +
                                "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test11AllHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec) " +
                    "having sum(Price) > 100" +
                    "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test12AllHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "having sum(Price) > 100" +
                                "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test12AllHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "having sum(Price) > 100" +
                    "output all every 1 seconds";
            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test13LastNoHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec)" +
                                "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test13LastNoHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec)" +
                    "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test14LastNoHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test14LastNoHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where theString=symbol " +
                    "output last every 1 seconds";
            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test15LastHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec)" +
                                "having sum(Price) > 100 " +
                                "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test15LastHavingNoJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec)" +
                    "having sum(Price) > 100 " +
                    "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test16LastHavingJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec), " +
                                "SupportBean.win:keepall() where TheString=Symbol " +
                                "having sum(Price) > 100 " +
                                "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test16LastHavingJoinHinted()
        {
            String stmtText = "@Hint('enable_outputlimit_opt') select Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec), " +
                    "SupportBean.win:keepall() where TheString=Symbol " +
                    "having sum(Price) > 100 " +
                    "output last every 1 seconds";
            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test17FirstNoHavingNoJoinIStreamOnly()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output first every 1 seconds";
            RunAssertion17IStreamOnly(stmtText, "first");
        }

        [Test]
        public void Test17FirstNoHavingNoJoinIRStream()
        {
            String stmtText = "select irstream Symbol, sum(Price) " +
                    "from MarketData.win:time(5.5 sec) " +
                    "output first every 1 seconds";
            RunAssertion17IRStream(stmtText, "first");
        }

        [Test]
        public void Test18SnapshotNoHavingNoJoin()
        {
            String stmtText = "select Symbol, sum(Price) " +
                                "from MarketData.win:time(5.5 sec) " +
                                "output snapshot every 1 seconds";
            RunAssertion18(stmtText, "first");
        }

        private void RunAssertion12(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new Object[][] { new Object[] { "IBM", 25d } });
            expected.AddResultInsert(800, 1, new Object[][] { new Object[] { "MSFT", 34d } });
            expected.AddResultInsert(1500, 1, new Object[][] { new Object[] { "IBM", 58d } });
            expected.AddResultInsert(1500, 2, new Object[][] { new Object[] { "YAH", 59d } });
            expected.AddResultInsert(2100, 1, new Object[][] { new Object[] { "IBM", 85d } });
            expected.AddResultInsert(3500, 1, new Object[][] { new Object[] { "YAH", 87d } });
            expected.AddResultInsert(4300, 1, new Object[][] { new Object[] { "IBM", 109d } });
            expected.AddResultInsert(4900, 1, new Object[][] { new Object[] { "YAH", 112d } });
            expected.AddResultRemove(5700, 0, new Object[][] { new Object[] { "IBM", 87d } });
            expected.AddResultInsert(5900, 1, new Object[][] { new Object[] { "YAH", 88d } });
            expected.AddResultRemove(6300, 0, new Object[][] { new Object[] { "MSFT", 79d } });
            expected.AddResultRemove(7000, 0, new Object[][] { new Object[] { "IBM", 54d }, new Object[] { "YAH", 54d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        private void RunAssertion34(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(4300, 1, new Object[][] { new Object[] { "IBM", 109d } });
            expected.AddResultInsert(4900, 1, new Object[][] { new Object[] { "YAH", 112d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        private void RunAssertion13_14(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new Object[][] { new Object[] { "MSFT", 34d } });
            expected.AddResultInsert(2200, 0, new Object[][] { new Object[] { "IBM", 85d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new Object[][] { new Object[] { "YAH", 87d } });
            expected.AddResultInsert(5200, 0, new Object[][] { new Object[] { "YAH", 112d } });
            expected.AddResultInsRem(6200, 0, new Object[][] { new Object[] { "YAH", 88d } }, new Object[][] { new Object[] { "IBM", 87d } });
            expected.AddResultRemove(7200, 0, new Object[][] { new Object[] { "YAH", 54d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        private void RunAssertion15_16(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new Object[][] { new Object[] { "YAH", 112d } });
            expected.AddResultInsRem(6200, 0, null, null);
            expected.AddResultInsRem(7200, 0, null, null);

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        private void RunAssertion78(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, null, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new Object[][] { new Object[] { "IBM", 109d }, new Object[] { "YAH", 112d } }, null);
            expected.AddResultInsRem(6200, 0, null, null);
            expected.AddResultInsRem(7200, 0, null, null);

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        private void RunAssertion56(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new Object[][] { new Object[] { "IBM", 25d }, new Object[] { "MSFT", 34d } });
            expected.AddResultInsert(2200, 0, new Object[][] { new Object[] { "IBM", 58d }, new Object[] { "YAH", 59d }, new Object[] { "IBM", 85d } });
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new Object[][] { new Object[] { "YAH", 87d } });
            expected.AddResultInsert(5200, 0, new Object[][] { new Object[] { "IBM", 109d }, new Object[] { "YAH", 112d } });
            expected.AddResultInsRem(6200, 0, new Object[][] { new Object[] { "YAH", 88d } }, new Object[][] { new Object[] { "IBM", 87d } });
            expected.AddResultRemove(7200, 0, new Object[][] { new Object[] { "MSFT", 79d }, new Object[] { "IBM", 54d }, new Object[] { "YAH", 54d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        private void RunAssertion17IStreamOnly(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new Object[][] { new Object[] { "IBM", 25d } });
            expected.AddResultInsert(1500, 1, new Object[][] { new Object[] { "IBM", 58d } });
            expected.AddResultInsert(3500, 1, new Object[][] { new Object[] { "YAH", 87d } });
            expected.AddResultInsert(4300, 1, new Object[][] { new Object[] { "IBM", 109d } });
            expected.AddResultInsert(5900, 1, new Object[][] { new Object[] { "YAH", 88d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute();
        }

        private void RunAssertion17IRStream(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(200, 1, new Object[][] { new Object[] { "IBM", 25d } });
            expected.AddResultInsert(1500, 1, new Object[][] { new Object[] { "IBM", 58d } });
            expected.AddResultInsert(3500, 1, new Object[][] { new Object[] { "YAH", 87d } });
            expected.AddResultInsert(4300, 1, new Object[][] { new Object[] { "IBM", 109d } });
            expected.AddResultRemove(5700, 0, new Object[][] { new Object[] { "IBM", 87d } });
            expected.AddResultRemove(6300, 0, new Object[][] { new Object[] { "MSFT", 79d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected, ResultAssertExecutionTestSelector.TEST_ONLY_AS_PROVIDED);
            execution.Execute();
        }

        private void RunAssertion18(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;

            String[] fields = new String[] { "Symbol", "sum(Price)" };
            ResultAssertTestResult expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);
            expected.AddResultInsert(1200, 0, new Object[][] { new Object[] { "IBM", 34d }, new Object[] { "MSFT", 34d } });
            expected.AddResultInsert(2200, 0, new Object[][] { new Object[] { "IBM", 85d }, new Object[] { "MSFT", 85d }, new Object[] { "IBM", 85d }, new Object[] { "YAH", 85d }, new Object[] { "IBM", 85d } });
            expected.AddResultInsert(3200, 0, new Object[][] { new Object[] { "IBM", 85d }, new Object[] { "MSFT", 85d }, new Object[] { "IBM", 85d }, new Object[] { "YAH", 85d }, new Object[] { "IBM", 85d } });
            expected.AddResultInsert(4200, 0, new Object[][] { new Object[] { "IBM", 87d }, new Object[] { "MSFT", 87d }, new Object[] { "IBM", 87d }, new Object[] { "YAH", 87d }, new Object[] { "IBM", 87d }, new Object[] { "YAH", 87d } });
            expected.AddResultInsert(5200, 0, new Object[][] { new Object[] { "IBM", 112d }, new Object[] { "MSFT", 112d }, new Object[] { "IBM", 112d }, new Object[] { "YAH", 112d }, new Object[] { "IBM", 112d }, new Object[] { "YAH", 112d }, new Object[] { "IBM", 112d }, new Object[] { "YAH", 112d } });
            expected.AddResultInsert(6200, 0, new Object[][] { new Object[] { "MSFT", 88d }, new Object[] { "IBM", 88d }, new Object[] { "YAH", 88d }, new Object[] { "IBM", 88d }, new Object[] { "YAH", 88d }, new Object[] { "IBM", 88d }, new Object[] { "YAH", 88d }, new Object[] { "YAH", 88d } });
            expected.AddResultInsert(7200, 0, new Object[][] { new Object[] { "IBM", 54d }, new Object[] { "YAH", 54d }, new Object[] { "IBM", 54d }, new Object[] { "YAH", 54d }, new Object[] { "YAH", 54d } });

            ResultAssertExecution execution = new ResultAssertExecution(_epService, stmt, _listener, expected);
            execution.Execute();
        }

        [Test]
        public void TestHaving()
        {
            SendTimer(0);

            String viewExpr = "select Symbol, avg(Price) as AvgPrice " +
                              "from " + typeof(SupportMarketDataBean).FullName + ".win:time(3 sec) " +
                              "having avg(Price) > 10" +
                              "output every 1 seconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;

            RunHavingAssertion();
        }

        [Test]
        public void TestHavingJoin()
        {
            SendTimer(0);

            String viewExpr = "select Symbol, avg(Price) as AvgPrice " +
                              "from " + typeof(SupportMarketDataBean).FullName + ".win:time(3 sec) as md, " +
                              typeof(SupportBean).FullName + ".win:keepall() as s where s.TheString = md.Symbol " +
                              "having avg(Price) > 10" +
                              "output every 1 seconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("SYM1", -1));

            RunHavingAssertion();
        }

        private void RunHavingAssertion()
        {
            SendEvent("SYM1", 10d);
            SendEvent("SYM1", 11d);
            SendEvent("SYM1", 9);

            SendTimer(1000);
            String[] fields = "Symbol,AvgPrice".Split(',');
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "SYM1", 10.5 });

            SendEvent("SYM1", 13d);
            SendEvent("SYM1", 10d);
            SendEvent("SYM1", 9);
            SendTimer(2000);

            Assert.AreEqual(3, _listener.LastNewData.Length);
            Assert.IsNull(_listener.LastOldData);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                    new Object[][] { new Object[] { "SYM1", 43 / 4.0 }, new Object[] { "SYM1", 53.0 / 5.0 }, new Object[] { "SYM1", 62 / 6.0 } });
        }

        [Test]
        public void TestMaxTimeWindow()
        {
            SendTimer(0);

            String viewExpr = "select irstream Volume, max(Price) as maxVol" +
                              " from " + typeof(SupportMarketDataBean).FullName + ".win:time(1 sec) " +
                              "output every 1 seconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;

            SendEvent("SYM1", 1d);
            SendEvent("SYM1", 2d);
            _listener.Reset();

            // moves all events out of the window,
            SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
            Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
            Assert.AreEqual(2, result.Second.Length);
            Assert.AreEqual(null, result.Second[0].Get("maxVol"));
            Assert.AreEqual(null, result.Second[1].Get("maxVol"));
        }

        [Test]
        public void TestLimitSnapshot()
        {
            SendTimer(0);
            String selectStmt = "select Symbol, sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
                                ".win:time(10 seconds) output snapshot every 1 seconds order by Symbol asc";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += _listener.Update;
            SendEvent("ABC", 20);

            SendTimer(500);
            SendEvent("IBM", 16);
            SendEvent("MSFT", 14);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendTimer(1000);
            String[] fields = new String[] { "Symbol", "sumPrice" };
            EPAssertionUtil.AssertPropsPerRow(
                _listener.LastNewData, fields, new Object[][]
                {
                    new Object[] {"ABC", 50d}, new Object[] {"IBM", 50d}, new Object[] {"MSFT", 50d}
                });
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(1500);
            SendEvent("YAH", 18);
            SendEvent("s4", 30);

            SendTimer(10000);
            EPAssertionUtil.AssertPropsPerRow(
                _listener.LastNewData, fields,
                new Object[][]
                {
                    new Object[] {"ABC", 98d}, new Object[] {"IBM", 98d}, new Object[] {"MSFT", 98d},
                    new Object[] {"YAH", 98d}, new Object[] {"s4", 98d}
                });
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(
                _listener.LastNewData, fields,
                new Object[][]
                {
                    new Object[] {"YAH", 48d}, new Object[] {"s4", 48d}
                });
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(12000);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsNull(_listener.LastNewData);
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(13000);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsNull(_listener.LastNewData);
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();
        }

        [Test]
        public void TestLimitSnapshotJoin()
        {
            SendTimer(0);
            String selectStmt = "select Symbol, sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
                    ".win:time(10 seconds) as m, " + typeof(SupportBean).FullName +
                    ".win:keepall() as s where s.TheString = m.Symbol output snapshot every 1 seconds order by Symbol asc";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(selectStmt);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("ABC", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("IBM", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("MSFT", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("YAH", 4));
            _epService.EPRuntime.SendEvent(new SupportBean("s4", 5));

            SendEvent("ABC", 20);

            SendTimer(500);
            SendEvent("IBM", 16);
            SendEvent("MSFT", 14);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendTimer(1000);
            String[] fields = new String[] { "Symbol", "sumPrice" };
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] { "ABC", 50d }, new Object[] { "IBM", 50d }, new Object[] { "MSFT", 50d } });
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(1500);
            SendEvent("YAH", 18);
            SendEvent("s4", 30);

            SendTimer(10000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] { "ABC", 98d }, new Object[] { "IBM", 98d }, new Object[] { "MSFT", 98d }, new Object[] { "YAH", 98d }, new Object[] { "s4", 98d } });
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(10500);
            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][] { new Object[] { "YAH", 48d }, new Object[] { "s4", 48d } });
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(11500);
            SendTimer(12000);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsNull(_listener.LastNewData);
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(13000);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsNull(_listener.LastNewData);
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();
        }

        [Test]
        public void TestJoinSortWindow()
        {
            SendTimer(0);

            String viewExpr = "select irstream Volume, max(Price) as maxVol" +
                              " from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(1, Volume desc) as s0," +
                              typeof(SupportBean).FullName + ".win:keepall() as s1 " +
                              "output every 1 seconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));

            SendEvent("JOIN_KEY", 1d);
            SendEvent("JOIN_KEY", 2d);
            _listener.Reset();

            // moves all events out of the window,
            SendTimer(1000);        // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();
            Assert.AreEqual(2, result.First.Length);
            Assert.AreEqual(1.0, result.First[0].Get("maxVol"));
            Assert.AreEqual(2.0, result.First[1].Get("maxVol"));
            Assert.AreEqual(1, result.Second.Length);
            Assert.AreEqual(2.0, result.Second[0].Get("maxVol"));
        }

        [Test]
        public void TestAggregateAllNoJoinLast()
        {
            RunAssertionAggregateAllNoJoinLast(true);
            RunAssertionAggregateAllNoJoinLast(false);
        }

        private void RunAssertionAggregateAllNoJoinLast(bool hinted)
        {
            String hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

            String viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
                            "from " + typeof(SupportBean).FullName + ".win:length(3) " +
                            "having sum(longBoxed) > 0 " +
                            "output last every 2 events";

            RunAssertLastSum(CreateStmtAndListenerNoJoin(viewExpr));

            viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
                        "from " + typeof(SupportBean).FullName + ".win:length(3) " +
                        "output last every 2 events";
            RunAssertLastSum(CreateStmtAndListenerNoJoin(viewExpr));
        }

        [Test]
        public void TestAggregateAllJoinAll()
        {
            RunAssertionAggregateAllJoinAll(true);
            RunAssertionAggregateAllJoinAll(false);
        }

        private void RunAssertionAggregateAllJoinAll(bool hinted)
        {
            String hint = hinted ? "@Hint('enable_outputlimit_opt') " : "";

            String viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
                            "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
                            typeof(SupportBean).FullName + ".win:length(3) as two " +
                            "having sum(LongBoxed) > 0 " +
                            "output all every 2 events";

            RunAssertAllSum(CreateStmtAndListenerJoin(viewExpr));

            viewExpr = hint + "select LongBoxed, sum(LongBoxed) as result " +
                        "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
                        typeof(SupportBean).FullName + ".win:length(3) as two " +
                        "output every 2 events";

            RunAssertAllSum(CreateStmtAndListenerJoin(viewExpr));
        }

        [Test]
        public void TestAggregateAllJoinLast()
        {
            String viewExpr = "select LongBoxed, sum(LongBoxed) as result " +
            "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
            typeof(SupportBean).FullName + ".win:length(3) as two " +
            "having sum(LongBoxed) > 0 " +
            "output last every 2 events";

            RunAssertLastSum(CreateStmtAndListenerJoin(viewExpr));

            viewExpr = "select LongBoxed, sum(LongBoxed) as result " +
            "from " + typeof(SupportBeanString).FullName + ".win:length(3) as one, " +
            typeof(SupportBean).FullName + ".win:length(3) as two " +
            "output last every 2 events";

            RunAssertLastSum(CreateStmtAndListenerJoin(viewExpr));
        }

        [Test]
        public void TestTime()
        {
            // Set the clock to 0
            _currentTime = 0;
            SendTimeEventRelative(0);

            // Create the EPL statement and add a listener
            String statementText = "select Symbol, sum(Volume) from " + EVENT_NAME + ".win:length(5) output first every 3 seconds";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            SupportUpdateListener updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
            updateListener.Reset();

            // Send the first event of the batch; should be output
            SendMarketDataEvent(10L);
            AssertEvent(updateListener, 10L);

            // Send another event, not the first, for aggregation
            // Update only, no output
            SendMarketDataEvent(20L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update time
            SendTimeEventRelative(3000);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Send first event of the next batch, should be output.
            // The aggregate value is computed over all events
            // received: 10 + 20 + 30 = 60
            SendMarketDataEvent(30L);
            AssertEvent(updateListener, 60L);

            // Send the next event of the batch, no output
            SendMarketDataEvent(40L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update time
            SendTimeEventRelative(3000);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Send first event of third batch
            SendMarketDataEvent(1L);
            AssertEvent(updateListener, 101L);

            // Update time
            SendTimeEventRelative(3000);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update time: no first event this batch, so a callback
            // is made at the end of the interval
            SendTimeEventRelative(3000);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestCount()
        {
            // Create the EPL statement and add a listener
            String statementText = "select Symbol, sum(Volume) from " + EVENT_NAME + ".win:length(5) output first every 3 events";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            SupportUpdateListener updateListener = new SupportUpdateListener();
            statement.Events += updateListener.Update;
            updateListener.Reset();

            // Send the first event of the batch, should be output
            SendEventLong(10L);
            AssertEvent(updateListener, 10L);

            // Send the second event of the batch, not output, used
            // for updating the aggregate value only
            SendEventLong(20L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Send the third event of the batch, still not output,
            // but should reset the batch
            SendEventLong(30L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // First event, next batch, aggregate value should be
            // 10 + 20 + 30 + 40 = 100
            SendEventLong(40L);
            AssertEvent(updateListener, 100L);

            // Next event again not output
            SendEventLong(50L);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());
        }

        private void SendEventLong(long volume)
        {
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("DELL", 0.0, volume, null));
        }

        private SupportUpdateListener CreateStmtAndListenerNoJoin(String viewExpr)
        {
            _epService.Initialize();
            SupportUpdateListener updateListener = new SupportUpdateListener();
            EPStatement view = _epService.EPAdministrator.CreateEPL(viewExpr);
            view.Events += updateListener.Update;

            return updateListener;
        }

        private void RunAssertAllSum(SupportUpdateListener updateListener)
        {
            // send an event
            SendEvent(1);

            // check no Update
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // send another event
            SendEvent(2);

            // check Update, all events present
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(2, updateListener.LastNewData.Length);
            Assert.AreEqual(1L, updateListener.LastNewData[0].Get("LongBoxed"));
            Assert.AreEqual(1L, updateListener.LastNewData[0].Get("result"));
            Assert.AreEqual(2L, updateListener.LastNewData[1].Get("LongBoxed"));
            Assert.AreEqual(3L, updateListener.LastNewData[1].Get("result"));
            Assert.IsNull(updateListener.LastOldData);
        }

        private void RunAssertLastSum(SupportUpdateListener updateListener)
        {
            // send an event
            SendEvent(1);

            // check no Update
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // send another event
            SendEvent(2);

            // check Update, all events present
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreEqual(2L, updateListener.LastNewData[0].Get("LongBoxed"));
            Assert.AreEqual(3L, updateListener.LastNewData[0].Get("result"));
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(long longBoxed)
        {
            SendEvent(longBoxed, 0, (short)0);
        }

        private void SendMarketDataEvent(long volume)
        {
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("SYM1", 0, volume, null));
        }

        private void SendTimeEventRelative(int timeIncrement)
        {
            _currentTime += timeIncrement;
            CurrentTimeEvent theEvent = new CurrentTimeEvent(_currentTime);
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private SupportUpdateListener CreateStmtAndListenerJoin(String viewExpr)
        {
            _epService.Initialize();

            SupportUpdateListener updateListener = new SupportUpdateListener();
            EPStatement view = _epService.EPAdministrator.CreateEPL(viewExpr);
            view.Events += updateListener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

            return updateListener;
        }

        private void AssertEvent(SupportUpdateListener updateListener, long volume)
        {
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.IsTrue(updateListener.LastNewData != null);
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreEqual(volume, updateListener.LastNewData[0].Get("sum(Volume)"));
        }

        private void SendEvent(String symbol, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendTimer(long time)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
