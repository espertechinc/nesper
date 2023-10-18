///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpLogSink
    {
        public static ICollection<RegressionExecution> Executions() {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowOpLogSinkSettings());
            execs.Add(new EPLDataflowOpLogSinkWWrapper(false));
            execs.Add(new EPLDataflowOpLogSinkWWrapper(true));
            return execs;
        }

        public class EPLDataflowOpLogSinkWWrapper : RegressionExecution
        {
            private readonly bool wrapperWithAdditionalProps;

            public EPLDataflowOpLogSinkWWrapper(bool wrapperWithAdditionalProps)
            {
                this.wrapperWithAdditionalProps = wrapperWithAdditionalProps;
            }

            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema A(value int);\n" +
                    (wrapperWithAdditionalProps
                        ? "insert into B select 'hello world' as hello, * from A; \n"
                        : "insert into B select * from A; \n") +
                    "@name('flow') create dataflow OutputFlow\n" +
                    "  EventBusSource -> TheEvents<B> {}\n" +
                    "  LogSink(TheEvents) { format: \"json\" };\n";

                env.CompileDeploy(epl);
                var instance =
                    env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "OutputFlow");
                instance.Start();

                env.SendEventMap(Collections.SingletonDataMap("value", 10), "A");
                instance.Cancel();

                env.UndeployAll();
            }

            public string Name()
            {
                return $"{this.GetType().Name}{{wrapperWithAdditionalProps={wrapperWithAdditionalProps}}}";
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private class EPLDataflowOpLogSinkSettings : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertion(env, null, null, null, null, null);
                RunAssertion(env, "summary", true, null, null, null);
                RunAssertion(env, "xml", true, null, null, null);
                RunAssertion(env, "json", true, null, null, null);
                RunAssertion(env, "summary", false, null, null, null);
                RunAssertion(
                    env,
                    "summary",
                    true,
                    "dataflow:%df port:%p instanceId:%i title:%t event:%e",
                    "mytitle",
                    null);
                RunAssertion(env, "xml", true, null, null, false);
                RunAssertion(env, "json", true, null, "JSON_HERE", true);

                // invalid: output stream
                env.TryInvalidCompile(
                    "create dataflow DF1 LogSink -> s1 {}",
                    "Failed to obtain operator 'LogSink': LogSink operator does not provide an output stream");

                var docSmple = "@name('flow') create dataflow MyDataFlow\n" +
                               "  BeaconSource -> instream {}\n" +
                               "  // Output textual event to log using defaults.\n" +
                               "  LogSink(instream) {}\n" +
                               "  \n" +
                               "  // Output JSON-formatted to console.\n" +
                               "  LogSink(instream) {\n" +
                               "    format : 'json',\n" +
                               "    layout : '%t [%e]',\n" +
                               "    log : false,\n" +
                               "    linefeed : true,\n" +
                               "    title : 'My Custom Title:'\n" +
                               "  }";
                env.CompileDeploy(docSmple);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private static void RunAssertion(
            RegressionEnvironment env,
            string format,
            bool? log,
            string layout,
            string title,
            bool? linefeed)
        {
            var graph =
                "@name('flow') create dataflow MyConsoleOut\n" +
                "Emitter -> instream<SupportBean>{name : 'e1'}\n" +
                "LogSink(instream) {\n" +
                (format == null ? "" : $"  format: '{format}',\n") +
                (log == null ? "" : $"  log: {log},\n") +
                (layout == null ? "" : $"  layout: '{layout}',\n") +
                (title == null ? "" : $"  title: '{title}',\n") +
                (linefeed == null ? "" : $"  linefeed: {linefeed},\n") +
                "}";
            env.CompileDeploy(graph);

            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyConsoleOut");

            var emitterOp = instance.StartCaptive().Emitters.Get("e1");
            emitterOp.Submit(new SupportBean("E1", 1));

            instance.Cancel();
            env.UndeployAll();
        }
    }
} // end of namespace