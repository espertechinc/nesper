///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;
using com.espertech.esperio.file;

using NUnit.Framework;

using static com.espertech.esperio.support.util.CompileUtil;

namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestFileSourceLineGraphs : AbstractIOTest
    {
        private EPRuntime runtime;

        [SetUp]
        public void SetUp()
        {
            var configuration = new Configuration();
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Common.AddImportType(typeof(FileSourceForge));
            configuration.Common.AddImportType(typeof(DefaultSupportCaptureOpForge));
            configuration.Common.AddEventType("MyLineEvent", typeof(MyLineEvent));
            configuration.Common.AddEventType("MyInvalidEvent", typeof(MyInvalidEvent));
            runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
            runtime.Initialize();
        }

        private void TryInvalid(
            string dataflowName,
            string epl,
            string substituion,
            string message)
        {
            epl = epl.Replace("${SUBS_HERE}", substituion);
            var stmtGraph = CompileDeploy(runtime, epl).Statements[0];
            try {
                var outputOp = new DefaultSupportCaptureOp<object>(container.LockManager());
                runtime.DataFlowService.Instantiate(
                    stmtGraph.DeploymentId,
                    dataflowName,
                    new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
                Assert.Fail();
            }
            catch (EPDataFlowInstantiationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
            finally {
                UndeployAll(runtime);
            }
        }

        private IList<IList<object>> RunDataFlow(string epl)
        {
            var deployment = CompileDeploy(runtime, epl);

            var outputOp = new DefaultSupportCaptureOp<object>(
                container.LockManager());
            var instance = runtime.DataFlowService.Instantiate(
                deployment.DeploymentId,
                "ReadCSV",
                new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
            instance.Run();
            return outputOp.GetAndReset();
        }

        public class MyLineEvent
        {
            public MyLineEvent()
            {
            }

            public MyLineEvent(string theLine)
            {
                TheLine = theLine;
            }

            public string TheLine { get; set; }

            protected bool Equals(MyLineEvent other)
            {
                return TheLine == other.TheLine;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) {
                    return false;
                }

                if (ReferenceEquals(this, obj)) {
                    return true;
                }

                if (obj.GetType() != GetType()) {
                    return false;
                }

                return Equals((MyLineEvent) obj);
            }

            public override int GetHashCode()
            {
                return TheLine != null ? TheLine.GetHashCode() : 0;
            }
        }

        public class MyInvalidEvent
        {
            public int SomeInt { get; set; }
        }

        [Test, RunInApplicationDomain]
        public void TestEndOfFileMarker()
        {
            CompileDeploy(runtime, "@public @buseventtype create objectarray schema MyBOF (filename string)");
            CompileDeploy(runtime, "@public @buseventtype create objectarray schema MyEOF (filename string)");
            CompileDeploy(runtime, "@public @buseventtype create objectarray schema MyLine (filename string, line string)");

            CompileDeploy(
                runtime,
                "@public create context FileContext " +
                "initiated by MyBOF as mybof " +
                "terminated by MyEOF(filename=mybof.filename)");

            var stmtCount = CompileDeploy(
                    runtime,
                    "context FileContext " +
                    "select context.mybof.filename as filename, count(*) as cnt " +
                    "from MyLine(filename=context.mybof.filename) " +
                    "output snapshot when terminated")
                .Statements[0];
            var listener = new SupportUpdateListener();
            stmtCount.AddListener(listener);

            var epl = "create dataflow MyEOFEventFileReader " +
                      "FileSource -> mylines<MyLine>, mybof<MyBOF>, myeof<MyEOF> { " +
                      "classpathFile: true, numLoops: 1, format: 'line', " +
                      "propertyNameLine: 'line', propertyNameFile: 'filename'}\n" +
                      "EventBusSink(mylines, mybof, myeof) {}\n";
            var deployment = CompileDeploy(runtime, epl);

            foreach (var filename in new[] {"regression/line_file_1.txt", "regression/line_file_2.txt"}) {
                var options = new EPDataFlowInstantiationOptions();
                options.AddParameterURI("FileSource/file", filename);
                var instance = runtime.DataFlowService.Instantiate(deployment.DeploymentId, "MyEOFEventFileReader", options);
                instance.Run();
                Assert.AreEqual(1, instance.Parameters.Count);
                Assert.AreEqual(filename, instance.Parameters.Get("FileSource/file"));
            }

            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                listener.NewDataListFlattened,
                "filename,cnt".SplitCsv(),
                new[] {
                    new object[] {"regression/line_file_1.txt", 3L},
                    new object[] {"regression/line_file_2.txt", 2L}
                });
        }

        [Test, RunInApplicationDomain]
        public void TestFileBeanEvent()
        {
            var graph = "create dataflow ReadCSV " +
                        "FileSource -> mystream<MyLineEvent> { " +
                        "  file: 'regression/myzippedtext.zip', " +
                        "  classpathFile: true, " +
                        "  format: 'line'," +
                        "  propertyNameLine: 'theLine'" +
                        "}" +
                        "DefaultSupportCaptureOp(mystream) {}";
            var received = RunDataFlow(graph);
            Assert.AreEqual(1, received.Count);
            var compare = received[0].ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(
                compare,
                new object[] {
                    new MyLineEvent("this is the first line"),
                    new MyLineEvent("this is the second line"), new MyLineEvent("this is the third line")
                });
        }

        [Test, RunInApplicationDomain]
        public void TestInvalid()
        {
            var epl = "create dataflow FlowOne " +
                      "FileSource -> mystream<MyInvalidEvent> { file: 'regression/myzippedtext.zip', classpathFile: true, format: 'line'," +
                      "${SUBS_HERE}}" +
                      "DefaultSupportCaptureOp(mystream) {}";

            TryInvalid(
                "FlowOne",
                epl,
                "",
                "Failed to instantiate data flow 'FlowOne': Failed to obtain operator instance for 'FileSource': Expecting an output event type that has a single property that is of type string, or alternatively specify the 'propertyNameLine' parameter");

            TryInvalid(
                "FlowOne",
                epl,
                "propertyNameLine: 'xxx'",
                "Failed to instantiate data flow 'FlowOne': Failed to obtain operator instance for 'FileSource': Failed to find property name 'xxx' in type 'MyInvalidEvent'");

            TryInvalid(
                "FlowOne",
                epl,
                "propertyNameLine: 'someInt'",
                "Failed to instantiate data flow 'FlowOne': Failed to obtain operator instance for 'FileSource': Invalid property type for property 'someInt', expected a property of type String");
        }

        [Test, RunInApplicationDomain]
        public void TestPropertyOrderWLoop()
        {
            var graph = "create dataflow ReadCSV " +
                        "create objectarray schema MyLine (line string)," +
                        "FileSource -> mystream<MyLine> { file: 'regression/ints.csv', classpathFile: true, numLoops: 3, format: 'line'}" +
                        "DefaultSupportCaptureOp(mystream) {}";
            var received = RunDataFlow(graph);
            Assert.AreEqual(1, received.Count);
            var compare = received[0].ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(compare, new object[] {new object[] {"1, 0"}, new object[] {"2, 0"}, new object[] {"3, 0"}});
        }

        [Test, RunInApplicationDomain]
        public void TestZipFileLine()
        {
            var graph = "create dataflow ReadCSV " +
                        "create objectarray schema MyLine (line string)," +
                        "FileSource -> mystream<MyLine> { file: 'regression/myzippedtext.zip', classpathFile: true, format: 'line'}" +
                        "DefaultSupportCaptureOp(mystream) {}";
            var received = RunDataFlow(graph);
            Assert.AreEqual(1, received.Count);
            var compare = received[0].ToArray();
            EPAssertionUtil.AssertEqualsExactOrder(
                compare,
                new object[] {
                    new object[] {"this is the first line"},
                    new object[] {"this is the second line"}, new object[] {"this is the third line"}
                });
        }
    }
} // end of namespace