///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableEventType
    {
        public static IList<RegressionExecution> Executions() {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new InfraNWTableEventTypeInvalid());
            execs.Add(new InfraNWTableEventTypeDefineFields());
            execs.Add(new InfraNWTableEventTypeInsertIntoProtected());
            return execs;
        }

        private class InfraNWTableEventTypeInsertIntoProtected : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "module test;\n" +
                             "@Name('event') @buseventtype @public create map schema Fubar as (foo string, bar double);\n" +
                             "@Name('window') @protected create window Snafu#keepall as Fubar;\n" +
                             "@Name('insert') @private insert into Snafu select * from Fubar;\n";
                env.CompileDeploy(epl);

                env.SendEventMap(CollectionUtil.BuildMap("foo", "a", "bar", 1d), "Fubar");
                env.SendEventMap(CollectionUtil.BuildMap("foo", "b", "bar", 2d), "Fubar");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("window"), "foo,bar".SplitCsv(), new Object[][] {
                    new object[] {"a", 1d}, 
                    new object[] {"b", 2d}
                });

                env.UndeployAll();
            }
        }

        private class InfraNWTableEventTypeDefineFields : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionType(env, true);
                RunAssertionType(env, false);
            }
        }

        private class InfraNWTableEventTypeInvalid : RegressionExecution
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
        }

        private static void RunAssertionType(
            RegressionEnvironment env,
            bool namedWindow)
        {
            var eplCreate = namedWindow
                ? "@Name('s0') create window MyInfra#keepall as (c0 int[], c1 int[primitive])"
                : "@Name('s0') create table MyInfra (c0 int[], c1 int[primitive])";
            env.CompileDeploy(eplCreate);

            object[][] expectedType = {
                new object[] {
                    "c0", typeof(int?[])
                },
                new object[] {
                    "c1", typeof(int[])
                }
            };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType,
                env.Statement("s0").EventType,
                SupportEventTypeAssertionEnum.NAME,
                SupportEventTypeAssertionEnum.TYPE);

            env.UndeployAll();
        }
    }
} // end of namespace