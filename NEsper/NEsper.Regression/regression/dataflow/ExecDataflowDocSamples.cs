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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.dataflow
{
    public class ExecDataflowDocSamples : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionDocSamples(epService);
            RunAssertionSODA(epService);
        }
    
        private void RunAssertionDocSamples(EPServiceProvider epService) {
            string epl = "create dataflow HelloWorldDataFlow\n" +
                    "BeaconSource -> helloworldStream { text: 'hello world', iterations : 1 }\n" +
                    "LogSink(helloworldStream) {}";
            epService.EPAdministrator.CreateEPL(epl);
    
            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("HelloWorldDataFlow");
            instance.Run();
    
            TryEpl(epService, "create dataflow MyDataFlow\n" +
                    "MyOperatorSimple {}");
            TryEpl(epService, "create dataflow MyDataFlow2\n" +
                    "create schema MyEvent as (id string, price double),\n" +
                    "MyOperator(myInStream) -> myOutStream<MyEvent> {\n" +
                    "myParameter : 10\n" +
                    "}");
            TryEpl(epService, "create dataflow MyDataFlow3\n" +
                    "MyOperator(myInStream as mis) {}");
            TryEpl(epService, "create dataflow MyDataFlow4\n" +
                    "MyOperator(streamOne as one, streamTwo as two) {}");
            TryEpl(epService, "create dataflow MyDataFlow5\n" +
                    "MyOperator( (streamA, streamB) as streamsAB) {}");
            TryEpl(epService, "create dataflow MyDataFlow6\n" +
                    "MyOperator(abc) -> my.out.stream {}");
            TryEpl(epService, "create dataflow MyDataFlow7\n" +
                    "MyOperator -> my.out.one, my.out.two {}");
            TryEpl(epService, "create dataflow MyDataFlow8\n" +
                    "create objectarray schema RFIDSchema (tagId string, locX double, locy double),\n" +
                    "MyOperator -> rfid.stream<RFIDSchema> {}");
            TryEpl(epService, "create dataflow MyDataFlow9\n" +
                    "create objectarray schema RFIDSchema (tagId string, locX double, locy double),\n" +
                    "MyOperator -> rfid.stream<eventbean<RFIDSchema>> {}");
            TryEpl(epService, "create dataflow MyDataFlow10\n" +
                    "MyOperator -> my.stream<eventbean<?>> {}");
            TryEpl(epService, "create dataflow MyDataFlow11\n" +
                    "MyOperator {\n" +
                    "stringParam : 'sample',\n" +
                    "secondString : \"double-quotes are fine\",\n" +
                    "intParam : 10\n" +
                    "}");
            TryEpl(epService, "create dataflow MyDataFlow12\n" +
                    "MyOperator {\n" +
                    "intParam : 24*60^60,\n" +
                    "threshold : var_threshold, // a variable defined in the engine\n" +
                    "}");
            TryEpl(epService, "create dataflow MyDataFlow13\n" +
                    "MyOperator {\n" +
                    "someSystemProperty : SystemProperties('mySystemProperty')\n" +
                    "}");
            TryEpl(epService, "create dataflow MyDataFlow14\n" +
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
    
        private void RunAssertionSODA(EPServiceProvider epService) {
    
            string soda = "@Name('create dataflow full')\n" +
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
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(soda);
            EPAssertionUtil.AssertEqualsIgnoreNewline(soda, model.ToEPL(new EPStatementFormatter(true)));
            epService.EPAdministrator.Create(model);
        }
    
        private void TryEpl(EPServiceProvider epService, string epl) {
            epService.EPAdministrator.CreateEPL(epl);
        }
    
    }
} // end of namespace
