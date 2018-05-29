///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.objectarray
{
    using Map = IDictionary<string, object>;

    public class ExecEventObjectArray : RegressionExecution {
        public override void Configure(Configuration configuration) {
            string[] names = {"myInt", "myString", "beanA"};
            object[] types = {typeof(int?), typeof(string), typeof(SupportBeanComplexProps)};
            configuration.AddEventType("MyObjectArrayEvent", names, types);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMetadata(epService);
            RunAssertionNestedObjects(epService);
            RunAssertionQueryFields(epService);
            RunAssertionInvalid(epService);
            RunAssertionNestedEventBeanArray(epService);
            RunAssertionAddRemoveType(epService);
        }
    
        private void RunAssertionNestedEventBeanArray(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema NBALvl1(val string)");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from NBALvl1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{"somevalue"}, "NBALvl1");
            EventBean @event = listener.AssertOneGetNewAndReset();
            stmt.Dispose();
    
            // add containing-type via API
            epService.EPAdministrator.Configuration.AddEventType("NBALvl0", new[]{"lvl1s"}, new object[] {
                new[]{@event.EventType}
            });
            stmt = epService.EPAdministrator.CreateEPL("select lvl1s[0] as c0 from NBALvl0");
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new object[]{new[]{ @event } }, "NBALvl0");
            Assert.AreEqual("somevalue", ((object[]) listener.AssertOneGetNewAndReset().Get("c0"))[0]);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("NBALvl1", true);
            epService.EPAdministrator.Configuration.RemoveEventType("NBALvl0", true);
        }
    
        internal static Object GetNestedKeyOA(object[] array, int index, string keyTwo) {
            Map map = (Map) array[index];
            return map.Get(keyTwo);
        }
    
        internal static Object GetNestedKeyOA(object[] array, int index, string keyTwo, string keyThree) {
            Map map = (Map) array[index];
            map = (Map) map.Get(keyTwo);
            return map.Get(keyThree);
        }
    
        private void RunAssertionMetadata(EPServiceProvider epService) {
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("MyObjectArrayEvent");
            Assert.AreEqual(ApplicationType.OBJECTARR, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyObjectArrayEvent", type.Metadata.PrimaryName);
            Assert.AreEqual("MyObjectArrayEvent", type.Metadata.PublicName);
            Assert.AreEqual("MyObjectArrayEvent", type.Name);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            var types = ((EPServiceProviderSPI) epService).EventAdapterService.AllTypes;
            Assert.AreEqual(1, types.Count);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new[]{
                    new EventPropertyDescriptor("myInt", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("myString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("beanA", typeof(SupportBeanComplexProps), null, false, false, false, false, true),
            }, type.PropertyDescriptors);
        }
    
        private void RunAssertionAddRemoveType(EPServiceProvider epService) {
            // test remove type with statement used (no force)
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select myInt from MyObjectArrayEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").ToArray(), new[]{"stmtOne"});
    
            int numTypes = epService.EPAdministrator.Configuration.EventTypes.Count;
            Assert.AreEqual(typeof(object[]), epService.EPAdministrator.Configuration.GetEventType("MyObjectArrayEvent").UnderlyingType);
    
            try {
                configOps.RemoveEventType("MyObjectArrayEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyObjectArrayEvent"));
            }
    
            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.IsTrue(configOps.RemoveEventType("MyObjectArrayEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("MyObjectArrayEvent", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.AreEqual(numTypes - 1, epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual(null, epService.EPAdministrator.Configuration.GetEventType("MyObjectArrayEvent"));
            try {
                epService.EPAdministrator.CreateEPL("select myInt from MyObjectArrayEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // add back the type
            configOps.AddEventType("MyObjectArrayEvent", new[]{"p01"}, new object[]{typeof(string)});
            Assert.IsTrue(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").IsEmpty());
            Assert.AreEqual(numTypes, epService.EPAdministrator.Configuration.EventTypes.Count);
            Assert.AreEqual("MyObjectArrayEvent", epService.EPAdministrator.Configuration.GetEventType("MyObjectArrayEvent").Name);
    
            // compile
            epService.EPAdministrator.CreateEPL("select p01 from MyObjectArrayEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").ToArray(), new[]{"stmtTwo"});
            try {
                epService.EPAdministrator.CreateEPL("select myInt from MyObjectArrayEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("MyObjectArrayEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyObjectArrayEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyObjectArrayEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyObjectArrayEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyObjectArrayEvent").IsEmpty());
    
            // add back the type
            configOps.AddEventType("MyObjectArrayEvent", new[]{"newprop"}, new object[]{typeof(string)});
            Assert.IsTrue(configOps.IsEventTypeExists("MyObjectArrayEvent"));
    
            // compile
            epService.EPAdministrator.CreateEPL("select newprop from MyObjectArrayEvent");
            try {
                epService.EPAdministrator.CreateEPL("select p01 from MyObjectArrayEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNestedObjects(EPServiceProvider epService) {
            string statementText = "select beanA.simpleProperty as simple," +
                    "beanA.nested.nestedValue as nested," +
                    "beanA.indexed[1] as indexed," +
                    "beanA.nested.nestedNested.nestedNestedValue as nestednested " +
                    "from MyObjectArrayEvent#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new object[]{3, "some string", SupportBeanComplexProps.MakeDefaultBean()}, "MyObjectArrayEvent");
            Assert.AreEqual("NestedValue", listener.LastNewData[0].Get("nested"));
            Assert.AreEqual(2, listener.LastNewData[0].Get("indexed"));
            Assert.AreEqual("NestedNestedValue", listener.LastNewData[0].Get("nestednested"));
            statement.Stop();
        }
    
        private void RunAssertionQueryFields(EPServiceProvider epService) {
            string statementText = "select myInt + 2 as intVal, 'x' || myString || 'x' as stringVal from MyObjectArrayEvent#length(5)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // send IDictionary<string, Object> event
            epService.EPRuntime.SendEvent(new object[]{3, "some string", SupportBeanComplexProps.MakeDefaultBean()}, "MyObjectArrayEvent");
            Assert.AreEqual(5, listener.LastNewData[0].Get("intVal"));
            Assert.AreEqual("xsome stringx", listener.LastNewData[0].Get("stringVal"));
    
            // send Map base event
            epService.EPRuntime.SendEvent(new object[]{4, "string2", null}, "MyObjectArrayEvent");
            Assert.AreEqual(6, listener.LastNewData[0].Get("intVal"));
            Assert.AreEqual("xstring2x", listener.LastNewData[0].Get("stringVal"));
    
            statement.Stop();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            try {
                Configuration configuration = SupportConfigFactory.GetConfiguration();
                configuration.AddEventType("MyInvalidEvent", new[]{"p00"}, new object[]{typeof(int), typeof(string)});
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Number of property names and property types do not match, found 1 property names and 2 property types", ex.Message);
            }
    
            TryInvalid(epService, "select XXX from MyObjectArrayEvent#length(5)");
            TryInvalid(epService, "select myString * 2 from MyObjectArrayEvent#length(5)");
            TryInvalid(epService, "select String.Trim(myInt) from MyObjectArrayEvent#length(5)");
    
            var invalidOAConfig = new ConfigurationEventTypeObjectArray();
            invalidOAConfig.SuperTypes = new HashSet<string>(Collections.List("A", "B"));
            var invalidOANames = new[]{"p00"};
            var invalidOATypes = new object[]{typeof(int)};
            try {
                Configuration configuration = SupportConfigFactory.GetConfiguration();
                configuration.AddEventType("MyInvalidEventTwo", invalidOANames, invalidOATypes, invalidOAConfig);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Object-array event types only allow a single supertype", ex.Message);
            }
    
            try {
                epService.EPAdministrator.Configuration.AddEventType("MyInvalidOA", invalidOANames, invalidOATypes, invalidOAConfig);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual("Object-array event types only allow a single supertype", ex.Message);
            }
    
            try {
                epService.EPAdministrator.CreateEPL("create objectarray schema InvalidOA () inherits A, B");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Object-array event types only allow a single supertype [create objectarray schema InvalidOA () inherits A, B]", ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryInvalid(EPServiceProvider epService, string statementText) {
            try {
                epService.EPAdministrator.CreateEPL(statementText);
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    }
} // end of namespace
