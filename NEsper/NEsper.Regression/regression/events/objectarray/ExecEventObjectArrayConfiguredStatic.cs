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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regression.events.map;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.objectarray
{
    using Map = IDictionary<string, object>;

    public class ExecEventObjectArrayConfiguredStatic : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.OBJECTARRAY;
            configuration.AddEventType("MyOAType", "bean,theString,map".Split(','), new object[]{typeof(SupportBean).FullName, "string", "Map"});
        }
    
        public override void Run(EPServiceProvider epService) {
            var eventType = epService.EPAdministrator.Configuration.GetEventType("MyOAType");
            Assert.AreEqual(typeof(object[]), eventType.UnderlyingType);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("theString"));
            Assert.AreEqual(typeof(Map), eventType.GetPropertyType("map"));
            Assert.AreEqual(typeof(SupportBean), eventType.GetPropertyType("bean"));
    
            var stmt = epService.EPAdministrator.CreateEPL("select bean, theString, Map('key'), bean.theString from MyOAType");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(object[]), stmt.EventType.UnderlyingType);
    
            var bean = new SupportBean("E1", 1);
            epService.EPRuntime.SendEvent(new object[]{bean, "abc", Collections.SingletonMap("key", "value")}, "MyOAType");
            EPAssertionUtil.AssertProps(listener.AssertOneGetNew(), "bean,theString,Map('key'),bean.theString".Split(','), new object[]{bean, "abc", "value", "E1"});
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
