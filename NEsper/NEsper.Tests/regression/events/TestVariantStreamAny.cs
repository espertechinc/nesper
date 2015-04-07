///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestVariantStreamAny 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            ConfigurationVariantStream variant = new ConfigurationVariantStream();
            variant.TypeVariance = TypeVarianceEnum.ANY;
            config.AddVariantStream("MyVariantStream", variant);
            Assert.IsTrue(config.IsVariantStreamExists("MyVariantStream"));
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listener = new SupportUpdateListener();
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI)_epService).ValueAddEventService.GetValueAddProcessor("MyVariantStream").ValueAddEventType;
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("MyVariantStream", type.Metadata.PrimaryName);
            Assert.AreEqual("MyVariantStream", type.Metadata.PublicName);
            Assert.AreEqual("MyVariantStream", type.Name);
            Assert.AreEqual(TypeClass.VARIANT, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            EventType[] valueAddTypes = ((EPServiceProviderSPI)_epService).ValueAddEventService.ValueAddedTypes;
            Assert.AreEqual(1, valueAddTypes.Length);
            Assert.AreSame(type, valueAddTypes[0]);
    
            Assert.AreEqual(0, type.PropertyNames.Length);
            Assert.AreEqual(0, type.PropertyDescriptors.Count);
        }
    
        [TearDown]
        public void TearDown()
        {
            _listener = null;
        }
    
        [Test]
        public void TestAnyType()
        {
            Assert.IsTrue(_epService.EPAdministrator.Configuration.IsVariantStreamExists("MyVariantStream"));
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from " + typeof(SupportBean).FullName);
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from " + typeof(SupportBeanVariantStream).FullName);
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select * from " + typeof(SupportBean_A).FullName);
            _epService.EPAdministrator.CreateEPL("insert into MyVariantStream select Symbol as TheString, Volume as IntPrimitive, Feed as Id from " + typeof(SupportMarketDataBean).FullName);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyVariantStream");
            stmt.Events += _listener.Update;
            Assert.AreEqual(0, stmt.EventType.PropertyNames.Length);
    
            Object eventOne = new SupportBean("E0", -1);
            _epService.EPRuntime.SendEvent(eventOne);
            Assert.AreSame(eventOne, _listener.AssertOneGetNewAndReset().Underlying);
    
            Object eventTwo = new SupportBean_A("E1");
            _epService.EPRuntime.SendEvent(eventTwo);
            Assert.AreSame(eventTwo, _listener.AssertOneGetNewAndReset().Underlying);
    
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("select TheString,Id,IntPrimitive from MyVariantStream");
            stmt.Events += _listener.Update;
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("TheString"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("Id"));
            Assert.AreEqual(typeof(Object), stmt.EventType.GetPropertyType("IntPrimitive"));
    
            String[] fields = "TheString,Id,IntPrimitive".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBeanVariantStream("E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1", null, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", null, 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, "E3", null});
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("s1", 100, 1000L, "f1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"s1", "f1", 1000L});

            _epService.EPAdministrator.DestroyAllStatements();

            // Test inserting a wrapper of underlying plus properties
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.CreateEPL(
                    "create variant schema TheVariantStream as *");
            _epService.EPAdministrator.CreateEPL(
                    "insert into TheVariantStream select 'test' as eventConfigId, * from SupportBean");
            _epService.EPAdministrator.CreateEPL("select * from TheVariantStream").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean theEvent = _listener.AssertOneGetNewAndReset();

            Assert.AreEqual("test", theEvent.Get("eventConfigId"));
            Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
        }
    
        [Test]
        public void TestAnyTypeStaggered()
        {
            // test insert into staggered with map
            ConfigurationVariantStream configVariantStream = new ConfigurationVariantStream();
            configVariantStream.TypeVariance = TypeVarianceEnum.ANY;
            _epService.EPAdministrator.Configuration.AddVariantStream("VarStream", configVariantStream);
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("SupportMarketDataBean", typeof(SupportMarketDataBean));
    
            _epService.EPAdministrator.CreateEPL("insert into MyStream select TheString, IntPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into VarStream select TheString as abc from MyStream");
            _epService.EPAdministrator.CreateEPL("@Name('Target') select * from VarStream.win:keepall()");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EventBean[] arr = EPAssertionUtil.EnumeratorToArray(_epService.EPAdministrator.GetStatement("Target").GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(arr, new String[] {"abc"}, new Object[][] { new Object[] {"E1"}});
    
            _epService.EPAdministrator.CreateEPL("insert into MyStream2 select Feed from SupportMarketDataBean");
            _epService.EPAdministrator.CreateEPL("insert into VarStream select Feed as abc from MyStream2");
    
            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 1, 1L, "E2"));
    
            arr = EPAssertionUtil.EnumeratorToArray(_epService.EPAdministrator.GetStatement("Target").GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(arr, new String[] {"abc"}, new Object[][] { new Object[] {"E1"}, new Object[] {"E2"}});
        }
    }
}
