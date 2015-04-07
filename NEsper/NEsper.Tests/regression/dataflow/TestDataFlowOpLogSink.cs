///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat.collections;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.dataflow
{
    [TestFixture]
    public class TestDataFlowOpLogSink
    {

        private EPServiceProvider epService;

        [SetUp]
        public void SetUp()
        {
            epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            epService.Initialize();
        }

        [Test]
        public void TestConsoleOp()
        {
            RunAssertion(null, null, null, null, null);
            RunAssertion("summary", true, null, null, null);
            RunAssertion("xml", true, null, null, null);
            RunAssertion("json", true, null, null, null);
            RunAssertion("summary", false, null, null, null);
            RunAssertion("summary", true, "dataflow:%df port:%p instanceId:%i title:%t event:%e", "mytitle", null);
            RunAssertion("xml", true, null, null, false);
            RunAssertion("json", true, null, "JSON_HERE", true);

            // invalid: output stream
            SupportDataFlowAssertionUtil.TryInvalidInstantiate(epService, "DF1", "create dataflow DF1 LogSink -> s1 {}",
                    "Failed to instantiate data flow 'DF1': Failed initialization for operator 'LogSink': LogSink operator does not provide an output stream");

            String docSmple = "create dataflow MyDataFlow\n" +
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
            epService.EPAdministrator.CreateEPL(docSmple);
            epService.EPRuntime.DataFlowRuntime.Instantiate("MyDataFlow");
        }

        private void RunAssertion(String format, bool? log, String layout, String title, bool? linefeed)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));

            String graph = "create dataflow MyConsoleOut\n" +
                    "Emitter -> instream<SupportBean>{name : 'e1'}\n" +
                    "LogSink(instream) {\n" +
                    (format == null ? "" : "  format: '" + format + "',\n") +
                    (log == null ? "" : "  log: " + log + ",\n") +
                    (layout == null ? "" : "  layout: '" + layout + "',\n") +
                    (title == null ? "" : "  title: '" + title + "',\n") +
                    (linefeed == null ? "" : "  linefeed: " + linefeed + ",\n") +
                    "}";
            EPStatement stmtGraph = epService.EPAdministrator.CreateEPL(graph);

            EPDataFlowInstance instance = epService.EPRuntime.DataFlowRuntime.Instantiate("MyConsoleOut");

            Emitter emitter = instance.StartCaptive().Emitters.Get("e1");
            emitter.Submit(new SupportBean("E1", 1));

            instance.Cancel();
            stmtGraph.Dispose();
        }
    }
}
