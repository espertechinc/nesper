///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.objectarray
{
    public class ExecEventObjectArrayNestedMap : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            EventAdapterService eventAdapterService = ((EPServiceProviderSPI) epService).EventAdapterService;
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EventType supportBeanType = epService.EPAdministrator.Configuration.GetEventType("SupportBean");
    
            var lev2def = new Dictionary<string, Object>();
            lev2def.Put("sb", "SupportBean");
            var lev1def = new Dictionary<string, Object>();
            lev1def.Put("lev1name", lev2def);
            epService.EPAdministrator.Configuration.AddEventType("MyMapNestedObjectArray", new string[]{"lev0name"}, new object[]{lev1def});
            Assert.AreEqual(typeof(object[]), epService.EPAdministrator.Configuration.GetEventType("MyMapNestedObjectArray").UnderlyingType);
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select lev0name.lev1name.sb.TheString as val from MyMapNestedObjectArray").Events += listener.Update;
    
            var lev2data = new Dictionary<string, Object>();
            lev2data.Put("sb", eventAdapterService.AdapterForTypedObject(new SupportBean("E1", 0), supportBeanType));
            var lev1data = new Dictionary<string, Object>();
            lev1data.Put("lev1name", lev2data);
    
            epService.EPRuntime.SendEvent(new object[]{lev1data}, "MyMapNestedObjectArray");
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("val"));
    
            try {
                epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyMapNestedObjectArray");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Event type named 'MyMapNestedObjectArray' has not been defined or is not a Map event type, the name 'MyMapNestedObjectArray' refers to a System.Object[] event type", ex.Message);
            }
        }
    }
} // end of namespace
