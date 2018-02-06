///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapUpdate : RegressionExecution {
        public override void Configure(Configuration configuration) {
            IDictionary<string, Object> type = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"base1", typeof(string)},
                    new object[] {"base2", ExecEventMap.MakeMap(new object[][]{new object[] {"n1", typeof(int)}})}
            });
            configuration.AddEventType("MyEvent", type);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            EPStatement statementOne = epService.EPAdministrator.CreateEPL(
                    "select base1 as v1, base2.n1 as v2, base3? as v3, base2.n2? as v4  from MyEvent");
            EPStatement statementOneSelectAll = epService.EPAdministrator.CreateEPL("select * from MyEvent");
            Assert.AreEqual("[base1, base2]", CompatExtensions.Render(statementOneSelectAll.EventType.PropertyNames));
            var listenerOne = new SupportUpdateListener();
            statementOne.Events += listenerOne.Update;
            string[] fields = "v1,v2,v3,v4".Split(',');
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap(new object[][]{
                    new object[] {"base1", "abc"},
                    new object[] {"base2", ExecEventMap.MakeMap(new object[][]{new object[] {"n1", 10}})}
            }), "MyEvent");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"abc", 10, null, null});
    
            // update type
            IDictionary<string, Object> typeNew = ExecEventMap.MakeMap(new object[][]{
                    new object[] {"base3", typeof(long)},
                    new object[] {"base2", ExecEventMap.MakeMap(new object[][]{new object[] {"n2", typeof(string)}})}
            });
            epService.EPAdministrator.Configuration.UpdateMapEventType("MyEvent", typeNew);
    
            EPStatement statementTwo = epService.EPAdministrator.CreateEPL("select base1 as v1, base2.n1 as v2, base3 as v3, base2.n2 as v4 from MyEvent");
            EPStatement statementTwoSelectAll = epService.EPAdministrator.CreateEPL("select * from MyEvent");
            var listenerTwo = new SupportUpdateListener();
            statementTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(ExecEventMap.MakeMap(new object[][]{
                    new object[] {"base1", "def"},
                    new object[] {"base2", ExecEventMap.MakeMap(new object[][]{new object[] {"n1", 9}, new object[] {"n2", "xyz"}})},
                    new object[] {"base3", 20L},
            }), "MyEvent");
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
                epService.EPAdministrator.Configuration.UpdateMapEventType("dummy", typeNew);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Error updating Map event type: Event type named 'dummy' has not been declared", ex.Message);
            }
    
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            try {
                epService.EPAdministrator.Configuration.UpdateMapEventType("SupportBean", typeNew);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Error updating Map event type: Event type by name 'SupportBean' is not a Map event type", ex.Message);
            }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
