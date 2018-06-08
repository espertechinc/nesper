///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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

namespace com.espertech.esper.regression.events.variant
{
    public class ExecEventVariantStreamAny : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var variant = new ConfigurationVariantStream();
            variant.TypeVariance = TypeVarianceEnum.ANY;
            configuration.AddVariantStream("MyVariantStream", variant);
            Assert.IsTrue(configuration.IsVariantStreamExists("MyVariantStream"));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMetadata(epService);
            RunAssertionAnyType(epService);
            RunAssertionAnyTypeStaggered(epService);
        }
    
        private void RunAssertionMetadata(EPServiceProvider epService) {
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).ValueAddEventService.GetValueAddProcessor("MyVariantStream").ValueAddEventType;
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyVariantStream", type.Metadata.PrimaryName);
            Assert.AreEqual("MyVariantStream", type.Metadata.PublicName);
            Assert.AreEqual("MyVariantStream", type.Name);
            Assert.AreEqual(TypeClass.VARIANT, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            EventType[] valueAddTypes = ((EPServiceProviderSPI) epService).ValueAddEventService.ValueAddedTypes;
            Assert.AreEqual(1, valueAddTypes.Length);
            Assert.AreSame(type, valueAddTypes[0]);
    
            Assert.AreEqual(0, type.PropertyNames.Length);
            Assert.AreEqual(0, type.PropertyDescriptors.Count);
        }
    
        private void RunAssertionAnyType(EPServiceProvider epService) {
            Assert.IsTrue(epService.EPAdministrator.Configuration.IsVariantStreamExists("MyVariantStream"));
            epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from " + typeof(SupportBean).FullName);
            epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from " + typeof(SupportBeanVariantStream).FullName);
            epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from " + typeof(SupportBean_A).FullName);
            epService.EPAdministrator.CreateEPL("insert into MyVariantStream select symbol as TheString, volume as IntPrimitive, feed as id from " + typeof(SupportMarketDataBean).FullName);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from MyVariantStream");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(0, stmt.EventType.PropertyNames.Length);
    
            var eventOne = new SupportBean("E0", -1);
            epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, listener.AssertOneGetNewAndReset().Underlying);
    
            var eventTwo = new SupportBean_A("E1");
            epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, listener.AssertOneGetNewAndReset().Underlying);
    
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select TheString,id,IntPrimitive from MyVariantStream");
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("id"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("IntPrimitive"));
    
            string[] fields = "TheString,id,IntPrimitive".Split(',');
            epService.EPRuntime.SendEvent(new SupportBeanVariantStream("E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", null, 10});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null, "E3", null});
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("s1", 100, 1000L, "f1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"s1", "f1", 1000L});
            epService.EPAdministrator.DestroyAllStatements();
    
            // Test inserting a wrapper of underlying plus properties
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create variant schema TheVariantStream as *");
            epService.EPAdministrator.CreateEPL("insert into TheVariantStream select 'test' as eventConfigId, * from SupportBean");
            epService.EPAdministrator.CreateEPL("select * from TheVariantStream").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean @event = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("test", @event.Get("eventConfigId"));
            Assert.AreEqual(1, @event.Get("IntPrimitive"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAnyTypeStaggered(EPServiceProvider epService) {
            // test insert into staggered with map
            var configVariantStream = new ConfigurationVariantStream();
            configVariantStream.TypeVariance = TypeVarianceEnum.ANY;
            epService.EPAdministrator.Configuration.AddVariantStream("VarStream", configVariantStream);
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportMarketDataBean", typeof(SupportMarketDataBean));
    
            epService.EPAdministrator.CreateEPL("insert into MyStream select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("insert into VarStream select TheString as abc from MyStream");
            epService.EPAdministrator.CreateEPL("@Name('Target') select * from VarStream#keepall");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EventBean[] arr = EPAssertionUtil.EnumeratorToArray(epService.EPAdministrator.GetStatement("Target").GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(arr, new string[]{"abc"}, new object[][]{new object[] {"E1"}});
    
            epService.EPAdministrator.CreateEPL("insert into MyStream2 select feed from SupportMarketDataBean");
            epService.EPAdministrator.CreateEPL("insert into VarStream select feed as abc from MyStream2");
    
            epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 1, 1L, "E2"));
    
            arr = EPAssertionUtil.EnumeratorToArray(epService.EPAdministrator.GetStatement("Target").GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(arr, new string[]{"abc"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
