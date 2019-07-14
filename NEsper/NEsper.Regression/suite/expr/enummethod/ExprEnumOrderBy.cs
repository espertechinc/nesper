///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumOrderBy
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumOrderByEvents());
            execs.Add(new ExprEnumOrderByScalar());
            execs.Add(new ExprEnumInvalid());
            return execs;
        }

        internal class ExprEnumOrderByEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3,val4,val5".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "contained.orderBy(x => p00) as val0," +
                                  "contained.orderBy(x => 10 - p00) as val1," +
                                  "contained.orderBy(x => 0) as val2," +
                                  "contained.orderByDesc(x => p00) as val3," +
                                  "contained.orderByDesc(x => 10 - p00) as val4," +
                                  "contained.orderByDesc(x => 0) as val5" +
                                  " from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>),
                        typeof(ICollection<object>), typeof(ICollection<object>), typeof(ICollection<object>)
                    });

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E2,2"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E2,E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "E2,E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val4", "E1,E2");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val5", "E1,E2");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E3,1", "E2,2", "E4,1", "E1,2"));
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val0", "E3,E4,E2,E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val1", "E2,E1,E3,E4");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val2", "E3,E2,E4,E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val3", "E2,E1,E3,E4");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val4", "E3,E4,E2,E1");
                LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), "val5", "E3,E2,E4,E1");
                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), field, null);
                }

                env.Listener("s0").Reset();

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                foreach (var field in fields) {
                    LambdaAssertionUtil.AssertST0Id(env.Listener("s0"), field, "");
                }

                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }

        internal class ExprEnumOrderByScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "strvals.orderBy() as val0, " +
                                  "strvals.orderByDesc() as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(ICollection<object>), typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E2", "E4", "E5");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E5", "E4", "E2", "E1");
                env.Listener("s0").Reset();

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(env, fields);
                env.UndeployAll();

                // test scalar-coll with lambda
                var eplLambda = "@Name('s0') select " +
                                "strvals.orderBy(v => extractNum(v)) as val0, " +
                                "strvals.orderByDesc(v => extractNum(v)) as val1 " +
                                "from SupportCollection";
                env.CompileDeploy(eplLambda).AddListener("s0");
                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(ICollection<object>), typeof(ICollection<object>)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val0", "E1", "E2", "E4", "E5");
                LambdaAssertionUtil.AssertValuesArrayScalar(env.Listener("s0"), "val1", "E5", "E4", "E2", "E1");
                env.Listener("s0").Reset();

                LambdaAssertionUtil.AssertSingleAndEmptySupportColl(env, fields);

                env.UndeployAll();
            }
        }

        internal class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select contained.orderBy() from SupportBean_ST0_Container";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'contained.orderBy()': Invalid input for built-in enumeration method 'orderBy' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean_ST0).Name +
                    "'");
            }
        }
    }
} // end of namespace