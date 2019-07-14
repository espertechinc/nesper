///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Numerics;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumSumOf
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumSumEvents());
            execs.Add(new ExprEnumSumOfScalar());
            execs.Add(new ExprEnumInvalid());
            execs.Add(new ExprEnumSumOfArray());
            return execs;
        }

        private static SupportBean Make(
            int? intBoxed,
            double? doubleBoxed,
            long? longBoxed,
            int decimalBoxed,
            int bigInteger)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            bean.BigInteger = new BigInteger(bigInteger);
            return bean;
        }

        internal class ExprEnumSumOfArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "{1d, 2d}.sumOf() as c0," +
                          "{BigInteger.valueOf(1), BigInteger.valueOf(2)}.sumOf() as c1, " +
                          "{1L, 2L}.sumOf() as c2, " +
                          "{1L, 2L, null}.sumOf() as c3 " +
                          " from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,c1,c2,c3".SplitCsv(),
                    new object[] {3d, new BigInteger(3), 3L, 3L});

                env.UndeployAll();
            }
        }

        internal class ExprEnumSumEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "beans.sumOf(x => intBoxed) as val0," +
                                  "beans.sumOf(x => doubleBoxed) as val1," +
                                  "beans.sumOf(x => longBoxed) as val2," +
                                  "beans.sumOf(x => bigDecimal) as val3, " +
                                  "beans.sumOf(x => bigInteger) as val4 " +
                                  "from SupportBean_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(double?), typeof(long?), typeof(decimal), typeof(BigInteger)});

                env.SendEventBean(new SupportBean_Container(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                env.SendEventBean(new SupportBean_Container(Collections.GetEmptyList<SupportBean>()));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null, null});

                IList<SupportBean> list = new List<SupportBean>();
                list.Add(Make(2, 3d, 4L, 5, 6));
                env.SendEventBean(new SupportBean_Container(list));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 3d, 4L, (decimal) 5, new BigInteger(6)});

                list.Add(Make(4, 6d, 8L, 10, 12));
                env.SendEventBean(new SupportBean_Container(list));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2 + 4, 3d + 6d, 4L + 8L, (decimal) (5 + 10), new BigInteger(18)});

                env.UndeployAll();
            }
        }

        internal class ExprEnumSumOfScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "intvals.sumOf() as val0, " +
                                  "bdvals.sumOf() as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(decimal)});

                env.SendEventBean(SupportCollection.MakeNumeric("1,4,5"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1 + 4 + 5, (decimal) (1 + 4 + 5)});

                env.SendEventBean(SupportCollection.MakeNumeric("3,4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3 + 4, (decimal) (3 + 4)});

                env.SendEventBean(SupportCollection.MakeNumeric("3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, (decimal) 3});

                env.SendEventBean(SupportCollection.MakeNumeric(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportCollection.MakeNumeric(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();

                // test average with lambda
                // lambda with string-array input
                var fieldsLambda = "val0,val1".SplitCsv();
                var eplLambda = "@Name('s0') select " +
                                "strvals.sumOf(v => extractNum(v)) as val0, " +
                                "strvals.sumOf(v => extractBigDecimal(v)) as val1 " +
                                "from SupportCollection";
                env.CompileDeploy(eplLambda).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fieldsLambda,
                    new[] {typeof(int?), typeof(decimal)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsLambda,
                    new object[] {2 + 1 + 5 + 4, (decimal) (2 + 1 + 5 + 4)});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsLambda,
                    new object[] {1, (decimal) 1});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsLambda,
                    new object[] {null, null});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsLambda,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select beans.sumof() from SupportBean_Container";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'beans.sumof()': Invalid input for built-in enumeration method 'sumof' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '");
            }
        }
    }
} // end of namespace