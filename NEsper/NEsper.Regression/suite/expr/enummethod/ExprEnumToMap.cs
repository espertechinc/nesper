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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.enummethod
{
    public class ExprEnumToMap : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            // - duplicate value allowed, latest value wins
            // - null key & value allowed

            var eplFragment =
                "@Name('s0') select Contained.toMap(c => Id, c=> P00) as val from SupportBean_ST0_Container";
            env.CompileDeploy(eplFragment).AddListener("s0");

            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                "val".SplitCsv(),
                new[] {typeof(IDictionary<string, object>)});

            env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,5"));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                "E1,E2,E3".SplitCsv(),
                1,
                5,
                12);

            env.SendEventBean(SupportBean_ST0_Container.Make2Value("E1,1", "E3,12", "E2,12", "E1,2"));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                "E1,E2,E3".SplitCsv(),
                2,
                12,
                12);

            env.SendEventBean(
                new SupportBean_ST0_Container(Collections.SingletonList(new SupportBean_ST0(null, null))));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val"),
                "E1,E2,E3".SplitCsv(),
                null,
                null,
                null);
            env.UndeployAll();

            // test scalar-coll with lambda
            var fields = "val0".SplitCsv();
            var eplLambda = "@Name('s0') select " +
                            "Strvals.toMap(c => c, c => extractNum(c)) as val0 " +
                            "from SupportCollection";
            env.CompileDeploy(eplLambda).AddListener("s0");
            LambdaAssertionUtil.AssertTypes(
                env.Statement("s0").EventType,
                fields,
                new[] {typeof(IDictionary<string, object>)});

            env.SendEventBean(SupportCollection.MakeString("E2,E1,E3"));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                "E1,E2,E3".SplitCsv(),
                1,
                2,
                3);

            env.SendEventBean(SupportCollection.MakeString("E1"));
            EPAssertionUtil.AssertPropsMap(
                (IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0"),
                "E1".SplitCsv(),
                1);

            env.SendEventBean(SupportCollection.MakeString(null));
            Assert.IsNull(env.Listener("s0").AssertOneGetNewAndReset().Get("val0"));

            env.SendEventBean(SupportCollection.MakeString(""));
            Assert.AreEqual(
                0,
                ((IDictionary<string, object>) env.Listener("s0").AssertOneGetNewAndReset().Get("val0")).Count);

            env.UndeployAll();
        }
    }
} // end of namespace