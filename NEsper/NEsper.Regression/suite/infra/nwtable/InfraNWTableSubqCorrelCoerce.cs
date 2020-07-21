///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubqCorrelCoerce
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            // named window tests
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, false, false, false)); // no share
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, false, false, true)); // no share create index
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, false, false)); // share
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, false, true)); // share create index
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, true, false)); // disable share
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(true, true, true, true)); // disable share create index

            // table tests
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(false, false, false, false)); // table
            execs.Add(new InfraNWTableSubqCorrelCoerceSimple(false, false, false, true)); // table + create index
            return execs;
        }

        private static void SendWindow(
            RegressionEnvironment env,
            string col0,
            long col1,
            string col2)
        {
            var theEvent = new Dictionary<string, object>();
            theEvent.Put("col0", col0);
            theEvent.Put("col1", col1);
            theEvent.Put("col2", col2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent())
            {
                env.SendEventObjectArray(theEvent.Values.ToArray(), "WindowSchema");
            }
            else
            {
                env.SendEventMap(theEvent, "WindowSchema");
            }
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string e0,
            int e1,
            string e2)
        {
            var theEvent = new Dictionary<string, object>();
            theEvent.Put("e0", e0);
            theEvent.Put("e1", e1);
            theEvent.Put("e2", e2);
            if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent())
            {
                env.SendEventObjectArray(theEvent.Values.ToArray(), "EventSchema");
            }
            else
            {
                env.SendEventMap(theEvent, "EventSchema");
            }
        }

        internal class InfraNWTableSubqCorrelCoerceSimple : RegressionExecution
        {
            private readonly bool _createExplicitIndex;
            private readonly bool _disableIndexShareConsumer;
            private readonly bool _enableIndexShareCreate;
            private readonly bool _namedWindow;

            public InfraNWTableSubqCorrelCoerceSimple(
                bool namedWindow,
                bool enableIndexShareCreate,
                bool disableIndexShareConsumer,
                bool createExplicitIndex)
            {
                _namedWindow = namedWindow;
                _enableIndexShareCreate = enableIndexShareCreate;
                _disableIndexShareConsumer = disableIndexShareConsumer;
                _createExplicitIndex = createExplicitIndex;
            }

            public void Run(RegressionEnvironment env)
            {
                var c1 = env.CompileWBusPublicType("create schema EventSchema(e0 string, e1 int, e2 string)");
                var c2 = env.CompileWBusPublicType("create schema WindowSchema(col0 string, col1 long, col2 string)");
                var path = new RegressionPath();
                path.Add(c1);
                path.Add(c2);
                env.Deploy(c1);
                env.Deploy(c2);

                var createEpl = _namedWindow
                    ? "create window MyInfra#keepall as WindowSchema"
                    : "create table MyInfra (col0 string primary key, col1 long, col2 string)";
                if (_enableIndexShareCreate)
                {
                    createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
                }

                env.CompileDeploy(createEpl, path);
                env.CompileDeploy("insert into MyInfra select * from WindowSchema", path);

                if (_createExplicitIndex)
                {
                    env.CompileDeploy("@name('index') create index MyIndex on MyInfra (col2, col1)", path);
                }

                var fields = new[] { "e0", "val" };
                var consumeEpl =
                    "@name('s0') select e0, (select col0 from MyInfra where col2 = es.e2 and col1 = es.e1) as val from EventSchema es";
                if (_disableIndexShareConsumer)
                {
                    consumeEpl = "@Hint('disable_window_subquery_indexshare') " + consumeEpl;
                }

                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                SendWindow(env, "W1", 10L, "c31");
                SendEvent(env, "E1", 10, "c31");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E1", "W1" });

                SendEvent(env, "E2", 11, "c32");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E2", null });

                SendWindow(env, "W2", 11L, "c32");
                SendEvent(env, "E3", 11, "c32");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E3", "W2" });

                SendWindow(env, "W3", 11L, "c31");
                SendWindow(env, "W4", 10L, "c32");

                SendEvent(env, "E4", 11, "c31");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E4", "W3" });

                SendEvent(env, "E5", 10, "c31");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E5", "W1" });

                SendEvent(env, "E6", 10, "c32");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E6", "W4" });

                // test late start
                env.UndeployModuleContaining("s0");
                env.CompileDeploy(consumeEpl, path).AddListener("s0");

                SendEvent(env, "E6", 10, "c32");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] { "E6", "W4" });

                env.UndeployModuleContaining("s0");
                if (env.Statement("index") != null)
                {
                    env.UndeployModuleContaining("index");
                }

                env.UndeployAll();
            }
        }
    }
} // end of namespace