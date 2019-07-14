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
using com.espertech.esper.compat;
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
            execs.Add(new ExprDefineExpressionSimpleSameStmt());
            execs.Add(new ExprDefineExpressionSimpleSameModule());
            execs.Add(new ExprDefineExpressionSimpleTwoModule());
            execs.Add(new ExprDefineAggregationNoAccess());
            execs.Add(new ExprDefineAggregatedResult());
            execs.Add(new ExprDefineAggregationAccess());
            execs.Add(new ExprDefineWildcardAndPattern());
            execs.Add(new ExprDefineScalarReturn());
            execs.Add(new ExprDefineNoParameterArithmetic());
            execs.Add(new ExprDefineOneParameterLambdaReturn());
            execs.Add(new ExprDefineNoParameterVariable());
            execs.Add(new ExprDefineAnnotationOrder());
            execs.Add(new ExprDefineWhereClauseExpression());
            execs.Add(new ExprDefineSequenceAndNested());
            execs.Add(new ExprDefineCaseNewMultiReturnNoElse());
            execs.Add(new ExprDefineSubqueryMultiresult());
            execs.Add(new ExprDefineSubqueryCross());
            execs.Add(new ExprDefineSubqueryJoinSameField());
            execs.Add(new ExprDefineSubqueryCorrelated());
            execs.Add(new ExprDefineSubqueryUncorrelated());
            execs.Add(new ExprDefineSubqueryNamedWindowUncorrelated());
            execs.Add(new ExprDefineSubqueryNamedWindowCorrelated());
            execs.Add(new ExprDefineNestedExpressionMultiSubquery());
            execs.Add(new ExprDefineEventTypeAndSODA());
            execs.Add(new ExprDefineInvalid());
            execs.Add(new ExprDefineSplitStream());
            return execs;
        }

        private static void TryAssertionAggregationAccess(
            RegressionEnvironment env,
            string epl)
        {
            env.CompileDeploy(epl).AddListener("s0");
            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                "val1".SplitCsv(),
                new[] {
                    typeof(ICollection<object>), typeof(ICollection<object>)
                });

            env.SendEventBean(new SupportBean("E1", 2));
            var outArray = ToArray(
                env.Listener("s0")
                    .AssertOneGetNewAndReset()
                    .Get("val1")
                    .Unwrap<object>());
            Assert.AreEqual(0, outArray.Length);

            env.SendEventBean(new SupportBean("E2", 3));
            outArray = ToArray(
                env.Listener("s0")
                    .AssertOneGetNewAndReset()
                    .Get("val1")
                    .Unwrap<object>());

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

        private static SupportBean[] ToArray(ICollection<object> items)
        {
            IList<SupportBean> result = new List<SupportBean>();
            foreach (var item in items) {
                result.Add((SupportBean) item);
            }

            return result.ToArray();
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
                var fields = "c0".SplitCsv();

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
                    "@Name('s0') expression abc { x => intPrimitive } " +
                    "expression def { (x, y) => x.IntPrimitive * y.IntPrimitive }" +
                    "select abc(*) as c0, def(*, *) as c1 from SupportBean";
                env.CompileDeploy(eplNonJoin).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0, c1".SplitCsv(),
                    new object[] {2, 4});
                env.UndeployAll();

                var eplPattern = "@Name('s0') expression abc { x => intPrimitive * 2} " +
                                 "select * from pattern [a=SupportBean => b=SupportBean(IntPrimitive = abc(a))]";
                env.CompileDeploy(eplPattern).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 2));
                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "a.TheString, b.TheString".SplitCsv(),
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
                env.CompileDeploy("insert into WindowOne select p00 as col1, p01 as col2 from SupportBean_S0", path);

                env.CompileDeploy("create window WindowTwo#keepall as (col1 string, col2 string)", path);
                env.CompileDeploy("insert into WindowTwo select p10 as col1, p11 as col2 from SupportBean_S1", path);

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
                var fieldsInner = "col1,col2".SplitCsv();
                var epl = "@Name('s0') expression gettotal {" +
                          " x => case " +
                          "  when theString = 'A' then new { col1 = 'X', col2 = 10 } " +
                          "  when theString = 'B' then new { col1 = 'Y', col2 = 20 } " +
                          "end" +
                          "} " +
                          "insert into OtherStream select gettotal(sb) as val0 from SupportBean sb";
                env.CompileDeploy(epl, path).AddListener("s0");

                Assert.AreEqual(
                    typeof(IDictionary<string, object>),
                    env.Statement("s0").EventType.GetPropertyType("val0"));

                env.CompileDeploy("@Name('s1') select val0.col1 as c1, val0.col2 as c2 from OtherStream", path)
                    .AddListener("s1");
                var fieldsConsume = "c1,c2".SplitCsv();

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
                    "scalar()".SplitCsv(),
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
                             "select p00/maxi() as val0, p00/mini() as val1 " +
                             "from SupportBean_ST0#lastevent";
                TryAssertionMultiResult(env, eplOne);

                var eplTwo = "@Name('s0') " +
                             "expression subq {" +
                             " (select max(IntPrimitive) as maxi, min(IntPrimitive) as mini from SupportBean#keepall)" +
                             "} " +
                             "select p00/subq().maxi as val0, p00/subq().mini as val1 " +
                             "from SupportBean_ST0#lastevent";
                TryAssertionMultiResult(env, eplTwo);

                var eplTwoAlias = "@Name('s0') " +
                                  "expression subq alias for " +
                                  " { (select max(IntPrimitive) as maxi, min(IntPrimitive) as mini from SupportBean#keepall) }" +
                                  " " +
                                  "select p00/subq().maxi as val0, p00/subq().mini as val1 " +
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
                                 " (x, y) => (select TheString from SupportBean#keepall where theString = x.id and intPrimitive = y.p10)" +
                                 "} " +
                                 "select subq(one, two) as val1 " +
                                 "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryAssertionSubqueryCross(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subq alias for { (select TheString from SupportBean#keepall where theString = one.id and intPrimitive = two.p10) }" +
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

                LambdaAssertionUtil.AssertTypes(env.Statement("s0").EventType, fields, new[] {typeof(string)});

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
                                 " x => (select IntPrimitive from SupportBean#keepall where theString = x.pcommon)" + // a common field
                                 "} " +
                                 "select subq(one) as val1, subq(two) as val2 " +
                                 "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryAssertionSubqueryJoinSameField(env, eplDeclare);

                var eplAlias = "@Name('s0') " +
                               "expression subq alias for {(select IntPrimitive from SupportBean#keepall where theString = pcommon) }" +
                               "select subq as val1, subq as val2 " +
                               "from SupportBean_ST0#lastevent as one, SupportBean_ST1#lastevent as two";
                TryInvalidCompile(
                    env,
                    eplAlias,
                    "Failed to plan subquery number 1 querying SupportBean: Failed to validate filter expression 'theString=pcommon': Property named 'pcommon' is ambiguous as is valid for more then one stream");
            }

            private void TryAssertionSubqueryJoinSameField(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val1", "val2"};
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
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
                                 " x => (select id from SupportBean_ST0#keepall where p00 = x.IntPrimitive)" +
                                 "} " +
                                 "select TheString as val0, subqOne(t) as val1 from SupportBean as t";
                TryAssertionSubqueryCorrelated(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subqOne alias for {(select id from SupportBean_ST0#keepall where p00 = t.IntPrimitive)} " +
                    "select TheString as val0, subqOne(t) as val1 from SupportBean as t";
                TryAssertionSubqueryCorrelated(env, eplAlias);
            }

            private void TryAssertionSubqueryCorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val0", "val1"};
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
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
                var eplDeclare = "@Name('s0') expression subqOne {(select id from SupportBean_ST0#lastevent)} " +
                                 "select TheString as val0, subqOne() as val1 from SupportBean as t";
                TryAssertionSubqueryUncorrelated(env, eplDeclare);

                var eplAlias =
                    "@Name('s0') expression subqOne alias for {(select id from SupportBean_ST0#lastevent)} " +
                    "select TheString as val0, subqOne as val1 from SupportBean as t";
                TryAssertionSubqueryUncorrelated(env, eplAlias);
            }

            private void TryAssertionSubqueryUncorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fields = new[] {"val0", "val1"};
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
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
                var fieldsSelected = "c0,c1".SplitCsv();
                var fieldsInside = "val0".SplitCsv();

                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.MAP.GetAnnotationText() +
                    " create window MyWindow#keepall as (val0 string, val1 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyWindow (val0, val1) select TheString, intPrimitive from SupportBean",
                    path);
                env.CompileDeploy(epl, path).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fieldsSelected,
                    new[] {typeof(ICollection<object>), typeof(ICollection<object>)});

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
                          "  x => MyWindow(val0 = x.key0).where(y => val1 > 10)" +
                          "} " +
                          "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // more or less prefixes
                epl = "@Name('s0') expression subqnamedwin {" +
                      "  x => MyWindow(val0 = x.key0).where(y => y.val1 > 10)" +
                      "} " +
                      "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // with property-explicit stream name
                epl = "@Name('s0') expression subqnamedwin {" +
                      "  x => MyWindow(MyWindow.val0 = x.key0).where(y => y.val1 > 10)" +
                      "} " +
                      "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // with alias
                epl =
                    "@Name('s0') expression subqnamedwin alias for {MyWindow(MyWindow.val0 = t.key0).where(y => y.val1 > 10)}" +
                    "select subqnamedwin as c0 from SupportBean_ST0 as t";
                TryAssertionSubqNWCorrelated(env, epl);

                // test ambiguous property names
                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.MAP.GetAnnotationText() +
                    " create window MyWindowTwo#keepall as (id string, p00 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyWindowTwo (id, p00) select TheString, intPrimitive from SupportBean",
                    path);
                epl = "expression subqnamedwin {" +
                      "  x => MyWindowTwo(MyWindowTwo.id = x.id).where(y => y.p00 > 10)" +
                      "} " +
                      "select subqnamedwin(t) as c0 from SupportBean_ST0 as t";
                env.CompileDeploy(epl, path);
                env.UndeployAll();
            }

            private void TryAssertionSubqNWCorrelated(
                RegressionEnvironment env,
                string epl)
            {
                var fieldSelected = "c0".SplitCsv();
                var fieldInside = "val0".SplitCsv();

                var path = new RegressionPath();
                env.CompileDeploy(
                    EventRepresentationChoice.MAP.GetAnnotationText() +
                    " create window MyWindow#keepall as (val0 string, val1 int)",
                    path);
                env.CompileDeploy(
                    "insert into MyWindow (val0, val1) select TheString, intPrimitive from SupportBean",
                    path);
                env.CompileDeploy(epl, path).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fieldSelected,
                    new[] {typeof(ICollection<object>)});

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
                          "   sum(x.intBoxed) " +
                          "} " +
                          "expression countC {" +
                          "   count(*) " +
                          "} " +
                          "select sumA(t) as val1, sumB(t) as val2, sumA(t)/sumB(t) as val3, countC() as val4 from SupportBean as t";

                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
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
                var fields = "c0,c1".SplitCsv();
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
                var eplScalarDeclare = "@Name('s0') expression scalarfilter {s => strvals.where(y => y != 'E1') } " +
                                       "select scalarfilter(t).where(x => x != 'E2') as val1 from SupportCollection as t";
                TryAssertionScalarReturn(env, eplScalarDeclare);

                var eplScalarAlias = "@Name('s0') expression scalarfilter alias for {strvals.where(y => y != 'E1')}" +
                                     "select scalarfilter.where(x => x != 'E2') as val1 from SupportCollection";
                TryAssertionScalarReturn(env, eplScalarAlias);

                // test with cast and with on-select and where-clause use
                var inner = "case when myEvent.one = 'X' then 0 else cast(myEvent.one, long) end ";
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
                    "(myObject) select cast(intPrimitive, long) from SupportBean",
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

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    "val1".SplitCsv(),
                    new[] {
                        typeof(ICollection<object>)
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
                               "expression fOne alias for {intPrimitive} " +
                               "expression fTwo alias for {intPrimitive+intPrimitive} " +
                               "expression fThree alias for {intPrimitive+100} " +
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
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(fields[0]));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(fields[1]));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(fields[2]));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType(fields[3]));
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
                                 "@Name('s0') expression one {x1 => x1.contained.where(y => y.p00 < 10) } " +
                                 "expression two {x2 => one(x2).where(y => y.p00 > 1)  } " +
                                 "select one(s0c) as val1, two(s0c) as val2 from SupportBean_ST0_Container as s0c";
                TryAssertionOneParameterLambdaReturn(env, eplDeclare);

                var eplAliasWParen = "" +
                                     "@Name('s0') expression one alias for {contained.where(y => y.p00 < 10)}" +
                                     "expression two alias for {one().where(y => y.p00 > 1)}" +
                                     "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
                TryAssertionOneParameterLambdaReturn(env, eplAliasWParen);

                var eplAliasNoParen = "" +
                                      "@Name('s0') expression one alias for {contained.where(y => y.p00 < 10)}" +
                                      "expression two alias for {one.where(y => y.p00 > 1)}" +
                                      "select one as val1, two as val2 from SupportBean_ST0_Container as s0c";
                TryAssertionOneParameterLambdaReturn(env, eplAliasNoParen);
            }

            private void TryAssertionOneParameterLambdaReturn(
                RegressionEnvironment env,
                string epl)
            {
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    "val1,val2".SplitCsv(),
                    new[] {
                        typeof(ICollection<object>),
                        typeof(ICollection<object>)
                    });

                var theEvent = SupportBean_ST0_Container.Make3Value("E1,K1,1", "E2,K2,2", "E20,K20,20");
                env.SendEventBean(theEvent);
                var resultVal1 = env.Listener("s0").LastNewData[0].Get("val1").Unwrap<object>().ToArray();
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
                var fields = "val1,val2".SplitCsv();
                env.CompileDeploy(epl).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
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

                var fields = "val1,val2,val3".SplitCsv();
                env.CompileDeploy(epl, path).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
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
                    "@Name('s0') expression one {x=>x.boolPrimitive} select * from SupportBean as sb where one(sb)";
                TryAssertionWhereClauseExpression(env, eplNoAlias);

                var eplAlias =
                    "@Name('s0') expression one alias for {boolPrimitive} select * from SupportBean as sb where one";
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
                    "expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=intPrimitive)} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'p00=intPrimitive': Property named 'intPrimitive' is not valid in any stream [expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=intPrimitive)} select abc() from SupportBean]");

                epl = "expression abc {x=>strvals.where(x=> x != 'E1')} select abc(str) from SupportCollection str";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc(str)': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'strvals.where()': Error validating enumeration method 'where', the lambda-parameter name 'x' has already been declared in this context [expression abc {x=>strvals.where(x=> x != 'E1')} select abc(str) from SupportCollection str]");

                epl = "expression abc {avg(IntPrimitive)} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'avg(IntPrimitive)': Property named 'intPrimitive' is not valid in any stream [expression abc {avg(IntPrimitive)} select abc() from SupportBean]");

                epl =
                    "expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=sb.IntPrimitive)} select abc() from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to plan subquery number 1 querying SupportBean_ST0: Failed to validate filter expression 'p00=sb.IntPrimitive': Failed to find a stream named 'sb' (did you mean 'st0'?) [expression abc {(select * from SupportBean_ST0#lastevent as st0 where p00=sb.IntPrimitive)} select abc() from SupportBean sb]");

                epl = "expression abc {window(*)} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'window(*)': The 'window' aggregation function requires that at least one stream is provided [expression abc {window(*)} select abc() from SupportBean]");

                epl = "expression abc {x => intPrimitive} select abc() from SupportBean";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Parameter count mismatches for declared expression 'abc', expected 1 parameters but received 0 parameters [expression abc {x => intPrimitive} select abc() from SupportBean]");

                epl = "expression abc {intPrimitive} select abc(sb) from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc(sb)': Parameter count mismatches for declared expression 'abc', expected 0 parameters but received 1 parameters [expression abc {intPrimitive} select abc(sb) from SupportBean sb]");

                epl = "expression abc {x=>} select abc(sb) from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Incorrect syntax near '}' at line 1 column 19 near reserved keyword 'select' [expression abc {x=>} select abc(sb) from SupportBean sb]");

                epl = "expression abc {intPrimitive} select abc() from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc()': Error validating expression declaration 'abc': Failed to validate declared expression body expression 'intPrimitive': Property named 'intPrimitive' is not valid in any stream [expression abc {intPrimitive} select abc() from SupportBean sb]");

                epl = "expression abc {x=>x} select abc(1) from SupportBean sb";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'abc(1)': Expression 'abc' requires a stream name as a parameter [expression abc {x=>x} select abc(1) from SupportBean sb]");

                epl = "expression abc {x=>intPrimitive} select * from SupportBean sb where abc(sb)";
                TryInvalidCompile(
                    env,
                    epl,
                    "Filter expression not returning a boolean value: 'abc(sb)' [expression abc {x=>intPrimitive} select * from SupportBean sb where abc(sb)]");

                epl =
                    "expression abc {x=>x.IntPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where abc(*)";
                TryInvalidCompile(
                    env,
                    epl,
                    "Error validating expression: Failed to validate filter expression 'abc(*)': Expression 'abc' only allows a wildcard parameter if there is a single stream available, please use a stream or tag name instead [expression abc {x=>x.IntPrimitive = 0} select * from SupportBean#lastevent sb1, SupportBean#lastevent sb2 where abc(*)]");
            }
        }
    }
} // end of namespace