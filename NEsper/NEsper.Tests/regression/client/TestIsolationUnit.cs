///////////////////////////////////////////////////////////////////////////////////////
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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestIsolationUnit
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();

            configuration.AddEventType("SupportBean", typeof (SupportBean));
            configuration.AddEventType("SupportBean_A", typeof (SupportBean_A));
            configuration.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            configuration.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void SendTimerUnisolated(long millis)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(millis));
        }

        private void SendTimerIso(long millis, EPServiceProviderIsolated unit)
        {
            unit.EPRuntime.SendEvent(new CurrentTimeEvent(millis));
        }

        [Test]
        public void TestMovePattern()
        {
            EPServiceProviderIsolated isolatedService = _epService.GetEPServiceIsolated("Isolated");
            EPStatement stmt = isolatedService.EPAdministrator.CreateEPL("select * from pattern [every (a=SupportBean -> b=SupportBean(theString=a.theString)) where timer:within(1 day)]", "TestStatement", null);
            isolatedService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.CurrentTimeMillis + 1000));
            isolatedService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmt.AddListener(_listener);
            isolatedService.EPAdministrator.RemoveStatement(stmt);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsTrue(_listener.IsInvokedAndReset());
        }

        [Test]
        public void TestCreateStmt()
        {
            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            SendTimerUnisolated(100000);
            SendTimerIso(1000, unit);

            var fields = new String[]
            {
                "ct"
            };
            EPStatement stmt = unit.EPAdministrator.CreateEPL(
                "select Current_timestamp() as ct from pattern[every timer:interval(10)]",
                null, null);

            stmt.Events += _listener.Update;

            SendTimerIso(10999, unit);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimerIso(11000, unit);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    11000L
                }
                );

            SendTimerIso(15000, unit);

            unit.EPAdministrator.RemoveStatement(stmt);

            SendTimerIso(21000, unit);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimerUnisolated(106000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    106000L
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestCurrentTimestamp()
        {
            SendTimerUnisolated(5000);
            var fields = new String[]
            {
                "ct"
            };
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select Current_timestamp() as ct from SupportBean");

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    5000L
                }
                );

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            SendTimerIso(100000, unit);
            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmt
            }
                );

            unit.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    100000L
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();

            stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString as ct from SupportBean where Current_timestamp() >= 10000");
            stmt.Events += _listener.Update;

            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_listener.IsInvoked);

            SendTimerUnisolated(10000);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            unit.EPAdministrator.AddStatement(stmt);

            unit.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString as ct from SupportBean where Current_timestamp() >= 120000");
            stmt.Events += _listener.Update;
            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmt
            }
                );

            unit.EPRuntime.SendEvent(new SupportBean("E3", 1));
            Assert.IsFalse(_listener.IsInvoked);

            SendTimerIso(120000, unit);

            unit.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E4"
                }
                );
        }

        [Test]
        public void TestDestroy()
        {
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(
                "@Name('A') select * from SupportBean", null, null);

            stmtOne.Events += _listener.Update;

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            EPStatement stmtTwo = unit.EPAdministrator.CreateEPL(
                "@Name('B') select * from SupportBean", null, null);

            stmtTwo.Events += _listener.Update;
            unit.EPAdministrator.AddStatement(stmtOne);

            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(2, _listener.GetNewDataListFlattened().Length);
            _listener.Reset();

            unit.Dispose();

            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(0, _listener.GetNewDataListFlattened().Length);
            _listener.Reset();

            _epService.EPRuntime.SendEvent(new SupportBean());
            Assert.AreEqual(2, _listener.GetNewDataListFlattened().Length);
            _listener.Reset();
        }

        [Test]
        public void TestInsertInto()
        {
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(
                "insert into MyStream select * from SupportBean");
            var listenerInsert = new SupportUpdateListener();

            stmtInsert.Events += listenerInsert.Update;

            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(
                "select * from MyStream");
            var listenerSelect = new SupportUpdateListener();

            stmtSelect.Events += listenerSelect.Update;

            // unit takes "insert"
            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            unit.EPAdministrator.AddStatement(stmtInsert);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            Assert.IsFalse(listenerInsert.GetAndClearIsInvoked());

            unit.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(),
                "TheString".Split(','), new Object[]
                {
                    "E2"
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            Assert.IsFalse(listenerInsert.GetAndClearIsInvoked());
            // is there a remaining event that gets flushed with the last one?

            // unit returns insert
            unit.EPAdministrator.RemoveStatement(stmtInsert);

            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(),
                "TheString".Split(','), new Object[]
                {
                    "E4"
                }
                );
            EPAssertionUtil.AssertProps(
                listenerSelect.AssertOneGetNewAndReset(),
                "TheString".Split(','), new Object[]
                {
                    "E4"
                }
                );

            unit.EPRuntime.SendEvent(new SupportBean("E5", 5));
            Assert.IsFalse(listenerSelect.GetAndClearIsInvoked());
            Assert.IsFalse(listenerInsert.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertProps(
                listenerInsert.AssertOneGetNewAndReset(),
                "TheString".Split(','), new Object[]
                {
                    "E6"
                }
                );
            EPAssertionUtil.AssertProps(
                listenerSelect.AssertOneGetNewAndReset(),
                "TheString".Split(','), new Object[]
                {
                    "E6"
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestInvalid()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "@Name('A') select * from SupportBean");
            EPServiceProviderIsolated unitOne = _epService.GetEPServiceIsolated("i1");
            EPServiceProviderIsolated unitTwo = _epService.GetEPServiceIsolated("i2");

            unitOne.EPRuntime.SendEvent(
                new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_INTERNAL));
            unitOne.EPRuntime.SendEvent(
                new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));

            unitOne.EPAdministrator.AddStatement(stmt);
            try
            {
                unitTwo.EPAdministrator.AddStatement(stmt);
                Assert.Fail();
            }
            catch (EPServiceIsolationException ex)
            {
                Assert.AreEqual(
                    "Statement named 'A' already in service isolation under 'i1'",
                    ex.Message);
            }

            try
            {
                unitTwo.EPAdministrator.RemoveStatement(stmt);
                Assert.Fail();
            }
            catch (EPServiceIsolationException ex)
            {
                Assert.AreEqual(
                    "Statement named 'A' not in this service isolation but under service isolation 'A'",
                    ex.Message);
            }

            unitOne.EPAdministrator.RemoveStatement(stmt);

            try
            {
                unitOne.EPAdministrator.RemoveStatement(stmt);
                Assert.Fail();
            }
            catch (EPServiceIsolationException ex)
            {
                Assert.AreEqual(
                    "Statement named 'A' is not currently in service isolation",
                    ex.Message);
            }

            try
            {
                unitTwo.EPAdministrator.RemoveStatement(new EPStatement[]
                {
                    null
                }
                    );
                Assert.Fail();
            }
            catch (EPServiceIsolationException ex)
            {
                Assert.AreEqual(
                    "Illegal argument, a null value was provided in the statement list",
                    ex.Message);
            }

            try
            {
                unitTwo.EPAdministrator.AddStatement(new EPStatement[]
                {
                    null
                }
                    );
                Assert.Fail();
            }
            catch (EPServiceIsolationException ex)
            {
                Assert.AreEqual(
                    "Illegal argument, a null value was provided in the statement list",
                    ex.Message);
            }
        }

        [Test]
        public void TestIsolateFilter()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from pattern [every a=SupportBean -> b=SupportBean(TheString=a.TheString)]");

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"i1"}, _epService.EPServiceIsolatedNames);

            // send fake to wrong place
            unit.EPRuntime.SendEvent(new SupportBean("E1", -1));

            unit.EPAdministrator.AddStatement(stmt);

            // send to 'wrong' engine
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            // send to 'right' engine
            unit.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(),
                "a.TheString,a.IntPrimitive,b.IntPrimitive".Split(','),
                new Object[]
                {
                    "E1", 1, 3
                }
                );

            // send second pair, and a fake to the wrong place
            unit.EPRuntime.SendEvent(new SupportBean("E2", 4));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -1));

            unit.EPAdministrator.RemoveStatement(stmt);

            // send to 'wrong' engine
            unit.EPRuntime.SendEvent(new SupportBean("E2", 5));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            // send to 'right' engine
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(),
                "a.TheString,a.IntPrimitive,b.IntPrimitive".Split(','),
                new Object[]
                {
                    "E2", 4, 6
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
            EPAssertionUtil.AssertEqualsAnyOrder(new string[] {"i1"}, _epService.EPServiceIsolatedNames);
            _epService.GetEPServiceIsolated("i1").Dispose();
            EPAssertionUtil.AssertEqualsAnyOrder(new string[0], _epService.EPServiceIsolatedNames);
        }

        [Test]
        public void TestIsolateMultiple()
        {
            var fields = new String[]
            {
                "TheString", "sumi"
            };
            int count = 4;
            var listeners = new SupportUpdateListener[count];

            for (int i = 0; i < count; i++)
            {
                String epl = "@Name('S" + i
                             + "') select TheString, sum(IntPrimitive) as sumi from SupportBean(TheString='"
                             + i + "').win:time(10)";

                listeners[i] = new SupportUpdateListener();
                _epService.EPAdministrator.CreateEPL(epl).Events += listeners[i].Update;
            }

            var statements = new EPStatement[2];

            statements[0] = _epService.EPAdministrator.GetStatement("S0");
            statements[1] = _epService.EPAdministrator.GetStatement("S2");

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            unit.EPAdministrator.AddStatement(statements);

            // send to unisolated
            for (int i = 0; i < count; i++)
            {
                _epService.EPRuntime.SendEvent(
                    new SupportBean(Convert.ToString(i), i));
            }
            Assert.IsFalse(listeners[0].IsInvoked);
            Assert.IsFalse(listeners[2].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "1", 1
                                        }
                );
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "3", 3
                                        }
                );

            // send to isolated
            for (int i = 0; i < count; i++)
            {
                unit.EPRuntime.SendEvent(
                    new SupportBean(Convert.ToString(i), i));
            }
            Assert.IsFalse(listeners[1].IsInvoked);
            Assert.IsFalse(listeners[3].IsInvoked);
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "0", 0
                                        }
                );
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "2", 2
                                        }
                );

            unit.EPRuntime.SendEvent(new SupportBean(Convert.ToString(2), 2));
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "2", 4
                                        }
                );

            // return
            unit.EPAdministrator.RemoveStatement(statements);

            // send to unisolated
            for (int i = 0; i < count; i++)
            {
                _epService.EPRuntime.SendEvent(
                    new SupportBean(Convert.ToString(i), i));
            }
            EPAssertionUtil.AssertProps(listeners[0].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "0", 0
                                        }
                );
            EPAssertionUtil.AssertProps(listeners[1].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "1", 2
                                        }
                );
            EPAssertionUtil.AssertProps(listeners[2].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "2", 6
                                        }
                );
            EPAssertionUtil.AssertProps(listeners[3].AssertOneGetNewAndReset(),
                                        fields, new Object[]
                                        {
                                            "3", 6
                                        }
                );

            // send to isolated
            for (int i = 0; i < count; i++)
            {
                unit.EPRuntime.SendEvent(
                    new SupportBean(Convert.ToString(i), i));
                Assert.IsFalse(listeners[i].IsInvoked);
            }

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestIsolatedSchedule()
        {
            SendTimerUnisolated(100000);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from pattern [every a=SupportBean -> timer:interval(10)]");

            stmt.Events += _listener.Update;

            SendTimerUnisolated(105000);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            SendTimerIso(0, unit);
            unit.EPAdministrator.AddStatement(stmt);

            SendTimerIso(9999, unit);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimerIso(10000, unit);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(),
                "a.TheString".Split(','), new Object[]
                {
                    "E1"
                }
                );

            SendTimerIso(11000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E2", 1));

            SendTimerUnisolated(120000);
            Assert.IsFalse(_listener.IsInvoked);

            unit.EPAdministrator.RemoveStatement(stmt);

            SendTimerUnisolated(129999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimerUnisolated(130000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(),
                "a.TheString".Split(','), new Object[]
                {
                    "E2"
                }
                );

            SendTimerIso(30000, unit);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestNamedWindowTakeCreate()
        {
            var fields = new String[]
            {
                "TheString"
            };
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(
                "@Name('create') create window MyWindow.win:keepall() as SupportBean");
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(
                "@Name('insert') insert into MyWindow select * from SupportBean");
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(
                "@Name('delete') on SupportBean_A delete from MyWindow where TheString = id");
            EPStatement stmtConsume = _epService.EPAdministrator.CreateEPL(
                "@Name('consume') select irstream * from MyWindow");

            stmtConsume.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    }
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            unit.EPAdministrator.AddStatement(
                _epService.EPAdministrator.GetStatement("create"));

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    }
                }
                );
            Assert.IsFalse(_listener.IsInvoked);

            unit.EPAdministrator.AddStatement(stmtInsert);

            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    },
                    new Object[]
                    {
                        "E5"
                    }
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E5"
                }
                ); // yes receives it

            // Note: Named window is global across isolation units: they are a relation and not a stream.

            // The insert-into to a named window is a stream that can be isolated from the named window.
            // The streams of on-select and on-delete can be isolated, however they select or change the named window even if that is isolated.
            // Consumers to a named window always receive all changes to a named window (regardless of whether the consuming statement is isolated or not), even if the window itself was isolated.
            //
            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E5"
                    }
                }
                );

            unit.EPAdministrator.AddStatement(stmtDelete);

            _epService.EPRuntime.SendEvent(new SupportBean_A("E5"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E5"
                    }
                }
                );

            unit.EPRuntime.SendEvent(new SupportBean_A("E5"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                null);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestNamedWindowTimeCatchup()
        {
            SendTimerUnisolated(100000);
            var fields = new String[]
            {
                "TheString"
            };
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(
                "@Name('create') create window MyWindow.win:time(10) as SupportBean");
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(
                "@Name('insert') insert into MyWindow select * from SupportBean");

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            SendTimerIso(0, unit);
            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmtCreate, stmtInsert
            });

            SendTimerIso(1000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E1", 1));

            SendTimerIso(2000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E2", 2));

            SendTimerIso(9000, unit);
            unit.EPRuntime.SendEvent(new SupportBean("E3", 3));

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    },
                    new Object[]
                    {
                        "E2"
                    },
                    new Object[]
                    {
                        "E3"
                    }
                }
                );
            unit.EPAdministrator.RemoveStatement(new EPStatement[]
            {
                stmtCreate
            });

            SendTimerUnisolated(101000); // equivalent to 10000  (E3 is 1 seconds old)

            SendTimerUnisolated(102000); // equivalent to 11000  (E3 is 2 seconds old)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E2"
                    },
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            SendTimerUnisolated(103000); // equivalent to 12000  (E3 is 3 seconds old)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            SendTimerUnisolated(109000); // equivalent to 18000 (E3 is 9 seconds old)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmtCreate
            }
                );

            SendTimerIso(9999, unit);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            SendTimerIso(10000, unit);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmtCreate.GetEnumerator(), fields,
                null);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestStartStop()
        {
            var fields = new String[]
            {
                "TheString"
            };
            String epl = "select TheString from SupportBean.win:time(60)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            unit.EPAdministrator.AddStatement(stmt);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E2"
                    }
                }
                );

            stmt.Stop();

            unit.EPAdministrator.RemoveStatement(stmt);

            stmt.Start();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, null);

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E4", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            unit.EPAdministrator.AddStatement(stmt);

            _epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    },
                    new Object[]
                    {
                        "E6"
                    }
                }
                );

            unit.EPAdministrator.RemoveStatement(stmt);

            _epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            unit.EPRuntime.SendEvent(new SupportBean("E8", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    },
                    new Object[]
                    {
                        "E6"
                    },
                    new Object[]
                    {
                        "E7"
                    }
                }
                );

            stmt.Stop();

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestSubscriberNamedWindowConsumerIterate()
        {
            _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:keepall() as select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            EPServiceProviderIsolated isolatedService = _epService.GetEPServiceIsolated(
                "isolatedStmts");

            isolatedService.EPRuntime.SendEvent(
                new CurrentTimeEvent(Environment.TickCount));

            var subscriber = new SupportSubscriber();
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBean");

            stmtOne.Subscriber = subscriber;

            EPStatement stmtTwo = isolatedService.EPAdministrator.CreateEPL(
                "select * from MyWindow", null, null);

            isolatedService.EPAdministrator.AddStatement(stmtOne);

            IEnumerator<EventBean> iter = stmtTwo.GetEnumerator();

            while (iter.MoveNext())
            {
                EventBean theEvent = iter.Current;

                isolatedService.EPRuntime.SendEvent(theEvent.Underlying);
            }

            Assert.IsTrue(subscriber.IsInvoked());
        }

        [Test]
        public void TestSuspend()
        {
            SendTimerUnisolated(1000);
            var fields = new String[]
            {
                "TheString"
            };
            String epl = "select irstream TheString from SupportBean.win:time(10)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;

            SendTimerUnisolated(2000);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));

            SendTimerUnisolated(3000);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));

            SendTimerUnisolated(7000);
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));

            SendTimerUnisolated(8000);
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(
                "select 'x' as TheString from pattern [timer:interval(10)]");

            stmtTwo.Events += _listener.Update;

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmt, stmtTwo
            }
                );
            EPAssertionUtil.AssertEqualsAnyOrder(new string[] {stmt.Name, stmtTwo.Name},
                                                 unit.EPAdministrator.StatementNames);
            Assert.AreEqual("i1", stmt.ServiceIsolated);
            Assert.AreEqual("i1", stmt.ServiceIsolated);

            _listener.Reset();
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            SendTimerUnisolated(15000);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    },
                    new Object[]
                    {
                        "E2"
                    },
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            unit.EPAdministrator.RemoveStatement(new EPStatement[]
            {
                stmt, stmtTwo
            }
                );
            EPAssertionUtil.AssertEqualsAnyOrder(new string[0], unit.EPAdministrator.StatementNames);
            Assert.IsNull(stmt.ServiceIsolated);
            Assert.IsNull(stmt.ServiceIsolated);

            SendTimerUnisolated(18999);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E1"
                    },
                    new Object[]
                    {
                        "E2"
                    },
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            SendTimerUnisolated(19000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E2"
                    },
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            SendTimerUnisolated(23999);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                    new Object[]
                    {
                        "E3"
                    }
                }
                );

            SendTimerUnisolated(24000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, null);

            SendTimerUnisolated(25000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "x"
                }
                );

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestUpdate()
        {
            SendTimerUnisolated(5000);
            var fields = new String[]
            {
                "TheString"
            };
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(
                "insert into NewStream select * from SupportBean");
            EPStatement stmtUpd = _epService.EPAdministrator.CreateEPL(
                "Update istream NewStream set TheString = 'X'");
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(
                "select * from NewStream");

            stmtSelect.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] { "X" });

            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");

            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmtSelect
            });

            unit.EPRuntime.SendEvent(new SupportBean());
            Assert.IsFalse(_listener.IsInvoked);

            // Update statements apply to a stream even if the statement is not isolated.
            unit.EPAdministrator.AddStatement(new EPStatement[]
            {
                stmtInsert
            });
            unit.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] { "X" });

            unit.EPAdministrator.AddStatement(stmtUpd);
            unit.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] { "X" });

            stmtUpd.Stop();

            unit.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[] { "E3" });

            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestEventSender()
        {
            EPServiceProviderIsolated unit = _epService.GetEPServiceIsolated("i1");
            EventSender sender = unit.EPRuntime.GetEventSender("SupportBean");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean").Events += _listener.Update;
            sender.SendEvent(new SupportBean());
            var testProp = _listener.IsInvoked;
        }
    }
}