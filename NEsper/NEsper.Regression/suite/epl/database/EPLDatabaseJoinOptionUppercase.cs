///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.database
{
    public class EPLDatabaseJoinOptionUppercase : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var sql = "select myint from mytesttable where ${TheString} = myvarchar'" +
                      "metadatasql 'select myint from mytesttable'";
            var stmtText = "@Name('s0') select MYINT from " +
                           " sql:MyDBUpperCase ['" +
                           sql +
                           "] as s0," +
                           "SupportBean#length(100) as s1";
            env.CompileDeploy(stmtText).AddListener("s0");

            Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("MYINT"));

            SendSupportBeanEvent(env, "A");
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("MYINT"));

            SendSupportBeanEvent(env, "H");
            Assert.AreEqual(80, env.Listener("s0").AssertOneGetNewAndReset().Get("MYINT"));

            env.UndeployAll();
        }

        private static void SendSupportBeanEvent(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
        }
    }
} // end of namespace