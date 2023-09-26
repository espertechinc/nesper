///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.view.derived;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewDerived
    {
        private const string SYMBOL = "CSCO.O";
        private const string FEED = "feed1";

        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSizeSceneOne(execs);
            WithSizeSceneTwo(execs);
            WithSizeAddProps(execs);
            WithDerivedAll(execs);
            WithDerivedLengthWUniSceneOne(execs);
            WithDerivedLengthWUniSceneTwo(execs);
            WithDerivedLengthWUniSceneThree(execs);
            WithDerivedLengthWWeightedAvgSceneOne(execs);
            WithDerivedLengthWWeightedAvgSceneTwo(execs);
            WithDerivedLengthWRegressionLinestSceneOne(execs);
            WithDerivedLengthWRegressionLinestSceneTwo(execs);
            WithDerivedLengthWCorrelation(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWCorrelation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWCorrelation());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWRegressionLinestSceneTwo(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWRegressionLinestSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWRegressionLinestSceneOne(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWRegressionLinestSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWWeightedAvgSceneTwo(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWWeightedAvgSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWWeightedAvgSceneOne(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWWeightedAvgSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWUniSceneThree(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWUniSceneThree());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWUniSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWUniSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedLengthWUniSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedLengthWUniSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithDerivedAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewDerivedAll());
            return execs;
        }

        public static IList<RegressionExecution> WithSizeAddProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSizeAddProps());
            return execs;
        }

        public static IList<RegressionExecution> WithSizeSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSizeSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSizeSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ViewSizeSceneOne());
            return execs;
        }

        private class ViewSizeSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream size from SupportMarketDataBean#size";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "DELL", 1L);
                AssertSize(env, 1, 0);

                SendEvent(env, "DELL", 1L);
                AssertSize(env, 2, 1);

                env.UndeployAll();

                epl = "@name('s0') select size, symbol, feed from SupportMarketDataBean#size(symbol, feed)";
                env.CompileDeployAddListenerMile(epl, "s0", 1);
                var fields = "size,symbol,feed".SplitCsv();

                SendEvent(env, "DELL", 1L);
                env.AssertPropsNew("s0", fields, new object[] { 1L, "DELL", "feed1" });

                SendEvent(env, "DELL", 1L);
                env.AssertPropsNew("s0", fields, new object[] { 2L, "DELL", "feed1" });

                env.UndeployAll();
            }
        }

        public class ViewSizeSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from SupportMarketDataBean#size()";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "size", 1L } },
                    new object[][] { new object[] { "size", 0L } });

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "size", 2L } },
                    new object[][] { new object[] { "size", 1L } });

                env.Milestone(2);

                for (var i = 3; i < 10; i++) {
                    env.SendEventBean(MakeMarketDataEvent("E" + i));
                    env.AssertPropsNV(
                        "s0",
                        new object[][] { new object[] { "size", (long)i } }, // new data
                        new object[][] { new object[] { "size", (long)i - 1 } } //  old data
                    );

                    env.Milestone(i);
                }

                // test iterator
                env.AssertPropsPerRowIterator("s0", new string[] { "size" }, new object[][] { new object[] { 9L } });

                env.UndeployAll();
            }
        }

        public class ViewSizeAddProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from  SupportMarketDataBean#size(symbol)";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "size", 1L }, new object[] { "symbol", "E1" } },
                    new object[][] { new object[] { "size", 0L }, new object[] { "symbol", null } });

                env.Milestone(0);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "size", 2L }, new object[] { "symbol", "E2" } },
                    new object[][] { new object[] { "size", 1L }, new object[] { "symbol", "E1" } });

                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "size", "symbol" },
                    new object[][] { new object[] { 2L, "E2" } });

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWUniSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from  SupportMarketDataBean#length(3)#uni(price)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeBean(50, "f1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "total", 50d }, new object[] { "datapoints", 1L } },
                    new object[][] { new object[] { "total", 0.0 }, new object[] { "datapoints", 0L } });

                env.Milestone(1);

                env.SendEventBean(MakeBean(25, "f2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "total", 75.0 }, new object[] { "datapoints", 2L } },
                    new object[][] { new object[] { "total", 50d }, new object[] { "datapoints", 1L } });

                env.Milestone(2);

                env.SendEventBean(MakeBean(25, "f3"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "total", 100.0 }, new object[] { "datapoints", 3L } },
                    new object[][] { new object[] { "total", 75d }, new object[] { "datapoints", 2L } });

                env.Milestone(3);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "total", "datapoints" },
                    new object[][] { new object[] { 100.0, 3L } });

                env.SendEventBean(MakeBean(1, "f4"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "total", 51.0 }, new object[] { "datapoints", 3L } },
                    new object[][] { new object[] { "total", 100d }, new object[] { "datapoints", 3L } });

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWUniSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select irstream * from SupportMarketDataBean(symbol='" +
                          SYMBOL +
                          "')#length(3)#uni(price, symbol, feed)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("average"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("variance"));
                        Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("datapoints"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("total"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("stddev"));
                        Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("stddevpa"));
                    });

                SendEvent(env, SYMBOL, 100);
                CheckOld(env, true, 0, 0, double.NaN, double.NaN, double.NaN, double.NaN);
                CheckNew(env, 1, 100, 100, 0, double.NaN, double.NaN);

                SendEvent(env, SYMBOL, 100.5);
                CheckOld(env, false, 1, 100, 100, 0, double.NaN, double.NaN);
                CheckNew(env, 2, 200.5, 100.25, 0.25, 0.353553391, 0.125);

                SendEvent(env, "DUMMY", 100.5);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, SYMBOL, 100.7);
                CheckOld(env, false, 2, 200.5, 100.25, 0.25, 0.353553391, 0.125);
                CheckNew(env, 3, 301.2, 100.4, 0.294392029, 0.360555128, 0.13);

                SendEvent(env, SYMBOL, 100.6);
                CheckOld(env, false, 3, 301.2, 100.4, 0.294392029, 0.360555128, 0.13);
                CheckNew(env, 3, 301.8, 100.6, 0.081649658, 0.1, 0.01);

                SendEvent(env, SYMBOL, 100.9);
                CheckOld(env, false, 3, 301.8, 100.6, 0.081649658, 0.1, 0.01);
                CheckNew(env, 3, 302.2, 100.733333333, 0.124721913, 0.152752523, 0.023333333);
                env.UndeployAll();

                // test select-star
                var eplWildcard = "@name('s0') select * from SupportBean#length(3)#uni(intPrimitive, *)";
                env.CompileDeployAddListenerMile(eplWildcard, "s0", 1);

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => {
                        Assert.AreEqual(1.0, @event.Get("average"));
                        Assert.AreEqual("E1", @event.Get("theString"));
                        Assert.AreEqual(1, @event.Get("intPrimitive"));
                    });

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWUniSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from SupportMarketDataBean#length(3)#uni(price, feed)";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeBean(50, "f1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "total", 50d }, new object[] { "datapoints", 1L }, new object[] { "feed", "f1" }
                    },
                    new object[][] {
                        new object[] { "total", 0.0 }, new object[] { "datapoints", 0L }, new object[] { "feed", null }
                    });

                env.Milestone(0);

                env.SendEventBean(MakeBean(25, "f2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "total", 75.0 }, new object[] { "datapoints", 2L }, new object[] { "feed", "f2" }
                    },
                    new object[][] {
                        new object[] { "total", 50d }, new object[] { "datapoints", 1L }, new object[] { "feed", "f1" }
                    });

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWWeightedAvgSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select irstream * from SupportMarketDataBean#length(3)#weighted_avg(price, volume)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeBean(10, 1000));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10d } },
                    new object[][] { new object[] { "average", double.NaN } });

                env.Milestone(1);

                env.SendEventBean(MakeBean(11, 2000));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10.666666666666666 } },
                    new object[][] { new object[] { "average", 10.0 } });

                env.Milestone(2);

                env.SendEventBean(MakeBean(10.5, 1500));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10.61111111111111 } },
                    new object[][] { new object[] { "average", 10.666666666666666 } });

                env.Milestone(3);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "average" },
                    new object[][] { new object[] { 10.61111111111111 } });

                env.SendEventBean(MakeBean(9.5, 600));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10.597560975609756 } },
                    new object[][] { new object[] { "average", 10.61111111111111 } });

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWWeightedAvgSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select irstream * from SupportMarketDataBean#length(3)#weighted_avg(price, volume, feed)";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeBean(10, 1000, "f1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10d }, new object[] { "feed", "f1" } },
                    new object[][] { new object[] { "average", double.NaN }, new object[] { "feed", null } });

                env.Milestone(0);

                env.SendEventBean(MakeBean(11, 2000, "f2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10.666666666666666 }, new object[] { "feed", "f2" } },
                    new object[][] { new object[] { "average", 10.0 }, new object[] { "feed", "f1" } });

                env.Milestone(1);

                env.SendEventBean(MakeBean(10.5, 1500, "f3"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "average", 10.61111111111111 }, new object[] { "feed", "f3" } },
                    new object[][] { new object[] { "average", 10.666666666666666 }, new object[] { "feed", "f2" } });

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWRegressionLinestSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from SupportMarketDataBean#length(3)#linest(price, volume)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeBean(70, 1000));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "slope", double.NaN }, new object[] { "YIntercept", double.NaN } },
                    new object[][] { new object[] { "slope", double.NaN }, new object[] { "YIntercept", double.NaN } });
                env.Milestone(1);

                env.SendEventBean(MakeBean(70.5, 1500));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "slope", 1000.0 }, new object[] { "YIntercept", -69000.0 } },
                    new object[][] { new object[] { "slope", double.NaN }, new object[] { "YIntercept", double.NaN } });
                env.Milestone(2);

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "slope", "YIntercept" },
                    new object[][] { new object[] { 1000.0, -69000.0 } });

                env.SendEventBean(MakeBean(70.1, 1200));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "slope", 928.571428587354 }, new object[] { "YIntercept", -63952.38095349892 }
                    },
                    new object[][] { new object[] { "slope", 1000.0 }, new object[] { "YIntercept", -69000.0 } });
                env.Milestone(3);

                env.SendEventBean(MakeBean(70.25, 1000));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "slope", 877.5510204634593 }, new object[] { "YIntercept", -60443.8775549068 }
                    },
                    new object[][] {
                        new object[] { "slope", 928.571428587354 }, new object[] { "YIntercept", -63952.38095349892 }
                    });
                env.Milestone(4);

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWRegressionLinestSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text =
                    "@name('s0') select irstream * from SupportMarketDataBean#length(3)#linest(price, volume, feed)";
                env.CompileDeploy(text).AddListener("s0");

                env.SendEventBean(MakeBean(70, 1000, "f1"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "slope", double.NaN }, new object[] { "YIntercept", double.NaN },
                        new object[] { "feed", "f1" }
                    },
                    new object[][] {
                        new object[] { "slope", double.NaN }, new object[] { "YIntercept", double.NaN },
                        new object[] { "feed", null }
                    });

                env.Milestone(0);

                env.SendEventBean(MakeBean(70.5, 1500, "f2"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "slope", 1000.0 }, new object[] { "YIntercept", -69000.0 },
                        new object[] { "feed", "f2" }
                    },
                    new object[][] {
                        new object[] { "slope", double.NaN }, new object[] { "YIntercept", double.NaN },
                        new object[] { "feed", "f1" }
                    });

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    new string[] { "slope", "YIntercept", "feed" },
                    new object[][] { new object[] { 1000.0, -69000.0, "f2" } });

                env.SendEventBean(MakeBean(70.1, 1200, "f3"));
                env.AssertPropsNV(
                    "s0",
                    new object[][] {
                        new object[] { "slope", 928.571428587354 }, new object[] { "YIntercept", -63952.38095349892 },
                        new object[] { "feed", "f3" }
                    },
                    new object[][] {
                        new object[] { "slope", 1000.0 }, new object[] { "YIntercept", -69000.0 },
                        new object[] { "feed", "f2" }
                    });

                env.UndeployAll();
            }
        }

        public class ViewDerivedLengthWCorrelation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@name('s0') select irstream * from SupportMarketDataBean#length(3)#correl(price, volume)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeBean(70, 1000));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "correlation", double.NaN } },
                    new object[][] { new object[] { "correlation", double.NaN } });

                env.Milestone(1);

                env.SendEventBean(MakeBean(70.5, 1500));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "correlation", 1.0 } },
                    new object[][] { new object[] { "correlation", double.NaN } });

                env.Milestone(2);

                env.SendEventBean(MakeBean(70.1, 1200));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "correlation", 0.9762210399358 } },
                    new object[][] { new object[] { "correlation", 1.0 } });

                // test iterator
                env.AssertPropsPerRowIterator(
                    "s0",
                    "correlation".SplitCsv(),
                    new object[][] { new object[] { 0.9762210399358 } });

                env.Milestone(3);

                env.SendEventBean(MakeBean(70.25, 1000));
                env.AssertPropsNV(
                    "s0",
                    new object[][] { new object[] { "correlation", 0.7046340397673054 } },
                    new object[][] { new object[] { "correlation", 0.9762210399358 } });

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        public class ViewDerivedAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // correlation
                var f1 = "correlation".SplitCsv();
                epl = "@name('S1') select irstream * from SupportMarketDataBean#correl(price, volume)";
                env.CompileDeploy(epl).AddListener("S1");

                // size
                var f2 = "size".SplitCsv();
                epl = "@name('S2') select irstream * from SupportMarketDataBean#size()";
                env.CompileDeploy(epl).AddListener("S2");

                // regression
                var f3 = "slope,YIntercept".SplitCsv();
                epl = "@name('S3') select irstream * from SupportMarketDataBean#linest(price, volume)";
                env.CompileDeploy(epl).AddListener("S3");

                // stat:uni
                var f4 = "total,datapoints".SplitCsv();
                epl = "@name('S4') select irstream * from SupportMarketDataBean#uni(volume)";
                env.CompileDeploy(epl).AddListener("S4");

                // stat:weighted_avg
                var f5 = "average".SplitCsv();
                epl = "@name('S5') select irstream * from SupportMarketDataBean#weighted_avg(price, volume)";
                env.CompileDeploy(epl).AddListener("S5");

                env.Milestone(0);

                env.SendEventBean(MakeBean(70, 1000));
                env.AssertPropsNV(
                    "S1",
                    new object[][] { new object[] { "correlation", double.NaN } },
                    new object[][] { new object[] { "correlation", double.NaN } });
                env.AssertPropsIRPair("S2", f2, new object[] { 1L }, new object[] { 0L });
                env.AssertPropsIRPair(
                    "S3",
                    f3,
                    new object[] { double.NaN, double.NaN },
                    new object[] { double.NaN, double.NaN });
                env.AssertPropsIRPair("S4", f4, new object[] { 1000.0, 1L }, new object[] { 0.0, 0L });
                env.AssertPropsIRPair("S5", f5, new object[] { 70.0 }, new object[] { double.NaN });

                env.Milestone(1);

                env.SendEventBean(MakeBean(70.5, 1500));
                env.AssertPropsNV(
                    "S1",
                    new object[][] { new object[] { "correlation", 1.0 } },
                    new object[][] { new object[] { "correlation", double.NaN } });
                env.AssertPropsIRPair("S2", f2, new object[] { 2L }, new object[] { 1L });
                env.AssertPropsIRPair(
                    "S3",
                    f3,
                    new object[] { 1000.0, -69000.0 },
                    new object[] { double.NaN, double.NaN });
                env.AssertPropsIRPair("S4", f4, new object[] { 2500.0, 2L }, new object[] { 1000.0, 1L });
                env.AssertPropsIRPair(
                    "S5",
                    f5,
                    new object[] { (70.0 * 1000 + 70.5 * 1500) / 2500.0 },
                    new object[] { 70.0 });

                env.Milestone(2);

                env.SendEventBean(MakeBean(70.1, 1200));
                env.AssertPropsNV(
                    "S1",
                    new object[][] { new object[] { "correlation", 0.9762210399358 } },
                    new object[][] { new object[] { "correlation", 1.0 } });
                env.AssertPropsIRPair("S2", f2, new object[] { 3L }, new object[] { 2L });
                env.AssertPropsIRPair(
                    "S3",
                    f3,
                    new object[] { 928.571428587354, -63952.38095349892 },
                    new object[] { 1000.0, -69000.0 });
                env.AssertPropsIRPair("S4", f4, new object[] { 3700.0, 3L }, new object[] { 2500.0, 2L });
                env.AssertPropsIRPair(
                    "S5",
                    f5,
                    new object[] { (70.0 * 1000 + 70.5 * 1500 + 70.1 * 1200) / 3700.0 },
                    new object[] { (70.0 * 1000 + 70.5 * 1500) / 2500.0 });

                // test iterator
                env.AssertPropsPerRowIterator("S1", f1, new object[][] { new object[] { 0.9762210399358 } });
                env.AssertPropsPerRowIterator("S2", f2, new object[][] { new object[] { 3L } });
                env.AssertPropsPerRowIterator(
                    "S3",
                    f3,
                    new object[][] { new object[] { 928.571428587354, -63952.38095349892 } });
                env.AssertPropsPerRowIterator("S4", f4, new object[][] { new object[] { 3700.0, 3L } });
                env.AssertPropsPerRowIterator(
                    "S5",
                    f5,
                    new object[][] { new object[] { (70.0 * 1000 + 70.5 * 1500 + 70.1 * 1200) / 3700.0 } });

                env.Milestone(3);

                env.Milestone(4);

                env.SendEventBean(MakeBean(70.25, 1000));
                env.AssertPropsNV(
                    "S1",
                    new object[][] { new object[] { "correlation", 0.7865410694065471 } },
                    new object[][] { new object[] { "correlation", 0.9762210399358 } });
                env.AssertPropsIRPair("S2", f2, new object[] { 4L }, new object[] { 3L });
                env.AssertPropsPerRowIterator(
                    "S3",
                    f3,
                    new object[][] { new object[] { 854.6255506976092, -58830.39647835589 } });
                env.AssertPropsIRPair("S4", f4, new object[] { 4700.0, 4L }, new object[] { 3700.0, 3L });
                env.AssertPropsIRPair(
                    "S5",
                    f5,
                    new object[] { (70.0 * 1000 + 70.5 * 1500 + 70.1 * 1200 + 70.25 * 1000) / 4700.0 },
                    new object[] { (70.0 * 1000 + 70.5 * 1500 + 70.1 * 1200) / 3700.0 });

                env.UndeployAll();
            }

            private SupportMarketDataBean MakeBean(
                double price,
                long volume)
            {
                return new SupportMarketDataBean("", price, volume, "");
            }
        }

        private static void AssertSize(
            RegressionEnvironment env,
            long newSize,
            long oldSize)
        {
            env.AssertPropsPerRowIRPair(
                "s0",
                "size".SplitCsv(),
                new object[][] { new object[] { newSize } },
                new object[][] { new object[] { oldSize } });
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static SupportMarketDataBean MakeBean(
            double price,
            string feed)
        {
            return new SupportMarketDataBean("", price, -1L, feed);
        }

        private static void CheckNew(
            RegressionEnvironment env,
            long countE,
            double sumE,
            double avgE,
            double stdevpaE,
            double stdevE,
            double varianceE)
        {
            env.AssertIterator(
                "s0",
                iterator => {
                    CheckValues(iterator.Advance(), false, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
                    Assert.IsFalse(iterator.MoveNext());
                });

            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(1, listener.LastNewData.Length);
                    var childViewValues = listener.LastNewData[0];
                    CheckValues(childViewValues, false, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
                    listener.Reset();
                });
        }

        private static void CheckOld(
            RegressionEnvironment env,
            bool isFirst,
            long countE,
            double sumE,
            double avgE,
            double stdevpaE,
            double stdevE,
            double varianceE)
        {
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(1, listener.LastOldData.Length);
                    var childViewValues = listener.LastOldData[0];
                    CheckValues(childViewValues, isFirst, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
                });
        }

        private static void CheckValues(
            EventBean values,
            bool isFirst,
            bool isNewData,
            long countE,
            double sumE,
            double avgE,
            double stdevpaE,
            double stdevE,
            double varianceE)
        {
            var count = GetLongValue(ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS, values);
            var sum = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL, values);
            var avg = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE, values);
            var stdevpa = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA, values);
            var stdev = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV, values);
            var variance = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE, values);

            Assert.AreEqual(count, countE);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(sum, sumE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stdevpa, stdevpaE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stdev, stdevE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(variance, varianceE, 6));
            if (isFirst && !isNewData) {
                Assert.AreEqual(null, values.Get("symbol"));
                Assert.AreEqual(null, values.Get("feed"));
            }
            else {
                Assert.AreEqual(SYMBOL, values.Get("symbol"));
                Assert.AreEqual(FEED, values.Get("feed"));
            }
        }

        private static double GetDoubleValue(
            ViewFieldEnum field,
            EventBean values)
        {
            return values.Get(field.GetName()).AsDouble();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, FEED);
            env.SendEventBean(theEvent);
        }

        private static SupportMarketDataBean MakeBean(
            double price,
            long volume)
        {
            return new SupportMarketDataBean("", price, volume, "");
        }

        private static long GetLongValue(
            ViewFieldEnum field,
            EventBean values)
        {
            return values.Get(field.GetName()).AsInt64();
        }

        private static SupportMarketDataBean MakeBean(
            double price,
            long volume,
            string feed)
        {
            return new SupportMarketDataBean("", price, volume, feed);
        }
    }
} // end of namespace