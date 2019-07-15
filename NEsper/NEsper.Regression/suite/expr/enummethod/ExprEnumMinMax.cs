///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMinMax
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumMinMaxScalarWithLambda());
            execs.Add(new ExprEnumMinMaxEvents());
            execs.Add(new ExprEnumMinMaxScalar());
            execs.Add(new ExprEnumMinMaxScalarChain());
            execs.Add(new ExprEnumInvalid());
            return execs;
        }

        internal class ExprEnumMinMaxScalarChain : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "@Name('s0') select coll.max().minus(1 minute) >= coll.min() as c0 from SupportEventWithLongArray");
                env.AddListener("s0");
                var fields = "c0".SplitCsv();

                env.SendEventBean(new SupportEventWithLongArray(new long[] {150000, 140000, 200000, 190000}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(new SupportEventWithLongArray(new long[] {150000, 139999, 200000, 190000}));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.UndeployAll();
            }
        }

        internal class ExprEnumMinMaxScalarWithLambda : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1,val2,val3".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "strvals.min(v => extractNum(v)) as val0, " +
                                  "strvals.max(v => extractNum(v)) as val1, " +
                                  "strvals.min(v => v) as val2, " +
                                  "strvals.max(v => v) as val3 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?), typeof(string), typeof(string)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 5, "E1", "E5"});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 1, "E1", "E1"});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumMinMaxEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "contained.min(x => p00) as val0, " +
                                  "contained.max(x => p00) as val1 " +
                                  "from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(int?)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, 12});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,12", "E2,0", "E2,2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, 12});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumMinMaxScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0,val1".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "strvals.min() as val0, " +
                                  "strvals.max() as val1 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(string), typeof(string)});

                env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E5"});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1"});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select contained.min() from SupportBean_ST0_Container";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to valIdate select-clause expression 'contained.min()': InvalId input for built-in enumeration method 'min' and 0-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type '" +
                    typeof(SupportBean_ST0).Name +
                    "'");
            }
        }

        public class MyService
        {
            public static int ExtractNum(string arg)
            {
                return int.Parse(arg.Substring(1));
            }

            public static decimal ExtractDecimal(string arg)
            {
                return decimal.Parse(arg.Substring(1));
            }
        }
    }
} // end of namespace