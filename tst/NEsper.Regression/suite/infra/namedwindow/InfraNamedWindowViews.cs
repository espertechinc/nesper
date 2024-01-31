///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client.scopetest;

using NEsper.Avro.Extensions;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil; // tryInvalidDeploy;
// tryInvalidFAFCompile;
using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil; // assertStatelessStmt;

namespace com.espertech.esper.regressionlib.suite.infra.namedwindow
{
    /// <summary>
    /// NOTE: More namedwindow-related tests in "nwtable"
    /// </summary>
    public class InfraNamedWindowViews
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithKeepAllSimple(execs);
            WithKeepAllSceneTwo(execs);
            WithBeanBacked(execs);
            WithTimeWindow(execs);
            WithTimeWindowSceneTwo(execs);
            WithTimeFirstWindow(execs);
            WithExtTimeWindow(execs);
            WithExtTimeWindowSceneTwo(execs);
            WithExtTimeWindowSceneThree(execs);
            WithTimeOrderWindow(execs);
            WithTimeOrderSceneTwo(execs);
            WithLengthWindow(execs);
            WithLengthWindowSceneTwo(execs);
            WithLengthFirstWindow(execs);
            WithTimeAccum(execs);
            WithTimeAccumSceneTwo(execs);
            WithTimeBatch(execs);
            WithTimeBatchSceneTwo(execs);
            WithTimeBatchLateConsumer(execs);
            WithLengthBatch(execs);
            WithLengthBatchSceneTwo(execs);
            WithSortWindow(execs);
            WithSortWindowSceneTwo(execs);
            WithTimeLengthBatch(execs);
            WithTimeLengthBatchSceneTwo(execs);
            WithLengthWindowSceneThree(execs);
            WithLengthWindowPerGroup(execs);
            WithTimeBatchPerGroup(execs);
            WithDoubleInsertSameWindow(execs);
            WithLastEvent(execs);
            WithLastEventSceneTwo(execs);
            WithFirstEvent(execs);
            WithUnique(execs);
            WithUniqueSceneTwo(execs);
            WithFirstUnique(execs);
            WithBeanContained(execs);
            WithIntersection(execs);
            WithBeanSchemaBacked(execs);
            WithDeepSupertypeInsert(execs);
            WithWithDeleteUseAs(execs);
            WithWithDeleteFirstAs(execs);
            WithWithDeleteSecondAs(execs);
            WithWithDeleteNoAs(execs);
            WithFilteringConsumer(execs);
            WithSelectGroupedViewLateStart(execs);
            WithFilteringConsumerLateStart(execs);
            WithInvalid(execs);
            WithNamedWindowInvalidAlreadyExists(execs);
            WithNamedWindowInvalidConsumerDataWindow(execs);
            WithPriorStats(execs);
            WithLateConsumer(execs);
            WithLateConsumerJoin(execs);
            WithPattern(execs);
            WithExternallyTimedBatch(execs);
            WithSelectStreamDotStarInsert(execs);
            WithSelectGroupedViewLateStartVariableIterate(execs);
            WithOnInsertPremptiveTwoWindow(execs);
            WithNamedWindowTimeToLiveDelete(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowTimeToLiveDelete(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowTimeToLiveDelete());
            return execs;
        }

