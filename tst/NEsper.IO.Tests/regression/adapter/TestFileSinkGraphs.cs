///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.file;
using com.espertech.esperio.support.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esperio.support.util.CompileUtil;

namespace com.espertech.esperio.regression.adapter
{
	[TestFixture]
	public class TestFileSinkGraphs : AbstractIOTest
	{
		private EPRuntimeProvider _runtimeProvider;
		private EPRuntime _runtime;

		[SetUp]
		public void SetUp()
		{
			var configuration = new Configuration(SupportContainer.Reset());
			configuration.Runtime.Threading.IsInternalTimerEnabled = false;
			configuration.Common.AddImportNamespace(typeof(FileSinkFactory));
			configuration.Common.AddImportNamespace(typeof(DefaultSupportSourceOpForge));
			configuration.Common.EventMeta.IsEnableXmlXsd = true;
			
			DefaultSupportGraphEventUtil.AddTypeConfiguration(configuration);
			_runtimeProvider = new EPRuntimeProvider();
			_runtime = _runtimeProvider.GetDefaultRuntimeInstance(configuration);
			_runtime.Initialize();
		}

		[Test]
		public void TestInvalid()
		{
			string graph;

			graph = "create dataflow FlowOne " +
			        "DefaultSupportSourceOp -> mystreamOne<MyMapEvent> {}" +
			        "FileSink(mystreamOne, mystreamOne) {file: 'x:\\a.bb'}";
			TryInvalidCompileGraph(_runtime, graph,
				"Failed to obtain operator 'FileSink': " +
				"FileSinkForge expected a single input port");

			graph = "create dataflow FlowOne " +
			        "DefaultSupportSourceOp -> mystreamOne<MyMapEvent> {}" +
			        "FileSink(mystreamOne) {}";
			TryInvalidInstantiate(
				"FlowOne",
				graph,
				"Failed to instantiate data flow 'FlowOne': " +
				"Failed to obtain operator instance for 'FileSink': " +
				"Parameter by name 'file' has no value");
		}

		private void TryInvalidInstantiate(
			string dataflowName,
			string epl,
			string message)
		{
			var stmtGraph = CompileDeploy(_runtime, epl).Statements[0];
			var outputOp = new DefaultSupportCaptureOp(container.LockManager());
			try {
				_runtime.DataFlowService.Instantiate(
					stmtGraph.DeploymentId,
					dataflowName,
					new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
				Assert.Fail();
			}
			catch (EPDataFlowInstantiationException ex) {
				ClassicAssert.AreEqual(message, ex.Message);
			}

			try {
				_runtime.DeploymentService.UndeployAll();
			}
			catch (EPUndeployException e) {
				throw new EPRuntimeException(e);
			}
		}

		[Test]
		public void TestWriteCSV()
		{
			RunAssertion("MyXMLEvent", DefaultSupportGraphEventUtil.GetXMLEvents(), true);
			RunAssertion("MyOAEvent", DefaultSupportGraphEventUtil.GetOAEvents(), true);
			RunAssertion("MyMapEvent", DefaultSupportGraphEventUtil.GetMapEvents(), false);
			RunAssertion("MyDefaultSupportGraphEvent", DefaultSupportGraphEventUtil.GetPONOEvents(), true);

			CompileDeploy(_runtime, "@public @buseventtype create json schema MyJsonEvent(MyDouble double, MyInt int, MyString string)");
			RunAssertion("MyJsonEvent", DefaultSupportGraphEventUtil.GetJsonEvents(), true);
		}

		private void RunAssertion(
			string typeName,
			object[] events,
			bool append)
		{
			// test classpath file
			var tempPath = Path.GetTempPath();
			var tempFile = Path.Combine(tempPath, "out_1.csv").Replace("\\", "/");

			try {
				var graph =
					"create dataflow WriteCSV " +
					"DefaultSupportSourceOp -> instream<" + typeName + ">{}" +
					"FileSink(instream) { " +
					"file: '" + tempFile + "', " +
					"append: " + append +
					"}";
				var stmtGraph = CompileDeploy(_runtime, graph).Statements[0];

				var source = new DefaultSupportSourceOp(events);
				var options = new EPDataFlowInstantiationOptions();
				options.OperatorProvider = new DefaultSupportGraphOpProvider(source);
				var instance = _runtime.DataFlowService.Instantiate(stmtGraph.DeploymentId, "WriteCSV", options);
				instance.Run();

				var contents = File.ReadAllLines(tempFile)
					.Where(_ => !string.IsNullOrEmpty(_))
					.ToArray();
				var expected = new string[] {
					"1.1,1,\"one\"",
					"2.2,2,\"two\""
				};

				CollectionAssert.AreEqual(expected, contents);
				//EPAssertionUtil.AssertEqualsExactOrder(expected, contents);
			}
			finally {
				try {
					File.Delete(tempFile);
				}
				catch {
					// ignored
				}
			}


			UndeployAll(_runtime);
		}
	}
} // end of namespace
