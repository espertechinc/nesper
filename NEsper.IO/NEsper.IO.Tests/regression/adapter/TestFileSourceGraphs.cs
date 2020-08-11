///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.file;

using NUnit.Framework;

using static com.espertech.esperio.support.util.CompileUtil;

namespace com.espertech.esperio.regression.adapter
{
	[TestFixture]
	public class TestFileSourceGraphs : AbstractIOTest
	{
		private EPRuntime runtime;

		[SetUp]
		public void SetUp()
		{
			var configuration = new Configuration();
			configuration.Runtime.Threading.IsInternalTimerEnabled = false;
			configuration.Common.AddImportType(typeof(FileSourceForge));
			configuration.Common.AddImportType(typeof(DefaultSupportCaptureOpForge));

			var propertyTypes = new Dictionary<string, object>();
			propertyTypes.Put("myInt", typeof(int?));
			propertyTypes.Put("myDouble", typeof(double?));
			propertyTypes.Put("myString", typeof(string));
			configuration.Common.AddEventType("MyMapEvent", propertyTypes);

			configuration.Common.AddEventType(
				"MyOAType",
				"p0,p1".SplitCsv(),
				new object[] {typeof(DateTime), typeof(DateTimeEx)});

			runtime = EPRuntimeProvider.GetDefaultRuntime(configuration);
			runtime.Initialize();
		}

		[Test]
		public void TestCSVZipFile()
		{
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyMapEvent> { file: 'regression/noTimestampOne.zip', classpathFile: true, propertyNames: ['myInt','myDouble','myString'], numLoops: 2}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(2, received.Count);
			foreach (var aReceived in received) {
				EPAssertionUtil.AssertPropsPerRow(
					container,
					aReceived.ToArray(),
					"myInt,myDouble,myString".SplitCsv(),
					new object[][] {
						new object[] {1, 1.1, "noTimestampOne.one"},
						new object[] {2, 2.2, "noTimestampOne.two"},
						new object[] {3, 3.3, "noTimestampOne.three"}
					});
			}
		}

		[Test]
		public void TestCSVGraph()
		{
			RunAssertionCSVGraphSchema(EventRepresentationChoice.OBJECTARRAY);
			RunAssertionCSVGraphSchema(EventRepresentationChoice.MAP);
		}

		[Test]
		public void TestPropertyOrderWLoop()
		{
			var graph =
				"create dataflow ReadCSV " +
				"FileSource -> mystream<MyMapEvent> { file: 'regression/noTimestampOne.csv', classpathFile: true, propertyNames: ['myInt','myDouble','myString'], numLoops: 3}" +
				"DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(3, received.Count);
			foreach (var aReceived in received) {
				EPAssertionUtil.AssertPropsPerRow(
					container,
					aReceived.ToArray(),
					"myInt,myDouble,myString".SplitCsv(),
					new object[][] {
						new object[] {1, 1.1, "noTimestampOne.one"},
						new object[] {2, 2.2, "noTimestampOne.two"},
						new object[] {3, 3.3, "noTimestampOne.three"}
					});
			}
		}

		[Test]
		public void TestAdditionalProperties()
		{
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyMapEvent> { file: 'regression/moreProperties.csv', classpathFile: true, hasTitleLine: true}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			EPAssertionUtil.AssertPropsPerRow(
				container,
				received[0].ToArray(),
				"myInt,myDouble,myString".SplitCsv(),
				new object[][] {
					new object[] {1, 1.1, "moreProperties.one"},
					new object[] {2, 2.2, "moreProperties.two"},
					new object[] {3, 3.3, "moreProperties.three"}
				});
		}

		[Test]
		public void TestConflictingPropertyOrderIgnoreTitle()
		{
			CompileDeploy(runtime, "@public @buseventtype create schema MyIntRowEvent (intOne int, intTwo int)");
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyIntRowEvent> { file: 'regression/intsTitleRow.csv', hasHeaderLine:true, classpathFile: true, propertyNames: ['intTwo','intOne'], numLoops: 1}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			EPAssertionUtil.AssertPropsPerRow(
				container,
				received[0].ToArray(),
				"intOne,intTwo".SplitCsv(),
				new object[][] {
					new object[] {0, 1},
					new object[] {0, 2},
					new object[] {0, 3}
				});
		}

		[Test]
		public void TestReorder()
		{
			CompileDeploy(runtime, "@public @buseventtype create schema MyIntRowEvent (p3 string, p1 int, p0 long, p2 double)");
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyIntRowEvent> { file: 'regression/timestampOne.csv', classpathFile: true, propertyNames: ['p0','p1','p2','p3']}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			EPAssertionUtil.AssertPropsPerRow(
				container,
				received[0].ToArray(),
				"p0,p1,p2,p3".SplitCsv(),
				new object[][] {
					new object[] {100L, 1, 1.1, "timestampOne.one"},
					new object[] {300L, 3, 3.3, "timestampOne.three"},
					new object[] {500L, 5, 5.5, "timestampOne.five"}
				});
		}

