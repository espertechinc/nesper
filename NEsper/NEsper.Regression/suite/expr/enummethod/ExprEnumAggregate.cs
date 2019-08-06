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

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumAggregate
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ExprEnumAggregateEvents());
            execs.Add(new ExprEnumAggregateScalar());
            return execs;
        }

        internal class ExprEnumAggregateEvents : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"val0", "val1", "val2"};
                var eplFragment = "@Name('s0') select " +
                                  "Contained.aggregate(0, (result, item) => result + item.P00) as val0, " +
                                  "Contained.aggregate('', (result, item) => result || ', ' || item.Id) as val1, " +
                                  "Contained.aggregate('', (result, item) => result || (case when result='' then '' else ',' end) || item.Id) as val2 " +
                                  " from SupportBean_ST0_Container";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(
                    env.Statement("s0").EventType,
                    fields,
                    new[] {typeof(int?), typeof(string), typeof(string)});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,12", "E2,11", "E2,2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {25, ", E1, E2, E2", "E1,E2,E2"});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value());
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0, "", ""});

                env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,12"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12, ", E1", "E1"});

                env.UndeployAll();
            }
        }

        internal class ExprEnumAggregateScalar : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "val0".SplitCsv();
                var eplFragment = "@Name('s0') select " +
                                  "Strvals.aggregate('', (result, item) => result || '+' || item) as val0 " +
                                  "from SupportCollection";
                env.CompileDeploy(eplFragment).AddListener("s0");

                LambdaAssertionUtil.AssertTypes(env.Statement("s0").EventType, fields, new[] {typeof(string)});

                env.SendEventBean(SupportCollection.MakeString("E1,E2,E3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"+E1+E2+E3"});

                env.SendEventBean(SupportCollection.MakeString("E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"+E1"});

                env.SendEventBean(SupportCollection.MakeString(""));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {""});

                env.SendEventBean(SupportCollection.MakeString(null));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.UndeployAll();
            }
        }
    }
} // end of namespace