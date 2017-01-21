// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.type;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class Test2StreamOuterJoin
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();

            _eventsS0 = new SupportBean_S0[15];
            _eventsS1 = new SupportBean_S1[15];
            int count = 100;
            for (int i = 0; i < _eventsS0.Length; i++)
            {
                _eventsS0[i] = new SupportBean_S0(count++, Convert.ToString(i));
            }
            count = 200;
            for (int i = 0; i < _eventsS1.Length; i++)
            {
                _eventsS1[i] = new SupportBean_S1(count++, Convert.ToString(i));
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _eventsS0 = null;
            _eventsS1 = null;
        }

        #endregion

        private static readonly String[] Fields = new[] {"s0.id", "s0.P00", "s1.id", "s1.p10"};

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private SupportBean_S0[] _eventsS0;
        private SupportBean_S1[] _eventsS1;

        private void RunAssertion(String epl)
        {
            String[] fields = "sbstr,sbint,sbrk,sbrs,sbre".Split(',');
            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(epl);
            outerJoinView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("K1", 10));
            _epService.EPRuntime.SendEvent(new SupportBeanRange("R1", "K1", 20, 30));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("K1", 30));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                              new[] {new Object[] {"K1", 30, "K1", 20, 30}});

            _epService.EPRuntime.SendEvent(new SupportBean("K1", 40));
            _epService.EPRuntime.SendEvent(new SupportBean("K1", 31));
            _epService.EPRuntime.SendEvent(new SupportBean("K1", 19));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 39, 41));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                              new[] {new Object[] {"K1", 40, "K1", 39, 41}});

            _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 38, 40));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                              new[] {new Object[] {"K1", 40, "K1", 38, 40}});

            _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 40, 42));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                              new[] {new Object[] {"K1", 40, "K1", 40, 42}});

            _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 41, 42));
            _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 38, 39));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("K1", 41));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new[]
            {
                new Object[] {"K1", 41, "K1", 39, 41},
                new Object[] {"K1", 41, "K1", 40, 42},
                new Object[] {"K1", 41, "K1", 41, 42}
            });

            _epService.EPRuntime.SendEvent(new SupportBeanRange("R2", "K1", 35, 42));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields,
                                              new[]
                                              {
                                                  new Object[] {"K1", 40, "K1", 35, 42},
                                                  new Object[] {"K1", 41, "K1", 35, 42}
                                              });

            outerJoinView.Dispose();
        }

        private void AssertMultiColumnLeft()
        {
            String[] fields = "s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {1, "A_1", "B_1", null, null, null});

            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {1, "A_1", "B_1", 2, "A_1", "B_1"});

            _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A_2", "B_1"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "A_1", "B_2"));
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void CompareEvent(EventBean receivedEvent, int? idS0, String p00, int? idS1, String p10)
        {
            Assert.AreEqual(idS0, receivedEvent.Get("s0.id"));
            Assert.AreEqual(idS1, receivedEvent.Get("s1.id"));
            Assert.AreEqual(p00, receivedEvent.Get("s0.P00"));
            Assert.AreEqual(p10, receivedEvent.Get("s1.p10"));
        }

        private void SendEvent(String s, int intPrimitive, double doublePrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(String s, int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEventMD(String symbol, long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            _epService.EPRuntime.SendEvent(bean);
        }

        private EPStatement SetupStatement(String outerJoinType)
        {
            String joinStatement = "select irstream s0.id, s0.P00, s1.id, s1.p10 from " +
                                   typeof (SupportBean_S0).FullName + ".win:length(3) as s0 " +
                                   outerJoinType + " outer join " +
                                   typeof (SupportBean_S1).FullName + ".win:length(5) as s1" +
                                   " on s0.P00 = s1.p10";

            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            outerJoinView.Events += _listener.Update;

            return outerJoinView;
        }

        private void SendEvent(Object theEvent)
        {
            _epService.EPRuntime.SendEvent(theEvent);
        }

        [Test]
        public void TestEventType()
        {
            EPStatement outerJoinView = SetupStatement("left");

            Assert.AreEqual(typeof (string), outerJoinView.EventType.GetPropertyType("s0.P00"));
            Assert.AreEqual(typeof (int?), outerJoinView.EventType.GetPropertyType("s0.id"));
            Assert.AreEqual(typeof (string), outerJoinView.EventType.GetPropertyType("s1.p10"));
            Assert.AreEqual(typeof (int?), outerJoinView.EventType.GetPropertyType("s1.id"));
            Assert.AreEqual(4, outerJoinView.EventType.PropertyNames.Length);
        }

        [Test]
        public void TestFullOuterIteratorGroupBy()
        {
            String stmt = "select TheString, IntPrimitive, Symbol, Volume " +
                          "from " + typeof (SupportMarketDataBean).FullName + ".win:keepall() " +
                          "full outer join " +
                          typeof (SupportBean).FullName + ".std:groupwin(TheString, IntPrimitive).win:length(2) " +
                          "on TheString = Symbol " +
                          "group by TheString, IntPrimitive, Symbol " +
                          "order by TheString, IntPrimitive, Symbol, Volume";

            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(stmt);
            outerJoinView.Events += _listener.Update;

            SendEventMD("c0", 200L);
            SendEventMD("c3", 400L);

            SendEvent("c0", 0);
            SendEvent("c0", 1);
            SendEvent("c0", 2);
            SendEvent("c1", 0);
            SendEvent("c1", 1);
            SendEvent("c1", 2);
            SendEvent("c2", 0);
            SendEvent("c2", 1);
            SendEvent("c2", 2);

            IEnumerator<EventBean> iterator = outerJoinView.GetSafeEnumerator();
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(iterator);
            Assert.AreEqual(10, events.Length);

            EPAssertionUtil.AssertPropsPerRow(
                events, "TheString,IntPrimitive,Symbol,Volume".Split(','),
                new[]
                {
                    new Object[] {null, null, "c3", 400L},
                    new Object[] {"c0", 0, "c0", 200L},
                    new Object[] {"c0", 1, "c0", 200L},
                    new Object[] {"c0", 2, "c0", 200L},
                    new Object[] {"c1", 0, null, null},
                    new Object[] {"c1", 1, null, null},
                    new Object[] {"c1", 2, null, null},
                    new Object[] {"c2", 0, null, null},
                    new Object[] {"c2", 1, null, null},
                    new Object[] {"c2", 2, null, null}
                });
        }

        [Test]
        public void TestFullOuterJoin()
        {
            EPStatement outerJoinView = SetupStatement("full");

            // Send S0[0]
            SendEvent(_eventsS0[0]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), 100, "0", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[] {new Object[] {100, "0", null, null}});

            // Send S1[1]
            SendEvent(_eventsS1[1]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), null, null, 201, "1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {100, "0", null, null},
                                                          new Object[] {null, null, 201, "1"}
                                                      });

            // Send S1[2] and S0[2]
            SendEvent(_eventsS1[2]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), null, null, 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {100, "0", null, null},
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {null, null, 202, "2"}
                                                      });

            SendEvent(_eventsS0[2]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {100, "0", null, null},
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {102, "2", 202, "2"}
                                                      });

            // Send S0[3] and S1[3]
            SendEvent(_eventsS0[3]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), 103, "3", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {100, "0", null, null},
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", null, null}
                                                      });
            SendEvent(_eventsS1[3]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), 103, "3", 203, "3");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {100, "0", null, null},
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"}
                                                      });

            // Send S0[4], pushes S0[0] out of window
            SendEvent(_eventsS0[4]);
            EventBean oldEvent = _listener.LastOldData[0];
            EventBean newEvent = _listener.LastNewData[0];
            CompareEvent(oldEvent, 100, "0", null, null);
            CompareEvent(newEvent, 104, "4", null, null);
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"},
                                                          new Object[] {104, "4", null, null}
                                                      });

            // Send S1[4]
            SendEvent(_eventsS1[4]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), 104, "4", 204, "4");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"},
                                                          new Object[] {104, "4", 204, "4"}
                                                      });

            // Send S1[5]
            SendEvent(_eventsS1[5]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), null, null, 205, "5");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {null, null, 201, "1"},
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"},
                                                          new Object[] {104, "4", 204, "4"},
                                                          new Object[] {null, null, 205, "5"}
                                                      });

            // Send S1[6], pushes S1[1] out of window
            SendEvent(_eventsS1[5]);
            oldEvent = _listener.LastOldData[0];
            newEvent = _listener.LastNewData[0];
            CompareEvent(oldEvent, null, null, 201, "1");
            CompareEvent(newEvent, null, null, 205, "5");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"},
                                                          new Object[] {104, "4", 204, "4"},
                                                          new Object[] {null, null, 205, "5"},
                                                          new Object[] {null, null, 205, "5"}
                                                      });
        }

        [Test]
        public void TestLeftOuterJoin()
        {
            EPStatement outerJoinView = SetupStatement("left");

            // Send S1 events, no events expected
            SendEvent(_eventsS1[0]);
            SendEvent(_eventsS1[1]);
            SendEvent(_eventsS1[3]);
            Assert.IsNull(_listener.LastNewData); // No events expected
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields, null);

            // Send S0 event, expect event back from outer join
            SendEvent(_eventsS0[2]);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 102, "2", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[] {new Object[] {102, "2", null, null}});

            // Send S1 event matching S0, expect event back
            SendEvent(_eventsS1[2]);
            theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[] {new Object[] {102, "2", 202, "2"}});

            // Send some more unmatched events
            SendEvent(_eventsS1[4]);
            SendEvent(_eventsS1[5]);
            SendEvent(_eventsS1[6]);
            Assert.IsNull(_listener.LastNewData); // No events expected
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[] {new Object[] {102, "2", 202, "2"}});

            // Send event, expect a join result
            SendEvent(_eventsS0[5]);
            theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 105, "5", 205, "5");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {105, "5", 205, "5"}
                                                      });

            // Let S1[2] go out of the window (lenght 5), expected old join event
            SendEvent(_eventsS1[7]);
            SendEvent(_eventsS1[8]);
            theEvent = _listener.AssertOneGetOldAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", null, null},
                                                          new Object[] {105, "5", 205, "5"}
                                                      });

            // S0[9] should generate an outer join event
            SendEvent(_eventsS0[9]);
            theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 109, "9", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", null, null},
                                                          new Object[] {109, "9", null, null},
                                                          new Object[] {105, "5", 205, "5"}
                                                      });

            // S0[2] Should leave the window (length 3), should get OLD and NEW event
            SendEvent(_eventsS0[10]);
            EventBean oldEvent = _listener.LastOldData[0];
            EventBean newEvent = _listener.LastNewData[0];
            CompareEvent(oldEvent, 102, "2", null, null); // S1[2] has left the window already
            CompareEvent(newEvent, 110, "10", null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {110, "10", null, null},
                                                          new Object[] {109, "9", null, null},
                                                          new Object[] {105, "5", 205, "5"}
                                                      });
        }

        [Test]
        public void TestMultiColumnLeft()
        {
            String joinStatement = "select s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11 from " +
                                   typeof (SupportBean_S0).FullName + ".win:length(3) as s0 " +
                                   "left outer join " +
                                   typeof (SupportBean_S1).FullName + ".win:length(5) as s1" +
                                   " on s0.P00 = s1.p10 and s0.p01 = s1.p11";

            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            outerJoinView.Events += _listener.Update;

            AssertMultiColumnLeft();
        }

        [Test]
        public void TestMultiColumnLeft_OM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11".Split(','));
            FromClause fromClause = FromClause.Create(
                FilterStream.Create(typeof (SupportBean_S0).FullName, "s0").AddView("win", "keepall"),
                FilterStream.Create(typeof (SupportBean_S1).FullName, "s1").AddView("win", "keepall"));
            fromClause.Add(OuterJoinQualifier.Create("s0.P00", OuterJoinType.LEFT, "s1.p10").Add("s1.p11", "s0.p01"));
            model.FromClause = fromClause;
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);

            String stmtText =
                "select s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11 from com.espertech.esper.support.bean.SupportBean_S0.win:keepall() as s0 left outer join com.espertech.esper.support.bean.SupportBean_S1.win:keepall() as s1 on s0.P00 = s1.p10 and s1.p11 = s0.p01";
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement outerJoinView = _epService.EPAdministrator.Create(model);
            outerJoinView.Events += _listener.Update;

            AssertMultiColumnLeft();

            EPStatementObjectModel modelReverse = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText, modelReverse.ToEPL());
        }

        [Test]
        public void TestMultiColumnRight()
        {
            String[] fields = "s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11".Split(',');
            String joinStatement = "select s0.id, s0.P00, s0.p01, s1.id, s1.p10, s1.p11 from " +
                                   typeof (SupportBean_S0).FullName + ".win:length(3) as s0 " +
                                   "right outer join " +
                                   typeof (SupportBean_S1).FullName + ".win:length(5) as s1" +
                                   " on s0.P00 = s1.p10 and s1.p11 = s0.p01";

            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            outerJoinView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A_1", "B_1"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {1, "A_1", "B_1", 2, "A_1", "B_1"});

            _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A_2", "B_1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {null, null, null, 3, "A_2", "B_1"});

            _epService.EPRuntime.SendEvent(new SupportBean_S1(4, "A_1", "B_2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[] {null, null, null, 4, "A_1", "B_2"});
        }

        [Test]
        public void TestMultiColumnRightCoercion()
        {
            String[] fields = "s0.TheString, s1.TheString".Split(',');
            String joinStatement = "select s0.TheString, s1.TheString from " +
                                   typeof (SupportBean).FullName + "(TheString like 'S0%').win:keepall() as s0 " +
                                   "right outer join " +
                                   typeof (SupportBean).FullName + "(TheString like 'S1%').win:keepall() as s1" +
                                   " on s0.IntPrimitive = s1.DoublePrimitive and s1.IntPrimitive = s0.DoublePrimitive";

            EPStatement outerJoinView = _epService.EPAdministrator.CreateEPL(joinStatement);
            outerJoinView.Events += _listener.Update;

            SendEvent("S1_1", 10, 20d);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, "S1_1"});

            SendEvent("S0_2", 11, 22d);
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("S0_3", 11, 21d);
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("S0_4", 12, 21d);
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("S1_2", 11, 22d);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, "S1_2"});

            SendEvent("S1_3", 22, 11d);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"S0_2", "S1_3"});

            SendEvent("S0_5", 22, 11d);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"S0_5", "S1_2"});
        }

        [Test]
        public void TestRangeOuterJoin()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof (SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof (SupportBeanRange));

            String stmtOne =
                "select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                "from SupportBean.win:keepall() sb " +
                "full outer join " +
                "SupportBeanRange.win:keepall() sbr " +
                "on TheString = key " +
                "where IntPrimitive between rangeStart and rangeEnd " +
                "order by rangeStart asc, IntPrimitive asc";
            RunAssertion(stmtOne);

            String stmtTwo =
                "select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                "from SupportBeanRange.win:keepall() sbr " +
                "full outer join " +
                "SupportBean.win:keepall() sb " +
                "on TheString = key " +
                "where IntPrimitive between rangeStart and rangeEnd " +
                "order by rangeStart asc, IntPrimitive asc";
            RunAssertion(stmtTwo);

            String stmtThree =
                "select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                "from SupportBeanRange.win:keepall() sbr " +
                "full outer join " +
                "SupportBean.win:keepall() sb " +
                "on TheString = key " +
                "where IntPrimitive >= rangeStart and IntPrimitive <= rangeEnd " +
                "order by rangeStart asc, IntPrimitive asc";
            RunAssertion(stmtThree);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestRightOuterJoin()
        {
            EPStatement outerJoinView = SetupStatement("right");

            // Send S0 events, no events expected
            SendEvent(_eventsS0[0]);
            SendEvent(_eventsS0[1]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields, null);

            // Send S1[2]
            SendEvent(_eventsS1[2]);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, null, null, 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[] {new Object[] {null, null, 202, "2"}});

            // Send S0[2] events, joined event expected
            SendEvent(_eventsS0[2]);
            theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[] {new Object[] {102, "2", 202, "2"}});

            // Send S1[3]
            SendEvent(_eventsS1[3]);
            theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, null, null, 203, "3");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {null, null, 203, "3"}
                                                      });

            // Send some more S0 events
            SendEvent(_eventsS0[3]);
            theEvent = _listener.AssertOneGetNewAndReset();
            CompareEvent(theEvent, 103, "3", 203, "3");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"}
                                                      });

            // Send some more S0 events
            SendEvent(_eventsS0[4]);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {102, "2", 202, "2"},
                                                          new Object[] {103, "3", 203, "3"}
                                                      });

            // Push S0[2] out of the window
            SendEvent(_eventsS0[5]);
            theEvent = _listener.AssertOneGetOldAndReset();
            CompareEvent(theEvent, 102, "2", 202, "2");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {null, null, 202, "2"},
                                                          new Object[] {103, "3", 203, "3"}
                                                      });

            // Some more S1 events
            SendEvent(_eventsS1[6]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), null, null, 206, "6");
            SendEvent(_eventsS1[7]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), null, null, 207, "7");
            SendEvent(_eventsS1[8]);
            CompareEvent(_listener.AssertOneGetNewAndReset(), null, null, 208, "8");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {null, null, 202, "2"},
                                                          new Object[] {103, "3", 203, "3"},
                                                          new Object[] {null, null, 206, "6"},
                                                          new Object[] {null, null, 207, "7"},
                                                          new Object[] {null, null, 208, "8"}
                                                      });

            // Push S1[2] out of the window
            SendEvent(_eventsS1[9]);
            EventBean oldEvent = _listener.LastOldData[0];
            EventBean newEvent = _listener.LastNewData[0];
            CompareEvent(oldEvent, null, null, 202, "2");
            CompareEvent(newEvent, null, null, 209, "9");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(outerJoinView.GetEnumerator(), Fields,
                                                      new[]
                                                      {
                                                          new Object[] {103, "3", 203, "3"},
                                                          new Object[] {null, null, 206, "6"},
                                                          new Object[] {null, null, 207, "7"},
                                                          new Object[] {null, null, 208, "8"},
                                                          new Object[] {null, null, 209, "9"}
                                                      });
        }
    }
}
