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

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.define
{
    public class ExprDefineBasic
    {
        private static readonly string NEWLINE = Environment.NewLine;

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithExpressionSimpleSameStmt(execs);
            WithExpressionSimpleSameModule(execs);
            WithExpressionSimpleTwoModule(execs);
            WithAggregationNoAccess(execs);
            WithAggregatedResult(execs);
            WithAggregationAccess(execs);
            WithWildcardAndPattern(execs);
            WithScalarReturn(execs);
            WithNoParameterArithmetic(execs);
            WithOneParameterLambdaReturn(execs);
            WithNoParameterVariable(execs);
            WithAnnotationOrder(execs);
            WithWhereClauseExpression(execs);
            WithSequenceAndNested(execs);
            WithCaseNewMultiReturnNoElse(execs);
            WithSubqueryMultiresult(execs);
            WithSubqueryCross(execs);
            WithSubqueryJoinSameField(execs);
            WithSubqueryCorrelated(execs);
            WithSubqueryUncorrelated(execs);
            WithSubqueryNamedWindowUncorrelated(execs);
            WithSubqueryNamedWindowCorrelated(execs);
            WithNestedExpressionMultiSubquery(execs);
            WithEventTypeAndSODA(execs);
            WithInvalid(execs);
            WithSplitStream(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSplitStream(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSplitStream());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithEventTypeAndSODA(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineEventTypeAndSODA());
            return execs;
        }

        public static IList<RegressionExecution> WithNestedExpressionMultiSubquery(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineNestedExpressionMultiSubquery());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryNamedWindowCorrelated(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryNamedWindowCorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryNamedWindowUncorrelated(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryNamedWindowUncorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryUncorrelated(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryUncorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryCorrelated(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryCorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryJoinSameField(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryJoinSameField());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryCross(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryCross());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryMultiresult(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSubqueryMultiresult());
            return execs;
        }

        public static IList<RegressionExecution> WithCaseNewMultiReturnNoElse(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineCaseNewMultiReturnNoElse());
            return execs;
        }

        public static IList<RegressionExecution> WithSequenceAndNested(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineSequenceAndNested());
            return execs;
        }

        public static IList<RegressionExecution> WithWhereClauseExpression(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineWhereClauseExpression());
            return execs;
        }

        public static IList<RegressionExecution> WithAnnotationOrder(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineAnnotationOrder());
            return execs;
        }

        public static IList<RegressionExecution> WithNoParameterVariable(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineNoParameterVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithOneParameterLambdaReturn(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineOneParameterLambdaReturn());
            return execs;
        }

        public static IList<RegressionExecution> WithNoParameterArithmetic(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineNoParameterArithmetic());
            return execs;
        }

        public static IList<RegressionExecution> WithScalarReturn(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineScalarReturn());
            return execs;
        }

        public static IList<RegressionExecution> WithWildcardAndPattern(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineWildcardAndPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationAccess(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineAggregationAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregatedResult(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineAggregatedResult());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregationNoAccess(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineAggregationNoAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionSimpleTwoModule(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineExpressionSimpleTwoModule());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionSimpleSameModule(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineExpressionSimpleSameModule());
            return execs;
        }

        public static IList<RegressionExecution> WithExpressionSimpleSameStmt(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ExprDefineExpressionSimpleSameStmt());
            return execs;
        }

        private static void TryAssertionAggregationAccess(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            SupportEventPropUtil.AssertTypes(
                env.Statement("s0").EventType,
                new[] {"val1"},
                new[] {typeof(ICollection<object>)});

            env.SendEventBean(new SupportBean("E1", 2));
            var outArray = env.Listener("s0")
                .AssertOneGetNewAndReset()
                .Get("val1")
                .UnwrapIntoArray<SupportBean>();

            Assert.AreEqual(0, outArray.Length);

            env.SendEventBean(new SupportBean("E2", 3));
            outArray = env.Listener("s0")
                .AssertOneGetNewAndReset()
                .Get("val1")
                .UnwrapIntoArray<SupportBean>();

            Assert.AreEqual(1, outArray.Length);
            Assert.AreEqual("E2", outArray[0].TheString);

            env.UndeployAll();
        }

        private static SupportBean GetSupportBean(
            int intPrimitive,
            int? intBoxed)
        {
            var b = new SupportBean(null, intPrimitive);
            b.IntBoxed = intBoxed;
            return b;
        }

        private static IDictionary<string, object>[] ToArrayMap(ICollection<object> items)
        {
            if (items == null) {
                return null;
            }

            IList<IDictionary<string, object>> result = new List<IDictionary<string, object>>();
            foreach (var item in items) {
                var map = (IDictionary<string, object>) item;
                result.Add(map);
            }

            return result.ToArray();
        }

        internal class ExprDefineExpressionSimpleSameStmt : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@Name('s0') expression returnsOne {1} select returnsOne as c0 from SupportBean")
                    .AddListener("s0");
                Assert.AreEqual(StatementType.SELECT, env.Statement("s0").GetProperty(StatementProperty.STATEMENTTYPE));
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployAll();
            }
        }

        internal class ExprDefineExpressionSimpleSameModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                        "create expression returnsOne {1};\n" +
                        "@Name('s0') select returnsOne as c0 from SupportBean;\n")
                    .AddListener("s0");
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployAll();
            }
        }

        internal class ExprDefineExpressionSimpleTwoModule : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create expression returnsOne {1}", path);
                env.CompileDeploy("@Name('s0') select returnsOne as c0 from SupportBean", path).AddListener("s0");
                env.SendEventBean(new SupportBean());
                Assert.AreEqual(1, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                env.UndeployAll();
            }
        }

        internal class ExprDefineNestedExpressionMultiSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0"};

                var path = new RegressionPath();
                env.CompileDeploy("create expression F1 { (select IntPrimitive from SupportBean#lastevent)}", path);
                env.CompileDeploy(
                    "create expression F2 { param => (select a.IntPrimitive from SupportBean#unique(TheString) as a where a.TheString = param.TheString) }",
                    path);
                env.CompileDeploy("create expression F3 { s => F1()+F2(s) }", path);
                env.CompileDeploy("@Name('s0') select F3(myevent) as c0 from SupportBean as myevent", path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {20});

                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {22});

                env.UndeployAll();
            }
        }

        internal class ExprDefineWildcardAndPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNonJoin =
                    "@Name('s0') expression abc { x => IntPrimitive } " +
                    "expression def { (x, y) => x.IntPrimitive * y.IntPrimitive }" +
                    "select abc(*) as c0, def(*, *) as c1 from SupportBean";
                env.CompileDeploy(eplNonJoin).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"c0", " c1"},
                    new object[] {2, 4});
                env.UndeployAll();

                var eplPattern = "@Name('s0') expression abc { x => IntPrimitive * 2} " +
                                 "select * from pattern [a=SupportBean -> b=SupportBean(IntPrimitive = abc(a))]";
                env.CompileDeploy(eplPattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 2));
                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"a.TheString", " b.TheString"},
                    new object[] {"E1", "E2"});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSequenceAndNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window WindowOne#keepall as (col1 string, col2 string)", path);
                env.CompileDeploy("insert into WindowOne select P00 as col1, P01 as col2 from SupportBean_S0", path);

                env.CompileDeploy("create window WindowTwo#keepall as (col1 string, col2 string)", path);
                env.CompileDeploy("insert into WindowTwo select P10 as col1, P11 as col2 from SupportBean_S1", path);

                env.SendEventBean(new SupportBean_S0(1, "A", "B1"));
                env.SendEventBean(new SupportBean_S0(2, "A", "B2"));

                env.SendEventBean(new SupportBean_S1(11, "A", "B1"));
                env.SendEventBean(new SupportBean_S1(12, "A", "B2"));

                var epl = "@Name('s0') @Audit('exprdef') " +
                          "expression last2X {\n" +
                          "  p => WindowOne(WindowOne.col1 = p.TheString).takeLast(2)\n" +
                          "} " +
                          "expression last2Y {\n" +
                          "  p => WindowTwo(WindowTwo.col1 = p.TheString).takeLast(2).selectFrom(q => q.col2)\n" +
                          "} " +
                          "select last2X(sb).selectFrom(a => a.col2).sequenceEqual(last2Y(sb)) as val from SupportBean as sb";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("A", 1));
                Assert.AreEqual(true, env.Listener("s0").AssertOneGetNewAndReset().Get("val"));

                env.UndeployAll();
            }
        }

        internal class ExprDefineCaseNewMultiReturnNoElse : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fieldsInner = new[] {"col1", "col2"};
                var epl = "@Name('s0') expression gettotal {" +
                          " x => case " +
                          "  when TheString = 'A' then new { col1 = 'X', col2 = 10 } " +
                          "  when TheString = 'B' then new { col1 = 'Y', col2 = 20 } " +
                          "end" +
                          "} " +
                          "insert into OtherStream select gettotal(sb) as val0 from SupportBean sb";
                env.CompileDeploy(epl, path).AddListener("s0");

                Assert.AreEqual(
                    typeof(IDictionary<string, object>),
                    env.Statement("s0").EventType.GetPropertyType("val0"));

                env.CompileDeploy("@Name('s1') select val0.col1 as c1, val0.col2 as c2 from OtherStream", path)
                    .AddListener("s1");
                var fieldsConsume = new[] {"c1", "c2"};

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    null,
                    null);
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsConsume,
                    new object[] {null, null});

                env.SendEventBean(new SupportBean("A", 2));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    "X",
                    10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsConsume,
                    new object[] {"X", 10});

                env.SendEventBean(new SupportBean("B", 3));
                EPAssertionUtil.AssertPropsMap(
                    (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                    fieldsInner,
                    "Y",
                    20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s1").AssertOneGetNewAndReset(),
                    fieldsConsume,
                    new object[] {"Y", 20});

                env.UndeployAll();
            }
        }

        internal class ExprDefineAnnotationOrder : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "expression scalar {1} @Name('s0') select scalar() from SupportBean_ST0";
                TryAssertionAnnotation(env, epl);

                epl = "@Name('s0') expression scalar {1} select scalar() from SupportBean_ST0";
                TryAssertionAnnotation(env, epl);
            }

            private void TryAssertionAnnotation(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("scalar()"));
                Assert.AreEqual("s0", env.Statement("s0").Name);

                env.SendEventBean(new SupportBean_ST0("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"scalar()"},
                    new object[] {1});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryMultiresult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "@Name('s0') " +
                             "expression maxi {" +
                             " (select max(IntPrimitive) from SupportBean#keepall)" +
                             "} " +
                             "expression mini {" +
                             " (select min(IntPrimitive) from SupportBean#keepall)" +
                             "} " +
                             "select P00/maxi() as val0, P00/mini() as val1 " +
                             "from SupportBean_ST0#lastevent";
                TryAssertionMultiResult(env, eplOne);

                var eplTwo = "@Name('s0') " +
                             "expression subq {" +
                             " (select max(IntPrimitive) as maxi, min(IntPrimitive) as mini from SupportBean#keepall)" +
                             "} " +
                             "select P00/subq().maxi as val0, P00/subq().mini as val1 " +
                             "from SupportBean_ST0#lastevent";
                TryAssertionMultiResult(env, eplTwo);

                var eplTwoAlias = "@Name('s0') " +
                                  "expression subq alias for " +
                                  " { (select max(IntPrimitive) as maxi, min(IntPrimitive) as mini from SupportBean#keepall) }" +
                                  " " +
                                  "select P00/subq().maxi as val0, P00/subq().mini as val1 " +
                                  "from SupportBean_ST0#lastevent";
                TryAssertionMultiResult(env, eplTwoAlias);
            }

            private void TryAssertionMultiResult(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val0", "val1"};
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                env.SendEventBean(new SupportBean("E2", 5));
                env.SendEventBean(new SupportBean_ST0("ST0", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2 / 10d, 2 / 5d});

                env.SendEventBean(new SupportBean("E3", 20));
                env.SendEventBean(new SupportBean("E4", 2));
                env.SendEventBean(new SupportBean_ST0("ST0", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4 / 20d, 4 / 2d});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryCross : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "@Name('s0') expression subq {" +
                                 " (x, y) => (select TheString from SupportBean#keepall where TheString = x.Id and IntPrimitive = y.P10)" +
                                 "} " +
                                 "select subq(one, two) as val1 " +
                                 "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryAssertionSubqueryCross(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subq alias for { (select TheString from SupportBean#keepall where TheString = one.Id and IntPrimitive = two.P10) }" +
                    "select subq as val1 " +
                    "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryAssertionSubqueryCross(env, eplAlias);
            }

            private void TryAssertionSubqueryCross(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val1"};
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(env.Statement("s0").EventType, fields, new[] {typeof(string)});

                env.SendEventBean(new SupportBean_ST0("ST0", 0));
                env.SendEventBean(new SupportBean_ST1("ST1", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.SendEventBean(new SupportBean("ST0", 20));

                env.SendEventBean(new SupportBean_ST1("x", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"ST0"});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryJoinSameField : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "@Name('s0') " +
                                 "expression subq {" +
                                 " x => (select IntPrimitive from SupportBean#keepall where TheString = x.Pcommon)" + // a common field
                                 "} " +
                                 "select subq(one) as val1, subq(two) as val2 " +
                                 "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryAssertionSubqueryJoinSameField(env, eplDeclare);

                var eplAlias = "@Name('s0') " +
                               "expression subq alias for {(select IntPrimitive from SupportBean#keepall where TheString = Pcommon) }" +
                               "select subq as val1, subq as val2 " +
                               "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryInvalidCompile(
                    env,
                    eplAlias,
                    "Failed to plan subquery number 1 querying SupportBean: Failed to validate filter expression 'TheString=Pcommon': Property named 'Pcommon' is ambiguous as is valid for more then one stream");
            }

            private void TryAssertionSubqueryJoinSameField(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val1", "val2"};
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(new SupportBean_ST0("ST0", 0));
                env.SendEventBean(new SupportBean_ST1("ST1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(new SupportBean("E0", 10));
                env.SendEventBean(new SupportBean_ST1("ST1", 0, "E0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, 10});

                env.SendEventBean(new SupportBean_ST0("ST0", 0, "E0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 10});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "@Name('s0') expression subqOne {" +
                                 " x => (select Id from SupportBean_ST0#keepall where P00 = x.IntPrimitive)" +
                                 "} " +
                                 "select TheString as val0, subqOne(t) as val1 from SupportBean as t";
                TryAssertionSubqueryCorrelated(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subqOne alias for {(select Id from SupportBean_ST0#keepall where P00 = t.IntPrimitive)} " +
                    "select TheString as val0, subqOne() as val1 from SupportBean as t";
                TryAssertionSubqueryCorrelated(env, eplAlias);
            }

            private void TryAssertionSubqueryCorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val0", "val1"};
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(string), typeof(string)});

                env.SendEventBean(new SupportBean("E0", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E0", null});

                env.SendEventBean(new SupportBean_ST0("ST0", 100));
                env.SendEventBean(new SupportBean("E1", 99));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", null});

                env.SendEventBean(new SupportBean("E2", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "ST0"});

                env.SendEventBean(new SupportBean_ST0("ST1", 100));
                env.SendEventBean(new SupportBean("E3", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", null});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "@Name('s0') expression subqOne {(select Id from SupportBean_ST0#lastevent)} " +
                                 "select TheString as val0, subqOne() as val1 from SupportBean as t";
                TryAssertionSubqueryUncorrelated(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subqOne alias for {(select Id from SupportBean_ST0#lastevent)} " +
                    "select TheString as val0, subqOne as val1 from SupportBean as t";
                TryAssertionSubqueryUncorrelated(env, eplAlias);
            }

            private void TryAssertionSubqueryUncorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val0", "val1"};
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(string), typeof(string)});

                env.SendEventBean(new SupportBean("E0", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E0", null});

                env.SendEventBean(new SupportBean_ST0("ST0", 0));
                env.SendEventBean(new SupportBean("E1", 99));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "ST0"});

                env.SendEventBean(new SupportBean_ST0("ST1", 0));
                env.SendEventBean(new SupportBean("E2", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "ST1"});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryNamedWindowUncorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare =
                    "@Name('s0') expression subqnamedwin { MyWindow.where(x => x.val1 > 10).orderBy(x => x.val0) } " +
                    "select subqnamedwin() as c0, subqnamedwin().where(x => x.val1 < 100) as c1 from SupportBean_ST0 as t";
                TryAssertionSubqueryNamedWindowUncorrelated(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subqnamedwin alias for {MyWindow.where(x => x.val1 > 10).orderBy(x => x.val0)}" +
                    "select subqnamedwin as c0, subqnamedwin.where(x => x.val1 < 100) as c1 from SupportBean_ST0";
                TryAssertionSubqueryNamedWindowUncorrelated(env, eplAlias);
            }

            private void TryAssertionSubqueryNamedWindowUncorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fieldsSelected = new[] {"c0", "c1"};
                var fieldsInside = new[] {"val0"};

                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.MAP.GetAnnotationText() +
                    " create window MyWindow#keepall as (val0 string, val1 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyWindow (val0, val1) select TheString, IntPrimitive from SupportBean",
                    path);
                env.CompileDeploy(epl, path).AddListener("s0");

                var inner = typeof(ICollection<IDictionary<string, object>>);
                SupportEventPropUtil.AssertTypes(env.Statement("s0").EventType, fieldsSelected, new[] {inner, inner});

                env.SendEventBean(new SupportBean("E0", 0));
                env.SendEventBean(new SupportBean_ST0("ID0", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldsInside,
                    null);
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c1").Unwrap<object>()),
                    fieldsInside,
                    null);
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E1", 11));
                env.SendEventBean(new SupportBean_ST0("ID1", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldsInside,
                    new[] {new object[] {"E1"}});
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c1").Unwrap<object>()),
                    fieldsInside,
                    new[] {new object[] {"E1"}});
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E2", 500));
                env.SendEventBean(new SupportBean_ST0("ID2", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldsInside,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c1").Unwrap<object>()),
                    fieldsInside,
                    new[] {new object[] {"E1"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprDefineSubqueryNamedWindowCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') expression subqnamedwin {" +
                          "  x => MyWindow(val0 = x.Key0).where(y => val1 > 10)" +
                          "} " +
                          "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // more or less prefixes
                epl = "@Name('s0') expression subqnamedwin {" +
                      "  x => MyWindow(val0 = x.Key0).where(y => y.val1 > 10)" +
                      "} " +
                      "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // with property-explicit stream name
                epl = "@Name('s0') expression subqnamedwin {" +
                      "  x => MyWindow(MyWindow.val0 = x.Key0).where(y => y.val1 > 10)" +
                      "} " +
                      "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // with alias
                epl =
                    "@Name('s0') expression subqnamedwin alias for {MyWindow(MyWindow.val0 = t.Key0).where(y => y.val1 > 10)}" +
                    "select subqnamedwin as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // test ambiguous property names
                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.MAP.GetAnnotationText() +
                    " create window MyWindowTwo#keepall as (Id string, P00 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyWindowTwo (Id, P00) select TheString, IntPrimitive from SupportBean",
                    path);
                epl = "expression subqnamedwin {" +
                      "  x => MyWindowTwo(MyWindowTwo.Id = x.Id).where(y => y.P00 > 10)" +
                      "} " +
                      "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                env.CompileDeploy(epl, path);
                env.UndeployAll();
            }

            private void TryAssertionSubqNWCorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fieldSelected = new[] {"c0"};
                var fieldInside = new[] {"val0"};

                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.MAP.GetAnnotationText() +
                    " create window MyWindow#keepall as (val0 string, val1 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyWindow (val0, val1) select TheString, IntPrimitive from SupportBean",
                    path);
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fieldSelected,
                    new[] {
                        typeof(ICollection<IDictionary<string, object>>)
                    });

                env.SendEventBean(new SupportBean("E0", 0));
                env.SendEventBean(new SupportBean_ST0("ID0", "x", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldInside,
                    null);
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E1", 11));
                env.SendEventBean(new SupportBean_ST0("ID1", "x", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldInside,
                    null);
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E2", 12));
                env.SendEventBean(new SupportBean_ST0("ID2", "E2", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldInside,
                    new[] {new object[] {"E2"}});
                env.Listener("s0").Reset();

                env.SendEventBean(new SupportBean("E3", 13));
                env.SendEventBean(new SupportBean_ST0("E3", "E3", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    ToArrayMap(env.Listener("s0").AssertOneGetNew().Get("c0").Unwrap<object>()),
                    fieldInside,
                    new[] {new object[] {"E3"}});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprDefineAggregationNoAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"val1", "val2", "val3", "val4"};
                var epl = "@Name('s0') " +
                          "expression sumA {x => " +
                          "   sum(x.IntPrimitive) " +
                          "} " +
                          "expression sumB {x => " +
                          "   sum(x.IntBoxed) " +
                          "} " +
                          "expression countC {" +
                          "   count(*) " +
                          "} " +
                          "select sumA(t) as val1, sumB(t) as val2, sumA(t)/sumB(t) as val3, countC() as val4 from SupportBean as t";

                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?), typeof(double?), typeof(long?)});

                env.SendEventBean(GetSupportBean(5, 6));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, 6, 5 / 6d, 1L});

                env.SendEventBean(GetSupportBean(8, 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5 + 8, 6 + 10, (5 + 8) / (6d + 10d), 2L});

                env.UndeployAll();
            }
        }

        internal class ExprDefineSplitStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var epl = "@Name('split') expression myLittleExpression { event => false }" +
                          "on SupportBean as myEvent " +
                          " insert into ABC select * where myLittleExpression(myEvent)" +
                          " insert into DEF select * where not myLittleExpression(myEvent)";
                env.CompileDeploy(epl, path);

                env.CompileDeploy("@Name('s0') select * from DEF", path).AddListener("s0");
                env.SendEventBean(new SupportBean());
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ExprDefineAggregationAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "@Name('s0') expression wb {s => window(*).where(y => y.IntPrimitive > 2) }" +
                                 "select wb(t) as val1 from SupportBean#keepall as t";
                TryAssertionAggregationAccess(env, eplDeclare);

                var eplAlias = "@Name('s0') expression wb alias for {window(*).where(y => y.IntPrimitive > 2)}" +
                               "select wb as val1 from SupportBean#keepall as t";
                TryAssertionAggregationAccess(env, eplAlias);
            }
        }

        internal class ExprDefineAggregatedResult : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1"};
                var epl = "@Name('s0') expression lambda1 { o => 1 * o.IntPrimitive }\n" +
                          "expression lambda2 { o => 3 * o.IntPrimitive }\n" +
                          "select sum(lambda1(e)) as c0, sum(lambda2(e)) as c1 from SupportBean as e";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 30});

                env.SendEventBean(new SupportBean("E2", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {15, 45});

                env.UndeployAll();
            }
        }

        internal class ExprDefineScalarReturn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplScalarDeclare = "@Name('s0') expression scalarfilter {s => Strvals.where(y => y != 'E1') } " +
                                       "select scalarfilter(t).where(x => x != 'E2') as val1 from SupportCollection as t";
                TryAssertionScalarReturn(env, eplScalarDeclare);

                var eplScalarAlias = "@Name('s0') expression scalarfilter alias for {Strvals.where(y => y != 'E1')}" +
                                     "select scalarfilter.where(x => x != 'E2') as val1 from SupportCollection";
                TryAssertionScalarReturn(env, eplScalarAlias);

                // test with cast and with on-select and where-clause use
                var inner = "case when myEvent.One = 'X' then 0 else cast(myEvent.One, long) end ";
                var eplCaseDeclare = "@Name('s0') expression theExpression { myEvent => " +
                                     inner +
                                     "} " +
                                     "on SupportBeanObject as myEvent select mw.* from MyWindowFirst as mw where mw.myObject = theExpression(myEvent)";
                TryAssertionNamedWindowCast(env, eplCaseDeclare, "First");

                var eplCaseAlias = "@Name('s0') expression theExpression alias for {" +
                                   inner +
                                   "}" +
                                   "on SupportBeanObject as myEvent select mw.* from MyWindowSecond as mw where mw.myObject = theExpression";
                TryAssertionNamedWindowCast(env, eplCaseAlias, "Second");
            }

            private void TryAssertionNamedWindowCast(
                RegressionEnvironment env,
                string epl,
                string windowPostfix)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindow" + windowPostfix + "#keepall as (myObject long)", path);
                env.CompileDeploy(
                    "insert into MyWindow" +
                    windowPostfix +
                    "(myObject) select cast(IntPrimitive, long) from SupportBean",
                    path);
                env.CompileDeploy(epl, path).AddListener("s0");

                var props = new[] {"myObject"};

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 1));

                env.SendEventBean(new SupportBeanObject(2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBeanObject("X"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    props,
                    new object[] {0L});

                env.SendEventBean(new SupportBeanObject(1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    props,
                    new object[] {1L});

                env.UndeployAll();
            }

            private void TryAssertionScalarReturn(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new[] {"val1"},
                    new[] {
                        typeof(ICollection<string>)
                    });

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E3", "E4");
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprDefineEventTypeAndSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"fZero()", "fOne(t)", "fTwo(t,t)", "fThree(t,t)"};
                var eplDeclared = "@Name('s0') " +
                                  "expression fZero {10} " +
                                  "expression fOne {x => x.IntPrimitive} " +
                                  "expression fTwo {(x,y) => x.IntPrimitive+y.IntPrimitive} " +
                                  "expression fThree {(x,y) => x.IntPrimitive+100} " +
                                  "select fZero(), fOne(t), fTwo(t,t), fThree(t,t) from SupportBean as t";
                var eplFormatted = "@Name('s0')" +
                                   NEWLINE +
                                   "expression fZero {10}" +
                                   NEWLINE +
                                   "expression fOne {x => x.IntPrimitive}" +
                                   NEWLINE +
                                   "expression fTwo {(x,y) => x.IntPrimitive+y.IntPrimitive}" +
                                   NEWLINE +
                                   "expression fThree {(x,y) => x.IntPrimitive+100}" +
                                   NEWLINE +
                                   "select fZero(), fOne(t), fTwo(t,t), fThree(t,t)" +
                                   NEWLINE +
                                   "from SupportBean as t";
                env.CompileDeploy(eplDeclared).AddListener("s0");
                TryAssertionTwoParameterArithmetic(env, fields);
                env.UndeployAll();

                var model = env.EplToModel(eplDeclared);
                Assert.AreEqual(eplDeclared, model.ToEPL());
                Assert.AreEqual(eplFormatted, model.ToEPL(new EPStatementFormatter(true)));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertionTwoParameterArithmetic(env, fields);
                env.UndeployAll();

                var eplAlias = "@Name('s0') " +
                               "expression fZero alias for {10} " +
                               "expression fOne alias for {IntPrimitive} " +
                               "expression fTwo alias for {IntPrimitive+IntPrimitive} " +
                               "expression fThree alias for {IntPrimitive+100} " +
                               "select fZero, fOne, fTwo, fThree from SupportBean";
                env.CompileDeploy(eplAlias).AddListener("s0");
                TryAssertionTwoParameterArithmetic(env, new[] {"fZero", "fOne", "fTwo", "fThree"});
                env.UndeployAll();
            }

            private void TryAssertionTwoParameterArithmetic(
                RegressionEnvironment env,
                string[] fields)
            {
                var props = env.Statement("s0").EventType.PropertyNames;
                EPAssertionUtil.AssertEqualsAnyOrder(props, fields);
                var eventType = env.Statement("s0").EventType;
                for (int i = 0; i < fields.Length; i++) {
                    Assert.AreEqual(typeof(int?), eventType.GetPropertyType(fields[i]));
                }
                var getter = env.Statement("s0").EventType.GetGetter(fields[3]);

                env.SendEventBean(new SupportBean("E1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNew(),
                    fields,
                    new object[] {10, 11, 22, 111});
                Assert.AreEqual(111, getter.Get(env.Listener("s0").AssertOneGetNewAndReset()));
            }
        }

        internal class ExprDefineOneParameterLambdaReturn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclare = "" +
                                 "@Name('s0') expression one {x1 => x1.Contained.where(y => y.P00 < 10) } " +
                                 "expression two {x2 => one(x2).where(y => y.P00 > 1)  } " +
                                 "select one(s0c) as val1, two(s0c) as val2 from SupportBean_ST0_Container as s0c";
                TryAssertionOneParameterLambdaReturn(env, eplDeclare);

                var eplAliasWParen = "" +
                                     "@Name('s0') expression one alias for {Contained.where(y => y.P00 < 10)}" +
                                     "expression two alias for {one().where(y => y.P00 > 1)}" +
                                     "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
                TryAssertionOneParameterLambdaReturn(env, eplAliasWParen);

                var eplAliasNoParen = "" +
                                      "@Name('s0') expression one alias for {Contained.where(y => y.P00 < 10)}" +
                                      "expression two alias for {one.where(y => y.P00 > 1)}" +
                                      "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
                TryAssertionOneParameterLambdaReturn(env, eplAliasNoParen);
            }

            private void TryAssertionOneParameterLambdaReturn(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    new[] {"val1", "val2"},
                    new[] {
                        typeof(ICollection<object>),
                        typeof(ICollection<object>)
                    });

                var theEvent = SupportBean_ST0_Container.Make3Value("E1,K1,1", "E2,K2,2", "E20,K20,20");
                env.SendEventBean(theEvent);
                var resultVal1 = env.Listener("s0")
                    .LastNewData[0]
                    .Get("val1")
                    .UnwrapIntoArray<object>();
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {
                        theEvent.Contained[0],
                        theEvent.Contained[1]
                    },
                    resultVal1);
                var resultVal2 = env.Listener("s0").LastNewData[0].Get("val2").Unwrap<object>().ToArray();
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {
                        theEvent.Contained[1]
                    },
                    resultVal2);

                env.UndeployAll();
            }
        }

        internal class ExprDefineNoParameterArithmetic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclared = "@Name('s0') expression getEnumerationSource {1} " +
                                  "select getEnumerationSource() as val1, getEnumerationSource()*5 as val2 from SupportBean";
                TryAssertionNoParameterArithmetic(env, eplDeclared);

                var eplDeclaredNoParen = "@Name('s0') expression getEnumerationSource {1} " +
                                         "select getEnumerationSource as val1, getEnumerationSource*5 as val2 from SupportBean";
                TryAssertionNoParameterArithmetic(env, eplDeclaredNoParen);

                var eplAlias = "@Name('s0') expression getEnumerationSource alias for {1} " +
                               "select getEnumerationSource as val1, getEnumerationSource*5 as val2 from SupportBean";
                TryAssertionNoParameterArithmetic(env, eplAlias);
            }

            private void TryAssertionNoParameterArithmetic(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val1", "val2"};
                env.CompileDeploy(epl).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 5});

                env.UndeployAll();
            }
        }

        internal class ExprDefineNoParameterVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplDeclared = "@Name('s0') expression one {myvar} " +
                                  "expression two {myvar * 10} " +
                                  "select one() as val1, two() as val2, one() * two() as val3 from SupportBean";
                TryAssertionNoParameterVariable(env, eplDeclared);

                var eplAlias = "@Name('s0') expression one alias for {myvar} " +
                               "expression two alias for {myvar * 10} " +
                               "select one() as val1, two() as val2, one * two as val3 from SupportBean";
                TryAssertionNoParameterVariable(env, eplAlias);
            }

            private void TryAssertionNoParameterVariable(
                RegressionEnvironment env,
                string epl)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@Name('var') create variable int myvar = 2", path);

                var fields = new[] {"val1", "val2", "val3"};
                env.CompileDeploy(epl, path).AddListener("s0");

                SupportEventPropUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?), typeof(int?)});

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 20, 40});

                env.Runtime.VariableService.SetVariableValue(env.DeploymentId("var"), "myvar", 3);
                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, 30, 90});

                env.UndeployAll();
            }
        }

        internal class ExprDefineWhereClauseExpression : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplNoAlias =
                    "@Name('s0') expression one {x=>x.BoolPrimitive} select * from SupportBean as sb where one(sb)";
                TryAssertionWhereClauseExpression(env, eplNoAlias);

                var eplAlias =
                    "@Name('s0') expression one alias for {BoolPrimitive} select * from SupportBean as sb where one";
                TryAssertionWhereClauseExpression(env, eplAlias);
            }

            private void TryAssertionWhereClauseExpression(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                var theEvent = new SupportBean();
                theEvent.BoolPrimitive = true;
                env.SendEventBean(theEvent);
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class ExprDefineInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "expression abc {(select * from SupportBean_ST0#lastevent as st0 where P00=IntPrimitive)} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'P00=IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [expression abc {(select * from SupportBean_ST0#lastevent as st0 where P00=IntPrimitive)} select abc() from SupportBean]");

                epl = "expression abc {x=>Strvals.where(x=> x != 'E1')} select abc(str) from SupportCollection str";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc(str)': Failed to validate expression declaration 'abc': Failed to validate declared expression body expression 'Strvals.where()': Failed to validate enumeration method 'where', the lambda-parameter name 'x' has already been declared in this context [expression abc {x=>Strvals.where(x=> x != 'E1')} select abc(str) from SupportCollection str]");

                epl = "expression abc {avg(IntPrimitive)} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Failed to validate expression declaration 'abc': Failed to validate declared expression body expression 'avg(IntPrimitive)': Property named 'IntPrimitive' is not valid in any stream [expression abc {avg(IntPrimitive)} select abc() from SupportBean]");

                epl =
                    "expression abc {(select * from SupportBean_ST0#lastevent as st0 where P00=sb.IntPrimitive)} select abc() from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'P00=sb.IntPrimitive': Failed to find a stream named 'sb' (did you mean 'st0'?) [expression abc {(select * from SupportBean_ST0#lastevent as st0 where P00=sb.IntPrimitive)} select abc() from SupportBean sb]");

                epl = "expression abc {window(*)} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Failed to validate expression declaration 'abc': Failed to validate declared expression body expression 'window(*)': The 'window' aggregation function requires that at least one stream is provided [expression abc {window(*)} select abc() from SupportBean]");

                epl = "expression abc {x => IntPrimitive} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Parameter count mismatches for declared expression 'abc', expected 1 parameters but received 0 parameters [expression abc {x => IntPrimitive} select abc() from SupportBean]");

                epl = "expression abc {IntPrimitive} select abc(sb) from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc(sb)': Parameter count mismatches for declared expression 'abc', expected 0 parameters but received 1 parameters [expression abc {IntPrimitive} select abc(sb) from SupportBean sb]");

                epl = "expression abc {x=>} select abc(sb) from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Incorrect syntax near '}' at line 1 column 19 near reserved keyword 'select' [expression abc {x=>} select abc(sb) from SupportBean sb]");

                epl = "expression abc {IntPrimitive} select abc() from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Failed to validate expression declaration 'abc': Failed to validate declared expression body expression 'IntPrimitive': Property named 'IntPrimitive' is not valid in any stream [expression abc {IntPrimitive} select abc() from SupportBean sb]");

                epl = "expression abc {x=>IntPrimitive} select * from SupportBean sb where abc(sb)";
                TryInvalidCompile(
                    env,
                    epl,
                    "Filter expression not returning a boolean value: 'abc(sb)' [expression abc {x=>IntPrimitive} select * from SupportBean sb where abc(sb)]");

                epl =
                    "expression abc {x=>x.IntPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where abc(*)";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate expression: Failed to validate filter expression 'abc(*)': Expression 'abc' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead [expression abc {x=>x.IntPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where abc(*)]");

                epl = "expression ABC alias for {1} select ABC(t) from SupportBean as t";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'ABC': Expression 'ABC is an expression-alias and does not allow parameters [expression ABC alias for {1} select ABC(t) from SupportBean as t]");
            }
        }
    }
} // end of namespace