///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumAverage
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumAverageEvents());
            execs.Add(new ExprEnumAverageScalar());
            execs.Add(new ExprEnumInvalid());
            return execs;
        }

        private static SupportBean Make(
            int? intBoxed,
            double? doubleBoxed,
            long? longBoxed,
            int decimalBoxed)
        {
            var bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            bean.DecimalBoxed = decimalBoxed;
            return bean;
        }

        internal class ExprEnumAverageEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Beans.average(x -> IntBoxed) as val0," +
                                  "Beans.average(x -> DoubleBoxed) as val1," +
                                  "Beans.average(x -> LongBoxed) as val2," +
                                  "Beans.average(x -> DecimalBoxed) as val3 " +
                                  "from SupportBean_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(double?), typeof(double?), typeof(double?), typeof(decimal?)});

                env.SendEventBean(new SupportBean_Container(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                env.SendEventBean(new SupportBean_Container(new EmptyList<SupportBean>()));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                IList<SupportBean> list = new List<SupportBean>();
                list.Add(Make(2, 3d, 4L, 5));
                env.SendEventBean(new SupportBean_Container(list));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2d, 3d, 4d, 5.0m});

                list.Add(Make(4, 6d, 8L, 10));
                env.SendEventBean(new SupportBean_Container(list));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {(2 + 4) / 2d, (3d + 6d) / 2d, (4L + 8L) / 2d, (5m + 10m) / 2m});

                env.UndeployAll();
            }
        }

        internal class ExprEnumAverageScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "intvals.average() as val0," +
                                  "bdvals.average() as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(double?), typeof(decimal?)});

                env.SendEventBean(SupportCollection.MakeNumeric("1,2,3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2d, 2m});

                env.SendEventBean(SupportCollection.MakeNumeric("1,null,3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2d, 2m});

                env.SendEventBean(SupportCollection.MakeNumeric("4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4d, 4m});
                env.UndeployAll();

                // test average with lambda
                var fieldsLambda = "val0,val1".SplitCsv();
                var eplLambda = "@Name('s0') select " +
                                "Strvals.average(v -> extractNum(v)) as val0, " +
                                "Strvals.average(v -> extractDecimal(v)) as val1 " +
                                "from SupportCollection";
                env.CompileDeploy(eplLambda).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fieldsLambda,
                    new[] {typeof(double?), typeof(decimal?)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsLambda,
                    new object[] {(2 + 1 + 5 + 4) / 4d, (2m + 1m + 5m + 4m) / 4m});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsLambda,
                    new object[] {1d, 1m});

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

                epl = "select Strvals.average() from SupportCollection";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'Strvals.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of numeric values as input, received collection of String [select Strvals.average() from SupportCollection]");

                epl = "select Beans.average() from SupportBean_Container";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'Beans.average()': Invalid input for built-in enumeration method 'average' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean).Name +
                    "'");
            }
        }
    }
} // end of namespace