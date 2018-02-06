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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.events.map.ExecEventMap;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.objectarray
{
    public class ExecEventObjectArrayTypeUpdate : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.OBJECTARRAY;
            string[] names = {"base1", "base2"};
            object[] types = {typeof(string), MakeMap(new object[][]{new object[] {"n1", typeof(int)}})};
            configuration.AddEventType("MyOAEvent", names, types);
        }
    
        public override void Run(EPServiceProvider epService) {
            EPStatement statementOne = epService.EPAdministrator.CreateEPL(
                    "select base1 as v1, base2.n1 as v2, base3? as v3, base2.n2? as v4 from MyOAEvent");
            Assert.AreEqual(typeof(object[]), statementOne.EventType.UnderlyingType);
            EPStatement statementOneSelectAll = epService.EPAdministrator.CreateEPL("select * from MyOAEvent");
            Assert.AreEqual("[base1, base2]", CompatExtensions.Render(statementOneSelectAll.EventType.PropertyNames));
            var listenerOne = new SupportUpdateListener();
            statementOne.Events += listenerOne.Update;
            string[] fields = "v1,v2,v3,v4".Split(',');
    
            epService.EPRuntime.SendEvent(new object[]{"abc", MakeMap(new object[][]{new object[] {"n1", 10}}), ""}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"abc", 10, null, null});
    
            // update type
            string[] namesNew = {"base3", "base2"};
            var typesNew = new object[]{typeof(long), MakeMap(new object[][]{new object[] {"n2", typeof(string)}})};
            epService.EPAdministrator.Configuration.UpdateObjectArrayEventType("MyOAEvent", namesNew, typesNew);
    
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL("select base1 as v1, base2.n1 as v2, base3 as v3, base2.n2 as v4 from MyOAEvent");
            EPStatement statementTwoSelectAll = epService.EPAdministrator.CreateEPL("select * from MyOAEvent");
            var listenerTwo = new SupportUpdateListener();
            statementTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(new object[]{"def", MakeMap(new object[][]{new object[] {"n1", 9}, new object[] {"n2", "xyz"}}), 20L}, "MyOAEvent");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"def", 9, 20L, "xyz"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{"def", 9, 20L, "xyz"});
    
            // assert event type
            Assert.AreEqual("[base1, base2, base3]", CompatExtensions.Render(statementOneSelectAll.EventType.PropertyNames));
            Assert.AreEqual("[base1, base2, base3]", CompatExtensions.Render(statementTwoSelectAll.EventType.PropertyNames));
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("base3", typeof(long), null, false, false, false, false, false),
                    new EventPropertyDescriptor("base2", typeof(IDictionary<string, object>), null, false, false, false, true, false),
                    new EventPropertyDescriptor("base1", typeof(string), typeof(char), false, false, true, false, false),
            }, statementTwoSelectAll.EventType.PropertyDescriptors);
    
            try {
                epService.EPAdministrator.Configuration.UpdateObjectArrayEventType("dummy", new string[0], new Object[0]);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Error updating Object-array event type: Event type named 'dummy' has not been declared", ex.Message);
            }
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            try {
                epService.EPAdministrator.Configuration.UpdateObjectArrayEventType("SupportBean", new string[0], new Object[0]);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Error updating Object-array event type: Event type by name 'SupportBean' is not an Object-array event type", ex.Message);
            }
        }
    }
} // end of namespace
