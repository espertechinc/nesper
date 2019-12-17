///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewSort
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewSortSceneOne());
            execs.Add(new ViewSortSceneTwo());
            execs.Add(new ViewSortedSingleKeyBuiltin());
            execs.Add(new ViewSortedMultikey());
            execs.Add(new ViewSortedPrimitiveKey());
            execs.Add(new ViewSortedPrev());
            return execs;
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

        internal class ViewSortSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#sort(3, IntPrimitive desc, LongPrimitive)";
                env.CompileDeployAddListenerMileZero(epl, "s0");
                var fields = new [] { "TheString","IntPrimitive","LongPrimitive" };

                env.SendEventBean(MakeEvent("E1", 100, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 100, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 100, 0L}});

                env.SendEventBean(MakeEvent("E2", 99, 5L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 99, 5L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 100, 0L}, new object[] {"E2", 99, 5L}});

                env.SendEventBean(MakeEvent("E3", 100, -1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 100, -1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 100, -1L}, new object[] {"E1", 100, 0L}, new object[] {"E2", 99, 5L}});

                env.SendEventBean(MakeEvent("E4", 100, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E4", 100, 1L},
                    new object[] {"E2", 99, 5L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 100, -1L}, new object[] {"E1", 100, 0L}, new object[] {"E4", 100, 1L}});

                env.SendEventBean(MakeEvent("E5", 101, 10L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E5", 101, 10L},
                    new object[] {"E4", 100, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E5", 101, 10L}, new object[] {"E3", 100, -1L}, new object[] {"E1", 100, 0L}});

                env.SendEventBean(MakeEvent("E6", 101, 11L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E6", 101, 11L},
                    new object[] {"E1", 100, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E5", 101, 10L}, new object[] {"E6", 101, 11L}, new object[] {"E3", 100, -1L}
                    });

                env.SendEventBean(MakeEvent("E6", 100, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E6", 100, 0L},
                    new object[] {"E6", 100, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E5", 101, 10L}, new object[] {"E6", 101, 11L}, new object[] {"E3", 100, -1L}
                    });

                env.UndeployAll();
            }
        }

        public class ViewSortSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","IntPrimitive" };

                var epl = "@Name('s0') select irstream * from SupportBean#sort(3, TheString)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "G", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G", 1});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"G", 1}});
                SendSupportBean(env, "E", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E", 2});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E", 2}, new object[] {"G", 1}});
                SendSupportBean(env, "H", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"H", 3});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E", 2}, new object[] {"G", 1}, new object[] {"H", 3}});
                SendSupportBean(env, "I", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"I", 4},
                    new object[] {"I", 4});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E", 2}, new object[] {"G", 1}, new object[] {"H", 3}});
                SendSupportBean(env, "A", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"A", 5},
                    new object[] {"H", 3});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 5}, new object[] {"E", 2}, new object[] {"G", 1}});
                SendSupportBean(env, "C", 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"C", 6},
                    new object[] {"G", 1});

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 5}, new object[] {"C", 6}, new object[] {"E", 2}});
                SendSupportBean(env, "C", 7);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"C", 7},
                    new object[] {"E", 2});

                env.Milestone(7);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 5}, new object[] {"C", 7}, new object[] {"C", 6}});
                SendSupportBean(env, "C", 8);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"C", 8},
                    new object[] {"C", 6});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 5}, new object[] {"C", 8}, new object[] {"C", 7}});

                env.UndeployAll();
            }
        }

        public class ViewSortedSingleKeyBuiltin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream * from  SupportMarketDataBean#sort(3, Symbol)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("B1"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "B1"}}, null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("D1"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "D1"}}, null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("C1"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "C1"}}, null);

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("A1"));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Symbol", "A1"}},
                        new[] {new object[] {"Symbol", "D1"}});

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("F1"));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Symbol", "F1"}},
                        new[] {new object[] {"Symbol", "F1"}});

                env.Milestone(5);

                env.SendEventBean(MakeMarketDataEvent("B2"));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Symbol", "B2"}},
                        new[] {new object[] {"Symbol", "C1"}});

                env.Milestone(6);

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"Symbol"},
                    new[] {new object[] {"A1"}, new object[] {"B1"}, new object[] {"B2"}});

                env.UndeployAll();
            }
        }

        public class ViewSortedMultikey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream * from SupportBeanWithEnum#sort(1, TheString, SupportEnum)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(new SupportBeanWithEnum("E1", SupportEnum.ENUM_VALUE_1));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"TheString", "E1"}, new object[] {"SupportEnum", SupportEnum.ENUM_VALUE_1}
                        },
                        null);

                env.Milestone(1);

                env.SendEventBean(new SupportBeanWithEnum("E2", SupportEnum.ENUM_VALUE_2));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"TheString", "E2"}},
                        new[] {new object[] {"TheString", "E2"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBeanWithEnum("E0", SupportEnum.ENUM_VALUE_1));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"TheString", "E0"}},
                        new[] {new object[] {"TheString", "E1"}});

                env.UndeployAll();
            }
        }

        public class ViewSortedPrimitiveKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream * from SupportMarketDataBean#sort(1, Price)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent(10.5));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Price", 10.5}}, null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent(10));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Price", 10.0}},
                        new[] {new object[] {"Price", 10.5}});

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent(11));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {new object[] {"Price", 11.0}},
                        new[] {new object[] {"Price", 11.0}});

                env.UndeployAll();
            }
        }

        public class ViewSortedPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream Symbol, " +
                           "prev(1, Symbol) as prev1," +
                           "prevtail(Symbol) as prevtail, " +
                           "prevcount(Symbol) as prevCountSym, " +
                           "prevwindow(Symbol) as prevWindowSym " +
                           "from SupportMarketDataBean#sort(3, Symbol)";
                env.CompileDeploy(text).AddListener("s0");
                string[] fields = {"Symbol", "prev1", "prevtail", "prevCountSym", "prevWindowSym"};

                env.SendEventBean(MakeMarketDataEvent("B1"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "B1", null, "B1", 1L,
                            new object[] {"B1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent("D1"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "D1", "D1", "D1", 2L,
                            new object[] {"B1", "D1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.SendEventBean(MakeMarketDataEvent("C1"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "C1", "C1", "D1", 3L,
                            new object[] {"B1", "C1", "D1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("A1"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "A1", "B1", "C1", 3L,
                            new object[] {"A1", "B1", "C1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("F1"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {
                        new object[] {
                            "F1", "B1", "C1", 3L,
                            new object[] {"A1", "B1", "C1"}
                        }
                    });
                env.Listener("s0").Reset();

                env.Milestone(3);

                env.UndeployAll();
            }
        }
    }
} // end of namespace