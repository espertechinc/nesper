///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.fromclausemethod
{
    public class ExecFromClauseMethodNStream : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddEventType(typeof(SupportBeanInt));
            configuration.AddImport(typeof(SupportJoinMethods));
            configuration.AddVariable("var1", typeof(int?), 0);
            configuration.AddVariable("var2", typeof(int?), 0);
            configuration.AddVariable("var3", typeof(int?), 0);
            configuration.AddVariable("var4", typeof(int?), 0);
        }

        public override void Run(EPServiceProvider epService) {
            RunAssertion1Stream2HistStarSubordinateCartesianLast(epService);
            RunAssertion1Stream2HistStarSubordinateJoinedKeepall(epService);
            RunAssertion1Stream2HistForwardSubordinate(epService);
            RunAssertion1Stream3HistStarSubordinateCartesianLast(epService);
            RunAssertion1Stream3HistForwardSubordinate(epService);
            RunAssertion1Stream3HistChainSubordinate(epService);
            RunAssertion2Stream2HistStarSubordinate(epService);
            RunAssertion3Stream1HistSubordinate(epService);
            RunAssertion3HistPureNoSubordinate(epService);
            RunAssertion3Hist1Subordinate(epService);
            RunAssertion3Hist2SubordinateChain(epService);
        }

        private void RunAssertion1Stream2HistStarSubordinateCartesianLast(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                         "from SupportBeanInt#lastevent as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                         "order by h0.val, h1.val";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E1", 1, 1);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11"
                    }
                });

            SendBeanInt(epService, "E2", 2, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E3", 0, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E3", 2, 2);
            var result = new object[][] {
                new object[] {
                    "E3",
                    "H01",
                    "H11"
                },
                new object[] {
                    "E3",
                    "H01",
                    "H12"
                },
                new object[] {
                    "E3",
                    "H02",
                    "H11"
                },
                new object[] {
                    "E3",
                    "H02",
                    "H12"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);

            SendBeanInt(epService, "E4", 2, 1);
            result = new object[][] {
                new object[] {
                    "E4",
                    "H01",
                    "H11"
                },
                new object[] {
                    "E4",
                    "H02",
                    "H11"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);

            stmt.Dispose();
        }

        private void RunAssertion1Stream2HistStarSubordinateJoinedKeepall(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                         "from SupportBeanInt#keepall as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                         "where h0.index = h1.index and h0.index = p02";
            TryAssertionOne(epService, expression);

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1   from " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "SupportBeanInt#keepall as s0 " +
                         "where h0.index = h1.index and h0.index = p02";
            TryAssertionOne(epService, expression);
        }

        private void TryAssertionOne(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E1", 20, 20, 3);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H03",
                        "H13"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H03",
                        "H13"
                    }
                });

            SendBeanInt(epService, "E2", 20, 20, 21);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H03",
                        "H13"
                    }
                });

            SendBeanInt(epService, "E3", 4, 4, 2);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new object[][] {
                    new object[] {
                        "E3",
                        "H02",
                        "H12"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new object[][] {
                    new object[] {
                        "E1",
                        "H03",
                        "H13"
                    },
                    new object[] {
                        "E3",
                        "H02",
                        "H12"
                    }
                });

            stmt.Dispose();
        }

        private void RunAssertion1Stream2HistForwardSubordinate(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 " +
                         "from SupportBeanInt#keepall as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal(h0.val, p01) as h1 " +
                         "order by h0.val, h1.val";
            TryAssertionTwo(epService, expression);

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1 from " +
                         "method:SupportJoinMethods.FetchVal(h0.val, p01) as h1, " +
                         "SupportBeanInt#keepall as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0 " +
                         "order by h0.val, h1.val";
            TryAssertionTwo(epService, expression);
        }

        private void TryAssertionTwo(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "id,valh0,valh1".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E1", 1, 1);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H011"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H011"
                    }
                });

            SendBeanInt(epService, "E2", 0, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H011"
                    }
                });

            SendBeanInt(epService, "E3", 1, 0);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H011"
                    }
                });

            SendBeanInt(epService, "E4", 2, 2);
            object[][] result = {
                new object[] {
                    "E4",
                    "H01",
                    "H011"
                }, new object[] {
                    "E4",
                    "H01",
                    "H012"
                }, new object[] {
                    "E4",
                    "H02",
                    "H021"
                }, new object[] {
                    "E4",
                    "H02",
                    "H022"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                EPAssertionUtil.ConcatenateArray2Dim(
                    result, new object[][] {
                        new object[] {
                            "E1",
                            "H01",
                            "H011"
                        }
                    }));

            stmt.Dispose();
        }

        private void RunAssertion1Stream3HistStarSubordinateCartesianLast(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                         "from SupportBeanInt#lastevent as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                         "method:SupportJoinMethods.FetchVal('H2', p02) as h2 " +
                         "order by h0.val, h1.val, h2.val";
            TryAssertionThree(epService, expression);

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal('H2', p02) as h2, " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "SupportBeanInt#lastevent as s0 " +
                         "order by h0.val, h1.val, h2.val";
            TryAssertionThree(epService, expression);
        }

        private void TryAssertionThree(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E1", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11",
                        "H21"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11",
                        "H21"
                    }
                });

            SendBeanInt(epService, "E2", 1, 1, 2);
            var result = new object[][] {
                new object[] {
                    "E2",
                    "H01",
                    "H11",
                    "H21"
                },
                new object[] {
                    "E2",
                    "H01",
                    "H11",
                    "H22"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, result);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);

            stmt.Dispose();
        }

        private void RunAssertion1Stream3HistForwardSubordinate(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                         "from SupportBeanInt#keepall as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1, " +
                         "method:SupportJoinMethods.FetchVal(h0.val||'H2', p02) as h2 " +
                         " where h0.index = h1.index and h1.index = h2.index and h2.index = p03";
            TryAssertionFour(epService, expression);

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal(h0.val||'H2', p02) as h2, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "SupportBeanInt#keepall as s0, " +
                         "method:SupportJoinMethods.FetchVal('H1', p01) as h1 " +
                         " where h0.index = h1.index and h1.index = h2.index and h2.index = p03";
            TryAssertionFour(epService, expression);
        }

        private void TryAssertionFour(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E1", 2, 2, 2, 1);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11",
                        "H01H21"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11",
                        "H01H21"
                    }
                });

            SendBeanInt(epService, "E2", 4, 4, 4, 3);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new object[][] {
                    new object[] {
                        "E2",
                        "H03",
                        "H13",
                        "H03H23"
                    }
                });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new object[][] {
                    new object[] {
                        "E1",
                        "H01",
                        "H11",
                        "H01H21"
                    },
                    new object[] {
                        "E2",
                        "H03",
                        "H13",
                        "H03H23"
                    }
                });

            stmt.Dispose();
        }

        private void RunAssertion1Stream3HistChainSubordinate(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as id, h0.val as valh0, h1.val as valh1, h2.val as valh2 " +
                         "from SupportBeanInt#keepall as s0, " +
                         "method:SupportJoinMethods.FetchVal('H0', p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal(h0.val||'H1', p01) as h1, " +
                         "method:SupportJoinMethods.FetchVal(h1.val||'H2', p02) as h2 " +
                         " where h0.index = h1.index and h1.index = h2.index and h2.index = p03";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "id,valh0,valh1,valh2".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "E2", 4, 4, 4, 3);
            EPAssertionUtil.AssertPropsPerRow(
                listener.GetAndResetLastNewData(), fields,
                new object[][] {
                    new object[] {
                        "E2",
                        "H03",
                        "H03H13",
                        "H03H13H23"
                    }
                });

            SendBeanInt(epService, "E2", 4, 4, 4, 5);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);

            SendBeanInt(epService, "E2", 4, 4, 0, 1);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "E2",
                        "H03",
                        "H03H13",
                        "H03H13H23"
                    }
                });

            stmt.Dispose();
        }

        private void RunAssertion2Stream2HistStarSubordinate(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as ids0, s1.id as ids1, h0.val as valh0, h1.val as valh1 " +
                         "from SupportBeanInt(id like 'S0%')#keepall as s0, " +
                         "SupportBeanInt(id like 'S1%')#lastevent as s1, " +
                         "method:SupportJoinMethods.FetchVal(s0.id||'H1', s0.p00) as h0, " +
                         "method:SupportJoinMethods.FetchVal(s1.id||'H2', s1.p00) as h1 " +
                         "order by s0.id asc";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "ids0,ids1,valh0,valh1".Split(',');
            SendBeanInt(epService, "S00", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanInt(epService, "S10", 1);
            var resultOne = new object[][] {
                new object[] {
                    "S00",
                    "S10",
                    "S00H11",
                    "S10H21"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);

            SendBeanInt(epService, "S01", 1);
            var resultTwo = new object[][] {
                new object[] {
                    "S01",
                    "S10",
                    "S01H11",
                    "S10H21"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

            SendBeanInt(epService, "S11", 1);
            var resultThree = new object[][] {
                new object[] {
                    "S00",
                    "S11",
                    "S00H11",
                    "S11H21"
                },
                new object[] {
                    "S01",
                    "S11",
                    "S01H11",
                    "S11H21"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultThree));

            stmt.Dispose();
        }

        private void RunAssertion3Stream1HistSubordinate(EPServiceProvider epService) {
            string expression;

            expression = "select s0.id as ids0, s1.id as ids1, s2.id as ids2, h0.val as valh0 " +
                         "from SupportBeanInt(id like 'S0%')#keepall as s0, " +
                         "SupportBeanInt(id like 'S1%')#lastevent as s1, " +
                         "SupportBeanInt(id like 'S2%')#lastevent as s2, " +
                         "method:SupportJoinMethods.FetchVal(s1.id||s2.id||'H1', s0.p00) as h0 " +
                         "order by s0.id, s1.id, s2.id, h0.val";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            string[] fields = "ids0,ids1,ids2,valh0".Split(',');
            SendBeanInt(epService, "S00", 2);
            SendBeanInt(epService, "S10", 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);
            Assert.IsFalse(listener.IsInvoked);

            SendBeanInt(epService, "S20", 1);
            var resultOne = new object[][] {
                new object[] {
                    "S00",
                    "S10",
                    "S20",
                    "S10S20H11"
                },
                new object[] {
                    "S00",
                    "S10",
                    "S20",
                    "S10S20H12"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultOne);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, resultOne);

            SendBeanInt(epService, "S01", 1);
            var resultTwo = new object[][] {
                new object[] {
                    "S01",
                    "S10",
                    "S20",
                    "S10S20H11"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultTwo);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultOne, resultTwo));

            SendBeanInt(epService, "S21", 1);
            var resultThree = new object[][] {
                new object[] {
                    "S00",
                    "S10",
                    "S21",
                    "S10S21H11"
                },
                new object[] {
                    "S00",
                    "S10",
                    "S21",
                    "S10S21H12"
                },
                new object[] {
                    "S01",
                    "S10",
                    "S21",
                    "S10S21H11"
                }
            };
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, resultThree);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, EPAssertionUtil.ConcatenateArray2Dim(resultThree));

            stmt.Dispose();
        }

        private void RunAssertion3HistPureNoSubordinate(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("on SupportBeanInt set var1=p00, var2=p01, var3=p02, var4=p03");

            string expression;
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                         "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                         "method:SupportJoinMethods.FetchVal('H2', var3) as h2";
            TryAssertionFive(epService, expression);

            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal('H2', var3) as h2," +
                         "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                         "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
            TryAssertionFive(epService, expression);
        }

        private void TryAssertionFive(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "valh0,valh1,valh2".Split(',');

            SendBeanInt(epService, "S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "H01",
                        "H11",
                        "H21"
                    }
                });

            SendBeanInt(epService, "S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new object[][] {
                    new object[] {
                        "H01",
                        "H11",
                        "H21"
                    },
                    new object[] {
                        "H01",
                        "H11",
                        "H22"
                    }
                });

            SendBeanInt(epService, "S04", 2, 2, 1);
            var result = new object[][] {
                new object[] {
                    "H01",
                    "H11",
                    "H21"
                },
                new object[] {
                    "H02",
                    "H11",
                    "H21"
                },
                new object[] {
                    "H01",
                    "H12",
                    "H21"
                },
                new object[] {
                    "H02",
                    "H12",
                    "H21"
                }
            };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);

            stmt.Dispose();
        }

        private void RunAssertion3Hist1Subordinate(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("on SupportBeanInt set var1=p00, var2=p01, var3=p02, var4=p03");

            string expression;
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                         "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                         "method:SupportJoinMethods.FetchVal(h0.val||'-H2', var3) as h2";
            TryAssertionSix(epService, expression);

            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal(h0.val||'-H2', var3) as h2," +
                         "method:SupportJoinMethods.FetchVal('H1', var2) as h1," +
                         "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
            TryAssertionSix(epService, expression);
        }

        private void TryAssertionSix(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "valh0,valh1,valh2".Split(',');

            SendBeanInt(epService, "S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "H01",
                        "H11",
                        "H01-H21"
                    }
                });

            SendBeanInt(epService, "S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new object[][] {
                    new object[] {
                        "H01",
                        "H11",
                        "H01-H21"
                    },
                    new object[] {
                        "H01",
                        "H11",
                        "H01-H22"
                    }
                });

            SendBeanInt(epService, "S04", 2, 2, 1);
            var result = new object[][] {
                new object[] {
                    "H01",
                    "H11",
                    "H01-H21"
                },
                new object[] {
                    "H02",
                    "H11",
                    "H02-H21"
                },
                new object[] {
                    "H01",
                    "H12",
                    "H01-H21"
                },
                new object[] {
                    "H02",
                    "H12",
                    "H02-H21"
                }
            };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);

            stmt.Dispose();
        }

        private void RunAssertion3Hist2SubordinateChain(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("on SupportBeanInt set var1=p00, var2=p01, var3=p02, var4=p03");

            string expression;
            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal('H0', var1) as h0," +
                         "method:SupportJoinMethods.FetchVal(h0.val||'-H1', var2) as h1," +
                         "method:SupportJoinMethods.FetchVal(h1.val||'-H2', var3) as h2";
            TryAssertionSeven(epService, expression);

            expression = "select h0.val as valh0, h1.val as valh1, h2.val as valh2 from " +
                         "method:SupportJoinMethods.FetchVal(h1.val||'-H2', var3) as h2," +
                         "method:SupportJoinMethods.FetchVal(h0.val||'-H1', var2) as h1," +
                         "method:SupportJoinMethods.FetchVal('H0', var1) as h0";
            TryAssertionSeven(epService, expression);
        }

        private void TryAssertionSeven(EPServiceProvider epService, string expression) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            string[] fields = "valh0,valh1,valh2".Split(',');

            SendBeanInt(epService, "S00", 1, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields, new object[][] {
                    new object[] {
                        "H01",
                        "H01-H11",
                        "H01-H11-H21"
                    }
                });

            SendBeanInt(epService, "S01", 0, 1, 1);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "S02", 1, 1, 0);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

            SendBeanInt(epService, "S03", 1, 1, 2);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                stmt.GetEnumerator(), fields,
                new object[][] {
                    new object[] {
                        "H01",
                        "H01-H11",
                        "H01-H11-H21"
                    },
                    new object[] {
                        "H01",
                        "H01-H11",
                        "H01-H11-H22"
                    }
                });

            SendBeanInt(epService, "S04", 2, 2, 1);
            var result = new object[][] {
                new object[] {
                    "H01",
                    "H01-H11",
                    "H01-H11-H21"
                },
                new object[] {
                    "H02",
                    "H02-H11",
                    "H02-H11-H21"
                },
                new object[] {
                    "H01",
                    "H01-H12",
                    "H01-H12-H21"
                },
                new object[] {
                    "H02",
                    "H02-H12",
                    "H02-H12-H21"
                }
            };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, result);

            stmt.Dispose();
        }

        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01, int p02, int p03) {
            epService.EPRuntime.SendEvent(new SupportBeanInt(id, p00, p01, p02, p03, -1, -1));
        }

        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01, int p02) {
            SendBeanInt(epService, id, p00, p01, p02, -1);
        }

        private void SendBeanInt(EPServiceProvider epService, string id, int p00, int p01) {
            SendBeanInt(epService, id, p00, p01, -1, -1);
        }

        private void SendBeanInt(EPServiceProvider epService, string id, int p00) {
            SendBeanInt(epService, id, p00, -1, -1, -1);
        }
    }
} // end of namespace
