///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
	public class EPLJoinUnidirectionalStream {
	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLJoinPatternUnidirectionalOuterJoinNoOn());
	        execs.Add(new EPLJoin2TableJoinGrouped());
	        execs.Add(new EPLJoin2TableJoinRowForAll());
	        execs.Add(new EPLJoin3TableOuterJoinVar1());
	        execs.Add(new EPLJoin3TableOuterJoinVar2());
	        execs.Add(new EPLJoinPatternJoin());
	        execs.Add(new EPLJoinPatternJoinOutputRate());
	        execs.Add(new EPLJoin3TableJoinVar1());
	        execs.Add(new EPLJoin3TableJoinVar2A());
	        execs.Add(new EPLJoin3TableJoinVar2B());
	        execs.Add(new EPLJoin3TableJoinVar3());
	        execs.Add(new EPLJoin2TableFullOuterJoin());
	        execs.Add(new EPLJoin2TableFullOuterJoinCompile());
	        execs.Add(new EPLJoin2TableFullOuterJoinOM());
	        execs.Add(new EPLJoin2TableFullOuterJoinBackwards());
	        execs.Add(new EPLJoin2TableJoin());
	        execs.Add(new EPLJoin2TableBackwards());
	        execs.Add(new EPLJoinInvalid());
	        return execs;
	    }

	    private class EPLJoinPatternUnidirectionalOuterJoinNoOn : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            // test 2-stream left outer join and SODA
	            //
	            var milestone = new AtomicLong();
	            env.AdvanceTime(1000);

	            var stmtTextLO = "@name('s0') select sum(intPrimitive) as c0, count(*) as c1 " +
	                             "from pattern [every timer:interval(1)] unidirectional " +
	                             "left outer join " +
	                             "SupportBean#keepall";
	            env.CompileDeployAddListenerMile(stmtTextLO, "s0", milestone.GetAndIncrement());

	            TryAssertionPatternUniOuterJoinNoOn(env, 0);

	            env.UndeployAll();

	            env.EplToModelCompileDeploy(stmtTextLO).AddListener("s0").Milestone(milestone.GetAndIncrement());

	            TryAssertionPatternUniOuterJoinNoOn(env, 100000);

	            env.UndeployAll();

	            // test 2-stream inner join
	            //
	            var fieldsIJ = "c0,c1".SplitCsv();
	            var stmtTextIJ = "@name('s0') select sum(intPrimitive) as c0, count(*) as c1 " +
	                             "from SupportBean_S0 unidirectional " +
	                             "inner join " +
	                             "SupportBean#keepall";
	            env.CompileDeployAddListenerMile(stmtTextIJ, "s0", milestone.GetAndIncrement());

	            env.SendEventBean(new SupportBean_S0(1, "S0_1"));
	            env.SendEventBean(new SupportBean("E1", 100));
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(new SupportBean_S0(2, "S0_2"));
	            env.AssertPropsNew("s0", fieldsIJ, new object[]{100, 1L});

	            env.SendEventBean(new SupportBean("E2", 200));

	            env.SendEventBean(new SupportBean_S0(3, "S0_3"));
	            env.AssertPropsNew("s0", fieldsIJ, new object[]{300, 2L});
	            env.UndeployAll();

	            // test 2-stream inner join with group-by
	            TryAssertion2StreamInnerWGroupBy(env);

	            // test 3-stream inner join
	            //
	            var fields3IJ = "c0,c1".SplitCsv();
	            var stmtText3IJ = "@name('s0') select sum(intPrimitive) as c0, count(*) as c1 " +
	                              "from " +
	                              "SupportBean_S0#keepall " +
	                              "inner join " +
	                              "SupportBean_S1#keepall " +
	                              "inner join " +
	                              "SupportBean#keepall";
	            env.CompileDeployAddListenerMile(stmtText3IJ, "s0", milestone.GetAndIncrement());

	            env.SendEventBean(new SupportBean_S0(1, "S0_1"));
	            env.SendEventBean(new SupportBean("E1", 50));
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(new SupportBean_S1(10, "S1_1"));
	            env.AssertPropsNew("s0", fields3IJ, new object[]{50, 1L});

	            env.SendEventBean(new SupportBean("E2", 51));
	            env.AssertPropsNew("s0", fields3IJ, new object[]{101, 2L});

	            env.UndeployAll();

	            // test 3-stream full outer join
	            //
	            var fields3FOJ = "p00,p10,theString".SplitCsv();
	            var stmtText3FOJ = "@name('s0') select p00, p10, theString " +
	                               "from " +
	                               "SupportBean_S0#keepall " +
	                               "full outer join " +
	                               "SupportBean_S1#keepall " +
	                               "full outer join " +
	                               "SupportBean#keepall";
	            env.CompileDeployAddListenerMile(stmtText3FOJ, "s0", milestone.GetAndIncrement());

	            env.SendEventBean(new SupportBean_S0(1, "S0_1"));
	            env.AssertPropsNew("s0", fields3FOJ, new object[]{"S0_1", null, null});

	            env.SendEventBean(new SupportBean("E10", 0));
	            env.AssertPropsNew("s0", fields3FOJ, new object[]{null, null, "E10"});

	            env.SendEventBean(new SupportBean_S0(2, "S0_2"));
	            env.AssertPropsPerRowLastNew("s0", fields3FOJ, new object[][]{new object[] {"S0_2", null, null}});

	            env.SendEventBean(new SupportBean_S1(1, "S1_0"));
	            env.AssertPropsPerRowLastNewAnyOrder("s0", fields3FOJ, new object[][]{new object[] {"S0_1", "S1_0", "E10"}, new object[] {"S0_2", "S1_0", "E10"}});

	            env.SendEventBean(new SupportBean_S0(2, "S0_3"));
	            env.AssertPropsPerRowLastNew("s0", fields3FOJ, new object[][]{new object[] {"S0_3", "S1_0", "E10"}});

	            env.SendEventBean(new SupportBean("E11", 0));
	            env.AssertPropsPerRowLastNewAnyOrder("s0", fields3FOJ, new object[][]{new object[] {"S0_1", "S1_0", "E11"}, new object[] {"S0_2", "S1_0", "E11"}, new object[] {"S0_3", "S1_0", "E11"}});
	            env.AssertIterator("s0", iterator => Assert.AreEqual(6, EPAssertionUtil.EnumeratorCount(iterator)));

	            env.UndeployAll();

	            // test 3-stream full outer join with where-clause
	            //
	            var fields3FOJW = "p00,p10,theString".SplitCsv();
	            var stmtText3FOJW = "@name('s0') select p00, p10, theString " +
	                                "from " +
	                                "SupportBean_S0#keepall as s0 " +
	                                "full outer join " +
	                                "SupportBean_S1#keepall as s1 " +
	                                "full outer join " +
	                                "SupportBean#keepall as sb " +
	                                "where s0.p00 = s1.p10";
	            env.CompileDeployAddListenerMile(stmtText3FOJW, "s0", milestone.GetAndIncrement());

	            env.SendEventBean(new SupportBean_S0(1, "X1"));
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(new SupportBean_S1(1, "Y1"));
	            env.AssertListenerNotInvoked("s0");

	            env.SendEventBean(new SupportBean_S0(1, "Y1"));
	            env.AssertPropsNew("s0", fields3FOJW, new object[]{"Y1", "Y1", null});

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableJoinGrouped : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select irstream symbol, count(*) as cnt " +
	                           "from SupportMarketDataBean unidirectional, SupportBean#keepall " +
	                           "where theString = symbol group by theString, symbol";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            // send event, expect result
	            SendEventMD(env, "E1", 1L);
	            var fields = "symbol,cnt".SplitCsv();
	            env.AssertListenerNotInvoked("s0");

	            SendEvent(env, "E1", 10);
	            env.AssertListenerNotInvoked("s0");

	            SendEventMD(env, "E1", 2L);
	            env.AssertPropsIRPair("s0", fields, new object[]{"E1", 1L}, new object[]{"E1", 0L});

	            SendEvent(env, "E1", 20);
	            env.AssertListenerNotInvoked("s0");

	            SendEventMD(env, "E1", 3L);
	            env.AssertPropsIRPair("s0", fields, new object[]{"E1", 2L}, new object[]{"E1", 0L});

	            env.AssertThat(() => {
	                try {
	                    env.Statement("s0").GetEnumerator();
	                    Assert.Fail();
	                } catch (UnsupportedOperationException ex) {
	                    Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
	                }
	            });

	            // assure lock given up by sending more events
	            SendEvent(env, "E2", 40);
	            SendEventMD(env, "E2", 4L);
	            env.AssertPropsIRPair("s0", fields, new object[]{"E2", 1L}, new object[]{"E2", 0L});

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableJoinRowForAll : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select irstream count(*) as cnt " +
	                           "from SupportMarketDataBean unidirectional, SupportBean#keepall " +
	                           "where theString = symbol";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            TryUnsupportedIterator(env);

	            // send event, expect result
	            SendEventMD(env, "E1", 1L);
	            var fields = "cnt".SplitCsv();
	            env.AssertListenerNotInvoked("s0");

	            SendEvent(env, "E1", 10);
	            env.AssertListenerNotInvoked("s0");

	            SendEventMD(env, "E1", 2L);
	            env.AssertPropsIRPair("s0", fields, new object[]{1L}, new object[]{0L});

	            SendEvent(env, "E1", 20);
	            env.AssertListenerNotInvoked("s0");

	            SendEventMD(env, "E1", 3L);
	            env.AssertPropsIRPair("s0", fields, new object[]{2L}, new object[]{0L});

	            SendEvent(env, "E2", 40);
	            SendEventMD(env, "E2", 4L);
	            env.AssertPropsIRPair("s0", fields, new object[]{1L}, new object[]{0L});

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3TableOuterJoinVar1 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select s0.id, s1.id, s2.id " +
	                           "from SupportBean_S0 as s0 unidirectional " +
	                           " full outer join SupportBean_S1#keepall as s1" +
	                           " on p00 = p10 " +
	                           " full outer join SupportBean_S2#keepall as s2" +
	                           " on p10 = p20";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            Try3TableOuterJoin(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3TableOuterJoinVar2 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select s0.id, s1.id, s2.id from SupportBean_S0 as s0 unidirectional " +
	                           " left outer join SupportBean_S1#keepall as s1 " +
	                           " on p00 = p10 " +
	                           " left outer join SupportBean_S2#keepall as s2 " +
	                           " on p10 = p20";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            Try3TableOuterJoin(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinPatternJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.AdvanceTime(1000);

	            // no iterator allowed
	            var stmtText = "@name('s0') select count(*) as num " +
	                           "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
	                           "SupportBean(intPrimitive=1)#unique(theString) a,\n" +
	                           "SupportBean(intPrimitive=2)#unique(theString) b\n" +
	                           "where a.theString = b.theString";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            SendEvent(env, "A", 1);
	            SendEvent(env, "A", 2);
	            SendEvent(env, "B", 1);
	            SendEvent(env, "B", 2);
	            env.AssertListenerNotInvoked("s0");

	            env.AdvanceTime(70000);
	            env.AssertEqualsNew("s0", "num", 2L);

	            env.AdvanceTime(140000);
	            env.AssertEqualsNew("s0", "num", 2L);

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoinPatternJoinOutputRate : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.AdvanceTime(1000);

	            // no iterator allowed
	            var stmtText = "@name('s0') select count(*) as num " +
	                           "from pattern [every timer:at(*/1,*,*,*,*)] unidirectional,\n" +
	                           "SupportBean(intPrimitive=1)#unique(theString) a,\n" +
	                           "SupportBean(intPrimitive=2)#unique(theString) b\n" +
	                           "where a.theString = b.theString output every 2 minutes";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            SendEvent(env, "A", 1);
	            SendEvent(env, "A", 2);
	            SendEvent(env, "B", 1);
	            SendEvent(env, "B", 2);
	            env.AssertListenerNotInvoked("s0");

	            env.AdvanceTime(70000);
	            env.AdvanceTime(140000);

	            env.AdvanceTime(210000);
	            env.AssertListener("s0", listener => {
	                Assert.AreEqual(2L, listener.LastNewData[0].Get("num"));
	                Assert.AreEqual(2L, listener.LastNewData[1].Get("num"));
	            });

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3TableJoinVar1 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select s0.id, s1.id, s2.id " +
	                           "from " +
	                           "SupportBean_S0 as s0 unidirectional, " +
	                           "SupportBean_S1#keepall as s1, " +
	                           "SupportBean_S2#keepall as s2 " +
	                           "where p00 = p10 and p10 = p20";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            Try3TableJoin(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3TableJoinVar2A : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select s0.id, s1.id, s2.id " +
	                           "from " +
	                           "SupportBean_S1#keepall as s1, " +
	                           "SupportBean_S0 as s0 unidirectional, " +
	                           "SupportBean_S2#keepall as s2 " +
	                           "where p00 = p10 and p10 = p20";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            Try3TableJoin(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3TableJoinVar2B : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select s0.id, s1.id, s2.id " +
	                           "from " +
	                           "SupportBean_S2#keepall as s2, " +
	                           "SupportBean_S0 as s0 unidirectional, " +
	                           "SupportBean_S1#keepall as s1 " +
	                           "where p00 = p10 and p10 = p20";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            Try3TableJoin(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin3TableJoinVar3 : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select s0.id, s1.id, s2.id " +
	                           "from " +
	                           "SupportBean_S1#keepall as s1, " +
	                           "SupportBean_S2#keepall as s2, " +
	                           "SupportBean_S0 as s0 unidirectional " +
	                           "where p00 = p10 and p10 = p20";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            Try3TableJoin(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableFullOuterJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select symbol, volume, theString, intPrimitive " +
	                           "from SupportMarketDataBean unidirectional " +
	                           "full outer join SupportBean#keepall on theString = symbol";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");
	            TryFullOuterPassive2Stream(env);
	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableFullOuterJoinCompile : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select symbol, volume, theString, intPrimitive " +
	                           "from SupportMarketDataBean unidirectional " +
	                           "full outer join SupportBean#keepall on theString = symbol";
	            env.EplToModelCompileDeploy(stmtText).AddListener("s0");

	            TryFullOuterPassive2Stream(env);

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableFullOuterJoinOM : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create("symbol", "volume", "theString", "intPrimitive");
	            model.FromClause = FromClause.Create(FilterStream.Create(nameof(SupportMarketDataBean)).Unidirectional(true));
	            model.FromClause.Add(FilterStream.Create(nameof(SupportBean)).AddView("keepall"));
	            model.FromClause.Add(OuterJoinQualifier.Create("theString", OuterJoinType.FULL, "symbol"));

	            var stmtText = "select symbol, volume, theString, intPrimitive " +
	                           "from SupportMarketDataBean unidirectional " +
	                           "full outer join SupportBean" +
	                           "#keepall on theString = symbol";
	            Assert.AreEqual(stmtText, model.ToEPL());

	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
	            env.CompileDeploy(model).AddListener("s0");

	            TryFullOuterPassive2Stream(env);

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableFullOuterJoinBackwards : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select symbol, volume, theString, intPrimitive " +
	                           "from SupportBean#keepall full outer join " +
	                           "SupportMarketDataBean unidirectional " +
	                           "on theString = symbol";
	            env.CompileDeployAddListenerMileZero(stmtText, "s0");

	            TryFullOuterPassive2Stream(env);

	            env.UndeployAll();
	        }
	    }

	    private class EPLJoin2TableJoin : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select symbol, volume, theString, intPrimitive " +
	                           "from SupportMarketDataBean unidirectional, SupportBean" +
	                           "#keepall where theString = symbol";

	            TryJoinPassive2Stream(env, stmtText);
	        }
	    }

	    private class EPLJoin2TableBackwards : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var stmtText = "@name('s0') select symbol, volume, theString, intPrimitive " +
	                           "from SupportBean#keepall, SupportMarketDataBean unidirectional " +
	                           "where theString = symbol";

	            TryJoinPassive2Stream(env, stmtText);
	        }
	    }

	    private class EPLJoinInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var text = "select * from SupportBean unidirectional " +
	                       "full outer join SupportMarketDataBean#keepall unidirectional " +
	                       "on theString = symbol";
	            env.TryInvalidCompile(text, "The unidirectional keyword requires that no views are declared onto the stream (applies to stream 1)");

	            text = "select * from SupportBean#length(2) unidirectional " +
	                "full outer join SupportMarketDataBean#keepall " +
	                "on theString = symbol";
	            env.TryInvalidCompile(text, "The unidirectional keyword requires that no views are declared onto the stream");
	        }
	    }

	    private static void TryFullOuterPassive2Stream(RegressionEnvironment env) {
	        TryUnsupportedIterator(env);

	        // send event, expect result
	        SendEventMD(env, "E1", 1L);
	        var fields = "symbol,volume,theString,intPrimitive".SplitCsv();
	        env.AssertPropsNew("s0", fields, new object[]{"E1", 1L, null, null});

	        SendEvent(env, "E1", 10);
	        env.AssertListenerNotInvoked("s0");

	        SendEventMD(env, "E1", 2L);
	        env.AssertPropsNew("s0", fields, new object[]{"E1", 2L, "E1", 10});

	        SendEvent(env, "E1", 20);
	        env.AssertListenerNotInvoked("s0");
	    }

	    private static void TryJoinPassive2Stream(RegressionEnvironment env, string stmtText) {
	        env.CompileDeployAddListenerMileZero(stmtText, "s0");
	        TryUnsupportedIterator(env);

	        // send event, expect result
	        SendEventMD(env, "E1", 1L);
	        var fields = "symbol,volume,theString,intPrimitive".SplitCsv();
	        env.AssertListenerNotInvoked("s0");

	        SendEvent(env, "E1", 10);
	        env.AssertListenerNotInvoked("s0");

	        SendEventMD(env, "E1", 2L);
	        env.AssertPropsNew("s0", fields, new object[]{"E1", 2L, "E1", 10});

	        SendEvent(env, "E1", 20);
	        env.AssertListenerNotInvoked("s0");

	        env.UndeployAll();
	    }

	    private static void SendEvent(RegressionEnvironment env, string s, int intPrimitive) {
	        var bean = new SupportBean();
	        bean.TheString = s;
	        bean.IntPrimitive = intPrimitive;
	        env.SendEventBean(bean);
	    }

	    private static void SendEventMD(RegressionEnvironment env, string symbol, long volume) {
	        var bean = new SupportMarketDataBean(symbol, 0, volume, "");
	        env.SendEventBean(bean);
	    }

	    private static void TryAssertionPatternUniOuterJoinNoOn(RegressionEnvironment env, long startTime) {
	        var fields = "c0,c1".SplitCsv();
	        env.AdvanceTime(startTime + 2000);
	        env.AssertPropsNew("s0", fields, new object[]{null, 1L});

	        env.SendEventBean(new SupportBean("E1", 10));
	        env.AssertListenerNotInvoked("s0");

	        env.AdvanceTime(startTime + 3000);
	        env.AssertPropsNew("s0", fields, new object[]{10, 1L});

	        env.SendEventBean(new SupportBean("E2", 11));

	        env.AdvanceTime(startTime + 4000);
	        env.AssertPropsNew("s0", fields, new object[]{21, 2L});

	        env.SendEventBean(new SupportBean("E3", 12));

	        env.AdvanceTime(startTime + 5000);
	        env.AssertPropsNew("s0", fields, new object[]{33, 3L});
	    }

	    private static void TryAssertion2StreamInnerWGroupBy(RegressionEnvironment env) {
	        var epl = "@buseventtype @public create objectarray schema E1 (id string, grp string, value int);\n" +
	                  "@buseventtype @public create objectarray schema E2 (id string, value2 int);\n" +
	                  "@name('s0') select count(*) as c0, sum(E1.value) as c1, E1.id as c2 " +
	                  "from E1 unidirectional inner join E2#keepall on E1.id = E2.id group by E1.grp";
	        env.CompileDeploy(epl, new RegressionPath());
	        env.AddListener("s0");
	        var fields = "c0,c1,c2".SplitCsv();

	        env.SendEventObjectArray(new object[]{"A", 100}, "E2");
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventObjectArray(new object[]{"A", "X", 10}, "E1");
	        env.AssertPropsNew("s0", fields, new object[]{1L, 10, "A"});

	        env.SendEventObjectArray(new object[]{"A", "Y", 20}, "E1");
	        env.AssertPropsNew("s0", fields, new object[]{1L, 20, "A"});

	        env.UndeployAll();
	    }

	    private static void Try3TableOuterJoin(RegressionEnvironment env) {
	        var fields = "s0.id,s1.id,s2.id".SplitCsv();

	        env.SendEventBean(new SupportBean_S0(1, "E1"));
	        env.AssertPropsNew("s0", fields, new object[]{1, null, null});
	        env.SendEventBean(new SupportBean_S1(2, "E1"));
	        env.SendEventBean(new SupportBean_S2(3, "E1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S1(20, "E2"));
	        env.SendEventBean(new SupportBean_S0(10, "E2"));
	        env.AssertPropsNew("s0", fields, new object[]{10, 20, null});
	        env.SendEventBean(new SupportBean_S2(30, "E2"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(300, "E3"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportBean_S0(100, "E3"));
	        env.AssertPropsNew("s0", fields, new object[]{100, null, null});
	        env.SendEventBean(new SupportBean_S1(200, "E3"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(31, "E4"));
	        env.SendEventBean(new SupportBean_S1(21, "E4"));
	        env.AssertListenerNotInvoked("s0");
	        env.SendEventBean(new SupportBean_S0(11, "E4"));
	        env.AssertPropsNew("s0", fields, new object[]{11, 21, 31});

	        env.SendEventBean(new SupportBean_S2(32, "E4"));
	        env.SendEventBean(new SupportBean_S1(22, "E4"));
	        env.AssertListenerNotInvoked("s0");
	    }

	    private static void Try3TableJoin(RegressionEnvironment env) {
	        env.SendEventBean(new SupportBean_S0(1, "E1"));
	        env.SendEventBean(new SupportBean_S1(2, "E1"));
	        env.SendEventBean(new SupportBean_S2(3, "E1"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S1(20, "E2"));
	        env.SendEventBean(new SupportBean_S0(10, "E2"));
	        env.SendEventBean(new SupportBean_S2(30, "E2"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(300, "E3"));
	        env.SendEventBean(new SupportBean_S0(100, "E3"));
	        env.SendEventBean(new SupportBean_S1(200, "E3"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S2(31, "E4"));
	        env.SendEventBean(new SupportBean_S1(21, "E4"));
	        env.AssertListenerNotInvoked("s0");

	        env.SendEventBean(new SupportBean_S0(11, "E4"));
	        var fields = "s0.id,s1.id,s2.id".SplitCsv();
	        env.AssertPropsNew("s0", fields, new object[]{11, 21, 31});

	        env.SendEventBean(new SupportBean_S2(32, "E4"));
	        env.SendEventBean(new SupportBean_S1(22, "E4"));
	        env.AssertListenerNotInvoked("s0");
	    }

	    private static void TryUnsupportedIterator(RegressionEnvironment env) {
	        env.AssertStatement("s0", statement => {
	            try {
	                statement.GetEnumerator();
	                Assert.Fail();
	            } catch (UnsupportedOperationException ex) {
	                Assert.AreEqual("Iteration over a unidirectional join is not supported", ex.Message);
	            }
	        });
	    }
	}
} // end of namespace
