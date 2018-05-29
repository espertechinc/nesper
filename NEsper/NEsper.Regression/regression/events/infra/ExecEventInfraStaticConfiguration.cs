///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Avro;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using NEsper.Avro.Extensions;
using static com.espertech.esper.supportregression.events.SupportEventInfra;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.infra
{
    public class ExecEventInfraStaticConfiguration : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            var avroSchema = SchemaBuilder.Record(AVRO_TYPENAME,
                    TypeBuilder.Field("IntPrimitive", TypeBuilder.IntType()));

            string avroSchemaText = avroSchema.ToString().Replace("\"", "&quot;");
    
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<esper-configuration>\t\n" +
                    "\t<event-type name=\"MyStaticBean\" class=\"" + typeof(SupportBean).FullName + "\"/>\n" +
                    "\t<event-type name=\"" + MAP_TYPENAME + "\">\n" +
                    "\t\t<map>\n" +
                    "\t  \t\t<map-property name=\"IntPrimitive\" class=\"int\"/>\n" +
                    "\t  \t</map>\n" +
                    "\t</event-type>\n" +
                    "\t\n" +
                    "\t<event-type name=\"" + OA_TYPENAME + "\">\n" +
                    "\t\t<objectarray>\n" +
                    "\t  \t\t<objectarray-property name=\"IntPrimitive\" class=\"int\"/>\n" +
                    "\t  \t</objectarray>\n" +
                    "\t</event-type>\n" +
                    "\t<event-type name=\"" + XML_TYPENAME + "\">\n" +
                    "\t\t<xml-dom root-element-name=\"myevent\">\n" +
                    "\t\t\t<xpath-property property-name=\"IntPrimitive\" xpath=\"@IntPrimitive\" type=\"number\"/>\n" +
                    "\t\t</xml-dom>\n" +
                    "\t</event-type>\n" +
                    "\t<event-type name=\"" + AVRO_TYPENAME + "\">\n" +
                    "\t\t<avro schema-text=\"" + avroSchemaText + "\"/>\n" +
                    "\t</event-type>\n" +
                    "</esper-configuration>\n";
            configuration.Configure(SupportXML.GetDocument(xml));
        }
    
        public override void Run(EPServiceProvider epService) {
    
            // Bean
            RunAssertion(epService, "MyStaticBean", FBEAN, new SupportBean("E1", 10));
    
            // Map
            RunAssertion(epService, MAP_TYPENAME, FMAP, Collections.SingletonMap<string, object>("IntPrimitive", 10));
    
            // Object-Array
            RunAssertion(epService, OA_TYPENAME, FOA, new object[]{10});
    
            // XML
            RunAssertion(epService, XML_TYPENAME, FXML, "<myevent IntPrimitive=\"10\"/>");
    
            // Avro
            var schema = SchemaBuilder.Record("somename", TypeBuilder.RequiredInt("IntPrimitive"));
            var record = new GenericRecord(schema);
            record.Put("IntPrimitive", 10);
            RunAssertion(epService, AVRO_TYPENAME, FAVRO, record);
        }
    
        private void RunAssertion(EPServiceProvider epService, string typename, FunctionSendEvent fsend, Object underlying) {
    
            string stmtText = "select IntPrimitive from " + typename;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            fsend.Invoke(epService, underlying);
            var n = listener.AssertOneGetNewAndReset().Get("IntPrimitive").AsInt();
            Assert.AreEqual(10, n);
    
            stmt.Dispose();
        }
    }
} // end of namespace
