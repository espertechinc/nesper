///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework; // assertEquals

namespace com.espertech.esper.regressionlib.suite.epl.insertinto
{
    public class EPLInsertIntoIRStreamFunc : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var fields = "c0,c1".SplitCsv();
            var path = new RegressionPath();

            var stmtTextOne = "@name('i0') @public insert irstream into MyStream " +
                              "select irstream theString as c0, istream() as c1 " +
                              "from SupportBean#lastevent";
            env.CompileDeploy(stmtTextOne, path).AddListener("i0");

            var stmtTextTwo = "@name('s0') select * from MyStream";
            env.CompileDeploy(stmtTextTwo, path).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 0));
            env.AssertPropsNew("i0", fields, new object[] { "E1", true });
            env.AssertPropsNew("s0", fields, new object[] { "E1", true });

            env.SendEventBean(new SupportBean("E2", 0));
            env.AssertPropsIRPair("i0", fields, new object[] { "E2", true }, new object[] { "E1", false });
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                fields,
                new object[][] { new object[] { "E2", true }, new object[] { "E1", false } },
                Array.Empty<object[]>());

            env.SendEventBean(new SupportBean("E3", 0));
            env.AssertPropsIRPair("i0", fields, new object[] { "E3", true }, new object[] { "E2", false });
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                fields,
                new object[][] { new object[] { "E3", true }, new object[] { "E2", false } },
                Array.Empty<object[]>());

            // test SODA
            var eplModel = "@name('s1') select istream() from SupportBean";
            env.EplToModelCompileDeploy(eplModel);
            env.AssertStatement(
                "s1",
                statement => Assert.AreEqual(typeof(bool?), statement.EventType.GetPropertyType("istream()")));

            // test join
            env.UndeployAll();
            fields = "c0,c1,c2".SplitCsv();
            var stmtTextJoin = "@name('s0') select irstream theString as c0, id as c1, istream() as c2 " +
                               "from SupportBean#lastevent, SupportBean_S0#lastevent";
            env.CompileDeploy(stmtTextJoin).AddListener("s0");
            env.SendEventBean(new SupportBean("E1", 0));
            env.SendEventBean(new SupportBean_S0(10));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 10, true });

            env.SendEventBean(new SupportBean("E2", 0));
            env.AssertPropsIRPair("s0", fields, new object[] { "E2", 10, true }, new object[] { "E1", 10, false });

            env.UndeployAll();
        }
    }
} // end of namespace