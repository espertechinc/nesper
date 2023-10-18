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

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
	public class InfraNWTableFAFIndex : IndexBackingTableInfo {
	    private static readonly ILog log = LogManager.GetLogger(typeof(InfraNWTableFAFIndex));

	    public static ICollection<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new InfraSelectIndexChoiceJoin(true));
	        execs.Add(new InfraSelectIndexChoiceJoin(false));
	        execs.Add(new InfraSelectIndexChoice(true));
	        execs.Add(new InfraSelectIndexChoice(false));
	        execs.Add(new InfraSelectIndexMultikeyWArray(true));
	        execs.Add(new InfraSelectIndexMultikeyWArray(false));
	        execs.Add(new InfraSelectIndexMultikeyWArrayTwoField(true));
	        execs.Add(new InfraSelectIndexMultikeyWArrayTwoField(false));
	        execs.Add(new InfraSelectIndexMultikeyWArrayCompositeArray(true));
	        execs.Add(new InfraSelectIndexMultikeyWArrayCompositeArray(false));
	        execs.Add(new InfraSelectIndexMultikeyWArrayCompositeTwoArray(true));
	        execs.Add(new InfraSelectIndexMultikeyWArrayCompositeTwoArray(false));
	        return execs;
	    }

	    private class InfraSelectIndexMultikeyWArrayCompositeTwoArray : RegressionExecution {
	        private readonly bool namedWindow;

	        public InfraSelectIndexMultikeyWArrayCompositeTwoArray(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = namedWindow ?
	                "@public create window MyInfra#keepall as (id string, arrayOne string[], arrayTwo string[], value int);\n" :
	                "@public create table MyInfra(id string primary key, arrayOne string[], arrayTwo string[], value int);\n";
	            epl += "insert into MyInfra select id, stringOne as arrayOne, stringTwo as arrayTwo, value from SupportEventWithManyArray;\n" +
	                "create index MyInfraIndex on MyInfra(arrayOne, arrayTwo, value btree);\n";
	            env.CompileDeploy(epl, path);

	            SendManyArray(env, "E1", new string[] {"a", "b"}, new string[] {"c", "d"}, 100);
	            SendManyArray(env, "E2", new string[] {"a", "b"}, new string[] {"e", "f"}, 200);
	            SendManyArray(env, "E3", new string[] {"a"}, new string[] {"b"}, 300);

	            env.Milestone(0);

	            AssertFAF(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'e', 'f'} and value > 150", "E2");
	            AssertFAF(env, path, "arrayOne = {'a'} and arrayTwo = {'b'} and value > 150", "E3");
	            AssertFAF(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'd'} and value > 90", "E1");
	            AssertFAFNot(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'd'} and value > 200");
	            AssertFAFNot(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'e'} and value > 90");
	            AssertFAFNot(env, path, "arrayOne = {'ax', 'b'} and arrayTwo = {'c', 'd'} and value > 90");

	            env.UndeployAll();
	        }

	        private void SendManyArray(RegressionEnvironment env, string id, string[] arrayOne, string[] arrayTwo, int value) {
	            env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(arrayOne).WithStringTwo(arrayTwo).WithValue(value));
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "namedWindow=" + namedWindow +
	                '}';
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private class InfraSelectIndexMultikeyWArrayCompositeArray : RegressionExecution {
	        private readonly bool namedWindow;

	        public InfraSelectIndexMultikeyWArrayCompositeArray(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = namedWindow ?
	                "@public create window MyInfra#keepall as (id string, arrayOne string[], value int);\n" :
	                "@public create table MyInfra(id string primary key, arrayOne string[], value int);\n";
	            epl += "insert into MyInfra select id, stringOne as arrayOne, value from SupportEventWithManyArray;\n" +
	                "create index MyInfraIndex on MyInfra(arrayOne, value btree);\n";
	            env.CompileDeploy(epl, path);

	            SendManyArray(env, "E1", new string[] {"a", "b"}, 100);
	            SendManyArray(env, "E2", new string[] {"a", "b"}, 200);
	            SendManyArray(env, "E3", new string[] {"a"}, 300);

	            env.Milestone(0);

	            AssertFAF(env, path, "arrayOne = {'a', 'b'} and value < 150", "E1");
	            AssertFAF(env, path, "arrayOne = {'a', 'b'} and value > 150", "E2");
	            AssertFAF(env, path, "arrayOne = {'a'} and value > 200", "E3");
	            AssertFAFNot(env, path, "arrayOne = {'a'} and value > 400");
	            AssertFAFNot(env, path, "arrayOne = {'a', 'c'} and value < 150");

	            env.UndeployAll();
	        }

	        private void SendManyArray(RegressionEnvironment env, string id, string[] arrayOne, int value) {
	            env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(arrayOne).WithValue(value));
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "namedWindow=" + namedWindow +
	                '}';
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private class InfraSelectIndexMultikeyWArrayTwoField : RegressionExecution {
	        private readonly bool namedWindow;

	        public InfraSelectIndexMultikeyWArrayTwoField(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = namedWindow ?
	                "@public create window MyInfra#keepall as (id string, arrayOne string[], arrayTwo string[]);\n" :
	                "@public create table MyInfra(id string primary key, arrayOne string[], arrayTwo string[]);\n";
	            epl += "insert into MyInfra select id, stringOne as arrayOne, stringTwo as arrayTwo from SupportEventWithManyArray;\n" +
	                "create index MyInfraIndex on MyInfra(arrayOne, arrayTwo);\n";
	            env.CompileDeploy(epl, path);

	            SendManyArray(env, "E1", new string[] {"a", "b"}, new string[] {"c", "d"});
	            SendManyArray(env, "E2", new string[] {"a"}, new string[] {"b"});

	            env.Milestone(0);

	            AssertFAF(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'd'}", "E1");
	            AssertFAF(env, path, "arrayOne = {'a'} and arrayTwo = {'b'}", "E2");
	            AssertFAFNot(env, path, "arrayOne = {'a', 'b', 'c'} and arrayTwo = {'c', 'd'}");
	            AssertFAFNot(env, path, "arrayOne = {'a', 'b'} and arrayTwo = {'c', 'c'}");

	            env.UndeployAll();
	        }

	        private void SendManyArray(RegressionEnvironment env, string id, string[] arrayOne, string[] arrayTwo) {
	            env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(arrayOne).WithStringTwo(arrayTwo));
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "namedWindow=" + namedWindow +
	                '}';
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private class InfraSelectIndexMultikeyWArray : RegressionExecution {
	        private readonly bool namedWindow;

	        public InfraSelectIndexMultikeyWArray(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = namedWindow ?
	                "@public create window MyInfra#keepall as (id string, array string[]);\n" :
	                "@public create table MyInfra(id string primary key, array string[]);\n";
	            epl += "insert into MyInfra select id, stringOne as array from SupportEventWithManyArray;\n" +
	                   "create index MyInfraIndex on MyInfra(array);\n";
	            env.CompileDeploy(epl, path);

	            SendManyArray(env, "E1", new string[] {"a", "b"});
	            SendManyArray(env, "E2", new string[] {"a"});
	            SendManyArray(env, "E3", null);

	            env.Milestone(0);

	            AssertFAF(env, path, "array = {'a', 'b'}", "E1");
	            AssertFAF(env, path, "array = {'a'}", "E2");
	            AssertFAF(env, path, "array is null", "E3");
	            AssertFAFNot(env, path, "array = {'b'}");

	            env.UndeployAll();
	        }

	        private void SendManyArray(RegressionEnvironment env, string id, string[] strings) {
	            env.SendEventBean(new SupportEventWithManyArray(id).WithStringOne(strings));
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "namedWindow=" + namedWindow +
	                '}';
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private class InfraSelectIndexChoiceJoin : RegressionExecution {
	        private readonly bool namedWindow;

	        public InfraSelectIndexChoiceJoin(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {

	            var preloadedEventsOne = new object[]{
	                new SupportSimpleBeanOne("E1", 10, 1, 2),
	                new SupportSimpleBeanOne("E2", 11, 3, 4),
	                new SupportSimpleBeanTwo("E1", 20, 1, 2),
	                new SupportSimpleBeanTwo("E2", 21, 3, 4),
	            };
	            IndexAssertionFAF fafAssertion = (result) =>  {
                    var fields = "w1.s1,w2.s2,w1.i1,w2.i2".SplitCsv();
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields,
                        new object[][]{new object[] {"E1", "E1", 10, 20}, new object[] {"E2", "E2", 11, 21}});
	            };

	            var assertionsSingleProp = new IndexAssertion[]{
	                new IndexAssertion(null, "s1 = s2", true, fafAssertion),
	                new IndexAssertion(null, "s1 = s2 and l1 = l2", true, fafAssertion),
	                new IndexAssertion(null, "l1 = l2 and s1 = s2", true, fafAssertion),
	                new IndexAssertion(null, "d1 = d2 and l1 = l2 and s1 = s2", true, fafAssertion),
	                new IndexAssertion(null, "d1 = d2 and l1 = l2", false, fafAssertion),
	            };

	            // single prop, no index, both declared unique (named window only)
	            if (namedWindow) {
	                AssertIndexChoiceJoin(env, namedWindow, Array.Empty<string>(), preloadedEventsOne, "std:unique(s1)", "std:unique(s2)", assertionsSingleProp);
	            }

	            // single prop, unique indexes, both declared keepall
	            var uniqueIndex = new string[]{"create unique index W1I1 on W1(s1)", "create unique index W1I2 on W2(s2)"};
	            AssertIndexChoiceJoin(env, namedWindow, uniqueIndex, preloadedEventsOne, "win:keepall()", "win:keepall()", assertionsSingleProp);

	            // single prop, mixed indexes, both declared keepall
	            var assertionsMultiProp = new IndexAssertion[]{
	                new IndexAssertion(null, "s1 = s2", false, fafAssertion),
	                new IndexAssertion(null, "s1 = s2 and l1 = l2", true, fafAssertion),
	                new IndexAssertion(null, "l1 = l2 and s1 = s2", true, fafAssertion),
	                new IndexAssertion(null, "d1 = d2 and l1 = l2 and s1 = s2", true, fafAssertion),
	                new IndexAssertion(null, "d1 = d2 and l1 = l2", false, fafAssertion),
	            };
	            if (namedWindow) {
	                var mixedIndex = new string[]{"create index W1I1 on W1(s1, l1)", "create unique index W1I2 on W2(s2)"};
	                AssertIndexChoiceJoin(env, namedWindow, mixedIndex, preloadedEventsOne, "std:unique(s1)", "win:keepall()", assertionsSingleProp);

	                // multi prop, no index, both declared unique
	                AssertIndexChoiceJoin(env, namedWindow, Array.Empty<string>(), preloadedEventsOne, "std:unique(s1, l1)", "std:unique(s2, l2)", assertionsMultiProp);
	            }

	            // multi prop, unique indexes, both declared keepall
	            var uniqueIndexMulti = new string[]{"create unique index W1I1 on W1(s1, l1)", "create unique index W1I2 on W2(s2, l2)"};
	            AssertIndexChoiceJoin(env, namedWindow, uniqueIndexMulti, preloadedEventsOne, "win:keepall()", "win:keepall()", assertionsMultiProp);

	            // multi prop, mixed indexes, both declared keepall
	            if (namedWindow) {
	                var mixedIndexMulti = new string[]{"create index W1I1 on W1(s1)", "create unique index W1I2 on W2(s2, l2)"};
	                AssertIndexChoiceJoin(env, namedWindow, mixedIndexMulti, preloadedEventsOne, "std:unique(s1, l1)", "win:keepall()", assertionsMultiProp);
	            }
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "namedWindow=" + namedWindow +
	                '}';
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }

	        private static void AssertIndexChoiceJoin(RegressionEnvironment env, bool namedWindow, string[] indexes, object[] preloadedEvents, string datawindowOne, string datawindowTwo,
	                                                  params IndexAssertion[] assertions) {

	            var path = new RegressionPath();
	            if (namedWindow) {
	                env.CompileDeploy("@public create window W1." + datawindowOne + " as SupportSimpleBeanOne", path);
	                env.CompileDeploy("@public create window W2." + datawindowTwo + " as SupportSimpleBeanTwo", path);
	            } else {
	                env.CompileDeploy("@public create table W1 (s1 String primary key, i1 int primary key, d1 double primary key, l1 long primary key)", path);
	                env.CompileDeploy("@public create table W2 (s2 String primary key, i2 int primary key, d2 double primary key, l2 long primary key)", path);
	            }
	            env.CompileDeploy("insert into W1 select s1,i1,d1,l1 from SupportSimpleBeanOne", path);
	            env.CompileDeploy("insert into W2 select s2,i2,d2,l2 from SupportSimpleBeanTwo", path);

	            foreach (var index in indexes) {
	                env.CompileDeploy(index, path);
	            }
	            foreach (var @event in preloadedEvents) {
	                env.SendEventBean(@event);
	            }

	            var count = 0;
	            foreach (var assertion in assertions) {
	                log.Info("======= Testing #" + count++);
	                var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
	                          (assertion.Hint ?? "") +
	                          "select * from W1 as w1, W2 as w2 " +
	                          "where " + assertion.WhereClause;
	                EPFireAndForgetQueryResult result = null;
	                try {
	                    result = env.CompileExecuteFAF(epl, path);
	                } catch (Exception ex) {
	                    log.Error("Failed to process:" + ex.Message, ex);
	                    if (assertion.EventSendAssertion == null) {
	                        // no assertion, expected
	                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
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

	    private class InfraSelectIndexChoice : RegressionExecution {
	        private readonly bool namedWindow;

	        public InfraSelectIndexChoice(bool namedWindow) {
	            this.namedWindow = namedWindow;
	        }

	        public void Run(RegressionEnvironment env) {
	            var preloadedEventsOne = new object[]{new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
	            IndexAssertionFAF fafAssertion = (result) =>  {
                    var fields = "s1,i1".SplitCsv();
                    EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]{new object[] {"E2", 20}});
	            };

	            // single index one field (plus declared unique)
	            var noindexes = Array.Empty<string>();
	            AssertIndexChoice(env, namedWindow, noindexes, preloadedEventsOne, "std:unique(s1)",
	                new IndexAssertion[]{
	                    new IndexAssertion(null, "s1 = 'E2'", null, null, fafAssertion),
	                    new IndexAssertion(null, "s1 = 'E2' and l1 = 22", null, null, fafAssertion),
	                    new IndexAssertion("@Hint('index(One)')", "s1 = 'E2' and l1 = 22", null, null, fafAssertion),
	                    new IndexAssertion("@Hint('index(Two,bust)')", "s1 = 'E2' and l1 = 22"), // should bust
	                });

	            // single index one field (plus declared unique)
	            var indexOneField = new string[]{"create unique index One on MyInfra (s1)"};
	            AssertIndexChoice(env, namedWindow, indexOneField, preloadedEventsOne, "std:unique(s1)",
	                new IndexAssertion[]{
	                    new IndexAssertion(null, "s1 = 'E2'", "One", IndexBackingTableInfo.BACKING_SINGLE_UNIQUE, fafAssertion),
	                    new IndexAssertion(null, "s1 in ('E2')", "One", IndexBackingTableInfo.BACKING_SINGLE_UNIQUE, fafAssertion),
	                    new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_SINGLE_UNIQUE, fafAssertion),
	                    new IndexAssertion("@Hint('index(One)')", "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_SINGLE_UNIQUE, fafAssertion),
	                    new IndexAssertion("@Hint('index(Two,bust)')", "s1 = 'E2' and l1 = 22"), // should bust
	                });

	            // single index two field (plus declared unique)
	            var indexTwoField = new string[]{"create unique index One on MyInfra (s1, l1)"};
	            AssertIndexChoice(env, namedWindow, indexTwoField, preloadedEventsOne, "std:unique(s1)",
	                new IndexAssertion[]{
	                    new IndexAssertion(null, "s1 = 'E2'", null, null, fafAssertion),
	                    new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_MULTI_UNIQUE, fafAssertion),
	                });

	            // two index one unique (plus declared unique)
	            var indexSetTwo = new string[]{
	                "create index One on MyInfra (s1)",
	                "create unique index Two on MyInfra (s1, d1)"};
	            AssertIndexChoice(env, namedWindow, indexSetTwo, preloadedEventsOne, "std:unique(s1)",
	                new IndexAssertion[]{
	                    new IndexAssertion(null, "s1 = 'E2'", "One", IndexBackingTableInfo.BACKING_SINGLE_DUPS, fafAssertion),
	                    new IndexAssertion(null, "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_SINGLE_DUPS, fafAssertion),
	                    new IndexAssertion("@Hint('index(One)')", "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_SINGLE_DUPS, fafAssertion),
	                    new IndexAssertion("@Hint('index(Two,One)')", "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_SINGLE_DUPS, fafAssertion),
	                    new IndexAssertion("@Hint('index(Two,bust)')", "s1 = 'E2' and l1 = 22"),  // busted
	                    new IndexAssertion("@Hint('index(explicit,bust)')", "s1 = 'E2' and l1 = 22", "One", IndexBackingTableInfo.BACKING_SINGLE_DUPS, fafAssertion),
	                    new IndexAssertion(null, "s1 = 'E2' and d1 = 21 and l1 = 22", "Two", IndexBackingTableInfo.BACKING_MULTI_UNIQUE, fafAssertion),
	                    new IndexAssertion("@Hint('index(explicit,bust)')", "d1 = 22 and l1 = 22"),   // busted
	                });

	            // range (unique)
	            var indexSetThree = new string[]{
	                "create index One on MyInfra (l1 btree)",
	                "create index Two on MyInfra (d1 btree)"};
	            AssertIndexChoice(env, namedWindow, indexSetThree, preloadedEventsOne, "std:unique(s1)",
	                new IndexAssertion[]{
	                    new IndexAssertion(null, "l1 between 22 and 23", "One", IndexBackingTableInfo.BACKING_SORTED, fafAssertion),
	                    new IndexAssertion(null, "d1 between 21 and 22", "Two", IndexBackingTableInfo.BACKING_SORTED, fafAssertion),
	                    new IndexAssertion("@Hint('index(One, bust)')", "d1 between 21 and 22"), // busted
	                });
	        }

	        public string Name() {
	            return this.GetType().Name + "{" +
	                "namedWindow=" + namedWindow +
	                '}';
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    private static void AssertIndexChoice(RegressionEnvironment env, bool namedWindow, string[] indexes, object[] preloadedEvents, string datawindow,
	                                          IndexAssertion[] assertions) {
	        var path = new RegressionPath();
	        var eplCreate = namedWindow ?
	            "@public create window MyInfra." + datawindow + " as SupportSimpleBeanOne" :
	            "@public create table MyInfra(s1 String primary key, i1 int primary key, d1 double primary key, l1 long primary key)";
	        env.CompileDeploy(eplCreate, path);
	        env.CompileDeploy("insert into MyInfra select s1,i1,d1,l1 from SupportSimpleBeanOne", path);
	        foreach (var index in indexes) {
	            env.CompileDeploy(index, path);
	        }
	        foreach (var @event in preloadedEvents) {
	            env.SendEventBean(@event);
	        }

	        var count = 0;
	        foreach (var assertion in assertions) {
	            log.Info("======= Testing #" + count++);
	            var epl = IndexBackingTableInfo.INDEX_CALLBACK_HOOK +
	                      (assertion.Hint ?? "") +
	                      "select * from MyInfra where " + assertion.WhereClause;

	            if (assertion.FafAssertion == null) {
	                try {
	                    env.CompileExecuteFAF(epl, path);
	                    Assert.Fail();
	                } catch (Exception) {
	                    // expected
	                }
	            } else {
	                // assert index and access
	                var result = env.CompileExecuteFAF(epl, path);
	                SupportQueryPlanIndexHook.AssertFAFAndReset(assertion.ExpectedIndexName, assertion.IndexBackingClass);
	                assertion.FafAssertion.Invoke(result);
	            }
	        }

	        env.UndeployAll();
	    }

	    private static void AssertFAF(RegressionEnvironment env, RegressionPath path, string epl, string expectedId) {
	        var faf = "@Hint('index(MyInfraIndex, bust)') select * from MyInfra where " + epl;
	        var result = env.CompileExecuteFAF(faf, path);
	        Assert.AreEqual(1, result.Array.Length);
	        Assert.AreEqual(expectedId, result.Array[0].Get("id"));
	    }

	    private static void AssertFAFNot(RegressionEnvironment env, RegressionPath path, string epl) {
	        var faf = "@Hint('index(MyInfraIndex, bust)') select * from MyInfra where " + epl;
	        var result = env.CompileExecuteFAF(faf, path);
	        Assert.AreEqual(0, result.Array.Length);
	    }
	}
} // end of namespace
