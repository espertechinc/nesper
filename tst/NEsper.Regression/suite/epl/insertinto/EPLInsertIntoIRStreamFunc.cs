///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoIRStreamFunc : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = new[] {"c0", "c1"};
            var path = new RegressionPath();

            var stmtTextOne = "@Name('i0') insert irstream into MyStream " +
                              "select irstream TheString as c0, istream() as c1 " +
                              "from SupportBean#lastevent";
            env.CompileDeploy(stmtTextOne, path).AddListener("i0");

            var stmtTextTwo = "@Name('s0') select * from MyStream";
            env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("i0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", true});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", true});

            env.SendEventBean(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("i0").AssertPairGetIRAndReset(),
                fields,
                new object[] {"E2", true},
                new object[] {"E1", false});
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetDataListsFlattened(),
                fields,
                new[] {new object[] {"E2", true}, new object[] {"E1", false}},
                new object[0][]);

            env.SendEventBean(new SupportBean("E3", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("i0").AssertPairGetIRAndReset(),
                fields,
                new object[] {"E3", true},
                new object[] {"E2", false});
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").GetAndResetDataListsFlattened(),
                fields,
                new[] {new object[] {"E3", true}, new object[] {"E2", false}},
                new object[0][]);

            // test SODA
            var eplModel = "@Name('s1') select istream() from SupportBean";
            env.EplToModelCompileDeploy(eplModel);
            Assert.AreEqual(typeof(bool?), env.Statement("s1").EventType.GetPropertyType("istream()"));

            // test join
            env.UndeployAll();
            fields = new[] {"c0", "c1", "c2"};
            var stmtTextJoin = "@Name('s0') select irstream TheString as c0, Id as c1, istream() as c2 " +
                               "from SupportBean#lastevent, SupportBean_S0#lastevent";
            env.CompileDeploy(stmtTextJoin).AddListener("s0");
            env.SendEventBean(new SupportBean("E1", 0));
            env.SendEventBean(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"E1", 10, true});

            env.SendEventBean(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastOldData[0],
                fields,
                new object[] {"E1", 10, false});
            EPAssertionUtil.AssertProps(
                env.Listener("s0").LastNewData[0],
                fields,
                new object[] {"E2", 10, true});

            env.UndeployAll();
        }
    }
} // end of namespace