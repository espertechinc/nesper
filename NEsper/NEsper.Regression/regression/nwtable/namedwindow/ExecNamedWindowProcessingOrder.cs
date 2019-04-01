///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowProcessingOrder : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("Event", typeof(SupportBean));
    
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                RunAssertionDispatchBackQueue(epService, rep);
            }
    
            RunAssertionOrderedDeleteAndSelect(epService);
        }
    
        private void RunAssertionDispatchBackQueue(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema StartValueEvent as (dummy string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TestForwardEvent as (prop1 string)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema TestInputEvent as (dummy string)");
            epService.EPAdministrator.CreateEPL("insert into TestForwardEvent select'V1' as prop1 from TestInputEvent");
    
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window NamedWin#unique(prop1) (prop1 string, prop2 string)");
    
            epService.EPAdministrator.CreateEPL("insert into NamedWin select 'V1' as prop1, 'O1' as prop2 from StartValueEvent");
    
            epService.EPAdministrator.CreateEPL("on TestForwardEvent update NamedWin as work set prop2 = 'U1' where work.prop1 = 'V1'");
    
            string[] fields = "prop1,prop2".Split(',');
            string eplSelect = "select irstream prop1, prop2 from NamedWin";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplSelect).Events += listener.Update;
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"dummyValue"}, "StartValueEvent");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "StartValueEvent");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                epService.EPRuntime.SendEventAvro(new GenericRecord(
                    SchemaBuilder.Record("soemthing")), "StartValueEvent");
            } else {
                Assert.Fail();
            }
    
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"V1", "O1"});
    
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"dummyValue"}, "TestInputEvent");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "TestInputEvent");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                epService.EPRuntime.SendEventAvro(new GenericRecord(
                    SchemaBuilder.Record("soemthing")), "TestInputEvent");
            } else {
                Assert.Fail();
            }
    
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[]{"V1", "O1"});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[0], fields, new object[]{"V1", "U1"});
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "StartValueEvent,TestForwardEvent,TestInputEvent,NamedWin".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void RunAssertionOrderedDeleteAndSelect(EPServiceProvider epService) {
            string stmtText;
            stmtText = "create window MyWindow#lastevent as select * from Event";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "insert into MyWindow select * from Event";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "on MyWindow e delete from MyWindow win where win.TheString=e.TheString and e.IntPrimitive = 7";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "on MyWindow e delete from MyWindow win where win.TheString=e.TheString and e.IntPrimitive = 5";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "on MyWindow e insert into ResultStream select e.* from MyWindow";
            epService.EPAdministrator.CreateEPL(stmtText);
    
            stmtText = "select * from ResultStream";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 7));
            Assert.IsFalse(listener.IsInvoked, "E1");
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 8));
            Assert.AreEqual("E2", listener.AssertOneGetNewAndReset().Get("TheString"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            Assert.IsFalse(listener.IsInvoked, "E3");
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 6));
            Assert.AreEqual("E4", listener.AssertOneGetNewAndReset().Get("TheString"));
        }
    }
} // end of namespace
