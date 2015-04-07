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
    public class TestDataWindowUnionExpiry
    {
        #region Setup/Teardown

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        #endregion

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void RunAssertionFirstUniqueAndFirstLength(EPStatement stmt)
        {
            var fields = new String[]
            {
                "TheString", "IntPrimitive"
            };

            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1", 1
                });

            SendEvent("E1", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 2
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1", 2
                });

            SendEvent("E2", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 2
                        }
                        ,
                        new Object[]
                        {
                            "E2", 1
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2", 1
                });

            SendEvent("E2", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 2
                        }
                        ,
                        new Object[]
                        {
                            "E2", 1
                        }
                });
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 2
                        }
                        ,
                        new Object[]
                        {
                            "E2", 1
                        }
                        ,
                        new Object[]
                        {
                            "E3", 3
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3", 3
                });

            SendEvent("E3", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 2
                        }
                        ,
                        new Object[]
                        {
                            "E2", 1
                        }
                        ,
                        new Object[]
                        {
                            "E3", 3
                        }
                });
            Assert.IsFalse(_listener.GetAndClearIsInvoked());
        }

        private void RunAssertionTimeWinUnique(EPStatement stmt)
        {
            var fields = new String[]
            {
                "TheString"
            };

            SendTimer(1000);
            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendTimer(2000);
            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            SendTimer(3000);
            SendEvent("E3", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );

            SendTimer(4000);
            SendEvent("E4", 3);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E4"
                }
                );
            SendEvent("E5", 1);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E5"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2", "E3", "E4", "E5"));
            SendEvent("E6", 3);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E6"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2", "E3", "E4", "E5", "E6"));

            SendTimer(5000);
            SendEvent("E7", 4);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E7"
                }
                );
            SendEvent("E8", 4);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E8"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8"));

            SendTimer(6000);
            SendEvent("E9", 4);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E9"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));

            SendTimer(10999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(11000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9"));

            SendTimer(12999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(13000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E2", "E4", "E5", "E6", "E7", "E8", "E9"));

            SendTimer(14000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E4"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E2", "E5", "E6", "E7", "E8", "E9"));

            SendTimer(15000);
            EPAssertionUtil.AssertProps(
                _listener.LastOldData[0], fields,
                new Object[]
                {
                    "E7"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.LastOldData[1], fields,
                new Object[]
                {
                    "E8"
                }
                );
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E2", "E5", "E6", "E9"));

            SendTimer(1000000);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E2", "E5", "E6", "E9"));
        }

        private void SendEvent(String theString, int intPrimitive, int intBoxed, double doublePrimitive)
        {
            var bean = new SupportBean();

            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            bean.DoublePrimitive = doublePrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(String theString, int intPrimitive, int intBoxed)
        {
            var bean = new SupportBean();

            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(String theString, int intPrimitive)
        {
            var bean = new SupportBean();

            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private Object[][] ToArr(params Object[] values)
        {
            var arr = new Object[values.Length][];

            for (int i = 0; i < values.Length; i++)
            {
                arr[i] = new Object[]
                {
                    values[i]
                };
            }
            return arr;
        }

        private void TryInvalid(String text, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(text);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;

            runtime.SendEvent(theEvent);
        }

        private void Init(bool isAllowMultipleDataWindows)
        {
            _listener = new SupportUpdateListener();

            Configuration config = SupportConfigFactory.GetConfiguration();

            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = isAllowMultipleDataWindows;

            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean", typeof (SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean_S0", typeof (SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean_S1", typeof (SupportBean_S1));
        }

        [Test]
        public void TestBatchWindow()
        {
            Init(false);
            var fields = new String[]
            {
                "TheString"
            };

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream TheString from SupportBean.win:length_batch(3).std:unique(IntPrimitive) retain-union");

            stmt.Events += _listener.Update;

            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            SendEvent("E3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );

            SendEvent("E4", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E4"
                }
                );

            SendEvent("E5", 4);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E5"
                }
                );

            SendEvent("E6", 4); // remove stream is E1, E2, E3
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E6"
                }
                );

            SendEvent("E7", 5);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E4", "E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E7"
                }
                );

            SendEvent("E8", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E5", "E4", "E6", "E7", "E8"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E8"
                }
                );

            SendEvent("E9", 7); // remove stream is E4, E5, E6; E4 and E5 get removed as their
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E6", "E7", "E8", "E9"));
            EPAssertionUtil.AssertPropsPerRow(
                _listener.LastOldData, fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E4"
                        }
                        ,
                        new Object[]
                        {
                            "E5"
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E9"
                });
            _listener.Reset();
        }

        [Test]
        public void TestFirstUniqueAndFirstLength()
        {
            Init(false);

            String epl =
                "select irstream TheString, IntPrimitive from SupportBean.win:firstlength(3).std:firstunique(TheString) retain-union";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;

            RunAssertionFirstUniqueAndFirstLength(stmt);

            stmt.Dispose();
            _listener.Reset();

            epl = "select irstream TheString, IntPrimitive from SupportBean.std:firstunique(TheString).win:firstlength(3) retain-union";
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            RunAssertionFirstUniqueAndFirstLength(stmt);
        }

        [Test]
        public void TestFirstUniqueAndLengthOnDelete()
        {
            Init(false);

            EPStatement nwstmt = _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.std:firstunique(TheString).win:firstlength(3) retain-union as SupportBean");

            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 delete from MyWindow where TheString = p00");

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream * from MyWindow");

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "TheString", "IntPrimitive"
            };

            SendEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                nwstmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1", 1
                });

            SendEvent("E1", 99);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                nwstmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 99
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1", 99
                });

            SendEvent("E2", 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                nwstmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 1
                        }
                        ,
                        new Object[]
                        {
                            "E1", 99
                        }
                        ,
                        new Object[]
                        {
                            "E2", 2
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2", 2
                });

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                nwstmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E2", 2
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.LastOldData[0],
                "TheString".Split(','), 
                new Object[]
                {
                    "E1"
                });
            EPAssertionUtil.AssertProps(
                _listener.LastOldData[1],
                "TheString".Split(','),
                new Object[]
                {
                    "E1"
                });
            _listener.Reset();

            SendEvent("E1", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                nwstmt.GetEnumerator(), fields,
                new Object[][]
                {
                        new Object[]
                        {
                            "E1", 3
                        }
                        ,
                        new Object[]
                        {
                            "E2", 2
                        }
                });
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1", 3
                });
        }

        [Test]
        public void TestInvalid()
        {
            Init(false);
            String text = null;

            text =
                "select TheString from SupportBean.std:groupwin(TheString).std:unique(TheString).std:merge(IntPrimitive) retain-union";
            TryInvalid(text,
                       "Error starting statement: Error attaching view to parent view: Groupwin view for this merge view could not be found among parent views [select TheString from SupportBean.std:groupwin(TheString).std:unique(TheString).std:merge(IntPrimitive) retain-union]");

            text =
                "select TheString from SupportBean.std:groupwin(TheString).std:groupwin(IntPrimitive).std:unique(TheString).std:unique(IntPrimitive) retain-union";
            TryInvalid(text,
                       "Error starting statement: Multiple groupwin views are not allowed in conjuntion with multiple data windows [select TheString from SupportBean.std:groupwin(TheString).std:groupwin(IntPrimitive).std:unique(TheString).std:unique(IntPrimitive) retain-union]");
        }

        [Test]
        public void TestUnionAndDerivedValue()
        {
            Init(false);
            var fields = new String[]
            {
                "total"
            }
                ;

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBean.std:unique(IntPrimitive).std:unique(IntBoxed).stat:uni(DoublePrimitive) retain-union");

            stmt.Events += _listener.Update;

            SendEvent("E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr(100d));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    100d
                }
                );

            SendEvent("E2", 2, 20, 50d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr(150d));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    150d
                }
                );

            SendEvent("E3", 1, 20, 20d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr(170d));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    170d
                }
                );
        }

        [Test]
        public void TestUnionGroupBy()
        {
            Init(false);
            var fields = new String[]
            {
                "TheString"
            }
                ;

            String text =
                "select irstream TheString from SupportBean.std:groupwin(IntPrimitive).win:length(2).std:unique(IntBoxed) retain-union";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);

            stmt.Events += _listener.Update;

            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendEvent("E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            SendEvent("E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );

            SendEvent("E4", 1, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E4"
                }
                );

            SendEvent("E5", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E5"
                }
                );

            SendEvent("E6", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E6"
                }
                );
            _listener.Reset();

            SendEvent("E7", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E2", "E4", "E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E1"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E7"
                }
                );
            _listener.Reset();

            SendEvent("E8", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E4", "E5", "E6", "E7", "E8"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E2"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E8"
                }
                );
            _listener.Reset();
        }

        [Test]
        public void TestUnionPattern()
        {
            Init(false);
            var fields = new String[]
            {
                "string"
            }
                ;

            String text =
                "select irstream a.p00||b.p10 as string from pattern [every a=SupportBean_S0 -> b=SupportBean_S1].std:unique(a.id).std:unique(b.id) retain-union";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1E2"
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(20, "E4"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1E2", "E3E4"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3E4"
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2, "E6"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E3E4", "E5E6"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E1E2"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E5E6"
                }
                );
        }

        [Test]
        public void TestUnionSorted()
        {
            Init(false);
            var fields = new String[]
            {
                "TheString"
            }
                ;

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream TheString from SupportBean.ext:sort(2, IntPrimitive).ext:sort(2, IntBoxed) retain-union");

            stmt.Events += _listener.Update;

            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendEvent("E2", 2, 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            SendEvent("E3", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );

            SendEvent("E4", -1, -1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E3", "E4"));
            Assert.AreEqual(2, _listener.LastOldData.Length);
            Object[] result =
                {
                    _listener.LastOldData[0].Get("TheString"),
                    _listener.LastOldData[1].Get("TheString")
                }
                ;

            EPAssertionUtil.AssertEqualsAnyOrder(result, new String[]
            {
                "E1", "E2"
            }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E4"
                }
                );
            _listener.Reset();

            SendEvent("E5", 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E3", "E4"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E5"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E5"
                }
                );
            _listener.Reset();

            SendEvent("E6", 0, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E4", "E6"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E6"
                }
                );
            _listener.Reset();
        }

        [Test]
        public void TestUnionSubselect()
        {
            Init(false);

            String text =
                "select * from SupportBean_S0 where p00 in (select TheString from SupportBean.win:length(2).std:unique(IntPrimitive) retain-union)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);

            stmt.Events += _listener.Update;

            SendEvent("E1", 1);
            SendEvent("E2", 2);
            SendEvent("E3", 3);
            SendEvent("E4", 2); // throws out E1
            SendEvent("E5", 1); // retains E3

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E2"));
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E3"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E4"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E5"));
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
        }

        [Test]
        public void TestUnionThreeUnique()
        {
            Init(false);
            var fields = new String[]
            {
                "TheString"
            }
                ;

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream TheString from SupportBean.std:unique(IntPrimitive).std:unique(IntBoxed).std:unique(DoublePrimitive) retain-union");

            stmt.Events += _listener.Update;

            SendEvent("E1", 1, 10, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendEvent("E2", 2, 10, 200d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            SendEvent("E3", 2, 20, 100d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );

            SendEvent("E4", 1, 30, 300d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E2", "E3", "E4"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E1"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E4"
                }
                );
            _listener.Reset();
        }

        [Test]
        public void TestUnionTimeWin()
        {
            Init(false);

            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream TheString from SupportBean.std:unique(IntPrimitive).win:time(10 sec) retain-union");

            stmt.Events += _listener.Update;

            RunAssertionTimeWinUnique(stmt);
        }

        [Test]
        public void TestUnionTimeWinNamedWindow()
        {
            Init(false);

            SendTimer(0);
            EPStatement stmtWindow = _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:time(10 sec).std:unique(IntPrimitive) retain-union as select * from SupportBean");

            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 delete from MyWindow where IntBoxed = id");
            stmtWindow.Events += _listener.Update;

            RunAssertionTimeWinUnique(stmtWindow);
        }

        [Test]
        public void TestUnionTimeWinNamedWindowDelete()
        {
            Init(false);

            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:time(10 sec).std:unique(IntPrimitive) retain-union as select * from SupportBean");

            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 delete from MyWindow where IntBoxed = id");
            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "TheString"
            }
                ;

            SendTimer(1000);
            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendTimer(2000);
            SendEvent("E2", 2, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1"));

            SendTimer(3000);
            SendEvent("E3", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            SendEvent("E4", 3, 40);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E3", "E4"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E4"
                }
                );

            SendTimer(4000);
            SendEvent("E5", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E3", "E4", "E5"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E5"
                }
                );
            SendEvent("E6", 4, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E3", "E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E6"
                }
                );

            _epService.EPRuntime.SendEvent(new SupportBean_S0(20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E3", "E4", "E5", "E6"));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(50));
            Assert.AreEqual(2, _listener.LastOldData.Length);
            Object[] result =
                {
                    _listener.LastOldData[0].Get("TheString"),
                    _listener.LastOldData[1].Get("TheString")
                }
                ;

            EPAssertionUtil.AssertEqualsAnyOrder(result, new String[]
            {
                "E5", "E6"
            }
                );
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E3", "E4"));

            SendTimer(12999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(13000);
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOldAndReset(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E1", "E4"));

            SendTimer(10000000);
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestUnionTimeWinSODA()
        {
            Init(false);

            SendTimer(0);
            String stmtText =
                "select irstream TheString from SupportBean.win:time(10 seconds).std:unique(IntPrimitive) retain-union";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(
                stmtText);

            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = _epService.EPAdministrator.Create(model);

            stmt.Events += _listener.Update;

            RunAssertionTimeWinUnique(stmt);
        }

        [Test]
        public void TestUnionTwoUnique()
        {
            Init(false);
            var fields = new String[]
            {
                "TheString"
            }
                ;

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select irstream TheString from SupportBean.std:unique(IntPrimitive).std:unique(IntBoxed) retain-union");

            stmt.Events += _listener.Update;

            SendEvent("E1", 1, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E1"
                }
                );

            SendEvent("E2", 2, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E1", "E2"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E2"
                }
                );

            SendEvent("E3", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E2", "E3"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E1"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            _listener.Reset();

            SendEvent("E4", 1, 20);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E2", "E4"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E3"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E4"
                }
                );
            _listener.Reset();

            SendEvent("E5", 2, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E2", "E4", "E5"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E5"
                }
                );

            SendEvent("E6", 3, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E4", "E5", "E6"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E2"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E6"
                }
                );
            _listener.Reset();

            SendEvent("E7", 3, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields,
                                                      ToArr("E4", "E5", "E6", "E7"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E7"
                                        }
                );

            SendEvent("E8", 4, 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E4", "E5", "E7", "E8"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetOld(), fields,
                new Object[]
                {
                    "E6"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E8"
                }
                );
            _listener.Reset();

            SendEvent("E9", 3, 50);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E4", "E5", "E7", "E8", "E9"));
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNewAndReset(), fields,
                new Object[]
                {
                    "E9"
                }
                );

            SendEvent("E10", 2, 30);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                ToArr("E4", "E8", "E9", "E10"));
            Assert.AreEqual(2, _listener.LastOldData.Length);
            EPAssertionUtil.AssertProps(
                _listener.LastOldData[0], fields,
                new Object[]
                {
                    "E5"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.LastOldData[1], fields,
                new Object[]
                {
                    "E7"
                }
                );
            EPAssertionUtil.AssertProps(
                _listener.AssertOneGetNew(), fields,
                new Object[]
                {
                    "E10"
                }
                );
            _listener.Reset();
        }

        [Test]
        public void TestValidLegacy()
        {
            Init(true);
            _epService.EPAdministrator.CreateEPL(
                "select TheString from SupportBean.std:unique(TheString).std:unique(IntPrimitive)");
        }
    }
}
