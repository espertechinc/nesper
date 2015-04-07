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
    public class TestOutputLimitEventPerGroup
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

        private const String SYMBOL_DELL = "DELL";
        private const String SYMBOL_IBM = "IBM";

        private const String CATEGORY = "Fully-Aggregated and Grouped";

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void TryOutputFirstHaving(String statementText)
        {
            String[] fields = "TheString,value".Split(',');

            _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                statementText);

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_A("E1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));

            SendBeanEvent("E1", 10);
            SendBeanEvent("E2", 15);
            SendBeanEvent("E1", 10);
            SendBeanEvent("E2", 5);
            Assert.IsFalse(_listener.IsInvoked);

            SendBeanEvent("E2", 5);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 25 });

            SendBeanEvent("E2", -6); // to 19, does not count toward condition
            SendBeanEvent("E2", 2); // to 21, counts toward condition
            Assert.IsFalse(_listener.IsInvoked);
            SendBeanEvent("E2", 1);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 22 });

            SendBeanEvent("E2", 1); // to 23, counts toward condition
            Assert.IsFalse(_listener.IsInvoked);
            SendBeanEvent("E2", 1); // to 24
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 24 });

            SendBeanEvent("E2", -10); // to 14
            SendBeanEvent("E2", 10); // to 24, counts toward condition
            Assert.IsFalse(_listener.IsInvoked);
            SendBeanEvent("E2", 0); // to 24, counts toward condition
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 24 });

            SendBeanEvent("E2", -10); // to 14
            SendBeanEvent("E2", 1); // to 15
            SendBeanEvent("E2", 5); // to 20
            SendBeanEvent("E2", 0); // to 20
            SendBeanEvent("E2", 1); // to 21    // counts
            Assert.IsFalse(_listener.IsInvoked);

            SendBeanEvent("E2", 0); // to 21
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 21 });

            // remove events
            SendMDEvent("E2", 0);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 21 });

            // remove events
            SendMDEvent("E2", -10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 41 });

            // remove events
            SendMDEvent("E2", -6); // since there is 3*-10 we output the next one
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 47 });

            SendMDEvent("E2", 2);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertion12(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(200, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 25d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", null
                }
            }
                );
            expected.AddResultInsRem(800, 1, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 9d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "MSFT", null
                }
            }
                );
            expected.AddResultInsRem(1500, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 49d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 25d
                }
            }
                );
            expected.AddResultInsRem(1500, 2, new Object[][]
            {
                new Object[]
                {
                    "YAH", 1d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", null
                }
            }
                );
            expected.AddResultInsRem(2100, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 49d
                }
            }
                );
            expected.AddResultInsRem(3500, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 3d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", 1d
                }
            }
                );
            expected.AddResultInsRem(4300, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }
                );
            expected.AddResultInsRem(4900, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 6d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", 3d
                }
            }
                );
            expected.AddResultInsRem(5700, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }
                );
            expected.AddResultInsRem(5900, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 7d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", 6d
                }
            }
                );
            expected.AddResultInsRem(6300, 0, new Object[][]
            {
                new Object[]
                {
                    "MSFT", null
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "MSFT", 9d
                }
            }
                );
            expected.AddResultInsRem(7000, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 48d
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                },
                new Object[]
                {
                    "YAH", 7d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion34(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(2100, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }, null);
            expected.AddResultInsRem(4300, 1, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }
                );
            expected.AddResultInsRem(5700, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }
                );
            expected.AddResultInsRem(7000, 0, null, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
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
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 25d
                },
                new Object[]
                {
                    "MSFT", 9d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", null
                },
                new Object[]
                {
                    "MSFT", null
                }
            }
                );
            expected.AddResultInsRem(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                },
                new Object[]
                {
                    "YAH", 1d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 25d
                },
                new Object[]
                {
                    "YAH", null
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, new Object[][]
            {
                new Object[]
                {
                    "YAH", 3d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", 1d
                }
            }
                );
            expected.AddResultInsRem(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                },
                new Object[]
                {
                    "YAH", 3d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                },
                new Object[]
                {
                    "YAH", 7d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }
                );
            expected.AddResultInsRem(7200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 48d
                },
                new Object[]
                {
                    "MSFT", null
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                },
                new Object[]
                {
                    "MSFT", 9d
                },
                new Object[]
                {
                    "YAH", 7d
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
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }
                );
            expected.AddResultInsRem(7200, 0, null, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                }
            }
                );

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
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }, null);
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, null, null);
            expected.AddResultInsRem(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                }
            }
                );
            expected.AddResultInsRem(7200, 0, null, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
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
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(1200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 25d
                },
                new Object[]
                {
                    "MSFT", 9d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", null
                },
                new Object[]
                {
                    "MSFT", null
                }
            }
                );
            expected.AddResultInsRem(2200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 49d
                },
                new Object[]
                {
                    "IBM", 75d
                },
                new Object[]
                {
                    "YAH", 1d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 25d
                },
                new Object[]
                {
                    "IBM", 49d
                },
                new Object[]
                {
                    "YAH", null
                }
            }
                );
            expected.AddResultInsRem(3200, 0, null, null);
            expected.AddResultInsRem(4200, 0, new Object[][]
            {
                new Object[]
                {
                    "YAH", 3d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", 1d
                }
            }
                );
            expected.AddResultInsRem(5200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 75d
                },
                new Object[]
                {
                    "YAH", 3d
                }
            }
                );
            expected.AddResultInsRem(6200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                },
                new Object[]
                {
                    "YAH", 7d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 97d
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }
                );
            expected.AddResultInsRem(7200, 0, new Object[][]
            {
                new Object[]
                {
                    "IBM", 48d
                },
                new Object[]
                {
                    "MSFT", null
                },
                new Object[]
                {
                    "YAH", 6d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "IBM", 72d
                },
                new Object[]
                {
                    "MSFT", 9d
                },
                new Object[]
                {
                    "YAH", 7d
                }
            }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion9_10(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(1200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 25d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", null
                                         },
                                         new Object[]
                                         {
                                             "MSFT", null
                                         }
                                     }
                );
            expected.AddResultInsRem(2200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 1d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 25d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", null
                                         }
                                     }
                );
            expected.AddResultInsRem(3200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 1d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 1d
                                         }
                                     }
                );
            expected.AddResultInsRem(4200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 3d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 1d
                                         }
                                     }
                );
            expected.AddResultInsRem(5200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 97d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 6d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 3d
                                         }
                                     }
                );
            expected.AddResultInsRem(6200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 72d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 7d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 97d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 6d
                                         }
                                     }
                );
            expected.AddResultInsRem(7200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 48d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", null
                                         },
                                         new Object[]
                                         {
                                             "YAH", 6d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 72d
                                         },
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 7d
                                         }
                                     }
                );

            var execution = new ResultAssertExecution(_epService,
                                                      stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertion11_12(String stmtText, String outputLimit)
        {
            SendTimer(0);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);

            stmt.Events += _listener.Update;

            var fields = new String[]
            {
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(1200, 0, null, null);
            expected.AddResultInsRem(2200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }, null);
            expected.AddResultInsRem(3200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }
                );
            expected.AddResultInsRem(4200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }
                );
            expected.AddResultInsRem(5200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 97d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }
                );
            expected.AddResultInsRem(6200, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 72d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 97d
                                         }
                                     }
                );
            expected.AddResultInsRem(7200, 0, null,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 72d
                                         }
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
                "symbol", "sum(price)"
            }
                ;
            var expected = new ResultAssertTestResult(CATEGORY,
                                                      outputLimit, fields);

            expected.AddResultInsRem(200, 1,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 25d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", null
                                         }
                                     }
                );
            expected.AddResultInsRem(800, 1,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "MSFT", null
                                         }
                                     }
                );
            expected.AddResultInsRem(1500, 1,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 49d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 25d
                                         }
                                     }
                );
            expected.AddResultInsRem(1500, 2,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "YAH", 1d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "YAH", null
                                         }
                                     }
                );
            expected.AddResultInsRem(3500, 1, new Object[][]
            {
                new Object[]
                {
                    "YAH", 3d
                }
            }, new Object[][]
            {
                new Object[]
                {
                    "YAH", 1d
                }
            }
                );
            expected.AddResultInsRem(4300, 1,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 97d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 75d
                                         }
                                     }
                );
            expected.AddResultInsRem(4900, 1,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "YAH", 6d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "YAH", 3d
                                         }
                                     }
                );
            expected.AddResultInsRem(5700, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 72d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 97d
                                         }
                                     }
                );
            expected.AddResultInsRem(5900, 1,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "YAH", 7d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "YAH", 6d
                                         }
                                     }
                );
            expected.AddResultInsRem(6300, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "MSFT", null
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "MSFT", 9d
                                         }
                                     }
                );
            expected.AddResultInsRem(7000, 0,
                                     new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 48d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 6d
                                         }
                                     }, new Object[][]
                                     {
                                         new Object[]
                                         {
                                             "IBM", 72d
                                         },
                                         new Object[]
                                         {
                                             "YAH", 7d
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

            var fields = new String[] { "symbol", "sum(price)" };

            var expected = new ResultAssertTestResult(CATEGORY, outputLimit, fields);

            expected.AddResultInsert(1200, 0, new Object[][] { new Object[] { "IBM", 25d }, new Object[] { "MSFT", 9d } });
            expected.AddResultInsert(2200, 0, new Object[][] { new Object[] { "IBM", 75d }, new Object[] { "MSFT", 9d }, new Object[] { "YAH", 1d } });
            expected.AddResultInsert(3200, 0, new Object[][] { new Object[] { "IBM", 75d }, new Object[] { "MSFT", 9d }, new Object[] { "YAH", 1d } });
            expected.AddResultInsert(4200, 0, new Object[][] { new Object[] { "IBM", 75d }, new Object[] { "MSFT", 9d }, new Object[] { "YAH", 3d } });
            expected.AddResultInsert(5200, 0, new Object[][] { new Object[] { "IBM", 97d }, new Object[] { "MSFT", 9d }, new Object[] { "YAH", 6d } });
            expected.AddResultInsert(6200, 0, new Object[][] { new Object[] { "IBM", 72d }, new Object[] { "MSFT", 9d }, new Object[] { "YAH", 7d } });
            expected.AddResultInsert(7200, 0, new Object[][] { new Object[] { "IBM", 48d }, new Object[] { "YAH", 6d } });

            var execution = new ResultAssertExecution(_epService, stmt, _listener, expected);

            execution.Execute();
        }

        private void RunAssertionLast(EPStatement selectTestView)
        {
            // assert select result type
            Assert.AreEqual(typeof (string),
                            selectTestView.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof (double?),
                            selectTestView.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof (double?),
                            selectTestView.EventType.GetPropertyType("myAvg"));

            SendMDEvent(SYMBOL_DELL, 10);
            Assert.IsFalse(_listener.IsInvoked);

            SendMDEvent(SYMBOL_DELL, 20);
            AssertEvent(SYMBOL_DELL, null, null, 30d, 15d);
            _listener.Reset();

            SendMDEvent(SYMBOL_DELL, 100);
            Assert.IsFalse(_listener.IsInvoked);

            SendMDEvent(SYMBOL_DELL, 50);
            AssertEvent(SYMBOL_DELL, 30d, 15d, 170d, 170/3d);
        }

        private void RunAssertionSingle(EPStatement selectTestView)
        {
            // assert select result type
            Assert.AreEqual(typeof (string),
                            selectTestView.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof (double?),
                            selectTestView.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof (double?),
                            selectTestView.EventType.GetPropertyType("myAvg"));

            SendMDEvent(SYMBOL_DELL, 10);
            Assert.IsTrue(_listener.IsInvoked);
            AssertEvent(SYMBOL_DELL, null, null, 10d, 10d);

            SendMDEvent(SYMBOL_IBM, 20);
            Assert.IsTrue(_listener.IsInvoked);
            AssertEvent(SYMBOL_IBM, null, null, 20d, 20d);
        }

        private void RunAssertionAll(EPStatement selectTestView)
        {
            // assert select result type
            Assert.AreEqual(typeof (string), selectTestView.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof (double?), selectTestView.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof (double?), selectTestView.EventType.GetPropertyType("myAvg"));

            SendMDEvent(SYMBOL_IBM, 70);
            Assert.IsFalse(_listener.IsInvoked);

            SendMDEvent(SYMBOL_DELL, 10);
            AssertEvents(SYMBOL_IBM, null, null, 70d, 70d, SYMBOL_DELL, null, null, 10d, 10d);
            _listener.Reset();

            SendMDEvent(SYMBOL_DELL, 20);
            Assert.IsFalse(_listener.IsInvoked);

            SendMDEvent(SYMBOL_DELL, 100);
            AssertEvents(SYMBOL_IBM, 70d, 70d, 70d, 70d, SYMBOL_DELL, 10d, 10d, 130d, 130d/3d);
        }

        private void AssertEvent(string symbol,
                                 double? oldSum,
                                 double? oldAvg,
                                 double? newSum,
                                 double? newAvg)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
            Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
            Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");

            _listener.Reset();
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void AssertEvents(string symbolOne,
                                  double? oldSumOne,
                                  double? oldAvgOne,
                                  double newSumOne,
                                  double newAvgOne,
                                  string symbolTwo,
                                  double? oldSumTwo,
                                  double? oldAvgTwo,
                                  double newSumTwo,
                                  double newAvgTwo)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.AreEqual(2, oldData.Length);
            Assert.AreEqual(2, newData.Length);

            int indexOne = 0;
            int indexTwo = 1;

            if (oldData[0].Get("symbol").Equals(symbolTwo))
            {
                indexTwo = 0;
                indexOne = 1;
            }
            Assert.AreEqual(newSumOne, newData[indexOne].Get("mySum"));
            Assert.AreEqual(newSumTwo, newData[indexTwo].Get("mySum"));
            Assert.AreEqual(oldSumOne, oldData[indexOne].Get("mySum"));
            Assert.AreEqual(oldSumTwo, oldData[indexTwo].Get("mySum"));

            Assert.AreEqual(newAvgOne, newData[indexOne].Get("myAvg"));
            Assert.AreEqual(newAvgTwo, newData[indexTwo].Get("myAvg"));
            Assert.AreEqual(oldAvgOne, oldData[indexOne].Get("myAvg"));
            Assert.AreEqual(oldAvgTwo, oldData[indexTwo].Get("myAvg"));

            _listener.Reset();
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void SendMDEvent(String symbol, double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L,
                                                 null);

            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendBeanEvent(String theString, int intPrimitive)
        {
            _epService.EPRuntime.SendEvent(
                new SupportBean(theString, intPrimitive));
        }

        private void SendTimer(long timeInMSec)
        {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;

            runtime.SendEvent(theEvent);
        }

        [Test]
        public void Test10AllNoHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "output all every 1 seconds "
                              + "order by symbol";

            RunAssertion9_10(stmtText, "all");
        }

        [Test]
        public void Test11AllHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) " + "group by symbol "
                              + "having sum(price) > 50 " + "output all every 1 seconds";

            RunAssertion11_12(stmtText, "all");
        }

        [Test]
        public void Test12AllHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "having sum(price) > 50 "
                              + "output all every 1 seconds";

            RunAssertion11_12(stmtText, "all");
        }

        [Test]
        public void Test13LastNoHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec)" + "group by symbol "
                              + "output last every 1 seconds " + "order by symbol";

            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test14LastNoHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "output last every 1 seconds "
                              + "order by symbol";

            RunAssertion13_14(stmtText, "last");
        }

        [Test]
        public void Test15LastHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec)" + "group by symbol "
                              + "having sum(price) > 50 " + "output last every 1 seconds";

            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test16LastHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "having sum(price) > 50 "
                              + "output last every 1 seconds";

            RunAssertion15_16(stmtText, "last");
        }

        [Test]
        public void Test17FirstNoHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "output first every 1 seconds";

            RunAssertion17(stmtText, "first");
        }

        [Test]
        public void Test17FirstNoHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) " + "group by symbol "
                              + "output first every 1 seconds";

            RunAssertion17(stmtText, "first");
        }

        [Test]
        public void Test18SnapshotNoHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "output snapshot every 1 seconds "
                              + "order by symbol";

            RunAssertion18(stmtText, "snapshot");
        }

        [Test]
        public void Test18SnapshotNoHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) " + "group by symbol "
                              + "output snapshot every 1 seconds " + "order by symbol";

            RunAssertion18(stmtText, "snapshot");
        }

        [Test]
        public void Test1NoneNoHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec)" + "group by symbol "
                              + "order by symbol asc";

            RunAssertion12(stmtText, "none");
        }

        [Test]
        public void Test2NoneNoHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "order by symbol asc";

            RunAssertion12(stmtText, "none");
        }

        [Test]
        public void Test3NoneHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) " + "group by symbol "
                              + " having sum(price) > 50";

            RunAssertion34(stmtText, "none");
        }

        [Test]
        public void Test4NoneHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "having sum(price) > 50";

            RunAssertion34(stmtText, "none");
        }

        [Test]
        public void Test5DefaultNoHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) " + "group by symbol "
                              + "output every 1 seconds order by symbol asc";

            RunAssertion56(stmtText, "default");
        }

        [Test]
        public void Test6DefaultNoHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol "
                              + "output every 1 seconds order by symbol asc";

            RunAssertion56(stmtText, "default");
        }

        [Test]
        public void Test7DefaultHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) \n" + "group by symbol "
                              + "having sum(price) > 50" + "output every 1 seconds";

            RunAssertion78(stmtText, "default");
        }

        [Test]
        public void Test8DefaultHavingJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec), "
                              + "SupportBean.win:keepall() where TheString=symbol "
                              + "group by symbol " + "having sum(price) > 50"
                              + "output every 1 seconds";

            RunAssertion78(stmtText, "default");
        }

        [Test]
        public void Test9AllNoHavingNoJoin()
        {
            String stmtText = "select symbol, sum(price) "
                              + "from MarketData.win:time(5.5 sec) " + "group by symbol "
                              + "output all every 1 seconds " + "order by symbol";

            RunAssertion9_10(stmtText, "all");
        }

        [Test]
        public void TestGroupBy_All()
        {
            String[] fields = "symbol,sum(price)".Split(',');
            String eventName = typeof (SupportMarketDataBean).FullName;
            String statementString = "select irstream symbol, sum(price) from "
                                     + eventName
                                     + ".win:length(5) group by symbol output all every 5 events";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                statementString);
            var updateListener = new SupportUpdateListener();

            statement.Events += updateListener.Update;

            // send some events and check that only the most recent
            // ones are kept
            SendMDEvent("IBM", 1D);
            SendMDEvent("IBM", 2D);
            SendMDEvent("HP", 1D);
            SendMDEvent("IBM", 3D);
            SendMDEvent("MAC", 1D);

            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            EventBean[] newData = updateListener.LastNewData;

            Assert.AreEqual(3, newData.Length);
            EPAssertionUtil.AssertPropsPerRow(newData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM", 6d
                                                  },
                                                  new Object[]
                                                  {
                                                      "HP", 1d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MAC", 1d
                                                  }
                                              }
                );
            EventBean[] oldData = updateListener.LastOldData;

            EPAssertionUtil.AssertPropsPerRow(oldData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM", null
                                                  },
                                                  new Object[]
                                                  {
                                                      "HP", null
                                                  },
                                                  new Object[]
                                                  {
                                                      "MAC", null
                                                  }
                                              }
                );
        }

        [Test]
        public void TestGroupBy_Default()
        {
            String[] fields = "symbol,sum(price)".Split(',');
            String eventName = typeof (SupportMarketDataBean).FullName;
            String statementString = "select irstream symbol, sum(price) from "
                                     + eventName
                                     + ".win:length(5) group by symbol output every 5 events";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(
                statementString);
            var updateListener = new SupportUpdateListener();

            statement.Events += updateListener.Update;

            // send some events and check that only the most recent
            // ones are kept
            SendMDEvent("IBM", 1D);
            SendMDEvent("IBM", 2D);
            SendMDEvent("HP", 1D);
            SendMDEvent("IBM", 3D);
            SendMDEvent("MAC", 1D);

            Assert.IsTrue(updateListener.GetAndClearIsInvoked());
            EventBean[] newData = updateListener.LastNewData;
            EventBean[] oldData = updateListener.LastOldData;

            Assert.AreEqual(5, newData.Length);
            Assert.AreEqual(5, oldData.Length);
            EPAssertionUtil.AssertPropsPerRow(newData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM", 1d
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 3d
                                                  },
                                                  new Object[]
                                                  {
                                                      "HP", 1d
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 6d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MAC", 1d
                                                  }
                                              }
                );
            EPAssertionUtil.AssertPropsPerRow(oldData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM", null
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 1d
                                                  },
                                                  new Object[]
                                                  {
                                                      "HP", null
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 3d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MAC", null
                                                  }
                                              }
                );
        }

        [Test]
        public void TestJoinAll()
        {
            String viewExpr = "select irstream symbol," + "sum(price) as mySum,"
                              + "avg(price) as myAvg " + "from "
                              + typeof (SupportBeanString).FullName
                              + ".win:length(100) as one, "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(5) as two "
                              + "where (symbol='DELL' or symbol='IBM' or symbol='GE') "
                              + "       and one.TheString = two.symbol " + "group by symbol "
                              + "output all every 2 events";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            RunAssertionAll(selectTestView);
        }

        [Test]
        public void TestJoinLast()
        {
            String viewExpr = "select irstream symbol," + "sum(price) as mySum,"
                              + "avg(price) as myAvg " + "from "
                              + typeof (SupportBeanString).FullName
                              + ".win:length(100) as one, "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(3) as two "
                              + "where (symbol='DELL' or symbol='IBM' or symbol='GE') "
                              + "       and one.TheString = two.symbol " + "group by symbol "
                              + "output last every 2 events";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            RunAssertionLast(selectTestView);
        }

        [Test]
        public void TestJoinSortWindow()
        {
            SendTimer(0);

            String[] fields = "symbol,maxVol".Split(',');
            String viewExpr = "select irstream symbol, max(price) as maxVol"
                              + " from " + typeof (SupportMarketDataBean).FullName
                              + ".ext:sort(1, volume desc) as s0,"
                              + typeof (SupportBean).FullName + ".win:keepall() as s1 "
                              + "group by symbol output every 1 seconds";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(viewExpr);

            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("JOIN_KEY", -1));

            SendMDEvent("JOIN_KEY", 1d);
            SendMDEvent("JOIN_KEY", 2d);
            _listener.Reset();

            // moves all events out of the window,
            SendTimer(1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();

            Assert.AreEqual(2, result.First.Length);
            EPAssertionUtil.AssertPropsPerRow(result.First, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "JOIN_KEY", 1.0
                                                  },
                                                  new Object[]
                                                  {
                                                      "JOIN_KEY", 2.0
                                                  }
                                              }
                );
            Assert.AreEqual(2, result.Second.Length);
            EPAssertionUtil.AssertPropsPerRow(result.Second, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "JOIN_KEY", null
                                                  },
                                                  new Object[]
                                                  {
                                                      "JOIN_KEY", 1.0
                                                  }
                                              }
                );
        }

        [Test]
        public void TestLastNoDataWindow()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            String epl =
                "select TheString, IntPrimitive as intp from SupportBean group by TheString output last every 1 seconds order by TheString asc";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);

            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 22));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));

            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(),
                                              new String[]
                                              {
                                                  "TheString", "intp"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "E1", 3
                                                  },
                                                  new Object[]
                                                  {
                                                      "E2", 21
                                                  },
                                                  new Object[]
                                                  {
                                                      "E3", 31
                                                  }
                                              }
                );

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 31));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 33));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));

            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(),
                                              new String[]
                                              {
                                                  "TheString", "intp"
                                              }, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "E1", 5
                                                  },
                                                  new Object[]
                                                  {
                                                      "E3", 33
                                                  }
                                              }
                );
        }

        [Test]
        public void TestLimitSnapshot()
        {
            SendTimer(0);
            String selectStmt = "select symbol, min(price) as minprice from "
                                + typeof (SupportMarketDataBean).FullName
                                +
                                ".win:time(10 seconds) group by symbol output snapshot every 1 seconds order by symbol asc";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(selectStmt);

            stmt.Events += _listener.Update;
            SendMDEvent("ABC", 20);

            SendTimer(500);
            SendMDEvent("IBM", 16);
            SendMDEvent("ABC", 14);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendTimer(1000);
            var fields = new String[]
            {
                "symbol", "minprice"
            }
                ;

            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "ABC", 14d
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 16d
                                                  }
                                              }
                );
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(1500);
            SendMDEvent("IBM", 18);
            SendMDEvent("MSFT", 30);

            SendTimer(10000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "ABC", 14d
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 16d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MSFT", 30d
                                                  }
                                              }
                );
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM", 18d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MSFT", 30d
                                                  }
                                              }
                );
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(12000);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsNull(_listener.LastNewData);
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();
        }

        [Test]
        public void TestLimitSnapshotLimit()
        {
            SendTimer(0);
            String selectStmt = "select symbol, min(price) as minprice from "
                                + typeof (SupportMarketDataBean).FullName
                                + ".win:time(10 seconds) as m, " + typeof (SupportBean).FullName
                                + ".win:keepall() as s where s.TheString = m.symbol "
                                + "group by symbol output snapshot every 1 seconds order by symbol asc";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(selectStmt);

            stmt.Events += _listener.Update;

            foreach (String theString in "ABC,IBM,MSFT".Split(','))
            {
                _epService.EPRuntime.SendEvent(new SupportBean(theString, 1));
            }

            SendMDEvent("ABC", 20);

            SendTimer(500);
            SendMDEvent("IBM", 16);
            SendMDEvent("ABC", 14);
            Assert.IsFalse(_listener.GetAndClearIsInvoked());

            SendTimer(1000);
            var fields = new String[]
            {
                "symbol", "minprice"
            }
                ;

            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "ABC", 14d
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 16d
                                                  }
                                              }
                );
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(1500);
            SendMDEvent("IBM", 18);
            SendMDEvent("MSFT", 30);

            SendTimer(10000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "ABC", 14d
                                                  },
                                                  new Object[]
                                                  {
                                                      "IBM", 16d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MSFT", 30d
                                                  }
                                              }
                );
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(10500);
            SendTimer(11000);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "IBM", 18d
                                                  },
                                                  new Object[]
                                                  {
                                                      "MSFT", 30d
                                                  }
                                              }
                );
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(11500);
            SendTimer(12000);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.IsNull(_listener.LastNewData);
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();
        }

        [Test]
        public void TestMaxTimeWindow()
        {
            SendTimer(0);

            String[] fields = "symbol,maxVol".Split(',');
            String viewExpr = "select irstream symbol, max(price) as maxVol"
                              + " from " + typeof (SupportMarketDataBean).FullName
                              + ".win:time(1 sec) " + "group by symbol output every 1 seconds";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            SendMDEvent("SYM1", 1d);
            SendMDEvent("SYM1", 2d);
            _listener.Reset();

            // moves all events out of the window,
            SendTimer(1000); // newdata is 2 eventa, old data is the same 2 events, therefore the sum is null
            UniformPair<EventBean[]> result = _listener.GetDataListsFlattened();

            Assert.AreEqual(3, result.First.Length);
            EPAssertionUtil.AssertPropsPerRow(result.First, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "SYM1", 1.0
                                                  },
                                                  new Object[]
                                                  {
                                                      "SYM1", 2.0
                                                  },
                                                  new Object[]
                                                  {
                                                      "SYM1", null
                                                  }
                                              }
                );
            Assert.AreEqual(3, result.Second.Length);
            EPAssertionUtil.AssertPropsPerRow(result.Second, fields,
                                              new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "SYM1", null
                                                  },
                                                  new Object[]
                                                  {
                                                      "SYM1", 1.0
                                                  },
                                                  new Object[]
                                                  {
                                                      "SYM1", 2.0
                                                  }
                                              }
                );
        }

        [Test]
        public void TestNoJoinAll()
        {
            String viewExpr = "select irstream symbol," + "sum(price) as mySum,"
                              + "avg(price) as myAvg " + "from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(5) "
                              + "where symbol='DELL' or symbol='IBM' or symbol='GE' "
                              + "group by symbol " + "output all every 2 events";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            RunAssertionAll(selectTestView);
        }

        [Test]
        public void TestNoJoinLast()
        {
            String viewExpr = "select irstream symbol," + "sum(price) as mySum,"
                              + "avg(price) as myAvg " + "from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(3) "
                              + "where symbol='DELL' or symbol='IBM' or symbol='GE' "
                              + "group by symbol " + "output last every 2 events";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            RunAssertionLast(selectTestView);
        }

        [Test]
        public void TestNoOutputClauseJoin()
        {
            String viewExpr = "select irstream symbol," + "sum(price) as mySum,"
                              + "avg(price) as myAvg " + "from "
                              + typeof (SupportBeanString).FullName
                              + ".win:length(100) as one, "
                              + typeof (SupportMarketDataBean).FullName
                              + ".win:length(3) as two "
                              + "where (symbol='DELL' or symbol='IBM' or symbol='GE') "
                              + "       and one.TheString = two.symbol " + "group by symbol";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            RunAssertionSingle(selectTestView);
        }

        [Test]
        public void TestNoOutputClauseView()
        {
            String viewExpr = "select irstream symbol," + "sum(price) as mySum,"
                              + "avg(price) as myAvg " + "from "
                              + typeof (SupportMarketDataBean).FullName + ".win:length(3) "
                              + "where symbol='DELL' or symbol='IBM' or symbol='GE' "
                              + "group by symbol";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(
                viewExpr);

            selectTestView.Events += _listener.Update;

            RunAssertionSingle(selectTestView);
        }

        [Test]
        public void TestOutputFirstCrontab()
        {
            SendTimer(0);
            String[] fields = "TheString,value".Split(',');

            _epService.EPAdministrator.Configuration.AddVariable("varout", typeof (bool), false);
            _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first at (*/2, *, *, *, *)");

            stmt.Events += _listener.Update;

            SendBeanEvent("E1", 10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 10
                                        }
                );

            SendTimer(2*60*1000 - 1);
            SendBeanEvent("E1", 11);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(2*60*1000);
            SendBeanEvent("E1", 12);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 33
                                        }
                );

            SendBeanEvent("E2", 20);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E2", 20
                                        }
                );

            SendBeanEvent("E2", 21);
            SendTimer(4*60*1000 - 1);
            SendBeanEvent("E2", 22);
            SendBeanEvent("E1", 13);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(4*60*1000);
            SendBeanEvent("E2", 23);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E2", 86
                                        }
                );
            SendBeanEvent("E1", 14);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 60
                                        }
                );
        }

        [Test]
        public void TestOutputFirstEveryNEvents()
        {
            String[] fields = "TheString,value".Split(',');

            _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every 3 events");

            stmt.Events += _listener.Update;

            SendBeanEvent("E1", 10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 10
                                        }
                );

            SendBeanEvent("E1", 12);
            SendBeanEvent("E1", 11);
            Assert.IsFalse(_listener.IsInvoked);

            SendBeanEvent("E1", 13);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 46
                                        }
                );

            SendMDEvent("S1", 12);
            SendMDEvent("S1", 11);
            Assert.IsFalse(_listener.IsInvoked);

            SendMDEvent("S1", 10);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 13
                                        }
                );

            SendBeanEvent("E1", 14);
            SendBeanEvent("E1", 15);
            Assert.IsFalse(_listener.IsInvoked);

            SendBeanEvent("E2", 20);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E2", 20
                                        }
                );

            // test variable
            _epService.EPAdministrator.CreateEPL("create variable int myvar = 1");
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first every myvar events");
            stmt.Events += _listener.Update;

            SendBeanEvent("E3", 10);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(),
                                              fields, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "E3", 10
                                                  }
                                              }
                );

            SendBeanEvent("E1", 5);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(),
                                              fields, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "E1", 47
                                                  }
                                              }
                );

            _epService.EPRuntime.SetVariableValue("myvar", 2);

            SendBeanEvent("E1", 6);
            Assert.IsFalse(_listener.IsInvoked);

            SendBeanEvent("E1", 7);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(),
                                              fields, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "E1", 60
                                                  }
                                              }
                );

            SendBeanEvent("E1", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendBeanEvent("E1", 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(),
                                              fields, new Object[][]
                                              {
                                                  new Object[]
                                                  {
                                                      "E1", 62
                                                  }
                                              }
                );
        }

        [Test]
        public void TestOutputFirstHavingJoinNoJoin()
        {
            _epService.EPAdministrator.Configuration.AddEventType(
                "SupportBean_A", typeof (SupportBean_A));

            String stmtText =
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events";

            TryOutputFirstHaving(stmtText);

            String stmtTextJoin = "select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A.win:keepall() a where a.id = mv.TheString "
                                  + "group by TheString having sum(IntPrimitive) > 20 output first every 2 events";

            TryOutputFirstHaving(stmtTextJoin);

            String stmtTextOrder =
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";

            TryOutputFirstHaving(stmtTextOrder);

            String stmtTextOrderJoin = "select TheString, sum(IntPrimitive) as value from MyWindow mv, SupportBean_A.win:keepall() a where a.id = mv.TheString "
                                       +
                                       "group by TheString having sum(IntPrimitive) > 20 output first every 2 events order by TheString asc";

            TryOutputFirstHaving(stmtTextOrderJoin);
        }

        [Test]
        public void TestOutputFirstWhenThen()
        {
            String[] fields = "TheString,value".Split(',');

            _epService.EPAdministrator.Configuration.AddVariable("varout", typeof (bool), false);
            _epService.EPAdministrator.CreateEPL(
                "create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL(
                "on MarketData md delete from MyWindow mw where mw.IntPrimitive = md.price");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select TheString, sum(IntPrimitive) as value from MyWindow group by TheString output first when varout then set varout = false");

            stmt.Events += _listener.Update;

            SendBeanEvent("E1", 10);
            SendBeanEvent("E1", 11);
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SetVariableValue("varout", true);
            SendBeanEvent("E1", 12);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E1", 33
                                        }
                );
            Assert.AreEqual(false, _epService.EPRuntime.GetVariableValue("varout"));

            _epService.EPRuntime.SetVariableValue("varout", true);
            SendBeanEvent("E2", 20);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                                        new Object[]
                                        {
                                            "E2", 20
                                        }
                );
            Assert.AreEqual(false, _epService.EPRuntime.GetVariableValue("varout"));

            SendBeanEvent("E1", 13);
            SendBeanEvent("E2", 21);
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestWildcardEventPerGroup()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBean group by TheString output last every 3 events order by TheString asc");
            var listener = new SupportUpdateListener();

            stmt.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("IBM", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("ATT", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("IBM", 100));

            EventBean[] events = listener.GetNewDataListFlattened();

            listener.Reset();
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual("ATT", events[0].Get("TheString"));
            Assert.AreEqual(11, events[0].Get("IntPrimitive"));
            Assert.AreEqual("IBM", events[1].Get("TheString"));
            Assert.AreEqual(100, events[1].Get("IntPrimitive"));
            stmt.Dispose();

            // All means each event
            stmt = _epService.EPAdministrator.CreateEPL(
                "select * from SupportBean group by TheString output all every 3 events");
            stmt.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("IBM", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("ATT", 11));
            _epService.EPRuntime.SendEvent(new SupportBean("IBM", 100));

            events = listener.GetNewDataListFlattened();
            Assert.AreEqual(3, events.Length);
            Assert.AreEqual("IBM", events[0].Get("TheString"));
            Assert.AreEqual(10, events[0].Get("IntPrimitive"));
            Assert.AreEqual("ATT", events[1].Get("TheString"));
            Assert.AreEqual(11, events[1].Get("IntPrimitive"));
            Assert.AreEqual("IBM", events[2].Get("TheString"));
            Assert.AreEqual(100, events[2].Get("IntPrimitive"));
        }
    }
}
