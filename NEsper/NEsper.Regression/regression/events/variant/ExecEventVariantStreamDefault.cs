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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.variant
{
    public class ExecEventVariantStreamDefault : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanVariantStream));
            epService.EPAdministrator.Configuration.AddImport(GetType().FullName);
    
            RunAssertionSingleColumnConversion(epService);
            RunAssertionCoercionBoxedTypeMatch(epService);
            RunAssertionSuperTypesInterfaces(epService);
            RunAssertionNamedWin(epService);
            RunAssertionPatternSubquery(epService);
            RunAssertionDynamicMapType(epService);
            RunAssertionInvalidInsertInto(epService);
            RunAssertionInvalidConfig(epService);
        }
    
        private void RunAssertionSingleColumnConversion(EPServiceProvider epService) {
    
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBean");
            variant.AddEventTypeName("SupportBeanVariantStream");
            epService.EPAdministrator.Configuration.AddVariantStream("AllEvents", variant);
    
            epService.EPAdministrator.CreateEPL("insert into AllEvents select * from SupportBean");
            epService.EPAdministrator.CreateEPL("create window MainEventWindow#length(10000) as AllEvents");
            epService.EPAdministrator.CreateEPL("insert into MainEventWindow select " + GetType().Name + ".PreProcessEvent(event) from AllEvents as event");
    
            EPStatement statement = epService.EPAdministrator.CreateEPL("select * from MainEventWindow where TheString = 'E'");
            statement.AddEventHandlerWithReplay((new SupportUpdateListener()).Update);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public static Object PreProcessEvent(Object o) {
            return new SupportBean("E2", 0);
        }
    
        private void RunAssertionCoercionBoxedTypeMatch(EPServiceProvider epService) {
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBean");
            variant.AddEventTypeName("SupportBeanVariantStream");
            epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStreamOne", variant);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyVariantStreamOne");
            var listenerOne = new SupportUpdateListener();
            stmt.Events += listenerOne.Update;
            EventType typeSelectAll = stmt.EventType;
            AssertEventTypeDefault(typeSelectAll);
            Assert.AreEqual(typeof(Object), stmt.EventType.UnderlyingType);
    
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamOne select * from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamOne select * from SupportBeanVariantStream");
    
            // try wildcard
            var eventOne = new SupportBean("E0", -1);
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, listenerOne.AssertOneGetNewAndReset().Underlying);
    
            var eventTwo = new SupportBeanVariantStream("E1");
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, listenerOne.AssertOneGetNewAndReset().Underlying);
    
            stmt.Dispose();
            string fields = "TheString,BoolBoxed,IntPrimitive,LongPrimitive,DoublePrimitive,EnumValue";
            stmt = epService.EPAdministrator.CreateEPL("select " + fields + " from MyVariantStreamOne");
            stmt.Events += listenerOne.Update;
            AssertEventTypeDefault(stmt.EventType);
    
            // coerces to the higher resolution type, accepts boxed versus not boxed
            epService.EPRuntime.SendEvent(new SupportBeanVariantStream("s1", true, 1, 20, 30, SupportEnum.ENUM_VALUE_1));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new object[]{"s1", true, 1, 20L, 30d, SupportEnum.ENUM_VALUE_1});
    
            var bean = new SupportBean("s2", 99);
            bean.LongPrimitive = 33;
            bean.DoublePrimitive = 50;
            bean.EnumValue = SupportEnum.ENUM_VALUE_3;
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new object[]{"s2", null, 99, 33L, 50d, SupportEnum.ENUM_VALUE_3});
    
            // make sure a property is not known since the property is not found on SupportBeanVariantStream
            try {
                epService.EPAdministrator.CreateEPL("select charBoxed from MyVariantStreamOne");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'charBoxed': Property named 'charBoxed' is not valid in any stream [select charBoxed from MyVariantStreamOne]", ex.Message);
            }
    
            // try dynamic property: should Exists but not show up as a declared property
            stmt.Dispose();
            fields = "v1,v2,v3";
            stmt = epService.EPAdministrator.CreateEPL("select LongBoxed? as v1,charBoxed? as v2,DoubleBoxed? as v3 from MyVariantStreamOne");
            stmt.Events += listenerOne.Update;
            AssertEventTypeDefault(typeSelectAll);  // asserts prior "select *" event type
    
            bean = new SupportBean();
            bean.LongBoxed = 33L;
            bean.CharBoxed = 'a';
            bean.DoubleBoxed = Double.NaN;
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new object[]{33L, 'a', Double.NaN});
    
            epService.EPRuntime.SendEvent(new SupportBeanVariantStream("s2"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields.Split(','), new object[]{null, null, null});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSuperTypesInterfaces(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantOne", typeof(SupportBeanVariantOne));
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantTwo", typeof(SupportBeanVariantTwo));
    
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBeanVariantOne");
            variant.AddEventTypeName("SupportBeanVariantTwo");
            epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStreamTwo", variant);
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamTwo select * from SupportBeanVariantOne");
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamTwo select * from SupportBeanVariantTwo");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyVariantStreamTwo");
            var listenerOne = new SupportUpdateListener();
            stmt.Events += listenerOne.Update;
            EventType eventType = stmt.EventType;
    
            string[] expected = "P0,P1,P2,P3,P4,P5,Indexed,Mapped,Inneritem".Split(',');
            string[] propertyNames = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
            Assert.AreEqual(typeof(ISupportBaseAB), eventType.GetPropertyType("P0"));
            Assert.AreEqual(typeof(ISupportAImplSuperG), eventType.GetPropertyType("P1"));
            Assert.AreEqual(typeof(object), eventType.GetPropertyType("P2"));
            Assert.AreEqual(typeof(IList<object>), eventType.GetPropertyType("P3"));
            Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("P4"));
            Assert.AreEqual(typeof(ICollection<object>), eventType.GetPropertyType("P5"));
            Assert.AreEqual(typeof(int[]), eventType.GetPropertyType("Indexed"));
            Assert.AreEqual(typeof(IDictionary<string, string>), eventType.GetPropertyType("Mapped"));
            Assert.AreEqual(typeof(SupportBeanVariantOne.SupportBeanVariantOneInner), eventType.GetPropertyType("Inneritem"));
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select P0,P1,P2,P3,P4,P5,Indexed[0] as P6,IndexArr[1] as P7,MappedKey('a') as P8,inneritem as P9,inneritem.val as P10 from MyVariantStreamTwo");
            stmt.Events += listenerOne.Update;
            eventType = stmt.EventType;
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("P6"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("P7"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("P8"));
            Assert.AreEqual(typeof(SupportBeanVariantOne.SupportBeanVariantOneInner), eventType.GetPropertyType("P9"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("P10"));
    
            var ev1 = new SupportBeanVariantOne();
            epService.EPRuntime.SendEvent(ev1);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "P6,P7,P8,P9,P10".Split(','), new object[]{1, 2, "val1", ev1.Inneritem, ev1.Inneritem.Val});
    
            var ev2 = new SupportBeanVariantTwo();
            epService.EPRuntime.SendEvent(ev2);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "P6,P7,P8,P9,P10".Split(','), new object[]{10, 20, "val2", ev2.Inneritem, ev2.Inneritem.Val});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertEventTypeDefault(EventType eventType) {
            string[] expected = "TheString,BoolBoxed,IntPrimitive,LongPrimitive,DoublePrimitive,EnumValue".Split(',');
            string[] propertyNames = eventType.PropertyNames;
            EPAssertionUtil.AssertEqualsAnyOrder(expected, propertyNames);
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("BoolBoxed"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("IntPrimitive"));
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("LongPrimitive"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("DoublePrimitive"));
            Assert.AreEqual(typeof(SupportEnum?), eventType.GetPropertyType("EnumValue"));
            foreach (string expectedProp in expected) {
                Assert.IsNotNull(eventType.GetGetter(expectedProp));
                Assert.IsTrue(eventType.IsProperty(expectedProp));
            }
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("TheString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("BoolBoxed", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("IntPrimitive", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("LongPrimitive", typeof(long?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("DoublePrimitive", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("EnumValue", typeof(SupportEnum?), null, false, false, false, false, false),
            }, eventType.PropertyDescriptors);
        }
    
        private void RunAssertionNamedWin(EPServiceProvider epService) {
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBeanVariantStream");
            variant.AddEventTypeName("SupportBean");
            epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStreamThree", variant);
    
            // test named window
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyVariantWindow#unique(TheString) as select * from MyVariantStreamThree");
            var listenerOne = new SupportUpdateListener();
            stmt.Events += listenerOne.Update;
            epService.EPAdministrator.CreateEPL("insert into MyVariantWindow select * from MyVariantStreamThree");
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamThree select * from SupportBeanVariantStream");
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamThree select * from SupportBean");
    
            var eventOne = new SupportBean("E1", -1);
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, listenerOne.AssertOneGetNewAndReset().Underlying);
    
            var eventTwo = new SupportBeanVariantStream("E2");
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, listenerOne.AssertOneGetNewAndReset().Underlying);
    
            var eventThree = new SupportBean("E2", -1);
            epService.EPRuntime.SendEvent(eventThree);
            Assert.AreSame(eventThree, listenerOne.LastNewData[0].Underlying);
            Assert.AreSame(eventTwo, listenerOne.LastOldData[0].Underlying);
            listenerOne.Reset();
    
            var eventFour = new SupportBeanVariantStream("E1");
            epService.EPRuntime.SendEvent(eventFour);
            Assert.AreSame(eventFour, listenerOne.LastNewData[0].Underlying);
            Assert.AreSame(eventOne, listenerOne.LastOldData[0].Underlying);
            listenerOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPatternSubquery(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBeanVariantStream");
            variant.AddEventTypeName("SupportBean");
            epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStreamFour", variant);
    
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamFour select * from SupportBeanVariantStream");
            epService.EPAdministrator.CreateEPL("insert into MyVariantStreamFour select * from SupportBean");
    
            // test pattern
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from pattern [a=MyVariantStreamFour -> b=MyVariantStreamFour]");
            var listenerOne = new SupportUpdateListener();
            stmt.Events += listenerOne.Update;
            object[] events = {new SupportBean("E1", -1), new SupportBeanVariantStream("E2")};
            epService.EPRuntime.SendEvent(events[0]);
            epService.EPRuntime.SendEvent(events[1]);
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), "a,b".Split(','), events);
    
            // test subquery
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean_A as a where Exists(select * from MyVariantStreamFour#lastevent as b where b.TheString=a.id)");
            stmt.Events += listenerOne.Update;
            events = new object[]{new SupportBean("E1", -1), new SupportBeanVariantStream("E2"), new SupportBean_A("E2")};
    
            epService.EPRuntime.SendEvent(events[0]);
            epService.EPRuntime.SendEvent(events[2]);
            Assert.IsFalse(listenerOne.IsInvoked);
    
            epService.EPRuntime.SendEvent(events[1]);
            epService.EPRuntime.SendEvent(events[2]);
            Assert.IsTrue(listenerOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDynamicMapType(EPServiceProvider epService) {
            var types = new Dictionary<string, Object>();
            types.Put("someprop", typeof(string));
    
            epService.EPAdministrator.Configuration.AddEventType("MyEvent", types);
            epService.EPAdministrator.Configuration.AddEventType("MySecondEvent", types);
    
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("MyEvent");
            variant.AddEventTypeName("MySecondEvent");
            epService.EPAdministrator.Configuration.AddVariantStream("MyVariant", variant);
    
            epService.EPAdministrator.CreateEPL("insert into MyVariant select * from MyEvent");
            epService.EPAdministrator.CreateEPL("insert into MyVariant select * from MySecondEvent");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyVariant");
            var listenerOne = new SupportUpdateListener();
            stmt.Events += listenerOne.Update;
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MyEvent");
            Assert.IsNotNull(listenerOne.AssertOneGetNewAndReset());
            epService.EPRuntime.SendEvent(new Dictionary<string, object>(), "MySecondEvent");
            Assert.IsNotNull(listenerOne.AssertOneGetNewAndReset());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalidInsertInto(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanVariantStream", typeof(SupportBeanVariantStream));
    
            var variant = new ConfigurationVariantStream();
            variant.AddEventTypeName("SupportBean");
            variant.AddEventTypeName("SupportBeanVariantStream");
            epService.EPAdministrator.Configuration.AddVariantStream("MyVariantStreamFive", variant);
    
            SupportMessageAssertUtil.TryInvalid(epService, "insert into MyVariantStreamFive select * from " + typeof(SupportBean_A).FullName,
                    "Error starting statement: Selected event type is not a valid event type of the variant stream 'MyVariantStreamFive'");
    
            SupportMessageAssertUtil.TryInvalid(epService, "insert into MyVariantStreamFive select IntPrimitive as k0 from " + typeof(SupportBean).FullName,
                    "Error starting statement: Selected event type is not a valid event type of the variant stream 'MyVariantStreamFive' ");
        }
    
        private void RunAssertionInvalidConfig(EPServiceProvider epService) {
            var config = new ConfigurationVariantStream();
            TryInvalidConfig(epService, "abc", config, "Invalid variant stream configuration, no event type name has been added and default type variance requires at least one type, for name 'abc'");
    
            config.AddEventTypeName("dummy");
            TryInvalidConfig(epService, "abc", config, "Event type by name 'dummy' could not be found for use in variant stream configuration by name 'abc'");
        }
    
        private void TryInvalidConfig(EPServiceProvider epService, string name, ConfigurationVariantStream config, string message) {
            try {
                epService.EPAdministrator.Configuration.AddVariantStream(name, config);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
} // end of namespace
