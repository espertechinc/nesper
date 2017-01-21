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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
    public class TestRevisionDeclared
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private EPServiceProvider _epService;
        private EPStatement _stmtCreateWin;
        private SupportUpdateListener _listenerOne;
        private SupportUpdateListener _listenerTwo;
        private SupportUpdateListener _listenerThree;
        private readonly String[] _fields = "K0,P0,P1,P2,P3,P4,P5".Split(',');
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
    
            config.AddEventType<SupportBean>();
            config.AddEventType("FullEvent", typeof(SupportRevisionFull));
            config.AddEventType("D1", typeof(SupportDeltaOne));
            config.AddEventType("D2", typeof(SupportDeltaTwo));
            config.AddEventType("D3", typeof(SupportDeltaThree));
            config.AddEventType("D4", typeof(SupportDeltaFour));
            config.AddEventType("D5", typeof(SupportDeltaFive));
    
            ConfigurationRevisionEventType configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = (new String[]{"K0"});
            configRev.AddNameBaseEventType("FullEvent");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D2");
            configRev.AddNameDeltaEventType("D3");
            configRev.AddNameDeltaEventType("D4");
            configRev.AddNameDeltaEventType("D5");
            config.AddRevisionEventType("RevisableQuote", configRev);
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _listenerOne = new SupportUpdateListener();
            _listenerTwo = new SupportUpdateListener();
            _listenerThree = new SupportUpdateListener();
    
            _stmtCreateWin = _epService.EPAdministrator.CreateEPL("create window RevQuote.win:keepall() as select * from RevisableQuote");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from FullEvent");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D1");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D2");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D3");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D4");
            _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D5");
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) _epService).ValueAddEventService.GetValueAddProcessor("RevQuote").ValueAddEventType;
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("RevisableQuote", type.Metadata.PrimaryName);
            Assert.AreEqual("RevisableQuote", type.Metadata.PublicName);
            Assert.AreEqual("RevisableQuote", type.Name);
            Assert.AreEqual(TypeClass.REVISION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            EventType[] valueAddTypes = ((EPServiceProviderSPI) _epService).ValueAddEventService.ValueAddedTypes;
            Assert.AreEqual(1, valueAddTypes.Length);
            Assert.AreSame(type, valueAddTypes[0]);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("K0", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("P0", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("P1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("P2", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("P3", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("P4", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("P5", typeof(string), typeof(char), false, false, true, false, false)
            }, type.PropertyDescriptors);
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listenerOne = null;
            _listenerTwo = null;
            _listenerThree = null;
        }
    
        [Test]
        public void TestRevision() {
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select * from RevQuote");
            consumerOne.Events += _listenerOne.Update;
            EPStatement consumerTwo = _epService.EPAdministrator.CreateEPL("select K0, count(*) as count, sum(Int64.Parse(P0)) as sum from RevQuote group by K0");
            consumerTwo.Events += _listenerTwo.Update;
            EPStatement consumerThree = _epService.EPAdministrator.CreateEPL("select * from RevQuote output every 2 events");
            consumerThree.Events += _listenerThree.Update;
            String[] agg = "K0,count,sum".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("K00", "01", "P10", "20", "P30", "40", "50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "01", "P10", "20", "P30", "40", "50"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "01", "P10", "20", "P30", "40", "50"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 1L});
            Assert.IsFalse(_listenerThree.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportDeltaThree("K00", "03", "41"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "03", "P10", "20", "P30", "41", "50"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "03", "P10", "20", "P30", "41", "50"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 3L});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[0], _fields, new Object[]{"K00", "01", "P10", "20", "P30", "40", "50"});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[1], _fields, new Object[]{"K00", "03", "P10", "20", "P30", "41", "50"});
            _listenerThree.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportDeltaOne("K00", "P11", "51"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "03", "P11", "20", "P30", "41", "51"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "03", "P11", "20", "P30", "41", "51"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 3L});
            Assert.IsFalse(_listenerThree.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportDeltaTwo("K00", "04", "21", "P31"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "04", "P11", "21", "P31", "41", "51"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "04", "P11", "21", "P31", "41", "51"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 4L});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[0], _fields, new Object[]{"K00", "03", "P11", "20", "P30", "41", "51"});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[1], _fields, new Object[]{"K00", "04", "P11", "21", "P31", "41", "51"});
            _listenerThree.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportDeltaFour("K00", "05", "22", "52"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "05", "P11", "22", "P31", "41", "52"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "05", "P11", "22", "P31", "41", "52"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 5L});
            Assert.IsFalse(_listenerThree.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportDeltaFive("K00", "P12", "53"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "05", "P12", "22", "P31", "41", "53"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "05", "P12", "22", "P31", "41", "53"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 5L});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[0], _fields, new Object[]{"K00", "05", "P11", "22", "P31", "41", "52"});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[1], _fields, new Object[]{"K00", "05", "P12", "22", "P31", "41", "53"});
            _listenerThree.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("K00", "06", "P13", "23", "P32", "42", "54"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "06", "P13", "23", "P32", "42", "54"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "06", "P13", "23", "P32", "42", "54"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 6L});
            Assert.IsFalse(_listenerThree.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportDeltaOne("K00", "P14", "55"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"K00", "06", "P14", "23", "P32", "42", "55"});
            EPAssertionUtil.AssertProps(_stmtCreateWin.First(), _fields, new Object[]{"K00", "06", "P14", "23", "P32", "42", "55"});
            EPAssertionUtil.AssertProps(_listenerTwo.AssertOneGetNewAndReset(), agg, new Object[]{"K00", 1L, 6L});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[0], _fields, new Object[]{"K00", "06", "P13", "23", "P32", "42", "54"});
            EPAssertionUtil.AssertProps(_listenerThree.LastNewData[1], _fields, new Object[]{"K00", "06", "P14", "23", "P32", "42", "55"});
            _listenerThree.Reset();
        }
    
        [Test]
        public void TestOnDelete()
        {
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from RevQuote");
            consumerOne.Events += _listenerOne.Update;
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=2) as sb delete from RevQuote where TheString = P2");

            Log.Debug("a00");
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "a00", "a10", "a20", "a30", "a40", "a50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"a", "a00", "a10", "a20", "a30", "a40", "a50"});
    
            _epService.EPRuntime.SendEvent(new SupportDeltaThree("x", "03", "41"));
            Assert.IsFalse(_listenerOne.IsInvoked);
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=3) as sb delete from RevQuote where TheString = P3");

            Log.Debug("b00");
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "b00", "b10", "b20", "b30", "b40", "b50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"b", "b00", "b10", "b20", "b30", "b40", "b50"});

            Log.Debug("a01");
            _epService.EPRuntime.SendEvent(new SupportDeltaThree("a", "a01", "a41"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[]{"a", "a01", "a10", "a20", "a30", "a41", "a50"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[]{"a", "a00", "a10", "a20", "a30", "a40", "a50"});
            _listenerOne.Reset();

            Log.Debug("c00");
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("c", "c00", "c10", "c20", "c30", "c40", "c50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"c", "c00", "c10", "c20", "c30", "c40", "c50"});
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=0) as sb delete from RevQuote where TheString = P0");

            Log.Debug("c11");
            _epService.EPRuntime.SendEvent(new SupportDeltaFive("c", "c11", "c51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[]{"c", "c00", "c11", "c20", "c30", "c40", "c51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[]{"c", "c00", "c10", "c20", "c30", "c40", "c50"});
            _listenerOne.Reset();
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=1) as sb delete from RevQuote where TheString = P1");

            Log.Debug("d00");
            _epService.EPRuntime.SendEvent(new SupportRevisionFull("d", "d00", "d10", "d20", "d30", "d40", "d50"));
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), _fields, new Object[]{"d", "d00", "d10", "d20", "d30", "d40", "d50"});

            Log.Debug("d01");
            _epService.EPRuntime.SendEvent(new SupportDeltaFour("d", "d01", "d21", "d51"));
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], _fields, new Object[]{"d", "d01", "d10", "d21", "d30", "d40", "d51"});
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], _fields, new Object[]{"d", "d00", "d10", "d20", "d30", "d40", "d50"});
            _listenerOne.Reset();
    
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                    new Object[][]{new Object[] {"b", "b00", "b10", "b20", "b30", "b40", "b50"}, new Object[] {"a", "a01", "a10", "a20", "a30", "a41", "a50"},
                            new Object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}, new Object[] {"d", "d01", "d10", "d21", "d30", "d40", "d51"}
                    });
    
            _epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=4) as sb delete from RevQuote where TheString = P4");
    
            _epService.EPRuntime.SendEvent(new SupportBean("abc", 1));
            Assert.IsFalse(_listenerOne.IsInvoked);

            Log.Debug("delete b");
            _epService.EPRuntime.SendEvent(new SupportBean("b40", 4));  // delete b
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields, new Object[]{"b", "b00", "b10", "b20", "b30", "b40", "b50"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                    new Object[][]{new Object[] {"a", "a01", "a10", "a20", "a30", "a41", "a50"}, new Object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}, new Object[] {"d", "d01", "d10", "d21", "d30", "d40", "d51"}});

            Log.Debug("delete d");
            _epService.EPRuntime.SendEvent(new SupportBean("d21", 2)); // delete d
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields, new Object[]{"d", "d01", "d10", "d21", "d30", "d40", "d51"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields,
                    new Object[][]{new Object[] {"a", "a01", "a10", "a20", "a30", "a41", "a50"}, new Object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}});

            Log.Debug("delete a");
            _epService.EPRuntime.SendEvent(new SupportBean("a30", 3)); // delete a
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields, new Object[]{"a", "a01", "a10", "a20", "a30", "a41", "a50"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields, new Object[][]{new Object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}});
    
            Log.Debug("delete c");
            _epService.EPRuntime.SendEvent(new SupportBean("c11", 1)); // delete c
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetOldAndReset(), _fields, new Object[]{"c", "c00", "c11", "c20", "c30", "c40", "c51"});
            EPAssertionUtil.AssertPropsPerRow(_stmtCreateWin.GetEnumerator(), _fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("c11", 1));
            Assert.IsFalse(_listenerOne.IsInvoked);
        }
    
        [Test]
        public void TestRevisionGen() {
            Random random = new Random();
            IDictionary<String, IDictionary<String, String>> last = new Dictionary<String, IDictionary<String, String>>();
            int count = 0;
            String[] groups = new String[]{"K0", "K1", "K2", "K4"};
    
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select * from RevQuote");
            consumerOne.Events += _listenerOne.Update;
    
            for (int i = 0; i < groups.Length; i++) {
                String key = groups[i];
                Object theEvent = new SupportRevisionFull(key, "0-" + Next(count), "1-" + Next(count), "2-" + Next(count),
                        "3-" + Next(count), "4-" + Next(count), "5-" + Next(count));
                Add(last, key, "0-" + Next(count), "1-" + Next(count), "2-" + Next(count),
                        "3-" + Next(count), "4-" + Next(count), "5-" + Next(count));
                _epService.EPRuntime.SendEvent(theEvent);
            }
            _listenerOne.Reset();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }  // ending instrumentation for now comes volume
    
            for (int i = 0; i < 10000; i++) {
                if (i % 20000 == 0) {
                    Log.Debug(".testRevisionGen Loop " + i);
                }
                int typeNum = random.Next(6);
                String key = groups[random.Next(groups.Length)];
                count++;
    
                Object theEvent;
                if (typeNum == 0) {
                    theEvent = new SupportRevisionFull(key, "0-" + Next(count), "1-" + Next(count), "2-" + Next(count),
                            "3-" + Next(count), "4-" + Next(count), "5-" + Next(count));
                    Add(last, key, "0-" + Next(count), "1-" + Next(count), "2-" + Next(count),
                            "3-" + Next(count), "4-" + Next(count), "5-" + Next(count));
                } else if (typeNum == 1) {
                    theEvent = new SupportDeltaOne(key, "1-" + Next(count), "5-" + Next(count));
                    Add(last, key, null, "1-" + Next(count), null, null, null, "5-" + Next(count));
                } else if (typeNum == 2) {
                    theEvent = new SupportDeltaTwo(key, "0-" + Next(count), "2-" + Next(count), "3-" + Next(count));
                    Add(last, key, "0-" + Next(count), null, "2-" + Next(count), "3-" + Next(count), null, null);
                } else if (typeNum == 3) {
                    theEvent = new SupportDeltaThree(key, "0-" + Next(count), "4-" + Next(count));
                    Add(last, key, "0-" + Next(count), null, null, null, "4-" + Next(count), null);
                } else if (typeNum == 4) {
                    theEvent = new SupportDeltaFour(key, "0-" + Next(count), "2-" + Next(count), "5-" + Next(count));
                    Add(last, key, "0-" + Next(count), null, "2-" + Next(count), null, null, "5-" + Next(count));
                } else if (typeNum == 5) {
                    theEvent = new SupportDeltaFive(key, "1-" + Next(count), "5-" + Next(count));
                    Add(last, key, null, "1-" + Next(count), null, null, null, "5-" + Next(count));
                } else {
                    throw new IllegalStateException();
                }
    
                _epService.EPRuntime.SendEvent(theEvent);
                AssertEvent(last, _listenerOne.AssertOneGetNewAndReset(), count);
            }
        }
    
        [Test]
        public void TestInvalidConfig() {
            ConfigurationRevisionEventType config = new ConfigurationRevisionEventType();
            TryInvalidConfig("abc", config, "Required base event type name is not set in the configuration for revision event type 'abc'");
    
            _epService.EPAdministrator.Configuration.AddEventType("MyEvent", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("MyComplex", typeof(SupportBeanComplexProps));
            _epService.EPAdministrator.Configuration.AddEventType("MyTypeChange", typeof(SupportBeanTypeChange));
    
            config.AddNameBaseEventType("XYZ");
            TryInvalidConfig("abc", config, "Could not locate event type for name 'XYZ' in the configuration for revision event type 'abc'");
    
            config.NameBaseEventTypes.Clear();
            config.AddNameBaseEventType("MyEvent");
            TryInvalidConfig("abc", config, "Required key properties are not set in the configuration for revision event type 'abc'");
    
            config.AddNameBaseEventType("AEvent");
            config.AddNameBaseEventType("AEvent");
            TryInvalidConfig("abc", config, "Only one base event type name may be added to revision event type 'abc', multiple base types are not yet supported");
    
            config.NameBaseEventTypes.Clear();
            config.AddNameBaseEventType("MyEvent");
            config.KeyPropertyNames = (new String[0]);
            TryInvalidConfig("abc", config, "Required key properties are not set in the configuration for revision event type 'abc'");
    
            config.KeyPropertyNames = (new String[]{"xyz"});
            TryInvalidConfig("abc", config, "Key property 'xyz' as defined in the configuration for revision event type 'abc' does not exists in event type 'MyEvent'");
    
            config.KeyPropertyNames = (new String[]{"IntPrimitive"});
            config.AddNameDeltaEventType("MyComplex");
            TryInvalidConfig("abc", config, "Key property 'IntPrimitive' as defined in the configuration for revision event type 'abc' does not exists in event type 'MyComplex'");
    
            config.AddNameDeltaEventType("XYZ");
            TryInvalidConfig("abc", config, "Could not locate event type for name 'XYZ' in the configuration for revision event type 'abc'");
    
            config.NameDeltaEventTypes.Clear();
            config.KeyPropertyNames = (new String[]{"IntBoxed"});
            config.AddNameDeltaEventType("MyTypeChange");  // invalid intPrimitive property type
            TryInvalidConfig("abc", config, "Property named 'IntPrimitive' does not have the same type for base and delta types of revision event type 'abc'");
    
            config.NameDeltaEventTypes.Clear();
            _epService.EPAdministrator.Configuration.AddRevisionEventType("abc", config);
        }
    
        [Test]
        public void TestInvalidInsertInto() {
            try {
                _epService.EPAdministrator.CreateEPL("insert into RevQuote select * from " + typeof(SupportBean).FullName);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Selected event type is not a valid base or delta event type of revision event type 'RevisableQuote' [insert into RevQuote select * from com.espertech.esper.support.bean.SupportBean]", ex.Message);
            }
    
            try {
                _epService.EPAdministrator.CreateEPL("insert into RevQuote select IntPrimitive as K0 from " + typeof(SupportBean).FullName);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Selected event type is not a valid base or delta event type of revision event type 'RevisableQuote' [insert into RevQuote select IntPrimitive as K0 from com.espertech.esper.support.bean.SupportBean]", ex.Message);
            }
        }
    
        private void TryInvalidConfig(String name, ConfigurationRevisionEventType config, String message) {
            try {
                _epService.EPAdministrator.Configuration.AddRevisionEventType(name, config);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void AssertEvent(IDictionary<String, IDictionary<String, String>> last, EventBean eventBean, int count) {
            String error = "Error asseting count " + count;
            String key = (String) eventBean.Get("K0");
            IDictionary<String, String> vals = last.Get(key);
            Assert.AreEqual(vals.Get("P0"), eventBean.Get("P0"), error);
            Assert.AreEqual(vals.Get("P1"), eventBean.Get("P1"), error);
            Assert.AreEqual(vals.Get("P2"), eventBean.Get("P2"), error);
            Assert.AreEqual(vals.Get("P3"), eventBean.Get("P3"), error);
            Assert.AreEqual(vals.Get("P4"), eventBean.Get("P4"), error);
            Assert.AreEqual(vals.Get("P5"), eventBean.Get("P5"), error);
        }
    
        private void Add(IDictionary<String, IDictionary<String, String>> last, String key, String s0, String s1, String s2, String s3, String s4, String s5) {
            IDictionary<String, String> entry = last.Get(key);
            if (entry == null) {
                entry = new Dictionary<String, String>();
                last.Put(key, entry);
            }
    
            if (s0 != null) {
                entry["P0"] = s0;
            }
            if (s1 != null) {
                entry["P1"] = s1;
            }
            if (s2 != null) {
                entry["P2"] = s2;
            }
            if (s3 != null) {
                entry["P3"] = s3;
            }
            if (s4 != null) {
                entry["P4"] = s4;
            }
            if (s5 != null) {
                entry["P5"] = s5;
            }
        }
    
        private String Next(int num) {
            return Convert.ToString(num);
        }
    }
}