        public static IList<RegressionExecution> WithOnInsertPremptiveTwoWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnInsertPremptiveTwoWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectGroupedViewLateStartVariableIterate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectGroupedViewLateStartVariableIterate());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectStreamDotStarInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectStreamDotStarInsert());
            return execs;
        }

        public static IList<RegressionExecution> WithExternallyTimedBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraExternallyTimedBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithLateConsumerJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLateConsumerJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithLateConsumer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLateConsumer());
            return execs;
        }

        public static IList<RegressionExecution> WithPriorStats(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraPriorStats());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowInvalidConsumerDataWindow(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowInvalidConsumerDataWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowInvalidAlreadyExists(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNamedWindowInvalidAlreadyExists());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithFilteringConsumerLateStart(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFilteringConsumerLateStart());
            return execs;
        }

        public static IList<RegressionExecution> WithSelectGroupedViewLateStart(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectGroupedViewLateStart());
            return execs;
        }

        public static IList<RegressionExecution> WithFilteringConsumer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFilteringConsumer());
            return execs;
        }

        public static IList<RegressionExecution> WithWithDeleteNoAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWithDeleteNoAs());
            return execs;
        }

        public static IList<RegressionExecution> WithWithDeleteSecondAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWithDeleteSecondAs());
            return execs;
        }

        public static IList<RegressionExecution> WithWithDeleteFirstAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWithDeleteFirstAs());
            return execs;
        }

        public static IList<RegressionExecution> WithWithDeleteUseAs(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraWithDeleteUseAs());
            return execs;
        }

        public static IList<RegressionExecution> WithDeepSupertypeInsert(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeepSupertypeInsert());
            return execs;
        }

        public static IList<RegressionExecution> WithBeanSchemaBacked(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraBeanSchemaBacked());
            return execs;
        }

        public static IList<RegressionExecution> WithIntersection(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraIntersection());
            return execs;
        }

        public static IList<RegressionExecution> WithBeanContained(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraBeanContained());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFirstUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithUniqueSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUniqueSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithUnique(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraUnique());
            return execs;
        }

        public static IList<RegressionExecution> WithFirstEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraFirstEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithLastEventSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLastEventSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithLastEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLastEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithDoubleInsertSameWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDoubleInsertSameWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchPerGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeBatchPerGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindowPerGroup(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthWindowPerGroup());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindowSceneThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthWindowSceneThree());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeLengthBatchSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeLengthBatchSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeLengthBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeLengthBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithSortWindowSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSortWindowSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithSortWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSortWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthBatchSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthBatchSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchLateConsumer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeBatchLateConsumer());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatchSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeBatchSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeBatch(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeBatch());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeAccumSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeAccumSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeAccum(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeAccum());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthFirstWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthFirstWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindowSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthWindowSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithLengthWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraLengthWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeOrderSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeOrderSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeOrderWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeOrderWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithExtTimeWindowSceneThree(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraExtTimeWindowSceneThree());
            return execs;
        }

        public static IList<RegressionExecution> WithExtTimeWindowSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraExtTimeWindowSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithExtTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraExtTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeFirstWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeFirstWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindowSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeWindowSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithTimeWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTimeWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithBeanBacked(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraBeanBacked());
            return execs;
        }

        public static IList<RegressionExecution> WithKeepAllSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraKeepAllSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithKeepAllSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraKeepAllSimple());
            return execs;
        }

        internal class InfraNamedWindowTimeToLiveDelete : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('win') create window MyWindow#timetolive(current_timestamp() + LongPrimitive) as SupportBean;\n" +
                    "on SupportBean merge MyWindow insert select *;\n" +
                    "on SupportBean_S0 delete from MyWindow where TheString = P00;\n";
                env.AdvanceTime(0);
                env.CompileDeploy(epl);

                SendSupportBeanLongPrim(env, "E1", 2000);
                SendSupportBeanLongPrim(env, "E2", 3000);
                SendSupportBeanLongPrim(env, "E3", 1000);
                SendSupportBeanLongPrim(env, "E4", 2000);
                AssertIterate(env, "E1", "E2", "E3", "E4");

                env.AdvanceTime(500);

                SendS0(env, "E2");
                AssertIterate(env, "E1", "E3", "E4");

                env.Milestone(0);

                SendS0(env, "E1");
                AssertIterate(env, "E3", "E4");

                env.Milestone(1);

                env.AdvanceTime(1000);
                AssertIterate(env, "E4");

                env.Milestone(2);

                env.AdvanceTime(2000);
                AssertIterate(env);

                env.UndeployAll();
            }

            private void SendS0(
                RegressionEnvironment env,
                string p00)
            {
                env.SendEventBean(new SupportBean_S0(0, p00));
            }

            private void AssertIterate(
                RegressionEnvironment env,
                params string[] values)
            {
                IList<string> strings = new List<string>();
                env.AssertIterator(
                    "win",
                    enumerator => {
                        enumerator.ForEachRemaining(@event => strings.Add(@event.Get("TheString").ToString()));
                        EPAssertionUtil.AssertEqualsAnyOrder(values, strings.ToArray());
                    });
            }
        }

        internal class InfraKeepAllSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();

                var path = new RegressionPath();
                var eplCreate = "@name('create') @public create window MyWindow.win:keepall() as SupportBean";
                env.CompileDeploy(eplCreate, path).AddListener("create");

                var eplInsert = "@name('insert') insert into MyWindow select * from SupportBean";
                env.CompileDeploy(eplInsert, path);

                env.Milestone(0);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("create", fields, new object[] { "E1" });

                env.Milestone(1);

                SendSupportBean(env, "E2");
                env.AssertPropsNew("create", fields, new object[] { "E2" });

                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("create");

                env.Milestone(2);
            }
        }

        internal class InfraKeepAllSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:keepall() as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "@name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                SendBeanInt(env, "G1", 10);
                env.AssertPropsNew("create", fields, new object[] { "G1", 10 });

                env.Milestone(0);

                // send event G2
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G2", 20);
                env.AssertPropsNew("create", fields, new object[] { "G2", 20 });

                env.Milestone(1);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G2", 20 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 20 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });

                env.Milestone(2);

                // send event G3
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G3", 30);
                env.AssertPropsNew("create", fields, new object[] { "G3", 30 });

                env.Milestone(3);

                // delete event G1
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G3", 30 } });
                SendMarketBean(env, "G1");
                env.AssertPropsOld("create", fields, new object[] { "G1", 10 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 30 } });

                env.Milestone(4);

                // send event G4
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 30 } });
                SendBeanInt(env, "G4", 40);
                env.AssertPropsNew("create", fields, new object[] { "G4", 40 });

                env.Milestone(5);

                // send event G5
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 } });
                SendBeanInt(env, "G5", 50);
                env.AssertPropsNew("create", fields, new object[] { "G5", 50 });

                env.Milestone(6);

                // send event G6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 }, new object[] { "G5", 50 } });
                SendBeanInt(env, "G6", 60);
                env.AssertPropsNew("create", fields, new object[] { "G6", 60 });

                env.Milestone(7);

                // delete event G6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] {
                        new object[] { "G3", 30 }, new object[] { "G4", 40 }, new object[] { "G5", 50 },
                        new object[] { "G6", 60 }
                    });
                SendMarketBean(env, "G6");
                env.AssertPropsOld("create", fields, new object[] { "G6", 60 });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 }, new object[] { "G5", 50 } });

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
                    EventRepresentationChoice.OBJECTARRAY.GetAnnotationText() +
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
                TryAssertionBeanBacked(env, EventRepresentationChoice.OBJECTARRAY);
                TryAssertionBeanBacked(env, EventRepresentationChoice.MAP);
                TryAssertionBeanBacked(env, EventRepresentationChoice.DEFAULT);
                TryAssertionBeanBacked(env, EventRepresentationChoice.AVRO);
            }
        }

        internal class InfraBeanContained : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                foreach (var rep in EventRepresentationChoiceExtensions.Values()) {
                    if (!rep.IsAvroOrJsonEvent()) {
                        TryAssertionBeanContained(env, rep);
                    }
                }

                var epl = EventRepresentationChoice.AVRO.GetAnnotationText() +
                          " @name('create') create window MyWindowBC#keepall as (bean SupportBean_S0)";
                env.TryInvalidCompile(
                    epl,
                    "Property 'bean' type '" +
                    typeof(SupportBean_S0).FullName +
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
                    "@name('s0') select irstream * from MyWindowINT");

                var fields = "TheString".SplitCsv();
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E1" } }, null);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsPerRowIRPair("s0", fields, new object[][] { new object[] { "E2" } }, null);

                env.SendEventBean(new SupportBean("E3", 2));
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        listener.AssertInvokedAndReset(),
                        fields,
                        new object[][] { new object[] { "E3" } },
                        new object[][] { new object[] { "E1" }, new object[] { "E2" } }));

                env.UndeployAll();
            }
        }

        internal class InfraBeanSchemaBacked : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // Test create from schema
                var epl = "@public create schema ABC as " +
                          typeof(SupportBean).FullName +
                          ";\n" +
                          "@public create window MyWindowBSB#keepall as ABC;\n" +
                          "insert into MyWindowBSB select * from SupportBean;\n";
                env.CompileDeploy(epl, path);

                env.SendEventBean(new SupportBean());
                env.AssertThat(
                    () => AssertEvent(
                        env.CompileExecuteFAF("select * from MyWindowBSB", path).Array[0],
                        "MyWindowBSB"));

                env.CompileDeploy("@name('s0') select * from ABC", path).AddListener("s0");

                env.SendEventBean(new SupportBean());
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraDeepSupertypeInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('create') create window MyWindowDSI#keepall as select * from SupportOverrideBase;\n" +
                          "insert into MyWindowDSI select * from SupportOverrideOneA;\n";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportOverrideOneA("1a", "1", "base"));
                env.AssertIterator("create", iterator => ClassicAssert.AreEqual("1a", iterator.Advance().Get("Val")));
                env.UndeployAll();
            }
        }

        internal class InfraOnInsertPremptiveTwoWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema TypeOne(col1 int);\n";
                epl += "@public @buseventtype create schema TypeTwo(col2 int);\n";
                epl += "@public @buseventtype create schema TypeTrigger(trigger int);\n";
                epl += "create window WinOne#keepall as TypeOne;\n";
                epl += "create window WinTwo#keepall as TypeTwo;\n";

                epl += "@name('insert-window-one') insert into WinOne(col1) select IntPrimitive from SupportBean;\n";

                epl += "@name('insert-otherstream') on TypeTrigger insert into OtherStream select col1 from WinOne;\n";
                epl += "@name('insert-window-two') on TypeTrigger insert into WinTwo(col2) select col1 from WinOne;\n";
                epl += "@name('s0') on OtherStream select col2 from WinTwo;\n";

                env.CompileDeploy(epl, new RegressionPath()).AddListener("s0");

                // populate WinOne
                env.SendEventBean(new SupportBean("E1", 9));

                // fire trigger
                if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                    env.EventService.GetEventSender("TypeTrigger").SendEvent(Array.Empty<object>());
                }
                else {
                    env.EventService.GetEventSender("TypeTrigger").SendEvent(new Dictionary<string, object>());
                }

                env.AssertEqualsNew("s0", "col2", 9);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EVENTSENDER);
            }
        }

        internal class InfraWithDeleteUseAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as MySimpleKeyValueMap",
                    "on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key");
            }
        }

        internal class InfraWithDeleteFirstAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as select key, value from MySimpleKeyValueMap",
                    "on SupportMarketDataBean delete from MyWindow as s1 where Symbol = s1.key");
            }
        }

        internal class InfraWithDeleteSecondAs : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryCreateWindow(
                    env,
                    "create window MyWindow#keepall as MySimpleKeyValueMap",
                    "on SupportMarketDataBean as s0 delete from MyWindow where s0.Symbol = key");
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
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindowTW#time(10 sec) as MySimpleKeyValueMap";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");
                env.AssertPropsPerRowIterator("create", fields, null);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindowTW select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create consumer
                var stmtTextSelectOne = "@name('s0') select irstream key, value as value from MyWindowTW";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");
                var listenerStmtOne = new SupportUpdateListener();

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowTW as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L } });

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 3L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                // Should push out the window
                SendTimer(env, 10999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 11000);
                env.AssertPropsOld("create", fields, new object[] { "E1", 1L });
                env.AssertPropsOld("s0", fields, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                SendSupportBean(env, "E4", 4L);
                env.AssertPropsNew("create", fields, new object[] { "E4", 4L });
                env.AssertPropsNew("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E2", 2L }, new object[] { "E3", 3L }, new object[] { "E4", 4L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 2L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E3", 3L }, new object[] { "E4", 4L } });

                // nothing pushed
                SendTimer(env, 15000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                // push last event
                SendTimer(env, 19999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                SendTimer(env, 20000);
                env.AssertPropsOld("create", fields, new object[] { "E3", 3L });
                env.AssertPropsOld("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E4", 4L } });

                // delete E4
                SendMarketBean(env, "E4");
                env.AssertPropsOld("create", fields, new object[] { "E4", 4L });
                env.AssertPropsOld("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 100000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraTimeWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow#time(10 sec) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "@name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create consumer
                var stmtTextSelectOne = "@name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("consume");

                env.Milestone(2);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(3);

                // send event
                env.AdvanceTime(0);
                SendBeanInt(env, "G1", 10);
                env.AssertPropsNew("create", fields, new object[] { "G1", 10 });

                env.Milestone(4);

                // send event
                env.AdvanceTime(5000);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G2", 20);
                env.AssertPropsNew("create", fields, new object[] { "G2", 20 });

                env.Milestone(5);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G2", 20 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 20 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });

                env.Milestone(6);

                // move time window
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                env.AdvanceTime(10000);
                env.AssertPropsOld("create", fields, new object[] { "G1", 10 });

                env.Milestone(7);

                env.AdvanceTime(25000);
                env.AssertListenerNotInvoked("create");

                env.Milestone(8);

                // send events
                env.AdvanceTime(25000);
                SendBeanInt(env, "G3", 30);
                env.AdvanceTime(26000);
                SendBeanInt(env, "G4", 40);
                env.AdvanceTime(27000);
                SendBeanInt(env, "G5", 50);
                env.ListenerReset("create");

                env.Milestone(9);

                // delete g3
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 }, new object[] { "G5", 50 } });
                SendMarketBean(env, "G3");
                env.AssertPropsOld("create", fields, new object[] { "G3", 30 });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 40 }, new object[] { "G5", 50 } });

                env.Milestone(10);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 40 }, new object[] { "G5", 50 } });
                env.AdvanceTime(35999);
                env.AssertListenerNotInvoked("create");
                env.AdvanceTime(36000);
                env.AssertPropsOld("create", fields, new object[] { "G4", 40 });

                env.Milestone(11);

                // delete g5
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G5", 50 } });
                SendMarketBean(env, "G5");
                env.AssertPropsOld("create", fields, new object[] { "G5", 50 });
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(12);

                env.AssertPropsPerRowIterator("create", fields, null);

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
                var fields = new string[] { "key", "value" };

                SendTimer(env, 1000);

                var epl = "@name('create') create window MyWindowTFW#firsttime(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTFW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowTFW;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowTFW as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("delete");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L } });

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 3L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                // Should not push out the window
                SendTimer(env, 12000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                SendSupportBean(env, "E4", 4L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 2L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L } });

                // nothing pushed
                SendTimer(env, 100000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraExtTimeWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl =
                    "@name('create') create window MyWindowETW#ext_timed(value, 10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowETW select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@name('s0') select irstream key, value as value from MyWindowETW;\n" +
                    "@name('delete') on SupportMarketDataBean delete from MyWindowETW where Symbol = key";
                env.CompileDeploy(epl).AddListener("s0").AddListener("create").AddListener("delete");

                SendSupportBean(env, "E1", 1000L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1000L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1000L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1000L } });

                SendSupportBean(env, "E2", 5000L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 5000L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 5000L });

                SendSupportBean(env, "E3", 10000L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 10000L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 10000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E1", 1000L }, new object[] { "E2", 5000L }, new object[] { "E3", 10000L } });

                // Should push out the window
                SendSupportBean(env, "E4", 11000L);
                env.AssertListener(
                    "create",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E4", 11000L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E1", 1000L });
                        listener.Reset();
                    });
                env.AssertPropsIRPair("s0", fields, new object[] { "E4", 11000L }, new object[] { "E1", 1000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E2", 5000L }, new object[] { "E3", 10000L }, new object[] { "E4", 11000L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 5000L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 5000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E3", 10000L }, new object[] { "E4", 11000L } });

                // nothing pushed other then E5 (E2 is deleted)
                SendSupportBean(env, "E5", 15000L);
                env.AssertPropsNew("s0", fields, new object[] { "E5", 15000L });
                env.AssertPropsNew("create", fields, new object[] { "E5", 15000L });

                env.UndeployAll();
            }
        }

        internal class InfraExtTimeWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:ext_timed(value, 10 sec) as select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, LongBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create consumer
                var stmtTextSelectOne = "@name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("consume");

                env.Milestone(2);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(3);

                // send event
                SendBeanLong(env, "G1", 0L);
                env.AssertPropsNew("create", fields, new object[] { "G1", 0L });

                env.Milestone(4);

                // send event
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 0L } });
                SendBeanLong(env, "G2", 5000L);
                env.AssertPropsNew("create", fields, new object[] { "G2", 5000L });

                env.Milestone(5);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 0L }, new object[] { "G2", 5000L } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 5000L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 0L } });

                env.Milestone(6);

                // move time window
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 0L } });
                SendBeanLong(env, "G3", 10000L);
                env.AssertListener(
                    "create",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G3", 10000L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G1", 0L });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 10000L } });

                env.Milestone(7);

                // move time window
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 10000L } });
                SendBeanLong(env, "G4", 15000L);
                env.AssertPropsNew("create", fields, new object[] { "G4", 15000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 10000L }, new object[] { "G4", 15000L } });

                env.Milestone(8);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 10000L }, new object[] { "G4", 15000L } });
                SendMarketBean(env, "G3");
                env.AssertPropsOld("create", fields, new object[] { "G3", 10000L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G4", 15000L } });

                env.Milestone(9);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G4", 15000L } });
                SendBeanLong(env, "G5", 21000L);
                env.AssertPropsNew("create", fields, new object[] { "G5", 21000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 15000L }, new object[] { "G5", 21000L } });

                env.Milestone(10);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 15000L }, new object[] { "G5", 21000L } });

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

        internal class InfraExtTimeWindowSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var path = new RegressionPath();

                env.AdvanceTime(0);
                env.CompileDeploy("@public create window ABCWin.win:time(10 sec) as SupportBean", path);
                env.CompileDeploy("insert into ABCWin select * from SupportBean", path);
                env.CompileDeploy("@name('s0') select irstream * from ABCWin", path);
                env.CompileDeploy("on SupportBean_A delete from ABCWin where TheString = Id", path);
                env.AddListener("s0");

                env.Milestone(0);
                env.AssertPropsPerRowIterator("s0", fields, null);

                env.AdvanceTime(1000);
                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1" } });

                env.AdvanceTime(2000);
                SendSupportBean_A(env, "E1"); // delete E1
                env.AssertPropsOld("s0", fields, new object[] { "E1" });

                env.Milestone(2);
                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());

                env.AdvanceTime(3000);
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });

                env.AdvanceTime(3000);
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(3);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });
                SendSupportBean_A(env, "E3"); // delete E3
                env.AssertPropsOld("s0", fields, new object[] { "E3" });

                env.Milestone(4);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E2" } });
                env.AdvanceTime(12999);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(13000);
                env.AssertPropsOld("s0", fields, new object[] { "E2" });

                env.Milestone(5);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());

                env.UndeployAll();
            }
        }

        internal class InfraTimeOrderWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl =
                    "@name('create') @public create window MyWindowTOW#time_order(value, 10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTOW select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@name('s0') select irstream key, value as value from MyWindowTOW;\n" +
                    "@name('delete') on SupportMarketDataBean delete from MyWindowTOW where Symbol = key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendTimer(env, 5000);
                SendSupportBean(env, "E1", 3000L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 3000L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 3000L });

                SendTimer(env, 6000);
                SendSupportBean(env, "E2", 2000L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 2000L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2000L });

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 1000L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 1000L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 1000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E3", 1000L }, new object[] { "E2", 2000L }, new object[] { "E1", 3000L } });

                // Should push out the window
                SendTimer(env, 11000);
                env.AssertPropsOld("create", fields, new object[] { "E3", 1000L });
                env.AssertPropsOld("s0", fields, new object[] { "E3", 1000L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E2", 2000L }, new object[] { "E1", 3000L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 2000L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2000L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 3000L } });

                SendTimer(env, 12999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 13000);
                env.AssertPropsOld("create", fields, new object[] { "E1", 3000L });
                env.AssertPropsOld("s0", fields, new object[] { "E1", 3000L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 100000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraTimeOrderSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.ext:time_order(value, 10) as select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, LongBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                env.AdvanceTime(20000);
                SendBeanLong(env, "G1", 23000);
                env.AssertPropsNew("create", fields, new object[] { "G1" });

                env.Milestone(0);

                // send event G2
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1" } });
                env.AdvanceTime(20000);
                SendBeanLong(env, "G2", 19000);
                env.AssertPropsNew("create", fields, new object[] { "G2" });

                env.Milestone(1);

                // send event G3, pass through
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G2" }, new object[] { "G1" } });
                env.AdvanceTime(21000);
                SendBeanLong(env, "G3", 10000);
                env.AssertListener(
                    "create",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G3" });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G3" });
                        listener.Reset();
                    });

                env.Milestone(2);

                // delete G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G2" }, new object[] { "G1" } });
                env.AdvanceTime(21000);
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2" });

                env.Milestone(3);

                // send event G4
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1" } });
                env.AdvanceTime(22000);
                SendBeanLong(env, "G4", 18000);
                env.AssertPropsNew("create", fields, new object[] { "G4" });

                env.Milestone(4);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4" }, new object[] { "G1" } });
                env.AdvanceTime(23000);
                SendBeanLong(env, "G5", 22000);
                env.AssertPropsNew("create", fields, new object[] { "G5" });

                env.Milestone(5);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4" }, new object[] { "G5" }, new object[] { "G1" } });
                env.AdvanceTime(27999);
                env.AssertListenerNotInvoked("create");
                env.AdvanceTime(28000);
                env.AssertPropsOld("create", fields, new object[] { "G4" });

                env.Milestone(6);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G5" }, new object[] { "G1" } });
                env.AdvanceTime(31999);
                env.AssertListenerNotInvoked("create");
                env.AdvanceTime(32000);
                env.AssertPropsOld("create", fields, new object[] { "G5" });

                env.Milestone(7);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1" } });
                env.AdvanceTime(32000);
                SendBeanLong(env, "G6", 25000);
                env.AssertPropsNew("create", fields, new object[] { "G6" });

                env.Milestone(8);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1" }, new object[] { "G6" } });
                env.AdvanceTime(32000);
                SendMarketBean(env, "G1");
                env.AssertPropsOld("create", fields, new object[] { "G1" });

                env.Milestone(9);

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G6" } });
                env.AdvanceTime(34999);
                env.AssertListenerNotInvoked("create");
                env.AdvanceTime(35000);
                env.AssertPropsOld("create", fields, new object[] { "G6" });

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
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowLW#length(3) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowLW;\n" +
                          "@name('delete') on SupportMarketDataBean delete from MyWindowLW where Symbol = key";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });

                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2L });

                SendSupportBean(env, "E3", 3L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 3L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 2L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L } });

                SendSupportBean(env, "E4", 4L);
                env.AssertPropsNew("create", fields, new object[] { "E4", 4L });
                env.AssertPropsNew("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L }, new object[] { "E4", 4L } });

                SendSupportBean(env, "E5", 5L);
                env.AssertPropsIRPair("create", fields, new object[] { "E5", 5L }, new object[] { "E1", 1L });
                env.AssertPropsIRPair("s0", fields, new object[] { "E5", 5L }, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E3", 3L }, new object[] { "E4", 4L }, new object[] { "E5", 5L } });

                SendSupportBean(env, "E6", 6L);
                env.AssertPropsIRPair("create", fields, new object[] { "E6", 6L }, new object[] { "E3", 3L });
                env.AssertPropsIRPair("s0", fields, new object[] { "E6", 6L }, new object[] { "E3", 3L });

                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraLengthWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:length(3) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                SendBeanInt(env, "G1", 10);
                env.AssertPropsNew("create", fields, new object[] { "G1", 10 });

                env.Milestone(0);

                // send event G2
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G2", 20);
                env.AssertPropsNew("create", fields, new object[] { "G2", 20 });

                env.Milestone(1);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G2", 20 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 20 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });

                env.Milestone(2);

                // send event G3
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G3", 30);
                env.AssertPropsNew("create", fields, new object[] { "G3", 30 });

                env.Milestone(3);

                // delete event G1
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G3", 30 } });
                SendMarketBean(env, "G1");
                env.AssertPropsOld("create", fields, new object[] { "G1", 10 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 30 } });

                env.Milestone(4);

                // send event G4
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 30 } });
                SendBeanInt(env, "G4", 40);
                env.AssertPropsNew("create", fields, new object[] { "G4", 40 });

                env.Milestone(5);

                // send event G5
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 } });
                SendBeanInt(env, "G5", 50);
                env.AssertPropsNew("create", fields, new object[] { "G5", 50 });

                env.Milestone(6);

                // send event G6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 }, new object[] { "G5", 50 } });
                SendBeanInt(env, "G6", 60);
                env.AssertPropsIRPair("create", fields, new object[] { "G6", 60 }, new object[] { "G3", 30 });

                env.Milestone(7);

                // delete event G6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 40 }, new object[] { "G5", 50 }, new object[] { "G6", 60 } });
                SendMarketBean(env, "G6");
                env.AssertPropsOld("create", fields, new object[] { "G6", 60 });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 40 }, new object[] { "G5", 50 } });

                env.Milestone(8);

                // send event G7
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 40 }, new object[] { "G5", 50 } });
                SendBeanInt(env, "G7", 70);
                env.AssertPropsNew("create", fields, new object[] { "G7", 70 });

                env.Milestone(9);

                // send event G8
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G4", 40 }, new object[] { "G5", 50 }, new object[] { "G7", 70 } });
                SendBeanInt(env, "G8", 80);
                env.AssertPropsIRPair("create", fields, new object[] { "G8", 80 }, new object[] { "G4", 40 });

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
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowLFW#firstlength(2) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLFW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowLFW;\n" +
                          "@name('delete') on SupportMarketDataBean delete from MyWindowLFW where Symbol = key";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });

                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2L });

                SendSupportBean(env, "E3", 3L);
                env.AssertListenerNotInvoked("create");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 2L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendSupportBean(env, "E4", 4L);
                env.AssertPropsNew("create", fields, new object[] { "E4", 4L });
                env.AssertPropsNew("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E4", 4L } });

                SendSupportBean(env, "E5", 5L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E4", 4L } });

                env.UndeployAll();
            }
        }

        internal class InfraTimeAccum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                // create window
                var epl = "@name('create') create window MyWindowTA#time_accum(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTA select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowTA;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowTA as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 2L });

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 3L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3L });

                SendTimer(env, 15000);
                SendSupportBean(env, "E4", 4L);
                env.AssertPropsNew("create", fields, new object[] { "E4", 4L });
                env.AssertPropsNew("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] {
                        new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L },
                        new object[] { "E4", 4L }
                    });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 2L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L }, new object[] { "E4", 4L } });

                // nothing pushed
                SendTimer(env, 24999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 25000);
                env.AssertListener(
                    "create",
                    listener => {
                        var oldData = listener.LastOldData;
                        ClassicAssert.AreEqual(3, oldData.Length);
                        EPAssertionUtil.AssertProps(oldData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(oldData[1], fields, new object[] { "E3", 3L });
                        EPAssertionUtil.AssertProps(oldData[2], fields, new object[] { "E4", 4L });
                        listener.Reset();
                    });
                env.ListenerReset("s0");
                env.AssertPropsPerRowIterator("create", fields, null);

                // delete E4
                SendMarketBean(env, "E4");
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 30000);
                SendSupportBean(env, "E5", 5L);
                env.AssertPropsNew("create", fields, new object[] { "E5", 5L });
                env.AssertPropsNew("s0", fields, new object[] { "E5", 5L });

                SendTimer(env, 31000);
                SendSupportBean(env, "E6", 6L);
                env.AssertPropsNew("create", fields, new object[] { "E6", 6L });
                env.AssertPropsNew("s0", fields, new object[] { "E6", 6L });

                SendTimer(env, 38000);
                SendSupportBean(env, "E7", 7L);
                env.AssertPropsNew("create", fields, new object[] { "E7", 7L });
                env.AssertPropsNew("s0", fields, new object[] { "E7", 7L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E5", 5L }, new object[] { "E6", 6L }, new object[] { "E7", 7L } });

                // delete E7 - deleting the last should spit out the first 2 timely
                SendMarketBean(env, "E7");
                env.AssertPropsOld("create", fields, new object[] { "E7", 7L });
                env.AssertPropsOld("s0", fields, new object[] { "E7", 7L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E5", 5L }, new object[] { "E6", 6L } });

                SendTimer(env, 40999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 41000);
                env.AssertListener(
                    "s0",
                    listener => {
                        ClassicAssert.IsNull(listener.LastNewData);
                        var oldData = listener.LastOldData;
                        ClassicAssert.AreEqual(2, oldData.Length);
                        EPAssertionUtil.AssertProps(oldData[0], fields, new object[] { "E5", 5L });
                        EPAssertionUtil.AssertProps(oldData[1], fields, new object[] { "E6", 6L });
                        listener.Reset();
                    });
                env.ListenerReset("create");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 50000);
                SendSupportBean(env, "E8", 8L);
                env.AssertPropsNew("create", fields, new object[] { "E8", 8L });
                env.AssertPropsNew("s0", fields, new object[] { "E8", 8L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E8", 8L } });

                SendTimer(env, 55000);
                SendMarketBean(env, "E8");
                env.AssertPropsOld("create", fields, new object[] { "E8", 8L });
                env.AssertPropsOld("s0", fields, new object[] { "E8", 8L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 100000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraTimeAccumSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:time_accum(10 sec) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "@name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create consumer
                var stmtTextSelectOne = "@name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("consume");

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(0);

                // send event
                env.AdvanceTime(1000);
                SendBeanInt(env, "G1", 1);
                env.AssertPropsNew("create", fields, new object[] { "G1", 1 });

                env.Milestone(1);

                // send event
                env.AdvanceTime(5000);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                SendBeanInt(env, "G2", 2);
                env.AssertPropsNew("create", fields, new object[] { "G2", 2 });

                env.Milestone(2);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1 }, new object[] { "G2", 2 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 2 });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });

                env.Milestone(3);

                // move time window
                env.AdvanceTime(10999);
                env.AssertListenerNotInvoked("create");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                env.AdvanceTime(11000);
                env.AssertPropsOld("create", fields, new object[] { "G1", 1 });
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(4);

                // Send G3
                env.AssertPropsPerRowIterator("create", fields, null);
                env.AdvanceTime(20000);
                SendBeanInt(env, "G3", 3);
                env.AssertPropsNew("create", fields, new object[] { "G3", 3 });

                env.Milestone(5);

                // Send G4
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 3 } });
                env.AdvanceTime(29999);
                SendBeanInt(env, "G4", 4);
                env.AssertPropsNew("create", fields, new object[] { "G4", 4 });

                env.Milestone(6);

                // Delete G3
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 } });
                SendMarketBean(env, "G3");
                env.AssertPropsOld("create", fields, new object[] { "G3", 3 });

                env.Milestone(7);

                // Delete G4
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G4", 4 } });
                SendMarketBean(env, "G4");
                env.AssertPropsOld("create", fields, new object[] { "G4", 4 });

                env.Milestone(8);

                // Send timer, no events
                env.AssertPropsPerRowIterator("create", fields, null);
                env.AdvanceTime(40000);
                env.AssertListenerNotInvoked("create");

                env.Milestone(9);

                // Send G5
                env.AdvanceTime(41000);
                SendBeanInt(env, "G5", 5);
                env.AssertPropsNew("create", fields, new object[] { "G5", 5 });

                env.Milestone(10);

                // Send G6
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G5", 5 } });
                env.AdvanceTime(42000);
                SendBeanInt(env, "G6", 6);
                env.AssertPropsNew("create", fields, new object[] { "G6", 6 });

                env.Milestone(11);

                // Send G7
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G5", 5 }, new object[] { "G6", 6 } });
                env.AdvanceTime(43000);
                SendBeanInt(env, "G7", 7);
                env.AssertPropsNew("create", fields, new object[] { "G7", 7 });

                env.Milestone(12);

                // Send G8
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G5", 5 }, new object[] { "G6", 6 }, new object[] { "G7", 7 } });
                env.AdvanceTime(44000);
                SendBeanInt(env, "G8", 8);
                env.AssertPropsNew("create", fields, new object[] { "G8", 8 });

                env.Milestone(13);

                // Delete G6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] {
                        new object[] { "G5", 5 }, new object[] { "G6", 6 }, new object[] { "G7", 7 },
                        new object[] { "G8", 8 }
                    });
                SendMarketBean(env, "G6");
                env.AssertPropsOld("create", fields, new object[] { "G6", 6 });

                env.Milestone(14);

                // Delete G8
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G5", 5 }, new object[] { "G7", 7 }, new object[] { "G8", 8 } });
                SendMarketBean(env, "G8");
                env.AssertPropsOld("create", fields, new object[] { "G8", 8 });

                env.Milestone(15);

                env.AdvanceTime(52999);
                env.AssertListenerNotInvoked("create");
                env.AdvanceTime(53000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G5", 5 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "G7", 7 });
                        ClassicAssert.IsNull(listener.LastNewData);
                        listener.Reset();
                    });

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
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowTB#time_batch(10 sec) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowTB select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select key, value as value from MyWindowTB;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowTB as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);

                SendTimer(env, 10000);
                SendSupportBean(env, "E3", 3L);
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L }, new object[] { "E3", 3L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L } });

                // nothing pushed
                SendTimer(env, 10999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 11000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.IsNull(listener.LastOldData);
                        var newData = listener.LastNewData;
                        ClassicAssert.AreEqual(2, newData.Length);
                        EPAssertionUtil.AssertProps(newData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(newData[1], fields, new object[] { "E3", 3L });
                        listener.Reset();
                    });
                env.ListenerReset("s0");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 21000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.IsNull(listener.LastNewData);
                        var oldData = listener.LastOldData;
                        ClassicAssert.AreEqual(2, oldData.Length);
                        EPAssertionUtil.AssertProps(oldData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(oldData[1], fields, new object[] { "E3", 3L });
                        listener.Reset();
                    });
                env.ListenerReset("s0");

                // send and delete E4, leaving an empty batch
                SendSupportBean(env, "E4", 4L);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E4", 4L } });

                SendMarketBean(env, "E4");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 31000);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraTimeBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:time_batch(10) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event
                env.AdvanceTime(1000);
                SendBeanInt(env, "G1", 1);
                env.AssertListenerNotInvoked("create");

                env.Milestone(0);

                // send event
                env.AdvanceTime(5000);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                SendBeanInt(env, "G2", 2);
                env.AssertListenerNotInvoked("create");

                env.Milestone(1);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1 }, new object[] { "G2", 2 } });
                SendMarketBean(env, "G2");
                env.AssertListenerNotInvoked("create");

                env.Milestone(2);

                // delete event G1
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                SendMarketBean(env, "G1");
                env.AssertListenerNotInvoked("create");

                env.Milestone(3);

                env.AssertPropsPerRowIterator("create", fields, null);
                env.AdvanceTime(11000);
                env.AssertListenerNotInvoked("create");

                env.Milestone(4);

                // Send g3, g4 and g5
                env.AdvanceTime(15000);
                SendBeanInt(env, "G3", 3);
                SendBeanInt(env, "G4", 4);
                SendBeanInt(env, "G5", 5);

                env.Milestone(5);

                // Delete g5
                SendMarketBean(env, "G5");
                env.AssertListenerNotInvoked("create");

                env.Milestone(6);

                // send g6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 } });
                env.AdvanceTime(18000);
                SendBeanInt(env, "G6", 6);
                env.AssertListenerNotInvoked("create");

                env.Milestone(7);

                // flush batch
                env.AdvanceTime(21000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G3", 3 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "G4", 4 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "G6", 6 });
                        ClassicAssert.IsNull(listener.LastOldData);
                        env.ListenerReset("create");
                    });
                env.AssertPropsPerRowIterator("create", fields, null);

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
                env.AssertListenerNotInvoked("create");

                env.Milestone(10);

                // flush
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G8", 8 } });
                env.AdvanceTime(31000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G8", 8 });
                        ClassicAssert.AreEqual(3, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G3", 3 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "G4", 4 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[2], fields, new object[] { "G6", 6 });
                        listener.Reset();
                    });

                env.AssertPropsPerRowIterator("create", fields, null);

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

                var epl =
                    "@name('create') @public create window MyWindowTBLC#time_batch(10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTBLC select TheString as key, LongBoxed as value from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendTimer(env, 0);
                SendSupportBean(env, "E1", 1L);

                SendTimer(env, 5000);
                SendSupportBean(env, "E2", 2L);

                // create consumer
                var stmtTextSelectOne = "@name('s0') select sum(value) as value from MyWindowTBLC";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");

                SendTimer(env, 8000);
                SendSupportBean(env, "E3", 3L);
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 10000);
                env.AssertPropsNew("s0", new string[] { "value" }, new object[] { 6L });
                env.AssertPropsPerRowIterator("create", new string[] { "value" }, null);

                env.UndeployAll();
            }
        }

        internal class InfraLengthBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowLB#length_batch(3) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLB select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select key, value as value from MyWindowLB;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowLB as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                SendSupportBean(env, "E2", 2L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendSupportBean(env, "E3", 3L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L } });

                SendSupportBean(env, "E4", 4L);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.IsNull(listener.LastOldData);
                        var newData = listener.LastNewData;
                        ClassicAssert.AreEqual(3, newData.Length);
                        EPAssertionUtil.AssertProps(newData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(newData[1], fields, new object[] { "E3", 3L });
                        EPAssertionUtil.AssertProps(newData[2], fields, new object[] { "E4", 4L });
                        listener.Reset();
                    });
                env.ListenerReset("s0");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E5", 5L);
                SendSupportBean(env, "E6", 6L);
                SendMarketBean(env, "E5");
                SendMarketBean(env, "E6");
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E7", 7L);
                SendSupportBean(env, "E8", 8L);
                SendSupportBean(env, "E9", 9L);
                env.AssertListener(
                    "create",
                    listener => {
                        var oldData = listener.LastOldData;
                        var newData = listener.LastNewData;
                        ClassicAssert.AreEqual(3, newData.Length);
                        ClassicAssert.AreEqual(3, oldData.Length);
                        EPAssertionUtil.AssertProps(newData[0], fields, new object[] { "E7", 7L });
                        EPAssertionUtil.AssertProps(newData[1], fields, new object[] { "E8", 8L });
                        EPAssertionUtil.AssertProps(newData[2], fields, new object[] { "E9", 9L });
                        EPAssertionUtil.AssertProps(oldData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(oldData[1], fields, new object[] { "E3", 3L });
                        EPAssertionUtil.AssertProps(oldData[2], fields, new object[] { "E4", 4L });
                        listener.Reset();
                    });
                env.ListenerReset("s0");

                SendSupportBean(env, "E10", 10L);
                SendSupportBean(env, "E10", 11L);
                SendMarketBean(env, "E10");

                SendSupportBean(env, "E21", 21L);
                SendSupportBean(env, "E22", 22L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                SendSupportBean(env, "E23", 23L);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        ClassicAssert.AreEqual(3, listener.LastOldData.Length);
                    });

                env.UndeployAll();
            }
        }

        internal class InfraLengthBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:length_batch(3) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                env.Milestone(2);

                // send event
                SendBeanInt(env, "G1", 10);
                env.AssertListenerNotInvoked("create");

                env.Milestone(3);

                // send event
                SendBeanInt(env, "G2", 20);
                env.AssertListenerNotInvoked("create");

                env.Milestone(4);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G2", 20 } });
                SendMarketBean(env, "G2");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });

                env.Milestone(5);

                // delete event G1
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendMarketBean(env, "G1");
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(6);

                // send event G3
                env.AssertPropsPerRowIterator("create", fields, null);
                SendBeanInt(env, "G3", 30);
                env.AssertListenerNotInvoked("create");

                env.Milestone(7);

                // send event g4
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 30 } });
                SendBeanInt(env, "G4", 40);
                env.AssertListenerNotInvoked("create");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 } });

                env.Milestone(8);

                // delete event G4
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G4", 40 } });
                SendMarketBean(env, "G4");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G3", 30 } });

                env.Milestone(9);

                // send G5
                SendBeanInt(env, "G5", 50);
                env.AssertListenerNotInvoked("create");

                env.Milestone(10);

                // send G6, batch fires
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 30 }, new object[] { "G5", 50 } });
                SendBeanInt(env, "G6", 60);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G3", 30 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "G5", 50 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "G6", 60 });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(11);

                // send G8
                env.AssertPropsPerRowIterator("create", fields, null);
                SendBeanInt(env, "G7", 70);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G7", 70 } });

                env.Milestone(12);

                // Send G8
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G7", 70 } });
                SendBeanInt(env, "G8", 80);
                env.AssertListenerNotInvoked("create");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G7", 70 }, new object[] { "G8", 80 } });

                env.Milestone(13);

                // Delete G7
                SendMarketBean(env, "G7");
                env.AssertListenerNotInvoked("create");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G8", 80 } });

                env.Milestone(14);

                // Send G9
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G8", 80 } });
                SendBeanInt(env, "G9", 90);
                env.AssertListenerNotInvoked("create");

                env.Milestone(15);

                // Send G10
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G8", 80 }, new object[] { "G9", 90 } });
                SendBeanInt(env, "G10", 100);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G8", 80 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "G9", 90 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "G10", 100 });
                        ClassicAssert.AreEqual(3, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G3", 30 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "G5", 50 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[2], fields, new object[] { "G6", 60 });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(16);

                // send g11
                env.AssertPropsPerRowIterator("create", fields, null);
                SendBeanInt(env, "G11", 110);
                SendBeanInt(env, "G12", 120);
                env.AssertListenerNotInvoked("create");

                env.Milestone(17);

                // delete g12
                SendMarketBean(env, "G12");
                env.AssertListenerNotInvoked("create");

                env.Milestone(18);

                // send g13
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G11", 110 } });
                SendBeanInt(env, "G13", 130);
                env.AssertListenerNotInvoked("create");

                // Send G14
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G11", 110 }, new object[] { "G13", 130 } });
                SendBeanInt(env, "G14", 140);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G11", 110 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "G13", 130 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "G14", 140 });
                        ClassicAssert.AreEqual(3, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G8", 80 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "G9", 90 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[2], fields, new object[] { "G10", 100 });
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("create", fields, null);

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
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowSW#sort(3, value asc) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowSW select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select key, value as value from MyWindowSW;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowSW as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("create").AddListener("s0");

                SendSupportBean(env, "E1", 10L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 10L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 10L });

                SendSupportBean(env, "E2", 20L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 20L });

                SendSupportBean(env, "E3", 15L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 15L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E1", 10L }, new object[] { "E3", 15L }, new object[] { "E2", 20L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 20L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 10L }, new object[] { "E3", 15L } });

                SendSupportBean(env, "E4", 18L);
                env.AssertPropsNew("create", fields, new object[] { "E4", 18L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E1", 10L }, new object[] { "E3", 15L }, new object[] { "E4", 18L } });

                SendSupportBean(env, "E5", 17L);
                env.AssertPropsIRPair("create", fields, new object[] { "E5", 17L }, new object[] { "E4", 18L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E1", 10L }, new object[] { "E3", 15L }, new object[] { "E5", 17L } });

                // delete E1
                SendMarketBean(env, "E1");
                env.AssertPropsOld("create", fields, new object[] { "E1", 10L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E3", 15L }, new object[] { "E5", 17L } });

                SendSupportBean(env, "E6", 16L);
                env.AssertPropsNew("create", fields, new object[] { "E6", 16L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E3", 15L }, new object[] { "E6", 16L }, new object[] { "E5", 17L } });

                SendSupportBean(env, "E7", 16L);
                env.AssertPropsIRPair("create", fields, new object[] { "E7", 16L }, new object[] { "E5", 17L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E3", 15L }, new object[] { "E7", 16L }, new object[] { "E6", 16L } });

                // delete E7 has no effect
                SendMarketBean(env, "E7");
                env.AssertPropsOld("create", fields, new object[] { "E7", 16L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E3", 15L }, new object[] { "E6", 16L } });

                SendSupportBean(env, "E8", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E8", 1L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E8", 1L }, new object[] { "E3", 15L }, new object[] { "E6", 16L } });

                SendSupportBean(env, "E9", 1L);
                env.AssertPropsIRPair("create", fields, new object[] { "E9", 1L }, new object[] { "E6", 16L });

                env.UndeployAll();
            }
        }

        internal class InfraSortWindowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.ext:sort(3, value) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event G1
                SendBeanInt(env, "G1", 10);
                env.AssertPropsNew("create", fields, new object[] { "G1", 10 });

                env.Milestone(0);

                // send event G2
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G2", 9);
                env.AssertPropsNew("create", fields, new object[] { "G2", 9 });
                env.ListenerReset("create");

                env.Milestone(1);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G2", 9 }, new object[] { "G1", 10 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 9 });

                env.Milestone(2);

                // send g3
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G3", 3);
                env.AssertPropsNew("create", fields, new object[] { "G3", 3 });

                env.Milestone(3);

                // send g4
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G1", 10 } });
                SendBeanInt(env, "G4", 4);
                env.AssertPropsNew("create", fields, new object[] { "G4", 4 });

                env.Milestone(4);

                // send g5
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 }, new object[] { "G1", 10 } });
                SendBeanInt(env, "G5", 5);
                env.AssertPropsIRPair("create", fields, new object[] { "G5", 5 }, new object[] { "G1", 10 });

                env.Milestone(5);

                // send g6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 }, new object[] { "G5", 5 } });
                SendBeanInt(env, "G6", 6);
                env.AssertPropsIRPair("create", fields, new object[] { "G6", 6 }, new object[] { "G6", 6 });

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
                var fields = new string[] { "key", "value" };

                var epl =
                    "@name('create') create window MyWindowTLB#time_length_batch(10 sec, 3) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTLB select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@name('s0') select key, value as value from MyWindowTLB;\n" +
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowTLB as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("delete").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 1L);
                SendSupportBean(env, "E2", 2L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 2L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendSupportBean(env, "E3", 3L);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 3L } });

                SendSupportBean(env, "E4", 4L);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.IsNull(listener.LastOldData);
                        var newData = listener.LastNewData;
                        ClassicAssert.AreEqual(3, newData.Length);
                        EPAssertionUtil.AssertProps(newData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(newData[1], fields, new object[] { "E3", 3L });
                        EPAssertionUtil.AssertProps(newData[2], fields, new object[] { "E4", 4L });
                        listener.Reset();
                    });
                env.ListenerReset("s0");
                env.AssertPropsPerRowIterator("create", fields, null);

                SendTimer(env, 5000);
                SendSupportBean(env, "E5", 5L);
                SendSupportBean(env, "E6", 6L);
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E5", 5L }, new object[] { "E6", 6L } });

                SendMarketBean(env, "E5"); // deleting E5
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E6", 6L } });

                SendTimer(env, 10999);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("s0");

                SendTimer(env, 11000);
                env.AssertListener(
                    "create",
                    listener => {
                        var newData = listener.LastNewData;
                        ClassicAssert.AreEqual(1, newData.Length);
                        EPAssertionUtil.AssertProps(newData[0], fields, new object[] { "E6", 6L });
                        var oldData = listener.LastOldData;
                        ClassicAssert.AreEqual(3, oldData.Length);
                        EPAssertionUtil.AssertProps(oldData[0], fields, new object[] { "E1", 1L });
                        EPAssertionUtil.AssertProps(oldData[1], fields, new object[] { "E3", 3L });
                        EPAssertionUtil.AssertProps(oldData[2], fields, new object[] { "E4", 4L });
                        env.ListenerReset("create");
                    });
                env.ListenerReset("s0");

                env.UndeployAll();
            }
        }

        internal class InfraTimeLengthBatchSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.win:time_length_batch(10 sec, 4) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event
                env.AdvanceTime(1000);
                SendBeanInt(env, "G1", 1);
                env.AssertListenerNotInvoked("create");

                env.Milestone(0);

                // send event
                env.AdvanceTime(5000);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                SendBeanInt(env, "G2", 2);
                env.AssertListenerNotInvoked("create");

                env.Milestone(1);

                // delete event G2
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1 }, new object[] { "G2", 2 } });
                SendMarketBean(env, "G2");
                env.AssertListenerNotInvoked("create");

                env.Milestone(2);

                // delete event G1
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                SendMarketBean(env, "G1");
                env.AssertListenerNotInvoked("create");

                env.Milestone(3);

                env.AssertPropsPerRowIterator("create", fields, null);
                env.AdvanceTime(11000);
                env.AssertListenerNotInvoked("create");

                env.Milestone(4);

                // Send g3, g4 and g5
                env.AdvanceTime(15000);
                SendBeanInt(env, "G3", 3);
                SendBeanInt(env, "G4", 4);

                env.Milestone(5);

                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 } });
                env.AdvanceTime(16000);
                SendBeanInt(env, "G5", 5);

                env.Milestone(6);

                // Delete g5
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 }, new object[] { "G5", 5 } });
                SendMarketBean(env, "G5");
                env.AssertListenerNotInvoked("create");

                env.Milestone(7);

                // send g6
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G3", 3 }, new object[] { "G4", 4 } });
                env.AdvanceTime(18000);
                SendBeanInt(env, "G6", 6);
                env.AssertListenerNotInvoked("create");

                env.Milestone(8);

                // flush batch
                env.AdvanceTime(24999);
                env.AssertListenerNotInvoked("create");
                env.AdvanceTime(25000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(3, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G3", 3 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "G4", 4 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "G6", 6 });
                        ClassicAssert.IsNull(listener.LastOldData);
                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(9);

                // send g7, g8 and g9
                env.AdvanceTime(28000);
                SendBeanInt(env, "G7", 7);
                SendBeanInt(env, "G8", 8);
                SendBeanInt(env, "G9", 9);

                env.Milestone(10);

                // delete g7 and g9
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "G7", 7 }, new object[] { "G8", 8 }, new object[] { "G9", 9 } });
                SendMarketBean(env, "G7");
                SendMarketBean(env, "G9");
                env.AssertListenerNotInvoked("create");

                env.Milestone(11);

                // flush
                env.AdvanceTime(34999);
                env.AssertListenerNotInvoked("create");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G8", 8 } });
                env.AdvanceTime(35000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(1, listener.LastNewData.Length);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "G8", 8 });
                        ClassicAssert.AreEqual(3, listener.LastOldData.Length);
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "G3", 3 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "G4", 4 });
                        EPAssertionUtil.AssertProps(listener.LastOldData[2], fields, new object[] { "G6", 6 });
                    });
                env.AssertPropsPerRowIterator("create", fields, null);

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

        internal class InfraLengthWindowSceneThree : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "TheString".SplitCsv();
                var path = new RegressionPath();
                env.CompileDeploy("@public create window ABCWin#length(2) as SupportBean", path);
                env.CompileDeploy("insert into ABCWin select * from SupportBean", path);
                env.CompileDeploy("on SupportBean_A delete from ABCWin where TheString = Id", path);
                env.CompileDeploy("@name('s0') select irstream * from ABCWin", path).AddListener("s0");

                env.Milestone(0);
                env.AssertPropsPerRowIterator("s0", fields, null);

                SendSupportBean(env, "E1");
                env.AssertPropsNew("s0", fields, new object[] { "E1" });

                env.Milestone(1);

                SendSupportBean_A(env, "E1");
                env.AssertPropsOld("s0", fields, new object[] { "E1" });

                env.Milestone(2);

                env.AssertPropsPerRowIterator("s0", fields, Array.Empty<object[]>());
                SendSupportBean(env, "E2");
                env.AssertPropsNew("s0", fields, new object[] { "E2" });
                SendSupportBean(env, "E3");
                env.AssertPropsNew("s0", fields, new object[] { "E3" });

                env.Milestone(3);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E2" }, new object[] { "E3" } });

                SendSupportBean_A(env, "E3");
                env.AssertPropsOld("s0", fields, new object[] { "E3" });

                env.Milestone(4);

                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E2" } });

                SendSupportBean(env, "E4");
                env.AssertPropsNew("s0", fields, new object[] { "E4" });
                SendSupportBean(env, "E5");
                env.AssertPropsIRPair("s0", fields, new object[] { "E5" }, new object[] { "E2" });

                env.UndeployAll();
            }
        }

        internal class InfraLengthWindowPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl =
                    "@name('create') create window MyWindowWPG#groupwin(value)#length(2) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowWPG select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@name('s0') select irstream key, value as value from MyWindowWPG;\n" +
                    "@name('delete') on SupportMarketDataBean delete from MyWindowWPG where Symbol = key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E1", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });

                SendSupportBean(env, "E2", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E2", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E2", 1L });

                SendSupportBean(env, "E3", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E3", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E3", 2L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E2", 1L }, new object[] { "E3", 2L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("create", fields, new object[] { "E2", 1L });
                env.AssertPropsOld("s0", fields, new object[] { "E2", 1L });
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1L }, new object[] { "E3", 2L } });

                SendSupportBean(env, "E4", 1L);
                env.AssertPropsNew("create", fields, new object[] { "E4", 1L });
                env.AssertPropsNew("s0", fields, new object[] { "E4", 1L });

                SendSupportBean(env, "E5", 1L);
                env.AssertListener(
                    "create",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E5", 1L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E1", 1L });
                        listener.Reset();
                    });
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E5", 1L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E1", 1L });
                        listener.Reset();
                    });

                SendSupportBean(env, "E6", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E6", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E6", 2L });

                // delete E6
                SendMarketBean(env, "E6");
                env.AssertPropsOld("create", fields, new object[] { "E6", 2L });
                env.AssertPropsOld("s0", fields, new object[] { "E6", 2L });

                SendSupportBean(env, "E7", 2L);
                env.AssertPropsNew("create", fields, new object[] { "E7", 2L });
                env.AssertPropsNew("s0", fields, new object[] { "E7", 2L });

                SendSupportBean(env, "E8", 2L);
                env.AssertListener(
                    "create",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E8", 2L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E3", 2L });
                        listener.Reset();
                    });
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E8", 2L });
                        EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E3", 2L });
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        internal class InfraTimeBatchPerGroup : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                SendTimer(env, 0);
                var epl =
                    "@name('create') create window MyWindowTBPG#groupwin(value)#time_batch(10 sec) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowTBPG select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@name('s0') select key, value as value from MyWindowTBPG;\n";
                env.CompileDeploy(epl).AddListener("s0").AddListener("create");

                SendTimer(env, 1000);
                SendSupportBean(env, "E1", 10L);
                SendSupportBean(env, "E2", 20L);
                SendSupportBean(env, "E3", 20L);
                SendSupportBean(env, "E4", 10L);

                SendTimer(env, 11000);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(listener.LastNewData.Length, 4);
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E1", 10L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "E4", 10L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "E2", 20L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[3], fields, new object[] { "E3", 20L });
                        listener.Reset();
                    });
                env.AssertListener(
                    "s0",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E1", 10L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "E4", 10L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[2], fields, new object[] { "E2", 20L });
                        EPAssertionUtil.AssertProps(listener.LastNewData[3], fields, new object[] { "E3", 20L });
                        listener.Reset();
                    });

                env.UndeployAll();
            }
        }

        internal class InfraDoubleInsertSameWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowDISM#keepall as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowDISM select TheString as key, LongBoxed+1 as value from SupportBean;\n" +
                          "insert into MyWindowDISM select TheString as key, LongBoxed+2 as value from SupportBean;\n" +
                          "@name('s0') select key, value as value from MyWindowDISM";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0");

                SendSupportBean(env, "E1", 10L);
                env.AssertListener(
                    "create",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.NewDataList.Count); // listener to window gets 2 individual events
                        ClassicAssert.AreEqual(2, listener.NewDataList.Count); // listener to statement gets 1 individual event
                        ClassicAssert.AreEqual(2, listener.NewDataListFlattened.Length);
                        ClassicAssert.AreEqual(2, listener.NewDataListFlattened.Length);
                    });
                env.AssertPropsPerRowNewFlattened(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 11L }, new object[] { "E1", 12L } });

                env.UndeployAll();
            }
        }

        internal class InfraLastEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowLE#lastevent as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowLE select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowLE;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowLE as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendSupportBean(env, "E2", 2L);
                env.AssertPropsIRPair("s0", fields, new object[] { "E2", 2L }, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E2", 2L } });

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertPropsOld("s0", fields, new object[] { "E2", 2L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E3", 3L);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E3", 3L } });

                // delete E3
                SendMarketBean(env, "E3");
                env.AssertPropsOld("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E4", 4L);
                env.AssertPropsNew("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E4", 4L } });

                // delete other event
                SendMarketBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraLastEventSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow.std:lastevent() as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // send event
                SendBeanInt(env, "G1", 1);
                env.AssertPropsNew("create", fields, new object[] { "G1", 1 });

                env.Milestone(0);

                // send event
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1 } });
                SendBeanInt(env, "G2", 2);
                env.AssertPropsIRPair("create", fields, new object[] { "G2", 2 }, new object[] { "G1", 1 });

                env.Milestone(1);

                // delete event G2
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G2", 2 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 2 });

                env.Milestone(2);

                env.AssertIterator("create", iterator => ClassicAssert.AreEqual(0, EPAssertionUtil.EnumeratorCount(iterator)));
                SendBeanInt(env, "G3", 3);
                env.AssertPropsNew("create", fields, new object[] { "G3", 3 });

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
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowFE#firstevent as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowFE select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowFE;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowFE as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                SendSupportBean(env, "E2", 2L);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1L } });

                // delete E2
                SendMarketBean(env, "E1");
                env.AssertPropsOld("s0", fields, new object[] { "E1", 1L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E3", 3L);
                env.AssertPropsNew("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E3", 3L } });

                // delete E3
                SendMarketBean(env, "E2"); // no effect
                SendMarketBean(env, "E3");
                env.AssertPropsOld("s0", fields, new object[] { "E3", 3L });
                env.AssertPropsPerRowIterator("create", fields, null);

                SendSupportBean(env, "E4", 4L);
                env.AssertPropsNew("s0", fields, new object[] { "E4", 4L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E4", 4L } });

                // delete other event
                SendMarketBean(env, "E1");
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraUnique : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowUN#unique(key) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowUN select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowUN;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowUN as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("s0").AddListener("create");

                SendSupportBean(env, "G1", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "G1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1L } });

                SendSupportBean(env, "G2", 20L);
                env.AssertPropsNew("s0", fields, new object[] { "G2", 20L });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1L }, new object[] { "G2", 20L } });

                // delete G2
                SendMarketBean(env, "G2");
                env.AssertPropsOld("s0", fields, new object[] { "G2", 20L });

                SendSupportBean(env, "G1", 2L);
                env.AssertPropsIRPair("s0", fields, new object[] { "G1", 2L }, new object[] { "G1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 2L } });

                SendSupportBean(env, "G2", 21L);
                env.AssertPropsNew("s0", fields, new object[] { "G2", 21L });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 2L }, new object[] { "G2", 21L } });

                SendSupportBean(env, "G2", 22L);
                env.AssertPropsIRPair("s0", fields, new object[] { "G2", 22L }, new object[] { "G2", 21L });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 2L }, new object[] { "G2", 22L } });

                SendMarketBean(env, "G1");
                env.AssertPropsOld("s0", fields, new object[] { "G1", 2L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G2", 22L } });

                env.UndeployAll();
            }
        }

        internal class InfraUniqueSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var path = new RegressionPath();

                // create window
                var stmtTextCreate =
                    "@name('create') @public create window MyWindow#unique(key) as select TheString as key, IntBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.Milestone(0);

                // create insert into
                var stmtTextInsert =
                    "@name('insert') insert into MyWindow(key, value) select irstream TheString, IntBoxed from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                env.Milestone(1);

                // create consumer
                var stmtTextSelectOne = "@name('consume') select irstream key, value as value from MyWindow";
                env.CompileDeploy(stmtTextSelectOne, path);

                env.Milestone(2);

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindow as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path);

                env.Milestone(3);

                // send event
                SendBeanInt(env, "G1", 10);
                env.AssertPropsNew("create", fields, new object[] { "G1", 10 });

                env.Milestone(4);

                // send event
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "G1", 10 } });
                SendBeanInt(env, "G2", 20);
                env.AssertPropsNew("create", fields, new object[] { "G2", 20 });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G2", 20 } });

                env.Milestone(5);

                // delete event G2
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 10 }, new object[] { "G2", 20 } });
                SendMarketBean(env, "G1");
                env.AssertPropsOld("create", fields, new object[] { "G1", 10 });
                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "G2", 20 } });

                env.Milestone(6);

                env.AssertPropsPerRowIteratorAnyOrder("create", fields, new object[][] { new object[] { "G2", 20 } });
                SendMarketBean(env, "G2");
                env.AssertPropsOld("create", fields, new object[] { "G2", 20 });

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
                var fields = new string[] { "key", "value" };

                var epl = "@name('create') create window MyWindowFU#firstunique(key) as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowFU select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select irstream key, value as value from MyWindowFU;\n" +
                          "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowFU as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("delete");

                SendSupportBean(env, "G1", 1L);
                env.AssertPropsNew("s0", fields, new object[] { "G1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1L } });

                SendSupportBean(env, "G2", 20L);
                env.AssertPropsNew("s0", fields, new object[] { "G2", 20L });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1L }, new object[] { "G2", 20L } });

                // delete G2
                SendMarketBean(env, "G2");
                env.AssertPropsOld("s0", fields, new object[] { "G2", 20L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1L } });

                SendSupportBean(env, "G1", 2L); // ignored
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G1", 1L } });

                SendSupportBean(env, "G2", 21L);
                env.AssertPropsNew("s0", fields, new object[] { "G2", 21L });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1L }, new object[] { "G2", 21L } });

                SendSupportBean(env, "G2", 22L); // ignored
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 1L }, new object[] { "G2", 21L } });

                SendMarketBean(env, "G1");
                env.AssertPropsOld("s0", fields, new object[] { "G1", 1L });
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "G2", 21L } });

                env.UndeployAll();
            }
        }

        internal class InfraFilteringConsumer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl =
                    "@name('create') create window MyWindowFC#unique(key) as select TheString as key, IntPrimitive as value from SupportBean;\n" +
                    "insert into MyWindowFC select TheString as key, IntPrimitive as value from SupportBean;\n" +
                    "@name('s0') select irstream key, value as value from MyWindowFC(value > 0, value < 10);\n" +
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowFC as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("delete");

                SendSupportBeanInt(env, "G1", 5);
                env.AssertPropsNew("s0", fields, new object[] { "G1", 5 });
                env.AssertPropsNew("create", fields, new object[] { "G1", 5 });

                SendSupportBeanInt(env, "G1", 15);
                env.AssertPropsOld("s0", fields, new object[] { "G1", 5 });
                env.AssertPropsIRPair("create", fields, new object[] { "G1", 15 }, new object[] { "G1", 5 });

                // send G2
                SendSupportBeanInt(env, "G2", 8);
                env.AssertPropsNew("s0", fields, new object[] { "G2", 8 });
                env.AssertPropsNew("create", fields, new object[] { "G2", 8 });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 15 }, new object[] { "G2", 8 } });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "G2", 8 } });

                // delete G2
                SendMarketBean(env, "G2");
                env.AssertPropsOld("s0", fields, new object[] { "G2", 8 });
                env.AssertPropsOld("create", fields, new object[] { "G2", 8 });

                // send G3
                SendSupportBeanInt(env, "G3", -1);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsNew("create", fields, new object[] { "G3", -1 });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "G1", 15 }, new object[] { "G3", -1 } });
                env.AssertPropsPerRowIterator("s0", fields, null);

                // delete G2
                SendMarketBean(env, "G3");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsOld("create", fields, new object[] { "G3", -1 });

                SendSupportBeanInt(env, "G1", 6);
                SendSupportBeanInt(env, "G2", 7);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    fields,
                    new object[][] { new object[] { "G1", 6 }, new object[] { "G2", 7 } });

                env.UndeployAll();
            }
        }

        internal class InfraSelectGroupedViewLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var epl =
                    "@name('create') @public create window MyWindowSGVS#groupwin(TheString, IntPrimitive)#length(9) as select TheString, IntPrimitive from SupportBean;\n" +
                    "@name('insert') insert into MyWindowSGVS select TheString, IntPrimitive from SupportBean;\n";
                env.CompileDeploy(epl, path);

                // fill window
                var stringValues = new string[] { "c0", "c1", "c2" };
                for (var i = 0; i < stringValues.Length; i++) {
                    for (var j = 0; j < 3; j++) {
                        env.SendEventBean(new SupportBean(stringValues[i], j));
                    }
                }

                env.SendEventBean(new SupportBean("c0", 1));
                env.SendEventBean(new SupportBean("c1", 2));
                env.SendEventBean(new SupportBean("c3", 3));
                env.AssertIterator(
                    "create",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        ClassicAssert.AreEqual(12, received.Length);
                    });

                // create select stmt
                var stmtTextSelect =
                    "@name('s0') select TheString, IntPrimitive, count(*) from MyWindowSGVS group by TheString, IntPrimitive order by TheString, IntPrimitive";
                env.CompileDeploy(stmtTextSelect, path);
                env.AssertIterator(
                    "s0",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        ClassicAssert.AreEqual(10, received.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "TheString,IntPrimitive,count(*)".SplitCsv(),
                            new object[][] {
                                new object[] { "c0", 0, 1L },
                                new object[] { "c0", 1, 2L },
                                new object[] { "c0", 2, 1L },
                                new object[] { "c1", 0, 1L },
                                new object[] { "c1", 1, 1L },
                                new object[] { "c1", 2, 2L },
                                new object[] { "c2", 0, 1L },
                                new object[] { "c2", 1, 1L },
                                new object[] { "c2", 2, 1L },
                                new object[] { "c3", 3, 1L },
                            });
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
                    "@name('create') @public create window MyWindowSGVLS#groupwin(TheString, IntPrimitive)#length(9) as select TheString, IntPrimitive, LongPrimitive, BoolPrimitive from SupportBean";
                env.CompileDeploy(stmtTextCreate, path);

                // create insert into
                var stmtTextInsert =
                    "insert into MyWindowSGVLS select TheString, IntPrimitive, LongPrimitive, BoolPrimitive from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // create variable
                env.CompileDeploy("@public create variable string var_1_1_1", path);
                env.CompileDeploy("on SupportVariableSetEvent(VariableName='var_1_1_1') set var_1_1_1 = Value", path);

                // fill window
                var stringValues = new string[] { "c0", "c1", "c2" };
                for (var i = 0; i < stringValues.Length; i++) {
                    for (var j = 0; j < 3; j++) {
                        var beanX = new SupportBean(stringValues[i], j);
                        beanX.LongPrimitive = j;
                        beanX.BoolPrimitive = true;
                        env.SendEventBean(beanX);
                    }
                }

                // extra record to create non-uniform data
                var bean = new SupportBean("c1", 1);
                bean.LongPrimitive = 10;
                bean.BoolPrimitive = true;
                env.SendEventBean(bean);
                env.AssertIterator(
                    "create",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        ClassicAssert.AreEqual(10, received.Length);
                    });

                // create select stmt
                var stmtTextSelect =
                    "@name('s0') select TheString, IntPrimitive, avg(LongPrimitive) as avgLong, count(BoolPrimitive) as cntBool" +
                    " from MyWindowSGVLS group by TheString, IntPrimitive having TheString = var_1_1_1 order by TheString, IntPrimitive";
                env.CompileDeploy(stmtTextSelect, path);

                // set variable to C0
                env.SendEventBean(new SupportVariableSetEvent("var_1_1_1", "c0"));

                // get iterator results
                env.AssertIterator(
                    "s0",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        ClassicAssert.AreEqual(3, received.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "TheString,IntPrimitive,avgLong,cntBool".SplitCsv(),
                            new object[][] {
                                new object[] { "c0", 0, 0.0, 1L },
                                new object[] { "c0", 1, 1.0, 1L },
                                new object[] { "c0", 2, 2.0, 1L },
                            });
                    });

                // set variable to C1
                env.SendEventBean(new SupportVariableSetEvent("var_1_1_1", "c1"));

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var received = EPAssertionUtil.EnumeratorToArray(iterator);
                        ClassicAssert.AreEqual(3, received.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            received,
                            "TheString,IntPrimitive,avgLong,cntBool".SplitCsv(),
                            new object[][] {
                                new object[] { "c1", 0, 0.0, 1L },
                                new object[] { "c1", 1, 5.5, 2L },
                                new object[] { "c1", 2, 2.0, 1L },
                            });
                    });

                env.UndeployAll();
            }
        }

        internal class InfraFilteringConsumerLateStart : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "sumvalue" };
                var path = new RegressionPath();

                var epl =
                    "@name('create') @public create window MyWindowFCLS#keepall as select TheString as key, IntPrimitive as value from SupportBean;\n" +
                    "insert into MyWindowFCLS select TheString as key, IntPrimitive as value from SupportBean;\n";
                env.CompileDeploy(epl, path);

                SendSupportBeanInt(env, "G1", 5);
                SendSupportBeanInt(env, "G2", 15);
                SendSupportBeanInt(env, "G3", 2);

                // create consumer
                var stmtTextSelectOne =
                    "@name('s0') select irstream sum(value) as sumvalue from MyWindowFCLS(value > 0, value < 10)";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 7 } });

                SendSupportBeanInt(env, "G4", 1);
                env.AssertPropsIRPair("s0", fields, new object[] { 8 }, new object[] { 7 });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 8 } });

                SendSupportBeanInt(env, "G5", 20);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 8 } });

                SendSupportBeanInt(env, "G6", 9);
                env.AssertPropsIRPair("s0", fields, new object[] { 17 }, new object[] { 8 });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 17 } });

                // create delete stmt
                var stmtTextDelete =
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowFCLS as s1 where s0.Symbol = s1.key";
                env.CompileDeploy(stmtTextDelete, path);

                SendMarketBean(env, "G4");
                env.AssertPropsIRPair("s0", fields, new object[] { 16 }, new object[] { 17 });
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 16 } });

                SendMarketBean(env, "G5");
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { 16 } });

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("delete");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "create window MyWindowI1#groupwin(value)#uni(value) as MySimpleKeyValueMap",
                    "Named windows require one or more child views that are data window views [create window MyWindowI1#groupwin(value)#uni(value) as MySimpleKeyValueMap]");

                env.TryInvalidCompile(
                    "create window MyWindowI2 as MySimpleKeyValueMap",
                    "Named windows require one or more child views that are data window views [create window MyWindowI2 as MySimpleKeyValueMap]");

                env.TryInvalidCompile(
                    "on MySimpleKeyValueMap delete from dummy",
                    "A named window or table 'dummy' has not been declared [on MySimpleKeyValueMap delete from dummy]");

                var path = new RegressionPath();
                env.CompileDeploy("@public create window SomeWindow#keepall as (a int)", path);
                env.TryInvalidCompile(
                    path,
                    "update SomeWindow set a = 'a' where a = 'b'",
                    "Provided EPL expression is an on-demand query expression (not a continuous query)");
                TryInvalidFAFCompile(
                    env,
                    path,
                    "update istream SomeWindow set a = 'a' where a = 'b'",
                    "Provided EPL expression is a continuous query expression (not an on-demand query)");

                // test model-after with no field
                env.TryInvalidCompile(
                    "create window MyWindowI3#keepall as select innermap.abc from OuterMap",
                    "Failed to validate select-clause expression 'innermap.abc': Failed to resolve property 'innermap.abc' to a stream or nested property in a stream");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        internal class InfraNamedWindowInvalidAlreadyExists : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile(
                    "create window MyWindowAE#keepall as MySimpleKeyValueMap",
                    compilerOptions => compilerOptions
                        .SetAccessModifierNamedWindow(ctx => NameAccessModifier.PUBLIC));
                env.Deploy(compiled);
                TryInvalidDeploy(
                    env,
                    compiled,
                    "A precondition is not satisfied: A named window by name 'MyWindowAE' has already been created for module '(unnamed)'");
                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }

        internal class InfraNamedWindowInvalidConsumerDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create window MyWindowCDW#keepall as MySimpleKeyValueMap", path);
                env.TryInvalidCompile(
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
                var fieldsPrior = new string[] { "priorKeyOne", "priorKeyTwo" };
                var fieldsStat = new string[] { "average" };

                var epl = "@name('create') create window MyWindowPS#keepall as MySimpleKeyValueMap;\n" +
                          "insert into MyWindowPS select TheString as key, LongBoxed as value from SupportBean;\n" +
                          "@name('s0') select prior(1, key) as priorKeyOne, prior(2, key) as priorKeyTwo from MyWindowPS;\n" +
                          "@name('s3') select average from MyWindowPS#uni(value);\n";
                env.CompileDeploy(epl).AddListener("create").AddListener("s0").AddListener("s3");

                env.AssertStatement(
                    "create",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("key"));
                        ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("value"));
                    });

                // send events
                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("s0", fieldsPrior, new object[] { null, null });
                env.AssertPropsNew("s3", fieldsStat, new object[] { 1d });
                env.AssertPropsPerRowIterator("s3", fieldsStat, new object[][] { new object[] { 1d } });

                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "E1", null });
                env.AssertPropsNew("s3", fieldsStat, new object[] { 1.5d });
                env.AssertPropsPerRowIterator("s3", fieldsStat, new object[][] { new object[] { 1.5d } });

                SendSupportBean(env, "E3", 2L);
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "E2", "E1" });
                env.AssertPropsNew("s3", fieldsStat, new object[] { 5 / 3d });
                env.AssertPropsPerRowIterator("s3", fieldsStat, new object[][] { new object[] { 5 / 3d } });

                SendSupportBean(env, "E4", 2L);
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "E3", "E2" });
                env.AssertPropsNew("s3", fieldsStat, new object[] { 1.75 });
                env.AssertPropsPerRowIterator("s3", fieldsStat, new object[][] { new object[] { 1.75d } });

                env.UndeployAll();
            }
        }

        internal class InfraLateConsumer : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsWin = new string[] { "key", "value" };
                var fieldsStat = new string[] { "average" };
                var fieldsCnt = new string[] { "cnt" };
                var path = new RegressionPath();

                var stmtTextCreate = "@name('create') @public create window MyWindowLCL#keepall as MySimpleKeyValueMap";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.AssertStatement(
                    "create",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("key"));
                        ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("value"));
                    });

                var stmtTextInsert =
                    "insert into MyWindowLCL select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // send events
                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fieldsWin, new object[] { "E1", 1L });

                SendSupportBean(env, "E2", 2L);
                env.AssertPropsNew("create", fieldsWin, new object[] { "E2", 2L });

                var stmtTextSelectOne = "@name('s0') select irstream average from MyWindowLCL#uni(value)";
                env.CompileDeploy(stmtTextSelectOne, path).AddListener("s0");
                env.AssertPropsPerRowIterator("s0", fieldsStat, new object[][] { new object[] { 1.5d } });

                SendSupportBean(env, "E3", 2L);
                env.AssertPropsIRPair("s0", fieldsStat, new object[] { 5 / 3d }, new object[] { 3 / 2d });
                env.AssertPropsPerRowIterator("s0", fieldsStat, new object[][] { new object[] { 5 / 3d } });

                SendSupportBean(env, "E4", 2L);
                env.AssertPropsPerRowLastNew("s0", fieldsStat, new object[][] { new object[] { 7 / 4d } });
                env.AssertPropsPerRowIterator("s0", fieldsStat, new object[][] { new object[] { 7 / 4d } });

                var stmtTextSelectTwo = "@name('s2') select count(*) as cnt from MyWindowLCL";
                env.CompileDeploy(stmtTextSelectTwo, path);
                env.AssertPropsPerRowIterator("s2", fieldsCnt, new object[][] { new object[] { 4L } });
                env.AssertPropsPerRowIterator("s0", fieldsStat, new object[][] { new object[] { 7 / 4d } });

                SendSupportBean(env, "E5", 3L);
                env.AssertPropsIRPair("s0", fieldsStat, new object[] { 10 / 5d }, new object[] { 7 / 4d });
                env.AssertPropsPerRowIterator("s0", fieldsStat, new object[][] { new object[] { 10 / 5d } });
                env.AssertPropsPerRowIterator("s2", fieldsCnt, new object[][] { new object[] { 5L } });

                env.UndeployAll();
            }
        }

        internal class InfraLateConsumerJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fieldsWin = new string[] { "key", "value" };
                var fieldsJoin = new string[] { "key", "value", "Symbol" };
                var path = new RegressionPath();

                var stmtTextCreate = "@name('create') @public create window MyWindowLCJ#keepall as MySimpleKeyValueMap";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                env.AssertStatement(
                    "create",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("key"));
                        ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("value"));
                    });

                var stmtTextInsert =
                    "insert into MyWindowLCJ select TheString as key, LongBoxed as value from SupportBean";
                env.CompileDeploy(stmtTextInsert, path);

                // send events
                SendSupportBean(env, "E1", 1L);
                env.AssertPropsNew("create", fieldsWin, new object[] { "E1", 1L });

                SendSupportBean(env, "E2", 1L);
                env.AssertPropsNew("create", fieldsWin, new object[] { "E2", 1L });

                // This replays into MyWindow
                var stmtTextSelectTwo = "@name('s2') select key, value, Symbol from MyWindowLCJ as s0" +
                                        " left outer join SupportMarketDataBean#keepall as s1" +
                                        " on s0.value = s1.Volume";
                env.CompileDeploy(stmtTextSelectTwo, path).AddListener("s2");
                env.AssertListenerNotInvoked("s2");
                env.AssertPropsPerRowIterator(
                    "s2",
                    fieldsJoin,
                    new object[][] { new object[] { "E1", 1L, null }, new object[] { "E2", 1L, null } });

                SendMarketBean(env, "S1", 1); // join on long
                env.AssertListener(
                    "s2",
                    listener => {
                        ClassicAssert.AreEqual(2, listener.LastNewData.Length);
                        if (listener.LastNewData[0].Get("key").Equals("E1")) {
                            EPAssertionUtil.AssertProps(
                                listener.LastNewData[0],
                                fieldsJoin,
                                new object[] { "E1", 1L, "S1" });
                            EPAssertionUtil.AssertProps(
                                listener.LastNewData[1],
                                fieldsJoin,
                                new object[] { "E2", 1L, "S1" });
                        }
                        else {
                            EPAssertionUtil.AssertProps(
                                listener.LastNewData[0],
                                fieldsJoin,
                                new object[] { "E2", 1L, "S1" });
                            EPAssertionUtil.AssertProps(
                                listener.LastNewData[1],
                                fieldsJoin,
                                new object[] { "E1", 1L, "S1" });
                        }

                        listener.Reset();
                    });
                env.AssertPropsPerRowIterator(
                    "s2",
                    fieldsJoin,
                    new object[][] { new object[] { "E1", 1L, "S1" }, new object[] { "E2", 1L, "S1" } });

                SendMarketBean(env, "S2", 2); // join on long
                env.AssertListenerNotInvoked("s2");
                env.AssertPropsPerRowIterator(
                    "s2",
                    fieldsJoin,
                    new object[][] { new object[] { "E1", 1L, "S1" }, new object[] { "E2", 1L, "S1" } });

                SendSupportBean(env, "E3", 2L);
                env.AssertPropsNew("create", fieldsWin, new object[] { "E3", 2L });
                env.AssertPropsNew("s2", fieldsJoin, new object[] { "E3", 2L, "S2" });
                env.AssertPropsPerRowIterator(
                    "s2",
                    fieldsJoin,
                    new object[][] {
                        new object[] { "E1", 1L, "S1" }, new object[] { "E2", 1L, "S1" },
                        new object[] { "E3", 2L, "S2" }
                    });

                env.UndeployAll();
            }
        }

        internal class InfraPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };
                var epl = "@name('create') create window MyWindowPAT#keepall as MySimpleKeyValueMap;\n" +
                          "@name('s0') select a.key as key, a.value as value from pattern [every a=MyWindowPAT(key='S1') or a=MyWindowPAT(key='S2')];\n" +
                          "insert into MyWindowPAT select TheString as key, LongBoxed as value from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSupportBean(env, "E1", 1L);
                env.AssertListenerNotInvoked("s0");

                SendSupportBean(env, "S1", 2L);
                env.AssertPropsNew("s0", fields, new object[] { "S1", 2L });

                SendSupportBean(env, "S1", 3L);
                env.AssertPropsNew("s0", fields, new object[] { "S1", 3L });

                SendSupportBean(env, "S2", 4L);
                env.AssertPropsNew("s0", fields, new object[] { "S2", 4L });

                SendSupportBean(env, "S1", 1L);
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class InfraExternallyTimedBatch : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new string[] { "key", "value" };

                var epl =
                    "@name('create') create window MyWindowETB#ext_timed_batch(value, 10 sec, 0L) as MySimpleKeyValueMap;\n" +
                    "insert into MyWindowETB select TheString as key, LongBoxed as value from SupportBean;\n" +
                    "@name('s0') select irstream key, value as value from MyWindowETB;\n" +
                    "@name('delete') on SupportMarketDataBean as s0 delete from MyWindowETB as s1 where s0.Symbol = s1.key;\n";
                env.CompileDeploy(epl).AddListener("delete").AddListener("create").AddListener("s0");

                env.Milestone(0);

                SendSupportBean(env, "E1", 1000L);
                SendSupportBean(env, "E2", 8000L);

                env.Milestone(1);

                SendSupportBean(env, "E3", 9999L);
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][]
                        { new object[] { "E1", 1000L }, new object[] { "E2", 8000L }, new object[] { "E3", 9999L } });

                env.Milestone(2);

                // delete E2
                SendMarketBean(env, "E2");
                env.AssertListener(
                    "create",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.AssertInvokedAndReset(),
                        fields,
                        null,
                        new object[][] { new object[] { "E2", 8000L } }));
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.AssertInvokedAndReset(),
                        fields,
                        null,
                        new object[][] { new object[] { "E2", 8000L } }));
                env.AssertPropsPerRowIterator(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1000L }, new object[] { "E3", 9999L } });

                env.Milestone(3);

                SendSupportBean(env, "E4", 10000L);
                env.AssertListener(
                    "create",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.AssertInvokedAndReset(),
                        fields,
                        new object[][] { new object[] { "E1", 1000L }, new object[] { "E3", 9999L } },
                        null));
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.AssertInvokedAndReset(),
                        fields,
                        new object[][] { new object[] { "E1", 1000L }, new object[] { "E3", 9999L } },
                        null));
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E4", 10000L } });

                env.Milestone(4);

                // delete E4
                SendMarketBean(env, "E4");
                env.AssertListener(
                    "create",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.AssertInvokedAndReset(),
                        fields,
                        null,
                        new object[][] { new object[] { "E4", 10000L } }));
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.AssertInvokedAndReset(),
                        fields,
                        null,
                        new object[][] { new object[] { "E4", 10000L } }));
                env.AssertPropsPerRowIterator("create", fields, null);

                env.Milestone(5);

                SendSupportBean(env, "E5", 14000L);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E5", 14000L } });

                env.Milestone(6);

                SendSupportBean(env, "E6", 21000L);
                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E6", 21000L } });
                env.AssertPropsPerRowIRPair(
                    "create",
                    fields,
                    new object[][] { new object[] { "E5", 14000L } },
                    new object[][] { new object[] { "E1", 1000L }, new object[] { "E3", 9999L } });

                env.UndeployAll();
            }
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

        private static SupportBean SendSupportBeanLongPrim(
            RegressionEnvironment env,
            string theString,
            long longPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
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
                rep.GetAnnotationText() +
                " @name('create') @public create window MyWindowBC#keepall as (bean SupportBean_S0)",
                path);
            env.AddListener("create");
            env.AssertStatement(
                "create",
                statement => ClassicAssert.IsTrue(rep.MatchesClass(statement.EventType.UnderlyingType)));
            env.CompileDeploy("insert into MyWindowBC select bean.* as bean from SupportBean_S0 as bean", path);

            env.SendEventBean(new SupportBean_S0(1, "E1"));
            env.AssertPropsNew("create", "bean.P00".SplitCsv(), new object[] { "E1" });

            env.UndeployAll();
        }

        private static void TryCreateWindow(
            RegressionEnvironment env,
            string createWindowStatement,
            string deleteStatement)
        {
            var fields = new string[] { "key", "value" };
            var path = new RegressionPath();

            var epl = "@name('create') @public " +
                      createWindowStatement +
                      ";\n" +
                      "@name('insert') insert into MyWindow select TheString as key, LongBoxed as value from SupportBean;\n" +
                      "@name('s0') select irstream key, value*2 as value from MyWindow;\n" +
                      "@name('s2') select irstream key, sum(value) as value from MyWindow group by key;\n" +
                      "@name('s3') select irstream key, value from MyWindow where value >= 10;\n";
            env.CompileDeploy(epl, path).AddListener("create").AddListener("s0").AddListener("s2").AddListener("s3");

            env.AssertStatement(
                "create",
                statement => {
                    ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("key"));
                    ClassicAssert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("value"));
                });

            // send events
            SendSupportBean(env, "E1", 10L);
            env.AssertPropsNew("s0", fields, new object[] { "E1", 20L });
            env.AssertPropsIRPair("s2", fields, new object[] { "E1", 10L }, new object[] { "E1", null });
            env.AssertPropsNew("s3", fields, new object[] { "E1", 10L });
            env.AssertPropsNew("create", fields, new object[] { "E1", 10L });
            env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 10L } });
            env.AssertPropsPerRowIterator("s0", fields, new object[][] { new object[] { "E1", 20L } });

            SendSupportBean(env, "E2", 20L);
            env.AssertPropsNew("s0", fields, new object[] { "E2", 40L });
            env.AssertPropsIRPair("s2", fields, new object[] { "E2", 20L }, new object[] { "E2", null });
            env.AssertPropsNew("s3", fields, new object[] { "E2", 20L });
            env.AssertPropsNew("create", fields, new object[] { "E2", 20L });
            env.AssertPropsPerRowIterator(
                "create",
                fields,
                new object[][] { new object[] { "E1", 10L }, new object[] { "E2", 20L } });
            env.AssertPropsPerRowIterator(
                "s0",
                fields,
                new object[][] { new object[] { "E1", 20L }, new object[] { "E2", 40L } });

            SendSupportBean(env, "E3", 5L);
            env.AssertPropsNew("s0", fields, new object[] { "E3", 10L });
            env.AssertPropsIRPair("s2", fields, new object[] { "E3", 5L }, new object[] { "E3", null });
            env.AssertListenerNotInvoked("s3");
            env.AssertPropsNew("create", fields, new object[] { "E3", 5L });
            env.AssertPropsPerRowIterator(
                "create",
                fields,
                new object[][] { new object[] { "E1", 10L }, new object[] { "E2", 20L }, new object[] { "E3", 5L } });

            // create delete stmt
            env.CompileDeploy("@name('delete') " + deleteStatement, path).AddListener("delete");

            // send delete event
            SendMarketBean(env, "E1");
            env.AssertPropsOld("s0", fields, new object[] { "E1", 20L });
            env.AssertPropsIRPair("s2", fields, new object[] { "E1", null }, new object[] { "E1", 10L });
            env.AssertPropsOld("s3", fields, new object[] { "E1", 10L });
            env.AssertPropsOld("create", fields, new object[] { "E1", 10L });
            env.AssertPropsPerRowIterator(
                "create",
                fields,
                new object[][] { new object[] { "E2", 20L }, new object[] { "E3", 5L } });

            // send delete event again, none deleted now
            SendMarketBean(env, "E1");
            env.AssertListenerNotInvoked("s0");
            env.AssertListenerNotInvoked("s2");
            env.AssertListenerNotInvoked("create");
            env.AssertListenerInvoked("delete");
            env.AssertPropsPerRowIterator(
                "create",
                fields,
                new object[][] { new object[] { "E2", 20L }, new object[] { "E3", 5L } });

            // send delete event
            SendMarketBean(env, "E2");
            env.AssertPropsOld("s0", fields, new object[] { "E2", 40L });
            env.AssertPropsIRPair("s2", fields, new object[] { "E2", null }, new object[] { "E2", 20L });
            env.AssertPropsOld("s3", fields, new object[] { "E2", 20L });
            env.AssertPropsOld("create", fields, new object[] { "E2", 20L });
            env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E3", 5L } });

            // send delete event
            SendMarketBean(env, "E3");
            env.AssertPropsOld("s0", fields, new object[] { "E3", 10L });
            env.AssertPropsIRPair("s2", fields, new object[] { "E3", null }, new object[] { "E3", 5L });
            env.AssertListenerNotInvoked("s3");
            env.AssertPropsOld("create", fields, new object[] { "E3", 5L });
            env.AssertListenerInvoked("delete");
            env.AssertPropsPerRowIterator("create", fields, null);

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
                " @name('create') @public create window MyWindowBB#keepall as SupportBean",
                path);
            env.AddListener("create");
            env.CompileDeploy("@public insert into MyWindowBB select * from SupportBean", path);

            env.CompileDeploy("@name('s0') select * from MyWindowBB", path).AddListener("s0");
            AssertStatelessStmt(env, "s0", true);

            env.SendEventBean(new SupportBean());
            env.AssertEventNew("create", @event => AssertEvent(@event, "MyWindowBB"));
            env.AssertListener("s0", listener => AssertEvent(listener.AssertOneGetNewAndReset(), "MyWindowBB"));

            env.CompileDeploy("@name('update') on SupportBean_A update MyWindowBB set TheString='s'", path)
                .AddListener("update");
            env.SendEventBean(new SupportBean_A("A1"));
            env.AssertListener("update", listener => AssertEvent(listener.LastNewData[0], "MyWindowBB"));

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
            ClassicAssert.IsTrue(theEvent.EventType is BeanEventType);
            ClassicAssert.IsTrue(theEvent.Underlying is SupportBean);
            ClassicAssert.AreEqual(EventTypeTypeClass.NAMED_WINDOW, theEvent.EventType.Metadata.TypeClass);
            ClassicAssert.AreEqual(name, theEvent.EventType.Name);
        }

        public static Schema GetSupportBeanS0Schema()
        {
            return SchemaBuilder.Record("SupportBean_S0", TypeBuilder.RequiredString("P00"));
        }
    }
} // end of namespace