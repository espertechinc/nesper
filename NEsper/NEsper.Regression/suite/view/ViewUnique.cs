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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewUnique
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewLastUniqueSceneOne(null));
            execs.Add(new ViewLastUniqueSceneTwo(null));
            execs.Add(new ViewLastUniqueWithAnnotationPrefix(null));
            execs.Add(new ViewUniqueExpressionParameter());
            execs.Add(new ViewUniqueTwoWindows());
            return execs;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            string feed,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, feed);
            return bean;
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price)
        {
            var bean = new SupportMarketDataBean(symbol, price, 0L, "");
            return bean;
        }

        public class ViewLastUniqueSceneOne : RegressionExecution
        {
            private readonly string optionalAnnotation;

            public ViewLastUniqueSceneOne(string optionalAnnotation)
            {
                this.optionalAnnotation = optionalAnnotation;
            }

            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select irstream Symbol, price from SupportMarketDataBean#unique(Symbol) order by Symbol";
                if (optionalAnnotation != null) {
                    text = optionalAnnotation + text;
                }

                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S1", 100));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S1"},
                            new object[] {"Price", 100.0}
                        },
                        null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", 5));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S2"},
                            new object[] {"Price", 5.0}
                        },
                        null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S1", 101));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S1"},
                            new object[] {"Price", 101.0}
                        },
                        new[] {new object[] {"Symbol", "S1"}, new object[] {"Price", 100.0}});

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", 102));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S1"},
                            new object[] {"Price", 102.0}
                        },
                        new[] {new object[] {"Symbol", "S1"}, new object[] {"Price", 101.0}});

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    new[] {"Price"},
                    new[] {new object[] {102.0}, new object[] {5.0}});

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S2", 6));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S2"},
                            new object[] {"Price", 6.0}
                        },
                        new[] {new object[] {"Symbol", "S2"}, new object[] {"Price", 5.0}});

                env.UndeployAll();
            }
        }

        public class ViewLastUniqueSceneTwo : RegressionExecution
        {
            private readonly string optionalAnnotation;

            public ViewLastUniqueSceneTwo(string optionalAnnotation)
            {
                this.optionalAnnotation = optionalAnnotation;
            }

            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@Name('s0') select irstream Symbol, Feed, price from  SupportMarketDataBean#unique(Symbol, Feed) order by Symbol, Feed";
                if (optionalAnnotation != null) {
                    text = optionalAnnotation + text;
                }

                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S1", "F1", 100));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S1"},
                            new object[] {"Feed", "F1"},
                            new object[] {"Price", 100.0}
                        },
                        null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", "F1", 5));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S2"},
                            new object[] {"Feed", "F1"},
                            new object[] {"Price", 5.0}
                        },
                        null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S1", "F1", 101));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S1"},
                            new object[] {"Feed", "F1"},
                            new object[] {"Price", 101.0}
                        },
                        new[] {
                            new object[] {"Symbol", "S1"}, new object[] {"Feed", "F1"}, new object[] {"Price", 100.0}
                        });

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S2", "F1", 102));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S2"},
                            new object[] {"Feed", "F1"},
                            new object[] {"Price", 102.0}
                        },
                        new[] {
                            new object[] {"Symbol", "S2"}, new object[] {"Feed", "F1"}, new object[] {"Price", 5.0}
                        });

                // test iterator
                var events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    events,
                    new[] {"Price"},
                    new[] {new object[] {101.0}, new object[] {102.0}});

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S1", "F2", 6));
                env.Listener("s0")
                    .AssertNewOldData(
                        new[] {
                            new object[] {"Symbol", "S1"},
                            new object[] {"Feed", "F2"},
                            new object[] {"Price", 6.0}
                        },
                        null);

                // test iterator
                events = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    events,
                    new[] {"Price"},
                    new[] {new object[] {101.0}, new object[] {6.0}, new object[] {102.0}});

                env.UndeployAll();
            }
        }

        public class ViewLastUniqueWithAnnotationPrefix : RegressionExecution
        {
            private readonly string optionalAnnotations;

            public ViewLastUniqueWithAnnotationPrefix(string optionalAnnotations)
            {
                this.optionalAnnotations = optionalAnnotations;
            }

            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1".SplitCsv();
                var epl =
                    "@Name('s0') select irstream TheString as c0, IntPrimitive as c1 from SupportBean#unique(TheString)";
                if (optionalAnnotations != null) {
                    epl = optionalAnnotations + epl;
                }

                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}});
                SendSupportBean(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 20}});
                SendSupportBean(env, "E1", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E1", 2},
                    new object[] {"E1", 1});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 2}, new object[] {"E2", 20}});
                SendSupportBean(env, "E2", 21);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E2", 21},
                    new object[] {"E2", 20});

                env.Milestone(5);
                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 2}, new object[] {"E2", 21}});
                SendSupportBean(env, "E2", 22);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E2", 22},
                    new object[] {"E2", 21});
                SendSupportBean(env, "E1", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E1", 3},
                    new object[] {"E1", 2});

                SendSupportBean(env, "E3", 30);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 30});

                env.UndeployAll();
            }
        }

        internal class ViewUniqueExpressionParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from SupportBean#unique(Math.abs(IntPrimitive))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSupportBean(env, "E1", 10);
                SendSupportBean(env, "E2", -10);
                SendSupportBean(env, "E3", -5);
                SendSupportBean(env, "E4", 5);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    "TheString".SplitCsv(),
                    new[] {new object[] {"E2"}, new object[] {"E4"}});

                env.UndeployAll();
            }
        }

        internal class ViewUniqueTwoWindows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select irstream * from SupportBean#unique(IntBoxed)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var beanOne = new SupportBean("E1", 1);
                env.SendEventBean(beanOne);
                env.Listener("s0").AssertOneGetNewAndReset();

                var eplTwo = "@Name('s1') select irstream * from SupportBean#unique(IntBoxed)";
                env.CompileDeployAddListenerMile(eplTwo, "s1", 1);

                var beanTwo = new SupportBean("E2", 2);
                env.SendEventBean(beanTwo);

                Assert.AreEqual(beanTwo, env.Listener("s0").LastNewData[0].Underlying);
                Assert.AreEqual(beanOne, env.Listener("s0").LastOldData[0].Underlying);
                Assert.AreEqual(beanTwo, env.Listener("s1").LastNewData[0].Underlying);
                Assert.IsNull(env.Listener("s1").LastOldData);

                env.UndeployAll();
            }
        }
    }
} // end of namespace