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
    public class ExprEnumSequenceEqual
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumSelectFrom());
            execs.Add(new ExprEnumTwoProperties());
            execs.Add(new ExprEnumInvalid());
            return execs;
        }

        internal class ExprEnumSelectFrom : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var eplFragment =
                    "@Name('s0') select Contained.selectFrom(x => Key0).sequenceEqual(Contained.selectFrom(y => Id)) as val0 " +
                    "from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    "val0".SplitCsv(),
                    new[] {typeof(bool?)});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("I1,E1,0", "I2,E2,0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("I3,I3,0", "X4,X4,0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("I3,I3,0", "X4,Y4,0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportBean_ST0_Container.Make3Value("I3,I3,0", "Y4,X4,0"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.UndeployAll();
            }
        }

        internal class ExprEnumTwoProperties : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.sequenceEqual(Strvalstwo) as val0 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    "val0".SplitCsv(),
                    new[] {typeof(bool?)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3", "E1,E2,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(SupportCollection.MakeString("E1,E3", "E1,E2,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString("E1,E3", "E1,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3", "E1,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,null,E3", "E1,E2,null,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3", "E1,E2,null"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,null", "E1,E2,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString("E1", ""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString("", "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString("E1", "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(SupportCollection.MakeString("", ""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {true});

                env.SendEventBean(SupportCollection.MakeString(null, ""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.SendEventBean(SupportCollection.MakeString("", null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {false});

                env.SendEventBean(SupportCollection.MakeString(null, null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.UndeployAll();
            }
        }

        internal class ExprEnumInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "select window(*).sequenceEqual(Strvals) from SupportCollection#lastevent";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate select-clause expression 'window(*).sequenceEqual(Strvals)': Invalid input for built-in enumeration method 'sequenceEqual' and 1-parameter footprint, expecting collection of values (typically scalar values) as input, received collection of events of type 'SupportCollection'");
            }
        }
    }
} // end of namespace