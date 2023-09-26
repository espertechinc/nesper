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

namespace com.espertech.esper.regressionlib.suite.rowrecog
{
    public class RowRecogVariantStream : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@public create variant schema MyVariantType as SupportBean_S0, SupportBean_S1", path);

            var fields = new[] { "a", "b" };
            var text = "@name('s0') select * from MyVariantType#keepall " +
                       "match_recognize (" +
                       "  measures A.Id? as a, B.Id? as b" +
                       "  pattern (A B) " +
                       "  define " +
                       "    A as typeof(A) = 'SupportBean_S0'," +
                       "    B as typeof(B) = 'SupportBean_S1'" +
                       ")";

            env.CompileDeploy(text, path).AddListener("s0");
            env.CompileDeploy("insert into MyVariantType select * from SupportBean_S0", path);
            env.CompileDeploy("insert into MyVariantType select * from SupportBean_S1", path);

            env.SendEventBean(new SupportBean_S0(1, "S0"));
            env.SendEventBean(new SupportBean_S1(2, "S1"));
            env.AssertPropsPerRowLastNew(
                "s0",
                fields,
                new[] { new object[] { 1, 2 } });
            EPAssertionUtil.AssertPropsPerRow(
                env.Statement("s0").GetEnumerator(),
                fields,
                new[] { new object[] { 1, 2 } });

            var epl = "// Declare one sample type\n" +
                      "create schema ST0 as (col string)\n;" +
                      "// Declare second sample type\n" +
                      "create schema ST1 as (col string)\n;" +
                      "// Declare variant stream holding either type\n" +
                      "create variant schema MyVariantStream as ST0, ST1\n;" +
                      "// Populate variant stream\n" +
                      "insert into MyVariantStream select * from ST0\n;" +
                      "// Populate variant stream\n" +
                      "insert into MyVariantStream select * from ST1\n;" +
                      "// Simple pattern to match ST0 ST1 pairs\n" +
                      "select * from MyVariantType#time(1 min)\n" +
                      "match_recognize (\n" +
                      "measures A.Id? as a, B.Id? as b\n" +
                      "pattern (A B)\n" +
                      "define\n" +
                      "A as typeof(A) = 'ST0',\n" +
                      "B as typeof(B) = 'ST1'\n" +
                      ");";
            env.CompileDeploy(epl, path);
            env.UndeployAll();
        }
    }
} // end of namespace