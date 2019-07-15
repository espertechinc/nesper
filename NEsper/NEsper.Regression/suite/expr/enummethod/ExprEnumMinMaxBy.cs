///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumMinMaxBy : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = "val0,val1,val2,val3".SplitCsv();
            var eplFragment = "@Name('s0') select " +
                              "contained.minBy(x => p00) as val0," +
                              "contained.maxBy(x => p00) as val1," +
                              "contained.minBy(x => p00).Id as val2," +
                              "contained.maxBy(x => p00).p00 as val3 " +
                              "from SupportBean_ST0_Container";
            env.CompileDeploy(eplFragment).AddListener("s0");

            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                fields,
                new[] {typeof(SupportBean_ST0), typeof(SupportBean_ST0), typeof(string), typeof(int?)});

            var bean = SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2");
            env.SendEventBean(bean);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {bean.Contained[2], bean.Contained[0], "E2", 12});

            bean = SupportBean_ST0_Container.Make2Value("E1,12");
            env.SendEventBean(bean);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {bean.Contained[0], bean.Contained[0], "E1", 12});

            env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null, null});

            env.SendEventBean(SupportBean_ST0_Container.Make2Value());
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {null, null, null, null});
            env.UndeployAll();

            // test scalar-coll with lambda
            var fieldsLambda = "val0,val1".SplitCsv();
            var eplLambda = "@Name('s0') select " +
                            "strvals.minBy(v => extractNum(v)) as val0, " +
                            "strvals.maxBy(v => extractNum(v)) as val1 " +
                            "from SupportCollection";
            env.CompileDeploy(eplLambda).AddListener("s0");
            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                fieldsLambda,
                new[] {typeof(string), typeof(string)});

            env.SendEventBean(SupportCollection.MakeString("E2,E1,E5,E4"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLambda,
                new object[] {"E1", "E5"});

            env.SendEventBean(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLambda,
                new object[] {"E1", "E1"});
            env.Listener("s0").Reset();

            env.SendEventBean(SupportCollection.MakeString(null));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLambda,
                new object[] {null, null});
            env.Listener("s0").Reset();

            env.SendEventBean(SupportCollection.MakeString(""));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fieldsLambda,
                new object[] {null, null});

            env.UndeployAll();
        }
    }
} // end of namespace