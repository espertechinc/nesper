///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestDocSamples
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestWithDocSamples()
        {
            String epl = "create dataflow HelloWorldDataFlow\n" +
                          "BeaconSource -> helloworldStream { text: 'hello world', iterations : 1 }\n" +
                          "LogSink(helloworldStream) {}";
            _epService.EPAdministrator.CreateEPL(epl);
            
            EPDataFlowInstance instance = _epService.EPRuntime.DataFlowRuntime.Instantiate("HelloWorldDataFlow");
            instance.Run();
    
            TryEpl("create dataflow MyDataFlow\n" +
                    "MyOperatorSimple {}");
            TryEpl("create dataflow MyDataFlow2\n" +
                    "create schema MyEvent as (id string, price double),\n" +
                    "MyOperator(myInStream) -> myOutStream<MyEvent> {\n" +
                    "myParameter : 10\n" +
                    "}");
            TryEpl("create dataflow MyDataFlow3\n" +
                    "MyOperator(myInStream as mis) {}");
            TryEpl("create dataflow MyDataFlow4\n" +
                    "MyOperator(streamOne as one, streamTwo as two) {}");
            TryEpl("create dataflow MyDataFlow5\n" +
                    "MyOperator( (streamA, streamB) as streamsAB) {}");
            TryEpl("create dataflow MyDataFlow6\n" +
                    "MyOperator(abc) -> my.out.stream {}");
            TryEpl("create dataflow MyDataFlow7\n" +
                    "MyOperator -> my.out.one, my.out.two {}");
            TryEpl("create dataflow MyDataFlow8\n" +
                    "create objectarray schema RFIDSchema (tagId string, locX double, locy double),\n" +
                    "MyOperator -> rfid.stream<RFIDSchema> {}");
            TryEpl("create dataflow MyDataFlow9\n" +
                    "create objectarray schema RFIDSchema (tagId string, locX double, locy double),\n" +
                    "MyOperator -> rfid.stream<eventbean<RFIDSchema>> {}");
            TryEpl("create dataflow MyDataFlow10\n" +
                    "MyOperator -> my.stream<eventbean<?>> {}");
            TryEpl("create dataflow MyDataFlow11\n" +
                    "MyOperator {\n" +
                    "stringParam : 'sample',\n" +
                    "secondString : \"double-quotes are fine\",\n" +
                    "intParam : 10\n" +
                    "}");
            TryEpl("create dataflow MyDataFlow12\n" +
                    "MyOperator {\n" +
                    "intParam : 24*60^60,\n" +
                    "threshold : var_threshold, // a variable defined in the engine\n" +
                    "}");
            TryEpl("create dataflow MyDataFlow13\n" +
                    "MyOperator {\n" +
                    "someSystemProperty : SystemProperties('mySystemProperty')\n" +
                    "}");
            TryEpl("create dataflow MyDataFlow14\n" +
                    "MyOperator {\n" +
                    "  myStringArray: ['a', \"b\",],\n" +
                    "  myMapOrObject: {\n" +
                    "    a : 10,\n" +
                    "    b : 'xyz',\n" +
                    "  },\n" +
                    "  myInstance: {\n" +
                    "    class: 'com.myorg.myapp.MyImplementation',\n" +
                    "    myValue : 'sample'\n" +
                    "  }\n" +
                    "}");
        }

        [Test]
        public void TestSODA()
        {

            String soda = "@Name('create dataflow full')\n" +
                    "create dataflow DFFull\n" +
                    "create map schema ABC1 as (col1 int, col2 int),\n" +
                    "create map schema ABC2 as (col1 int, col2 int),\n" +
                    "MyOperatorOne(instream.one) -> outstream.one {}\n" +
                    "MyOperatorTwo(instream.two as IN1, input.three as IN2) -> outstream.one<Test>, outstream.two<EventBean<TestTwo>> {}\n" +
                    "MyOperatorThree((instream.two, input.three) as IN1) {}\n" +
                    "MyOperatorFour -> teststream {}\n" +
                    "MyOperatorFive {\n" +
                    "const_str: \"abc\",\n" +
                    "somevalue: def*2,\n" +
                    "select: (select * from ABC where 1=2),\n" +
                    "jsonarr: [\"a\",\"b\"],\n" +
                    "jsonobj: {a: \"a\",b: \"b\"}\n" +
                    "}\n";
            var model = _epService.EPAdministrator.CompileEPL(soda);
            EPAssertionUtil.AssertEqualsIgnoreNewline(soda, model.ToEPL(new EPStatementFormatter(true)));
            _epService.EPAdministrator.Create(model);
        }
        
        private void TryEpl(String epl) {
            _epService.EPAdministrator.CreateEPL(epl);
        }
        
    }
}
