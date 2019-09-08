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
using com.espertech.esper.regressionlib.support.util;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewRank
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewRankedPrev());
            execs.Add(new ViewRankPrevAndGroupWin());
            execs.Add(new ViewRankMultiexpression());
            execs.Add(new ViewRankRemoveStream());
            execs.Add(new ViewRankRanked());
            execs.Add(new ViewRankedSceneOne());
            execs.Add(new ViewRankInvalid());
            return execs;
        }

        private static void AssertWindowAggAndPrev(
            RegressionEnvironment env,
            object[][] expected)
        {
            var fields = new [] { "TheString","IntPrimitive","LongPrimitive" };
            var @event = env.Listener("s0").AssertOneGetNewAndReset();
            SupportBeanAssertionUtil.AssertPropsPerRow((object[]) @event.Get("win"), fields, expected);
            for (var i = 0; i < 5; i++) {
                var prevValue = @event.Get("prev" + i);
                if (prevValue == null && expected.Length <= i) {
                    continue;
                }

                SupportBeanAssertionUtil.AssertPropsBean((SupportBean) prevValue, fields, expected[i]);
            }
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            return MakeEvent(theString, intPrimitive, longPrimitive, 0d);
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive,
            double doublePrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            return bean;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        public class ViewRankedPrev : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select prevwindow(ev) as win, prev(0, ev) as prev0, prev(1, ev) as prev1, prev(2, ev) as prev2, prev(3, ev) as prev3, prev(4, ev) as prev4 " +
                    "from SupportBean#rank(TheString, 3, IntPrimitive) as ev";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 100, 0L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E1", 100, 0L}});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 99, 0L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E2", 99, 0L}, new object[] {"E1", 100, 0L}});

                env.SendEventBean(MakeEvent("E1", 98, 1L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E1", 98, 1L}, new object[] {"E2", 99, 0L}});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E3", 98, 0L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E1", 98, 1L}, new object[] {"E3", 98, 0L}, new object[] {"E2", 99, 0L}});

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E2", 97, 1L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E2", 97, 1L}, new object[] {"E1", 98, 1L}, new object[] {"E3", 98, 0L}});

                env.UndeployAll();
            }
        }

        internal class ViewRankPrevAndGroupWin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select prevwindow(ev) as win, prev(0, ev) as prev0, prev(1, ev) as prev1, prev(2, ev) as prev2, prev(3, ev) as prev3, prev(4, ev) as prev4 " +
                    "from SupportBean#rank(TheString, 3, IntPrimitive) as ev";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(MakeEvent("E1", 100, 0L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E1", 100, 0L}});

                env.SendEventBean(MakeEvent("E2", 99, 0L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E2", 99, 0L}, new object[] {"E1", 100, 0L}});

                env.SendEventBean(MakeEvent("E1", 98, 1L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E1", 98, 1L}, new object[] {"E2", 99, 0L}});

                env.SendEventBean(MakeEvent("E3", 98, 0L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E1", 98, 1L}, new object[] {"E3", 98, 0L}, new object[] {"E2", 99, 0L}});

                env.SendEventBean(MakeEvent("E2", 97, 1L));
                AssertWindowAggAndPrev(
                    env,
                    new[] {new object[] {"E2", 97, 1L}, new object[] {"E1", 98, 1L}, new object[] {"E3", 98, 0L}});
                env.UndeployAll();

                epl =
                    "@Name('s0') select irstream * from SupportBean#groupwin(TheString)#rank(IntPrimitive, 2, DoublePrimitive) as ev";
                env.CompileDeployAddListenerMile(epl, "s0", 1);

                var fields = new [] { "TheString","IntPrimitive","LongPrimitive","DoublePrimitive" };
                env.SendEventBean(MakeEvent("E1", 100, 0L, 1d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 100, 0L, 1d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 100, 0L, 1d}});

                env.SendEventBean(MakeEvent("E2", 100, 0L, 2d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 100, 0L, 2d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 100, 0L, 1d}, new object[] {"E2", 100, 0L, 2d}});

                env.SendEventBean(MakeEvent("E1", 200, 0L, 0.5d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 200, 0L, 0.5d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 200, 0L, 0.5d}, new object[] {"E1", 100, 0L, 1d},
                        new object[] {"E2", 100, 0L, 2d}
                    });

                env.SendEventBean(MakeEvent("E2", 200, 0L, 2.5d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 200, 0L, 2.5d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 200, 0L, 0.5d}, new object[] {"E1", 100, 0L, 1d},
                        new object[] {"E2", 100, 0L, 2d}, new object[] {"E2", 200, 0L, 2.5d}
                    });

                env.SendEventBean(MakeEvent("E1", 300, 0L, 0.1d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E1", 300, 0L, 0.1d},
                    new object[] {"E1", 100, 0L, 1d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 300, 0L, 0.1d}, new object[] {"E1", 200, 0L, 0.5d},
                        new object[] {"E2", 100, 0L, 2d}, new object[] {"E2", 200, 0L, 2.5d}
                    });

                env.UndeployAll();
            }
        }

        internal class ViewRankMultiexpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","IntPrimitive","LongPrimitive","DoublePrimitive" };
                var epl =
                    "@Name('s0') select irstream * from SupportBean#rank(TheString, IntPrimitive, 3, LongPrimitive, DoublePrimitive)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(MakeEvent("E1", 100, 1L, 10d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 100, 1L, 10d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 100, 1L, 10d}});

                env.SendEventBean(MakeEvent("E1", 200, 1L, 9d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 200, 1L, 9d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 200, 1L, 9d}, new object[] {"E1", 100, 1L, 10d}});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E1", 150, 1L, 11d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 150, 1L, 11d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 200, 1L, 9d}, new object[] {"E1", 100, 1L, 10d},
                        new object[] {"E1", 150, 1L, 11d}
                    });

                env.SendEventBean(MakeEvent("E1", 100, 1L, 8d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E1", 100, 1L, 8d},
                    new object[] {"E1", 100, 1L, 10d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 100, 1L, 8d}, new object[] {"E1", 200, 1L, 9d},
                        new object[] {"E1", 150, 1L, 11d}
                    });

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E2", 300, 2L, 7d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E2", 300, 2L, 7d},
                    new object[] {"E2", 300, 2L, 7d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 100, 1L, 8d}, new object[] {"E1", 200, 1L, 9d},
                        new object[] {"E1", 150, 1L, 11d}
                    });

                env.SendEventBean(MakeEvent("E3", 300, 1L, 8.5d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E3", 300, 1L, 8.5d},
                    new object[] {"E1", 150, 1L, 11d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 100, 1L, 8d}, new object[] {"E3", 300, 1L, 8.5d},
                        new object[] {"E1", 200, 1L, 9d}
                    });

                env.Milestone(3);

                env.SendEventBean(MakeEvent("E4", 400, 1L, 9d));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E4", 400, 1L, 9d},
                    new object[] {"E1", 200, 1L, 9d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 100, 1L, 8d}, new object[] {"E3", 300, 1L, 8.5d},
                        new object[] {"E4", 400, 1L, 9d}
                    });

                env.UndeployAll();
            }
        }

        internal class ViewRankRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","IntPrimitive","LongPrimitive" };
                var epl =
                    "@Name('create') create window MyWindow#rank(TheString, 3, IntPrimitive asc) as SupportBean;\n" +
                    "insert into MyWindow select * from SupportBean;\n" +
                    "@Name('s0') select irstream * from MyWindow;\n" +
                    "on SupportBean_A delete from MyWindow mw where TheString = Id;\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(MakeEvent("E1", 10, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10, 0L}});

                env.SendEventBean(MakeEvent("E2", 50, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 50, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10, 0L}, new object[] {"E2", 50, 0L}});

                env.SendEventBean(MakeEvent("E3", 5, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 5, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 5, 0L}, new object[] {"E1", 10, 0L}, new object[] {"E2", 50, 0L}});

                env.SendEventBean(MakeEvent("E4", 5, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E4", 5, 0L},
                    new object[] {"E2", 50, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 5, 0L}, new object[] {"E4", 5, 0L}, new object[] {"E1", 10, 0L}});

                env.SendEventBean(new SupportBean_A("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3", 5, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E4", 5, 0L}, new object[] {"E1", 10, 0L}});

                env.SendEventBean(new SupportBean_A("E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4", 5, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10, 0L}});

                env.SendEventBean(new SupportBean_A("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 10, 0L});
                EPAssertionUtil.AssertPropsPerRow(env.Statement("create").GetEnumerator(), fields, new object[0][]);

                env.SendEventBean(MakeEvent("E3", 100, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 100, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 100, 0L}});

                env.SendEventBean(MakeEvent("E3", 101, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E3", 101, 1L},
                    new object[] {"E3", 100, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("create").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 101, 1L}});

                env.SendEventBean(new SupportBean_A("E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3", 101, 1L});
                EPAssertionUtil.AssertPropsPerRow(env.Statement("create").GetEnumerator(), fields, new object[0][]);

                env.UndeployAll();
            }
        }

        public class ViewRankedSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","IntPrimitive","LongPrimitive" };
                var epl = "@Name('s0') select irstream * from SupportBean.ext:rank(TheString, 3, IntPrimitive)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "A", 10, 100L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 10, 100L});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 10, 100L}});
                SendSupportBean(env, "B", 20, 101L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 20, 101L});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 10, 100L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "A", 8, 102L); // replace A
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"A", 8, 102L},
                    new object[] {"A", 10, 100L});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 8, 102L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "C", 15, 103L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"C", 15, 103L});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 8, 102L}, new object[] {"C", 15, 103L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "D", 21, 104L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"D", 21, 104L},
                    new object[] {"D", 21, 104L});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 8, 102L}, new object[] {"C", 15, 103L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "A", 16, 105L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"A", 16, 105L},
                    new object[] {"A", 8, 102L});

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"C", 15, 103L}, new object[] {"A", 16, 105L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "C", 16, 106L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"C", 16, 106L},
                    new object[] {"C", 15, 103L});

                env.Milestone(7);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 16, 105L}, new object[] {"C", 16, 106L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "C", 16, 107L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"C", 16, 107L},
                    new object[] {"C", 16, 106L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"A", 16, 105L}, new object[] {"C", 16, 107L}, new object[] {"B", 20, 101L}});
                SendSupportBean(env, "E", 1, 108L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E", 1, 108L},
                    new object[] {"B", 20, 101L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E", 1, 108L}, new object[] {"A", 16, 105L}, new object[] {"C", 16, 107L}});

                env.UndeployAll();
            }
        }

        internal class ViewRankRanked : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString","IntPrimitive","LongPrimitive" };
                var epl = "@Name('s0') select irstream * from SupportBean#rank(TheString, 4, IntPrimitive desc)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 10, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 10, 0L}});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 30, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 30, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", 30, 0L}, new object[] {"E1", 10, 0L}});

                env.SendEventBean(MakeEvent("E1", 50, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E1", 50, 0L},
                    new object[] {"E1", 10, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 50, 0L}, new object[] {"E2", 30, 0L}});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E3", 40, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 40, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 50, 0L}, new object[] {"E3", 40, 0L}, new object[] {"E2", 30, 0L}});

                env.SendEventBean(MakeEvent("E2", 45, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E2", 45, 0L},
                    new object[] {"E2", 30, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 50, 0L}, new object[] {"E2", 45, 0L}, new object[] {"E3", 40, 0L}});

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E1", 43, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E1", 43, 0L},
                    new object[] {"E1", 50, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E3", 40, 0L}});

                env.SendEventBean(MakeEvent("E3", 50, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E3", 50, 0L},
                    new object[] {"E3", 40, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E3", 50, 0L}, new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}});

                env.Milestone(3);

                env.SendEventBean(MakeEvent("E3", 10, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E3", 10, 0L},
                    new object[] {"E3", 50, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E3", 10, 0L}});

                env.SendEventBean(MakeEvent("E4", 43, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 43, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E4", 43, 0L},
                        new object[] {"E3", 10, 0L}
                    });

                env.Milestone(4);

                // in-place replacement
                env.SendEventBean(MakeEvent("E4", 43, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E4", 43, 1L},
                    new object[] {"E4", 43, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 0L}, new object[] {"E1", 43, 0L}, new object[] {"E4", 43, 1L},
                        new object[] {"E3", 10, 0L}
                    });

                env.SendEventBean(MakeEvent("E2", 45, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E2", 45, 1L},
                    new object[] {"E2", 45, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 1L}, new object[] {"E1", 43, 0L}, new object[] {"E4", 43, 1L},
                        new object[] {"E3", 10, 0L}
                    });

                env.Milestone(5);

                env.SendEventBean(MakeEvent("E1", 43, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E1", 43, 1L},
                    new object[] {"E1", 43, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L},
                        new object[] {"E3", 10, 0L}
                    });

                // out-of-space: pushing out the back end
                env.SendEventBean(MakeEvent("E5", 10, 2L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E5", 10, 2L},
                    new object[] {"E3", 10, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L},
                        new object[] {"E5", 10, 2L}
                    });

                env.Milestone(6);

                env.SendEventBean(MakeEvent("E5", 11, 3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E5", 11, 3L},
                    new object[] {"E5", 10, 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L},
                        new object[] {"E5", 11, 3L}
                    });

                env.SendEventBean(MakeEvent("E6", 43, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E6", 43, 0L},
                    new object[] {"E5", 11, 3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E2", 45, 1L}, new object[] {"E4", 43, 1L}, new object[] {"E1", 43, 1L},
                        new object[] {"E6", 43, 0L}
                    });

                env.Milestone(7);

                env.SendEventBean(MakeEvent("E7", 50, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E7", 50, 0L},
                    new object[] {"E4", 43, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E7", 50, 0L}, new object[] {"E2", 45, 1L}, new object[] {"E1", 43, 1L},
                        new object[] {"E6", 43, 0L}
                    });

                env.SendEventBean(MakeEvent("E8", 45, 0L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E8", 45, 0L},
                    new object[] {"E1", 43, 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E7", 50, 0L}, new object[] {"E2", 45, 1L}, new object[] {"E8", 45, 0L},
                        new object[] {"E6", 43, 0L}
                    });

                env.Milestone(8);

                env.SendEventBean(MakeEvent("E8", 46, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertPairGetIRAndReset(),
                    fields,
                    new object[] {"E8", 46, 1L},
                    new object[] {"E8", 45, 0L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E7", 50, 0L}, new object[] {"E8", 46, 1L}, new object[] {"E2", 45, 1L},
                        new object[] {"E6", 43, 0L}
                    });

                env.UndeployAll();
            }
        }

        internal class ViewRankInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#rank(1, IntPrimitive desc)",
                    "Failed to validate data window declaration: Rank view requires a list of expressions providing unique keys, a numeric size parameter and a list of expressions providing sort keys [select * from SupportBean#rank(1, IntPrimitive desc)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#rank(1, IntPrimitive, TheString desc)",
                    "Failed to validate data window declaration: Failed to find unique value expressions that are expected to occur before the numeric size parameter [select * from SupportBean#rank(1, IntPrimitive, TheString desc)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#rank(TheString, IntPrimitive, 1)",
                    "Failed to validate data window declaration: Failed to find sort key expressions after the numeric size parameter [select * from SupportBean#rank(TheString, IntPrimitive, 1)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#rank(TheString, IntPrimitive, TheString desc)",
                    "Failed to validate data window declaration: Failed to find constant value for the numeric size parameter [select * from SupportBean#rank(TheString, IntPrimitive, TheString desc)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#rank(TheString, 1, 1, IntPrimitive, TheString desc)",
                    "Failed to validate data window declaration: Invalid view parameter expression 2 for Rank view, the expression returns a constant result value, are you sure? [select * from SupportBean#rank(TheString, 1, 1, IntPrimitive, TheString desc)]");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "select * from SupportBean#rank(TheString, IntPrimitive, 1, IntPrimitive, 1, TheString desc)",
                    "Failed to validate data window declaration: Invalid view parameter expression 4 for Rank view, the expression returns a constant result value, are you sure? [select * from SupportBean#rank(TheString, IntPrimitive, 1, IntPrimitive, 1, TheString desc)]");
            }
        }
    }
} // end of namespace