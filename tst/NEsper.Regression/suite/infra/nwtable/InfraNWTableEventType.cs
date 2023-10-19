///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableEventType
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInvalid(execs);
            WithDefineFields(execs);
            WithInsertIntoProtected(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInsertIntoProtected(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableEventTypeInsertIntoProtected());
            return execs;
        }

        public static IList<RegressionExecution> WithDefineFields(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableEventTypeDefineFields());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableEventTypeInvalid());
            return execs;
        }

        private class InfraNWTableEventTypeInsertIntoProtected : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "module test;\n" +
                          "@name('event') @public @buseventtype @public create map schema Fubar as (foo string, bar double);\n" +
                          "@name('window') @protected create window Snafu#keepall as Fubar;\n" +
                          "@name('insert') @private insert into Snafu select * from Fubar;\n";
                env.CompileDeploy(epl);

                env.SendEventMap(CollectionUtil.BuildMap("foo", "a", "bar", 1d), "Fubar");
                env.SendEventMap(CollectionUtil.BuildMap("foo", "b", "bar", 2d), "Fubar");

                env.AssertPropsPerRowIterator(
                    "window",
                    "foo,bar".SplitCsv(),
                    new object[][] { new object[] { "a", 1d }, new object[] { "b", 2d } });

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
                string epl;

                // name cannot be the same as an existing event type
                epl = "create schema SchemaOne as (p0 string);\n" +
                      "create window SchemaOne#keepall as SchemaOne;\n";
                env.TryInvalidCompile(
                    epl,
                    "Error starting statement: An event type or schema by name 'SchemaOne' already exists");

                epl = "create schema SchemaTwo as (p0 string);\n" +
                      "create table SchemaTwo(c0 int);\n";
                env.TryInvalidCompile(
                    epl,
                    "An event type by name 'SchemaTwo' has already been declared");
            }
        }

        private static void RunAssertionType(
            RegressionEnvironment env,
            bool namedWindow)
        {
            var eplCreate = namedWindow
                ? "@name('s0') @public create window MyInfra#keepall as (c0 int[], c1 int[primitive])"
                : "@name('s0') @public create table MyInfra (c0 int[], c1 int[primitive])";
            env.CompileDeploy(eplCreate);

            var expectedType = new object[][]
                { new object[] { "c0", typeof(int?[]) }, new object[] { "c1", typeof(int[]) } };
            env.AssertStatement(
                "s0",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            env.UndeployAll();
        }
    }
} // end of namespace