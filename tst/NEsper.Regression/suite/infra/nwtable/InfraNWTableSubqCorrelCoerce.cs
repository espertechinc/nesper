///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqCorrelCoerce
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();

            WithCoerceSimpleWithNamedWindowsNoShare(execs);
            WithCoerceSimpleWithNamedWindowsShare(execs);
            WithCoerceSimpleWithNamedWindowsDisableShare(execs);
            WithCoerceSimpleWithTables(execs);
            
            return execs;
        }

        public static IList<RegressionExecution> WithCoerceSimpleWithTables(IList<RegressionExecution> execs = null)
        {
            // table tests
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(false, false, false, false)); // table
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(false, false, false, true)); // table + create index
            return execs;
        }

        public static IList<RegressionExecution> WithCoerceSimpleWithNamedWindowsNoShare(IList<RegressionExecution> execs = null)
        {
            // named window tests
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, false, false, false)); // no share
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, false, false, true)); // no share create index
            return execs;
        }

        public static IList<RegressionExecution> WithCoerceSimpleWithNamedWindowsShare(IList<RegressionExecution> execs = null)
        {
            // named window tests
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, false, false)); // share
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, false, true)); // share create index
            return execs;
        }

        public static IList<RegressionExecution> WithCoerceSimpleWithNamedWindowsDisableShare(IList<RegressionExecution> execs = null)
        {
            // named window tests
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, true, false)); // disable share
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, true, true)); // disable share create index
            return execs;
        }

        private class InfraNWTableSubqCorrelCoerceSimple : RegressionExecution
        {
            private readonly bool namedWindow;
            private readonly bool enableIndexShareCreate;
            private readonly bool disableIndexShareConsumer;
            private readonly bool createExplicitIndex;

            public InfraNWTableSubqCorrelCoerceSimple(
                bool namedWindow,
                bool enableIndexShareCreate,
                bool disableIndexShareConsumer,
                bool createExplicitIndex)
            {
                this.namedWindow = namedWindow;
                this.enableIndexShareCreate = enableIndexShareCreate;
                this.disableIndexShareConsumer = disableIndexShareConsumer;
                this.createExplicitIndex = createExplicitIndex;
            }

            public void Run(RegressionEnvironment env)
            {
                var c1 = env.Compile("@public @buseventtype create schema EventSchema(e0 string, e1 int, e2 string)");
                var c2 = env.Compile(
                    "@public @buseventtype create schema WindowSchema(col0 string, col1 long, col2 string)");
                var path = new RegressionPath();
                path.Add(c1);
                path.Add(c2);
                env.Deploy(c1);
                env.Deploy(c2);

                var createEpl = namedWindow
                    ? "@public create window MyInfra#keepall as WindowSchema"
                    : "@public create table MyInfra (col0 string primary key, col1 long, col2 string)";
                if (enableIndexShareCreate) {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyInfra select * from WindowSchema", path);

                if (createExplicitIndex) {
                    env.CompileDeploy("@name('index') create index MyIndex on MyInfra (col2, col1)", path);
                }

                var fields = "e0,val".SplitCsv();
                var consumeEpl =
                    "@name('s0') select e0, (select col0 from MyInfra where col2 = es.e2 and col1 = es.e1) as val from EventSchema es";
                if (disableIndexShareConsumer) {
                    consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
                }

                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                SendWindow(env, "W1", 10L, "c31");
                SendEvent(env, "E1", 10, "c31");
                env.AssertPropsNew("s0", fields, new object[] { "E1", "W1" });

                SendEvent(env, "E2", 11, "c32");
                env.AssertPropsNew("s0", fields, new object[] { "E2", null });

                SendWindow(env, "W2", 11L, "c32");
                SendEvent(env, "E3", 11, "c32");
                env.AssertPropsNew("s0", fields, new object[] { "E3", "W2" });

                SendWindow(env, "W3", 11L, "c31");
                SendWindow(env, "W4", 10L, "c32");

                SendEvent(env, "E4", 11, "c31");
                env.AssertPropsNew("s0", fields, new object[] { "E4", "W3" });

                SendEvent(env, "E5", 10, "c31");
                env.AssertPropsNew("s0", fields, new object[] { "E5", "W1" });

                SendEvent(env, "E6", 10, "c32");
                env.AssertPropsNew("s0", fields, new object[] { "E6", "W4" });

                // test late start
                env.UndeployModuleContaining("s0");
                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                SendEvent(env, "E6", 10, "c32");
                env.AssertPropsNew("s0", fields, new object[] { "E6", "W4" });

                env.UndeployModuleContaining("s0");
                if (createExplicitIndex) {
                    env.UndeployModuleContaining("index");
                }

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       ", enableIndexShareCreate=" +
                       enableIndexShareCreate +
                       ", disableIndexShareConsumer=" +
                       disableIndexShareConsumer +
                       ", createExplicitIndex=" +
                       createExplicitIndex +
                       '}';
            }
        }

        private static void SendWindow(
            RegressionEnvironment env,
            string col0,
            long col1,
            string col2)
        {
            IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("col0", col0);
            theEvent.Put("col1", col1);
            theEvent.Put("col2", col2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                env.SendEventObjectArray(theEvent.Values.ToArray(), "WindowSchema");
            }
            else {
                env.SendEventMap(theEvent, "WindowSchema");
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string e0,
            int e1,
            string e2)
        {
            IDictionary<string, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("e0", e0);
            theEvent.Put("e1", e1);
            theEvent.Put("e2", e2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                env.SendEventObjectArray(theEvent.Values.ToArray(), "EventSchema");
            }
            else {
                env.SendEventMap(theEvent, "EventSchema");
            }
        }
    }
} // end of namespace