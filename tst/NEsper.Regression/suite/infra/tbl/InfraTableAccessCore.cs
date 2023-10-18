///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
	/// <summary>
	/// NOTE: More table-related tests in "nwtable"
	/// </summary>
	public class InfraTableAccessCore {

	    public static ICollection<RegressionExecution> Executions() {
	        var execs = new List<RegressionExecution>();
	        execs.Add(new InfraTableAccessCoreUnGroupedWindowAndSum());
	        execs.Add(new InfraIntegerIndexedPropertyLookAlike());
	        execs.Add(new InfraFilterBehavior());
	        execs.Add(new InfraExprSelectClauseRenderingUnnamedCol());
	        execs.Add(new InfraTopLevelReadGrouped2Keys());
	        execs.Add(new InfraTopLevelReadUnGrouped());
	        execs.Add(new InfraExpressionAliasAndDecl());
	        execs.Add(new InfraGroupedTwoKeyNoContext());
	        execs.Add(new InfraGroupedThreeKeyNoContext());
	        execs.Add(new InfraGroupedSingleKeyNoContext());
	        execs.Add(new InfraUngroupedWContext());
	        execs.Add(new InfraOrderOfAggregationsAndPush());
	        execs.Add(new InfraMultiStmtContributing());
	        execs.Add(new InfraGroupedMixedMethodAndAccess());
	        execs.Add(new InfraNamedWindowAndFireAndForget());
	        execs.Add(new InfraSubquery());
	        execs.Add(new InfraOnMergeExpressions());
	        execs.Add(new InfraTableAccessCoreSplitStream());
	        execs.Add(new InfraTableAccessMultikeyWArrayOneArrayKey());
	        execs.Add(new InfraTableAccessMultikeyWArrayTwoArrayKey());
	        return execs;
	    }

	    internal class InfraTableAccessMultikeyWArrayTwoArrayKey : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "create table MyTable(k1 int[primitive] primary key, k2 int[primitive] primary key, value int);\n" +
	                      "insert into MyTable select intOne as k1, intTwo as k2, value from SupportEventWithManyArray(id = 'I');\n" +
	                      "@name('s0') select MyTable[intOne, intTwo].value as c0 from SupportEventWithManyArray(id = 'Q');\n" +
	                      "@name('s1') select MyTable.keys() as keys from SupportBean;\n";
	            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

	            SendManyArrayI(env, new int[] {1, 2}, new int[] {1, 2}, 10);
	            SendManyArrayI(env, new int[] {1, 3}, new int[] {1, 1}, 20);
	            SendManyArrayI(env, new int[] {1, 2}, new int[] {1, 1}, 30);

	            env.Milestone(0);

	            SendManyArrayQAssert(env, new int[] {1, 2}, new int[] {1, 2}, 10);
	            SendManyArrayQAssert(env, new int[] {1, 2}, new int[] {1, 1}, 30);
	            SendManyArrayQAssert(env, new int[] {1, 3}, new int[] {1, 1}, 20);
	            SendManyArrayQAssert(env, new int[] {1, 2}, new int[] {1, 2, 2}, null);

	            env.SendEventBean(new SupportBean());
	            env.AssertEventNew("s1", @event => {
	                var keys = (object[]) @event.Get("keys");
	                EPAssertionUtil.AssertEqualsAnyOrder(keys, new object[] {
	                    new object[] {new int[] {1, 2}, new int[] {1, 2}},
	                    new object[] {new int[] {1, 3}, new int[] {1, 1}},
	                    new object[] {new int[] {1, 2}, new int[] {1, 1}},
	                });
	            });

	            env.UndeployAll();
	        }

	        private void SendManyArrayQAssert(RegressionEnvironment env, int[] arrayOne, int[] arrayTwo, int? expected) {
	            env.SendEventBean(new SupportEventWithManyArray("Q").WithIntOne(arrayOne).WithIntTwo(arrayTwo));
	            env.AssertEqualsNew("s0", "c0", expected);
	        }

	        private void SendManyArrayI(RegressionEnvironment env, int[] arrayOne, int[] arrayTwo, int value) {
	            env.SendEventBean(new SupportEventWithManyArray("I").WithIntOne(arrayOne).WithIntTwo(arrayTwo).WithValue(value));
	        }
	    }

	    internal class InfraTableAccessMultikeyWArrayOneArrayKey : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "create table MyTable(k int[primitive] primary key, value int);\n" +
	                      "insert into MyTable select intOne as k, value from SupportEventWithManyArray(id = 'I');\n" +
	                      "@name('s0') select MyTable[intOne].value as c0 from SupportEventWithManyArray(id = 'Q');\n" +
	                      "@name('s1') select MyTable.keys() as keys from SupportBean;\n";
	            env.CompileDeploy(epl).AddListener("s0").AddListener("s1");

	            SendManyArrayI(env, new int[] {1, 2}, 10);
	            SendManyArrayI(env, new int[] {2, 1}, 20);
	            SendManyArrayI(env, new int[] {1, 2, 1}, 30);

	            env.Milestone(0);

	            SendManyArrayQAssert(env, new int[] {1, 2}, 10);
	            SendManyArrayQAssert(env, new int[] {1, 2, 1}, 30);
	            SendManyArrayQAssert(env, new int[] {2, 1}, 20);
	            SendManyArrayQAssert(env, new int[] {1, 2, 2}, null);

	            env.SendEventBean(new SupportBean());
	            env.AssertEventNew("s1", @event => {
	                var keys = (object[]) @event.Get("keys");
	                EPAssertionUtil.AssertEqualsAnyOrder(keys, new object[] {new int[] {2, 1}, new int[] {1, 2}, new int[] {1, 2, 1}});
	            });

	            env.UndeployAll();
	        }

	        private void SendManyArrayQAssert(RegressionEnvironment env, int[] arrayOne, int? expected) {
	            env.SendEventBean(new SupportEventWithManyArray("Q").WithIntOne(arrayOne));
	            env.AssertEqualsNew("s0", "c0", expected);
	        }

	        private void SendManyArrayI(RegressionEnvironment env, int[] arrayOne, int value) {
	            env.SendEventBean(new SupportEventWithManyArray("I").WithIntOne(arrayOne).WithValue(value));
	        }
	    }

	    internal class InfraIntegerIndexedPropertyLookAlike : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            TryAssertionIntegerIndexedPropertyLookAlike(env, false, milestone);
	            TryAssertionIntegerIndexedPropertyLookAlike(env, true, milestone);
	        }

	        private static void TryAssertionIntegerIndexedPropertyLookAlike(RegressionEnvironment env, bool soda, AtomicLong milestone) {
	            var path = new RegressionPath();
	            var eplDeclare = "@name('infra') @public create table varaggIIP (key int primary key, myevents window(*) @type('SupportBean'))";
	            env.CompileDeploy(soda, eplDeclare, path);
	            env.AssertStatement("infra", statement =>{
	                Assert.AreEqual(StatementType.CREATE_TABLE, statement.GetProperty(StatementProperty.STATEMENTTYPE));
	                Assert.AreEqual("varaggIIP", statement.GetProperty(StatementProperty.CREATEOBJECTNAME));
	            });

	            var eplInto = "into table varaggIIP select window(*) as myevents from SupportBean#length(3) group by intPrimitive";
	            env.CompileDeploy(soda, eplInto, path);

	            var eplSelect = "@name('s0') select varaggIIP[1] as c0, varaggIIP[1].myevents as c1, varaggIIP[1].myevents.last(*) as c2, varaggIIP[1].myevents.last(*,1) as c3 from SupportBean_S0";
	            env.CompileDeploy(soda, eplSelect, path).AddListener("s0");

	            var e1 = MakeSendBean(env, "E1", 1, 10L);
	            var e2 = MakeSendBean(env, "E2", 1, 20L);

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(0));
	            env.AssertEventNew("s0", @event => AssertIntegerIndexed(@event, new SupportBean[]{e1, e2}));

	            env.UndeployAll();
	        }
	    }

	    internal class InfraFilterBehavior : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table varaggFB (total count(*))", path);
	            env.CompileDeploy("into table varaggFB select count(*) as total from SupportBean_S0", path);
	            env.CompileDeploy("@name('s0') select * from SupportBean(varaggFB.total = intPrimitive)", path).AddListener("s0");

	            env.SendEventBean(new SupportBean_S0(0));

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertListenerInvoked("s0");

	            env.SendEventBean(new SupportBean_S0(0));

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean("E1", 2));
	            env.AssertListenerInvoked("s0");

	            env.SendEventBean(new SupportBean("E1", 3));
	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertListenerNotInvoked("s0");

	            env.UndeployAll();
	        }
	    }

	    internal class InfraExprSelectClauseRenderingUnnamedCol : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table varaggESC (" +
	                "key string primary key, theEvents window(*) @type(SupportBean))", path);

	            env.CompileDeploy("@name('s0') select " +
	                "varaggESC.keys()," +
	                "varaggESC[p00].theEvents," +
	                "varaggESC[p00]," +
	                "varaggESC[p00].theEvents.last(*)," +
	                "varaggESC[p00].theEvents.window(*).take(1) from SupportBean_S0", path);

	            var expectedAggType = new object[][]{
		            new object[] {"varaggESC.keys()", typeof(object[])},
	                new object[] {"varaggESC[p00].theEvents", typeof(SupportBean[])},
	                new object[] {"varaggESC[p00]", typeof(IDictionary<string, object>)},
	                new object[] {"varaggESC[p00].theEvents.last(*)", typeof(SupportBean)},
	                new object[] {"varaggESC[p00].theEvents.window(*).take(1)", typeof(ICollection<object>)},
	            };
	            env.AssertStatement("s0", statement =>{
	                var eventType = statement.EventType;
	                SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, eventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
	            });
	            env.UndeployAll();
	        }
	    }

	    internal class InfraTopLevelReadGrouped2Keys : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            TryAssertionTopLevelReadGrouped2Keys(env, false, milestone);
	            TryAssertionTopLevelReadGrouped2Keys(env, true, milestone);
	        }

	        private static void TryAssertionTopLevelReadGrouped2Keys(RegressionEnvironment env, bool soda, AtomicLong milestone) {
	            var path = new RegressionPath();
	            var typeCompiled = env.Compile("@buseventtype @public create objectarray schema MyEventOA as (c0 int, c1 string, c2 int)");
	            env.Deploy(typeCompiled);
	            path.Add(typeCompiled);

	            env.CompileDeploy(soda, "@public create table windowAndTotalTLP2K (" +
	                "keyi int primary key, keys string primary key, thewindow window(*) @type('MyEventOA'), thetotal sum(int))", path);
	            env.CompileDeploy(soda, "into table windowAndTotalTLP2K " +
	                "select window(*) as thewindow, sum(c2) as thetotal from MyEventOA#length(2) group by c0, c1", path);

	            env.CompileDeploy(soda, "@name('s0') select windowAndTotalTLP2K[id,p00] as val0 from SupportBean_S0", path).AddListener("s0");
	            env.AssertStatement("s0", statement =>AssertTopLevelTypeInfo(statement));

	            var e1 = new object[]{10, "G1", 100};
	            env.SendEventObjectArray(e1, "MyEventOA");

	            var fieldsInner = "thewindow,thetotal".SplitCsv();
	            env.SendEventBean(new SupportBean_S0(10, "G1"));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e1}, 100));

	            var e2 = new object[]{20, "G2", 200};
	            env.SendEventObjectArray(e2, "MyEventOA");

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(20, "G2"));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e2}, 200));

	            var e3 = new object[]{20, "G2", 300};
	            env.SendEventObjectArray(e3, "MyEventOA");

	            env.SendEventBean(new SupportBean_S0(10, "G1"));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, null, null));
	            env.SendEventBean(new SupportBean_S0(20, "G2"));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e2, e3}, 500));

	            // test typable output
	            env.UndeployModuleContaining("s0");
	            env.CompileDeploy("@name('i1') insert into OutStream select windowAndTotalTLP2K[20, 'G2'] as val0 from SupportBean_S0", path);
	            env.AddListener("i1");

	            env.SendEventBean(new SupportBean_S0(0));
	            env.AssertPropsNew("i1", "val0.thewindow,val0.thetotal".SplitCsv(), new object[]{new object[][]{e2, e3}, 500});

	            env.UndeployAll();
	        }
	    }

	    internal class InfraTopLevelReadUnGrouped : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var e1 = new object[]{10};
	            var e2 = new object[]{20};
	            var e3 = new object[]{30};

	            var path = new RegressionPath();
	            var typeCompiled = env.Compile("@public @buseventtype create objectarray schema MyEventOATLRU(c0 int)");
	            env.Deploy(typeCompiled);
	            path.Add(typeCompiled);

	            env.CompileDeploy("@public create table windowAndTotalTLRUG (" +
	                "thewindow window(*) @type(MyEventOATLRU), thetotal sum(int))", path);
	            env.CompileDeploy("into table windowAndTotalTLRUG " +
	                "select window(*) as thewindow, sum(c0) as thetotal from MyEventOATLRU#length(2)", path);

	            env.CompileDeploy("@name('s0') select windowAndTotalTLRUG as val0 from SupportBean_S0", path);
	            env.AddListener("s0");

	            env.SendEventObjectArray(e1, "MyEventOATLRU");

	            var fieldsInner = "thewindow,thetotal".SplitCsv();
	            env.SendEventBean(new SupportBean_S0(0));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e1}, 10));

	            env.SendEventObjectArray(e2, "MyEventOATLRU");

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e1, e2}, 30));

	            env.SendEventObjectArray(e3, "MyEventOATLRU");

	            env.SendEventBean(new SupportBean_S0(2));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e2, e3}, 50));

	            // test typable output
	            env.UndeployModuleContaining("s0");

	            env.CompileDeploy("create schema AggBean as " + typeof(AggBean).FullName + ";\n" +
	                "@name('s0') insert into AggBean select windowAndTotalTLRUG as val0 from SupportBean_S0;\n", path).AddListener("s0");

	            env.SendEventBean(new SupportBean_S0(2));
	            env.AssertPropsNew("s0", "val0.thewindow,val0.thetotal".SplitCsv(), new object[]{new object[][]{e2, e3}, 50});

	            env.UndeployAll();
	        }
	    }

	    internal class InfraExpressionAliasAndDecl : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            TryAssertionIntoTableFromExpression(env, milestone);

	            TryAssertionExpressionHasTableAccess(env, milestone);

	            TryAssertionSubqueryWithExpressionHasTableAccess(env, milestone);
	        }

	        private static void TryAssertionSubqueryWithExpressionHasTableAccess(RegressionEnvironment env, AtomicLong milestone) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table MyTableTwo(theString string primary key, intPrimitive int)", path);
	            env.CompileDeploy("@public create expression getMyValue{o => (select MyTableTwo[o.p00].intPrimitive from SupportBean_S1#lastevent)}", path);
	            env.CompileDeploy("insert into MyTableTwo select theString, intPrimitive from SupportBean", path);
	            env.CompileDeploy("@name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0", path).AddListener("s0");

	            env.SendEventBean(new SupportBean_S1(1000));
	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E2", 2));

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(0, "E2"));
	            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[]{2});

	            env.UndeployAll();
	        }

	        private static void TryAssertionExpressionHasTableAccess(RegressionEnvironment env, AtomicLong milestone) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table MyTableOne(theString string primary key, intPrimitive int)", path);
	            env.CompileDeploy("@public create expression getMyValue{o => MyTableOne[o.p00].intPrimitive}", path);
	            env.CompileDeploy("insert into MyTableOne select theString, intPrimitive from SupportBean", path);
	            env.CompileDeploy("@name('s0') select getMyValue(s0) as c0 from SupportBean_S0 as s0", path).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.SendEventBean(new SupportBean("E2", 2));

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(0, "E2"));
	            env.AssertPropsNew("s0", "c0".SplitCsv(), new object[]{2});

	            env.UndeployAll();
	        }

	        private static void TryAssertionIntoTableFromExpression(RegressionEnvironment env, AtomicLong milestone) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create expression sumi {a -> sum(intPrimitive)}", path);
	            env.CompileDeploy("@public create expression sumd alias for {sum(doublePrimitive)}", path);
	            env.CompileDeploy("@public create table varaggITFE (" +
	                "sumi sum(int), sumd sum(double), sumf sum(float), suml sum(long))", path);
	            env.CompileDeploy("expression suml alias for {sum(longPrimitive)} " +
	                "into table varaggITFE " +
	                "select suml, sum(floatPrimitive) as sumf, sumd, sumi(sb) from SupportBean as sb", path);

	            MakeSendBean(env, "E1", 10, 100L, 1000d, 10000f);

	            var fields = "varaggITFE.sumi,varaggITFE.sumd,varaggITFE.sumf,varaggITFE.suml";
	            var listener = new SupportUpdateListener();
	            env.CompileDeploy("@name('s0') select " + fields + " from SupportBean_S0", path).AddListener("s0");

	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertPropsNew("s0", fields.SplitCsv(), new object[]{10, 1000d, 10000f, 100L});

	            env.MilestoneInc(milestone);

	            MakeSendBean(env, "E1", 11, 101L, 1001d, 10001f);

	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertPropsNew("s0", fields.SplitCsv(), new object[]{21, 2001d, 20001f, 201L});

	            env.UndeployAll();
	        }
	    }

	    internal class InfraGroupedTwoKeyNoContext : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplDeclare = "@public create table varTotalG2K (key0 string primary key, key1 int primary key, total sum(long), cnt count(*))";
	            env.CompileDeploy(eplDeclare, path);

	            var eplBind = "into table varTotalG2K " +
	                          "select sum(longPrimitive) as total, count(*) as cnt " +
	                          "from SupportBean group by theString, intPrimitive";
	            env.CompileDeploy(eplBind, path);

	            var eplUse = "@name('s0') select varTotalG2K[p00, id].total as c0, varTotalG2K[p00, id].cnt as c1 from SupportBean_S0";
	            env.CompileDeploy(eplUse, path).AddListener("s0");

	            MakeSendBean(env, "E1", 10, 100);

	            var fields = "c0,c1".SplitCsv();
	            env.SendEventBean(new SupportBean_S0(10, "E1"));
	            env.AssertPropsNew("s0", fields, new object[]{100L, 1L});

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean_S0(0, "E1"));
	            env.AssertPropsNew("s0", fields, new object[]{null, null});
	            env.SendEventBean(new SupportBean_S0(10, "E2"));
	            env.AssertPropsNew("s0", fields, new object[]{null, null});

	            env.UndeployAll();
	        }
	    }

	    internal class InfraGroupedThreeKeyNoContext : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var eplDeclare = "@public create table varTotalG3K (key0 string primary key, key1 int primary key," +
	                             "key2 long primary key, total sum(double), cnt count(*))";
	            env.CompileDeploy(eplDeclare, path);

	            var eplBind = "into table varTotalG3K " +
	                          "select sum(doublePrimitive) as total, count(*) as cnt " +
	                          "from SupportBean group by theString, intPrimitive, longPrimitive";
	            env.CompileDeploy(eplBind, path);

	            var fields = "c0,c1".SplitCsv();
	            var eplUse = "@name('s0') select varTotalG3K[p00, id, 100L].total as c0, varTotalG3K[p00, id, 100L].cnt as c1 from SupportBean_S0";
	            env.CompileDeploy(eplUse, path).AddListener("s0");

	            MakeSendBean(env, "E1", 10, 100, 1000);

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean_S0(10, "E1"));
	            env.AssertPropsNew("s0", fields, new object[]{1000.0, 1L});

	            env.Milestone(1);

	            MakeSendBean(env, "E1", 10, 100, 1001);

	            env.SendEventBean(new SupportBean_S0(10, "E1"));
	            env.AssertPropsNew("s0", fields, new object[]{2001.0, 2L});

	            env.UndeployAll();
	        }
	    }

	    internal class InfraGroupedSingleKeyNoContext : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            TryAssertionGroupedSingleKeyNoContext(env, false, milestone);
	            TryAssertionGroupedSingleKeyNoContext(env, true, milestone);
	        }

	        private static void TryAssertionGroupedSingleKeyNoContext(RegressionEnvironment env, bool soda, AtomicLong milestone) {
	            var path = new RegressionPath();
	            var eplDeclare = "@public create table varTotalG1K (key string primary key, total sum(int))";
	            env.CompileDeploy(soda, eplDeclare, path);

	            var eplBind = "into table varTotalG1K " +
	                          "select theString, sum(intPrimitive) as total from SupportBean group by theString";
	            env.CompileDeploy(soda, eplBind, path);

	            var eplUse = "@name('s0') select p00 as c0, varTotalG1K[p00].total as c1 from SupportBean_S0";
	            env.CompileDeploy(soda, eplUse, path).AddListener("s0");

	            TryAssertionTopLevelSingle(env, milestone);

	            env.UndeployAll();
	        }
	    }

	    internal class InfraUngroupedWContext : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var eplPart = "create context PartitionedByString partition by theString from SupportBean, p00 from SupportBean_S0;\n" +
	                          "context PartitionedByString create table varTotalUG (total sum(int));\n" +
	                          "context PartitionedByString into table varTotalUG select sum(intPrimitive) as total from SupportBean;\n" +
	                          "@name('s0') context PartitionedByString select p00 as c0, varTotalUG.total as c1 from SupportBean_S0;\n";
	            env.CompileDeploy(eplPart);
	            env.AddListener("s0");

	            TryAssertionTopLevelSingle(env, new AtomicLong());

	            env.UndeployAll();
	        }
	    }

	    internal class InfraOrderOfAggregationsAndPush : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            TryAssertionOrderOfAggs(env, true, milestone);
	            TryAssertionOrderOfAggs(env, false, milestone);
	        }
	    }

	    internal class InfraMultiStmtContributing : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();

	            TryAssertionMultiStmtContributingDifferentAggs(env, false, milestone);
	            TryAssertionMultiStmtContributingDifferentAggs(env, true, milestone);

	            // contribute to the same aggregation
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table sharedagg (total sum(int))", path);
	            env.CompileDeploy("@name('i1') into table sharedagg " +
	                "select p00 as c0, sum(id) as total from SupportBean_S0", path).AddListener("i1");
	            env.CompileDeploy("@name('i2') into table sharedagg " +
	                "select p10 as c0, sum(id) as total from SupportBean_S1", path).AddListener("i2");
	            env.CompileDeploy("@name('s0') select theString as c0, sharedagg.total as total from SupportBean", path).AddListener("s0");

	            env.SendEventBean(new SupportBean_S0(10, "A"));
	            AssertMultiStmtContributingTotal(env, "i1", "A", 10);

	            env.SendEventBean(new SupportBean_S1(-5, "B"));
	            AssertMultiStmtContributingTotal(env, "i2", "B", 5);

	            env.MilestoneInc(milestone);

	            env.SendEventBean(new SupportBean_S0(2, "C"));
	            AssertMultiStmtContributingTotal(env, "i1", "C", 7);

	            env.UndeployAll();
	        }

	        private static void AssertMultiStmtContributingTotal(RegressionEnvironment env, string stmtName, string c0, int total) {
	            var fields = "c0,total".SplitCsv();
	            env.AssertEventNew(stmtName, @event => EPAssertionUtil.AssertProps(@event, fields, new object[]{c0, total}));

	            env.SendEventBean(new SupportBean(c0, 0));
	            env.AssertPropsNew("s0", fields, new object[]{c0, total});
	        }

	        private static void TryAssertionMultiStmtContributingDifferentAggs(RegressionEnvironment env, bool grouped, AtomicLong milestone) {
	            var path = new RegressionPath();
	            var eplDeclare = "@public create table varaggMSC (" +
	                             (grouped ? "key string primary key," : "") +
	                             "s0sum sum(int), s0cnt count(*), s0win window(*) @type(SupportBean_S0)," +
	                             "s1sum sum(int), s1cnt count(*), s1win window(*) @type(SupportBean_S1)" +
	                             ")";
	            env.CompileDeploy(eplDeclare, path);

	            var fieldsSelect = "c0,c1,c2,c3,c4,c5".SplitCsv();
	            var eplSelectUngrouped = "@name('s0') select varaggMSC.s0sum as c0, varaggMSC.s0cnt as c1," +
	                                     "varaggMSC.s0win as c2, varaggMSC.s1sum as c3, varaggMSC.s1cnt as c4," +
	                                     "varaggMSC.s1win as c5 from SupportBean";
	            var eplSelectGrouped = "@name('s0') select varaggMSC[theString].s0sum as c0, varaggMSC[theString].s0cnt as c1," +
	                                   "varaggMSC[theString].s0win as c2, varaggMSC[theString].s1sum as c3, varaggMSC[theString].s1cnt as c4," +
	                                   "varaggMSC[theString].s1win as c5 from SupportBean";
	            env.CompileDeploy(grouped ? eplSelectGrouped : eplSelectUngrouped, path).AddListener("s0");

	            var fieldsOne = "s0sum,s0cnt,s0win".SplitCsv();
	            var eplBindOne = "@name('s1') into table varaggMSC select sum(id) as s0sum, count(*) as s0cnt, window(*) as s0win from SupportBean_S0#length(2) " +
	                             (grouped ? "group by p00" : "");
	            env.CompileDeploy(eplBindOne, path).AddListener("s1");

	            var fieldsTwo = "s1sum,s1cnt,s1win".SplitCsv();
	            var eplBindTwo = "@name('s2') into table varaggMSC select sum(id) as s1sum, count(*) as s1cnt, window(*) as s1win from SupportBean_S1#length(2) " +
	                             (grouped ? "group by p10" : "");
	            env.CompileDeploy(eplBindTwo, path).AddListener("s2");

	            // contribute S1
	            var s1Bean1 = MakeSendS1(env, 10, "G1");
	            env.AssertPropsNew("s2", fieldsTwo, new object[]{10, 1L, new object[]{s1Bean1}});

	            env.SendEventBean(new SupportBean("G1", 0));
	            env.AssertPropsNew("s0", fieldsSelect,
	                new object[]{null, 0L, null, 10, 1L, new object[]{s1Bean1}});

	            env.MilestoneInc(milestone);

	            // contribute S0
	            var s0Bean1 = MakeSendS0(env, 20, "G1");
	            env.AssertPropsNew("s1", fieldsOne, new object[]{20, 1L, new object[]{s0Bean1}});

	            env.SendEventBean(new SupportBean("G1", 0));
	            env.AssertPropsNew("s0", fieldsSelect,
	                new object[]{20, 1L, new object[]{s0Bean1}, 10, 1L, new object[]{s1Bean1}});

	            // contribute S1 and S0
	            var s1Bean2 = MakeSendS1(env, 11, "G1");
	            env.AssertPropsNew("s2", fieldsTwo, new object[]{21, 2L, new object[]{s1Bean1, s1Bean2}});
	            var s0Bean2 = MakeSendS0(env, 21, "G1");
	            env.AssertPropsNew("s1", fieldsOne, new object[]{41, 2L, new object[]{s0Bean1, s0Bean2}});

	            env.SendEventBean(new SupportBean("G1", 0));
	            env.AssertPropsNew("s0", fieldsSelect,
	                new object[]{41, 2L, new object[]{s0Bean1, s0Bean2}, 21, 2L, new object[]{s1Bean1, s1Bean2}});

	            env.MilestoneInc(milestone);

	            // contribute S1 and S0 (leave)
	            var s1Bean3 = MakeSendS1(env, 12, "G1");
	            env.AssertPropsNew("s2", fieldsTwo, new object[]{23, 2L, new object[]{s1Bean2, s1Bean3}});
	            var s0Bean3 = MakeSendS0(env, 22, "G1");
	            env.AssertPropsNew("s1", fieldsOne, new object[]{43, 2L, new object[]{s0Bean2, s0Bean3}});

	            env.SendEventBean(new SupportBean("G1", 0));
	            env.AssertPropsNew("s0", fieldsSelect, new object[]{43, 2L, new object[]{s0Bean2, s0Bean3}, 23, 2L, new object[]{s1Bean2, s1Bean3}});

	            env.UndeployAll();
	        }
	    }

	    internal class InfraGroupedMixedMethodAndAccess : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var milestone = new AtomicLong();
	            TryAssertionGroupedMixedMethodAndAccess(env, false, milestone);
	            TryAssertionGroupedMixedMethodAndAccess(env, true, milestone);
	        }
	    }

	    internal class InfraNamedWindowAndFireAndForget : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = "@public create window MyWindow#length(2) as SupportBean;\n" +
	                      "insert into MyWindow select * from SupportBean;\n" +
	                      "@public create table varaggNWFAF (total sum(int));\n" +
	                      "into table varaggNWFAF select sum(intPrimitive) as total from MyWindow;\n";
	            env.CompileDeploy(epl, path);

	            env.SendEventBean(new SupportBean("E1", 10));
	            var resultSelect = env.CompileExecuteFAF("select varaggNWFAF.total as c0 from MyWindow", path);
	            Assert.AreEqual(10, resultSelect.Array[0].Get("c0"));

	            var resultDelete = env.CompileExecuteFAF("delete from MyWindow where varaggNWFAF.total = intPrimitive", path);
	            Assert.AreEqual(1, resultDelete.Array.Length);

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean("E2", 20));
	            var resultUpdate = env.CompileExecuteFAF("update MyWindow set doublePrimitive = 100 where varaggNWFAF.total = intPrimitive", path);
	            Assert.AreEqual(100d, resultUpdate.Array[0].Get("doublePrimitive"));

	            var resultInsert = env.CompileExecuteFAF("insert into MyWindow (theString, intPrimitive) values ('A', varaggNWFAF.total)", path);
	            EPAssertionUtil.AssertProps(resultInsert.Array[0], "theString,intPrimitive".SplitCsv(), new object[]{"A", 20});

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.FIREANDFORGET);
	        }
	    }

	    internal class InfraSubquery : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table subquery_var_agg (key string primary key, total count(*))", path);
	            env.CompileDeploy("@name('s0') select (select subquery_var_agg[p00].total from SupportBean_S0#lastevent) as c0 " +
	                "from SupportBean_S1", path).AddListener("s0");
	            env.CompileDeploy("into table subquery_var_agg select count(*) as total from SupportBean group by theString", path);

	            env.SendEventBean(new SupportBean("E1", -1));
	            env.SendEventBean(new SupportBean_S0(0, "E1"));

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean_S1(1));
	            env.AssertEqualsNew("s0", "c0", 1L);

	            env.SendEventBean(new SupportBean("E1", -1));

	            env.Milestone(1);

	            env.SendEventBean(new SupportBean_S1(2));
	            env.AssertEqualsNew("s0", "c0", 2L);

	            env.UndeployAll();
	        }
	    }

	    internal class InfraOnMergeExpressions : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create table the_table (key string primary key, total count(*), value int)", path);
	            env.CompileDeploy("into table the_table select count(*) as total from SupportBean group by theString", path);
	            env.CompileDeploy("on SupportBean_S0 as s0 " +
	                "merge the_table as tt " +
	                "where s0.p00 = tt.key " +
	                "when matched and the_table[s0.p00].total > 0" +
	                "  then update set value = 1", path);
	            env.CompileDeploy("@name('s0') select the_table[p10].value as c0 from SupportBean_S1", path).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", -1));
	            env.SendEventBean(new SupportBean_S0(0, "E1"));

	            env.Milestone(0);

	            env.SendEventBean(new SupportBean_S1(0, "E1"));
	            env.AssertEqualsNew("s0", "c0", 1);

	            env.UndeployAll();
	        }
	    }

	    internal class InfraTableAccessCoreSplitStream : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            var epl = "@public create table MyTable(k1 string primary key, c1 int);\n" +
	                      "insert into MyTable select theString as k1, intPrimitive as c1 from SupportBean;\n";
	            env.CompileDeploy(epl, path);

	            epl = "@public on SupportBean_S0 " +
	                "  insert into AStream select MyTable['A'].c1 as c0 where id=1" +
	                "  insert into AStream select MyTable['B'].c1 as c0 where id=2;\n";
	            env.CompileDeploy(epl, path);

	            env.CompileDeploy("@name('out') select * from AStream", path).AddListener("out");

	            env.SendEventBean(new SupportBean("A", 10));
	            env.SendEventBean(new SupportBean("B", 20));

	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertEqualsNew("out", "c0", 10);

	            env.SendEventBean(new SupportBean_S0(2));
	            env.AssertEqualsNew("out", "c0", 20);

	            env.UndeployAll();
	        }
	    }

	    internal class InfraTableAccessCoreUnGroupedWindowAndSum : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public @buseventtype create objectarray schema MyEvent(c0 int)", path);

	            env.CompileDeploy("@public create table windowAndTotal (" +
	                "thewindow window(*) @type(MyEvent), thetotal sum(int))", path);
	            env.CompileDeploy("into table windowAndTotal " +
	                "select window(*) as thewindow, sum(c0) as thetotal from MyEvent#length(2)", path);

	            env.CompileDeploy("@name('s0') select windowAndTotal as val0 from SupportBean_S0", path).AddListener("s0");

	            var e1 = new object[]{10};
	            env.SendEventObjectArray(e1, "MyEvent");

	            env.Milestone(0);

	            var fieldsInner = "thewindow,thetotal".SplitCsv();
	            env.SendEventBean(new SupportBean_S0(0));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e1}, 10));

	            env.Milestone(1);

	            var e2 = new object[]{20};
	            env.SendEventObjectArray(e2, "MyEvent");

	            env.Milestone(2);

	            env.SendEventBean(new SupportBean_S0(1));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e1, e2}, 30));

	            env.Milestone(3);

	            var e3 = new object[]{30};
	            env.SendEventObjectArray(e3, "MyEvent");

	            env.Milestone(4);

	            env.SendEventBean(new SupportBean_S0(2));
	            env.AssertEventNew("s0", @event => EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) @event.Get("val0"), fieldsInner, new object[][]{e2, e3}, 50));

	            env.UndeployAll();
	        }
	    }

	    private static void TryAssertionGroupedMixedMethodAndAccess(RegressionEnvironment env, bool soda, AtomicLong milestone) {
	        var path = new RegressionPath();
	        var eplDeclare = "@public create table varMyAgg (" +
	                         "key string primary key, " +
	                         "c0 count(*), " +
	                         "c1 count(distinct int), " +
	                         "c2 window(*) @type('SupportBean'), " +
	                         "c3 sum(long)" +
	                         ")";
	        env.CompileDeploy(soda, eplDeclare, path);

	        var eplBind = "into table varMyAgg select " +
	                      "count(*) as c0, " +
	                      "count(distinct intPrimitive) as c1, " +
	                      "window(*) as c2, " +
	                      "sum(longPrimitive) as c3 " +
	                      "from SupportBean#length(3) group by theString";
	        env.CompileDeploy(soda, eplBind, path);

	        var eplSelect = "@name('s0') select " +
	                        "varMyAgg[p00].c0 as c0, " +
	                        "varMyAgg[p00].c1 as c1, " +
	                        "varMyAgg[p00].c2 as c2, " +
	                        "varMyAgg[p00].c3 as c3" +
	                        " from SupportBean_S0";
	        env.CompileDeploy(soda, eplSelect, path).AddListener("s0");
	        var fields = "c0,c1,c2,c3".SplitCsv();

	        env.AssertStatement("s0", statement =>{
	            var eventType = statement.EventType;
	            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c0"));
	            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c1"));
	            Assert.AreEqual(typeof(SupportBean[]), eventType.GetPropertyType("c2"));
	            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("c3"));
	        });

	        var b1 = MakeSendBean(env, "E1", 10, 100);
	        var b2 = MakeSendBean(env, "E1", 11, 101);
	        var b3 = MakeSendBean(env, "E1", 10, 102);

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportBean_S0(0, "E1"));
	        env.AssertPropsNew("s0", fields,
	            new object[]{3L, 2L, new SupportBean[]{b1, b2, b3}, 303L});

	        env.SendEventBean(new SupportBean_S0(0, "E2"));
	        env.AssertPropsNew("s0", fields,
	            new object[]{null, null, null, null});

	        var b4 = MakeSendBean(env, "E2", 20, 200);

	        env.MilestoneInc(milestone);

	        env.SendEventBean(new SupportBean_S0(0, "E2"));
	        env.AssertPropsNew("s0", fields,
	            new object[]{1L, 1L, new SupportBean[]{b4}, 200L});

	        env.UndeployAll();
	    }

	    private static void TryAssertionTopLevelSingle(RegressionEnvironment env, AtomicLong milestone) {
	        SendEventsAndAssert(env, "A", 10, "A", 10);
	        SendEventsAndAssert(env, "A", 11, "A", 21);
	        SendEventsAndAssert(env, "B", 20, "A", 21);

	        env.MilestoneInc(milestone);

	        SendEventsAndAssert(env, "B", 21, "B", 41);
	        SendEventsAndAssert(env, "C", 30, "A", 21);
	        SendEventsAndAssert(env, "D", 40, "C", 30);

	        var fields = "c0,c1".SplitCsv();
	        var expected = new int[]{21, 41, 30, 40};
	        var count = 0;
	        foreach (var p00 in "A,B,C,D".SplitCsv()) {
	            env.SendEventBean(new SupportBean_S0(0, p00));
	            env.AssertPropsNew("s0", fields, new object[]{p00, expected[count]});
	            count++;
	        }

	        env.SendEventBean(new SupportBean_S0(0, "A"));
	        env.AssertPropsNew("s0", fields, new object[]{"A", 21});
	    }

	    private static void SendEventsAndAssert(RegressionEnvironment env, string theString, int intPrimitive, string p00, int total) {
	        var fields = "c0,c1".SplitCsv();
	        env.SendEventBean(new SupportBean(theString, intPrimitive));
	        env.SendEventBean(new SupportBean_S0(0, p00));
	        env.AssertPropsNew("s0", fields, new object[]{p00, total});
	    }

	    private static SupportBean MakeSendBean(RegressionEnvironment env, string theString, int intPrimitive, long longPrimitive) {
	        return MakeSendBean(env, theString, intPrimitive, longPrimitive, -1);
	    }

	    private static SupportBean MakeSendBean(RegressionEnvironment env, string theString, int intPrimitive, long longPrimitive, double doublePrimitive) {
	        return MakeSendBean(env, theString, intPrimitive, longPrimitive, doublePrimitive, -1);
	    }

	    private static SupportBean MakeSendBean(RegressionEnvironment env, string theString, int intPrimitive, long longPrimitive, double doublePrimitive, float floatPrimitive) {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        bean.DoublePrimitive = doublePrimitive;
	        bean.FloatPrimitive = floatPrimitive;
	        env.SendEventBean(bean);
	        return bean;
	    }

	    private static void AssertTopLevelTypeInfo(EPStatement stmt) {
	        Assert.AreEqual(typeof(IDictionary<string, object>), stmt.EventType.GetPropertyType("val0"));
	        var fragType = stmt.EventType.GetFragmentType("val0");
	        Assert.IsFalse(fragType.IsIndexed);
	        Assert.IsFalse(fragType.IsNative);
	        Assert.AreEqual(typeof(object[][]), fragType.FragmentType.GetPropertyType("thewindow"));
	        Assert.AreEqual(typeof(int?), fragType.FragmentType.GetPropertyType("thetotal"));
	    }

	    private static void AssertIntegerIndexed(EventBean @event, SupportBean[] events) {
	        EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c0.myevents"));
	        EPAssertionUtil.AssertEqualsExactOrder(events, (object[]) @event.Get("c1"));
	        Assert.AreEqual(events[^1], @event.Get("c2"));
	        Assert.AreEqual(events[^2], @event.Get("c3"));
	    }

	    private static SupportBean_S1 MakeSendS1(RegressionEnvironment env, int id, string p10) {
	        var bean = new SupportBean_S1(id, p10);
	        env.SendEventBean(bean);
	        return bean;
	    }

	    private static SupportBean_S0 MakeSendS0(RegressionEnvironment env, int id, string p00) {
	        var bean = new SupportBean_S0(id, p00);
	        env.SendEventBean(bean);
	        return bean;
	    }

	    private static void TryAssertionOrderOfAggs(RegressionEnvironment env, bool ungrouped, AtomicLong milestone) {

	        var path = new RegressionPath();
	        var eplDeclare = "@public create table varaggOOA (" + (ungrouped ? "" : "key string primary key, ") +
	                         "sumint sum(int), " +
	                         "sumlong sum(long), " +
	                         "mysort sorted(intPrimitive) @type(SupportBean)," +
	                         "mywindow window(*) @type(SupportBean)" +
	                         ")";
	        env.CompileDeploy(eplDeclare, path);

	        var fieldsTable = "sumint,sumlong,mywindow,mysort".SplitCsv();
	        var eplSelect = "@name('into') into table varaggOOA select " +
	                        "sum(longPrimitive) as sumlong, " +
	                        "sum(intPrimitive) as sumint, " +
	                        "window(*) as mywindow," +
	                        "sorted() as mysort " +
	                        "from SupportBean#length(2) " +
	                        (ungrouped ? "" : "group by theString ");
	        env.CompileDeploy(eplSelect, path).AddListener("into");

	        var fieldsSelect = "c0,c1,c2,c3".SplitCsv();
	        var groupKey = ungrouped ? "" : "['E1']";
	        env.CompileDeploy("@name('s0') select " +
	            "varaggOOA" + groupKey + ".sumint as c0, " +
	            "varaggOOA" + groupKey + ".sumlong as c1," +
	            "varaggOOA" + groupKey + ".mywindow as c2," +
	            "varaggOOA" + groupKey + ".mysort as c3 from SupportBean_S0", path).AddListener("s0");

	        var e1 = MakeSendBean(env, "E1", 10, 100);
	        env.AssertPropsNew("into", fieldsTable, new object[]{10, 100L, new object[]{e1}, new object[]{e1}});

	        env.MilestoneInc(milestone);

	        var e2 = MakeSendBean(env, "E1", 5, 50);
	        env.AssertPropsNew("into", fieldsTable,
	            new object[]{15, 150L, new object[]{e1, e2}, new object[]{e2, e1}});

	        env.SendEventBean(new SupportBean_S0(0));
	        env.AssertPropsNew("s0", fieldsSelect, new object[]{15, 150L, new object[]{e1, e2}, new object[]{e2, e1}});

	        env.MilestoneInc(milestone);

	        var e3 = MakeSendBean(env, "E1", 12, 120);
	        env.AssertPropsNew("into", fieldsTable, new object[]{17, 170L, new object[]{e2, e3}, new object[]{e2, e3}});

	        env.SendEventBean(new SupportBean_S0(0));
	        env.AssertPropsNew("s0", fieldsSelect,
	            new object[]{17, 170L, new object[]{e2, e3}, new object[]{e2, e3}});

	        env.UndeployAll();
	    }

	    public class AggSubBean
	    {
		    private int thetotal;
		    private object[][] thewindow;

		    public int Thetotal {
			    get => thetotal;
			    set => thetotal = value;
		    }

		    public object[][] Thewindow {
			    get => thewindow;
			    set => thewindow = value;
		    }
	    }

	    public class AggBean {
	        private AggSubBean val0;

	        public AggSubBean Val0 {
		        get => val0;
		        set => val0 = value;
	        }
	    }
	}
} // end of namespace
