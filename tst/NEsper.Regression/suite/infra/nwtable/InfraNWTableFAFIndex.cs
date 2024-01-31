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
using com.espertech.esper.common.client.fireandforget;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFIndex : IndexBackingTableInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InfraNWTableFAFIndex));

        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithChoiceJoin(execs);
            WithChoice(execs);
            WithMultikeyWArray(execs);
            WithMultikeyWArrayTwoField(execs);
            WithMultikeyWArrayCompositeArray(execs);
            WithMultikeyWArrayCompositeTwoArray(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayCompositeTwoArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexMultikeyWArrayCompositeTwoArray(true));
            execs.Add(new InfraSelectIndexMultikeyWArrayCompositeTwoArray(false));
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayCompositeArray(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexMultikeyWArrayCompositeArray(true));
            execs.Add(new InfraSelectIndexMultikeyWArrayCompositeArray(false));
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayTwoField(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexMultikeyWArrayTwoField(true));
            execs.Add(new InfraSelectIndexMultikeyWArrayTwoField(false));
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexMultikeyWArray(true));
            execs.Add(new InfraSelectIndexMultikeyWArray(false));
            return execs;
        }

        public static IList<RegressionExecution> WithChoice(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexChoice(true));
            execs.Add(new InfraSelectIndexChoice(false));
            return execs;
        }

        public static IList<RegressionExecution> WithChoiceJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectIndexChoiceJoin(true));
            execs.Add(new InfraSelectIndexChoiceJoin(false));
            return execs;
        }

        private class InfraSelectIndexMultikeyWArrayCompositeTwoArray : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexMultikeyWArrayCompositeTwoArray(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = namedWindow
                    ? "@public create window MyInfra#keepall as (Id string, arrayOne string[], arrayTwo string[], value int);\n"
                    : "@public create table MyInfra(Id string primary key, arrayOne string[], arrayTwo string[], value int);\n";
                epl +=
                    "insert into MyInfra select Id, StringOne as arrayOne, StringTwo as arrayTwo, Value as value from SupportEventWithManyArray;\n" +
                    "create index MyInfraIndex on MyInfra(arrayOne, arrayTwo, value btree);\n";
                env.CompileDeploy(epl, path);

                SendManyArray(env, "E1", new string[] { "a", "b" }, new string[] { "c", "d" }, 100);
                SendManyArray(env, "E2", new string[] { "a", "b" }, new string[] { "e", "f" }, 200);
                SendManyArray(env, "E3", new string[] { "a" }, new string[] { "b" }, 300);

                env.Milestone(0);

                AssertFAF(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'e', 'f'} and value > 150", "E2");
                AssertFAF(env, path, "arrayOne = {'a'} and arrayTwo = {'b'} and value > 150", "E3");
                AssertFAF(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'd'} and value > 90", "E1");
                AssertFAFNot(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'd'} and value > 200");
                AssertFAFNot(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'e'} and value > 90");
                AssertFAFNot(env, path, "arrayOne = {'ax', 'b'} and arrayTwo = {'c', 'd'} and value > 90");

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                string[] arrayOne,
                string[] arrayTwo,
                int value)
            {
                env.SendEventBean(
                    new SupportEventWithManyArray(id).WithStringOne(arrayOne).WithStringTwo(arrayTwo).WithValue(value));
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraSelectIndexMultikeyWArrayCompositeArray : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexMultikeyWArrayCompositeArray(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = namedWindow
                    ? "@public create window MyInfra#keepall as (Id string, arrayOne string[], value int);\n"
                    : "@public create table MyInfra(Id string primary key, arrayOne string[], value int);\n";
                epl += "insert into MyInfra select Id, StringOne as arrayOne, Value as value from SupportEventWithManyArray;\n" +
                       "create index MyInfraIndex on MyInfra(arrayOne, value btree);\n";
                env.CompileDeploy(epl, path);

                SendManyArray(env, "E1", new string[] { "a", "b" }, 100);
                SendManyArray(env, "E2", new string[] { "a", "b" }, 200);
                SendManyArray(env, "E3", new string[] { "a" }, 300);

                env.Milestone(0);

                AssertFAF(env, path, "arrayOne = {'a', 'b'} and value < 150", "E1");
                AssertFAF(env, path, "arrayOne = {'a', 'b'} and value > 150", "E2");
                AssertFAF(env, path, "arrayOne = {'a'} and value > 200", "E3");
                AssertFAFNot(env, path, "arrayOne = {'a'} and value > 400");
                AssertFAFNot(env, path, "arrayOne = {'a', 'c'} and value < 150");

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                string[] arrayOne,
                int value)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(arrayOne).WithValue(value));
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraSelectIndexMultikeyWArrayTwoField : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexMultikeyWArrayTwoField(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = namedWindow
                    ? "@public create window MyInfra#keepall as (Id string, arrayOne string[], arrayTwo string[]);\n"
                    : "@public create table MyInfra(Id string primary key, arrayOne string[], arrayTwo string[]);\n";
                epl +=
                    "insert into MyInfra select Id, StringOne as arrayOne, StringTwo as arrayTwo from SupportEventWithManyArray;\n" +
                    "create index MyInfraIndex on MyInfra(arrayOne, arrayTwo);\n";
                env.CompileDeploy(epl, path);

                SendManyArray(env, "E1", new string[] { "a", "b" }, new string[] { "c", "d" });
                SendManyArray(env, "E2", new string[] { "a" }, new string[] { "b" });

                env.Milestone(0);

                AssertFAF(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'd'}", "E1");
                AssertFAF(env, path, "arrayOne = {'a'} and arrayTwo = {'b'}", "E2");
                AssertFAFNot(env, path, "arrayOne = {'a', 'b', 'c'} and arrayTwo = {'c', 'd'}");
                AssertFAFNot(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'c'}");

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                string[] arrayOne,
                string[] arrayTwo)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(arrayOne).WithStringTwo(arrayTwo));
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraSelectIndexMultikeyWArray : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexMultikeyWArray(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = namedWindow
                    ? "@public create window MyInfra#keepall as (Id string, array string[]);\n"
                    : "@public create table MyInfra(Id string primary key, array string[]);\n";
                epl += "insert into MyInfra select Id, StringOne as array from SupportEventWithManyArray;\n" +
                       "create index MyInfraIndex on MyInfra(array);\n";
                env.CompileDeploy(epl, path);

                SendManyArray(env, "E1", new string[] { "a", "b" });
                SendManyArray(env, "E2", new string[] { "a" });
                SendManyArray(env, "E3", null);

                env.Milestone(0);

                AssertFAF(env, path, "array = {'a', 'b'}", "E1");
                AssertFAF(env, path, "array = {'a'}", "E2");
                AssertFAF(env, path, "array is null", "E3");
                AssertFAFNot(env, path, "array = {'b'}");

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                string[] strings)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(strings));
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class InfraSelectIndexChoiceJoin : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexChoiceJoin(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var preloadedEventsOne = new object[] {
                    new SupportSimpleBeanOne("E1", 10, 1, 2),
                    new SupportSimpleBeanOne("E2", 11, 3, 4),
                    new SupportSimpleBeanTwo("E1", 20, 1, 2),
                    new SupportSimpleBeanTwo("E2", 21, 3, 4),
                };
                IndexAssertionFAF fafAssertion = (result) => {
                    var fields = "W1.S1,W2.S2,W1.I1,W2.I2".SplitCsv();
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        result.Array,
                        fields,
                        new object[][] { new object[] { "E1", "E1", 10, 20 }, new object[] { "E2", "E2", 11, 21 } });
                };

                var assertionsSingleProp = new IndexAssertion[] {
                    new IndexAssertion(null, "S1 = S2", true, fafAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", true, fafAssertion),
                    new IndexAssertion(null, "L1 = L2 and S1 = S2", true, fafAssertion),
                    new IndexAssertion(null, "D1 = D2 and L1 = L2 and S1 = S2", true, fafAssertion),
                    new IndexAssertion(null, "D1 = D2 and L1 = L2", false, fafAssertion),
                };

                // single prop, no index, both declared unique (named window only)
                if (namedWindow) {
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        Array.Empty<string>(),
                        preloadedEventsOne,
                        "std:unique(S1)",
                        "std:unique(S2)",
                        assertionsSingleProp);
                }

                // single prop, unique indexes, both declared keepall
                var uniqueIndex = new string[]
                    { "create unique index W1I1 on W1(S1)", "create unique index W1I2 on W2(S2)" };
                AssertIndexChoiceJoin(
                    env,
                    namedWindow,
                    uniqueIndex,
                    preloadedEventsOne,
                    "win:keepall()",
                    "win:keepall()",
                    assertionsSingleProp);

                // single prop, mixed indexes, both declared keepall
                var assertionsMultiProp = new IndexAssertion[] {
                    new IndexAssertion(null, "S1 = S2", false, fafAssertion),
                    new IndexAssertion(null, "S1 = S2 and L1 = L2", true, fafAssertion),
                    new IndexAssertion(null, "L1 = L2 and S1 = S2", true, fafAssertion),
                    new IndexAssertion(null, "D1 = D2 and L1 = L2 and S1 = S2", true, fafAssertion),
                    new IndexAssertion(null, "D1 = D2 and L1 = L2", false, fafAssertion),
                };
                if (namedWindow) {
                    var mixedIndex = new string[]
                        { "create index W1I1 on W1(S1, L1)", "create unique index W1I2 on W2(S2)" };
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        mixedIndex,
                        preloadedEventsOne,
                        "std:unique(S1)",
                        "win:keepall()",
                        assertionsSingleProp);

                    // multi prop, no index, both declared unique
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        Array.Empty<string>(),
                        preloadedEventsOne,
                        "std:unique(S1, L1)",
                        "std:unique(S2, L2)",
                        assertionsMultiProp);
                }

                // multi prop, unique indexes, both declared keepall
                var uniqueIndexMulti = new string[]
                    { "create unique index W1I1 on W1(S1, L1)", "create unique index W1I2 on W2(S2, L2)" };
                AssertIndexChoiceJoin(
                    env,
                    namedWindow,
                    uniqueIndexMulti,
                    preloadedEventsOne,
                    "win:keepall()",
                    "win:keepall()",
                    assertionsMultiProp);

                // multi prop, mixed indexes, both declared keepall
                if (namedWindow) {
                    var mixedIndexMulti = new string[]
                        { "create index W1I1 on W1(S1)", "create unique index W1I2 on W2(S2, L2)" };
                    AssertIndexChoiceJoin(
                        env,
                        namedWindow,
                        mixedIndexMulti,
                        preloadedEventsOne,
                        "std:unique(S1, L1)",
                        "win:keepall()",
                        assertionsMultiProp);
                }
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }

            private static void AssertIndexChoiceJoin(
                RegressionEnvironment env,
                bool namedWindow,
                string[] indexes,
                object[] preloadedEvents,
                string datawindowOne,
                string datawindowTwo,
                params IndexAssertion[] assertions)
            {
                var path = new RegressionPath();
                if (namedWindow) {
                    env.CompileDeploy("@public create window W1." + datawindowOne + " as SupportSimpleBeanOne", path);
                    env.CompileDeploy("@public create window W2." + datawindowTwo + " as SupportSimpleBeanTwo", path);
                }
                else {
                    env.CompileDeploy(
                        "@public create table W1 (S1 String primary key, I1 int primary key, D1 double primary key, L1 long primary key)",
                        path);
                    env.CompileDeploy(
                        "@public create table W2 (S2 String primary key, I2 int primary key, D2 double primary key, L2 long primary key)",
                        path);
                }

                env.CompileDeploy("insert into W1 select S1,I1,D1,L1 from SupportSimpleBeanOne", path);
                env.CompileDeploy("insert into W2 select S2,I2,D2,L2 from SupportSimpleBeanTwo", path);

                foreach (var index in indexes) {
                    env.CompileDeploy(index, path);
                }

                foreach (var @event in preloadedEvents) {
                    env.SendEventBean(@event);
                }

                var count = 0;
                foreach (var assertion in assertions) {
                    Log.Info("======= Testing #" + count++);
                    var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                              (assertion.Hint ?? "") +
                              "select * from W1 as W1, W2 as W2 " +
                              "where " +
                              assertion.WhereClause;
                    EPFireAndForgetQueryResult result = null;
                    try {
                        result = env.CompileExecuteFAF(epl, path);
                    }
                    catch (Exception ex) {
                        Log.Error("Failed to process:" + ex.Message, ex);
                        if (assertion.EventSendAssertion == null) {
                            // no assertion, expected
                            ClassicAssert.IsTrue(ex.Message.Contains("index hint busted"));
                            continue;
                        }

                        throw new EPRuntimeException("Unexpected statement exception: " + ex.Message, ex);
                    }

                    // assert index and access
                    SupportQueryPlanIndexHook.AssertJoinAllStreamsAndReset(assertion.Unique);
                    assertion.FafAssertion.Invoke(result);
                }

                env.UndeployAll();
            }
        }

        private class InfraSelectIndexChoice : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectIndexChoice(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var preloadedEventsOne = new object[]
                    { new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22) };
                IndexAssertionFAF fafAssertion = (result) => {
                    var fields = "S1,I1".SplitCsv();
                    EPAssertionUtil.AssertPropsPerRow(
                        result.Array,
                        fields,
                        new object[][] { new object[] { "E2", 20 } });
                };

                // single index one field (plus declared unique)
                var noindexes = Array.Empty<string>();
                AssertIndexChoice(
                    env,
                    namedWindow,
                    noindexes,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "S1 = 'E2'", null, null, fafAssertion),
                        new IndexAssertion(null, "S1 = 'E2' and L1 = 22", null, null, fafAssertion),
                        new IndexAssertion("@Hint('index(One)')", "S1 = 'E2' and L1 = 22", null, null, fafAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "S1 = 'E2' and L1 = 22"), // should bust
                    });

                // single index one field (plus declared unique)
                var indexOneField = new string[] { "create unique index One on MyInfra (S1)" };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexOneField,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion[] {
                        new IndexAssertion(
                            null,
                            "S1 = 'E2'",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_UNIQUE,
                            fafAssertion),
                        new IndexAssertion(
                            null,
                            "S1 in ('E2')",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_UNIQUE,
                            fafAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_UNIQUE,
                            fafAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_UNIQUE,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "S1 = 'E2' and L1 = 22"), // should bust
                    });

                // single index two field (plus declared unique)
                var indexTwoField = new string[] { "create unique index One on MyInfra (S1, L1)" };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexTwoField,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion[] {
                        new IndexAssertion(null, "S1 = 'E2'", null, null, fafAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_MULTI_UNIQUE,
                            fafAssertion),
                    });

                // two index one unique (plus declared unique)
                var indexSetTwo = new string[] {
                    "create index One on MyInfra (S1)",
                    "create unique index Two on MyInfra (S1, D1)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetTwo,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion[] {
                        new IndexAssertion(
                            null,
                            "S1 = 'E2'",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion(
                            "@Hint('index(Two,One)')",
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "S1 = 'E2' and L1 = 22"), // busted
                        new IndexAssertion(
                            "@Hint('index(explicit,bust)')",
                            "S1 = 'E2' and L1 = 22",
                            "One",
                            IndexBackingTableInfo.BACKING_SINGLE_DUPS,
                            fafAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = 'E2' and D1 = 21 and L1 = 22",
                            "Two",
                            IndexBackingTableInfo.BACKING_MULTI_UNIQUE,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(explicit,bust)')", "D1 = 22 and L1 = 22"), // busted
                    });

                // range (unique)
                var indexSetThree = new string[] {
                    "create index One on MyInfra (L1 btree)",
                    "create index Two on MyInfra (D1 btree)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetThree,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new IndexAssertion[] {
                        new IndexAssertion(
                            null,
                            "L1 between 22 and 23",
                            "One",
                            IndexBackingTableInfo.BACKING_SORTED,
                            fafAssertion),
                        new IndexAssertion(
                            null,
                            "D1 between 21 and 22",
                            "Two",
                            IndexBackingTableInfo.BACKING_SORTED,
                            fafAssertion),
                        new IndexAssertion("@Hint('index(One, bust)')", "D1 between 21 and 22"), // busted
                    });
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            bool namedWindow,
            string[] indexes,
            object[] preloadedEvents,
            string datawindow,
            IndexAssertion[] assertions)
        {
            var path = new RegressionPath();
            var eplCreate = namedWindow
                ? "@public create window MyInfra." + datawindow + " as SupportSimpleBeanOne"
                : "@public create table MyInfra(S1 String primary key, I1 int primary key, D1 double primary key, L1 long primary key)";
            env.CompileDeploy(eplCreate, path);
            env.CompileDeploy("insert into MyInfra select S1,I1,D1,L1 from SupportSimpleBeanOne", path);
            foreach (var index in indexes) {
                env.CompileDeploy(index, path);
            }

            foreach (var @event in preloadedEvents) {
                env.SendEventBean(@event);
            }

            var count = 0;
            foreach (var assertion in assertions) {
                Log.Info("======= Testing #" + count++);
                var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
                          (assertion.Hint ?? "") +
                          "select * from MyInfra where " +
                          assertion.WhereClause;

                if (assertion.FafAssertion == null) {
                    try {
                        env.CompileExecuteFAF(epl, path);
                        Assert.Fail();
                    }
                    catch (Exception) {
                        // expected
                    }
                }
                else {
                    // assert index and access
                    var result = env.CompileExecuteFAF(epl, path);
                    SupportQueryPlanIndexHook.AssertFAFAndReset(
                        assertion.ExpectedIndexName,
                        assertion.IndexBackingClass);
                    assertion.FafAssertion.Invoke(result);
                }
            }

            env.UndeployAll();
        }

        private static void AssertFAF(
            RegressionEnvironment env,
            RegressionPath path,
            string epl,
            string expectedId)
        {
            var faf = "@Hint('index(MyInfraIndex, bust)') select * from MyInfra where " + epl;
            var result = env.CompileExecuteFAF(faf, path);
            ClassicAssert.AreEqual(1, result.Array.Length);
            ClassicAssert.AreEqual(expectedId, result.Array[0].Get("Id"));
        }

        private static void AssertFAFNot(
            RegressionEnvironment env,
            RegressionPath path,
            string epl)
        {
            var faf = "@Hint('index(MyInfraIndex, bust)') select * from MyInfra where " + epl;
            var result = env.CompileExecuteFAF(faf, path);
            ClassicAssert.AreEqual(0, result.Array.Length);
        }
    }
} // end of namespace