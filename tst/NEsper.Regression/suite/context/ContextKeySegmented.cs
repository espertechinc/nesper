///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmented
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPatternFilter(execs);
            WithJoinRemoveStream(execs);
            WithSelector(execs);
            WithLargeNumberPartitions(execs);
            WithAdditionalFilters(execs);
            WithMultiStatementFilterCount(execs);
            WithSubtype(execs);
            WithJoinMultitypeMultifield(execs);
            WithSubselectPrevPrior(execs);
            WithPrior(execs);
            WithSubqueryFiltered(execs);
            WithJoin(execs);
            WithPattern(execs);
            WithPatternSceneTwo(execs);
            WithViewSceneOne(execs);
            WithViewSceneTwo(execs);
            WithJoinWhereClauseOnPartitionKey(execs);
            WithNullSingleKey(execs);
            WithNullKeyMultiKey(execs);
            WithInvalid(execs);
            WithTermByFilter(execs);
            WithMatchRecognize(execs);
            WithMultikeyWArrayOfPrimitive(execs);
            WithMultikeyWArrayTwoField(execs);
            WithWInitTermEndEvent(execs);
            WithWPatternFireWhenAllocated(execs);
            WithWInitTermPatternAsName(execs);
            WithTermEventSelect(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTermEventSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedTermEventSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithWInitTermPatternAsName(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedWInitTermPatternAsName());
            return execs;
        }

        public static IList<RegressionExecution> WithWPatternFireWhenAllocated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedWPatternFireWhenAllocated());
            return execs;
        }

        public static IList<RegressionExecution> WithWInitTermEndEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedWInitTermEndEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayTwoField(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedMultikeyWArrayTwoField());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayOfPrimitive(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedMultikeyWArrayOfPrimitive());
            return execs;
        }

        public static IList<RegressionExecution> WithMatchRecognize(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedMatchRecognize());
            return execs;
        }

        public static IList<RegressionExecution> WithTermByFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedTermByFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNullKeyMultiKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedNullKeyMultiKey());
            return execs;
        }

        public static IList<RegressionExecution> WithNullSingleKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedNullSingleKey());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinWhereClauseOnPartitionKey(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedJoinWhereClauseOnPartitionKey());
            return execs;
        }

        public static IList<RegressionExecution> WithViewSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedViewSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithViewSceneOne(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedViewSceneOne());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternSceneTwo(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedPatternSceneTwo());
            return execs;
        }

        public static IList<RegressionExecution> WithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryFiltered(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedSubqueryFiltered());
            return execs;
        }

        public static IList<RegressionExecution> WithPrior(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedPrior());
            return execs;
        }

        public static IList<RegressionExecution> WithSubselectPrevPrior(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedSubselectPrevPrior());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinMultitypeMultifield(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedJoinMultitypeMultifield());
            return execs;
        }

        public static IList<RegressionExecution> WithSubtype(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedSubtype());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiStatementFilterCount(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedMultiStatementFilterCount());
            return execs;
        }

        public static IList<RegressionExecution> WithAdditionalFilters(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedAdditionalFilters());
            return execs;
        }

        public static IList<RegressionExecution> WithLargeNumberPartitions(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedLargeNumberPartitions());
            return execs;
        }

        public static IList<RegressionExecution> WithSelector(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedSelector());
            return execs;
        }

        public static IList<RegressionExecution> WithJoinRemoveStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedJoinRemoveStream());
            return execs;
        }

        public static IList<RegressionExecution> WithPatternFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedPatternFilter());
            return execs;
        }

        internal class ContextKeySegmentedTermEventSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema UserEvent(UserId string, alert string);\n" +
                          "create context UserSessionContext partition by UserId from UserEvent\n" +
                          "  initiated by UserEvent(alert = 'A')\n" +
                          "  terminated by UserEvent(alert = 'B') as termEvent;\n" +
                          "@name('s0') context UserSessionContext select *, context.termEvent as term from UserEvent#firstevent\n" +
                          "  output snapshot when terminated;";
                env.CompileDeploy(epl).AddListener("s0");

                SendUser(env, "U1", "A");
                SendUser(env, "U1", null);
                SendUser(env, "U1", null);
                env.AssertListenerNotInvoked("s0");
                env.Milestone(0);

                var term = SendUser(env, "U1", "B");
                env.AssertEventNew("s0", @event => Assert.AreEqual(term, @event.Get("term")));

                env.UndeployAll();
            }

            private IDictionary<string, object> SendUser(
                RegressionEnvironment env,
                string user,
                string alert)
            {
                var data = CollectionUtil.BuildMap("UserId", user, "alert", alert);
                env.SendEventMap(data, "UserEvent");
                return data;
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }

        internal class ContextKeySegmentedWInitTermPatternAsName : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext partition by TheString from SupportBean\n" +
                          "initiated by SupportBean(IntPrimitive = 1) as startevent\n" +
                          "terminated by pattern[s=SupportBean(IntPrimitive = 2)] as endpattern;\n" +
                          "@name('s0') context MyContext select context.startevent.IntBoxed as c0, context.endpattern.s.IntBoxed as c1 from SupportBean#firstevent output snapshot when terminated;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendSBEvent(env, "A", 10, 1);
                SendSBEvent(env, "A", 20, 2);
                env.AssertPropsNew("s0", "c0,c1".Split(","), new object[] { 10, 20 });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedWInitTermEndEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext partition by TheString from SupportBean\n" +
                          "initiated by SupportBean(IntPrimitive = 1) as startevent\n" +
                          "terminated by SupportBean(IntPrimitive = 0) as endevent;\n" +
                          "@name('s0') context MyContext select context.startevent as c0, context.endevent as c1 from SupportBean output all when terminated;\n";
                env.CompileDeploy(epl).AddListener("s0");

                var sb1 = SendSBEvent(env, "A", 1);
                var sb2 = SendSBEvent(env, "A", 0);

                env.AssertPropsNew("s0", "c0,c1".Split(","), new object[] { sb1, sb2 });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedWPatternFireWhenAllocated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContext partition by TheString from SupportBean;\n" +
                          "@name('s0') context MyContext select context.key1 as key1 from pattern[timer:interval(0)];\n" +
                          "context MyContext create variable String lastString = null;\n" +
                          "context MyContext on pattern[timer:interval(0)] set lastString = context.key1;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertKey(env, "E1");
                SendAssertNoReceived(env, "E1");

                env.Milestone(0);

                SendAssertNoReceived(env, "E1");
                SendAssertKey(env, "E2");

                env.UndeployAll();
            }

            private void SendAssertNoReceived(
                RegressionEnvironment env,
                string theString)
            {
                env.SendEventBean(new SupportBean(theString, 1));
                env.AssertListenerNotInvoked("s0");
            }

            private void SendAssertKey(
                RegressionEnvironment env,
                string theString)
            {
                env.SendEventBean(new SupportBean(theString, 0));
                env.AssertEqualsNew("s0", "key1", theString);

                env.AssertThat(
                    () => {
                        var pair = new DeploymentIdNamePair(env.DeploymentId("s0"), "lastString");
                        var set = Collections.SingletonSet(pair);
                        var values = env.Runtime.VariableService.GetVariableValue(
                            set,
                            new SupportSelectorPartitioned(theString));
                        Assert.AreEqual(theString, values.Get(pair).GetEnumerator().Advance().State);
                    });
            }
        }

        internal class ContextKeySegmentedMultikeyWArrayTwoField : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context PartitionByArray partition by Id, array from SupportEventWithIntArray;\n" +
                          "@name('s0') context PartitionByArray select sum(value) as thesum from SupportEventWithIntArray;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertArray(env, "G1", new int[] { 1, 2 }, 1, 1);
                SendAssertArray(env, "G2", new int[] { 1, 2 }, 2, 2);
                SendAssertArray(env, "G1", new int[] { 1 }, 3, 3);

                env.Milestone(0);

                SendAssertArray(env, "G2", new int[] { 1, 2 }, 10, 2 + 10);
                SendAssertArray(env, "G1", new int[] { 1, 2 }, 15, 1 + 15);
                SendAssertArray(env, "G1", new int[] { 1 }, 18, 3 + 18);

                var selector = new SupportSelectorPartitioned(
                    Collections.SingletonList(new object[] { "G2", new int[] { 1, 2 } }));
                env.AssertStatement(
                    "s0",
                    statement => EPAssertionUtil.AssertPropsPerRow(
                        statement.GetEnumerator(selector),
                        "thesum".Split(","),
                        new object[][] { new object[] { 2 + 10 } }));

                ContextPartitionSelectorFiltered selectorWFilter = new ProxyContextPartitionSelectorFiltered(
                    contextPartitionIdentifier => {
                        var partitioned = (ContextPartitionIdentifierPartitioned)contextPartitionIdentifier;
                        return partitioned.Keys[0].Equals("G2") &&
                               Arrays.Equals((int[])partitioned.Keys[1], new int[] { 1, 2 });
                    });
                env.AssertStatement(
                    "s0",
                    statement => EPAssertionUtil.AssertPropsPerRow(
                        statement.GetEnumerator(selectorWFilter),
                        "thesum".Split(","),
                        new object[][] { new object[] { 2 + 10 } }));

                env.UndeployAll();
            }

            private void SendAssertArray(
                RegressionEnvironment env,
                string id,
                int[] array,
                int value,
                int expected)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, array, value));
                env.AssertEqualsNew("s0", "thesum", expected);
            }
        }

        internal class ContextKeySegmentedMultikeyWArrayOfPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context PartitionByArray partition by array from SupportEventWithIntArray;\n" +
                          "@name('s0') context PartitionByArray select sum(value) as thesum from SupportEventWithIntArray;\n";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssertArray(env, "E1", new int[] { 1, 2 }, 10, 10);
                SendAssertArray(env, "E2", new int[] { 1, 2 }, 11, 21);
                SendAssertArray(env, "E3", new int[] { 1 }, 12, 12);
                SendAssertArray(env, "E4", new int[] { }, 13, 13);
                SendAssertArray(env, "E5", null, 14, 14);

                env.Milestone(0);

                SendAssertArray(env, "E10", null, 20, 14 + 20);
                SendAssertArray(env, "E11", new int[] { 1, 2 }, 21, 21 + 21);
                SendAssertArray(env, "E12", new int[] { 1 }, 22, 12 + 22);
                SendAssertArray(env, "E13", new int[] { }, 23, 13 + 23);

                var selectorPartition =
                    new SupportSelectorPartitioned(Collections.SingletonList(new object[] { new int[] { 1, 2 } }));
                env.AssertStatement(
                    "s0",
                    statement => EPAssertionUtil.AssertPropsPerRow(
                        statement.GetEnumerator(selectorPartition),
                        "thesum".Split(","),
                        new object[][] { new object[] { 21 + 21 } }));

                ContextPartitionSelectorFiltered selectorWFilter = new ProxyContextPartitionSelectorFiltered(
                    contextPartitionIdentifier => {
                        var partitioned = (ContextPartitionIdentifierPartitioned)contextPartitionIdentifier;
                        return Arrays.Equals((int[])partitioned.Keys[0], new int[] { 1 });
                    });
                env.AssertStatement(
                    "s0",
                    statement => EPAssertionUtil.AssertPropsPerRow(
                        statement.GetEnumerator(selectorWFilter),
                        "thesum".Split(","),
                        new object[][] { new object[] { 12 + 22 } }));

                env.UndeployAll();
            }

            private void SendAssertArray(
                RegressionEnvironment env,
                string id,
                int[] array,
                int value,
                int expected)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, array, value));
                env.AssertEqualsNew("s0", "thesum", expected);
            }
        }

        internal class ContextKeySegmentedPatternFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext = "@public create context IndividualBean partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var eplAnalysis = "@name('s0') context IndividualBean " +
                                  "select * from pattern [every (event1=SupportBean(stringContainsX(TheString) = false) -> event2=SupportBean(stringContainsX(TheString) = true))]";
                env.CompileDeploy(eplAnalysis, path).AddListener("s0");

                env.SendEventBean(new SupportBean("F1", 0));
                env.SendEventBean(new SupportBean("F1", 0));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("X1", 0));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("X1", 0));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedMatchRecognize : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                var eplContextOne = "@public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContextOne, path);

                var eplMatchRecog = "@name('s0') context SegmentedByString " +
                                    "select * from SupportBean\n" +
                                    "match_recognize ( \n" +
                                    "  measures A.LongPrimitive as a, B.LongPrimitive as b\n" +
                                    "  pattern (A B) \n" +
                                    "  define " +
                                    "    A as A.IntPrimitive = 1," +
                                    "    B as B.IntPrimitive = 2\n" +
                                    ")";
                env.CompileDeploy(eplMatchRecog, path).AddListener("s0");

                env.SendEventBean(MakeEvent("A", 1, 10));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("B", 1, 30));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("A", 2, 20));
                env.AssertPropsNew("s0", "a,b".Split(","), new object[] { 10L, 20L });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("B", 2, 40));
                env.AssertPropsNew("s0", "a,b".Split(","), new object[] { 30L, 40L });

                env.UndeployAll();

                // try with "prev"
                path.Clear();
                var eplContextTwo = "@public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContextTwo, path);

                var eplMatchRecogWithPrev = "@name('s0') context SegmentedByString select * from SupportBean " +
                                            "match_recognize ( " +
                                            "  measures A.LongPrimitive as e1, B.LongPrimitive as e2" +
                                            "  pattern (A B) " +
                                            "  define A as A.IntPrimitive >= prev(A.IntPrimitive),B as B.IntPrimitive >= prev(B.IntPrimitive) " +
                                            ")";
                env.CompileDeploy(eplMatchRecogWithPrev, path).AddListener("s0");

                env.SendEventBean(MakeEvent("A", 1, 101));
                env.SendEventBean(MakeEvent("B", 1, 201));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("A", 2, 102));
                env.SendEventBean(MakeEvent("B", 2, 202));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("A", 3, 103));
                env.AssertPropsNew("s0", "e1,e2".Split(","), new object[] { 102L, 103L });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("B", 3, 203));
                env.AssertPropsNew("s0", "e1,e2".Split(","), new object[] { 202L, 203L });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoinRemoveStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();

                var stmtContext =
                    "@public create context SegmentedBySession partition by sessionId from SupportWebEvent";
                env.CompileDeploy(stmtContext, path);

                var epl = "@name('s0') context SegmentedBySession " +
                          " select rstream A.pageName as pageNameA , A.sessionId as sessionIdA, B.pageName as pageNameB, C.pageName as pageNameC from " +
                          "SupportWebEvent(pageName='Start')#time(30) A " +
                          "full outer join " +
                          "SupportWebEvent(pageName='Middle')#time(30) B on A.sessionId = B.sessionId " +
                          "full outer join " +
                          "SupportWebEvent(pageName='End')#time(30) C on A.sessionId  = C.sessionId " +
                          "where A.pageName is not null and (B.pageName is null or C.pageName is null) ";
                env.CompileDeploy(epl, path);

                env.AddListener("s0");

                // Set up statement for finding missing events
                SendWebEventsComplete(env, 0);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(20000);
                SendWebEventsComplete(env, 1);

                env.AdvanceTime(40000);
                SendWebEventsComplete(env, 2);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(60000);
                SendWebEventsIncomplete(env, 3);

                env.AdvanceTime(80000);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(100000);
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSelector : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context PartitionedByString partition by TheString from SupportBean",
                    path);
                var fields = "c0,c1".Split(",");
                env.CompileDeploy(
                    "@name('s0') context PartitionedByString select context.key1 as c0, sum(IntPrimitive) as c1 from SupportBean#length(5)",
                    path);

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E2", 21));

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 41 } });

                env.Milestone(1);

                env.AssertPropsPerRowIterator(
                    "s0",
                    fields,
                    new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 41 } });

                // test iterator targeted
                env.AssertStatement(
                    "s0",
                    statement => {
                        var selector = new SupportSelectorPartitioned(Collections.SingletonList(new object[] { "E2" }));
                        EPAssertionUtil.AssertPropsPerRow(
                            statement.GetEnumerator(selector),
                            statement.GetSafeEnumerator(selector),
                            fields,
                            new object[][] { new object[] { "E2", 41 } });
                        Assert.IsFalse(
                            statement.GetEnumerator(new SupportSelectorPartitioned((IList<object[]>)null)).MoveNext());
                        Assert.IsFalse(
                            statement.GetEnumerator(
                                    new SupportSelectorPartitioned(Collections.SingletonList(new object[] { "EX" })))
                                .MoveNext());
                        Assert.IsFalse(
                            statement.GetEnumerator(new SupportSelectorPartitioned(EmptyList<object[]>.Instance))
                                .MoveNext());

                        // test iterator filtered
                        var filtered = new MySelectorFilteredPartitioned(new object[] { "E2" });
                        EPAssertionUtil.AssertPropsPerRow(
                            statement.GetEnumerator(filtered),
                            statement.GetSafeEnumerator(filtered),
                            fields,
                            new object[][] { new object[] { "E2", 41 } });

                        // test always-false filter - compare context partition info
                        var filteredFalse = new MySelectorFilteredPartitioned(null);
                        Assert.IsFalse(statement.GetEnumerator(filteredFalse).MoveNext());
                        EPAssertionUtil.AssertEqualsAnyOrder(
                            new object[] { new object[] { "E1" }, new object[] { "E2" } },
                            filteredFalse.GetContexts().ToArray());

                        try {
                            statement.GetEnumerator(new ProxyContextPartitionSelectorCategory(() => null));
                            Assert.Fail();
                        }
                        catch (InvalidContextPartitionSelector ex) {
                            Assert.IsTrue(
                                ex.Message.StartsWith(
                                    "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com."),
                                "message: " + ex.Message);
                        }
                    });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid filter spec
                epl = "create context SegmentedByAString partition by string from SupportBean(dummy = 1)";
                env.TryInvalidCompile(
                    epl,
                    "Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");

                // property not found
                epl = "create context SegmentedByAString partition by dummy from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "For context 'SegmentedByAString' property name 'dummy' not found on type SupportBean [");

                // mismatch number pf properties
                epl =
                    "create context SegmentedByAString partition by TheString from SupportBean, Id, P00 from SupportBean_S0";
                env.TryInvalidCompile(
                    epl,
                    "For context 'SegmentedByAString' expected the same number of property names for each event type, found 1 properties for event type 'SupportBean' and 2 properties for event type 'SupportBean_S0' [create context SegmentedByAString partition by TheString from SupportBean, Id, P00 from SupportBean_S0]");

                // incompatible property types
                epl =
                    "create context SegmentedByAString partition by TheString from SupportBean, Id from SupportBean_S0";
                env.TryInvalidCompile(
                    epl,
                    "For context 'SegmentedByAString' for context 'SegmentedByAString' found mismatch of property types, property 'TheString' of type 'String' compared to property 'Id' of type 'Integer' [");

                // duplicate type specification
                epl =
                    "create context SegmentedByAString partition by TheString from SupportBean, TheString from SupportBean";
                env.TryInvalidCompile(
                    epl,
                    "For context 'SegmentedByAString' the event type 'SupportBean' is listed twice [");

                // duplicate type: subtype
                epl = "create context SegmentedByAString partition by baseAB from ISupportBaseAB, a from ISupportA";
                env.TryInvalidCompile(
                    epl,
                    "For context 'SegmentedByAString' the event type 'ISupportA' is listed twice: Event type 'ISupportA' is a subtype or supertype of event type 'ISupportBaseAB' [");

                // validate statement not applicable filters
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context SegmentedByAString partition by TheString from SupportBean",
                    path);
                epl = "context SegmentedByAString select * from SupportBean_S0";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Segmented context 'SegmentedByAString' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

                // invalid attempt to partition a named window's streams
                env.CompileDeploy("@public create window MyWindow#keepall as SupportBean", path);
                epl = "@public create context SegmentedByWhat partition by TheString from MyWindow";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Partition criteria may not include named windows [@public create context SegmentedByWhat partition by TheString from MyWindow]");

                // partitioned with named window
                env.CompileDeploy("@public create schema SomeSchema(ipAddress string)", path);
                env.CompileDeploy(
                    "@public create context TheSomeSchemaCtx Partition By ipAddress From SomeSchema",
                    path);
                epl = "@public context TheSomeSchemaCtx create window MyEvent#time(30 sec) (ipAddress string)";
                env.TryInvalidCompile(
                    path,
                    epl,
                    "Segmented context 'TheSomeSchemaCtx' requires that named windows are associated to an existing event type and that the event type is listed among the partitions defined by the create-context statement");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedLargeNumberPartitions : RegressionExecution
        {
            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.EXCLUDEWHENINSTRUMENTED);
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByAString  partition by TheString from SupportBean",
                    path);

                var fields = "col1".Split(",");
                env.CompileDeploy(
                    "@name('s0') context SegmentedByAString " +
                    "select sum(IntPrimitive) as col1," +
                    "prev(1, IntPrimitive)," +
                    "prior(1, IntPrimitive)," +
                    "(select Id from SupportBean_S0#lastevent)" +
                    "  from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                for (var i = 0; i < 10000; i++) {
                    env.SendEventBean(new SupportBean("E" + i, i));
                    env.AssertPropsNew("s0", fields, new object[] { i });
                }

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedAdditionalFilters : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByAString " +
                    "partition by TheString from SupportBean(IntPrimitive>0), P00 from SupportBean_S0(Id > 0)",
                    path);

                // first send a view events
                env.SendEventBean(new SupportBean("B1", -1));
                env.SendEventBean(new SupportBean_S0(-2, "S0"));
                env.AssertThat(() => Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                var fields = "col1,col2".Split(",");
                env.CompileDeploy(
                    "@name('s0') context SegmentedByAString " +
                    "select sum(sb.IntPrimitive) as col1, sum(s0.Id) as col2 " +
                    "from pattern [every (s0=SupportBean_S0 or sb=SupportBean)]",
                    path);
                env.AddListener("s0");

                env.AssertThat(() => Assert.AreEqual(2, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(-3, "S0"));
                env.SendEventBean(new SupportBean("S0", -1));
                env.SendEventBean(new SupportBean("S1", -2));
                env.AssertListenerNotInvoked("s0");
                env.AssertThat(() => Assert.AreEqual(2, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(2, "S0"));
                env.AssertPropsNew("s0", fields, new object[] { null, 2 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("S1", 10));
                env.AssertPropsNew("s0", fields, new object[] { 10, null });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(-2, "S0"));
                env.SendEventBean(new SupportBean("S1", -10));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(3, "S1"));
                env.AssertPropsNew("s0", fields, new object[] { 10, 3 });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("S0", 9));
                env.AssertPropsNew("s0", fields, new object[] { 9, 2 });

                env.Milestone(6);

                env.UndeployAll();
                env.AssertThat(() => Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.Milestone(7);

                // Test unnecessary filter
                var epl = "@public create context CtxSegmented partition by TheString from SupportBean;" +
                          "context CtxSegmented select * from pattern [every a=SupportBean -> c=SupportBean(c.TheString=a.TheString)];";
                env.CompileDeploy(epl);
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedMultiStatementFilterCount : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByAString " +
                    "partition by TheString from SupportBean, P00 from SupportBean_S0",
                    path);
                env.AssertThat(() => Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                // first send a view events
                env.SendEventBean(new SupportBean("B1", 1));
                env.SendEventBean(new SupportBean_S0(10, "S0"));

                var fields = new string[] { "col1" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByAString select sum(Id) as col1 from SupportBean_S0",
                    path);
                env.AddListener("s0");

                env.AssertThat(() => Assert.AreEqual(2, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.SendEventBean(new SupportBean_S0(10, "S0"));
                env.AssertPropsNew("s0", fields, new object[] { 10 });

                env.Milestone(0);

                env.AssertThat(() => Assert.AreEqual(3, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.SendEventBean(new SupportBean_S0(8, "S1"));
                env.AssertPropsNew("s0", fields, new object[] { 8 });

                env.Milestone(1);

                env.AssertThat(() => Assert.AreEqual(4, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.SendEventBean(new SupportBean_S0(4, "S0"));
                env.AssertPropsNew("s0", fields, new object[] { 14 });

                env.Milestone(2);

                env.AssertThat(() => Assert.AreEqual(4, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.CompileDeploy(
                    "@name('s1') context SegmentedByAString select sum(IntPrimitive) as col1 from SupportBean",
                    path);
                env.AddListener("s1");

                env.AssertThat(() => Assert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.SendEventBean(new SupportBean("S0", 5));
                env.AssertPropsNew("s1", fields, new object[] { 5 });

                env.AssertThat(() => Assert.AreEqual(6, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.Milestone(3);

                env.SendEventBean(new SupportBean("S2", 6));
                env.AssertPropsNew("s1", fields, new object[] { 6 });

                env.AssertThat(() => Assert.AreEqual(8, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.UndeployModuleContaining("s0");
                env.AssertThat(
                    () => Assert.AreEqual(
                        5,
                        SupportFilterServiceHelper
                            .GetFilterSvcCountApprox(env))); // 5 = 3 from context instances and 2 from context itself

                env.Milestone(4);

                env.UndeployModuleContaining("s1");
                env.AssertThat(() => Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.UndeployModuleContaining("context");
                env.AssertThat(() => Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env)));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubtype : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "col1".Split(",");
                var epl =
                    "@name('context') create context SegmentedByString partition by baseAB from ISupportBaseAB;\n" +
                    "@name('s0') context SegmentedByString select count(*) as col1 from ISupportA;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new ISupportAImpl("A1", "AB1"));
                env.AssertPropsNew("s0", fields, new object[] { 1L });

                env.SendEventBean(new ISupportAImpl("A2", "AB1"));
                env.AssertPropsNew("s0", fields, new object[] { 2L });

                env.Milestone(1);

                env.SendEventBean(new ISupportAImpl("A3", "AB2"));
                env.AssertPropsNew("s0", fields, new object[] { 1L });

                env.SendEventBean(new ISupportAImpl("A4", "AB1"));
                env.AssertPropsNew("s0", fields, new object[] { 3L });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoinMultitypeMultifield : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedBy2Fields " +
                    "partition by TheString and IntPrimitive from SupportBean, P00 and Id from SupportBean_S0",
                    path);

                var fields = "c1,c2,c3,c4,c5,c6".Split(",");
                env.CompileDeploy(
                    "@name('s0') context SegmentedBy2Fields " +
                    "select TheString as c1, IntPrimitive as c2, Id as c3, P00 as c4, context.key1 as c5, context.key2 as c6 " +
                    "from SupportBean#lastevent, SupportBean_S0#lastevent",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(2, "G1"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 2));

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(1, "G2"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G2", 1));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 1, 1, "G2", "G2", 1 });

                env.SendEventBean(new SupportBean_S0(2, "G2"));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 2, 2, "G2", "G2", 2 });

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "G1"));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 1, 1, "G1", "G1", 1 });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G1", 2));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 2, 2, "G1", "G1", 2 });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubselectPrevPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsPrev = new string[] { "TheString", "col1" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select TheString, (select prev(0, Id) from SupportBean_S0#keepall) as col1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsPrev, new object[] { "G1", null });

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fieldsPrev, new object[] { "G1", 1 });

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew("s0", fieldsPrev, new object[] { "G2", null });

                env.SendEventBean(new SupportBean_S0(2, "E2"));
                env.SendEventBean(new SupportBean("G2", 21));
                env.AssertPropsNew("s0", fieldsPrev, new object[] { "G2", 2 });

                env.SendEventBean(new SupportBean("G1", 12));
                env.AssertPropsNew("s0", fieldsPrev, new object[] { "G1", null }); // since returning multiple rows

                env.UndeployModuleContaining("s0");

                var fieldsPrior = new string[] { "TheString", "col1" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select TheString, (select prior(0, Id) from SupportBean_S0#keepall) as col1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "G1", null });

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "G1", 1 });

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew(
                    "s0",
                    fieldsPrior,
                    new object[] { "G2", null }); // since category started as soon as statement added

                env.SendEventBean(new SupportBean_S0(2, "E2"));
                env.SendEventBean(new SupportBean("G2", 21));
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "G2", 2 }); // since returning multiple rows

                env.SendEventBean(new SupportBean("G1", 12));
                env.AssertPropsNew("s0", fieldsPrior, new object[] { "G1", null }); // since returning multiple rows

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedPrior : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fields = new string[] { "val0", "val1" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select IntPrimitive as val0, prior(1, IntPrimitive) as val1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { 10, null });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew("s0", fields, new object[] { 20, null });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fields, new object[] { 11, 10 });

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubqueryFiltered : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fields = new string[] { "TheString", "IntPrimitive", "val0" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select P00 from SupportBean_S0#lastevent as s0 where sb.IntPrimitive = s0.Id) as val0 " +
                    "from SupportBean as sb",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "s1"));
                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, null });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(10, "s2"));
                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, "s2" });

                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 10, null });

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(10, "s3"));
                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 10, "s3" });

                env.SendEventBean(new SupportBean("G3", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G3", 10, null });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, "s3" });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fields = new string[] { "sb.TheString", "sb.IntPrimitive", "s0.Id" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select * from SupportBean#keepall as sb, SupportBean_S0#keepall as s0 " +
                    "where IntPrimitive = Id",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(20));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 20, 20 });

                env.SendEventBean(new SupportBean_S0(30));
                env.SendEventBean(new SupportBean("G3", 30));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G1", 30));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 30, 30 });

                env.SendEventBean(new SupportBean("G2", 30));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 30, 30 });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fields = new string[] { "a.TheString", "a.IntPrimitive", "b.TheString", "b.IntPrimitive" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean(IntPrimitive=a.IntPrimitive+1)]",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G1", 20));
                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 21));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 20, "G2", 21 });

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, "G1", 11 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 22));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 21, "G2", 22 });

                env.UndeployModuleContaining("s0");

                // add another statement: contexts already exist, this one uses @Consume
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean(IntPrimitive=a.IntPrimitive+1)@Consume]",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G1", 20));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G2", 21));
                env.AssertPropsNew("s0", fields, new object[] { "G2", 20, "G2", 21 });

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, "G1", 11 });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G2", 22));
                env.AssertListenerNotInvoked("s0");

                env.UndeployModuleContaining("s0");

                // test truly segmented consume
                var fieldsThree = new string[] { "a.TheString", "a.IntPrimitive", "b.Id", "b.P00" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select * from pattern [every a=SupportBean -> b=SupportBean_S0(Id=a.IntPrimitive)@Consume]",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S0(10, "E1")); // should be 2 output rows
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fieldsThree,
                    new object[][] { new object[] { "G1", 10, 10, "E1" }, new object[] { "G2", 10, 10, "E1" } });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedPatternSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('CTX') @public create context SegmentedByString partition by TheString from SupportBean, P00 from SupportBean_S0;\n" +
                    "@name('S1') context SegmentedByString " +
                    "select a.TheString as c0, a.IntPrimitive as c1, b.Id as c2, b.P00 as c3 from pattern [" +
                    "every a=SupportBean -> b=SupportBean_S0(Id=a.IntPrimitive)];\n";
                env.CompileDeploy(epl).AddListener("S1");
                var fields = "c0,c1,c2,c3".Split(",");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 20));
                env.SendEventBean(new SupportBean_S0(0, "G1"));
                env.SendEventBean(new SupportBean_S0(10, "G2"));
                env.AssertListenerNotInvoked("S1");

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(20, "G2"));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 20, 20, "G2" });

                env.Milestone(2);

                env.SendEventBean(new SupportBean_S0(20, "G2"));
                env.SendEventBean(new SupportBean_S0(0, "G1"));
                env.AssertListenerNotInvoked("S1");

                env.SendEventBean(new SupportBean_S0(10, "G1"));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 10, 10, "G1" });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedViewSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var contextEPL =
                    "@name('context') @public create context SegmentedByString as partition by TheString from SupportBean";
                env.CompileDeploy(contextEPL, path);

                var fieldsIterate = "IntPrimitive".Split(",");
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
"select irstream IntPrimitive, prevwindow(Items) as pw from SupportBean#length(2) as Items",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                AssertViewData(env, 10, new object[][] { new object[] { "G1", 10 } }, null);
                env.AssertPropsPerRowIterator("s0", fieldsIterate, new object[][] { new object[] { 10 } });

                env.SendEventBean(new SupportBean("G2", 20));
                AssertViewData(env, 20, new object[][] { new object[] { "G2", 20 } }, null);

                env.SendEventBean(new SupportBean("G1", 11));
                AssertViewData(env, 11, new object[][] { new object[] { "G1", 11 }, new object[] { "G1", 10 } }, null);
                env.AssertPropsPerRowIterator(
                    "s0",
                    fieldsIterate,
                    new object[][] { new object[] { 10 }, new object[] { 11 }, new object[] { 20 } });

                env.SendEventBean(new SupportBean("G2", 21));
                AssertViewData(env, 21, new object[][] { new object[] { "G2", 21 }, new object[] { "G2", 20 } }, null);

                env.SendEventBean(new SupportBean("G1", 12));
                AssertViewData(env, 12, new object[][] { new object[] { "G1", 12 }, new object[] { "G1", 11 } }, 10);

                env.SendEventBean(new SupportBean("G2", 22));
                AssertViewData(env, 22, new object[][] { new object[] { "G2", 22 }, new object[] { "G2", 21 } }, 20);

                env.UndeployModuleContaining("s0");

                // test SODA
                env.UndeployAll();
                path.Clear();

                env.EplToModelCompileDeploy(contextEPL, path);

                // test built-in properties
                var fields = "c1,c2,c3,c4".Split(",");
                var ctx = "SegmentedByString";
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select context.name as c1, context.Id as c2, context.key1 as c3, TheString as c4 " +
"from SupportBean#length(2) as Items",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { ctx, 0, "G1", "G1" });
                SupportContextPropUtil.AssertContextProps(
                    env,
                    "context",
                    "SegmentedByString",
                    new int[] { 0 },
                    "key1",
                    new object[][] { new object[] { "G1" } });

                env.UndeployAll();

                // test grouped delivery
                path.Clear();
                env.CompileDeploy("@name('var') @public create variable boolean trigger = false", path);
                env.CompileDeploy("@public create context MyCtx partition by TheString from SupportBean", path);
                env.CompileDeploy(
                    "@name('s0') context MyCtx select * from SupportBean#expr(not trigger) for grouped_delivery(TheString)",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.RuntimeSetVariable("var", "trigger", true);
                env.AdvanceTime(100);

                env.AssertListener("s0", listener => Assert.AreEqual(2, listener.NewDataList.Count));

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedViewSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@name('CTX') @public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fields = "TheString,IntPrimitive".Split(",");
                var eplSelect = "@name('S1') context SegmentedByString select irstream * from SupportBean#lastevent()";
                env.CompileDeploy(eplSelect, path).AddListener("S1");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 1 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 10 });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G1", 2));
                env.AssertPropsIRPair("S1", fields, new object[] { "G1", 2 }, new object[] { "G1", 1 });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G2", 11));
                env.AssertPropsIRPair("S1", fields, new object[] { "G2", 11 }, new object[] { "G2", 10 });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedJoinWhereClauseOnPartitionKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyCtx partition by TheString from SupportBean;\n" +
                          "@name('select') context MyCtx select * from SupportBean#lastevent as sb, SupportBean_S0#lastevent as s0 " +
                          "where TheString is 'Test'";
                env.CompileDeploy(epl).AddListener("select");

                env.SendEventBean(new SupportBean("Test", 10));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean_S0(1));
                env.AssertListenerInvoked("select");

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedNullSingleKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create context MyContext partition by TheString from SupportBean", path);
                env.CompileDeploy("@name('s0') context MyContext select count(*) as cnt from SupportBean", path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean(null, 10));
                env.AssertEqualsNew("s0", "cnt", 1L);

                env.SendEventBean(new SupportBean(null, 20));
                env.AssertEqualsNew("s0", "cnt", 2L);

                env.SendEventBean(new SupportBean("A", 30));
                env.AssertEqualsNew("s0", "cnt", 1L);

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedNullKeyMultiKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context MyContext partition by TheString, IntBoxed, IntPrimitive from SupportBean",
                    path);
                env.CompileDeploy("@name('s0') context MyContext select count(*) as cnt from SupportBean", path);
                env.AddListener("s0");

                SendSBEvent(env, "A", null, 1);
                env.AssertEqualsNew("s0", "cnt", 1L);

                SendSBEvent(env, "A", null, 1);
                env.AssertEqualsNew("s0", "cnt", 2L);

                SendSBEvent(env, "A", 10, 1);
                env.AssertEqualsNew("s0", "cnt", 1L);

                env.UndeployAll();
            }
        }

        private static void AssertViewData(
            RegressionEnvironment env,
            int newIntExpected,
            object[][] newArrayExpected,
            int? oldIntExpected)
        {
            env.AssertListener(
                "s0",
                listener => {
                    Assert.AreEqual(1, listener.LastNewData.Length);
                    Assert.AreEqual(newIntExpected, listener.LastNewData[0].Get("IntPrimitive"));
                    var beans = (SupportBean[])listener.LastNewData[0].Get("pw");
                    Assert.AreEqual(newArrayExpected.Length, beans.Length);
                    for (var i = 0; i < beans.Length; i++) {
                        Assert.AreEqual(newArrayExpected[i][0], beans[i].TheString);
                        Assert.AreEqual(newArrayExpected[i][1], beans[i].IntPrimitive);
                    }

                    if (oldIntExpected != null) {
                        Assert.AreEqual(1, listener.LastOldData.Length);
                        Assert.AreEqual(oldIntExpected, listener.LastOldData[0].Get("IntPrimitive"));
                    }
                    else {
                        Assert.IsNull(listener.LastOldData);
                    }

                    listener.Reset();
                });
        }

        internal class ContextKeySegmentedTermByFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context ByP0 as partition by TheString from SupportBean terminated by SupportBean(IntPrimitive<0)",
                    path);
                env.CompileDeploy(
                    "@name('s0') context ByP0 select TheString, count(*) as cnt from SupportBean(IntPrimitive>= 0)",
                    path);

                env.AddListener("s0");

                SendAssertSB(1, env, "A", 0);

                env.Milestone(0);

                SendAssertSB(2, env, "A", 0);
                SendAssertNone(env, new SupportBean("A", -1));
                SendAssertSB(1, env, "A", 0);

                env.Milestone(1);

                SendAssertSB(1, env, "B", 0);
                SendAssertNone(env, new SupportBean("B", -1));

                env.Milestone(2);

                SendAssertSB(1, env, "B", 0);

                env.Milestone(3);

                SendAssertSB(2, env, "B", 0);
                SendAssertNone(env, new SupportBean("B", -1));
                SendAssertSB(1, env, "B", 0);

                SendAssertNone(env, new SupportBean("C", -1));

                env.UndeployAll();
            }
        }

        private class MySelectorFilteredPartitioned : ContextPartitionSelectorFiltered
        {
            private object[] match;

            private IList<object[]> contexts = new List<object[]>();
            private LinkedHashSet<int> cpids = new LinkedHashSet<int>();

            internal MySelectorFilteredPartitioned(object[] match)
            {
                this.match = match;
            }

            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierPartitioned)contextPartitionIdentifier;
                if (match == null && cpids.Contains(id.ContextPartitionId)) {
                    throw new EPRuntimeException("Already exists context Id: " + id.ContextPartitionId);
                }

                cpids.Add(id.ContextPartitionId);
                contexts.Add(id.Keys);
                return Arrays.Equals(id.Keys, match);
            }

            public IList<object[]> GetContexts()
            {
                return contexts;
            }
        }

        private static void SendWebEventsIncomplete(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportWebEvent("Start", id.ToString()));
            env.SendEventBean(new SupportWebEvent("End", id.ToString()));
        }

        private static void SendWebEventsComplete(
            RegressionEnvironment env,
            int id)
        {
            env.SendEventBean(new SupportWebEvent("Start", id.ToString()));
            env.SendEventBean(new SupportWebEvent("Middle", id.ToString()));
            env.SendEventBean(new SupportWebEvent("End", id.ToString()));
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

        public static bool StringContainsX(string theString)
        {
            return theString.Contains("X");
        }

        private static void SendAssertSB(
            long expected,
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
            env.AssertPropsNew("s0", "TheString,cnt".Split(","), new object[] { theString, expected });
        }

        private static void SendAssertNone(
            RegressionEnvironment env,
            object @event)
        {
            env.SendEventBean(@event);
            env.AssertListenerNotInvoked("s0");
        }

        private static SupportBean SendSBEvent(
            RegressionEnvironment env,
            string @string,
            int? intBoxed,
            int intPrimitive)
        {
            var bean = new SupportBean(@string, intPrimitive);
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
            return bean;
        }

        private static SupportBean SendSBEvent(
            RegressionEnvironment env,
            string @string,
            int intPrimitive)
        {
            return SendSBEvent(env, @string, null, intPrimitive);
        }
    }
} // end of namespace