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
            var stmtText = "@name('s0') select MYINT from " +
                           " sql:MyDBUpperCase ['" +
                           sql +
                           "] as S0," +
                           "SupportBean#length(100) as S1";
            env.CompileDeploy(stmtText).AddListener("s0");

            env.AssertStatement("s0", statement => Assert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("MYINT")));

            SendSupportBeanEvent(env, "A");
            env.AssertEqualsNew("s0", "MYINT", 10);

            SendSupportBeanEvent(env, "H");
            env.AssertEqualsNew("s0", "MYINT", 10);

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