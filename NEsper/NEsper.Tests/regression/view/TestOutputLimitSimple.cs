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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.support;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestOutputLimitSimple
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();

            config.AddEventType("MarketData", typeof (SupportMarketDataBean));
            config.AddEventType("SupportBean", typeof (SupportBean));
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

        #endregion

        private const String JOIN_KEY = "KEY";
        private const String CATEGORY = "Un-aggregated and Un-grouped";

        private EPServiceProvider _epService;
        private long _currentTime;
        private SupportUpdateListener _listener;

        private void RunAssertion34(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;
            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;

            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);

            expected.AddResultInsert(200, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultInsert(1500, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                }
            }
                );
            expected.AddResultInsert(2100, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsert(4300, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 22d
                }
            }
                );
            expected.AddResultRemove(5700, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultRemove(7000, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion15_16(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultInsert(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 22d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, null, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultRemove(7200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion12(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(200, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultInsert(800, 1, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 5000L, 9d
                }
            }
                );
            expected.AddResultInsert(1500, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                }
            }
                );
            expected.AddResultInsert(1500, 2, new Object[][]
            {
                new Object[]
                {
                    "YAH", 10000L, 1d
                }
            }
                );
            expected.AddResultInsert(2100, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsert(3500, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 11000L, 2d
                }
            }
                );
            expected.AddResultInsert(4300, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 22d
                }
            }
                );
            expected.AddResultInsert(4900, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 11500L, 3d
                }
            }
                );
            expected.AddResultRemove(5700, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultInsert(5900, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 10500L, 1d
                }
            }
                );
            expected.AddResultRemove(6300, 0, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 5000L, 9d
                }
            }
                );
            expected.AddResultRemove(7000, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion13_14(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 5000L, 9d
                }
            }
                );
            expected.AddResultInsert(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0,
                new Object[][]
                {
                    new Object[]
                    {
                        "YAH", 11000L, 2d
                    }
                });
            expected.AddResultInsert(5200, 0,
                new Object[][]
                {
                    new Object[]
                    {
                        "YAH", 11500L, 3d
                    }
                });
            expected.AddResultInsRem(6200, 0,
                new Object[][]
                {
                    new Object[]
                    {
                        "YAH", 10500L, 1d
                    }
                },
                new Object[][]
                {
                    new Object[]
                    {
                        "IBM", 100L, 25d
                    }
                });
            expected.AddResultRemove(7200, 0,
                new Object[][]
                {
                    new Object[]
                    {
                        "YAH", 10000L, 1d
                    },
                });

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion78(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultInsert(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsert(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 22d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, null, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultRemove(7200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion56(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                },
                new Object[]
                {
                    "MSFT", 5000L, 9d
                }
            }
                );
            expected.AddResultInsert(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(4200, 0, new Object[][]
            {
                new Object[]
                {
                    "YAH", 11000L, 2d
                }
            }
                );
            expected.AddResultInsert(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 22d
                },
                new Object[]
                {
                    "YAH", 11500L, 3d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, new Object[][]
            {
                new Object[]
                {
                    "YAH", 10500L, 1d
                }
            }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 100L, 25d
                                         }
                                     }
                );
            expected.AddResultRemove(7200, 0, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 5000L, 9d
                },
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion17(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(200, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultInsert(1500, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 24d
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsert(3500, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 11000L, 2d
                }
            }
                );
            expected.AddResultInsert(4300, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 150L, 22d
                }
            }
                );
            expected.AddResultRemove(5700, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                }
            }
                );
            expected.AddResultRemove(6300, 0, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 5000L, 9d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion18(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "volume", "price"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsert(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                },
                new Object[]
                {
                    "MSFT", 5000L, 9d
                }
            }
                );
            expected.AddResultInsert(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                },
                new Object[]
                {
                    "MSFT", 5000L, 9d
                },
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsert(3200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                },
                new Object[]
                {
                    "MSFT", 5000L, 9d
                },
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                }
            }
                );
            expected.AddResultInsert(4200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                },
                new Object[]
                {
                    "MSFT", 5000L, 9d
                },
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                },
                new Object[]
                {
                    "YAH", 11000L, 2d
                }
            }
                );
            expected.AddResultInsert(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 100L, 25d
                },
                new Object[]
                {
                    "MSFT", 5000L, 9d
                },
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                },
                new Object[]
                {
                    "YAH", 11000L, 2d
                },
                new Object[]
                {
                    "IBM", 150L, 22d
                },
                new Object[]
                {
                    "YAH", 11500L, 3d
                }
            }
                );
            expected.AddResultInsert(6200, 0, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 5000L, 9d
                },
                new Object[]
                {
                    "IBM", 150L, 24d
                },
                new Object[]
                {
                    "YAH", 10000L, 1d
                },
                new Object[]
                {
                    "IBM", 155L, 26d
                },
                new Object[]
                {
                    "YAH", 11000L, 2d
                },
                new Object[]
                {
                    "IBM", 150L, 22d
                },
                new Object[]
                {
                    "YAH", 11500L, 3d
                },
                new Object[]
                {
                    "YAH", 10500L, 1d
                }
            }
                );
            expected.AddResultInsert(7200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 155L, 26d
                },
                new Object[]
                {
                    "YAH", 11000L, 2d
                },
                new Object[]
                {
                    "IBM", 150L, 22d
                },
                new Object[]
                {
                    "YAH", 11500L, 3d
                },
                new Object[]
                {
                    "YAH", 10500L, 1d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private SupportUpdateListener CreateStmtAndListenerNoJoin(String viewExpr)
        {
            _epService.Initialize();
            var updateListener = new SupportUpdateListener();
            var view = _epService.EPAdministrator.CreateEPL(viewExpr);
            view.Events += updateListener.Update;

            return updateListener;
        }

        private void RunAssertAll(SupportUpdateListener updateListener)
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
            Assert.AreEqual(2L, updateListener.LastNewData[1].Get("LongBoxed"));
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendEvent(long longBoxed, int intBoxed = 0, short shortBoxed = (short) 0)
        {
            var bean = new SupportBean();

            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private SupportUpdateListener CreateStmtAndListenerJoin(String viewExpr)
        {
            _epService.Initialize();

            var updateListener = new SupportUpdateListener();
            EPStatement view = _epService.EPAdministrator.CreateEPL(viewExpr);

            view.Events += updateListener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

            return updateListener;
        }

        private void RunAssertLast(SupportUpdateListener updateListener)
        {
            // send an event
            SendEvent(1);

            // check no Update
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // send another event
            SendEvent(2);

            // check Update, only the last event present
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.AreEqual(2L, updateListener.LastNewData[0].Get("LongBoxed"));
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendTimer(long time)
        {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = _epService.EPRuntime;

            runtime.SendEvent(theEvent);
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

        private void TimeCallback(String statementString, int timeToCallback)
        {
            // clear any old events
            _epService.Initialize();

            // set the clock to 0
            _currentTime = 0;
            SendTimeEvent(0);

            // create the EPL statement and add a listener
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                statementString);
            var updateListener = new SupportUpdateListener();

            statement.Events += updateListener.Update;
            updateListener.Reset();

            // send an event
            SendEvent("IBM");

            // check that the listener hasn't been updated
            SendTimeEvent(timeToCallback - 1);
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update the clock
            SendTimeEvent(timeToCallback);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);

            // send another event
            SendEvent("MSFT");

            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update the clock
            SendTimeEvent(timeToCallback);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);

            // don't send an event
            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update the clock
            SendTimeEvent(timeToCallback);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.IsNull(updateListener.LastNewData);
            Assert.IsNull(updateListener.LastOldData);

            // don't send an event
            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update the clock
            SendTimeEvent(timeToCallback);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.IsNull(updateListener.LastNewData);
            Assert.IsNull(updateListener.LastOldData);

            // send several events
            SendEvent("YAH");
            SendEvent("s4");
            SendEvent("s5");

            // check that the listener hasn't been updated
            Assert.IsFalse(updateListener.GetAndClearIsInvoked());

            // Update the clock
            SendTimeEvent(timeToCallback);

            // check that the listener has been updated
            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            Assert.AreEqual(3, updateListener.LastNewData.Length);
            Assert.IsNull(updateListener.LastOldData);
        }

        private void SendTimeEvent(int timeIncrement)
        {
            _currentTime += timeIncrement;
            var theEvent = new CurrentTimeEvent(_currentTime);

            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendCurrentTime(String time)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time)));
        }

        private void SendCurrentTimeWithMinus(String time, long minus)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeHelper.ParseDefaultMSec(time) - minus));
        }

        private void SendJoinEvents(String s)
        {
            var event1 = new SupportBean();

            event1.TheString = s;
            event1.DoubleBoxed = 0.0;
            event1.IntPrimitive = 0;
            event1.IntBoxed = 0;

            var event2 = new SupportBean_A(s);

            _epService.EPRuntime.SendEvent(event1);
            _epService.EPRuntime.SendEvent(event2);
        }

        private void SendMDEvent(String symbol, long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume,
                                                 null);

            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(String symbol, double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L,
                                                 null);

            _epService.EPRuntime.SendEvent(bean);
        }

        [Test]
        public void Test10AllNoHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "output all every 1 seconds";

            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void Test11AllHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) " + "having price > 10"
                              + "output all every 1 seconds";

            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test12AllHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "having price > 10" + "output all every 1 seconds";

            RunAssertion78(stmtText, "all");
        }

        [Test]
        public void Test13LastNoHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec)"
                              + "output last every 1 seconds";

            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test14LastNoHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "output last every 1 seconds";

            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test15LastHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec)" + "having price > 10 "
                              + "output last every 1 seconds";

            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test16LastHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "having price > 10 " + "output last every 1 seconds";

            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test17FirstNoHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) "
                              + "output first every 1 seconds";

            RunAssertion17(stmtText, "first");
        }

        [Test]
        public void Test18SnapshotNoHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) "
                              + "output snapshot every 1 seconds";

            RunAssertion18(stmtText, "first");
        }

        [Test]
        public void Test1NoneNoHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec)";

            RunAssertion12(stmtText, "none");
        }

        [Test]
        public void Test2NoneNoHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol";

            RunAssertion12(stmtText, "none");
        }

        [Test]
        public void Test3NoneHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) " + " having price > 10";

            RunAssertion34(stmtText, "none");
        }

        [Test]
        public void Test4NoneHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + " having price > 10";

            RunAssertion34(stmtText, "none");
        }

        [Test]
        public void Test5DefaultNoHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) "
                              + "output every 1 seconds";

            RunAssertion56(stmtText, "default");
        }

        [Test]
        public void Test6DefaultNoHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "output every 1 seconds";

            RunAssertion56(stmtText, "default");
        }

        [Test]
        public void Test7DefaultHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) \n" + "having price > 10"
                              + "output every 1 seconds";

            RunAssertion78(stmtText, "default");
        }

        [Test]
        public void Test8DefaultHavingJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "having price > 10" + "output every 1 seconds";

            RunAssertion78(stmtText, "default");
        }

        [Test]
        public void Test9AllNoHavingNoJoin()
        {
            String stmtText = "select symbol, volume, price "
                              + "from MarketData.win:time(5.5 sec) "
                              + "output all every 1 seconds";

            RunAssertion56(stmtText, "all");
        }

        [Test]
        public void TestAggAllHaving()
        {
            String stmtText = "select symbol, volume " + "from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as two " + "having volume > 0 "
                              + "output every 5 events";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();

            stmt.Events += listener.Update;
            var fields = new String[]
            {
                "symbol", "volume"
            };

            SendMDEvent("S0", 20);
            SendMDEvent("IBM", -1);
            SendMDEvent("MSFT", -2);
            SendMDEvent("YAH", 10);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent("IBM", 0);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "S0", 20L
                                                  },
                                                  new Object[]
                                                  {
                                                      "YAH", 10L
                                                  }
                                              }
                );
            listener.Reset();
        }

        [Test]
        public void TestAggAllHavingJoin()
        {
            String stmtText = "select symbol, volume " + "from "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(10) as one," + typeof (SupportBean).FullName
                              + ".win:length(10) as two " + "where one.symbol=two.TheString "
                              + "having volume > 0 " + "output every 5 events";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();

            stmt.Events += listener.Update;
            var fields = new String[]
            {
                "symbol", "volume"
            }
                ;

            _epService.EPRuntime.SendEvent(new SupportBean("S0", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("IBM", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("MSFT", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("YAH", 0));

            SendMDEvent("S0", 20);
            SendMDEvent("IBM", -1);
            SendMDEvent("MSFT", -2);
            SendMDEvent("YAH", 10);
            Assert.IsFalse(listener.IsInvoked);

            SendMDEvent("IBM", 0);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "S0", 20L
                                                  },
                                                  new Object[]
                                                  {
                                                      "YAH", 10L
                                                  }
                                              }
                );
            listener.Reset();
        }

        [Test]
        public void TestIterator()
        {
            var fields = new String[]
            {
                "symbol", "price"
            }
                ;
            String statementString = "select symbol, TheString, price from "
                                     + typeof (SupportMarketDataBean).FullName
                                     + ".win:length(10) as one, " + typeof (SupportBeanString).FullName
                                     + ".win:length(100) as two "
                                     + "where one.symbol = two.TheString " + "output every 3 events";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                statementString);

            _epService.EPRuntime.SendEvent(new SupportBeanString("CAT"));
            _epService.EPRuntime.SendEvent(new SupportBeanString("IBM"));

            // Output limit clause ignored when iterating, for both joins and no-join
            SendEvent("CAT", 50);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "CAT", 50d
                                                  }
                                              }
                );

            SendEvent("CAT", 60);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                                                      new Object[][]
                                                      {
                                                          new Object[]
                                                          {
                                                              "CAT", 50d
                                                          },
                                                          new Object[]
                                                          {
                                                              "CAT", 60d
                                                          }
                                                      }
                );

            SendEvent("IBM", 70);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                                                      new Object[][]
                                                      {
                                                          new Object[]
                                                          {
                                                              "CAT", 50d
                                                          },
                                                          new Object[]
                                                          {
                                                              "CAT", 60d
                                                          },
                                                          new Object[]
                                                          {
                                                              "IBM", 70d
                                                          }
                                                      }
                );

            SendEvent("IBM", 90);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields,
                                                      new Object[][]
                                                      {
                                                          new Object[]
                                                          {
                                                              "CAT", 50d
                                                          },
                                                          new Object[]
                                                          {
                                                              "CAT", 60d
                                                          },
                                                          new Object[]
                                                          {
                                                              "IBM", 70d
                                                          },
                                                          new Object[]
                                                          {
                                                              "IBM", 90d
                                                          }
                                                      }
                );
        }

        [Test]
        public void TestLimitEventJoin()
        {
            String eventName1 = typeof (SupportBean).FullName;
            String eventName2 = typeof (SupportBean_A).FullName;
            String joinStatement = "select * from " + eventName1
                                   + ".win:length(5) as event1," + eventName2
                                   + ".win:length(5) as event2"
                                   + " where event1.TheString = event2.id";
            String outputStmt1 = joinStatement + " output every 1 events";
            String outputStmt3 = joinStatement + " output every 3 events";

            EPStatement fireEvery1 = _epService.EPAdministrator.CreateEPL(
                outputStmt1);
            EPStatement fireEvery3 = _epService.EPAdministrator.CreateEPL(
                outputStmt3);

            var updateListener1 = new SupportUpdateListener();

            fireEvery1.Events += updateListener1.Update;
            var updateListener3 = new SupportUpdateListener();

            fireEvery3.Events += updateListener3.Update;

            // send event 1
            SendJoinEvents("IBM");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
            Assert.IsNull(updateListener3.LastNewData);
            Assert.IsNull(updateListener3.LastOldData);

            // send event 2
            SendJoinEvents("MSFT");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
            Assert.IsNull(updateListener3.LastNewData);
            Assert.IsNull(updateListener3.LastOldData);

            // send event 3
            SendJoinEvents("YAH");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsTrue(updateListener3.GetAndClearIsInvoked());
            Assert.AreEqual(3, updateListener3.LastNewData.Length);
            Assert.IsNull(updateListener3.LastOldData);
        }

        [Test]
        public void TestLimitEventSimple()
        {
            var updateListener1 = new SupportUpdateListener();
            var updateListener2 = new SupportUpdateListener();
            var updateListener3 = new SupportUpdateListener();

            String eventName = typeof (SupportBean).FullName;
            String selectStmt = "select * from " + eventName + ".win:length(5)";
            String statement1 = selectStmt + " output every 1 events";
            String statement2 = selectStmt + " output every 2 events";
            String statement3 = selectStmt + " output every 3 events";

            EPStatement rateLimitStmt1 = _epService.EPAdministrator.CreateEPL(
                statement1);

            rateLimitStmt1.Events += updateListener1.Update;
            EPStatement rateLimitStmt2 = _epService.EPAdministrator.CreateEPL(
                statement2);

            rateLimitStmt2.Events += updateListener2.Update;
            EPStatement rateLimitStmt3 = _epService.EPAdministrator.CreateEPL(
                statement3);

            rateLimitStmt3.Events += updateListener3.Update;

            // send event 1
            SendEvent("IBM");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener2.GetAndClearIsInvoked());
            Assert.IsNull(updateListener2.LastNewData);
            Assert.IsNull(updateListener2.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());
            Assert.IsNull(updateListener3.LastNewData);
            Assert.IsNull(updateListener3.LastOldData);

            // send event 2
            SendEvent("MSFT");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsTrue(updateListener2.GetAndClearIsInvoked());
            Assert.AreEqual(2, updateListener2.LastNewData.Length);
            Assert.IsNull(updateListener2.LastOldData);

            Assert.IsFalse(updateListener3.GetAndClearIsInvoked());

            // send event 3
            SendEvent("YAH");

            Assert.IsTrue(updateListener1.GetAndClearIsInvoked());
            Assert.AreEqual(1, updateListener1.LastNewData.Length);
            Assert.IsNull(updateListener1.LastOldData);

            Assert.IsFalse(updateListener2.GetAndClearIsInvoked());

            Assert.IsTrue(updateListener3.GetAndClearIsInvoked());
            Assert.AreEqual(3, updateListener3.LastNewData.Length);
            Assert.IsNull(updateListener3.LastOldData);
        }

        [Test]
        public void TestLimitSnapshot()
        {
            var listener = new SupportUpdateListener();

            SendTimer(0);
            String selectStmt = "select * from " + typeof (SupportBean).FullName
                                + ".win:time(10) output snapshot every 3 events";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(selectStmt);

            stmt.Events += listener.Update;

            SendTimer(1000);
            SendEvent("IBM");
            SendEvent("MSFT");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(2000);
            SendEvent("YAH");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM"
                                                  },
                                                  new Object[]
                                                  {
                                                      "MSFT"
                                                  },
                                                  new Object[]
                                                  {
                                                      "YAH"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(3000);
            SendEvent("s4");
            SendEvent("s5");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(10000);
            SendEvent("s6");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM"
                                                  },
                                                  new Object[]
                                                  {
                                                      "MSFT"
                                                  },
                                                  new Object[]
                                                  {
                                                      "YAH"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s4"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s5"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s6"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(11000);
            SendEvent("s7");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent("s8");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent("s9");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "YAH"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s4"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s5"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s6"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s7"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s8"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s9"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(14000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s6"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s7"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s8"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s9"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent("s10");
            SendEvent("s11");
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(23000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s10"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s11"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent("s12");
            Assert.IsFalse(listener.IsInvoked);
        }

        [Test]
        public void TestLimitSnapshotJoin()
        {
            var listener = new SupportUpdateListener();

            SendTimer(0);
            String selectStmt = "select TheString from "
                                + typeof (SupportBean).FullName + ".win:time(10) as s,"
                                + typeof (SupportMarketDataBean).FullName
                                +
                                ".win:keepall() as m where s.TheString = m.symbol output snapshot every 3 events order by symbol asc";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(selectStmt);

            stmt.Events += listener.Update;

            foreach (String symbol in "s0,s1,s2,s3,s4,s5,s6,s7,s8,s9,s10,s11".Split(','))
            {
                _epService.EPRuntime.SendEvent(
                    new SupportMarketDataBean(symbol, 0, 0L, ""));
            }

            SendTimer(1000);
            SendEvent("s0");
            SendEvent("s1");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(2000);
            SendEvent("s2");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s0"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s1"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s2"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(3000);
            SendEvent("s4");
            SendEvent("s5");
            Assert.IsFalse(listener.GetAndClearIsInvoked());

            SendTimer(10000);
            SendEvent("s6");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s0"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s1"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s2"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s4"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s5"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s6"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(11000);
            SendEvent("s7");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent("s8");
            Assert.IsFalse(listener.IsInvoked);

            SendEvent("s9");
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s2"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s4"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s5"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s6"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s7"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s8"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s9"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendTimer(14000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s6"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s7"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s8"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s9"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent("s10");
            SendEvent("s11");
            Assert.IsFalse(listener.IsInvoked);

            SendTimer(23000);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData,
                                              new String[]
                                              {
                                                  "TheString"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "s10"
                                                  },
                                                  new Object[]
                                                  {
                                                      "s11"
                                                  }
                                              }
                );
            Assert.IsNull(listener.LastOldData);
            listener.Reset();

            SendEvent("s12");
            Assert.IsFalse(listener.IsInvoked);
        }

        [Test]
        public void TestLimitTime()
        {
            String eventName = typeof (SupportBean).FullName;
            String selectStatement = "select * from " + eventName + ".win:length(5)";

            // test integer seconds
            String statementString1 = selectStatement + " output every 3 seconds";

            TimeCallback(statementString1, 3000);

            // test fractional seconds
            String statementString2 = selectStatement + " output every 3.3 seconds";

            TimeCallback(statementString2, 3300);

            // test integer minutes
            String statementString3 = selectStatement + " output every 2 minutes";

            TimeCallback(statementString3, 120000);

            // test fractional minutes
            String statementString4 = "select * from " + eventName
                                      + ".win:length(5)" + " output every .05 minutes";

            TimeCallback(statementString4, 3000);
        }

        [Test]
        public void TestOutputEveryTimePeriod()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));

            String stmtText =
                "select symbol from MarketData.win:keepall() output snapshot every 1 day 2 hours 3 minutes 4 seconds 5 milliseconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;
            SendMDEvent("E1", 0);

            long deltaSec = 26*60*60 + 3*60 + 4;
            long deltaMSec = deltaSec*1000 + 5 + 2000;

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec - 1));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec));
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("symbol"));
        }

        [Test]
        public void TestOutputEveryTimePeriodVariable()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            _epService.EPAdministrator.Configuration.AddVariable("D",
                                                                 typeof (int), 1);
            _epService.EPAdministrator.Configuration.AddVariable("H",
                                                                 typeof (int), 2);
            _epService.EPAdministrator.Configuration.AddVariable("M",
                                                                 typeof (int), 3);
            _epService.EPAdministrator.Configuration.AddVariable("S",
                                                                 typeof (int), 4);
            _epService.EPAdministrator.Configuration.AddVariable("MS",
                                                                 typeof (int), 5);

            String stmtText =
                "select symbol from MarketData.win:keepall() output snapshot every D days H hours M minutes S seconds MS milliseconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;
            SendMDEvent("E1", 0);

            long deltaSec = 26*60*60 + 3*60 + 4;
            long deltaMSec = deltaSec*1000 + 5 + 2000;

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec - 1));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(deltaMSec));
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("symbol"));

            // test statement model
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(
                stmtText);

            Assert.AreEqual(stmtText, model.ToEPL());
        }

        [Test]
        public void TestSimpleJoinAll()
        {
            String viewExpr = "select LongBoxed  " + "from "
                              + typeof (SupportBeanString).FullName + ".win:length(3) as one, "
                              + typeof (SupportBean).FullName + ".win:length(3) as two "
                              + "output all every 2 events";

            RunAssertAll(CreateStmtAndListenerJoin(viewExpr));
        }

        [Test]
        public void TestSimpleJoinLast()
        {
            String viewExpr = "select LongBoxed " + "from "
                              + typeof (SupportBeanString).FullName + ".win:length(3) as one, "
                              + typeof (SupportBean).FullName + ".win:length(3) as two "
                              + "output last every 2 events";

            RunAssertLast(CreateStmtAndListenerJoin(viewExpr));
        }

        [Test]
        public void TestSimpleNoJoinAll()
        {
            String viewExpr = "select LongBoxed " + "from "
                              + typeof (SupportBean).FullName + ".win:length(3) "
                              + "output all every 2 events";

            RunAssertAll(CreateStmtAndListenerNoJoin(viewExpr));

            viewExpr = "select LongBoxed " + "from " + typeof (SupportBean).FullName
                       + ".win:length(3) " + "output every 2 events";

            RunAssertAll(CreateStmtAndListenerNoJoin(viewExpr));

            viewExpr = "select * " + "from " + typeof (SupportBean).FullName
                       + ".win:length(3) " + "output every 2 events";

            RunAssertAll(CreateStmtAndListenerNoJoin(viewExpr));
        }

        [Test]
        public void TestSimpleNoJoinLast()
        {
            String viewExpr = "select LongBoxed " + "from "
                              + typeof (SupportBean).FullName + ".win:length(3) "
                              + "output last every 2 events";

            RunAssertLast(CreateStmtAndListenerNoJoin(viewExpr));

            viewExpr = "select * " + "from " + typeof (SupportBean).FullName
                       + ".win:length(3) " + "output last every 2 events";

            RunAssertLast(CreateStmtAndListenerNoJoin(viewExpr));
        }

        [Test]
        public void TestTimeBatchOutputEvents()
        {
            String stmtText = "select * from " + typeof (SupportBean).FullName
                              + ".win:time_batch(10 seconds) output every 10 seconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
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
            EventBean[] newEvents = listener.GetAndResetLastNewData();

            Assert.AreEqual(1, newEvents.Length);
            Assert.AreEqual("e1", newEvents[0].Get("TheString"));
            listener.Reset();

            SendTimer(50000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            SendTimer(60000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            SendTimer(70000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();

            SendEvent("e2");
            SendEvent("e3");
            SendTimer(80000);
            newEvents = listener.GetAndResetLastNewData();
            Assert.AreEqual(2, newEvents.Length);
            Assert.AreEqual("e2", newEvents[0].Get("TheString"));
            Assert.AreEqual("e3", newEvents[1].Get("TheString"));

            SendTimer(90000);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();
        }

        [Test]
        public void TestSnapshotMonthScoped()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime("2002-02-01T9:00:00.000");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean.std:lastevent() output snapshot every 1 month").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendCurrentTime("2002-03-01T9:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','), new Object[][] { new object[] {"E1"}});
        }

        [Test]
        public void TestFirstMonthScoped()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime("2002-02-01T9:00:00.000");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean.std:lastevent() output first every 1 month").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendCurrentTime("2002-03-01T9:00:00.000");
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','), new Object[][] { new object[] { "E4" } });
        }
    }
}
