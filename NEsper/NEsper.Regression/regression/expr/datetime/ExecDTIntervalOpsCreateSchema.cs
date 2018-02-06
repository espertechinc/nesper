///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml.XPath;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;
using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTIntervalOpsCreateSchema : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCreateSchema(epService);
        }
    
        private void RunAssertionCreateSchema(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionCreateSchema(epService, rep);
            }
        }
    
        private void TryAssertionCreateSchema(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            string startA = "2002-05-30T09:00:00.000";
            string endA = "2002-05-30T09:00:01.000";
            string startB = "2002-05-30T09:00:00.500";
            string endB = "2002-05-30T09:00:00.700";
    
            // test Map type long-type timestamps
            RunAssertionCreateSchemaWTypes(epService, eventRepresentationEnum, "long",
                    DateTimeParser.ParseDefaultMSec(startA), DateTimeParser.ParseDefaultMSec(endA),
                    DateTimeParser.ParseDefaultMSec(startB), DateTimeParser.ParseDefaultMSec(endB));
    
            // test Map type DateTimeEx-type timestamps
            if (!eventRepresentationEnum.IsAvroEvent())
            {
                RunAssertionCreateSchemaWTypes(
                    epService, eventRepresentationEnum, typeof(DateTimeEx).FullName,
                    DateTimeParser.ParseDefaultEx(startA), DateTimeParser.ParseDefaultEx(endA),
                    DateTimeParser.ParseDefaultEx(startB), DateTimeParser.ParseDefaultEx(endB));
            }

            // test Map type DateTimeEx-type timestamps
            if (!eventRepresentationEnum.IsAvroEvent())
            {
                RunAssertionCreateSchemaWTypes(epService, eventRepresentationEnum, typeof(DateTimeOffset).FullName,
                    DateTimeParser.ParseDefaultDate(startA), DateTimeParser.ParseDefaultDate(endA),
                    DateTimeParser.ParseDefaultDate(startB), DateTimeParser.ParseDefaultDate(endB));
            }
    
            // test Bean-type Date-type timestamps
            string epl = eventRepresentationEnum.GetAnnotationText() + " create schema SupportBean as " + typeof(SupportBean).FullName + " starttimestamp LongPrimitive endtimestamp LongBoxed";
            epService.EPAdministrator.CreateEPL(epl);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select a.Get('month') as val0 from SupportBean a");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = DateTimeParser.ParseDefaultMSec(startA);
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(5, listener.AssertOneGetNewAndReset().Get("val0"));
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl.Trim(), model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(epl.Trim(), stmt.Text);
    
            // try XML
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.RootElementName = "ABC";
            desc.StartTimestampPropertyName = "mystarttimestamp";
            desc.EndTimestampPropertyName = "myendtimestamp";
            desc.AddXPathProperty("mystarttimestamp", "/test/prop", XPathResultType.Number);
            try {
                epService.EPAdministrator.Configuration.AddEventType("TypeXML", desc);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Declared start timestamp property 'mystarttimestamp' is expected to return a DateTime, DateTimeEx or long-typed value but returns '" + Name.Clean<double>() + "'", ex.Message);
            }
        }
    
        private void RunAssertionCreateSchemaWTypes(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string typeOfDatetimeProp, Object startA, Object endA, Object startB, Object endB) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeA as (startts " + typeOfDatetimeProp + ", endts " + typeOfDatetimeProp + ") starttimestamp startts endtimestamp endts");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TypeB as (startts " + typeOfDatetimeProp + ", endts " + typeOfDatetimeProp + ") starttimestamp startts endtimestamp endts");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select a.includes(b) as val0 from TypeA#lastevent as a, TypeB#lastevent as b");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            MakeSendEvent(epService, "TypeA", eventRepresentationEnum, startA, endA);
            MakeSendEvent(epService, "TypeB", eventRepresentationEnum, startB, endB);
            Assert.AreEqual(true, listener.AssertOneGetNewAndReset().Get("val0"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("TypeA", true);
            epService.EPAdministrator.Configuration.RemoveEventType("TypeB", true);
        }
    
        private void MakeSendEvent(EPServiceProvider epService, string typeName, EventRepresentationChoice eventRepresentationEnum, Object startTs, Object endTs) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{startTs, endTs}, typeName);
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("startts", startTs);
                theEvent.Put("endts", endTs);
                epService.EPRuntime.SendEvent(theEvent, typeName);
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, typeName).AsRecordSchema());
                record.Put("startts", startTs);
                record.Put("endts", endTs);
                epService.EPRuntime.SendEventAvro(record, typeName);
            } else {
                throw new IllegalStateException("Unrecognized enum " + eventRepresentationEnum);
            }
        }
    }
} // end of namespace
