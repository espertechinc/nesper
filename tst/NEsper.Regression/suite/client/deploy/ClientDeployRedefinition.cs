///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.deploy
{
    public class ClientDeployRedefinition
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithCreateSchemaNamedWindowInsert(execs);
            WithNamedWindow(execs);
            WithInsertInto(execs);
            WithVariables(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithVariables(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRedefinitionVariables());
            return execs;
        }

        public static IList<RegressionExecution> WithInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRedefinitionInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRedefinitionNamedWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateSchemaNamedWindowInsert(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientDeployRedefinitionCreateSchemaNamedWindowInsert());
            return execs;
        }

        internal class ClientDeployRedefinitionCreateSchemaNamedWindowInsert : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "module test.test1;\n" +
                           "create schema MyTypeOne(col1 string, col2 int);" +
                           "create window MyWindowOne#keepall as select * from MyTypeOne;" +
                           "insert into MyWindowOne select * from MyTypeOne;";
                env.CompileDeploy(text).UndeployAll();
                env.CompileDeploy(text).UndeployAll();
                text = "module test.test1;\n" +
                       "create schema MyTypeOne(col1 string, col2 int, col3 long);" +
                       "create window MyWindowOne#keepall as select * from MyTypeOne;" +
                       "insert into MyWindowOne select * from MyTypeOne;";
                env.CompileDeploy(text).UndeployAll();
                Assert.AreEqual(0, SupportFilterServiceHelper.GetFilterSvcCountApprox(env));

                // test on-merge
                var moduleString =
                    "@name('S0') create window MyWindow#unique(IntPrimitive) as SupportBean;\n" +
                    "@name('S1') on MyWindow insert into SecondStream select *;\n" +
                    "@name('S2') on SecondStream merge MyWindow when matched then insert into ThirdStream select * then delete\n";
                var compiled = env.Compile(moduleString);
                env.Deploy(compiled).UndeployAll().Deploy(compiled).UndeployAll();

                // test table
                var moduleTableOne = "create table MyTable(c0 string, c1 string)";
                env.CompileDeploy(moduleTableOne).UndeployAll();
                var moduleTableTwo = "create table MyTable(c0 string, c1 string, c2 string)";
                env.CompileDeploy(moduleTableTwo).UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientDeployRedefinitionNamedWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("create window MyWindow#time(30) as (col1 int, col2 string)");
                env.CompileDeploy("create window MyWindow#time(30) as (col1 short, col2 long)");
                env.UndeployAll();
            }
        }

        internal class ClientDeployRedefinitionInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "create schema MySchema (col1 int, col2 string);" + "insert into MyStream select * from MySchema;");
                env.CompileDeploy(
                    "create schema MySchema (col1 short, col2 long);" + "insert into MyStream select * from MySchema;");
                env.UndeployAll();
            }
        }

        internal class ClientDeployRedefinitionVariables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy(
                    "create variable int MyVar;" +
                    "create schema MySchema (col1 short, col2 long);" +
                    "select MyVar from MySchema;");
                env.CompileDeploy(
                    "create variable string MyVar;" +
                    "create schema MySchema (col1 short, col2 long);" +
                    "select MyVar from MySchema;");
                env.UndeployAll();
            }
        }
    }
} // end of namespace