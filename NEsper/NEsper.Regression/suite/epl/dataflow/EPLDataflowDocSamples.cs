///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.dataflow
{
    public class EPLDataflowDocSamples
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new EPLDataflowDocSamplesRun());
            execs.Add(new EPLDataflowSODA());
            return execs;
        }

        private static void TryEpl(
            RegressionEnvironment env,
            string epl)
        {
            try {
                env.Compiler.ParseModule(epl);
            }
            catch (Exception t) {
                Assert.Fail(t.Message);
            }
        }

        internal class EPLDataflowDocSamplesRun : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('flow') create dataflow HelloWorldDataFlow\n" +
                          "BeaconSource -> helloworldStream { text: 'hello world', iterations : 1 }\n" +
                          "LogSink(helloworldStream) {}";
                env.CompileDeploy(epl);

                var instance = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("flow"), "HelloWorldDataFlow");
                instance.Run();

                TryEpl(
                    env,
                    "create dataflow MyDataFlow\n" +
                    "MyOperator {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow2\n" +
                    "create schema MyEvent as (Id string, Price double),\n" +
                    "MyOperator -> myOutStream<MyEvent> {\n" +
                    "myParameter : 10\n" +
                    "}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow3\n" +
                    "MyOperator(myInStream as mis) {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow4\n" +
                    "MyOperator(streamOne as one, streamTwo as two) {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow5\n" +
                    "MyOperator( (streamA, streamB) as streamsAB) {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow6\n" +
                    "MyOperator(abc) -> my.out.stream {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow7\n" +
                    "MyOperator -> my.out.one, my.out.two {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow8\n" +
                    "create objectarray schema RFIDSchema (tagId string, locX double, locy double),\n" +
                    "MyOperator -> rfid.stream<RFIDSchema> {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow9\n" +
                    "create objectarray schema RFIDSchema (tagId string, locX double, locy double),\n" +
                    "MyOperator -> rfid.stream<eventbean<RFIDSchema>> {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow10\n" +
                    "MyOperator -> my.stream<eventbean<?>> {}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow11\n" +
                    "MyOperator {\n" +
                    "stringParam : 'sample',\n" +
                    "secondString : \"double-quotes are fine\",\n" +
                    "intParam : 10\n" +
                    "}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow12\n" +
                    "MyOperator {\n" +
                    "intParam : 24*60^60,\n" +
                    "threshold : var_threshold, // a variable defined in the runtime\n" +
                    "}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow13\n" +
                    "MyOperator {\n" +
                    "someSystemProperty : systemProperties('mySystemProperty')\n" +
                    "}");
                TryEpl(
                    env,
                    "create dataflow MyDataFlow14\n" +
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
        }

        internal class EPLDataflowSODA : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var soda = "@Name('create dataflow full')\n" +
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
                var model = env.EplToModel(soda);
                EPAssertionUtil.AssertEqualsIgnoreNewline(soda, model.ToEPL(new EPStatementFormatter(true)));
            }
        }
    }
} // end of namespace