		[Test]
		public void TestStringPropertyTypes()
		{
			CompileDeploy(runtime, "@public @buseventtype create schema MyStrRowEvent (myInt string, myDouble string, myString string)");

			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyStrRowEvent> { file: 'regression/noTimestampOne.csv', classpathFile: true, propertyNames: [\"myInt\", \"myDouble\", \"myString\"],}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			EPAssertionUtil.AssertPropsPerRow(
				container,
				received[0].ToArray(),
				"myInt,myDouble,myString".SplitCsv(),
				new object[][] {
					new object[] {"1", "1.1", "noTimestampOne.one"},
					new object[] {"2", "2.2", "noTimestampOne.two"},
					new object[] {"3", "3.3", "noTimestampOne.three"}
				});
		}

		[Test]
		public void TestEmptyFile()
		{
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyMapEvent> { file: 'regression/emptyFile.csv', classpathFile: true}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			Assert.IsTrue(received[0].IsEmpty());
		}

		[Test]
		public void TestTitleRowOnlyFile()
		{
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyMapEvent> { file: 'regression/titleRowOnly.csv', classpathFile: true, hasTitleLine: true}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			Assert.IsTrue(received[0].IsEmpty());
		}

		[Test]
		public void TestDateFormat()
		{
			// no date format specified
			var testtime = DateTimeParsingFunctions.ParseDefaultMSec("2012-01-30T08:43:32.116");
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyOAType> { file: 'regression/dateprocessing_one.csv', classpathFile: true, hasTitleLine: false}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			var data = (object[]) received[0][0];
			Assert.AreEqual(testtime, ((DateTime) data[0]).UtcMillis());
			Assert.AreEqual(testtime, ((DateTimeEx) data[1]).UtcMillis);

			// with date format specified
			
			var testTimeOffset = DateTimeOffset.ParseExact(
				"20120320084332000",
				new string[] { "yyyyMMddHHmmssfff" },
				CultureInfo.InvariantCulture,
				DateTimeStyles.AllowWhiteSpaces);

			testtime = testTimeOffset.UtcMillis();

			graph = "create dataflow ReadCSV " +
			        "FileSource -> mystream<MyOAType> { file: 'regression/dateprocessing_two.csv', classpathFile: true, hasTitleLine: false, dateFormat: 'yyyyMMDDHHmmssSSS'}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			data = (object[]) received[0][0];
			Assert.AreEqual(testtime, ((DateTime) data[0]).UtcMillis());
			Assert.AreEqual(testtime, ((DateTimeEx) data[1]).UtcMillis);
		}

		[Test]
		public void TestInvalid()
		{
			string graph;

			// file not found
			graph = "create dataflow FlowOne " +
			        "FileSource -> mystream<MyMapEvent> { file: 'nonExistentFile', classpathFile: true}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			TryInvalidRun(
				"FlowOne",
				graph,
				"Exception encountered opening data flow 'FlowOne' in operator FileSourceCSV: Resource 'nonExistentFile' not found in classpath");

			// has-title-line and actual column names don't match the expected event type (no properties match)
			graph = "create dataflow FlowOne " +
			        "FileSource -> mystream<MyMapEvent> { file: 'regression/differentMap.csv', hasTitleLine:true, classpathFile: true}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			TryInvalidRun(
				"FlowOne",
				graph,
				"Exception encountered running data flow 'FlowOne': Failed to match any of the properties [value one, line one] to the event type properties of event type 'MyMapEvent'");

			// no event type provided
			graph = "create dataflow FlowOne " +
			        "FileSource -> mystream { file: 'nonExistentFile', classpathFile: true}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			TryInvalidCompileGraph(
				runtime,
				graph,
				"Failed to obtain operator 'FileSource': No event type provided for output, please provide an event type name");

