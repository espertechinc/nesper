///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.dataflow;

using NUnit.Framework; // assertEquals

// assertSame

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowOpFilter
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInvalid(execs);
            WithAllTypes(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAllTypes(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowAllTypes());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLDataflowInvalid());
            return execs;
        }

        private class EPLDataflowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // invalid: no filter
                env.TryInvalidCompile(
                    "create dataflow DF1 BeaconSource -> instream<SupportBean> {} Filter(instream) -> abc {}",
                    "Failed to obtain operator 'Filter': Required parameter 'filter' providing the filter expression is not provided");

                // invalid: too many output streams
                env.TryInvalidCompile(
                    "create dataflow DF1 BeaconSource -> instream<SupportBean> {} Filter(instream) -> abc,def,efg { filter : true }",
                    "Failed to obtain operator 'Filter': Filter operator requires one or two output stream(s) but produces 3 streams");

                // invalid: too few output streams
                env.TryInvalidCompile(
                    "create dataflow DF1 BeaconSource -> instream<SupportBean> {} Filter(instream) { filter : true }",
                    "Failed to obtain operator 'Filter': Filter operator requires one or two output stream(s) but produces 0 streams");

                // invalid filter expressions
                TryInvalidFilter(
                    env,
                    "TheString = 1",
                    "Failed to obtain operator 'Filter': Failed to validate filter dataflow operator expression 'TheString=1': Implicit conversion from datatype 'Integer' to 'String' is not allowed");

                TryInvalidFilter(
                    env,
                    "prev(TheString, 1) = 'abc'",
                    "Failed to obtain operator 'Filter': Invalid filter dataflow operator expression 'prev(TheString,1)=\"abc\"': Aggregation, sub-select, previous or prior functions are not supported in this context");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW, RegressionFlag.INVALIDITY);
            }
        }

        private class EPLDataflowAllTypes : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionAllTypes(
                    env,
                    DefaultSupportGraphEventUtil.EVENTTYPENAME,
                    DefaultSupportGraphEventUtil.GetPONOEvents());
                RunAssertionAllTypes(env, "MyXMLEvent", DefaultSupportGraphEventUtil.GetXMLEvents());
                RunAssertionAllTypes(env, "MyOAEvent", DefaultSupportGraphEventUtil.GetOAEvents());
                RunAssertionAllTypes(env, "MyMapEvent", DefaultSupportGraphEventUtil.GetMapEvents());

                // test doc sample
                var epl = "@name('flow') create dataflow MyDataFlow\n" +
                          "  create schema SampleSchema(tagId string, locX double),\t// sample type\n" +
                          "  BeaconSource -> samplestream<SampleSchema> {}\n" +
                          "  \n" +
                          "  // Filter all events that have a tag Id of '001'\n" +
                          "  Filter(samplestream) -> tags_001 {\n" +
                          "    filter : tagId = '001' \n" +
                          "  }\n" +
                          "  \n" +
                          "  // Filter all events that have a tag Id of '001', putting all other tags into the second stream\n" +
                          "  Filter(samplestream) -> tags_001, tags_other {\n" +
                          "    filter : tagId = '001' \n" +
                          "  }";
                env.CompileDeploy(epl);
                env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyDataFlow");
                env.UndeployAll();

                // test two streams
                DefaultSupportCaptureOpStatic<object>.Instances.Clear();
                var graph = "@name('flow') create dataflow MyFilter\n" +
                            "Emitter -> sb<SupportBean> {name : 'e1'}\n" +
                            "Filter(sb) -> out.ok, out.fail {filter: TheString = 'x'}\n" +
                            "DefaultSupportCaptureOpStatic(out.ok) {}" +
                            "DefaultSupportCaptureOpStatic(out.fail) {}";
                env.CompileDeploy(graph);

                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MyFilter");
                var captive = instance.StartCaptive();

                captive.Emitters.Get("e1").Submit(new SupportBean("x", 10));
                captive.Emitters.Get("e1").Submit(new SupportBean("y", 11));
                Assert.AreEqual(
                    10,
                    ((SupportBean)DefaultSupportCaptureOpStatic<object>.Instances[0].Current[0]).IntPrimitive);
                Assert.AreEqual(
                    11,
                    ((SupportBean)DefaultSupportCaptureOpStatic<object>.Instances[1].Current[0]).IntPrimitive);
                DefaultSupportCaptureOpStatic<object>.Instances.Clear();

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.DATAFLOW);
            }
        }

        private static void TryInvalidFilter(
            RegressionEnvironment env,
            string filter,
            string message)
        {
            var graph = "@name('flow') create dataflow MySelect\n" +
                        "DefaultSupportSourceOp -> instream<SupportBean>{}\n" +
                        "Filter(instream as ME) -> outstream {filter: " +
                        filter +
                        "}\n" +
                        "DefaultSupportCaptureOp(outstream) {}";
            env.TryInvalidCompile(graph, message);
        }

        private static void RunAssertionAllTypes(
            RegressionEnvironment env,
            string typeName,
            object[] events)
        {
            var graph = "@name('flow') create dataflow MySelect\n" +
                        "DefaultSupportSourceOp -> instream.with.dot<" +
                        typeName +
                        ">{}\n" +
                        "Filter(instream.with.dot) -> outstream.dot {filter: MyString = 'two'}\n" +
                        "DefaultSupportCaptureOp(outstream.dot) {}";
            env.CompileDeploy(graph);

            var source = new DefaultSupportSourceOp(events);
            var capture = new DefaultSupportCaptureOp<object>(2, env.Container.LockManager());
            var options = new EPDataFlowInstantiationOptions();
            options.DataFlowInstanceUserObject = "myuserobject";
            options.DataFlowInstanceId = "myinstanceid";
            options.WithOperatorProvider(new DefaultSupportGraphOpProvider(source, capture));
            var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "MySelect", options);
            Assert.AreEqual("myuserobject", instance.UserObject);
            Assert.AreEqual("myinstanceid", instance.InstanceId);

            instance.Run();

            var result = capture.GetAndReset()[0].ToArray();
            Assert.AreEqual(1, result.Length);
            Assert.AreSame(events[1], result[0]);

            instance.Cancel();

            env.UndeployAll();
        }
    }
} // end of namespace