///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.csv;
using com.espertech.esperio.file;
using com.espertech.esperio.support.util;

using NUnit.Framework;

using static com.espertech.esperio.regression.adapter.TestCSVAdapterUseCases;

namespace com.espertech.esperio.regression.adapter
{
    /// <summary>
    /// Cause all parent class unit tests to be run but sending beans instead of Maps
    /// </summary>
    /// <author>Jerry Shea</author>
    [TestFixture]
    public class TestCSVAdapterUseCasesBean 
    {
        private readonly TestCSVAdapterUseCases _baseUseCase;
        private readonly IContainer _container;
        private EPRuntime _runtime;

        public TestCSVAdapterUseCasesBean()
    	{
	        _container = SupportContainer.Reset();
    	    _baseUseCase = new TestCSVAdapterUseCases(true);
    	}
    
        [Test]
        public void TestReadWritePropsBean()
        {
            var configuration = new Configuration(_container);
            configuration.Common.AddEventType("ExampleMarketDataBeanReadWrite", typeof(ExampleMarketDataBeanReadWrite));
            configuration.Common.AddImportNamespace(typeof(FileSourceCSV));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportCaptureOp));

            var runtimeProvider = new EPRuntimeProvider();
            _runtime = runtimeProvider.GetRuntimeInstance("testExistingTypeNoOptions", configuration);
            _runtime.Initialize();

            var stmt = CompileUtil.CompileDeploy(_runtime, "select * from ExampleMarketDataBeanReadWrite#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var inputAdapter = new CSVInputAdapter(
                _runtime, new AdapterInputSource(_container, CSV_FILENAME_ONELINE_TRADE), "ExampleMarketDataBeanReadWrite");
            inputAdapter.Start();
 
            Assert.AreEqual(1, listener.GetNewDataList().Count);
            var eb = listener.GetNewDataList()[0][0];
            Assert.IsTrue(typeof(ExampleMarketDataBeanReadWrite) == eb.Underlying.GetType());
            Assert.AreEqual(55.5 * 1000, eb.Get("value"));
            
            // test graph
            var graph =
                "create dataflow ReadCSV " +
                "FileSource -> mystream<ExampleMarketDataBeanReadWrite> { file: '" + CSV_FILENAME_ONELINE_TRADE + "', hasTitleLine: true }" +
                "DefaultSupportCaptureOp(mystream) {}";
            var deployment = CompileUtil.CompileDeploy(_runtime, graph);

            var outputOp = new DefaultSupportCaptureOp();
            var instance = _runtime.DataFlowService.Instantiate(
                deployment.DeploymentId,
                "ReadCSV",
                new EPDataFlowInstantiationOptions().WithOperatorProvider(new DefaultSupportGraphOpProvider(outputOp)));
            instance.Run();
            var received = outputOp.GetAndReset()[0].ToArray();
            Assert.That(received.Length, Is.EqualTo(1));
            Assert.That(received[0], Is.InstanceOf<ExampleMarketDataBean>());
            Assert.That(
                ((ExampleMarketDataBeanReadWrite) received[0]).Value,
                Is.EqualTo(55.5 * 1000.0));
        }
    }
}
