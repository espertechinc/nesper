///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    ///     NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowViews
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraKeepAllSimple());
            execs.Add(new InfraKeepAllSceneTwo());
            execs.Add(new InfraBeanBacked());
            execs.Add(new InfraTimeWindow());
            execs.Add(new InfraTimeWindowSceneTwo());
            execs.Add(new InfraTimeFirstWindow());
            execs.Add(new InfraExtTimeWindow());
            execs.Add(new InfraExtTimeWindowSceneTwo());
            execs.Add(new InfraExtTimeWindowSceneThree());
            execs.Add(new InfraTimeOrderWindow());
            execs.Add(new InfraTimeOrderSceneTwo());
            execs.Add(new InfraLengthWindow());
            execs.Add(new InfraLengthWindowSceneTwo());
            execs.Add(new InfraLengthFirstWindow());
            execs.Add(new InfraTimeAccum());
            execs.Add(new InfraTimeAccumSceneTwo());
            execs.Add(new InfraTimeBatch());
            execs.Add(new InfraTimeBatchSceneTwo());
            execs.Add(new InfraTimeBatchLateConsumer());
            execs.Add(new InfraLengthBatch());
            execs.Add(new InfraLengthBatchSceneTwo());
            execs.Add(new InfraSortWindow());
            execs.Add(new InfraSortWindowSceneTwo());
            execs.Add(new InfraTimeLengthBatch());
            execs.Add(new InfraTimeLengthBatchSceneTwo());
            execs.Add(new InfraLengthWindowSceneThree());
            execs.Add(new InfraLengthWindowPerGroup());
            execs.Add(new InfraTimeBatchPerGroup());
            execs.Add(new InfraDoubleInsertSameWindow());
            execs.Add(new InfraLastEvent());
            execs.Add(new InfraLastEventSceneTwo());
            execs.Add(new InfraFirstEvent());
            execs.Add(new InfraUnique());
            execs.Add(new InfraUniqueSceneTwo());
            execs.Add(new InfraFirstUnique());
            execs.Add(new InfraBeanContained());
            execs.Add(new InfraIntersection());
            execs.Add(new InfraBeanSchemaBacked());
            execs.Add(new InfraDeepSupertypeInsert());
            execs.Add(new InfraWithDeleteUseAs());
            execs.Add(new InfraWithDeleteFirstAs());
            execs.Add(new InfraWithDeleteSecondAs());
            execs.Add(new InfraWithDeleteNoAs());
            execs.Add(new InfraFilteringConsumer());
            execs.Add(new InfraSelectGroupedViewLateStart());
            execs.Add(new InfraFilteringConsumerLateStart());
            execs.Add(new InfraInvalid());
            execs.Add(new InfraNamedWindowInvalidAlreadyExists());
            execs.Add(new InfraNamedWindowInvalidConsumerDataWindow());
            execs.Add(new InfraPriorStats());
            execs.Add(new InfraLateConsumer());
            execs.Add(new InfraLateConsumerJoin());
            execs.Add(new InfraPattern());
            execs.Add(new InfraExternallyTimedBatch());
            execs.Add(new InfraSelectStreamDotStarInsert());
            execs.Add(new InfraSelectGroupedViewLateStartVariableIterate());
            execs.Add(new InfraOnInsertPremptiveTwoWindow());
            return execs;
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            env.SendEventBean(new SupportBean_A(id));
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
            return bean;
        }

        private static void SendSupportBeanInt(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            env.SendEventBean(bean);
        }

        private static void TryAssertionBeanContained(
            RegressionEnvironment env,
            EventRepresentationChoice rep)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                rep.GetAnnotationText() + " @Name('create') create window MyWindowBC#keepall as (bean SupportBean_S0)",
                path);
            env.AddListener("create");
            Assert.IsTrue(rep.MatchesClass(env.Statement("create").EventType.UnderlyingType));
            env.CompileDeploy("insert into MyWindowBC select bean.* as bean from SupportBean_S0 as bean", path);

            env.SendEventBean(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetNewAndReset(),
                new [] { "bean.P00" },
                new object[] {"E1"});

            env.UndeployAll();
        }

        private static void TryCreateWindow(
            RegressionEnvironment env,
            string createWindowStatement,
            string deleteStatement)
        {
            var fields = new[] {"key", "value"};
            var path = new RegressionPath();

            var epl = "@Name('create') " +
                      createWindowStatement +
                      ";\n" +
                      "@Name('insert') insert into MyWindow select TheString as key, LongBoxed as value from SupportBean;\n" +
                      "@Name('s0') select irstream key, value*2 as value from MyWindow;\n" +
                      "@Name('s2') select irstream key, sum(value) as value from MyWindow group by key;\n" +
                      "@Name('s3') select irstream key, value from MyWindow where value >= 10;\n";
            env.CompileDeploy(epl, path).AddListener("create").AddListener("s0").AddListener("s2").AddListener("s3");

            Assert.AreEqual(typeof(string), env.Statement("create").EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long?), env.Statement("create").EventType.GetPropertyType("value"));

            // send events
            SendSupportBean(env, "E1", 10L);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 20L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastNewData[0],
                fields,
                new object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastOldData[0],
                fields,
                new object[] {"E1", null});
            env.Listener("s2").Reset();
            EPAssertionUtil.AssertProps(
                env.Listener("s3").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"E1", 10L}});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {"E1", 20L}});

            SendSupportBean(env, "E2", 20L);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 40L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastNewData[0],
                fields,
                new object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastOldData[0],
                fields,
                new object[] {"E2", null});
            env.Listener("s2").Reset();
            EPAssertionUtil.AssertProps(
                env.Listener("s3").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"E1", 10L}, new object[] {"E2", 20L}});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("s0"),
                fields,
                new[] {new object[] {"E1", 20L}, new object[] {"E2", 40L}});

            SendSupportBean(env, "E3", 5L);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", 10L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastNewData[0],
                fields,
                new object[] {"E3", 5L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastOldData[0],
                fields,
                new object[] {"E3", null});
            env.Listener("s2").Reset();
            Assert.IsFalse(env.Listener("s3").IsInvoked);
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E3", 5L});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"E1", 10L}, new object[] {"E2", 20L}, new object[] {"E3", 5L}});

            // create delete stmt
            env.CompileDeploy("@Name('delete') " + deleteStatement, path).AddListener("delete");

            // send delete event
            SendMarketBean(env, "E1");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E1", 20L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastNewData[0],
                fields,
                new object[] {"E1", null});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastOldData[0],
                fields,
                new object[] {"E1", 10L});
            env.Listener("s2").Reset();
            EPAssertionUtil.AssertProps(
                env.Listener("s3").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E1", 10L});
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"E2", 20L}, new object[] {"E3", 5L}});

            // send delete event again, none deleted now
            SendMarketBean(env, "E1");
            Assert.IsFalse(env.Listener("s0").IsInvoked);
            Assert.IsFalse(env.Listener("s2").IsInvoked);
            Assert.IsFalse(env.Listener("create").IsInvoked);
            Assert.IsTrue(env.Listener("delete").IsInvoked);
            env.Listener("delete").Reset();
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"E2", 20L}, new object[] {"E3", 5L}});

            // send delete event
            SendMarketBean(env, "E2");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E2", 40L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastNewData[0],
                fields,
                new object[] {"E2", null});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastOldData[0],
                fields,
                new object[] {"E2", 20L});
            env.Listener("s2").Reset();
            EPAssertionUtil.AssertProps(
                env.Listener("s3").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E2", 20L});
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(
                env.GetEnumerator("create"),
                fields,
                new[] {new object[] {"E3", 5L}});

            // send delete event
            SendMarketBean(env, "E3");
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E3", 10L});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastNewData[0],
                fields,
                new object[] {"E3", null});
            EPAssertionUtil.AssertProps(
                env.Listener("s2").LastOldData[0],
                fields,
                new object[] {"E3", 5L});
            env.Listener("s2").Reset();
            Assert.IsFalse(env.Listener("s3").IsInvoked);
            EPAssertionUtil.AssertProps(
                env.Listener("create").AssertOneGetOldAndReset(),
                fields,
                new object[] {"E3", 5L});
            Assert.IsTrue(env.Listener("delete").IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

            env.UndeployModuleContaining("delete");
            env.UndeployModuleContaining("s0");
        }

        private static void TryAssertionBeanBacked(
            RegressionEnvironment env,
            EventRepresentationChoice eventRepresentationEnum)
        {
            var path = new RegressionPath();
            // Test create from class
            env.CompileDeploy(
                eventRepresentationEnum.GetAnnotationText() +
                " @Name('create') create window MyWindowBB#keepall as SupportBean",
                path);
            env.AddListener("create");
            env.CompileDeploy("insert into MyWindowBB select * from SupportBean", path);

            env.CompileDeploy("@Name('s0') select * from MyWindowBB", path).AddListener("s0");
            AssertStatelessStmt(env, "s0", true);

            env.SendEventBean(new SupportBean());
            AssertEvent(env.Listener("create").AssertOneGetNewAndReset(), "MyWindowBB");
            AssertEvent(env.Listener("s0").AssertOneGetNewAndReset(), "MyWindowBB");

            env.CompileDeploy("@Name('update') on SupportBean_A update MyWindowBB set TheString='s'", path)
                .AddListener("update");
            env.SendEventBean(new SupportBean_A("A1"));
            AssertEvent(env.Listener("update").LastNewData[0], "MyWindowBB");

            // test bean-property
            env.UndeployAll();
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(bean);
        }

        private static void SendTimer(
            RegressionEnvironment env,
            long timeInMSec)
        {
            env.AdvanceTime(timeInMSec);
        }

        private static void AssertEvent(
            EventBean theEvent,
            string name)
        {
            Assert.IsTrue(theEvent.EventType is BeanEventType);
            Assert.IsTrue(theEvent.Underlying is SupportBean);
            Assert.AreEqual(EventTypeTypeClass.NAMED_WINDOW, theEvent.EventType.Metadata.TypeClass);
            Assert.AreEqual(name, theEvent.EventType.Name);
        }

        public static Schema GetSupportBeanS0Schema()
        {
            return SchemaBuilder.Record("SupportBean_S0", TypeBuilder.RequiredString("P00"));
        }

        public class InfraKeepAllSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString" };

                var path = new RegressionPath();
                var eplCreate = "@Name('create') create window MyWindow.win:keepall() as SupportBean";
                env.CompileDeploy(eplCreate, path).AddListener("create");

                var eplInsert = "@Name('insert') insert into MyWindow select * from SupportBean";
                env.CompileDeploy(eplInsert, path);

                env.Milestone(0);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("create");

                env.Milestone(2);
            }
        }

        public class InfraKeepAllSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:keepall() as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "@Name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                SendBeanInt(env, "G1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(0);

                // send event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20});

                env.Milestone(1);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 20});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});

                env.Milestone(2);

                // send event G3
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G3", 30);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 30});

                env.Milestone(3);

                // delete event G1
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G3", 30}});
                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G1", 10});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}});

                env.Milestone(4);

                // send event G4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}});
                SendBeanInt(env, "G4", 40);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G4", 40});

                env.Milestone(5);

                // send event G5
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}});
                SendBeanInt(env, "G5", 50);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G5", 50});

                env.Milestone(6);

                // send event G6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}, new object[] {"G5", 50}});
                SendBeanInt(env, "G6", 60);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G6", 60});

                env.Milestone(7);

                // delete event G6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {
                        new object[] {"G3", 30}, new object[] {"G4", 40}, new object[] {"G5", 50},
                        new object[] {"G6", 60}
                    });
                SendMarketBean(env, "G6");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G6", 60});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}, new object[] {"G5", 50}});

                // destroy all
                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("create");
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraSelectStreamDotStarInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.ARRAY.GetAnnotationText() +
                    " create window MyNWWindowObjectArray#keepall (p0 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyNWWindowObjectArray select IntPrimitive as p0, sb.* as c0 from SupportBean as sb",
                    path);
                env.UndeployAll();
            }
        }

        internal class InfraBeanBacked : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryAssertionBeanBacked(env, EventRepresentationChoice.ARRAY);
                TryAssertionBeanBacked(env, EventRepresentationChoice.MAP);
                TryAssertionBeanBacked(env, EventRepresentationChoice.DEFAULT);

                try {
                    TryAssertionBeanBacked(env, EventRepresentationChoice.AVRO);
                }
                catch (EPCompileExceptionItem ex) {
                    AssertMessage(
                        ex,
                        "Error starting statement: Avro event type does not allow Contained beans");
                }
            }
        }

        internal class InfraBeanContained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                    if (!rep.IsAvroEvent()) {
                        TryAssertionBeanContained(env, rep);
                    }
                }

                var epl = EventRepresentationChoice.AVRO.GetAnnotationText() +
                          " @Name('create') create window MyWindowBC#keepall as (bean SupportBean_S0)";
                TryInvalidCompile(
                    env,
                    epl,
                    "Property 'bean' type 'class " +
                    typeof(SupportBean_S0).Name +
                    "' does not have a mapping to an Avro type ");
            }
        }

        internal class InfraIntersection : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "create window MyWindowINT#length(2)#unique(IntPrimitive) as SupportBean;\n" +
                    "insert into MyWindowINT select * from SupportBean;\n" +
                    "@Name('s0') select irstream * from MyWindowINT");

                var fields = new [] { "TheString" };
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    fields,
                    new[] {new object[] {"E1"}},
                    null);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    fields,
                    new[] {new object[] {"E2"}},
                    null);

                env.SendEventBean(new SupportBean("E3", 2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").AssertInvokedAndReset(),
                    fields,
                    new[] {new object[] {"E3"}},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        internal class InfraBeanSchemaBacked : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // Test create from schema
                var epl = "create schema ABC as " +
                          typeof(SupportBean).Name +
                          ";\n" +
                          "create window MyWindowBSB#keepall as ABC;\n" +
                          "insert into MyWindowBSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean());
                AssertEvent(env.CompileExecuteFAF("select * from MyWindowBSB", path).Array[0], "MyWindowBSB");

                env.CompileDeploy("@Name('s0') select * from ABC", path).AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraDeepSupertypeInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('create') create window MyWindowDSI#keepall as select * from SupportOverrideBase;\n" +
                          "insert into MyWindowDSI select * from SupportOverrideOneA;\n";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportOverrideOneA("1a", "1", "base"));
                Assert.AreEqual("1a", env.GetEnumerator("create").Advance().Get("val"));
                env.UndeployAll();
            }
        }

        internal class InfraOnInsertPremptiveTwoWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create schema TypeOne(col1 int);\n";
                epl += "create schema TypeTwo(col2 int);\n";
                epl += "create schema TypeTrigger(trigger int);\n";
                epl += "create window WinOne#keepall as TypeOne;\n";
                epl += "create window WinTwo#keepall as TypeTwo;\n";

                epl += "@Name('insert-window-one') insert into WinOne(col1) select IntPrimitive from SupportBean;\n";

                epl += "@Name('insert-otherstream') on TypeTrigger insert into OtherStream select col1 from WinOne;\n";
                epl += "@Name('insert-window-two') on TypeTrigger insert into WinTwo(col2) select col1 from WinOne;\n";
                epl += "@Name('s0') on OtherStream select col2 from WinTwo;\n";

                env.CompileDeployWBusPublicType(epl, new RegressionPath()).AddListener("s0");

                // populate WinOne
                env.SendEventBean(new SupportBean("E1", 9));

                // fire trigger
                if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                    env.EventService.GetEventSender("TypeTrigger").SendEvent(new object[0]);
                }
                else {
                    env.EventService.GetEventSender("TypeTrigger").SendEvent(new Dictionary<string, object>());
                }

                Assert.AreEqual(9, env.Listener("s0").AssertOneGetNewAndReset().Get("col2"));

                env.UndeployAll();
            }
        }

        internal class InfraWithDeleteUseAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as MySimpleKeyValueMap",
                    "on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key");
            }
        }

        internal class InfraWithDeleteFirstAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as select key, value from MySimpleKeyValueMap",
                    "on SupportMarketDataBean delete from MyWindow as S1 where Symbol = S1.Key");
            }
        }

        internal class InfraWithDeleteSecondAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as MySimpleKeyValueMap",
                    "on SupportMarketDataBean as S0 delete from MyWindow where S0.Symbol = key");
            }
        }

        internal class InfraWithDeleteNoAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as select key as key, value as value from MySimpleKeyValueMap",
                    "on SupportMarketDataBean delete from MyWindow where Symbol = key");
            }
        }

        internal class InfraTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = "@Name('create') create window MyWindowTW#time(10 sec) as MySimpleKeyValueMap";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindowTW select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create consumer
                var stmtTextSelectOne = "@Name('s0') select irstream key, value as value from MyWindowTW";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");
                var listenerStmtOne = new SupportUpdateListener();

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowTW as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                // Should push out the window
                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 11000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 2L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 3L}, new object[] {"E4", 4L}});

                // nothing pushed
                SendTimer(env, 15000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                // push last event
                SendTimer(env, 19999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendTimer(env, 20000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E4", 4L}});

                // delete E4
                SendMarketBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 100000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class InfraTimeWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow#time(10 sec) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "@Name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create consumer
                var stmtTextSelectOne = "@Name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("consume");

                env.Milestone(2);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(3);

                // send event
                env.AdvanceTime(0);
                SendBeanInt(env, "G1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(4);

                // send event
                env.AdvanceTime(5000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20});

                env.Milestone(5);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 20});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});

                env.Milestone(6);

                // move time window
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                env.AdvanceTime(10000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(7);

                env.AdvanceTime(25000);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(8);

                // send events
                env.AdvanceTime(25000);
                SendBeanInt(env, "G3", 30);
                env.AdvanceTime(26000);
                SendBeanInt(env, "G4", 40);
                env.AdvanceTime(27000);
                SendBeanInt(env, "G5", 50);
                env.Listener("create").Reset();

                env.Milestone(9);

                // delete g3
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}, new object[] {"G5", 50}});
                SendMarketBean(env, "G3");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G3", 30});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 40}, new object[] {"G5", 50}});

                env.Milestone(10);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 40}, new object[] {"G5", 50}});
                env.AdvanceTime(35999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                env.AdvanceTime(36000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G4", 40});

                env.Milestone(11);

                // delete g5
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G5", 50}});
                SendMarketBean(env, "G5");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G5", 50});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(12);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                // destroy all
                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("consume");
                env.UndeployModuleContaining("create");
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraTimeFirstWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                SendTimer(env, 1000);

                var epl = "@Name('create') create window MyWindowTFW#firsttime(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTFW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowTFW;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowTFW as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("delete");
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                // Should not push out the window
                SendTimer(env, 12000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                SendSupportBean(env, "E4", 4L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});

                // nothing pushed
                SendTimer(env, 100000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraExtTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl =
                    "@Name('create') create window MyWindowETW#ext_timed(value, 10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowETW select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@Name('s0') select irstream key, value as value from MyWindowETW;\n" +
                    "@Name('delete') on SupportMarketDataBean delete from MyWindowETW where Symbol = key";
                env.CompileDeploy(epl).AddListener("s0").AddListener("create").AddListener("delete");

                SendSupportBean(env, "E1", 1000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1000L}});

                SendSupportBean(env, "E2", 5000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 5000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 5000L});

                SendSupportBean(env, "E3", 10000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 10000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 10000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1000L}, new object[] {"E2", 5000L}, new object[] {"E3", 10000L}});

                // Should push out the window
                SendSupportBean(env, "E4", 11000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E4", 11000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E1", 1000L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E4", 11000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 1000L});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 5000L}, new object[] {"E3", 10000L}, new object[] {"E4", 11000L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 5000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 5000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 10000L}, new object[] {"E4", 11000L}});

                // nothing pushed other then E5 (E2 is deleted)
                SendSupportBean(env, "E5", 15000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E5", 15000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E5", 15000L});
                Assert.IsNull(env.Listener("create").LastOldData);
                Assert.IsNull(env.Listener("s0").LastOldData);

                env.UndeployAll();
            }
        }

        public class InfraExtTimeWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:ext_timed(value, 10 sec) as select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, LongBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create consumer
                var stmtTextSelectOne = "@Name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("consume");

                env.Milestone(2);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(3);

                // send event
                SendBeanLong(env, "G1", 0L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 0L});

                env.Milestone(4);

                // send event
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 0L}});
                SendBeanLong(env, "G2", 5000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 5000L});

                env.Milestone(5);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 0L}, new object[] {"G2", 5000L}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 5000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 0L}});

                env.Milestone(6);

                // move time window
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 0L}});
                SendBeanLong(env, "G3", 10000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G3", 10000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G1", 0L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 10000L}});

                env.Milestone(7);

                // move time window
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 10000L}});
                SendBeanLong(env, "G4", 15000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G4", 15000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 10000L}, new object[] {"G4", 15000L}});

                env.Milestone(8);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 10000L}, new object[] {"G4", 15000L}});
                SendMarketBean(env, "G3");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G3", 10000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 15000L}});

                env.Milestone(9);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 15000L}});
                SendBeanLong(env, "G5", 21000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G5", 21000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 15000L}, new object[] {"G5", 21000L}});

                env.Milestone(10);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 15000L}, new object[] {"G5", 21000L}});

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanLong(
                RegressionEnvironment env,
                string @string,
                long longBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.LongBoxed = longBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        public class InfraExtTimeWindowSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString" };
                var path = new RegressionPath();

                env.AdvanceTime(0);
                env.CompileDeploy("create window ABCWin.win:time(10 sec) as SupportBean", path);
                env.CompileDeploy("insert into ABCWin select * from SupportBean", path);
                env.CompileDeploy("@Name('s0') select irstream * from ABCWin", path);
                env.CompileDeploy("on SupportBean_A delete from ABCWin where TheString = Id", path);
                env.AddListener("s0");

                env.Milestone(0);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                env.AdvanceTime(1000);
                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});

                env.AdvanceTime(2000);
                SendSupportBean_A(env, "E1"); // delete E1
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(2);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);

                env.AdvanceTime(3000);
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.AdvanceTime(3000);
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});
                SendSupportBean_A(env, "E3"); // delete E3
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}});
                env.AdvanceTime(12999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(13000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);

                env.UndeployAll();
            }
        }

        internal class InfraTimeOrderWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl =
                    "@Name('create') create window MyWindowTOW#time_order(value, 10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTOW select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@Name('s0') select irstream key, value as value from MyWindowTOW;\n" +
                    "@Name('delete') on SupportMarketDataBean delete from MyWindowTOW where Symbol = key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendTimer(env, 5000);
                SendSupportBean(env, "E1", 3000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 3000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 3000L});

                SendTimer(env, 6000);
                SendSupportBean(env, "E2", 2000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2000L});

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 1000L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 1000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 1000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 1000L}, new object[] {"E2", 2000L}, new object[] {"E1", 3000L}});

                // Should push out the window
                SendTimer(env, 11000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3", 1000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3", 1000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 2000L}, new object[] {"E1", 3000L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2000L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 3000L}});

                SendTimer(env, 12999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 13000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 3000L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 3000L});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 100000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class InfraTimeOrderSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.ext:time_order(value, 10) as select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, LongBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                env.AdvanceTime(20000);
                SendBeanLong(env, "G1", 23000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1"});

                env.Milestone(0);

                // send event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1"}});
                env.AdvanceTime(20000);
                SendBeanLong(env, "G2", 19000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2"});

                env.Milestone(1);

                // send event G3, pass through
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2"}, new object[] {"G1"}});
                env.AdvanceTime(21000);
                SendBeanLong(env, "G3", 10000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G3"});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G3"});
                env.Listener("create").Reset();

                env.Milestone(2);

                // delete G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2"}, new object[] {"G1"}});
                env.AdvanceTime(21000);
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2"});

                env.Milestone(3);

                // send event G4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1"}});
                env.AdvanceTime(22000);
                SendBeanLong(env, "G4", 18000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G4"});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4"}, new object[] {"G1"}});
                env.AdvanceTime(23000);
                SendBeanLong(env, "G5", 22000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G5"});

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4"}, new object[] {"G5"}, new object[] {"G1"}});
                env.AdvanceTime(27999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                env.AdvanceTime(28000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G4"});

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G5"}, new object[] {"G1"}});
                env.AdvanceTime(31999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                env.AdvanceTime(32000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G5"});

                env.Milestone(7);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1"}});
                env.AdvanceTime(32000);
                SendBeanLong(env, "G6", 25000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G6"});

                env.Milestone(8);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1"}, new object[] {"G6"}});
                env.AdvanceTime(32000);
                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G1"});

                env.Milestone(9);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G6"}});
                env.AdvanceTime(34999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                env.AdvanceTime(35000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G6"});

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanLong(
                RegressionEnvironment env,
                string @string,
                long longBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.LongBoxed = longBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraLengthWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowLW#length(3) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowLW;\n" +
                          "@Name('delete') on SupportMarketDataBean delete from MyWindowLW where Symbol = key";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});

                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});

                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});

                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});

                SendSupportBean(env, "E5", 5L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E5", 5L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E1", 1L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E5", 5L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 1L});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 3L}, new object[] {"E4", 4L}, new object[] {"E5", 5L}});

                SendSupportBean(env, "E6", 6L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E6", 6L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E3", 3L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E6", 6L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E3", 3L});
                env.Listener("s0").Reset();

                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class InfraLengthWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:length(3) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                SendBeanInt(env, "G1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(0);

                // send event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20});

                env.Milestone(1);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 20});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});

                env.Milestone(2);

                // send event G3
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G3", 30);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 30});

                env.Milestone(3);

                // delete event G1
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G3", 30}});
                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G1", 10});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}});

                env.Milestone(4);

                // send event G4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}});
                SendBeanInt(env, "G4", 40);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G4", 40});

                env.Milestone(5);

                // send event G5
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}});
                SendBeanInt(env, "G5", 50);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G5", 50});

                env.Milestone(6);

                // send event G6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}, new object[] {"G5", 50}});
                SendBeanInt(env, "G6", 60);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G6", 60});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G3", 30});
                env.Listener("create").Reset();

                env.Milestone(7);

                // delete event G6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 40}, new object[] {"G5", 50}, new object[] {"G6", 60}});
                SendMarketBean(env, "G6");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G6", 60});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 40}, new object[] {"G5", 50}});

                env.Milestone(8);

                // send event G7
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 40}, new object[] {"G5", 50}});
                SendBeanInt(env, "G7", 70);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G7", 70});

                env.Milestone(9);

                // send event G8
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 40}, new object[] {"G5", 50}, new object[] {"G7", 70}});
                SendBeanInt(env, "G8", 80);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G8", 80});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G4", 40});
                env.Listener("create").Reset();

                env.Milestone(10);

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraLengthFirstWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowLFW#firstlength(2) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLFW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowLFW;\n" +
                          "@Name('delete') on SupportMarketDataBean delete from MyWindowLFW where Symbol = key";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});

                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});

                SendSupportBean(env, "E3", 3L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E4", 4L}});

                SendSupportBean(env, "E5", 5L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E4", 4L}});

                env.UndeployAll();
            }
        }

        internal class InfraTimeAccum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                // create window
                var epl = "@Name('create') create window MyWindowTA#time_accum(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTA select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowTA;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowTA as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3L});

                SendTimer(env, 15000);
                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 4L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {
                        new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L},
                        new object[] {"E4", 4L}
                    });

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});

                // nothing pushed
                SendTimer(env, 24999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 25000);
                Assert.IsNull(env.Listener("create").LastNewData);
                var oldData = env.Listener("create").LastOldData;
                Assert.AreEqual(3, oldData.Length);
                EPAssertionUtil.AssertProps(
                    oldData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    oldData[1],
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    oldData[2],
                    fields,
                    new object[] {"E4", 4L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                // delete E4
                SendMarketBean(env, "E4");
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 30000);
                SendSupportBean(env, "E5", 5L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5", 5L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E5", 5L});

                SendTimer(env, 31000);
                SendSupportBean(env, "E6", 6L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6", 6L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6", 6L});

                SendTimer(env, 38000);
                SendSupportBean(env, "E7", 7L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E7", 7L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E7", 7L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E5", 5L}, new object[] {"E6", 6L}, new object[] {"E7", 7L}});

                // delete E7 - deleting the last should spit out the first 2 timely
                SendMarketBean(env, "E7");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E7", 7L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E7", 7L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E5", 5L}, new object[] {"E6", 6L}});

                SendTimer(env, 40999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 41000);
                Assert.IsNull(env.Listener("s0").LastNewData);
                oldData = env.Listener("s0").LastOldData;
                Assert.AreEqual(2, oldData.Length);
                EPAssertionUtil.AssertProps(
                    oldData[0],
                    fields,
                    new object[] {"E5", 5L});
                EPAssertionUtil.AssertProps(
                    oldData[1],
                    fields,
                    new object[] {"E6", 6L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 50000);
                SendSupportBean(env, "E8", 8L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E8", 8L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E8", 8L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E8", 8L}});

                SendTimer(env, 55000);
                SendMarketBean(env, "E8");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E8", 8L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E8", 8L});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 100000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class InfraTimeAccumSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:time_accum(10 sec) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "@Name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create consumer
                var stmtTextSelectOne = "@Name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("consume");

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(0);

                // send event
                env.AdvanceTime(1000);
                SendBeanInt(env, "G1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1});

                env.Milestone(1);

                // send event
                env.AdvanceTime(5000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                SendBeanInt(env, "G2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2});

                env.Milestone(2);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}, new object[] {"G2", 2}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 2});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});

                env.Milestone(3);

                // move time window
                env.AdvanceTime(10999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                env.AdvanceTime(11000);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G1", 1});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(4);

                // Send G3
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                env.AdvanceTime(20000);
                SendBeanInt(env, "G3", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 3});

                env.Milestone(5);

                // Send G4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}});
                env.AdvanceTime(29999);
                SendBeanInt(env, "G4", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G4", 4});

                env.Milestone(6);

                // Delete G3
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}});
                SendMarketBean(env, "G3");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G3", 3});

                env.Milestone(7);

                // Delete G4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G4", 4}});
                SendMarketBean(env, "G4");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G4", 4});

                env.Milestone(8);

                // Send timer, no events
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                env.AdvanceTime(40000);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(9);

                // Send G5
                env.AdvanceTime(41000);
                SendBeanInt(env, "G5", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G5", 5});

                env.Milestone(10);

                // Send G6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G5", 5}});
                env.AdvanceTime(42000);
                SendBeanInt(env, "G6", 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G6", 6});

                env.Milestone(11);

                // Send G7
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G5", 5}, new object[] {"G6", 6}});
                env.AdvanceTime(43000);
                SendBeanInt(env, "G7", 7);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G7", 7});

                env.Milestone(12);

                // Send G8
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G5", 5}, new object[] {"G6", 6}, new object[] {"G7", 7}});
                env.AdvanceTime(44000);
                SendBeanInt(env, "G8", 8);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G8", 8});

                env.Milestone(13);

                // Delete G6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {
                        new object[] {"G5", 5}, new object[] {"G6", 6}, new object[] {"G7", 7}, new object[] {"G8", 8}
                    });
                SendMarketBean(env, "G6");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G6", 6});

                env.Milestone(14);

                // Delete G8
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G5", 5}, new object[] {"G7", 7}, new object[] {"G8", 8}});
                SendMarketBean(env, "G8");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G8", 8});

                env.Milestone(15);

                env.AdvanceTime(52999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                env.AdvanceTime(53000);
                Assert.AreEqual(2, env.Listener("create").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G5", 5});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[1],
                    fields,
                    new object[] {"G7", 7});
                Assert.IsNull(env.Listener("create").LastNewData);
                env.Listener("create").Reset();

                // destroy all
                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("consume");
                env.UndeployModuleContaining("create");
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraTimeBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowTB#time_batch(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTB select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select key, value as value from MyWindowTB;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowTB as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});

                // nothing pushed
                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 11000);
                Assert.IsNull(env.Listener("create").LastOldData);
                var newData = env.Listener("create").LastNewData;
                Assert.AreEqual(2, newData.Length);
                EPAssertionUtil.AssertProps(
                    newData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    newData[1],
                    fields,
                    new object[] {"E3", 3L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 21000);
                Assert.IsNull(env.Listener("create").LastNewData);
                var oldData = env.Listener("create").LastOldData;
                Assert.AreEqual(2, oldData.Length);
                EPAssertionUtil.AssertProps(
                    oldData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    oldData[1],
                    fields,
                    new object[] {"E3", 3L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();

                // send and delete E4, leaving an empty batch
                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E4", 4L}});

                SendMarketBean(env, "E4");
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 31000);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class InfraTimeBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:time_batch(10) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event
                env.AdvanceTime(1000);
                SendBeanInt(env, "G1", 1);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(0);

                // send event
                env.AdvanceTime(5000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                SendBeanInt(env, "G2", 2);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(1);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}, new object[] {"G2", 2}});
                SendMarketBean(env, "G2");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(2);

                // delete event G1
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                SendMarketBean(env, "G1");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                env.AdvanceTime(11000);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(4);

                // Send g3, g4 and g5
                env.AdvanceTime(15000);
                SendBeanInt(env, "G3", 3);
                SendBeanInt(env, "G4", 4);
                SendBeanInt(env, "G5", 5);

                env.Milestone(5);

                // Delete g5
                SendMarketBean(env, "G5");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(6);

                // send g6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}});
                env.AdvanceTime(18000);
                SendBeanInt(env, "G6", 6);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(7);

                // flush batch
                env.AdvanceTime(21000);
                Assert.AreEqual(3, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G3", 3});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[1],
                    fields,
                    new object[] {"G4", 4});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[2],
                    fields,
                    new object[] {"G6", 6});
                Assert.IsNull(env.Listener("create").LastOldData);
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(8);

                // send g7, g8 and g9
                env.AdvanceTime(22000);
                SendBeanInt(env, "G7", 7);
                SendBeanInt(env, "G8", 8);
                SendBeanInt(env, "G9", 9);

                env.Milestone(9);

                // delete g7 and g9
                SendMarketBean(env, "G7");
                SendMarketBean(env, "G9");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(10);

                // flush
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G8", 8}});
                env.AdvanceTime(31000);
                Assert.AreEqual(1, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G8", 8});
                Assert.AreEqual(3, env.Listener("create").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G3", 3});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[1],
                    fields,
                    new object[] {"G4", 4});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[2],
                    fields,
                    new object[] {"G6", 6});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraTimeBatchLateConsumer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimer(env, 0);

                var epl = "@Name('create') create window MyWindowTBLC#time_batch(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTBLC select TheString as key, LongBoxed as value from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendTimer(env, 0);
                SendSupportBean(env, "E1", 1L);

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);

                // create consumer
                var stmtTextSelectOne = "@Name('s0') select sum(value) as value from MyWindowTBLC";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");

                SendTimer(env, 8000);
                SendSupportBean(env, "E3", 3L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 10000);
                var newData = env.Listener("s0").LastNewData;
                Assert.AreEqual(1, newData.Length);
                EPAssertionUtil.AssertProps(
                    newData[0],
                    new[] {"value"},
                    new object[] {6L});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), new[] {"value"}, null);

                env.UndeployAll();
            }
        }

        internal class InfraLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowLB#length_batch(3) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLB select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select key, value as value from MyWindowLB;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowLB as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                SendSupportBean(env, "E2", 2L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendSupportBean(env, "E3", 3L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});

                SendSupportBean(env, "E4", 4L);
                Assert.IsNull(env.Listener("create").LastOldData);
                var newData = env.Listener("create").LastNewData;
                Assert.AreEqual(3, newData.Length);
                EPAssertionUtil.AssertProps(
                    newData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    newData[1],
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    newData[2],
                    fields,
                    new object[] {"E4", 4L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E5", 5L);
                SendSupportBean(env, "E6", 6L);
                SendMarketBean(env, "E5");
                SendMarketBean(env, "E6");
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E7", 7L);
                SendSupportBean(env, "E8", 8L);
                SendSupportBean(env, "E9", 9L);
                var oldData = env.Listener("create").LastOldData;
                newData = env.Listener("create").LastNewData;
                Assert.AreEqual(3, newData.Length);
                Assert.AreEqual(3, oldData.Length);
                EPAssertionUtil.AssertProps(
                    newData[0],
                    fields,
                    new object[] {"E7", 7L});
                EPAssertionUtil.AssertProps(
                    newData[1],
                    fields,
                    new object[] {"E8", 8L});
                EPAssertionUtil.AssertProps(
                    newData[2],
                    fields,
                    new object[] {"E9", 9L});
                EPAssertionUtil.AssertProps(
                    oldData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    oldData[1],
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    oldData[2],
                    fields,
                    new object[] {"E4", 4L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();

                SendSupportBean(env, "E10", 10L);
                SendSupportBean(env, "E10", 11L);
                SendMarketBean(env, "E10");

                SendSupportBean(env, "E21", 21L);
                SendSupportBean(env, "E22", 22L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SendSupportBean(env, "E23", 23L);
                oldData = env.Listener("create").LastOldData;
                newData = env.Listener("create").LastNewData;
                Assert.AreEqual(3, newData.Length);
                Assert.AreEqual(3, oldData.Length);

                env.UndeployAll();
            }
        }

        public class InfraLengthBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:length_batch(3) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(2);

                // send event
                SendBeanInt(env, "G1", 10);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(3);

                // send event
                SendBeanInt(env, "G2", 20);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(4);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});

                env.Milestone(5);

                // delete event G1
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(6);

                // send event G3
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                SendBeanInt(env, "G3", 30);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(7);

                // send event g4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}});
                SendBeanInt(env, "G4", 40);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}});

                env.Milestone(8);

                // delete event G4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G4", 40}});
                SendMarketBean(env, "G4");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}});

                env.Milestone(9);

                // send G5
                SendBeanInt(env, "G5", 50);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(10);

                // send G6, batch fires
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 30}, new object[] {"G5", 50}});
                SendBeanInt(env, "G6", 60);
                Assert.AreEqual(3, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G3", 30});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[1],
                    fields,
                    new object[] {"G5", 50});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[2],
                    fields,
                    new object[] {"G6", 60});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                env.Listener("create").Reset();

                env.Milestone(11);

                // send G8
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                SendBeanInt(env, "G7", 70);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G7", 70}});

                env.Milestone(12);

                // Send G8
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G7", 70}});
                SendBeanInt(env, "G8", 80);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G7", 70}, new object[] {"G8", 80}});

                env.Milestone(13);

                // Delete G7
                SendMarketBean(env, "G7");
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G8", 80}});

                env.Milestone(14);

                // Send G9
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G8", 80}});
                SendBeanInt(env, "G9", 90);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(15);

                // Send G10
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G8", 80}, new object[] {"G9", 90}});
                SendBeanInt(env, "G10", 100);
                Assert.AreEqual(3, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G8", 80});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[1],
                    fields,
                    new object[] {"G9", 90});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[2],
                    fields,
                    new object[] {"G10", 100});
                Assert.AreEqual(3, env.Listener("create").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G3", 30});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[1],
                    fields,
                    new object[] {"G5", 50});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[2],
                    fields,
                    new object[] {"G6", 60});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(16);

                // send g11
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                SendBeanInt(env, "G11", 110);
                SendBeanInt(env, "G12", 120);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(17);

                // delete g12
                SendMarketBean(env, "G12");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(18);

                // send g13
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G11", 110}});
                SendBeanInt(env, "G13", 130);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                // Send G14
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G11", 110}, new object[] {"G13", 130}});
                SendBeanInt(env, "G14", 140);
                Assert.AreEqual(3, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G11", 110});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[1],
                    fields,
                    new object[] {"G13", 130});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[2],
                    fields,
                    new object[] {"G14", 140});
                Assert.AreEqual(3, env.Listener("create").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G8", 80});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[1],
                    fields,
                    new object[] {"G9", 90});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[2],
                    fields,
                    new object[] {"G10", 100});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraSortWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowSW#sort(3, value asc) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowSW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select key, value as value from MyWindowSW;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowSW as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("create").AddListener("s0");

                SendSupportBean(env, "E1", 10L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 10L});

                SendSupportBean(env, "E2", 20L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20L});

                SendSupportBean(env, "E3", 15L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 15L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E2", 20L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 20L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}});

                SendSupportBean(env, "E4", 18L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 18L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E4", 18L}});

                SendSupportBean(env, "E5", 17L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E5", 17L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E4", 18L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E5", 17L}});

                // delete E1
                SendMarketBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1", 10L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 15L}, new object[] {"E5", 17L}});

                SendSupportBean(env, "E6", 16L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6", 16L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 15L}, new object[] {"E6", 16L}, new object[] {"E5", 17L}});

                SendSupportBean(env, "E7", 16L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E7", 16L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E5", 17L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 15L}, new object[] {"E7", 16L}, new object[] {"E6", 16L}});

                // delete E7 has no effect
                SendMarketBean(env, "E7");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E7", 16L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 15L}, new object[] {"E6", 16L}});

                SendSupportBean(env, "E8", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E8", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E8", 1L}, new object[] {"E3", 15L}, new object[] {"E6", 16L}});

                SendSupportBean(env, "E9", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E9", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E6", 16L});
                env.Listener("create").Reset();

                env.UndeployAll();
            }
        }

        public class InfraSortWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.ext:sort(3, value) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                SendBeanInt(env, "G1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(0);

                // send event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G2", 9);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 9});
                env.Listener("create").Reset();

                env.Milestone(1);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2", 9}, new object[] {"G1", 10}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 9});

                env.Milestone(2);

                // send g3
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G3", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 3});

                env.Milestone(3);

                // send g4
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G1", 10}});
                SendBeanInt(env, "G4", 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G4", 4});

                env.Milestone(4);

                // send g5
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}, new object[] {"G1", 10}});
                SendBeanInt(env, "G5", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G5", 5});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(5);

                // send g6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}, new object[] {"G5", 5}});
                SendBeanInt(env, "G6", 6);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G6", 6});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G6", 6});

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraTimeLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl =
                    "@Name('create') create window MyWindowTLB#time_length_batch(10 sec, 3) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTLB select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@Name('s0') select key, value as value from MyWindowTLB;\n" +
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowTLB as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("delete").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);
                SendSupportBean(env, "E2", 2L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});

                // delete E2
                SendMarketBean(env, "E2");
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendSupportBean(env, "E3", 3L);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});

                SendSupportBean(env, "E4", 4L);
                Assert.IsNull(env.Listener("create").LastOldData);
                var newData = env.Listener("create").LastNewData;
                Assert.AreEqual(3, newData.Length);
                EPAssertionUtil.AssertProps(
                    newData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    newData[1],
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    newData[2],
                    fields,
                    new object[] {"E4", 4L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendTimer(env, 5000);
                SendSupportBean(env, "E5", 5L);
                SendSupportBean(env, "E6", 6L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E5", 5L}, new object[] {"E6", 6L}});

                SendMarketBean(env, "E5"); // deleting E5
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E6", 6L}});

                SendTimer(env, 10999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimer(env, 11000);
                newData = env.Listener("create").LastNewData;
                Assert.AreEqual(1, newData.Length);
                EPAssertionUtil.AssertProps(
                    newData[0],
                    fields,
                    new object[] {"E6", 6L});
                var oldData = env.Listener("create").LastOldData;
                Assert.AreEqual(3, oldData.Length);
                EPAssertionUtil.AssertProps(
                    oldData[0],
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    oldData[1],
                    fields,
                    new object[] {"E3", 3L});
                EPAssertionUtil.AssertProps(
                    oldData[2],
                    fields,
                    new object[] {"E4", 4L});
                env.Listener("create").Reset();
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        public class InfraTimeLengthBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.win:time_length_batch(10 sec, 4) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event
                env.AdvanceTime(1000);
                SendBeanInt(env, "G1", 1);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(0);

                // send event
                env.AdvanceTime(5000);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                SendBeanInt(env, "G2", 2);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(1);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}, new object[] {"G2", 2}});
                SendMarketBean(env, "G2");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(2);

                // delete event G1
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                SendMarketBean(env, "G1");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);
                env.AdvanceTime(11000);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(4);

                // Send g3, g4 and g5
                env.AdvanceTime(15000);
                SendBeanInt(env, "G3", 3);
                SendBeanInt(env, "G4", 4);

                env.Milestone(5);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}});
                env.AdvanceTime(16000);
                SendBeanInt(env, "G5", 5);

                env.Milestone(6);

                // Delete g5
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}, new object[] {"G5", 5}});
                SendMarketBean(env, "G5");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(7);

                // send g6
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G3", 3}, new object[] {"G4", 4}});
                env.AdvanceTime(18000);
                SendBeanInt(env, "G6", 6);
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(8);

                // flush batch
                env.AdvanceTime(24999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                env.AdvanceTime(25000);
                Assert.AreEqual(3, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G3", 3});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[1],
                    fields,
                    new object[] {"G4", 4});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[2],
                    fields,
                    new object[] {"G6", 6});
                Assert.IsNull(env.Listener("create").LastOldData);
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(9);

                // send g7, g8 and g9
                env.AdvanceTime(28000);
                SendBeanInt(env, "G7", 7);
                SendBeanInt(env, "G8", 8);
                SendBeanInt(env, "G9", 9);

                env.Milestone(10);

                // delete g7 and g9
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G7", 7}, new object[] {"G8", 8}, new object[] {"G9", 9}});
                SendMarketBean(env, "G7");
                SendMarketBean(env, "G9");
                Assert.IsFalse(env.Listener("create").IsInvoked);

                env.Milestone(11);

                // flush
                env.AdvanceTime(34999);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G8", 8}});
                env.AdvanceTime(35000);
                Assert.AreEqual(1, env.Listener("create").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G8", 8});
                Assert.AreEqual(3, env.Listener("create").LastOldData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G3", 3});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[1],
                    fields,
                    new object[] {"G4", 4});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[2],
                    fields,
                    new object[] {"G6", 6});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        public class InfraLengthWindowSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "TheString" };
                var path = new RegressionPath();
                env.CompileDeploy("create window ABCWin#length(2) as SupportBean", path);
                env.CompileDeploy("insert into ABCWin select * from SupportBean", path);
                env.CompileDeploy("on SupportBean_A delete from ABCWin where TheString = Id", path);
                env.CompileDeploy("@Name('s0') select irstream * from ABCWin", path).AddListener("s0");

                env.Milestone(0);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                SendSupportBean_A(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(3);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}, new object[] {"E3"}});

                SendSupportBean_A(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(4);

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E2"}});

                SendSupportBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});
                SendSupportBean(env, "E5");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertGetAndResetIRPair(),
                    fields,
                    new object[] {"E5"},
                    new object[] {"E2"});

                env.UndeployAll();
            }
        }

        internal class InfraLengthWindowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl =
                    "@Name('create') create window MyWindowWPG#groupwin(value)#length(2) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowWPG select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@Name('s0') select irstream key, value as value from MyWindowWPG;\n" +
                    "@Name('delete') on SupportMarketDataBean delete from MyWindowWPG where Symbol = key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});

                SendSupportBean(env, "E2", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 1L});

                SendSupportBean(env, "E3", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 2L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E2", 1L}, new object[] {"E3", 2L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E2", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}, new object[] {"E3", 2L}});

                SendSupportBean(env, "E4", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", 1L});

                SendSupportBean(env, "E5", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E5", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E1", 1L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E5", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 1L});
                env.Listener("s0").Reset();

                SendSupportBean(env, "E6", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E6", 2L});

                // delete E6
                SendMarketBean(env, "E6");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E6", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"E6", 2L});

                SendSupportBean(env, "E7", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E7", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E7", 2L});

                SendSupportBean(env, "E8", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E8", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"E3", 2L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E8", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E3", 2L});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class InfraTimeBatchPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                SendTimer(env, 0);
                var epl =
                    "@Name('create') create window MyWindowTBPG#groupwin(value)#time_batch(10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTBPG select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@Name('s0') select key, value as value from MyWindowTBPG;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 10L);
                SendSupportBean(env, "E2", 20L);
                SendSupportBean(env, "E3", 20L);
                SendSupportBean(env, "E4", 10L);

                SendTimer(env, 11000);
                Assert.AreEqual(env.Listener("create").LastNewData.Length, 4);
                Assert.AreEqual(env.Listener("s0").LastNewData.Length, 4);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"E1", 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[1],
                    fields,
                    new object[] {"E4", 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[2],
                    fields,
                    new object[] {"E2", 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[3],
                    fields,
                    new object[] {"E3", 20L});
                env.Listener("create").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E1", 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[1],
                    fields,
                    new object[] {"E4", 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[2],
                    fields,
                    new object[] {"E2", 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[3],
                    fields,
                    new object[] {"E3", 20L});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class InfraDoubleInsertSameWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowDISM#keepall as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowDISM select TheString as key, LongBoxed+1 as value from SupportBean;\n" +
                          "insert into MyWindowDISM select TheString as key, LongBoxed+2 as value from SupportBean;\n" +
                          "@Name('s0') select key, value as value from MyWindowDISM";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0");

                SendSupportBean(env, "E1", 10L);
                Assert.AreEqual(
                    2,
                    env.Listener("create").NewDataList.Count); // listener to window gets 2 indivIdual events
                Assert.AreEqual(
                    2,
                    env.Listener("s0").NewDataList.Count); // listener to statement gets 1 indivIdual event
                Assert.AreEqual(2, env.Listener("create").NewDataListFlattened.Length);
                Assert.AreEqual(2, env.Listener("s0").NewDataListFlattened.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").NewDataListFlattened,
                    fields,
                    new[] {new object[] {"E1", 11L}, new object[] {"E1", 12L}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class InfraLastEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowLE#lastevent as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLE select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowLE;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowLE as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E1", 1L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E2", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 1L});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 2L}});

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E2", 2L});
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E3", 3L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 3L}});

                // delete E3
                SendMarketBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E3", 3L});
                Assert.IsNull(env.Listener("s0").LastNewData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E4", 4L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E4", 4L}});

                // delete other event
                SendMarketBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        public class InfraLastEventSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow.std:lastevent() as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event
                SendBeanInt(env, "G1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1});

                env.Milestone(0);

                // send event
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1}});
                SendBeanInt(env, "G2", 2);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G2", 2});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G1", 1});
                env.Listener("create").Reset();

                env.Milestone(1);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2", 2}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 2});

                env.Milestone(2);

                Assert.AreEqual(0, EPAssertionUtil.EnumeratorCount(env.GetEnumerator("create")));
                SendBeanInt(env, "G3", 3);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 3});

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraFirstEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowFE#firstevent as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowFE select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowFE;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowFE as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E1", 1L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                SendSupportBean(env, "E2", 2L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1L}});

                // delete E2
                SendMarketBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E1", 1L});
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E3", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E3", 3L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E3", 3L}});

                // delete E3
                SendMarketBean(env, "E2"); // no effect
                SendMarketBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"E3", 3L});
                Assert.IsNull(env.Listener("s0").LastNewData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                SendSupportBean(env, "E4", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"E4", 4L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E4", 4L}});

                // delete other event
                SendMarketBean(env, "E1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowUN#unique(Key) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowUN select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowUN;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowUN as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "G1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"G1", 1L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}});

                SendSupportBean(env, "G2", 20L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"G2", 20L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}, new object[] {"G2", 20L}});

                // delete G2
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"G2", 20L});
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}});

                SendSupportBean(env, "G1", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"G1", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"G1", 1L});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 2L}});

                SendSupportBean(env, "G2", 21L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"G2", 21L});
                Assert.IsNull(env.Listener("s0").LastOldData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 2L}, new object[] {"G2", 21L}});

                SendSupportBean(env, "G2", 22L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"G2", 22L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"G2", 21L});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 2L}, new object[] {"G2", 22L}});

                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"G1", 2L});
                Assert.IsNull(env.Listener("s0").LastNewData);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2", 22L}});

                env.UndeployAll();
            }
        }

        public class InfraUniqueSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindow#unique(Key) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "@Name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create consumer
                var stmtTextSelectOne = "@Name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path);

                env.Milestone(2);

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindow as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path);

                env.Milestone(3);

                // send event
                SendBeanInt(env, "G1", 10);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G1", 10});
                Assert.IsNull(env.Listener("create").LastOldData);
                env.Listener("create").Reset();

                env.Milestone(4);

                // send event
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}});
                SendBeanInt(env, "G2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G2", 20});
                Assert.IsNull(env.Listener("create").LastOldData);
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});

                env.Milestone(5);

                // delete event G2
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 10}, new object[] {"G2", 20}});
                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G1", 10});
                Assert.IsNull(env.Listener("create").LastNewData);
                env.Listener("create").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2", 20}});

                env.Milestone(6);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2", 20}});
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G2", 20});
                Assert.IsNull(env.Listener("create").LastNewData);
                env.Listener("create").Reset();

                // destroy all
                env.UndeployAll();
            }

            private SupportBean SendBeanInt(
                RegressionEnvironment env,
                string @string,
                int intBoxed)
            {
                var bean = new SupportBean();
                bean.TheString = @string;
                bean.IntBoxed = intBoxed;
                env.SendEventBean(bean);
                return bean;
            }

            private void SendMarketBean(
                RegressionEnvironment env,
                string symbol)
            {
                var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
                env.SendEventBean(bean);
            }
        }

        internal class InfraFirstUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl = "@Name('create') create window MyWindowFU#firstunique(Key) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowFU select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select irstream key, value as value from MyWindowFU;\n" +
                          "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowFU as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("delete");

                SendSupportBean(env, "G1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}});

                SendSupportBean(env, "G2", 20L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20L});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}, new object[] {"G2", 20L}});

                // delete G2
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 20L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}});

                SendSupportBean(env, "G1", 2L); // ignored
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}});

                SendSupportBean(env, "G2", 21L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 21L});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}, new object[] {"G2", 21L}});

                SendSupportBean(env, "G2", 22L); // ignored
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 1L}, new object[] {"G2", 21L}});

                SendMarketBean(env, "G1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G1", 1L});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G2", 21L}});

                env.UndeployAll();
            }
        }

        internal class InfraFilteringConsumer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl =
                    "@Name('create') create window MyWindowFC#unique(Key) as select TheString as key, IntPrimitive as value from SupportBean;\n" +
                    "insert into MyWindowFC select TheString as key, IntPrimitive as value from SupportBean;\n" +
                    "@Name('s0') select irstream key, value as value from MyWindowFC(value > 0, value < 10);\n" +
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowFC as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("delete");

                SendSupportBeanInt(env, "G1", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 5});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 5});

                SendSupportBeanInt(env, "G1", 15);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"G1", 5});
                Assert.IsNull(env.Listener("s0").LastNewData);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastOldData[0],
                    fields,
                    new object[] {"G1", 5});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").LastNewData[0],
                    fields,
                    new object[] {"G1", 15});
                env.Listener("create").Reset();

                // send G2
                SendSupportBeanInt(env, "G2", 8);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 8});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 8});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 15}, new object[] {"G2", 8}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"G2", 8}});

                // delete G2
                SendMarketBean(env, "G2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 8});
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G2", 8});

                // send G3
                SendSupportBeanInt(env, "G3", -1);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", -1});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"G1", 15}, new object[] {"G3", -1}});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, null);

                // delete G2
                SendMarketBean(env, "G3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetOldAndReset(),
                    fields,
                    new object[] {"G3", -1});

                SendSupportBeanInt(env, "G1", 6);
                SendSupportBeanInt(env, "G2", 7);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"G1", 6}, new object[] {"G2", 7}});

                env.UndeployAll();
            }
        }

        internal class InfraSelectGroupedViewLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var epl =
                    "@Name('create') create window MyWindowSGVS#groupwin(TheString, IntPrimitive)#length(9) as select TheString, IntPrimitive from SupportBean;\n" +
                    "@Name('insert') insert into MyWindowSGVS select TheString, IntPrimitive from SupportBean;\n";
                env.CompileDeploy(epl, path);

                // fill window
                var stringValues = new[] {"c0", "c1", "c2"};
                for (var i = 0; i < stringValues.Length; i++) {
                    for (var j = 0; j < 3; j++) {
                        env.SendEventBean(new SupportBean(stringValues[i], j));
                    }
                }

                env.SendEventBean(new SupportBean("c0", 1));
                env.SendEventBean(new SupportBean("c1", 2));
                env.SendEventBean(new SupportBean("c3", 3));
                var received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("create"));
                Assert.AreEqual(12, received.Length);

                // create select stmt
                var stmtTextSelect =
                    "@Name('s0') select TheString, IntPrimitive, count(*) from MyWindowSGVS group by TheString, IntPrimitive order by TheString, IntPrimitive";
                env.CompileDeploy(stmtTextSelect, path);
                received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                Assert.AreEqual(10, received.Length);

                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    new [] { "TheString","IntPrimitive","count(*)" },
                    new[] {
                        new object[] {"c0", 0, 1L},
                        new object[] {"c0", 1, 2L},
                        new object[] {"c0", 2, 1L},
                        new object[] {"c1", 0, 1L},
                        new object[] {"c1", 1, 1L},
                        new object[] {"c1", 2, 2L},
                        new object[] {"c2", 0, 1L},
                        new object[] {"c2", 1, 1L},
                        new object[] {"c2", 2, 1L},
                        new object[] {"c3", 3, 1L}
                    });

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraSelectGroupedViewLateStartVariableIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@Name('create') create window MyWindowSGVLS#groupwin(TheString, IntPrimitive)#length(9) as select TheString, IntPrimitive, LongPrimitive, BoolPrimitive from SupportBean";
                env.CompileDeploy(stmtTextCreate, path);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindowSGVLS select TheString, IntPrimitive, LongPrimitive, BoolPrimitive from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create variable
                env.CompileDeploy("create variable string var_1_1_1", path);
                env.CompileDeploy("on SupportVariableSetEvent(variableName='var_1_1_1') set var_1_1_1 = value", path);

                // fill window
                var stringValues = new[] {"c0", "c1", "c2"};
                for (var i = 0; i < stringValues.Length; i++) {
                    for (var j = 0; j < 3; j++) {
                        var innerBean = new SupportBean(stringValues[i], j);
                        innerBean.LongPrimitive = j;
                        innerBean.BoolPrimitive = true;
                        env.SendEventBean(innerBean);
                    }
                }

                // extra record to create non-uniform data
                var bean = new SupportBean("c1", 1);
                bean.LongPrimitive = 10;
                bean.BoolPrimitive = true;
                env.SendEventBean(bean);
                var received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("create"));
                Assert.AreEqual(10, received.Length);

                // create select stmt
                var stmtTextSelect =
                    "@Name('s0') select TheString, IntPrimitive, avg(LongPrimitive) as avgLong, count(BoolPrimitive) as cntBool" +
                    " from MyWindowSGVLS group by TheString, IntPrimitive having TheString = var_1_1_1 order by TheString, IntPrimitive";
                env.CompileDeploy(stmtTextSelect, path);

                // set variable to C0
                env.SendEventBean(new SupportVariableSetEvent("var_1_1_1", "c0"));

                // get iterator results
                received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                Assert.AreEqual(3, received.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    new [] { "TheString","IntPrimitive","avgLong","cntBool" },
                    new[] {
                        new object[] {"c0", 0, 0.0, 1L},
                        new object[] {"c0", 1, 1.0, 1L},
                        new object[] {"c0", 2, 2.0, 1L}
                    });

                // set variable to C1
                env.SendEventBean(new SupportVariableSetEvent("var_1_1_1", "c1"));

                received = EPAssertionUtil.EnumeratorToArray(env.GetEnumerator("s0"));
                Assert.AreEqual(3, received.Length);
                EPAssertionUtil.AssertPropsPerRow(
                    received,
                    new [] { "TheString","IntPrimitive","avgLong","cntBool" },
                    new[] {
                        new object[] {"c1", 0, 0.0, 1L},
                        new object[] {"c1", 1, 5.5, 2L},
                        new object[] {"c1", 2, 2.0, 1L}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraFilteringConsumerLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"sumvalue"};
                var path = new RegressionPath();

                var epl =
                    "@Name('create') create window MyWindowFCLS#keepall as select TheString as key, IntPrimitive as value from SupportBean;\n" +
                    "insert into MyWindowFCLS select TheString as key, IntPrimitive as value from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendSupportBeanInt(env, "G1", 5);
                SendSupportBeanInt(env, "G2", 15);
                SendSupportBeanInt(env, "G3", 2);

                // create consumer
                var stmtTextSelectOne =
                    "@Name('s0') select irstream sum(value) as sumvalue from MyWindowFCLS(value > 0, value < 10)";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {7}});

                SendSupportBeanInt(env, "G4", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {8});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {7});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {8}});

                SendSupportBeanInt(env, "G5", 20);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {8}});

                SendSupportBeanInt(env, "G6", 9);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {17});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {8});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {17}});

                // create delete stmt
                var stmtTextDelete =
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowFCLS as S1 where S0.Symbol = S1.Key";
                env.CompileDeploy(stmtTextDelete, path);

                SendMarketBean(env, "G4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {16});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {17});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {16}});

                SendMarketBean(env, "G5");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {16}});

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "create window MyWindowI1#groupwin(value)#uni(value) as MySimpleKeyValueMap",
                    "Named windows require one or more child views that are data window views [create window MyWindowI1#groupwin(value)#uni(value) as MySimpleKeyValueMap]");

                TryInvalidCompile(
                    env,
                    "create window MyWindowI2 as MySimpleKeyValueMap",
                    "Named windows require one or more child views that are data window views [create window MyWindowI2 as MySimpleKeyValueMap]");

                TryInvalidCompile(
                    env,
                    "on MySimpleKeyValueMap delete from dummy",
                    "A named window or table 'dummy' has not been declared [on MySimpleKeyValueMap delete from dummy]");

                var path = new RegressionPath();
                env.CompileDeploy("create window SomeWindow#keepall as (a int)", path);
                TryInvalidCompile(
                    env,
                    path,
                    "update SomeWindow set a = 'a' where a = 'b'",
                    "ProvIded EPL expression is an on-demand query expression (not a continuous query)");
                TryInvalidFAFCompile(
                    env,
                    path,
                    "update istream SomeWindow set a = 'a' where a = 'b'",
                    "ProvIded EPL expression is a continuous query expression (not an on-demand query)");

                // test model-after with no field
                TryInvalidCompile(
                    env,
                    "create window MyWindowI3#keepall as select innermap.abc from OuterMap",
                    "Failed to validate select-clause expression 'innermap.abc': Failed to resolve property 'innermap.abc' to a stream or nested property in a stream");

                env.UndeployAll();
            }
        }

        internal class InfraNamedWindowInvalidAlreadyExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile(
                    "create window MyWindowAE#keepall as MySimpleKeyValueMap",
                    compilerOptions => {
                        compilerOptions.AccessModifierNamedWindow = ctx => NameAccessModifier.PUBLIC;
                    });
                env.Deploy(compiled);
                TryInvalidDeploy(
                    env,
                    compiled,
                    "A precondition is not satisfied: A named window by name 'MyWindowAE' has already been created for module '(unnamed)'");
                env.UndeployAll();
            }
        }

        internal class InfraNamedWindowInvalidConsumerDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindowCDW#keepall as MySimpleKeyValueMap", path);
                TryInvalidCompile(
                    env,
                    path,
                    "select key, value as value from MyWindowCDW#time(10 sec)",
                    "Consuming statements to a named window cannot declare a data window view onto the named window [select key, value as value from MyWindowCDW#time(10 sec)]");
                env.UndeployAll();
            }
        }

        internal class InfraPriorStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsPrior = new[] {"priorKeyOne", "priorKeyTwo"};
                var fieldsStat = new[] {"average"};

                var epl = "@Name('create') create window MyWindowPS#keepall as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowPS select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@Name('s0') select prior(1, key) as priorKeyOne, prior(2, key) as priorKeyTwo from MyWindowPS;\n" +
                          "@Name('s3') select average from MyWindowPS#uni(value);\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("s3");

                Assert.AreEqual(typeof(string), env.Statement("create").EventType.GetPropertyType("key"));
                Assert.AreEqual(typeof(long?), env.Statement("create").EventType.GetPropertyType("value"));

                // send events
                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {null, null});
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").LastNewData[0],
                    fieldsStat,
                    new object[] {1d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s3"),
                    fieldsStat,
                    new[] {new object[] {1d}});

                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"E1", null});
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").LastNewData[0],
                    fieldsStat,
                    new object[] {1.5d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s3"),
                    fieldsStat,
                    new[] {new object[] {1.5d}});

                SendSupportBean(env, "E3", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"E2", "E1"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").LastNewData[0],
                    fieldsStat,
                    new object[] {5 / 3d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s3"),
                    fieldsStat,
                    new[] {new object[] {5 / 3d}});

                SendSupportBean(env, "E4", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsPrior,
                    new object[] {"E3", "E2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").LastNewData[0],
                    fieldsStat,
                    new object[] {1.75});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s3"),
                    fieldsStat,
                    new[] {new object[] {1.75d}});

                env.UndeployAll();
            }
        }

        internal class InfraLateConsumer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsWin = new[] {"key", "value"};
                var fieldsStat = new[] {"average"};
                var fieldsCnt = new[] {"cnt"};
                var path = new RegressionPath();

                var stmtTextCreate = "@Name('create') create window MyWindowLCL#keepall as MySimpleKeyValueMap";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                Assert.AreEqual(typeof(string), env.Statement("create").EventType.GetPropertyType("key"));
                Assert.AreEqual(typeof(long?), env.Statement("create").EventType.GetPropertyType("value"));

                var stmtTextInsert =
                    "insert into MyWindowLCL select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // send events
                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fieldsWin,
                    new object[] {"E1", 1L});

                SendSupportBean(env, "E2", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fieldsWin,
                    new object[] {"E2", 2L});

                var stmtTextSelectOne = "@Name('s0') select irstream average from MyWindowLCL#uni(value)";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsStat,
                    new[] {new object[] {1.5d}});

                SendSupportBean(env, "E3", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsStat,
                    new object[] {5 / 3d});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fieldsStat,
                    new object[] {3 / 2d});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsStat,
                    new[] {new object[] {5 / 3d}});

                SendSupportBean(env, "E4", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsStat,
                    new object[] {7 / 4d});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsStat,
                    new[] {new object[] {7 / 4d}});

                var stmtTextSelectTwo = "@Name('s2') select count(*) as cnt from MyWindowLCL";
                env.CompileDeploy(stmtTextSelectTwo, path);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s2"),
                    fieldsCnt,
                    new[] {new object[] {4L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsStat,
                    new[] {new object[] {7 / 4d}});

                SendSupportBean(env, "E5", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsStat,
                    new object[] {10 / 5d});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fieldsStat,
                    new object[] {7 / 4d});
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    fieldsStat,
                    new[] {new object[] {10 / 5d}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s2"),
                    fieldsCnt,
                    new[] {new object[] {5L}});

                env.UndeployAll();
            }
        }

        internal class InfraLateConsumerJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsWin = new[] {"key", "value"};
                var fieldsJoin = new[] {"key", "value", "Symbol"};
                var path = new RegressionPath();

                var stmtTextCreate = "@Name('create') create window MyWindowLCJ#keepall as MySimpleKeyValueMap";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                Assert.AreEqual(typeof(string), env.Statement("create").EventType.GetPropertyType("key"));
                Assert.AreEqual(typeof(long?), env.Statement("create").EventType.GetPropertyType("value"));

                var stmtTextInsert =
                    "insert into MyWindowLCJ select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // send events
                SendSupportBean(env, "E1", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fieldsWin,
                    new object[] {"E1", 1L});

                SendSupportBean(env, "E2", 1L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fieldsWin,
                    new object[] {"E2", 1L});

                // This replays into MyWindow
                var stmtTextSelectTwo = "@Name('s2') select key, value, Symbol from MyWindowLCJ as S0" +
                                        " left outer join SupportMarketDataBean#keepall as S1" +
                                        " on S0.Value = S1.Volume";
                env.CompileDeploy(stmtTextSelectTwo, path).AddListener("s2");
                Assert.IsFalse(env.Listener("s2").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s2"),
                    fieldsJoin,
                    new[] {new object[] {"E1", 1L, null}, new object[] {"E2", 1L, null}});

                SendMarketBean(env, "S1", 1); // join on long
                Assert.AreEqual(2, env.Listener("s2").LastNewData.Length);
                if (env.Listener("s2").LastNewData[0].Get("key").Equals("E1")) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("s2").LastNewData[0],
                        fieldsJoin,
                        new object[] {"E1", 1L, "S1"});
                    EPAssertionUtil.AssertProps(
                        env.Listener("s2").LastNewData[1],
                        fieldsJoin,
                        new object[] {"E2", 1L, "S1"});
                }
                else {
                    EPAssertionUtil.AssertProps(
                        env.Listener("s2").LastNewData[0],
                        fieldsJoin,
                        new object[] {"E2", 1L, "S1"});
                    EPAssertionUtil.AssertProps(
                        env.Listener("s2").LastNewData[1],
                        fieldsJoin,
                        new object[] {"E1", 1L, "S1"});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s2"),
                    fieldsJoin,
                    new[] {new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}});
                env.Listener("s2").Reset();

                SendMarketBean(env, "S2", 2); // join on long
                Assert.IsFalse(env.Listener("s2").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s2"),
                    fieldsJoin,
                    new[] {new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}});

                SendSupportBean(env, "E3", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("create").AssertOneGetNewAndReset(),
                    fieldsWin,
                    new object[] {"E3", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").LastNewData[0],
                    fieldsJoin,
                    new object[] {"E3", 2L, "S2"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s2"),
                    fieldsJoin,
                    new[] {
                        new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}, new object[] {"E3", 2L, "S2"}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};
                var epl = "@Name('create') create window MyWindowPAT#keepall as MySimpleKeyValueMap;\n" +
                          "@Name('s0') select a.Key as key, a.Value as value from pattern [every a=MyWindowPAT(key='S1') or a=MyWindowPAT(key='S2')];\n" +
                          "insert into MyWindowPAT select TheString as key, LongBoxed as value from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, "E1", 1L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "S1", 2L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 2L});

                SendSupportBean(env, "S1", 3L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S1", 3L});

                SendSupportBean(env, "S2", 4L);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S2", 4L});

                SendSupportBean(env, "S1", 1L);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraExternallyTimedBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"key", "value"};

                var epl =
                    "@Name('create') create window MyWindowETB#ext_timed_batch(value, 10 sec, 0L) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowETB select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@Name('s0') select irstream key, value as value from MyWindowETB;\n" +
                    "@Name('delete') on SupportMarketDataBean as S0 delete from MyWindowETB as S1 where S0.Symbol = S1.Key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("create").AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 1000L);
                SendSupportBean(env, "E2", 8000L);

                env.Milestone(1);

                SendSupportBean(env, "E3", 9999L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1000L}, new object[] {"E2", 8000L}, new object[] {"E3", 9999L}});

                env.Milestone(2);

                // delete E2
                SendMarketBean(env, "E2");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("create").AssertInvokedAndReset(),
                    fields,
                    null,
                    new[] {new object[] {"E2", 8000L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    fields,
                    null,
                    new[] {new object[] {"E2", 8000L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}});

                env.Milestone(3);

                SendSupportBean(env, "E4", 10000L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("create").AssertInvokedAndReset(),
                    fields,
                    new[] {
                        new object[] {"E1", 1000L}, new object[] {"E3", 9999L}
                    },
                    null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    fields,
                    new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}},
                    null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E4", 10000L}});

                env.Milestone(4);

                // delete E4
                SendMarketBean(env, "E4");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("create").AssertInvokedAndReset(),
                    fields,
                    null,
                    new[] {new object[] {"E4", 10000L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").AssertInvokedAndReset(),
                    fields,
                    null,
                    new[] {new object[] {"E4", 10000L}});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("create"), fields, null);

                env.Milestone(5);

                SendSupportBean(env, "E5", 14000L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E5", 14000L}});

                env.Milestone(6);

                SendSupportBean(env, "E6", 21000L);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E6", 21000L}});
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("create").AssertInvokedAndReset(),
                    fields,
                    new[] {new object[] {"E5", 14000L}},
                    new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace