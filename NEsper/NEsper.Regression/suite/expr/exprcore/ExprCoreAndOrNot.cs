///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.expr.exprcore
{
    public class ExprCoreAndOrNot : RegressionExecution
    {
        private static readonly string[] FIELDS = "c0,c1,c2".SplitCsv();

        public void Run(RegressionEnvironment env)
        {
            var epl = "@Name('s0') select " +
                      "(intPrimitive=1) or (intPrimitive=2) as c0, " +
                      "(intPrimitive>0) and (intPrimitive<3) as c1," +
                      "not(intPrimitive=2) as c2" +
                      " from SupportBean";
            env.CompileDeploy(epl).AddListener("s0");

            MakeSendBeanAssert(
                env,
                1,
                new object[] {true, true, true});

            MakeSendBeanAssert(
                env,
                2,
                new object[] {true, true, false});

            env.Milestone(0);

            MakeSendBeanAssert(
                env,
                3,
                new object[] {false, false, true});

            env.UndeployAll();
        }

        private void MakeSendBeanAssert(
            RegressionEnvironment env,
            int intPrimitive,
            object[] expected)
        {
            var bean = new SupportBean("", intPrimitive);
            env.SendEventBean(bean);
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), FIELDS, expected);
        }
    }
} // end of namespace