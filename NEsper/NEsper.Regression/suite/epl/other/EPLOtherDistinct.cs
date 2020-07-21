///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using SupportBean = com.espertech.esper.common.@internal.support.SupportBean;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;
using SupportBean_N = com.espertech.esper.regressionlib.support.bean.SupportBean_N;

namespace com.espertech.esper.regressionlib.suite.epl.other
{
    public class EPLOtherDistinct
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLOtherOutputSimpleColumn());
            execs.Add(new EPLOtherBatchWindow());
            execs.Add(new EPLOtherBatchWindowJoin());
            execs.Add(new EPLOtherBatchWindowInsertInto());
            execs.Add(new EPLOtherOnDemandAndOnSelect());
            execs.Add(new EPLOtherSubquery());
            execs.Add(new EPLOtherBeanEventWildcardThisProperty());
            execs.Add(new EPLOtherBeanEventWildcardSODA());
            execs.Add(new EPLOtherBeanEventWildcardPlusCols());
            execs.Add(new EPLOtherMapEventWildcard());
            execs.Add(new EPLOtherOutputLimitEveryColumn());
            execs.Add(new EPLOtherOutputRateSnapshotColumn());
            execs.Add(new EPLOtherDistinctWildcardJoinPatternOne());
            execs.Add(new EPLOtherDistinctWildcardJoinPatternTwo());
            execs.Add(new EPLOtherDistinctOutputLimitMultikeyWArraySingleArray());
            execs.Add(new EPLOtherDistinctOutputLimitMultikeyWArrayTwoArray());
            execs.Add(new EPLOtherDistinctFireAndForgetMultikeyWArray());
            execs.Add(new EPLOtherDistinctIterateMultikeyWArray());
            execs.Add(new EPLOtherDistinctOnSelectMultikeyWArray());
            execs.Add(new EPLOtherDistinctVariantStream());

            return execs;
        }

        private static void TryAssertionOutputEvery(
            RegressionEnvironment env,
            string[] fields)
        {
            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}});
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
            env.Listener("s0").Reset();

            env.SendEventBean(new SupportBean("E2", 2));
            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E2", 2}, new object[] {"E1", 1}});
            env.Listener("s0").Reset();

            env.Milestone(0);

            env.SendEventBean(new SupportBean("E2", 3));
            env.SendEventBean(new SupportBean("E2", 3));
            env.SendEventBean(new SupportBean("E2", 3));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E2", 3}});
            env.Listener("s0").Reset();
        }

        private static void TryAssertionSimpleColumn(
            RegressionEnvironment env,
            SupportListener listener,
            EPStatement stmt,
            string[] fields)
        {
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 1});

            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 1});

            env.SendEventBean(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 1}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 1});

            env.SendEventBean(new SupportBean("E1", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 2});

            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 2});

            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 2});

            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 1});
        }

        private static void TryAssertionSnapshotColumn(
            RegressionEnvironment env,
            SupportListener listener,
            EPStatement stmt,
            string[] fields)
        {
            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}});
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

            env.SendEventBean(new SupportBean("E2", 2));
            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
            env.Listener("s0").Reset();

            env.SendEventBean(new SupportBean("E3", 3));
            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").LastNewData,
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            env.Listener("s0").Reset();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendMapEvent(
            RegressionEnvironment env,
            string s,
            int i)
        {
            IDictionary<string, object> def = new Dictionary<string, object>();
            def.Put("k1", s);
            def.Put("v1", i);
            env.SendEventMap(def, "MyMapTypeKVDistinct");
        }

        private static void SendManyArray(
            RegressionEnvironment env,
            int[] intOne,
            int[] intTwo)
        {
            env.SendEventBean(new SupportEventWithManyArray("id").WithIntOne(intOne).WithIntTwo(intTwo));
        }

        private static void SendManyArray(
            RegressionEnvironment env,
            int[] ints)
        {
            env.SendEventBean(new SupportEventWithManyArray("id").WithIntOne(ints));
        }

        internal class EPLOtherDistinctVariantStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create variant schema MyVariant as SupportEventWithManyArray;\n" +
                             "insert into MyVariant select * from SupportEventWithManyArray;\n" +
                             "@name('s0') select distinct * from MyVariant#keepall;\n" +
                             "@name('s1') select distinct intOne from MyVariant#keepall;\n" +
                             "@name('s2') select distinct intOne, intTwo from MyVariant#keepall;\n";
                env.CompileDeploy(epl);

                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 5});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});

                Assert.AreEqual(3, EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0")).Length);
                Assert.AreEqual(2, EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s1")).Length);
                Assert.AreEqual(3, EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s2")).Length);

                env.UndeployAll();
            }
        }

        internal class EPLOtherDistinctOnSelectMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create window MyWindow#keepall as SupportEventWithManyArray;\n" +
                             "insert into MyWindow select * from SupportEventWithManyArray;\n" +
                             "@name('s0') on SupportBean_S0 select distinct intOne from MyWindow;\n" +
                             "@name('s1') on SupportBean_S1 select distinct intOne, intTwo from MyWindow;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 5});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});

                env.SendEventBean(new SupportBean_S0(0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "intOne".SplitCsv(),
                    new object[][] { 
                        new object[] {new int[] {1, 2}},  
                        new object[] {new int[] {3, 4}}});

                env.SendEventBean(new SupportBean_S1(0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s1").GetAndResetLastNewData(),
                    "intOne,intTwo".SplitCsv(),
                    new object[][] {
                         
                        new object[] {new int[] {1, 2}, new int[] {3, 4}},  
                        new object[] {new int[] {3, 4}, new int[] {1, 2}},  
                        new object[] {new int[] {1, 2}, new int[] {3, 5}}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLOtherDistinctIterateMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl =
                    "@name('s0') select distinct intOne from SupportEventWithManyArray#keepall;\n" +
                    "@name('s1') select distinct intOne, intTwo from SupportEventWithManyArray#keepall;\n";
                env.CompileDeploy(epl);

                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 5});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    "intOne".SplitCsv(),
                    new object[][] { 
                        new object[] {new int[] {1, 2}},  
                        new object[] {new int[] {3, 4}}});

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s1"),
                    "intOne,intTwo".SplitCsv(),
                    new object[][] {
                        new object[] {new int[] {1, 2}, new int[] {3, 4}}, 
                        new object[] {new int[] {3, 4}, new int[] {1, 2}}, 
                        new object[] {new int[] {1, 2}, new int[] {3, 5}}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLOtherDistinctFireAndForgetMultikeyWArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RegressionPath path = new RegressionPath();
                string epl = "@name('s0') create window MyWindow#keepall as SupportEventWithManyArray;\n" +
                             "insert into MyWindow select * from SupportEventWithManyArray;\n";
                env.CompileDeploy(epl, path);

                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 5});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});

                EPFireAndForgetQueryResult result = env.CompileExecuteFAF("select distinct intOne from MyWindow", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    "intOne".SplitCsv(),
                    new object[][] {
                        new object[] {new int[] {1, 2}}, 
                        new object[] {new int[] {3, 4}}});

                result = env.CompileExecuteFAF("select distinct intOne, intTwo from MyWindow", path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    "intOne,intTwo".SplitCsv(),
                    new object[][] {
                        new object[] {new int[] {1, 2}, new int[] {3, 4}}, 
                        new object[] {new int[] {3, 4}, new int[] {1, 2}}, 
                        new object[] {new int[] {1, 2}, new int[] {3, 5}}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLOtherDistinctOutputLimitMultikeyWArrayTwoArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                string epl = "@name('s0') select distinct intOne, intTwo from SupportEventWithManyArray output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 5});
                SendManyArray(env, new int[] {3, 4}, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2}, new int[] {3, 4});

                env.AdvanceTime(1000);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "intOne,intTwo".SplitCsv(),
                    new object[][] {
                        new object[] {new int[] {1, 2}, new int[] {3, 4}}, 
                        new object[] {new int[] {3, 4}, new int[] {1, 2}}, 
                        new object[] {new int[] {1, 2}, new int[] {3, 5}}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLOtherDistinctOutputLimitMultikeyWArraySingleArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                string epl = "@name('s0') select distinct intOne from SupportEventWithManyArray output every 1 seconds";
                env.CompileDeploy(epl).AddListener("s0");

                SendManyArray(env, new int[] {1, 2});
                SendManyArray(env, new int[] {2, 1});
                SendManyArray(env, new int[] {2, 3});
                SendManyArray(env, new int[] {1, 2});
                SendManyArray(env, new int[] {1, 2});

                env.AdvanceTime(1000);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    "intOne".SplitCsv(),
                    new object[][] { 
                        new object[] {new int[] {1, 2}},  
                        new object[] {new int[] {2, 1}},  
                        new object[] {new int[] {2, 3}}});

                env.UndeployAll();
            }
        }

        internal class EPLOtherDistinctWildcardJoinPatternOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select distinct * from " +
                          "SupportBean(IntPrimitive=0) as fooB unidirectional " +
                          "inner join " +
                          "pattern [" +
                          "every-distinct(fooA.TheString) fooA=SupportBean(IntPrimitive=1)" +
                          "->" +
                          "every-distinct(wooA.TheString) wooA=SupportBean(IntPrimitive=2)" +
                          " where timer:within(1 hour)" +
                          "]#time(1 hour) as fooWooPair " +
                          "on fooB.LongPrimitive = fooWooPair.fooA.LongPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "E1", 1, 10L);
                SendEvent(env, "E1", 2, 10L);

                env.Milestone(0);

                SendEvent(env, "E2", 1, 10L);
                SendEvent(env, "E2", 2, 10L);

                SendEvent(env, "E3", 1, 10L);
                SendEvent(env, "E3", 2, 10L);

                SendEvent(env, "Query", 0, 10L);
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class EPLOtherDistinctWildcardJoinPatternTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select distinct * from " +
                          "SupportBean(IntPrimitive=0) as fooB unidirectional " +
                          "inner join " +
                          "pattern [" +
                          "every-distinct(fooA.TheString) fooA=SupportBean(IntPrimitive=1)" +
                          "->" +
                          "every-distinct(wooA.TheString) wooA=SupportBean(IntPrimitive=2)" +
                          " where timer:within(1 hour)" +
                          "]#time(1 hour) as fooWooPair " +
                          "on fooB.LongPrimitive = fooWooPair.fooA.LongPrimitive" +
                          " order by fooWooPair.wooA.TheString asc";
                env.CompileDeploy(epl);
                var subscriber = new SupportSubscriberMRD();
                env.Statement("s0").Subscriber = subscriber;

                SendEvent(env, "E1", 1, 10L);
                SendEvent(env, "E2", 2, 10L);
                SendEvent(env, "E3", 2, 10L);
                SendEvent(env, "Query", 0, 10L);

                Assert.IsTrue(subscriber.IsInvoked());
                Assert.AreEqual(1, subscriber.InsertStreamList.Count);
                var inserted = subscriber.InsertStreamList[0];
                Assert.AreEqual(2, inserted.Length);
                Assert.AreEqual("Query", ((SupportBean) inserted[0][0]).TheString);
                Assert.AreEqual("Query", ((SupportBean) inserted[1][0]).TheString);
                var mapOne = (IDictionary<string, object>) inserted[0][1];
                Assert.AreEqual("E2", ((EventBean) mapOne.Get("wooA")).Get("TheString"));
                Assert.AreEqual("E1", ((EventBean) mapOne.Get("fooA")).Get("TheString"));
                var mapTwo = (IDictionary<string, object>) inserted[1][1];
                Assert.AreEqual("E3", ((EventBean) mapTwo.Get("wooA")).Get("TheString"));
                Assert.AreEqual("E1", ((EventBean) mapTwo.Get("fooA")).Get("TheString"));

                env.UndeployAll();
            }

            private static void SendEvent(
                RegressionEnvironment env,
                string theString,
                int intPrimitive,
                long longPrimitive)
            {
                var bean = new SupportBean(theString, intPrimitive);
                bean.LongPrimitive = longPrimitive;
                env.SendEventBean(bean);
            }
        }

        internal class EPLOtherOnDemandAndOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow#keepall as select * from SupportBean", path);
                env.CompileDeploy("insert into MyWindow select * from SupportBean", path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E1", 1));

                var query = "select distinct TheString, IntPrimitive from MyWindow order by TheString, IntPrimitive";
                var result = env.CompileExecuteFAF(query, path);
                EPAssertionUtil.AssertPropsPerRow(
                    result.Array,
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});

                env.CompileDeploy(
                        "@name('s0') on SupportBean_A select distinct TheString, IntPrimitive from MyWindow order by TheString, IntPrimitive asc",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean_A("x"));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E1", 2}, new object[] {"E2", 2}});

                env.UndeployAll();
            }
        }

        internal class EPLOtherSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                env.CompileDeploy(
                    "@name('s0') select * from SupportBean where TheString in (select distinct Id from SupportBean_A#keepall)");
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 2});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E1"));
                env.SendEventBean(new SupportBean("E1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 3});

                env.UndeployAll();
            }
        } // Since the "this" property will always be unique, this test verifies that condition

        internal class EPLOtherBeanEventWildcardThisProperty : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var statementText = "@name('s0') select distinct * from SupportBean#keepall";
                env.CompileDeploy(statementText);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 1}
                    });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 1},
                        new object[] {"E2", 2}
                    });

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {
                        new object[] {"E1", 1},
                        new object[] {"E2", 2}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLOtherBeanEventWildcardSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"Id"};
                var statementText = "@name('s0') select distinct * from SupportBean_A#keepall";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}});

                env.SendEventBean(new SupportBean_A("E2"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E1"));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                var model = env.EplToModel(statementText);
                Assert.AreEqual(statementText, model.ToEPL());

                model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard().Distinct(true);
                model.FromClause = FromClause.Create(FilterStream.Create("SupportBean_A"));
                Assert.AreEqual("select distinct * from SupportBean_A", model.ToEPL());

                env.UndeployAll();
            }
        }

        internal class EPLOtherBeanEventWildcardPlusCols : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"IntPrimitive", "val1", "val2"};
                var statementText =
                    "@name('s0') select distinct *, IntBoxed%5 as val1, IntBoxed as val2 from SupportBean_N#keepall";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean_N(1, 8));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, 3, 8}});

                env.SendEventBean(new SupportBean_N(1, 3));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, 3, 8}, new object[] {1, 3, 3}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_N(1, 8));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, 3, 8}, new object[] {1, 3, 3}});

                env.UndeployAll();
            }
        }

        internal class EPLOtherMapEventWildcard : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"k1", "v1"};
                var statementText = "@name('s0') select distinct * from MyMapTypeKVDistinct#keepall";
                env.CompileDeploy(statementText).AddListener("s0");

                SendMapEvent(env, "E1", 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}});

                SendMapEvent(env, "E2", 2);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                env.Milestone(0);

                SendMapEvent(env, "E1", 1);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                env.UndeployAll();
            }
        }

        internal class EPLOtherOutputSimpleColumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var statementText = "@name('s0') select distinct TheString, IntPrimitive from SupportBean#keepall";
                env.CompileDeploy(statementText).AddListener("s0");

                TryAssertionSimpleColumn(env, env.Listener("s0"), env.Statement("s0"), fields);
                env.UndeployAll();

                // test join
                statementText =
                    "@name('s0') select distinct TheString, IntPrimitive from SupportBean#keepall a, SupportBean_A#keepall b where a.TheString = b.Id";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                env.SendEventBean(new SupportBean_A("E2"));
                TryAssertionSimpleColumn(env, env.Listener("s0"), env.Statement("s0"), fields);

                env.UndeployAll();
            }
        }

        internal class EPLOtherOutputLimitEveryColumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var statementText =
                    "@name('s0') @IterableUnbound select distinct TheString, IntPrimitive from SupportBean output every 3 events";
                env.CompileDeploy(statementText).AddListener("s0");

                TryAssertionOutputEvery(env, fields);
                env.UndeployAll();

                // test join
                statementText =
                    "@name('s0') select distinct TheString, IntPrimitive from SupportBean#lastevent a, SupportBean_A#keepall b where a.TheString = b.Id output every 3 events";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                env.SendEventBean(new SupportBean_A("E2"));
                TryAssertionOutputEvery(env, fields);

                env.UndeployAll();
            }
        }

        internal class EPLOtherOutputRateSnapshotColumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var statementText =
                    "@name('s0') select distinct TheString, IntPrimitive from SupportBean#keepall output snapshot every 3 events order by TheString asc";
                env.CompileDeploy(statementText).AddListener("s0");

                TryAssertionSnapshotColumn(env, env.Listener("s0"), env.Statement("s0"), fields);
                env.UndeployAll();

                statementText =
                    "@name('s0') select distinct TheString, IntPrimitive from SupportBean#keepall a, SupportBean_A#keepall b where a.TheString = b.Id output snapshot every 3 events order by TheString asc";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_A("E2"));
                env.SendEventBean(new SupportBean_A("E3"));
                TryAssertionSnapshotColumn(env, env.Listener("s0"), env.Statement("s0"), fields);

                env.UndeployAll();
            }
        }

        internal class EPLOtherBatchWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var statementText =
                    "@name('s0') select distinct TheString, IntPrimitive from SupportBean#length_batch(3)";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}});
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", 2}, new object[] {"E1", 1}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean("E2", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"E2", 3}});

                env.UndeployAll();

                // test batch window with aggregation
                env.AdvanceTime(0);
                string[] fieldsTwo = {"c1", "c2"};
                var epl =
                    "@name('s0') insert into ABC select distinct TheString as c1, first(IntPrimitive) as c2 from SupportBean#time_batch(1 second)";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                env.AdvanceTime(1000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsTwo,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 1}});

                env.AdvanceTime(2000);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class EPLOtherBatchWindowJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var statementText =
                    "@name('s0') select distinct TheString, IntPrimitive from SupportBean#length_batch(3) a, SupportBean_A#keepall b where a.TheString = b.Id";
                env.CompileDeploy(statementText).AddListener("s0");

                env.SendEventBean(new SupportBean_A("E1"));
                env.SendEventBean(new SupportBean_A("E2"));

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E2", 2}, new object[] {"E1", 1}});

                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean("E2", 3));
                env.SendEventBean(new SupportBean("E2", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastNewData,
                    fields,
                    new[] {new object[] {"E2", 3}});

                env.UndeployAll();
            }
        }

        internal class EPLOtherBatchWindowInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"TheString", "IntPrimitive"};
                var path = new RegressionPath();

                var statementText =
                    "insert into MyStream select distinct TheString, IntPrimitive from SupportBean#length_batch(3)";
                env.CompileDeploy(statementText, path);

                statementText = "@name('s0') select * from MyStream";
                env.CompileDeploy(statementText, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E3", 3));
                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").NewDataListFlattened[0],
                    fields,
                    new object[] {"E2", 2});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").NewDataListFlattened[1],
                    fields,
                    new object[] {"E3", 3});

                env.UndeployAll();
            }
        }
    }
} // end of namespace