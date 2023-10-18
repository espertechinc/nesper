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
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.epl;

using static com.espertech.esper.regressionlib.support.util.SupportAdminUtil; // AssertStatelessStmt

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.fromclausemethod
{
	public class EPLFromClauseMethod {
	    public static IList<RegressionExecution> Executions() {
	        IList<RegressionExecution> execs = new List<RegressionExecution>();
	        execs.Add(new EPLFromClauseMethod2JoinHistoricalIndependentOuter());
	        execs.Add(new EPLFromClauseMethod2JoinHistoricalSubordinateOuterMultiField());
	        execs.Add(new EPLFromClauseMethod2JoinHistoricalSubordinateOuter());
	        execs.Add(new EPLFromClauseMethod2JoinHistoricalOnlyDependent());
	        execs.Add(new EPLFromClauseMethod2JoinHistoricalOnlyIndependent());
	        execs.Add(new EPLFromClauseMethodNoJoinIterateVariables());
	        execs.Add(new EPLFromClauseMethodOverloaded());
	        execs.Add(new EPLFromClauseMethod2StreamMaxAggregation());
	        execs.Add(new EPLFromClauseMethodDifferentReturnTypes());
	        execs.Add(new EPLFromClauseMethodArrayNoArg());
	        execs.Add(new EPLFromClauseMethodArrayWithArg());
	        execs.Add(new EPLFromClauseMethodObjectNoArg());
	        execs.Add(new EPLFromClauseMethodObjectWithArg());
	        execs.Add(new EPLFromClauseMethodInvocationTargetEx());
	        execs.Add(new EPLFromClauseMethodStreamNameWContext());
	        execs.Add(new EPLFromClauseMethodWithMethodResultParam());
	        execs.Add(new EPLFromClauseMethodInvalid());
	        execs.Add(new EPLFromClauseMethodEventBeanArray());
	        execs.Add(new EPLFromClauseMethodUDFAndScriptReturningEvents());
	        execs.Add(new EPLFromClauseMethod2JoinEventItselfProvidesMethod());
	        return execs;
	    }

	    private class EPLFromClauseMethod2JoinEventItselfProvidesMethod : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "import " + typeof(SupportEventWithStaticMethod).FullName + ";\n" +
	                      "@public @buseventtype create schema SupportEventWithStaticMethod as SupportEventWithStaticMethod;\n" +
	                      "@name('s0') select * from SupportEventWithStaticMethod as e, " +
	                      "  method:SupportEventWithStaticMethod.returnLower() as lower,\n" +
	                      "  method:SupportEventWithStaticMethod.returnUpper() as upper\n" +
	                      "  where e.value in [lower.getValue():upper.getValue()]";
	            env.CompileDeploy(epl).AddListener("s0");

	            SendAssert(env, 9, false);
	            SendAssert(env, 10, true);
	            SendAssert(env, 20, true);
	            SendAssert(env, 21, false);

	            env.UndeployAll();
	        }

	        private void SendAssert(RegressionEnvironment env, int value, bool expected) {
	            env.SendEventBean(new SupportEventWithStaticMethod(value));
	            env.AssertListenerInvokedFlag("s0", expected);
	        }
	    }

	    private class EPLFromClauseMethodWithMethodResultParam : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean as e,\n" +
	                      "method:" + typeof(EPLFromClauseMethod).FullName + ".getWithMethodResultParam('somevalue', e, "
	                      + typeof(EPLFromClauseMethod).FullName + ".getWithMethodResultParamCompute(true)) as s";
	            env.CompileDeploy(epl).AddListener("s0");
	            AssertStatelessStmt(env, "s0", false);

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.AssertPropsNew("s0", "s.p00,s.p01,s.p02".SplitCsv(), new object[]{"somevalue", "E1", "s0"});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodStreamNameWContext : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var epl = "@name('s0') select * from SupportBean as e,\n" +
	                      "method:" + typeof(EPLFromClauseMethod).FullName + ".getStreamNameWContext('somevalue', e) as s";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean("E1", 10));
	            env.AssertPropsNew("s0", "s.p00,s.p01,s.p02".SplitCsv(), new object[]{"somevalue", "E1", "s0"});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodUDFAndScriptReturningEvents : RegressionExecution {
	        public void Run(RegressionEnvironment env) {

	            var path = new RegressionPath();
	            env.CompileDeploy("@buseventtype @public create schema ItemEvent(id string)", path);

	            var script = "@name('script') @public create expression EventBean[] @type(ItemEvent) js:myItemProducerScript() [\n" +
	                         "myItemProducerScript();" +
	                         "function myItemProducerScript() {" +
	                         "  var EventBeanArray = Java.type(\"com.espertech.esper.common.client.EventBean[]\");\n" +
	                         "  var events = new EventBeanArray(2);\n" +
	                         "  events[0] = epl.getEventBeanService().adapterForMap(java.util.Collections.singletonMap(\"id\", \"id1\"), \"ItemEvent\");\n" +
	                         "  events[1] = epl.getEventBeanService().adapterForMap(java.util.Collections.singletonMap(\"id\", \"id3\"), \"ItemEvent\");\n" +
	                         "  return events;\n" +
	                         "}]";
	            env.CompileDeploy(script, path);

	            TryAssertionUDFAndScriptReturningEvents(env, path, "myItemProducerUDF");
	            TryAssertionUDFAndScriptReturningEvents(env, path, "myItemProducerScript");

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodEventBeanArray : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@buseventtype @public create schema MyItemEvent(p0 string)", path);

	            TryAssertionEventBeanArray(env, path, "eventBeanArrayForString", false);
	            TryAssertionEventBeanArray(env, path, "eventBeanArrayForString", true);
	            TryAssertionEventBeanArray(env, path, "eventBeanCollectionForString", false);
	            TryAssertionEventBeanArray(env, path, "eventBeanIteratorForString", false);

	            env.TryInvalidCompile(path, "select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".fetchResult12(0) @type(ItemEvent)",
	                "The @type annotation is only allowed when the invocation target returns EventBean instances");

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodOverloaded : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            TryAssertionOverloaded(env, "", "A", "B");
	            TryAssertionOverloaded(env, "10", "10", "B");
	            TryAssertionOverloaded(env, "10, 20", "10", "20");
	            TryAssertionOverloaded(env, "'x'", "x", "B");
	            TryAssertionOverloaded(env, "'x', 50", "x", "50");
	        }

	        private static void TryAssertionOverloaded(RegressionEnvironment env, string @params, string expectedFirst, string expectedSecond) {
	            var epl = "@name('s0') select col1, col2 from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".overloadedMethodForJoin(" + @params + ")";
	            env.CompileDeploy(epl).AddListener("s0");

	            env.SendEventBean(new SupportBean());
	            env.AssertPropsNew("s0", "col1,col2".SplitCsv(), new object[]{expectedFirst, expectedSecond});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethod2StreamMaxAggregation : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var className = typeof(SupportStaticMethodLib).FullName;
	            string stmtText;
	            var fields = "maxcol1".SplitCsv();

	            // ESPER 556
	            stmtText = "@name('s0') select max(col1) as maxcol1 from SupportBean#unique(theString), method:" + className + ".fetchResult100() ";
	            env.CompileDeploy(stmtText).AddListener("s0");
	            AssertStatelessStmt(env, "s0", false);

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[] {9}});

	            env.SendEventBean(new SupportBean("E1", 1));
	            env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[] {9}});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethod2JoinHistoricalSubordinateOuterMultiField : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var className = typeof(SupportStaticMethodLib).FullName;
	            string stmtText;

	            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	            stmtText = "@name('s0') select intPrimitive,intBoxed,col1,col2 from SupportBean#keepall " +
	                "left outer join " +
	                "method:" + className + ".fetchResult100() " +
	                "on intPrimitive = col1 and intBoxed = col2";

	            var fields = "intPrimitive,intBoxed,col1,col2".SplitCsv();
	            env.CompileDeploy(stmtText).AddListener("s0");
	            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

	            SendSupportBeanEvent(env, 2, 4);
	            env.AssertPropsPerRowLastNew("s0", fields, new object[][]{new object[] {2, 4, 2, 4}});
	            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {2, 4, 2, 4}});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethod2JoinHistoricalSubordinateOuter : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var className = typeof(SupportStaticMethodLib).FullName;
	            string stmtText;

	            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	            stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".fetchResult12(0) as s0 " +
	                "left outer join " +
	                "method:" + className + ".fetchResult23(s0.value) as s1 on s0.value = s1.value";
	            AssertJoinHistoricalSubordinateOuter(env, stmtText);

	            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchResult23(s0.value) as s1 " +
	                "right outer join " +
	                "method:" + className + ".fetchResult12(0) as s0 on s0.value = s1.value";
	            AssertJoinHistoricalSubordinateOuter(env, stmtText);

	            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchResult23(s0.value) as s1 " +
	                "full outer join " +
	                "method:" + className + ".fetchResult12(0) as s0 on s0.value = s1.value";
	            AssertJoinHistoricalSubordinateOuter(env, stmtText);

	            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchResult12(0) as s0 " +
	                "full outer join " +
	                "method:" + className + ".fetchResult23(s0.value) as s1 on s0.value = s1.value";
	            AssertJoinHistoricalSubordinateOuter(env, stmtText);
	        }
	    }

	    private class EPLFromClauseMethod2JoinHistoricalIndependentOuter : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var fields = "valueOne,valueTwo".SplitCsv();
	            var className = typeof(SupportStaticMethodLib).FullName;
	            string stmtText;

	            stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".fetchResult12(0) as s0 " +
	                "left outer join " +
	                "method:" + className + ".fetchResult23(0) as s1 on s0.value = s1.value";
	            env.CompileDeploy(stmtText).AddListener("s0");
	            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, null}, new object[] {2, 2}});
	            env.UndeployAll();

	            stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchResult23(0) as s1 " +
	                "right outer join " +
	                "method:" + className + ".fetchResult12(0) as s0 on s0.value = s1.value";
	            env.CompileDeploy(stmtText);
	            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, null}, new object[] {2, 2}});
	            env.UndeployAll();

	            stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchResult23(0) as s1 " +
	                "full outer join " +
	                "method:" + className + ".fetchResult12(0) as s0 on s0.value = s1.value";
	            env.CompileDeploy(stmtText);
	            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, null}, new object[] {2, 2}, new object[] {null, 3}});
	            env.UndeployAll();

	            stmtText = "@name('s0') select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchResult12(0) as s0 " +
	                "full outer join " +
	                "method:" + className + ".fetchResult23(0) as s1 on s0.value = s1.value";
	            env.CompileDeploy(stmtText);
	            env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, null}, new object[] {2, 2}, new object[] {null, 3}});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethod2JoinHistoricalOnlyDependent : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create variable int lower", path);
	            env.CompileDeploy("@public create variable int upper", path);
	            env.CompileDeploy("on SupportBean set lower=intPrimitive,upper=intBoxed", path);

	            var className = typeof(SupportStaticMethodLib).FullName;
	            string stmtText;

	            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	            stmtText = "select value,result from method:" + className + ".fetchBetween(lower, upper), " +
	                "method:" + className + ".fetchIdDelimited(value)";
	            AssertJoinHistoricalOnlyDependent(env, path, stmtText);

	            stmtText = "select value,result from " +
	                "method:" + className + ".fetchIdDelimited(value), " +
	                "method:" + className + ".fetchBetween(lower, upper)";
	            AssertJoinHistoricalOnlyDependent(env, path, stmtText);

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.OBSERVEROPS);
	        }
	    }

	    private class EPLFromClauseMethod2JoinHistoricalOnlyIndependent : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create variable int lower", path);
	            env.CompileDeploy("@public create variable int upper", path);
	            env.CompileDeploy("on SupportBean set lower=intPrimitive,upper=intBoxed", path);

	            var className = typeof(SupportStaticMethodLib).FullName;
	            string stmtText;

	            // fetchBetween must execute first, fetchIdDelimited is dependent on the result of fetchBetween
	            stmtText = "select s0.value as valueOne, s1.value as valueTwo from method:" + className + ".fetchBetween(lower, upper) as s0, " +
	                "method:" + className + ".fetchBetweenString(lower, upper) as s1";
	            AssertJoinHistoricalOnlyIndependent(env, path, stmtText);

	            stmtText = "select s0.value as valueOne, s1.value as valueTwo from " +
	                "method:" + className + ".fetchBetweenString(lower, upper) as s1, " +
	                "method:" + className + ".fetchBetween(lower, upper) as s0 ";
	            AssertJoinHistoricalOnlyIndependent(env, path, stmtText);

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodNoJoinIterateVariables : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var path = new RegressionPath();
	            env.CompileDeploy("@public create variable int lower", path);
	            env.CompileDeploy("@public create variable int upper", path);
	            env.CompileDeploy("on SupportBean set lower=intPrimitive,upper=intBoxed", path);

	            // Test int and singlerow
	            var className = typeof(SupportStaticMethodLib).FullName;
	            var stmtText = "@name('s0') select value from method:" + className + ".fetchBetween(lower, upper)";
	            env.CompileDeploy(stmtText, path).AddListener("s0");

	            env.AssertPropsPerRowIteratorAnyOrder("s0", new string[]{"value"}, null);

	            SendSupportBeanEvent(env, 5, 10);
	            env.AssertPropsPerRowIteratorAnyOrder("s0", new string[]{"value"}, new object[][]{new object[] {5}, new object[] {6}, new object[] {7}, new object[] {8}, new object[] {9}, new object[] {10}});

	            env.Milestone(0);

	            SendSupportBeanEvent(env, 10, 5);
	            env.AssertPropsPerRowIteratorAnyOrder("s0", new string[]{"value"}, null);

	            SendSupportBeanEvent(env, 4, 4);
	            env.AssertPropsPerRowIteratorAnyOrder("s0", new string[]{"value"}, new object[][]{new object[] {4}});

	            env.AssertListenerNotInvoked("s0");
	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodDifferentReturnTypes : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            TryAssertionSingleRowFetch(env, "fetchMap(theString, intPrimitive)");
	            TryAssertionSingleRowFetch(env, "fetchMapEventBean(s1, 'theString', 'intPrimitive')");
	            TryAssertionSingleRowFetch(env, "fetchObjectArrayEventBean(theString, intPrimitive)");
	            TryAssertionSingleRowFetch(env, "fetchPONOArray(theString, intPrimitive)");
	            TryAssertionSingleRowFetch(env, "fetchPONOCollection(theString, intPrimitive)");
	            TryAssertionSingleRowFetch(env, "fetchPONOIterator(theString, intPrimitive)");

	            TryAssertionReturnTypeMultipleRow(env, "fetchMapArrayMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchOAArrayMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchPONOArrayMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchMapCollectionMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchOACollectionMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchPONOCollectionMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchMapIteratorMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchOAIteratorMR(theString, intPrimitive)");
	            TryAssertionReturnTypeMultipleRow(env, "fetchPONOIteratorMR(theString, intPrimitive)");
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.SERDEREQUIRED);
	        }
	    }

	    private class EPLFromClauseMethodArrayNoArg : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select id, theString from " +
	                                "SupportBean#length(3) as s1, " +
	                                "method:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayNoArg";
	            env.CompileDeploy(joinStatement).AddListener("s0");
	            TryArrayNoArg(env);

	            joinStatement = "@name('s0') select id, theString from " +
	                "SupportBean#length(3) as s1, " +
	                "method:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayNoArg()";
	            env.CompileDeploy(joinStatement).AddListener("s0");
	            TryArrayNoArg(env);

	            env.EplToModelCompileDeploy(joinStatement).AddListener("s0");
	            TryArrayNoArg(env);

	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create("id", "theString");
	            model.FromClause = FromClause.Create()
	                .Add(FilterStream.Create("SupportBean", "s1").AddView("length", Expressions.Constant(3)))
	                .Add(MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "fetchArrayNoArg"));
	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
	            env.CompileDeploy(model).AddListener("s0");
	            Assert.AreEqual(joinStatement, model.ToEPL());

	            TryArrayNoArg(env);
	        }

	        private static void TryArrayNoArg(RegressionEnvironment env) {
	            var fields = new string[]{"id", "theString"};

	            SendBeanEvent(env, "E1");
	            env.AssertPropsNew("s0", fields, new object[]{"1", "E1"});

	            SendBeanEvent(env, "E2");
	            env.AssertPropsNew("s0", fields, new object[]{"1", "E2"});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodArrayWithArg : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select irstream id, theString from " +
	                                "SupportBean()#length(3) as s1, " +
	                                " method:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayGen(intPrimitive)";
	            env.CompileDeploy(joinStatement).AddListener("s0");
	            TryArrayWithArg(env);

	            joinStatement = "@name('s0') select irstream id, theString from " +
	                "method:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayGen(intPrimitive) as s0, " +
	                "SupportBean#length(3)";
	            env.CompileDeploy(joinStatement).AddListener("s0");
	            TryArrayWithArg(env);

	            env.EplToModelCompileDeploy(joinStatement).AddListener("s0");
	            TryArrayWithArg(env);

	            var model = new EPStatementObjectModel();
	            model.SelectClause = SelectClause.Create("id", "theString").SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);
	            model.FromClause = FromClause.Create()
		            .Add(
			            MethodInvocationStream.Create(typeof(SupportStaticMethodLib).FullName, "fetchArrayGen", "s0")
				            .AddParameter(Expressions.Property("intPrimitive")))
		            .Add(FilterStream.Create("SupportBean").AddView("length", Expressions.Constant(3)));

	            model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
	            env.CompileDeploy(model).AddListener("s0");
	            Assert.AreEqual(joinStatement, model.ToEPL());

	            TryArrayWithArg(env);
	        }

	        private static void TryArrayWithArg(RegressionEnvironment env) {

	            var fields = new string[]{"id", "theString"};

	            SendBeanEvent(env, "E1", -1);
	            env.AssertListenerNotInvoked("s0");

	            SendBeanEvent(env, "E2", 0);
	            env.AssertListenerNotInvoked("s0");

	            SendBeanEvent(env, "E3", 1);
	            env.AssertPropsNew("s0", fields, new object[]{"A", "E3"});

	            SendBeanEvent(env, "E4", 2);
	            env.AssertListener("s0", listener => {
	                EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"A", "E4"}, new object[] {"B", "E4"}});
	                Assert.IsNull(listener.LastOldData);
	                listener.Reset();
	            });

	            SendBeanEvent(env, "E5", 3);
	            env.AssertListener("s0", listener => {
	                EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, fields, new object[][]{new object[] {"A", "E5"}, new object[] {"B", "E5"}, new object[] {"C", "E5"}});
	                Assert.IsNull(listener.LastOldData);
	                listener.Reset();
	            });

	            SendBeanEvent(env, "E6", 1);
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[] {"A", "E6"}}, new object[][]{new object[] {"A", "E3"}});

	            SendBeanEvent(env, "E7", 1);
	            env.AssertPropsPerRowIRPair("s0", fields, new object[][]{new object[] {"A", "E7"}}, new object[][]{new object[] {"A", "E4"}, new object[] {"B", "E4"}});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodObjectNoArg : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select id, theString from " +
	                                "SupportBean()#length(3) as s1, " +
	                                " method:" + typeof(SupportStaticMethodLib).FullName + ".fetchObjectNoArg()";
	            env.CompileDeploy(joinStatement).AddListener("s0");
	            var fields = new string[]{"id", "theString"};

	            SendBeanEvent(env, "E1");
	            env.AssertPropsNew("s0", fields, new object[]{"2", "E1"});

	            env.Milestone(0);

	            SendBeanEvent(env, "E2");
	            env.AssertPropsNew("s0", fields, new object[]{"2", "E2"});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodObjectWithArg : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "@name('s0') select id, theString from " +
	                                "SupportBean()#length(3) as s1, " +
	                                " method:" + typeof(SupportStaticMethodLib).FullName + ".fetchObject(theString)";
	            env.CompileDeploy(joinStatement).AddListener("s0");

	            var fields = new string[]{"id", "theString"};

	            SendBeanEvent(env, "E1");
	            env.AssertPropsNew("s0", fields, new object[]{"|E1|", "E1"});

	            SendBeanEvent(env, null);
	            env.AssertListenerNotInvoked("s0");

	            SendBeanEvent(env, "E2");
	            env.AssertPropsNew("s0", fields, new object[]{"|E2|", "E2"});

	            env.UndeployAll();
	        }
	    }

	    private class EPLFromClauseMethodInvocationTargetEx : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            var joinStatement = "select s1.theString from " +
	                                "SupportBean()#length(3) as s1, " +
	                                " method:" + typeof(SupportStaticMethodLib).FullName + ".throwExceptionBeanReturn()";

	            env.CompileDeploy(joinStatement);

	            try {
	                SendBeanEvent(env, "E1");
	                Assert.Fail(); // default test configuration rethrows this exception
	            } catch (EPException ex) {
	                // fine
	            }

	            env.UndeployAll();
	        }

	        public ISet<RegressionFlag> Flags() {
	            return Collections.Set(RegressionFlag.INVALIDITY);
	        }
	    }

	    private class EPLFromClauseMethodInvalid : RegressionExecution {
	        public void Run(RegressionEnvironment env) {
	            env.TryInvalidCompile("select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayGen()",
	                "Method footprint does not match the number or type of expression parameters, expecting no parameters in method: Could not find static method named 'fetchArrayGen' in class '" + typeof(SupportStaticMethodLib).FullName + "' taking no parameters (nearest match found was 'fetchArrayGen' taking type(s) 'int') [");

	            env.TryInvalidCompile("select * from SupportBean, method:.abc where 1=2",
	                "Incorrect syntax near '.' at line 1 column 34, please check the method invocation join within the from clause [select * from SupportBean, method:.abc where 1=2]");

	            env.TryInvalidCompile("select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".fetchObjectAndSleep(1)",
	                "Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed 'int': Could not find static method named 'fetchObjectAndSleep' in class '" + typeof(SupportStaticMethodLib).FullName + "' with matching parameter number and expected parameter type(s) 'int' (nearest match found was 'fetchObjectAndSleep' taking type(s) 'String, int, long') [");

	            env.TryInvalidCompile("select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".sleep(100) where 1=2",
	                "Invalid return type for static method 'sleep' of class '" + typeof(SupportStaticMethodLib).FullName + "', expecting a Java class [select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".sleep(100) where 1=2]");

	            env.TryInvalidCompile("select * from SupportBean, method:AClass. where 1=2",
	                "Incorrect syntax near 'where' (a reserved keyword) expecting an identifier but found 'where' at line 1 column 42, please check the view specifications within the from clause [select * from SupportBean, method:AClass. where 1=2]");

	            env.TryInvalidCompile("select * from SupportBean, method:Dummy.abc where 1=2",
	                "Could not load class by name 'Dummy', please check imports [select * from SupportBean, method:Dummy.abc where 1=2]");

	            env.TryInvalidCompile("select * from SupportBean, method:Math where 1=2",
	                "A function named 'Math' is not defined");

	            env.TryInvalidCompile("select * from SupportBean, method:Dummy.dummy()#length(100) where 1=2",
	                "Method data joins do not allow views onto the data, view 'length' is not valid in this context [select * from SupportBean, method:Dummy.dummy()#length(100) where 1=2]");

	            env.TryInvalidCompile("select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".dummy where 1=2",
	                "Could not find public static method named 'dummy' in class '" + typeof(SupportStaticMethodLib).FullName + "' [");

	            env.TryInvalidCompile("select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".minusOne(10) where 1=2",
	                "Invalid return type for static method 'minusOne' of class '" + typeof(SupportStaticMethodLib).FullName + "', expecting a Java class [");

	            env.TryInvalidCompile("select * from SupportBean, xyz:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayNoArg() where 1=2",
	                "Expecting keyword 'method', found 'xyz' [select * from SupportBean, xyz:" + typeof(SupportStaticMethodLib).FullName + ".fetchArrayNoArg() where 1=2]");

	            env.TryInvalidCompile("select * from method:" + typeof(SupportStaticMethodLib).FullName + ".fetchBetween(s1.value, s1.value) as s0, method:" + typeof(SupportStaticMethodLib).FullName + ".fetchBetween(s0.value, s0.value) as s1",
	                "Circular dependency detected between historical streams [");

	            env.TryInvalidCompile("select * from method:" + typeof(SupportStaticMethodLib).FullName + ".fetchBetween(s0.value, s0.value) as s0, method:" + typeof(SupportStaticMethodLib).FullName + ".fetchBetween(s0.value, s0.value) as s1",
	                "Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");

	            env.TryInvalidCompile("select * from method:" + typeof(SupportStaticMethodLib).FullName + ".fetchBetween(s0.value, s0.value) as s0",
	                "Parameters for historical stream 0 indicate that the stream is subordinate to itself as stream parameters originate in the same stream [");

	            env.TryInvalidCompile("select * from method:SupportMethodInvocationJoinInvalid.readRowNoMetadata()",
	                "Could not find getter method for method invocation, expected a method by name 'readRowNoMetadataMetadata' accepting no parameters [select * from method:SupportMethodInvocationJoinInvalid.readRowNoMetadata()]");

	            env.TryInvalidCompile("select * from method:SupportMethodInvocationJoinInvalid.readRowWrongMetadata()",
	                "Getter method 'readRowWrongMetadataMetadata' does not return System.Collections.Generic.IDictionary [select * from method:SupportMethodInvocationJoinInvalid.readRowWrongMetadata()]");

	            env.TryInvalidCompile("select * from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + ".invalidOverloadForJoin(null)",
	                "Method by name 'invalidOverloadForJoin' is overloaded in class '" + typeof(SupportStaticMethodLib).FullName + "' and overloaded methods do not return the same type");
	        }
	    }

	    private static void TryAssertionUDFAndScriptReturningEvents(RegressionEnvironment env, RegressionPath path, string methodName) {
	        env.CompileDeploy("@name('s0') select id from SupportBean, method:" + methodName, path).AddListener("s0");

	        env.SendEventBean(new SupportBean());
	        env.AssertPropsPerRowLastNew("s0", "id".SplitCsv(), new object[][]{new object[] {"id1"}, new object[] {"id3"}});

	        env.UndeployModuleContaining("s0");
	    }

	    private static void TryAssertionEventBeanArray(RegressionEnvironment env, RegressionPath path, string methodName, bool soda) {
	        var epl = "@name('s0') select p0 from SupportBean, method:" + typeof(SupportStaticMethodLib).FullName + "." + methodName + "(theString) @type(MyItemEvent)";
	        env.CompileDeploy(soda, epl, path).AddListener("s0");

	        env.SendEventBean(new SupportBean("a,b", 0));
	        env.AssertPropsPerRowLastNew("s0", "p0".SplitCsv(), new object[][]{new object[] {"a"}, new object[] {"b"}});

	        env.UndeployModuleContaining("s0");
	    }

	    private static void SendBeanEvent(RegressionEnvironment env, string theString) {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        env.SendEventBean(bean);
	    }

	    private static void SendBeanEvent(RegressionEnvironment env, string theString, int intPrimitive) {
	        var bean = new SupportBean();
	        bean.TheString = theString;
	        bean.IntPrimitive = intPrimitive;
	        env.SendEventBean(bean);
	    }

	    private static void SendSupportBeanEvent(RegressionEnvironment env, int intPrimitive, int intBoxed) {
	        var bean = new SupportBean();
	        bean.IntPrimitive = intPrimitive;
	        bean.IntBoxed = intBoxed;
	        env.SendEventBean(bean);
	    }

	    public static EventBean[] MyItemProducerUDF(EPLMethodInvocationContext context) {
	        var events = new EventBean[2];
	        var count = 0;
	        foreach (var id in "id1,id3".SplitCsv()) {
	            events[count++] = context.EventBeanService.AdapterForMap(Collections.SingletonDataMap("id", id), "ItemEvent");
	        }
	        return events;
	    }

	    private static void AssertJoinHistoricalSubordinateOuter(RegressionEnvironment env, string expression) {
	        var fields = "valueOne,valueTwo".SplitCsv();
	        env.CompileDeploy("@name('s0') " + expression).AddListener("s0");
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, null}, new object[] {2, 2}});
	        env.UndeployAll();
	    }

	    private static void AssertJoinHistoricalOnlyDependent(RegressionEnvironment env, RegressionPath path, string expression) {
	        env.CompileDeploy("@name('s0') " + expression, path).AddListener("s0");

	        var fields = "value,result".SplitCsv();
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

	        SendSupportBeanEvent(env, 5, 5);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {5, "|5|"}});

	        SendSupportBeanEvent(env, 1, 2);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, "|1|"}, new object[] {2, "|2|"}});

	        SendSupportBeanEvent(env, 0, -1);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

	        SendSupportBeanEvent(env, 4, 6);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {4, "|4|"}, new object[] {5, "|5|"}, new object[] {6, "|6|"}});

	        var listener = env.Listener("s0");
	        env.UndeployModuleContaining("s0");

	        SendSupportBeanEvent(env, 0, -1);
	        Assert.IsFalse(listener.IsInvoked);
	    }

	    private static void AssertJoinHistoricalOnlyIndependent(RegressionEnvironment env, RegressionPath path, string expression) {
	        env.CompileDeploy("@name('s0') " + expression, path).AddListener("s0");

	        var fields = "valueOne,valueTwo".SplitCsv();
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

	        SendSupportBeanEvent(env, 5, 5);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {5, "5"}});

	        SendSupportBeanEvent(env, 1, 2);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, new object[][]{new object[] {1, "1"}, new object[] {1, "2"}, new object[] {2, "1"}, new object[] {2, "2"}});

	        SendSupportBeanEvent(env, 0, -1);
	        env.AssertPropsPerRowIteratorAnyOrder("s0", fields, null);

	        env.UndeployModuleContaining("s0");
	    }

	    private static void TryAssertionSingleRowFetch(RegressionEnvironment env, string method) {
	        var epl = "@name('s0') select theString, intPrimitive, mapstring, mapint from " +
	                  "SupportBean as s1, " +
	                  "method:" + typeof(SupportStaticMethodLib).FullName + "." + method;
	        env.CompileDeploy(epl).AddListener("s0");

	        var fields = new string[]{"theString", "intPrimitive", "mapstring", "mapint"};

	        SendBeanEvent(env, "E1", 1);
	        env.AssertPropsNew("s0", fields, new object[]{"E1", 1, "|E1|", 2});

	        SendBeanEvent(env, "E2", 3);
	        env.AssertPropsNew("s0", fields, new object[]{"E2", 3, "|E2|", 4});

	        SendBeanEvent(env, "E3", 0);
	        env.AssertPropsNew("s0", fields, new object[]{"E3", 0, null, null});

	        SendBeanEvent(env, "E4", -1);
	        env.AssertListenerNotInvoked("s0");

	        env.UndeployAll();
	    }

	    private static void TryAssertionReturnTypeMultipleRow(RegressionEnvironment env, string method) {
	        var epl = "@name('s0') select theString, intPrimitive, mapstring, mapint from " +
	                  "SupportBean#keepall as s1, " +
	                  "method:" + typeof(SupportStaticMethodLib).FullName + "." + method;
	        var fields = "theString,intPrimitive,mapstring,mapint".SplitCsv();
	        env.CompileDeploy(epl).AddListener("s0");

	        env.AssertPropsPerRowIterator("s0", fields, null);

	        SendBeanEvent(env, "E1", 0);
	        env.AssertListenerNotInvoked("s0");
	        env.AssertPropsPerRowIterator("s0", fields, null);

	        SendBeanEvent(env, "E2", -1);
	        env.AssertListenerNotInvoked("s0");
	        env.AssertPropsPerRowIterator("s0", fields, null);

	        SendBeanEvent(env, "E3", 1);
	        env.AssertPropsNew("s0", fields, new object[]{"E3", 1, "|E3_0|", 100});
	        env.AssertPropsPerRowIterator("s0", fields, new object[][]{new object[] {"E3", 1, "|E3_0|", 100}});

	        SendBeanEvent(env, "E4", 2);
	        env.AssertPropsPerRowLastNew("s0", fields,
	            new object[][]{new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101}});
	        env.AssertPropsPerRowIterator("s0", fields, new object[][]{new object[] {"E3", 1, "|E3_0|", 100}, new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101}});

	        SendBeanEvent(env, "E5", 3);
	        env.AssertPropsPerRowLastNew("s0", fields,
	            new object[][]{new object[] {"E5", 3, "|E5_0|", 100}, new object[] {"E5", 3, "|E5_1|", 101}, new object[] {"E5", 3, "|E5_2|", 102}});
	        env.AssertPropsPerRowIterator("s0", fields, new object[][]{new object[] {"E3", 1, "|E3_0|", 100},
	            new object[] {"E4", 2, "|E4_0|", 100}, new object[] {"E4", 2, "|E4_1|", 101},
	            new object[] {"E5", 3, "|E5_0|", 100}, new object[] {"E5", 3, "|E5_1|", 101}, new object[] {"E5", 3, "|E5_2|", 102}});

	        env.UndeployAll();
	    }

	    public static SupportBean_S0 GetStreamNameWContext(string a, SupportBean bean, EPLMethodInvocationContext context) {
	        return new SupportBean_S0(1, a, bean.TheString, context.StatementName);
	    }

	    public static SupportBean_S0 GetWithMethodResultParam(string a, SupportBean bean, string b) {
	        return new SupportBean_S0(1, a, bean.TheString, b);
	    }

	    public static string GetWithMethodResultParamCompute(bool param, EPLMethodInvocationContext context) {
	        return context.StatementName;
	    }

	    /// <summary>
	    /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
	    /// </summary>
	    [Serializable] public class SupportEventWithStaticMethod {
	        	        private readonly int value;

	        public SupportEventWithStaticMethod(int value) {
	            this.value = value;
	        }

	        public int GetValue() {
	            return value;
	        }

	        public static SupportEventWithStaticMethodValue ReturnLower() {
	            return new SupportEventWithStaticMethodValue(10);
	        }

	        public static SupportEventWithStaticMethodValue ReturnUpper() {
	            return new SupportEventWithStaticMethodValue(20);
	        }
	    }

	    /// <summary>
	    /// Test event; only serializable because it *may* go over the wire  when running remote tests and serialization is just convenient. Serialization generally not used for HA and HA testing.
	    /// </summary>
	    [Serializable] public class SupportEventWithStaticMethodValue {
	        	        private readonly int value;

	        public SupportEventWithStaticMethodValue(int value) {
	            this.value = value;
	        }

	        public int GetValue() {
	            return value;
	        }
	    }
	}
} // end of namespace
