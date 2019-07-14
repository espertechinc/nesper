///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableEventType : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionType(env, true);
            RunAssertionType(env, false);

            string epl;

            // name cannot be the same as an existing event type
            epl = "create schema SchemaOne as (p0 string);\n" +
                  "create window SchemaOne#keepall as SchemaOne;\n";
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                epl,
                "Error starting statement: An event type or schema by name 'SchemaOne' already exists");

            epl = "create schema SchemaTwo as (p0 string);\n" +
                  "create table SchemaTwo(c0 int);\n";
            SupportMessageAssertUtil.TryInvalidCompile(
                env,
                epl,
                "An event type by name 'SchemaTwo' has already been declared");
        }

        private static void RunAssertionType(
            RegressionEnvironment env,
            bool namedWindow)
        {
            var eplCreate = namedWindow
                ? "@Name('s0') create window MyInfra#keepall as (c0 int[], c1 int[primitive])"
                : "@Name('s0') create table MyInfra (c0 int[], c1 int[primitive])";
            env.CompileDeploy(eplCreate);

            object[][] expectedType = {new object[] {"c0", typeof(int?[])}, new object[] {"c1", typeof(int[])}};
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            env.UndeployAll();
        }
    }
} // end of namespace