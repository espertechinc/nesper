///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;

using NUnit.Framework;

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
            //configuration.Common.AddImportNamespace(typeof(FileSourceCSV));
            configuration.Common.AddImportNamespace(typeof(DefaultSupportCaptureOp));

            _runtime = EPRuntimeProvider.GetRuntime("testExistingTypeNoOptions", configuration);
            _runtime.Initialize();

            var stmt = CompileUtil.CompileDeploy(_runtime, "select * from ExampleMarketDataBeanReadWrite#length(100)").Statements[0];
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var inputAdapter = new CSVInputAdapter(
                _runtime,  // _baseUseCase.Runtime, 
                new AdapterInputSource(_container, TestCSVAdapterUseCases.CSV_FILENAME_ONELINE_TRADE), "ReadWrite");
            inputAdapter.Start();
    
            Assert.AreEqual(1, listener.GetNewDataList().Count);
            var eb = listener.GetNewDataList()[0][0];
            Assert.IsTrue(typeof(ExampleMarketDataBeanReadWrite) == eb.Underlying.GetType());
            Assert.AreEqual(55.5 * 1000, eb.Get("value"));
        }
    }
}
