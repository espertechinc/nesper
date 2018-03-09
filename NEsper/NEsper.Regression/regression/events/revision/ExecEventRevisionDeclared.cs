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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.revision
{
    public class ExecEventRevisionDeclared : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string[] fields = "K0,P0,P1,P2,P3,P4,P5".Split(',');
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("FullEvent", typeof(SupportRevisionFull));
            configuration.AddEventType("D1", typeof(SupportDeltaOne));
            configuration.AddEventType("D2", typeof(SupportDeltaTwo));
            configuration.AddEventType("D3", typeof(SupportDeltaThree));
            configuration.AddEventType("D4", typeof(SupportDeltaFour));
            configuration.AddEventType("D5", typeof(SupportDeltaFive));
    
            var configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = new string[]{"K0"};
            configRev.AddNameBaseEventType("FullEvent");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D2");
            configRev.AddNameDeltaEventType("D3");
            configRev.AddNameDeltaEventType("D4");
            configRev.AddNameDeltaEventType("D5");
            configuration.AddRevisionEventType("RevisableQuote", configRev);
        }
    
        public override void Run(EPServiceProvider epService) {
            EPStatement stmtCreateWin = epService.EPAdministrator.CreateEPL("create window RevQuote#keepall as select * from RevisableQuote");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from FullEvent");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D1");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D2");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D3");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D4");
            epService.EPAdministrator.CreateEPL("insert into RevQuote select * from D5");
    
            RunAssertionMetadata(epService);
            RunAssertionRevision(epService, stmtCreateWin);
            RunAssertionOnDelete(epService, stmtCreateWin);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionRevisionGen(epService);
            }
            RunAssertionInvalidConfig(epService);
            RunAssertionInvalidInsertInto(epService);
        }
    
        private void RunAssertionMetadata(EPServiceProvider epService) {
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).ValueAddEventService.GetValueAddProcessor("RevQuote").ValueAddEventType;
            Assert.AreEqual(null, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("RevisableQuote", type.Metadata.PrimaryName);
            Assert.AreEqual("RevisableQuote", type.Metadata.PublicName);
            Assert.AreEqual("RevisableQuote", type.Name);
            Assert.AreEqual(TypeClass.REVISION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            EventType[] valueAddTypes = ((EPServiceProviderSPI) epService).ValueAddEventService.ValueAddedTypes;
            Assert.AreEqual(1, valueAddTypes.Length);
            Assert.AreSame(type, valueAddTypes[0]);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("K0", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("P0", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("P1", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("P2", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("P3", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("P4", typeof(string), null, false, false, false, false, false),
                    new EventPropertyDescriptor("P5", typeof(string), null, false, false, false, false, false)
            }, type.PropertyDescriptors);
        }
    
        private void RunAssertionRevision(EPServiceProvider epService, EPStatement stmtCreateWin) {
            var listenerOne = new SupportUpdateListener();
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select * from RevQuote");
            consumerOne.Events += listenerOne.Update;
            EPStatement consumerTwo = epService.EPAdministrator.CreateEPL("select K0, count(*) as count, sum(Int64.Parse(P0)) as sum from RevQuote group by K0");
            var listenerTwo = new SupportUpdateListener();
            consumerTwo.Events += listenerTwo.Update;
            EPStatement consumerThree = epService.EPAdministrator.CreateEPL("select * from RevQuote output every 2 events");
            var listenerThree = new SupportUpdateListener();
            consumerThree.Events += listenerThree.Update;
            string[] agg = "K0,count,sum".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("k00", "01", "p10", "20", "p30", "40", "50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "01", "p10", "20", "p30", "40", "50"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "01", "p10", "20", "p30", "40", "50"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 1L});
            Assert.IsFalse(listenerThree.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportDeltaThree("k00", "03", "41"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "03", "p10", "20", "p30", "41", "50"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "03", "p10", "20", "p30", "41", "50"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 3L});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[0], fields, new object[]{"k00", "01", "p10", "20", "p30", "40", "50"});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[1], fields, new object[]{"k00", "03", "p10", "20", "p30", "41", "50"});
            listenerThree.Reset();
    
            epService.EPRuntime.SendEvent(new SupportDeltaOne("k00", "p11", "51"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "03", "p11", "20", "p30", "41", "51"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "03", "p11", "20", "p30", "41", "51"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 3L});
            Assert.IsFalse(listenerThree.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportDeltaTwo("k00", "04", "21", "p31"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "04", "p11", "21", "p31", "41", "51"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "04", "p11", "21", "p31", "41", "51"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 4L});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[0], fields, new object[]{"k00", "03", "p11", "20", "p30", "41", "51"});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[1], fields, new object[]{"k00", "04", "p11", "21", "p31", "41", "51"});
            listenerThree.Reset();
    
            epService.EPRuntime.SendEvent(new SupportDeltaFour("k00", "05", "22", "52"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "05", "p11", "22", "p31", "41", "52"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "05", "p11", "22", "p31", "41", "52"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 5L});
            Assert.IsFalse(listenerThree.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("k00", "p12", "53"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "05", "p12", "22", "p31", "41", "53"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "05", "p12", "22", "p31", "41", "53"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 5L});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[0], fields, new object[]{"k00", "05", "p11", "22", "p31", "41", "52"});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[1], fields, new object[]{"k00", "05", "p12", "22", "p31", "41", "53"});
            listenerThree.Reset();
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("k00", "06", "p13", "23", "p32", "42", "54"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "06", "p13", "23", "p32", "42", "54"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "06", "p13", "23", "p32", "42", "54"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 6L});
            Assert.IsFalse(listenerThree.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportDeltaOne("k00", "p14", "55"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"k00", "06", "p14", "23", "p32", "42", "55"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"k00", "06", "p14", "23", "p32", "42", "55"});
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), agg, new object[]{"k00", 1L, 6L});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[0], fields, new object[]{"k00", "06", "p13", "23", "p32", "42", "54"});
            EPAssertionUtil.AssertProps(listenerThree.LastNewData[1], fields, new object[]{"k00", "06", "p14", "23", "p32", "42", "55"});
            listenerThree.Reset();
    
            consumerOne.Dispose();
            consumerTwo.Dispose();
            consumerThree.Dispose();
            epService.EPRuntime.ExecuteQuery("delete from RevQuote");
        }
    
        private void RunAssertionOnDelete(EPServiceProvider epService, EPStatement stmtCreateWin) {
            var statements = new List<EPStatement>();
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select irstream * from RevQuote");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
            statements.Add(consumerOne);
    
            statements.Add(epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=2) as sb delete from RevQuote where TheString = P2"));

            Log.Debug("a00");
            epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "a00", "a10", "a20", "a30", "a40", "a50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"a", "a00", "a10", "a20", "a30", "a40", "a50"});
    
            epService.EPRuntime.SendEvent(new SupportDeltaThree("x", "03", "41"));
            Assert.IsFalse(listenerOne.IsInvoked);
    
            statements.Add(epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=3) as sb delete from RevQuote where TheString = P3"));

            Log.Debug("b00");
            epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "b00", "b10", "b20", "b30", "b40", "b50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"b", "b00", "b10", "b20", "b30", "b40", "b50"});

            Log.Debug("a01");
            epService.EPRuntime.SendEvent(new SupportDeltaThree("a", "a01", "a41"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"a", "a01", "a10", "a20", "a30", "a41", "a50"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"a", "a00", "a10", "a20", "a30", "a40", "a50"});
            listenerOne.Reset();

            Log.Debug("c00");
            epService.EPRuntime.SendEvent(new SupportRevisionFull("c", "c00", "c10", "c20", "c30", "c40", "c50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"c", "c00", "c10", "c20", "c30", "c40", "c50"});
    
            statements.Add(epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=0) as sb delete from RevQuote where TheString = P0"));

            Log.Debug("c11");
            epService.EPRuntime.SendEvent(new SupportDeltaFive("c", "c11", "c51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"c", "c00", "c11", "c20", "c30", "c40", "c51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"c", "c00", "c10", "c20", "c30", "c40", "c50"});
            listenerOne.Reset();
    
            statements.Add(epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=1) as sb delete from RevQuote where TheString = P1"));

            Log.Debug("d00");
            epService.EPRuntime.SendEvent(new SupportRevisionFull("d", "d00", "d10", "d20", "d30", "d40", "d50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"d", "d00", "d10", "d20", "d30", "d40", "d50"});

            Log.Debug("d01");
            epService.EPRuntime.SendEvent(new SupportDeltaFour("d", "d01", "d21", "d51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"d", "d01", "d10", "d21", "d30", "d40", "d51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"d", "d00", "d10", "d20", "d30", "d40", "d50"});
            listenerOne.Reset();
    
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields,
                    new object[][]{
                        new object[] {"b", "b00", "b10", "b20", "b30", "b40", "b50"},
                        new object[] {"a", "a01", "a10", "a20", "a30", "a41", "a50"},
                        new object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"},
                        new object[] {"d", "d01", "d10", "d21", "d30", "d40", "d51"}
                    });
    
            statements.Add(epService.EPAdministrator.CreateEPL("on SupportBean(IntPrimitive=4) as sb delete from RevQuote where TheString = P4"));
    
            epService.EPRuntime.SendEvent(new SupportBean("abc", 1));
            Assert.IsFalse(listenerOne.IsInvoked);
    
            Log.Debug("delete b");
            epService.EPRuntime.SendEvent(new SupportBean("b40", 4));  // delete b
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"b", "b00", "b10", "b20", "b30", "b40", "b50"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields,
                    new object[][]{new object[] {"a", "a01", "a10", "a20", "a30", "a41", "a50"}, new object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}, new object[] {"d", "d01", "d10", "d21", "d30", "d40", "d51"}});

            Log.Debug("delete d");
            epService.EPRuntime.SendEvent(new SupportBean("d21", 2)); // delete d
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"d", "d01", "d10", "d21", "d30", "d40", "d51"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields,
                    new object[][]{new object[] {"a", "a01", "a10", "a20", "a30", "a41", "a50"}, new object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}});

            Log.Debug("delete a");
            epService.EPRuntime.SendEvent(new SupportBean("a30", 3)); // delete a
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"a", "a01", "a10", "a20", "a30", "a41", "a50"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields, new object[][]{new object[] {"c", "c00", "c11", "c20", "c30", "c40", "c51"}});

            Log.Debug("delete c");
            epService.EPRuntime.SendEvent(new SupportBean("c11", 1)); // delete c
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetOldAndReset(), fields, new object[]{"c", "c00", "c11", "c20", "c30", "c40", "c51"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("c11", 1));
            Assert.IsFalse(listenerOne.IsInvoked);
    
            foreach (EPStatement statement in statements) {
                statement.Dispose();
            }
        }
    
        private void RunAssertionRevisionGen(EPServiceProvider epService) {
            var random = new Random();
            var last = new Dictionary<string, IDictionary<string, string>>();
            int count = 0;
            var groups = new string[]{"K0", "K1", "K2", "K4"};
    
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select * from RevQuote");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
    
            for (int i = 0; i < groups.Length; i++) {
                string key = groups[i];
                var theEvent = new SupportRevisionFull(key, "0-" + Next(count), "1-" + Next(count), "2-" + Next(count),
                        "3-" + Next(count), "4-" + Next(count), "5-" + Next(count));
                Add(last, key, "0-" + Next(count), "1-" + Next(count), "2-" + Next(count),
                        "3-" + Next(count), "4-" + Next(count), "5-" + Next(count));
                epService.EPRuntime.SendEvent(theEvent);
            }
            listenerOne.Reset();
    
            for (int i = 0; i < 10000; i++) {
                if (i % 20000 == 0) {
                    Log.Debug(".testRevisionGen Loop " + i);
                }
                int typeNum = random.Next(6);
                string key = groups[random.Next(groups.Length)];
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
    
                epService.EPRuntime.SendEvent(theEvent);
                AssertEvent(last, listenerOne.AssertOneGetNewAndReset(), count);
            }
            consumerOne.Dispose();
        }
    
        private void RunAssertionInvalidConfig(EPServiceProvider epService) {
            var config = new ConfigurationRevisionEventType();
            TryInvalidConfig(epService, "abc", config, "Required base event type name is not set in the configuration for revision event type 'abc'");
    
            epService.EPAdministrator.Configuration.AddEventType("MyEvent", typeof(SupportBean));
            epService.EPAdministrator.Configuration.AddEventType("MyComplex", typeof(SupportBeanComplexProps));
            epService.EPAdministrator.Configuration.AddEventType("MyTypeChange", typeof(SupportBeanTypeChange));
    
            config.AddNameBaseEventType("XYZ");
            TryInvalidConfig(epService, "abc", config, "Could not locate event type for name 'XYZ' in the configuration for revision event type 'abc'");
    
            config.NameBaseEventTypes.Clear();
            config.AddNameBaseEventType("MyEvent");
            TryInvalidConfig(epService, "abc", config, "Required key properties are not set in the configuration for revision event type 'abc'");
    
            config.AddNameBaseEventType("AEvent");
            config.AddNameBaseEventType("AEvent");
            TryInvalidConfig(epService, "abc", config, "Only one base event type name may be added to revision event type 'abc', multiple base types are not yet supported");
    
            config.NameBaseEventTypes.Clear();
            config.AddNameBaseEventType("MyEvent");
            config.KeyPropertyNames = new string[0];
            TryInvalidConfig(epService, "abc", config, "Required key properties are not set in the configuration for revision event type 'abc'");
    
            config.KeyPropertyNames = new string[]{"xyz"};
            TryInvalidConfig(epService, "abc", config, "Key property 'xyz' as defined in the configuration for revision event type 'abc' does not exists in event type 'MyEvent'");
    
            config.KeyPropertyNames = new string[]{"IntPrimitive"};
            config.AddNameDeltaEventType("MyComplex");
            TryInvalidConfig(epService, "abc", config, "Key property 'IntPrimitive' as defined in the configuration for revision event type 'abc' does not exists in event type 'MyComplex'");
    
            config.AddNameDeltaEventType("XYZ");
            TryInvalidConfig(epService, "abc", config, "Could not locate event type for name 'XYZ' in the configuration for revision event type 'abc'");
    
            config.NameDeltaEventTypes.Clear();
            config.KeyPropertyNames = new string[]{"IntBoxed"};
            config.AddNameDeltaEventType("MyTypeChange");  // invalid IntPrimitive property type
            TryInvalidConfig(epService, "abc", config, "Property named 'IntPrimitive' does not have the same type for base and delta types of revision event type 'abc'");
    
            config.NameDeltaEventTypes.Clear();
            epService.EPAdministrator.Configuration.AddRevisionEventType("abc", config);
        }
    
        private void RunAssertionInvalidInsertInto(EPServiceProvider epService) {
            SupportMessageAssertUtil.TryInvalid(epService, "insert into RevQuote select * from " + typeof(SupportBean).FullName,
                    "Error starting statement: Selected event type is not a valid base or delta event type of revision event type 'RevisableQuote' [");
    
            SupportMessageAssertUtil.TryInvalid(epService, "insert into RevQuote select IntPrimitive as K0 from " + typeof(SupportBean).FullName,
                    "Error starting statement: Selected event type is not a valid base or delta event type of revision event type 'RevisableQuote' ");
        }
    
        private void TryInvalidConfig(EPServiceProvider epService, string name, ConfigurationRevisionEventType config, string message) {
            try {
                epService.EPAdministrator.Configuration.AddRevisionEventType(name, config);
                Assert.Fail();
            } catch (ConfigurationException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void AssertEvent(IDictionary<string, IDictionary<string, string>> last, EventBean eventBean, int count) {
            string error = "Error asseting count " + count;
            string key = (string) eventBean.Get("K0");
            IDictionary<string, string> vals = last.Get(key);
            Assert.AreEqual(vals.Get("P0"), eventBean.Get("P0"), error);
            Assert.AreEqual(vals.Get("P1"), eventBean.Get("P1"), error);
            Assert.AreEqual(vals.Get("P2"), eventBean.Get("P2"), error);
            Assert.AreEqual(vals.Get("P3"), eventBean.Get("P3"), error);
            Assert.AreEqual(vals.Get("P4"), eventBean.Get("P4"), error);
            Assert.AreEqual(vals.Get("P5"), eventBean.Get("P5"), error);
        }
    
        private void Add(IDictionary<string, IDictionary<string, string>> last, string key, string s0, string s1, string s2, string s3, string s4, string s5) {
            IDictionary<string, string> entry = last.Get(key);
            if (entry == null) {
                entry = new Dictionary<string, string>();
                last.Put(key, entry);
            }
    
            if (s0 != null) {
                entry.Put("P0", s0);
            }
            if (s1 != null) {
                entry.Put("P1", s1);
            }
            if (s2 != null) {
                entry.Put("P2", s2);
            }
            if (s3 != null) {
                entry.Put("P3", s3);
            }
            if (s4 != null) {
                entry.Put("P4", s4);
            }
            if (s5 != null) {
                entry.Put("P5", s5);
            }
        }
    
        private string Next(int num) {
            return Convert.ToString(num);
        }
    }
} // end of namespace
