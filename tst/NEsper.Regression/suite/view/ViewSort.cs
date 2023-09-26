///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;


namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewSort
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSceneOne(execs);
            WithSceneTwo(execs);
            WithedSingleKeyBuiltin(execs);
            WithedMultikey(execs);
            WithedPrimitiveKey(execs);
            WithedPrev(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithedPrev(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSortedPrev());
            return execs;
        }

        public static IList<RegressionExecution> WithedPrimitiveKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSortedPrimitiveKey());
            return execs;
        }

        public static IList<RegressionExecution> WithedMultikey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSortedMultikey());
            return execs;
        }

        public static IList<RegressionExecution> WithedSingleKeyBuiltin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSortedSingleKeyBuiltin());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSortSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSortSceneOne());
            return execs;
        }

        private class ViewSortSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream * from SupportBean#sort(3, intPrimitive desc, longPrimitive)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var fields = "theString,intPrimitive,longPrimitive".SplitCsv();

                env.SendEventBean(MakeEvent("E1", 100, 0L));
                env.AssertPropsNew("s0", fields, new object[] { "E1", 100, 0L });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1", 100, 0L } });

                env.SendEventBean(MakeEvent("E2", 99, 5L));
                env.AssertPropsNew("s0", fields, new object[] { "E2", 99, 5L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 100, 0L }, new object[] { "E2", 99, 5L } });

                env.SendEventBean(MakeEvent("E3", 100, -1L));
                env.AssertPropsNew("s0", fields, new object[] { "E3", 100, -1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E3", 100, -1L }, new object[] { "E1", 100, 0L }, new object[] { "E2", 99, 5L }
                    });

                env.SendEventBean(MakeEvent("E4", 100, 1L));
                env.AssertPropsIRPair("s0", fields, new object[] { "E4", 100, 1L }, new object[] { "E2", 99, 5L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E3", 100, -1L }, new object[] { "E1", 100, 0L }, new object[] { "E4", 100, 1L }
                    });

                env.SendEventBean(MakeEvent("E5", 101, 10L));
                env.AssertPropsIRPair("s0", fields, new object[] { "E5", 101, 10L }, new object[] { "E4", 100, 1L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E5", 101, 10L }, new object[] { "E3", 100, -1L }, new object[] { "E1", 100, 0L }
                    });

                env.SendEventBean(MakeEvent("E6", 101, 11L));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6", 101, 11L }, new object[] { "E1", 100, 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E5", 101, 10L }, new object[] { "E6", 101, 11L },
                        new object[] { "E3", 100, -1L }
                    });

                env.SendEventBean(MakeEvent("E6", 100, 0L));
                env.AssertPropsIRPair("s0", fields, new object[] { "E6", 100, 0L }, new object[] { "E6", 100, 0L });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "E5", 101, 10L }, new object[] { "E6", 101, 11L },
                        new object[] { "E3", 100, -1L }
                    });

                env.UndeployAll();
            }
        }

        public class ViewSortSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "theString,intPrimitive".SplitCsv();

                var epl = "@name('s0') select irstream * from SupportBean#sort(3, theString)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertPropsPerRowIterator("s0", fields, null);

                SendSupportBean(env, "G", 1);
                env.AssertPropsNew("s0", fields, new object[] { "G", 1 });

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "G", 1 } });
                SendSupportBean(env, "E", 2);
                env.AssertPropsNew("s0", fields, new object[] { "E", 2 });

                env.Milestone(2);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E", 2 }, new object[] { "G", 1 } });
                SendSupportBean(env, "H", 3);
                env.AssertPropsNew("s0", fields, new object[] { "H", 3 });

                env.Milestone(3);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E", 2 }, new object[] { "G", 1 }, new object[] { "H", 3 } });
                SendSupportBean(env, "I", 4);
                env.AssertPropsIRPair("s0", fields, new object[] { "I", 4 }, new object[] { "I", 4 });

                env.Milestone(4);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E", 2 }, new object[] { "G", 1 }, new object[] { "H", 3 } });
                SendSupportBean(env, "A", 5);
                env.AssertPropsIRPair("s0", fields, new object[] { "A", 5 }, new object[] { "H", 3 });

                env.Milestone(5);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 5 }, new object[] { "E", 2 }, new object[] { "G", 1 } });
                SendSupportBean(env, "C", 6);
                env.AssertPropsIRPair("s0", fields, new object[] { "C", 6 }, new object[] { "G", 1 });

                env.Milestone(6);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 5 }, new object[] { "C", 6 }, new object[] { "E", 2 } });
                SendSupportBean(env, "C", 7);
                env.AssertPropsIRPair("s0", fields, new object[] { "C", 7 }, new object[] { "E", 2 });

                env.Milestone(7);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 5 }, new object[] { "C", 7 }, new object[] { "C", 6 } });
                SendSupportBean(env, "C", 8);
                env.AssertPropsIRPair("s0", fields, new object[] { "C", 8 }, new object[] { "C", 6 });
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A", 5 }, new object[] { "C", 8 }, new object[] { "C", 7 } });

                env.UndeployAll();
            }
        }

        public class ViewSortedSingleKeyBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from  SupportMarketDataBean#sort(3, symbol)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("B1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "B1" } }, null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("D1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "D1" } }, null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("C1"));
                env.AssertPropsNV("s0", new object[][] { new object[] { "symbol", "C1" } }, null);

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("A1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "symbol", "A1" } },
                    new object[][] { new object[] { "symbol", "D1" } });

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("F1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "symbol", "F1" } },
                    new object[][] { new object[] { "symbol", "F1" } });

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("B2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "symbol", "B2" } },
                    new object[][] { new object[] { "symbol", "C1" } });

                env.Milestone(6);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "symbol" },
                    new object[][] { new object[] { "A1" }, new object[] { "B1" }, new object[] { "B2" } });

                env.UndeployAll();
            }
        }

        public class ViewSortedMultikey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from SupportBeanWithEnum#sort(1, theString, supportEnum)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(new SupportBeanWithEnum("E1", SupportEnum.ENUM_VALUE_1));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "theString", "E1" }, new object[] { "supportEnum", SupportEnum.ENUM_VALUE_1 }
                    },
                    null);

                env.Milestone(1);

                env.SendEventBean(new SupportBeanWithEnum("E2", SupportEnum.ENUM_VALUE_2));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "theString", "E2" } },
                    new object[][] { new object[] { "theString", "E2" } });

                env.Milestone(2);

                env.SendEventBean(new SupportBeanWithEnum("E0", SupportEnum.ENUM_VALUE_1));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "theString", "E0" } },
                    new object[][] { new object[] { "theString", "E1" } });

                env.UndeployAll();
            }
        }

        public class ViewSortedPrimitiveKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from SupportMarketDataBean#sort(1, price)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent(10.5));
                env.AssertPropsNV("s0", new object[][] { new object[] { "price", 10.5 } }, null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent(10));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "price", 10.0 } },
                    new object[][] { new object[] { "price", 10.5 } });

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent(11));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "price", 11.0 } },
                    new object[][] { new object[] { "price", 11.0 } });

                env.UndeployAll();
            }
        }

        public class ViewSortedPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream symbol, " +
                           "prev(1, symbol) as prev1," +
                           "prevtail(symbol) as prevtail, " +
                           "prevcount(symbol) as prevCountSym, " +
                           "prevwindow(symbol) as prevWindowSym " +
                           "from SupportMarketDataBean#sort(3, symbol)";
                env.CompileDeploy(text).AddListener("s0");
                var fields = new string[] { "symbol", "prev1", "prevtail", "prevCountSym", "prevWindowSym" };

                env.SendEventBean(MakeMarketDataEvent("B1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "B1", null, "B1", 1L, new object[] { "B1" } } });

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent("D1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "D1", "D1", "D1", 2L, new object[] { "B1", "D1" } } });

                env.SendEventBean(MakeMarketDataEvent("C1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "C1", "C1", "D1", 3L, new object[] { "B1", "C1", "D1" } } });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("A1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "A1", "B1", "C1", 3L, new object[] { "A1", "B1", "C1" } } });

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("F1"));
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "F1", "B1", "C1", 3L, new object[] { "A1", "B1", "C1" } } });

                env.Milestone(3);

                env.UndeployAll();
            }
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(double price)
        {
            return new SupportMarketDataBean("IBM", price, 0L, null);
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }
    }
} // end of namespace