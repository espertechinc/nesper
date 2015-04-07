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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestRowLimit
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddEventType("SupportBeanNumeric", typeof(SupportBeanNumeric));
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
        public void TestLimitOneWithOrderOptimization()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S1>();

            // batch-window assertions
            String eplWithBatchSingleKey = "select TheString from SupportBean.win:length_batch(10) order by TheString limit 1";
            RunAssertionLimitOneSingleKeySortBatch(eplWithBatchSingleKey);

            String eplWithBatchMultiKey = "select TheString, IntPrimitive from SupportBean.win:length_batch(5) order by TheString asc, IntPrimitive desc limit 1";
            RunAssertionLimitOneMultiKeySortBatch(eplWithBatchMultiKey);

            // context output-when-terminated assertions
            _epService.EPAdministrator.CreateEPL("create context StartS0EndS1 as start SupportBean_S0 end SupportBean_S1");

            String eplContextSingleKey = "context StartS0EndS1 " +
                    "select TheString from SupportBean.win:keepall() " +
                    "output snapshot when terminated " +
                    "order by TheString limit 1";
            RunAssertionLimitOneSingleKeySortBatch(eplContextSingleKey);

            String eplContextMultiKey = "context StartS0EndS1 " +
                    "select TheString, IntPrimitive from SupportBean.win:keepall() " +
                    "output snapshot when terminated " +
                    "order by TheString asc, IntPrimitive desc limit 1";
            RunAssertionLimitOneMultiKeySortBatch(eplContextMultiKey);
        }

        private void RunAssertionLimitOneMultiKeySortBatch(String epl)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);

            SendSBSequenceAndAssert("F", 10, new Object[][] { new Object[] { "F", 10 }, new Object[] { "X", 8 }, new Object[] { "F", 8 }, new Object[] { "G", 10 }, new Object[] { "X", 1 } });
            SendSBSequenceAndAssert("G", 12, new Object[][] { new Object[] { "X", 10 }, new Object[] { "G", 12 }, new Object[] { "H", 100 }, new Object[] { "G", 10 }, new Object[] { "X", 1 } });
            SendSBSequenceAndAssert("G", 11, new Object[][] { new Object[] { "G", 10 }, new Object[] { "G", 8 }, new Object[] { "G", 8 }, new Object[] { "G", 10 }, new Object[] { "G", 11 } });

            stmt.Dispose();
        }

        private void RunAssertionLimitOneSingleKeySortBatch(String epl)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);

            SendSBSequenceAndAssert("A", new String[] {"F", "Q", "R", "T", "M", "T", "A", "I", "P", "B"});
            SendSBSequenceAndAssert("B", new String[] {"P", "Q", "P", "T", "P", "T", "P", "P", "P", "B"});
            SendSBSequenceAndAssert("C", new String[] {"C", "P", "Q", "P", "T", "P", "T", "P", "P", "P", "X"});

            stmt.Dispose();
        }

        private void RunAssertionVariable(EPStatement stmt)
        {
            String[] fields = "TheString".Split(',');
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E1", 1);
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E2"}});

            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E2"}, new Object[] {"E3"}});

            SendEvent("E4", 4);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E2"}, new Object[] {"E3"}});
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("E5", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E2"}, new Object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E2"}, new Object[] {"E3"}});

            SendEvent("E6", 6);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E3"}, new Object[] {"E4"}});
            Assert.IsFalse(_listener.IsInvoked);

            // change variable values
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 3));
            SendEvent("E7", 7);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E6"}, new Object[] {"E7"}});
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 0));
            SendEvent("E8", 8);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[]
                                                  {
                                                      new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"},
                                                      new Object[] {"E7"}, new Object[] {"E8"}
                                                  });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(10, 0));
            SendEvent("E9", 9);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[]
                                                  {
                                                      new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"},
                                                      new Object[] {"E8"}, new Object[] {"E9"}
                                                  });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(6, 3));
            SendEvent("E10", 10);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E9"}, new Object[] {"E10"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E9"}, new Object[] {"E10"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E7"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E7"}, new Object[] {"E8"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E8"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(6, 6));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(1, 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E10"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, null));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[]
                                                  {
                                                      new Object[] {"E6"}, new Object[] {"E7"}, new Object[] {"E8"},
                                                      new Object[] {"E9"}, new Object[] {"E10"}
                                                  });

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(null, 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E8"}, new Object[] {"E9"}, new Object[] {"E10"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, null));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E6"}, new Object[] {"E7"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E10"}});

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(-1, 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[]
                                                  {
                                                      new Object[] {"E6"}, new Object[] {"E7"}, new Object[] {"E8"},
                                                      new Object[] {"E9"}, new Object[] {"E10"}
                                                  });

            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(0, 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
        }

        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void RunAssertion(EPStatement stmt)
        {
            String[] fields = "TheString".Split(',');
            stmt.Events += _listener.Update;
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E1"}});

            SendEvent("E2", 2);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E1"}});

            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new[] {new Object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E4", 4);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E4"}});

            SendEvent("E5", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E4"}});

            SendEvent("E6", 6);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new[] {new Object[] {"E4"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new[] {new Object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
        }

        private void TryInvalid(String expression,
                                String expected)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        private void SendEvent(String stringValue,
                               int intPrimitive)
        {
            _epService.EPRuntime.SendEvent(new SupportBean(stringValue, intPrimitive));
        }

        [Test]
        public void TestBatchNoOffsetNoOrder()
        {
            String statementString = "select irstream * from SupportBean.win:length_batch(3) limit 1";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);

            RunAssertion(stmt);
        }

        [Test]
        public void TestBatchOffsetNoOrderOM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.SelectClause.StreamSelector = StreamSelector.RSTREAM_ISTREAM_BOTH;
            model.FromClause = FromClause.Create(FilterStream.Create("SupportBean").AddView("win", "length_batch",
                Expressions.Constant(3)));
            model.RowLimitClause = RowLimitClause.Create(1);

            String statementString = "select irstream * from SupportBean.win:length_batch(3) limit 1";
            Assert.AreEqual(statementString, model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            RunAssertion(stmt);
            stmt.Dispose();
            _listener.Reset();

            model = _epService.EPAdministrator.CompileEPL(statementString);
            Assert.AreEqual(statementString, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            RunAssertion(stmt);
        }

        [Test]
        public void TestEventPerRowUnGrouped()
        {
            SendTimer(1000);
            String statementString =
                "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) output every 10 seconds order by TheString desc limit 2";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);

            String[] fields = "TheString,mysum".Split(',');
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E1", 10);
            SendEvent("E2", 5);
            SendEvent("E3", 20);
            SendEvent("E4", 30);

            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                                 new[] {new Object[] {"E4", 65}, new Object[] {"E3", 35}});
        }

        [Test]
        public void TestFullyGroupedOrdered()
        {
            String statementString =
                "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) group by TheString order by sum(IntPrimitive) limit 2";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);

            String[] fields = "TheString,mysum".Split(',');
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E1", 90);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E1", 90}});

            SendEvent("E2", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E2", 5}, new Object[] {"E1", 90}});

            SendEvent("E3", 60);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E2", 5}, new Object[] {"E3", 60}});

            SendEvent("E3", 40);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E2", 5}, new Object[] {"E1", 90}});

            SendEvent("E2", 1000);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E1", 90}, new Object[] {"E3", 100}});
        }

        [Test]
        public void TestGroupedSnapshot()
        {
            SendTimer(1000);
            String statementString =
                "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) group by TheString output snapshot every 10 seconds order by sum(IntPrimitive) desc limit 2";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);

            String[] fields = "TheString,mysum".Split(',');
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E1", 10);
            SendEvent("E2", 5);
            SendEvent("E3", 20);
            SendEvent("E1", 30);

            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                                 new[] {new Object[] {"E1", 40}, new Object[] {"E3", 20}});
        }

        [Test]
        public void TestGroupedSnapshotNegativeRowcount()
        {
            SendTimer(1000);
            String statementString =
                "select TheString, sum(IntPrimitive) as mysum from SupportBean.win:length(5) group by TheString output snapshot every 10 seconds order by sum(IntPrimitive) desc limit -1 offset 1";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);

            String[] fields = "TheString,mysum".Split(',');
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E1", 10);
            SendEvent("E2", 5);
            SendEvent("E3", 20);
            SendEvent("E1", 30);

            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                                 new[] {new Object[] {"E3", 20}, new Object[] {"E2", 5}});
        }

        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.CreateEPL("create variable String myrows = 'abc'");
            TryInvalid("select * from SupportBean limit myrows",
                       "Error starting statement: Limit clause requires a variable of numeric type [select * from SupportBean limit myrows]");
            TryInvalid("select * from SupportBean limit 1, myrows",
                       "Error starting statement: Limit clause requires a variable of numeric type [select * from SupportBean limit 1, myrows]");
            TryInvalid("select * from SupportBean limit dummy",
                       "Error starting statement: Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit dummy]");
            TryInvalid("select * from SupportBean limit 1,dummy",
                       "Error starting statement: Limit clause variable by name 'dummy' has not been declared [select * from SupportBean limit 1,dummy]");
        }

        [Test]
        public void TestLengthOffsetVariable()
        {
            _epService.EPAdministrator.CreateEPL("create variable int myrows = 2");
            _epService.EPAdministrator.CreateEPL("create variable int myoffset = 1");
            _epService.EPAdministrator.CreateEPL("on SupportBeanNumeric set myrows = intOne, myoffset = intTwo");

            String statementString =
                "select * from SupportBean.win:length(5) output every 5 events limit myoffset, myrows";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);
            RunAssertionVariable(stmt);
            stmt.Dispose();
            _listener.Reset();
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));

            statementString =
                "select * from SupportBean.win:length(5) output every 5 events limit myrows offset myoffset";
            stmt = _epService.EPAdministrator.CreateEPL(statementString);
            RunAssertionVariable(stmt);
            stmt.Dispose();
            _listener.Reset();
            _epService.EPRuntime.SendEvent(new SupportBeanNumeric(2, 1));

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(statementString);
            Assert.AreEqual(statementString, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            RunAssertionVariable(stmt);
        }

        [Test]
        public void TestOrderBy()
        {
            String statementString =
                "select * from SupportBean.win:length(5) output every 5 events order by IntPrimitive limit 2 offset 2";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(statementString);

            String[] fields = "TheString".Split(',');
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E1", 90);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E2", 5);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendEvent("E3", 60);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {"E1"}});

            SendEvent("E4", 99);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E1"}, new Object[] {"E4"}});
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("E5", 6);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {"E3"}, new Object[] {"E1"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E3"}, new Object[] {"E1"}});
        }

        private void SendSBSequenceAndAssert(String expected, String[] theStrings)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            foreach (String theString in theStrings) {
                SendEvent(theString, 0);
            }
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[]{expected});
        }

        private void SendSBSequenceAndAssert(String expectedString, int expectedInt, Object[][] rows)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            foreach (Object[] row in rows) {
                SendEvent(row[0].ToString(), (int) row[1]);
            }
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "TheString,IntPrimitive".Split(','), new Object[]{expectedString, expectedInt});
        }
    }
}