			// wrong file format
			graph = "create dataflow FlowOne " +
			        "FileSource -> mystream<MyMapEvent> { file: 'nonExistentFile', classpathFile: true, format: 'dummy',}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			TryInvalid(
				"FlowOne",
				graph,
				"Failed to instantiate data flow 'FlowOne': Failed to obtain operator instance for 'FileSource': Unrecognized file format 'dummy'");

			// where is the input source
			graph = "create dataflow FlowOne " +
			        "FileSource -> mystream<MyMapEvent> {}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			TryInvalid(
				"FlowOne",
				graph,
				"Failed to instantiate data flow 'FlowOne': Failed to obtain operator instance for 'FileSource': Failed to find required parameter, either the file or the adapterInputSource parameter is required");

			// line-format with Map output
			graph = "create dataflow FlowOne " +
			        "FileSource -> mystream<MyMapEvent> {format: 'line', file: 'nonExistentFile'}" +
			        "DefaultSupportCaptureOp(mystream) {}";
			TryInvalid(
				"FlowOne",
				graph,
				"Failed to instantiate data flow 'FlowOne': Failed to obtain operator instance for 'FileSource': Expecting an output event type that has a single property that is of type string, or alternatively specify the 'propertyNameLine' parameter");
		}

		private void TryInvalid(
			string dataflowName,
			string epl,
			string message)
		{
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

		private void TryInvalidRun(
			string dataflowName,
			string epl,
			string message)
		{
			var stmtGraph = CompileDeploy(runtime, epl).Statements[0];
			var outputOp = new DefaultSupportCaptureOp<object>(container.LockManager());
			var df = runtime.DataFlowService.Instantiate(
				stmtGraph.DeploymentId,
				dataflowName,
				new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
			try {
				df.Run();
				Assert.Fail();
			}
			catch (EPDataFlowExecutionException ex) {
				Assert.AreEqual(message, ex.Message);
			}

			UndeployAll(runtime);
		}

		private IList<IList<object>> RunDataFlow(string epl)
		{
			var stmt = CompileDeploy(runtime, epl).Statements[0];

			var outputOp = new DefaultSupportCaptureOp<object>(container.LockManager());
			var instance = runtime.DataFlowService.Instantiate(
				stmt.DeploymentId,
				"ReadCSV",
				new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
			instance.Run();
			UndeployAll(runtime);
			return outputOp.GetAndReset();
		}

		[Test]
		public void TestLoopTitleRow()
		{
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyMapEvent> { file: 'regression/titleRow.csv', classpathFile: true, hasTitleLine:true, numLoops: 3}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(3, received.Count);
			foreach (var aReceived in received) {
				EPAssertionUtil.AssertPropsPerRow(
					container,
					aReceived.ToArray(),
					"myInt,myDouble,myString".SplitCsv(),
					new object[][] {
						new object[] {1, 1.1, "one"},
						new object[] {3, 3.3, "three"},
						new object[] {5, 5.5, "five"}
					});
			}
		}

		[Test]
		public void TestCommentAndOtherProp()
		{
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyMapEvent> { file: 'regression/comments.csv', classpathFile: true, propertyNames: ['other', 'myInt','myDouble','myString']}" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var received = RunDataFlow(graph);
			Assert.AreEqual(1, received.Count);
			EPAssertionUtil.AssertPropsPerRow(
				container,
				received[0].ToArray(),
				"myInt,myDouble,myString".SplitCsv(),
				new object[][] {
					new object[] {1, 1.1, "one"},
					new object[] {3, 3.3, "three"},
					new object[] {5, 5.5, "five"}
				});
		}

		private void RunAssertionCSVGraphSchema(EventRepresentationChoice representationEnum)
		{

			var fields = "myString,myInt,timestamp, myDouble".SplitCsv();
			CompileDeploy(
				runtime,
				representationEnum.GetAnnotationText() +
				" @public @buseventtype create schema MyEvent(myString string, myInt int, timestamp long, myDouble double)");
			var graph = "create dataflow ReadCSV " +
			            "FileSource -> mystream<MyEvent> { file: 'regression/titleRow.csv', hasHeaderLine: true, classpathFile: true }" +
			            "DefaultSupportCaptureOp(mystream) {}";
			var deployment = CompileDeploy(runtime, graph);

			var outputOp = new DefaultSupportCaptureOp();
			var instance = runtime.DataFlowService.Instantiate(
				deployment.DeploymentId,
				"ReadCSV",
				new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
			instance.Run();
			var received = outputOp.GetAndReset();
			Assert.AreEqual(1, received.Count);
			EPAssertionUtil.AssertPropsPerRow(
				container,
				received[0].ToArray(),
				fields,
				new object[][] {
					new object[] {"one", 1, 100L, 1.1},
					new object[] {"three", 3, 300L, 3.3},
					new object[] {"five", 5, 500L, 5.5}
				});
			Assert.IsTrue(representationEnum.MatchesClass(received[0].ToArray()[0].GetType()));

			UndeployAll(runtime);
		}

		public class MyArgCtorClass
		{
			private readonly string _arg;

			public MyArgCtorClass(string arg)
			{
				this._arg = arg;
			}
		}
	}
} // end of namespace
