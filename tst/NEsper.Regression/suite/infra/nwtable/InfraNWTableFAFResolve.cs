///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableFAFResolve : IndexBackingTableInfo
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            Withd(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraSelectWildcard(true));
            execs.Add(new InfraSelectWildcard(false));
            return execs;
        }

        internal class InfraSelectWildcard : RegressionExecution
        {
            private readonly bool namedWindow;

            internal InfraSelectWildcard(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                SetupInfra(env, namedWindow);

                var insertA = CompileRuntimePath(env, "A", "insert into MyInfra(c0, c1) values ('A1', 10)");
                var insertB = CompileRuntimePath(env, "B", "insert into MyInfra(c2, c3) values (20, 'B1')");

                env.Runtime.FireAndForgetService.ExecuteQuery(insertA);
                env.Runtime.FireAndForgetService.ExecuteQuery(insertB);

                var selectA = CompileRuntimePath(env, "A", "select * from MyInfra");
                var selectB = CompileRuntimePath(env, "B", "select * from MyInfra");

                var resultA = env.Runtime.FireAndForgetService.ExecuteQuery(selectA);
                EPAssertionUtil.AssertPropsPerRow(
                    resultA.GetEnumerator(),
                    "c0,c1".SplitCsv(),
                    new object[][] {
                        new object[] { "A1", 10 }
                    });

                var resultB = env.Runtime.FireAndForgetService.ExecuteQuery(selectB);
                EPAssertionUtil.AssertPropsPerRow(
                    resultB.GetEnumerator(),
                    "c2,c3".SplitCsv(),
                    new object[][] {
                        new object[] { 20, "B1" }
                    });

                env.UndeployAll();
            }


            public string Name()
            {
                return $"{this.GetType().Name}{{namedWindow={namedWindow}}}";
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private static EPCompiled CompileRuntimePath(
            RegressionEnvironment env,
            string moduleName,
            string query)
        {
            var args = new CompilerArguments();
            args.Options.ModuleUses = (_) => new HashSet<string>() { moduleName };
            args.Path.Add(env.Runtime.RuntimePath);
            return env.Compiler.CompileQuery(query, args);
        }

        private static void SetupInfra(
            RegressionEnvironment env,
            bool namedWindow)
        {
            var eplCreate = namedWindow
                ? "module A; @Name('TheInfra') @protected create window MyInfra#keepall as (c0 string, c1 int)"
                : "module A; @Name('TheInfra') @protected create table MyInfra as (c0 string primary key, c1 int primary key)";
            env.CompileDeploy(eplCreate);

            eplCreate = namedWindow
                ? "module B; @Name('TheInfra') @protected create window MyInfra#keepall as (c2 int, c3 string)"
                : "module B; @Name('TheInfra') @protected create table MyInfra as (c2 int primary key, c3 string primary key)";
            env.CompileDeploy(eplCreate);
        }
    }
} // end of namespace