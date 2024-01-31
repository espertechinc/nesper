///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewUnique
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithLastUniqueSceneOne(execs);
            WithLastUniqueSceneTwo(execs);
            WithLastUniqueWithAnnotationPrefix(execs);
            WithUniqueExpressionParameter(execs);
            WithUniqueTwoWindows(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithUniqueTwoWindows(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUniqueTwoWindows());
            return execs;
        }

        public static IList<RegressionExecution> WithUniqueExpressionParameter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewUniqueExpressionParameter());
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueWithAnnotationPrefix(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLastUniqueWithAnnotationPrefix(null));
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLastUniqueSceneTwo(null));
            return execs;
        }

        public static IList<RegressionExecution> WithLastUniqueSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewLastUniqueSceneOne(null));
            return execs;
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
                    "@name('s0') select irstream Symbol, Price from SupportMarketDataBean#unique(Symbol) order by Symbol";
                if (optionalAnnotation != null) {
                    text = optionalAnnotation + text;
                }

                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S1", 100));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 100.0 } },
                    null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", 5));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S2" }, new object[] { "Price", 5.0 } },
                    null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S1", 101));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 101.0 } },
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 100.0 } });

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S1", 102));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 102.0 } },
                    new object[][] { new object[] { "Symbol", "S1" }, new object[] { "Price", 101.0 } });

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "Price" },
                    new object[][] { new object[] { 102.0 }, new object[] { 5.0 } });

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S2", 6));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "Symbol", "S2" }, new object[] { "Price", 6.0 } },
                    new object[][] { new object[] { "Symbol", "S2" }, new object[] { "Price", 5.0 } });

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
                    "@name('s0') select irstream Symbol, Feed, Price from  SupportMarketDataBean#unique(Symbol, Feed) order by Symbol, Feed";
                if (optionalAnnotation != null) {
                    text = optionalAnnotation + text;
                }

                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("S1", "F1", 100));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "Symbol", "S1" }, new object[] { "Feed", "F1" }, new object[] { "Price", 100.0 }
                    },
                    null);

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("S2", "F1", 5));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "Symbol", "S2" }, new object[] { "Feed", "F1" }, new object[] { "Price", 5.0 }
                    },
                    null);

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("S1", "F1", 101));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "Symbol", "S1" }, new object[] { "Feed", "F1" }, new object[] { "Price", 101.0 }
                    },
                    new object[][] {
                        new object[] { "Symbol", "S1" }, new object[] { "Feed", "F1" }, new object[] { "Price", 100.0 }
                    });

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("S2", "F1", 102));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "Symbol", "S2" }, new object[] { "Feed", "F1" }, new object[] { "Price", 102.0 }
                    },
                    new object[][] {
                        new object[] { "Symbol", "S2" }, new object[] { "Feed", "F1" }, new object[] { "Price", 5.0 }
                    });

                // test iterator
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    new string[] { "Price" },
                    new object[][] { new object[] { 101.0 }, new object[] { 102.0 } });

                env.Milestone(4);

                env.SendEventBean(MakeMarketDataEvent("S1", "F2", 6));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "Symbol", "S1" }, new object[] { "Feed", "F2" }, new object[] { "Price", 6.0 }
                    },
                    null);

                // test iterator
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    new string[] { "Price" },
                    new object[][] { new object[] { 101.0 }, new object[] { 6.0 }, new object[] { 102.0 } });

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
                    "@name('s0') select irstream TheString as c0, IntPrimitive as c1 from SupportBean#unique(TheString)";
                if (optionalAnnotations != null) {
                    epl = optionalAnnotations + epl;
                }

                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E1", 1);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

                env.Milestone(2);

                env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][] { new object[] { "E1", 1 } });
                SendSupportBean(env, "E2", 20);
                env.AssertPropsNew("s0", fields, new object[] { "E2", 20 });

                env.Milestone(3);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 20 } });
                SendSupportBean(env, "E1", 2);
                env.AssertPropsIRPair("s0", fields, new object[] { "E1", 2 }, new object[] { "E1", 1 });

                env.Milestone(4);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 2 }, new object[] { "E2", 20 } });
                SendSupportBean(env, "E2", 21);
                env.AssertPropsIRPair("s0", fields, new object[] { "E2", 21 }, new object[] { "E2", 20 });

                env.Milestone(5);
                env.Milestone(6);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 2 }, new object[] { "E2", 21 } });
                SendSupportBean(env, "E2", 22);
                env.AssertPropsIRPair("s0", fields, new object[] { "E2", 22 }, new object[] { "E2", 21 });
                SendSupportBean(env, "E1", 3);
                env.AssertPropsIRPair("s0", fields, new object[] { "E1", 3 }, new object[] { "E1", 2 });

                SendSupportBean(env, "E3", 30);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 30 });

                env.UndeployAll();
            }
        }

        private class ViewUniqueExpressionParameter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * from SupportBean#unique(Math.Abs(IntPrimitive))";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendSupportBean(env, "E1", 10);
                SendSupportBean(env, "E2", -10);
                SendSupportBean(env, "E3", -5);
                SendSupportBean(env, "E4", 5);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    "TheString".SplitCsv(),
                    new object[][] { new object[] { "E2" }, new object[] { "E4" } });

                env.UndeployAll();
            }
        }

        private class ViewUniqueTwoWindows : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream * from SupportBean#unique(IntBoxed)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                var beanOne = new SupportBean("E1", 1);
                env.SendEventBean(beanOne);
                env.AssertListenerInvoked("s0");

                var eplTwo = "@name('s1') select irstream * from SupportBean#unique(IntBoxed)";
                env.CompileDeployAddListenerMile(eplTwo, "s1", 1);

                var beanTwo = new SupportBean("E2", 2);
                env.SendEventBean(beanTwo);

                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.AreEqual(beanTwo, listener.LastNewData[0].Underlying);
                        ClassicAssert.AreEqual(beanOne, listener.LastOldData[0].Underlying);
                    });
                env.AssertListener(
                    "s1",
                    listener => {
                        ClassicAssert.AreEqual(beanTwo, listener.LastNewData[0].Underlying);
                        ClassicAssert.IsNull(listener.LastOldData);
                    });

                env.UndeployAll();
            }
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
    }
} // end of namespace