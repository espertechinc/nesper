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
using com.espertech.esper.regressionlib.support.events;

using static com.espertech.esper.regressionlib.support.events.SupportGenericColUtil;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableCreate
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            Withe(execs);
            return execs;
        }

        public static IList<RegressionExecution> Withe(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraCreateGenericColType(true));
            execs.Add(new InfraCreateGenericColType(false));
            return execs;
        }

        private class InfraCreateGenericColType : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraCreateGenericColType(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl = "@public @buseventtype create schema MyInputEvent(" +
                          SupportGenericColUtil.AllNamesAndTypes() +
                          ");\n";
                epl += "@name('infra')";
                epl += namedWindow ? "create window MyInfra#keepall as (" : "create table MyInfra as (";
                epl += SupportGenericColUtil.AllNamesAndTypes();
                epl += ");\n";
                epl += "on MyInputEvent merge MyInfra insert select " + SupportGenericColUtil.AllNames() + ";\n";

                env.CompileDeploy(epl);
                env.AssertStatement("infra", statement => AssertPropertyTypes(statement.EventType));

                env.SendEventMap(SupportGenericColUtil.GetSampleEvent(), "MyInputEvent");

                env.Milestone(0);

                env.AssertIterator(
                    "infra",
                    iterator => SupportGenericColUtil.Compare(env.GetEnumerator("infra").Advance()));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.SERDEREQUIRED);
            }
        }
    }
} // end of namespace