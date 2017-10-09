///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;

using NUnit.Framework;

using NEsper.Avro.Extensions;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestEventInfraStaticConfiguration
    {
	    private EPServiceProvider _epService;

        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            var avroSchema = SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME,
                TypeBuilder.Field("intPrimitive", TypeBuilder.Int()));
                
            //SchemaBuilder.Record(SupportEventInfra.AVRO_TYPENAME).Fields()
            //             .Name("intPrimitive").Type().IntType().NoDefault().EndRecord();

	        string avroSchemaText = avroSchema.ToString().Replace("\"", "&quot;");

	        string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	                     "<esper-configuration>\t\n" +
	                     "\t<event-type name=\"MyStaticBean\" class=\"" + typeof(SupportBean).FullName + "\"/>\n" +
                         "\t<event-type name=\"" + SupportEventInfra.MAP_TYPENAME + "\">\n" +
	                     "\t\t<map>\n" +
	                     "\t  \t\t<map-property name=\"intPrimitive\" class=\"int\"/>\n" +
	                     "\t  \t</map>\n" +
	                     "\t</event-type>\n" +
	                     "\t\n" +
                         "\t<event-type name=\"" + SupportEventInfra.OA_TYPENAME + "\">\n" +
	                     "\t\t<objectarray>\n" +
	                     "\t  \t\t<objectarray-property name=\"intPrimitive\" class=\"int\"/>\n" +
	                     "\t  \t</objectarray>\n" +
	                     "\t</event-type>\n" +
                         "\t<event-type name=\"" + SupportEventInfra.XML_TYPENAME + "\">\n" +
	                     "\t\t<xml-dom root-element-name=\"myevent\">\n" +
	                     "\t\t\t<xpath-property property-name=\"intPrimitive\" xpath=\"@intPrimitive\" type=\"number\"/>\n" +
	                     "\t\t</xml-dom>\n" +
	                     "\t</event-type>\n" +
                         "\t<event-type name=\"" + SupportEventInfra.AVRO_TYPENAME + "\">\n" +
	                     "\t\t<avro schema-text=\"" + avroSchemaText + "\"/>\n" +
	                     "\t</event-type>\n" +
	                     "</esper-configuration>\n";
	        configuration.Configure(SupportXML.GetDocument(xml));

	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }

        [Test]
	    public void TestIt() {

	        // Bean
            RunAssertion("MyStaticBean", SupportEventInfra.FBEAN, new SupportBean("E1", 10));

	        // Map
            RunAssertion(SupportEventInfra.MAP_TYPENAME, SupportEventInfra.FMAP, Collections.SingletonMap<string, object>("intPrimitive", 10));

	        // Object-Array
            RunAssertion(SupportEventInfra.OA_TYPENAME, SupportEventInfra.FOA, new object[] { 10 });

	        // XML
            RunAssertion(SupportEventInfra.XML_TYPENAME, SupportEventInfra.FXML, "<myevent intPrimitive=\"10\"/>");

            // Avro
            var schema = SchemaBuilder.Record(
                "somename", TypeBuilder.RequiredInt("intPrimitive"));
                
            // SchemaBuilder.Record("somename").Fields().RequiredInt("intPrimitive").EndRecord();
	        GenericRecord record = new GenericRecord(schema);
	        record.Put("intPrimitive", 10);
            RunAssertion(SupportEventInfra.AVRO_TYPENAME, SupportEventInfra.FAVRO, record);
	    }

	    private void RunAssertion(string typename, FunctionSendEvent fsend, object underlying) {
	        string stmtText = "select intPrimitive from " + typename;
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
	        SupportUpdateListener listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        fsend.Invoke(_epService, underlying);
	        var n = listener.AssertOneGetNewAndReset().Get("intPrimitive").AsInt();
	        Assert.AreEqual(10, n);

	        stmt.Dispose();
	    }
	}
} // end of namespace
