///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowViews : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var types = new Dictionary<string, object>();
            types.Put("key", typeof(string));
            types.Put("value", typeof(long));
    
            configuration.AddEventType("MyMap", types);
            configuration.EngineDefaults.EventMeta.AvroSettings.ObjectValueTypeWidenerFactoryClass = typeof(MyAvroTypeWidenerFactory).FullName;
            configuration.EngineDefaults.EventMeta.AvroSettings.TypeRepresentationMapperClass = typeof(MyAvroTypeRepMapper).FullName;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionBeanBacked(epService);
            RunAssertionBeanContained(epService);
            RunAssertionIntersection(epService);
            RunAssertionBeanSchemaBacked(epService);
            RunAssertionDeepSupertypeInsert(epService);
            RunAssertionOnInsertPremptiveTwoWindow(epService);
            RunAssertionWithDeleteUseAs(epService);
            RunAssertionWithDeleteFirstAs(epService);
            RunAssertionWithDeleteSecondAs(epService);
            RunAssertionWithDeleteNoAs(epService);
            RunAssertionTimeWindow(epService);
            RunAssertionTimeFirstWindow(epService);
            RunAssertionExtTimeWindow(epService);
            RunAssertionTimeOrderWindow(epService);
            RunAssertionLengthWindow(epService);
            RunAssertionLengthFirstWindow(epService);
            RunAssertionTimeAccum(epService);
            RunAssertionTimeBatch(epService);
            RunAssertionTimeBatchLateConsumer(epService);
            RunAssertionLengthBatch(epService);
            RunAssertionSortWindow(epService);
            RunAssertionTimeLengthBatch(epService);
            RunAssertionLengthWindowPerGroup(epService);
            RunAssertionTimeBatchPerGroup(epService);
            RunAssertionDoubleInsertSameWindow(epService);
            RunAssertionLastEvent(epService);
            RunAssertionFirstEvent(epService);
            RunAssertionUnique(epService);
            RunAssertionFirstUnique(epService);
            RunAssertionFilteringConsumer(epService);
            RunAssertionSelectGroupedViewLateStart(epService);
            RunAssertionSelectGroupedViewLateStartVariableIterate(epService);
            RunAssertionFilteringConsumerLateStart(epService);
            RunAssertionInvalid(epService);
            RunAssertionAlreadyExists(epService);
            RunAssertionConsumerDataWindow(epService);
            RunAssertionPriorStats(epService);
            RunAssertionLateConsumer(epService);
            RunAssertionLateConsumerJoin(epService);
            RunAssertionPattern(epService);
            RunAssertionExternallyTimedBatch(epService);
        }
    
        private void RunAssertionBeanBacked(EPServiceProvider epService) {
            TryAssertionBeanBacked(epService, EventRepresentationChoice.ARRAY);
            TryAssertionBeanBacked(epService, EventRepresentationChoice.MAP);
            TryAssertionBeanBacked(epService, EventRepresentationChoice.DEFAULT);
    
            try {
                TryAssertionBeanBacked(epService, EventRepresentationChoice.AVRO);
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Avro event type does not allow contained beans");
            }
        }
    
        private void RunAssertionBeanContained(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                if (!rep.IsAvroEvent()) {
                    TryAssertionBeanContained(epService, rep);
                }
            }
    
            try {
                TryAssertionBeanContained(epService, EventRepresentationChoice.AVRO);
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Avro event type does not allow contained beans");
            }
        }
    
        private void TryAssertionBeanContained(EPServiceProvider epService, EventRepresentationChoice rep) {
            EPStatement stmtW = epService.EPAdministrator.CreateEPL(rep.GetAnnotationText() + " create window MyWindowBC#keepall as (bean " + typeof(SupportBean_S0).FullName + ")");
            var listenerWindow = new SupportUpdateListener();
            stmtW.Events += listenerWindow.Update;
            Assert.IsTrue(rep.MatchesClass(stmtW.EventType.UnderlyingType));
            epService.EPAdministrator.CreateEPL("insert into MyWindowBC select bean.* as bean from " + typeof(SupportBean_S0).FullName + " as bean");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), "bean.p00".Split(','), new object[]{"E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowBC", true);
        }
    
        private void RunAssertionIntersection(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                    "create window MyWindowINT#length(2)#unique(IntPrimitive) as SupportBean;\n" +
                            "insert into MyWindowINT select * from SupportBean;\n" +
                            "@Name('out') select irstream * from MyWindowINT");
    
            string[] fields = "TheString".Split(',');
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields, new[] {new object[] {"E1"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields, new[] {new object[] {"E2"}}, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listener.AssertInvokedAndReset(), fields, new[] {new object[] {"E3"}}, new[] {new object[] {"E1"}, new object[] {"E2"}});
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    
        private void TryAssertionBeanBacked(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
    
            // Test create from class
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindowBB#keepall as SupportBean");
            var listenerWindow = new SupportUpdateListener();
            stmt.Events += listenerWindow.Update;
            epService.EPAdministrator.CreateEPL("insert into MyWindowBB select * from SupportBean");
    
            EPStatementSPI stmtConsume = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from MyWindowBB");
            Assert.IsTrue(stmtConsume.StatementContext.IsStatelessSelect);
            var listenerStmtOne = new SupportUpdateListener();
            stmtConsume.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            AssertEvent(listenerWindow.AssertOneGetNewAndReset(), "MyWindowBB");
            AssertEvent(listenerStmtOne.AssertOneGetNewAndReset(), "MyWindowBB");
    
            EPStatement stmtUpdate = epService.EPAdministrator.CreateEPL("on SupportBean_A update MyWindowBB set TheString='s'");
            var listenerStmtTwo = new SupportUpdateListener();
            stmtUpdate.Events += listenerStmtTwo.Update;
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            AssertEvent(listenerStmtTwo.LastNewData[0], "MyWindowBB");
    
            // test bean-property
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyWindowBB", true);
            listenerWindow.Reset();
            listenerStmtOne.Reset();
        }
    
        private void RunAssertionBeanSchemaBacked(EPServiceProvider epService) {
    
            // Test create from schema
            epService.EPAdministrator.CreateEPL("create schema ABC as " + typeof(SupportBean).FullName);
            epService.EPAdministrator.CreateEPL("create window MyWindowBSB#keepall as ABC");
            epService.EPAdministrator.CreateEPL("insert into MyWindowBSB select * from " + typeof(SupportBean).FullName);
    
            epService.EPRuntime.SendEvent(new SupportBean());
            AssertEvent(epService.EPRuntime.ExecuteQuery("select * from MyWindowBSB").Array[0], "MyWindowBSB");
    
            var stmtABC = epService.EPAdministrator.CreateEPL("select * from ABC");
            var listenerStmtOne = new SupportUpdateListener();
            stmtABC.Events += listenerStmtOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean());
            Assert.IsTrue(listenerStmtOne.IsInvoked);
        }
    
        private void AssertEvent(EventBean theEvent, string name) {
            Assert.IsTrue(theEvent.EventType is BeanEventType);
            Assert.IsTrue(theEvent.Underlying is SupportBean);
            Assert.AreEqual(TypeClass.NAMED_WINDOW, ((EventTypeSPI) theEvent.EventType).Metadata.TypeClass);
            Assert.AreEqual(name, theEvent.EventType.Name);
        }
    
        private void RunAssertionDeepSupertypeInsert(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportOverrideOneA", typeof(SupportOverrideOneA));
            epService.EPAdministrator.Configuration.AddEventType("SupportOverrideOne", typeof(SupportOverrideOne));
            epService.EPAdministrator.Configuration.AddEventType("SupportOverrideBase", typeof(SupportOverrideBase));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyWindowDSI#keepall as select * from SupportOverrideBase");
            epService.EPAdministrator.CreateEPL("insert into MyWindowDSI select * from SupportOverrideOneA");
            epService.EPRuntime.SendEvent(new SupportOverrideOneA("1a", "1", "base"));
            Assert.AreEqual("1a", stmt.First().Get("val"));
        }
    
        // Assert the named window is updated at the time that a subsequent event queries the named window
        private void RunAssertionOnInsertPremptiveTwoWindow(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema TypeOne(col1 int)");
            epService.EPAdministrator.CreateEPL("create schema TypeTwo(col2 int)");
            epService.EPAdministrator.CreateEPL("create schema TypeTrigger(trigger int)");
            epService.EPAdministrator.CreateEPL("create schema SupportBean as " + typeof(SupportBean).FullName);
    
            epService.EPAdministrator.CreateEPL("create window WinOne#keepall as TypeOne");
            epService.EPAdministrator.CreateEPL("create window WinTwo#keepall as TypeTwo");
    
            epService.EPAdministrator.CreateEPL("insert into WinOne(col1) select IntPrimitive from SupportBean");
    
            epService.EPAdministrator.CreateEPL("on TypeTrigger insert into OtherStream select col1 from WinOne");
            epService.EPAdministrator.CreateEPL("on TypeTrigger insert into WinTwo(col2) select col1 from WinOne");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on OtherStream select col2 from WinTwo");
            var listenerStmtOne = new SupportUpdateListener();
            stmt.Events += listenerStmtOne.Update;
    
            // populate WinOne
            epService.EPRuntime.SendEvent(new SupportBean("E1", 9));
    
            // fire trigger
            if (EventRepresentationChoiceExtensions.GetEngineDefault(epService).IsObjectArrayEvent()) {
                epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new object[0]);
            } else {
                epService.EPRuntime.GetEventSender("TypeTrigger").SendEvent(new Dictionary<string, object>());
            }
    
            Assert.AreEqual(9, listenerStmtOne.AssertOneGetNewAndReset().Get("col2"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWithDeleteUseAs(EPServiceProvider epService) {
            TryCreateWindow(epService, "create window MyWindow#keepall as MyMap",
                    "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow as s1 where s0.symbol = s1.key");
        }
    
        private void RunAssertionWithDeleteFirstAs(EPServiceProvider epService) {
            TryCreateWindow(epService, "create window MyWindow#keepall as select key, value from MyMap",
                    "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow as s1 where symbol = s1.key");
        }
    
        private void RunAssertionWithDeleteSecondAs(EPServiceProvider epService) {
            TryCreateWindow(epService, "create window MyWindow#keepall as MyMap",
                    "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindow where s0.symbol = key");
        }
    
        private void RunAssertionWithDeleteNoAs(EPServiceProvider epService) {
            TryCreateWindow(epService, "create window MyWindow#keepall as select key as key, value as value from MyMap",
                    "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindow where symbol = key");
        }
    
        private void TryCreateWindow(EPServiceProvider epService, string createWindowStatement, string deleteStatement) {
            var fields = new[]{"key", "value"};
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(createWindowStatement);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindow select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            string stmtTextSelectOne = "select irstream key, value*2 as value from MyWindow";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            string stmtTextSelectTwo = "select irstream key, sum(value) as value from MyWindow group by key";
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            var listenerStmtTwo = new SupportUpdateListener();
            stmtSelectTwo.Events += listenerStmtTwo.Update;
    
            string stmtTextSelectThree = "select irstream key, value from MyWindow where value >= 10";
            EPStatement stmtSelectThree = epService.EPAdministrator.CreateEPL(stmtTextSelectThree);
            var listenerStmtThree = new SupportUpdateListener();
            stmtSelectThree.Events += listenerStmtThree.Update;
    
            // send events
            SendSupportBean(epService, "E1", 10L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastOldData[0], fields, new object[]{"E1", null});
            listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(listenerStmtThree.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {"E1", 20L}});
    
            SendSupportBean(epService, "E2", 20L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastOldData[0], fields, new object[]{"E2", null});
            listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(listenerStmtThree.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}, new object[] {"E2", 20L}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {"E1", 20L}, new object[] {"E2", 40L}});
    
            SendSupportBean(epService, "E3", 5L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 10L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fields, new object[]{"E3", 5L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastOldData[0], fields, new object[]{"E3", null});
            listenerStmtTwo.Reset();
            Assert.IsFalse(listenerStmtThree.IsInvoked);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 5L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}, new object[] {"E2", 20L}, new object[] {"E3", 5L}});
    
            // create delete stmt
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(deleteStatement);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            // send delete event
            SendMarketBean(epService, "E1");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 20L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fields, new object[]{"E1", null});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastOldData[0], fields, new object[]{"E1", 10L});
            listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(listenerStmtThree.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 20L}, new object[] {"E3", 5L}});
    
            // send delete event again, none deleted now
            SendMarketBean(epService, "E1");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            Assert.IsFalse(listenerStmtTwo.IsInvoked);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsTrue(listenerStmtDelete.IsInvoked);
            listenerStmtDelete.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 20L}, new object[] {"E3", 5L}});
    
            // send delete event
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 40L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fields, new object[]{"E2", null});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastOldData[0], fields, new object[]{"E2", 20L});
            listenerStmtTwo.Reset();
            EPAssertionUtil.AssertProps(listenerStmtThree.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 5L}});
    
            // send delete event
            SendMarketBean(epService, "E3");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E3", 10L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fields, new object[]{"E3", null});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastOldData[0], fields, new object[]{"E3", 5L});
            listenerStmtTwo.Reset();
            Assert.IsFalse(listenerStmtThree.IsInvoked);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E3", 5L});
            Assert.IsTrue(listenerStmtDelete.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            stmtSelectOne.Dispose();
            stmtSelectTwo.Dispose();
            stmtSelectThree.Dispose();
            stmtDelete.Dispose();
            stmtInsert.Dispose();
            stmtCreate.Dispose();
        }
    
        private void RunAssertionTimeWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowTW#time(10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowTW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowTW as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendTimer(epService, 1000);
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            SendTimer(epService, 10000);
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // Should push out the window
            SendTimer(epService, 10999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 2L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // nothing pushed
            SendTimer(epService, 15000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            // push last event
            SendTimer(epService, 19999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            SendTimer(epService, 20000);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E4", 4L}});
    
            // delete E4
            SendMarketBean(epService, "E4");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 100000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeFirstWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            SendTimer(epService, 1000);
    
            // create window
            string stmtTextCreate = "create window MyWindowTFW#firsttime(10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTFW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowTFW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowTFW as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            SendTimer(epService, 10000);
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // Should not push out the window
            SendTimer(epService, 12000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            SendSupportBean(epService, "E4", 4L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            // nothing pushed
            SendTimer(epService, 100000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionExtTimeWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowETW#ext_timed(value, 10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowETW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowETW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindowETW where symbol = key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1000L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1000L}});
    
            SendSupportBean(epService, "E2", 5000L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 5000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 5000L});
    
            SendSupportBean(epService, "E3", 10000L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 10000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 10000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1000L}, new object[] {"E2", 5000L}, new object[] {"E3", 10000L}});
    
            // Should push out the window
            SendSupportBean(epService, "E4", 11000L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E4", 11000L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E1", 1000L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E4", 11000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E1", 1000L});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 5000L}, new object[] {"E3", 10000L}, new object[] {"E4", 11000L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 5000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 5000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 10000L}, new object[] {"E4", 11000L}});
    
            // nothing pushed other then E5 (E2 is deleted)
            SendSupportBean(epService, "E5", 15000L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E5", 15000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E5", 15000L});
            Assert.IsNull(listenerWindow.LastOldData);
            Assert.IsNull(listenerStmtOne.LastOldData);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeOrderWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowTOW#time_order(value, 10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTOW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowTOW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindowTOW where symbol = key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E1", 3000L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 3000L});
    
            SendTimer(epService, 6000);
            SendSupportBean(epService, "E2", 2000L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2000L});
    
            SendTimer(epService, 10000);
            SendSupportBean(epService, "E3", 1000L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 1000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 1000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 1000L}, new object[] {"E2", 2000L}, new object[] {"E1", 3000L}});
    
            // Should push out the window
            SendTimer(epService, 11000);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E3", 1000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E3", 1000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 2000L}, new object[] {"E1", 3000L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 3000L}});
    
            SendTimer(epService, 12999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 13000);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 3000L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E1", 3000L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 100000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLengthWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowLW#length(3) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowLW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowLW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindowLW where symbol = key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
    
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
    
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            SendSupportBean(epService, "E5", 5L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E5", 5L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E1", 1L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E5", 5L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E1", 1L});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 3L}, new object[] {"E4", 4L}, new object[] {"E5", 5L}});
    
            SendSupportBean(epService, "E6", 6L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E6", 6L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E3", 3L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E6", 6L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E3", 3L});
            listenerStmtOne.Reset();
    
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLengthFirstWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowLFW#firstlength(2) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowLFW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowLFW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindowLFW where symbol = key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
    
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
    
            SendSupportBean(epService, "E3", 3L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E4", 4L}});
    
            SendSupportBean(epService, "E5", 5L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E4", 4L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeAccum(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowTA#time_accum(10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTA select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowTA";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowTA as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendTimer(epService, 1000);
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2L});
    
            SendTimer(epService, 10000);
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3L});
    
            SendTimer(epService, 15000);
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}, new object[] {"E4", 4L}});
    
            // nothing pushed
            SendTimer(epService, 24999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 25000);
            Assert.IsNull(listenerWindow.LastNewData);
            EventBean[] oldData = listenerWindow.LastOldData;
            Assert.AreEqual(3, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(oldData[2], fields, new object[]{"E4", 4L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            // delete E4
            SendMarketBean(epService, "E4");
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 30000);
            SendSupportBean(epService, "E5", 5L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E5", 5L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E5", 5L});
    
            SendTimer(epService, 31000);
            SendSupportBean(epService, "E6", 6L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E6", 6L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E6", 6L});
    
            SendTimer(epService, 38000);
            SendSupportBean(epService, "E7", 7L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E7", 7L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E7", 7L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E5", 5L}, new object[] {"E6", 6L}, new object[] {"E7", 7L}});
    
            // delete E7 - deleting the last should spit out the first 2 timely
            SendMarketBean(epService, "E7");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E7", 7L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E7", 7L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E5", 5L}, new object[] {"E6", 6L}});
    
            SendTimer(epService, 40999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 41000);
            Assert.IsNull(listenerStmtOne.LastNewData);
            oldData = listenerStmtOne.LastOldData;
            Assert.AreEqual(2, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new object[]{"E5", 5L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new object[]{"E6", 6L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 50000);
            SendSupportBean(epService, "E8", 8L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E8", 8L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E8", 8L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E8", 8L}});
    
            SendTimer(epService, 55000);
            SendMarketBean(epService, "E8");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E8", 8L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E8", 8L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 100000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeBatch(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowTB#time_batch(10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTB select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindowTB";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowTB as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendTimer(epService, 1000);
            SendSupportBean(epService, "E1", 1L);
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E2", 2L);
    
            SendTimer(epService, 10000);
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}, new object[] {"E3", 3L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            // nothing pushed
            SendTimer(epService, 10999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 11000);
            Assert.IsNull(listenerWindow.LastOldData);
            EventBean[] newData = listenerWindow.LastNewData;
            Assert.AreEqual(2, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(newData[1], fields, new object[]{"E3", 3L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 21000);
            Assert.IsNull(listenerWindow.LastNewData);
            EventBean[] oldData = listenerWindow.LastOldData;
            Assert.AreEqual(2, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new object[]{"E3", 3L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
    
            // send and delete E4, leaving an empty batch
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E4", 4L}});
    
            SendMarketBean(epService, "E4");
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 31000);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeBatchLateConsumer(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            // create window
            string stmtTextCreate = "create window MyWindowTBLC#time_batch(10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTBLC select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendTimer(epService, 0);
            SendSupportBean(epService, "E1", 1L);
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E2", 2L);
    
            // create consumer
            string stmtTextSelectOne = "select sum(value) as value from MyWindowTBLC";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendTimer(epService, 8000);
            SendSupportBean(epService, "E3", 3L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 10000);
            EventBean[] newData = listenerStmtOne.LastNewData;
            Assert.AreEqual(1, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], new[]{"value"}, new object[]{6L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), new[]{"value"}, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLengthBatch(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowLB#length_batch(3) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowLB select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindowLB";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowLB as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            SendSupportBean(epService, "E2", 2L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendSupportBean(epService, "E3", 3L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            SendSupportBean(epService, "E4", 4L);
            Assert.IsNull(listenerWindow.LastOldData);
            EventBean[] newData = listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(newData[1], fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(newData[2], fields, new object[]{"E4", 4L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E5", 5L);
            SendSupportBean(epService, "E6", 6L);
            SendMarketBean(epService, "E5");
            SendMarketBean(epService, "E6");
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E7", 7L);
            SendSupportBean(epService, "E8", 8L);
            SendSupportBean(epService, "E9", 9L);
            EventBean[] oldData = listenerWindow.LastOldData;
            newData = listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            Assert.AreEqual(3, oldData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new object[]{"E7", 7L});
            EPAssertionUtil.AssertProps(newData[1], fields, new object[]{"E8", 8L});
            EPAssertionUtil.AssertProps(newData[2], fields, new object[]{"E9", 9L});
            EPAssertionUtil.AssertProps(oldData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(oldData[2], fields, new object[]{"E4", 4L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
    
            SendSupportBean(epService, "E10", 10L);
            SendSupportBean(epService, "E10", 11L);
            SendMarketBean(epService, "E10");
    
            SendSupportBean(epService, "E21", 21L);
            SendSupportBean(epService, "E22", 22L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            SendSupportBean(epService, "E23", 23L);
            oldData = listenerWindow.LastOldData;
            newData = listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            Assert.AreEqual(3, oldData.Length);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSortWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowSW#sort(3, value asc) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowSW select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindowSW";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowSW as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 10L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10L});
    
            SendSupportBean(epService, "E2", 20L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 20L});
    
            SendSupportBean(epService, "E3", 15L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 15L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E2", 20L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}});
    
            SendSupportBean(epService, "E4", 18L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 18L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E4", 18L}});
    
            SendSupportBean(epService, "E5", 17L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E5", 17L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E4", 18L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 10L}, new object[] {"E3", 15L}, new object[] {"E5", 17L}});
    
            // delete E1
            SendMarketBean(epService, "E1");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 15L}, new object[] {"E5", 17L}});
    
            SendSupportBean(epService, "E6", 16L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E6", 16L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 15L}, new object[] {"E6", 16L}, new object[] {"E5", 17L}});
    
            SendSupportBean(epService, "E7", 16L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E7", 16L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E5", 17L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 15L}, new object[] {"E7", 16L}, new object[] {"E6", 16L}});
    
            // delete E7 has no effect
            SendMarketBean(epService, "E7");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E7", 16L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 15L}, new object[] {"E6", 16L}});
    
            SendSupportBean(epService, "E8", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E8", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E8", 1L}, new object[] {"E3", 15L}, new object[] {"E6", 16L}});
    
            SendSupportBean(epService, "E9", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E9", 1L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E6", 16L});
            listenerWindow.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeLengthBatch(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowTLB#time_length_batch(10 sec, 3) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTLB select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindowTLB";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowTLB as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendTimer(epService, 1000);
            SendSupportBean(epService, "E1", 1L);
            SendSupportBean(epService, "E2", 2L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendSupportBean(epService, "E3", 3L);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 3L}});
    
            SendSupportBean(epService, "E4", 4L);
            Assert.IsNull(listenerWindow.LastOldData);
            EventBean[] newData = listenerWindow.LastNewData;
            Assert.AreEqual(3, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(newData[1], fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(newData[2], fields, new object[]{"E4", 4L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendTimer(epService, 5000);
            SendSupportBean(epService, "E5", 5L);
            SendSupportBean(epService, "E6", 6L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E5", 5L}, new object[] {"E6", 6L}});
    
            SendMarketBean(epService, "E5");   // deleting E5
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E6", 6L}});
    
            SendTimer(epService, 10999);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendTimer(epService, 11000);
            newData = listenerWindow.LastNewData;
            Assert.AreEqual(1, newData.Length);
            EPAssertionUtil.AssertProps(newData[0], fields, new object[]{"E6", 6L});
            EventBean[] oldData = listenerWindow.LastOldData;
            Assert.AreEqual(3, oldData.Length);
            EPAssertionUtil.AssertProps(oldData[0], fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(oldData[1], fields, new object[]{"E3", 3L});
            EPAssertionUtil.AssertProps(oldData[2], fields, new object[]{"E4", 4L});
            listenerWindow.Reset();
            listenerStmtOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLengthWindowPerGroup(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowWPG#groupwin(value)#length(2) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowWPG select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowWPG";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " delete from MyWindowWPG where symbol = key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1L});
    
            SendSupportBean(epService, "E2", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E2", 1L});
    
            SendSupportBean(epService, "E3", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E3", 2L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E2", 1L}, new object[] {"E3", 2L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E2", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}, new object[] {"E3", 2L}});
    
            SendSupportBean(epService, "E4", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E4", 1L});
    
            SendSupportBean(epService, "E5", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E5", 1L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E1", 1L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E5", 1L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E1", 1L});
            listenerStmtOne.Reset();
    
            SendSupportBean(epService, "E6", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E6", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E6", 2L});
    
            // delete E6
            SendMarketBean(epService, "E6");
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E6", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"E6", 2L});
    
            SendSupportBean(epService, "E7", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E7", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"E7", 2L});
    
            SendSupportBean(epService, "E8", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E8", 2L});
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"E3", 2L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E8", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E3", 2L});
            listenerStmtOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTimeBatchPerGroup(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            SendTimer(epService, 0);
            string stmtTextCreate = "create window MyWindowTBPG#groupwin(value)#time_batch(10 sec) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowTBPG select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindowTBPG";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendTimer(epService, 1000);
            SendSupportBean(epService, "E1", 10L);
            SendSupportBean(epService, "E2", 20L);
            SendSupportBean(epService, "E3", 20L);
            SendSupportBean(epService, "E4", 10L);
    
            SendTimer(epService, 11000);
            Assert.AreEqual(listenerWindow.LastNewData.Length, 4);
            Assert.AreEqual(listenerStmtOne.LastNewData.Length, 4);
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[1], fields, new object[]{"E4", 10L});
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[2], fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[3], fields, new object[]{"E3", 20L});
            listenerWindow.Reset();
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E1", 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[1], fields, new object[]{"E4", 10L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[2], fields, new object[]{"E2", 20L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[3], fields, new object[]{"E3", 20L});
            listenerStmtOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDoubleInsertSameWindow(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowDISM#keepall as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowDISM select TheString as key, LongBoxed+1 as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            stmtTextInsert = "insert into MyWindowDISM select TheString as key, LongBoxed+2 as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select key, value as value from MyWindowDISM";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            SendSupportBean(epService, "E1", 10L);
            Assert.AreEqual(2, listenerWindow.NewDataList.Count);    // listener to window gets 2 individual events
            Assert.AreEqual(2, listenerStmtOne.NewDataList.Count);   // listener to statement gets 1 individual event
            Assert.AreEqual(2, listenerWindow.GetNewDataListFlattened().Length);
            Assert.AreEqual(2, listenerStmtOne.GetNewDataListFlattened().Length);
            EPAssertionUtil.AssertPropsPerRow(listenerStmtOne.GetNewDataListFlattened(), fields, new[] {new object[] {"E1", 11L}, new object[] {"E1", 12L}});
            listenerStmtOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLastEvent(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowLE#lastevent as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowLE select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowLE";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowLE as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E1", 1L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E2", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E1", 1L});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E2", 2L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E2", 2L});
            Assert.IsNull(listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E3", 3L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 3L}});
    
            // delete E3
            SendMarketBean(epService, "E3");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E3", 3L});
            Assert.IsNull(listenerStmtOne.LastNewData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E4", 4L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E4", 4L}});
    
            // delete other event
            SendMarketBean(epService, "E1");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFirstEvent(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowFE#firstevent as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowFE select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowFE";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowFE as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E1", 1L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            SendSupportBean(epService, "E2", 2L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1L}});
    
            // delete E2
            SendMarketBean(epService, "E1");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E1", 1L});
            Assert.IsNull(listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E3", 3L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E3", 3L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E3", 3L}});
    
            // delete E3
            SendMarketBean(epService, "E2");   // no effect
            SendMarketBean(epService, "E3");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"E3", 3L});
            Assert.IsNull(listenerStmtOne.LastNewData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E4", 4L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"E4", 4L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E4", 4L}});
    
            // delete other event
            SendMarketBean(epService, "E1");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnique(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowUN#unique(key) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowUN select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowUN";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowUN as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "G1", 1L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"G1", 1L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}});
    
            SendSupportBean(epService, "G2", 20L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"G2", 20L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}, new object[] {"G2", 20L}});
    
            // delete G2
            SendMarketBean(epService, "G2");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"G2", 20L});
            Assert.IsNull(listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}});
    
            SendSupportBean(epService, "G1", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"G1", 2L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"G1", 1L});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 2L}});
    
            SendSupportBean(epService, "G2", 21L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"G2", 21L});
            Assert.IsNull(listenerStmtOne.LastOldData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 2L}, new object[] {"G2", 21L}});
    
            SendSupportBean(epService, "G2", 22L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{"G2", 22L});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"G2", 21L});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 2L}, new object[] {"G2", 22L}});
    
            SendMarketBean(epService, "G1");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"G1", 2L});
            Assert.IsNull(listenerStmtOne.LastNewData);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G2", 22L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFirstUnique(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowFU#firstunique(key) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowFU select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowFU";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowFU as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "G1", 1L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"G1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}});
    
            SendSupportBean(epService, "G2", 20L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20L});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}, new object[] {"G2", 20L}});
    
            // delete G2
            SendMarketBean(epService, "G2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"G2", 20L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}});
    
            SendSupportBean(epService, "G1", 2L);  // ignored
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}});
    
            SendSupportBean(epService, "G2", 21L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"G2", 21L});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}, new object[] {"G2", 21L}});
    
            SendSupportBean(epService, "G2", 22L); // ignored
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 1L}, new object[] {"G2", 21L}});
    
            SendMarketBean(epService, "G1");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"G1", 1L});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G2", 21L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilteringConsumer(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowFC#unique(key) as select TheString as key, IntPrimitive as value from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowFC select TheString as key, IntPrimitive as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowFC(value > 0, value < 10)";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowFC as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBeanInt(epService, "G1", 5);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"G1", 5});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"G1", 5});
    
            SendSupportBeanInt(epService, "G1", 15);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{"G1", 5});
            Assert.IsNull(listenerStmtOne.LastNewData);
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertProps(listenerWindow.LastOldData[0], fields, new object[]{"G1", 5});
            EPAssertionUtil.AssertProps(listenerWindow.LastNewData[0], fields, new object[]{"G1", 15});
            listenerWindow.Reset();
    
            // send G2
            SendSupportBeanInt(epService, "G2", 8);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"G2", 8});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"G2", 8});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 15}, new object[] {"G2", 8}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {"G2", 8}});
    
            // delete G2
            SendMarketBean(epService, "G2");
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetOldAndReset(), fields, new object[]{"G2", 8});
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"G2", 8});
    
            // send G3
            SendSupportBeanInt(epService, "G3", -1);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"G3", -1});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"G1", 15}, new object[] {"G3", -1}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, null);
    
            // delete G2
            SendMarketBean(epService, "G3");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"G3", -1});
    
            SendSupportBeanInt(epService, "G1", 6);
            SendSupportBeanInt(epService, "G2", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {"G1", 6}, new object[] {"G2", 7}});
    
            stmtSelectOne.Dispose();
            stmtDelete.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSelectGroupedViewLateStart(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowSGVS#groupwin(TheString, IntPrimitive)#length(9) as select TheString, IntPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowSGVS select TheString, IntPrimitive from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // fill window
            var stringValues = new[]{"c0", "c1", "c2"};
            for (int i = 0; i < stringValues.Length; i++) {
                for (int j = 0; j < 3; j++) {
                    epService.EPRuntime.SendEvent(new SupportBean(stringValues[i], j));
                }
            }
            epService.EPRuntime.SendEvent(new SupportBean("c0", 1));
            epService.EPRuntime.SendEvent(new SupportBean("c1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("c3", 3));
            EventBean[] received = EPAssertionUtil.EnumeratorToArray(stmtCreate.GetEnumerator());
            Assert.AreEqual(12, received.Length);
    
            // create select stmt
            string stmtTextSelect = "select TheString, IntPrimitive, count(*) from MyWindowSGVS group by TheString, IntPrimitive order by TheString, IntPrimitive";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            Assert.AreEqual(10, received.Length);
    
            EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive,count(*)".Split(','),
                    new object[][]{
                        new object[]{"c0", 0, 1L},
                        new object[]{"c0", 1, 2L},
                        new object[]{"c0", 2, 1L},
                        new object[]{"c1", 0, 1L},
                        new object[]{"c1", 1, 1L},
                        new object[]{"c1", 2, 2L},
                        new object[]{"c2", 0, 1L},
                        new object[]{"c2", 1, 1L},
                        new object[]{"c2", 2, 1L},
                        new object[]{"c3", 3, 1L},
                    });
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSelectGroupedViewLateStartVariableIterate(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindowSGVLS#groupwin(TheString, IntPrimitive)#length(9) as select TheString, IntPrimitive, LongPrimitive, BoolPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowSGVLS select TheString, IntPrimitive, LongPrimitive, BoolPrimitive from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create variable
            epService.EPAdministrator.CreateEPL("create variable string var_1_1_1");
            epService.EPAdministrator.CreateEPL("on " + typeof(SupportVariableSetEvent).FullName + "(variableName='var_1_1_1') set var_1_1_1 = value");
    
            // fill window
            var stringValues = new[]{"c0", "c1", "c2"};
            for (int i = 0; i < stringValues.Length; i++) {
                for (int j = 0; j < 3; j++) {
                    var beanX = new SupportBean(stringValues[i], j);
                    beanX.LongPrimitive = j;
                    beanX.BoolPrimitive = true;
                    epService.EPRuntime.SendEvent(beanX);
                }
            }
            // extra record to create non-uniform data
            var bean = new SupportBean("c1", 1);
            bean.LongPrimitive = 10;
            bean.BoolPrimitive = true;
            epService.EPRuntime.SendEvent(bean);
            EventBean[] received = EPAssertionUtil.EnumeratorToArray(stmtCreate.GetEnumerator());
            Assert.AreEqual(10, received.Length);
    
            // create select stmt
            string stmtTextSelect = "select TheString, IntPrimitive, avg(LongPrimitive) as avgLong, count(BoolPrimitive) as cntBool" +
                    " from MyWindowSGVLS group by TheString, IntPrimitive having TheString = var_1_1_1 order by TheString, IntPrimitive";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
    
            // set variable to C0
            epService.EPRuntime.SendEvent(new SupportVariableSetEvent("var_1_1_1", "c0"));
    
            // get iterator results
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            Assert.AreEqual(3, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive,avgLong,cntBool".Split(','),
                    new object[][]{
                            new object[] {"c0", 0, 0.0, 1L},
                            new object[] {"c0", 1, 1.0, 1L},
                            new object[] {"c0", 2, 2.0, 1L},
                    });
    
            // set variable to C1
            epService.EPRuntime.SendEvent(new SupportVariableSetEvent("var_1_1_1", "c1"));
    
            received = EPAssertionUtil.EnumeratorToArray(stmtSelect.GetEnumerator());
            Assert.AreEqual(3, received.Length);
            EPAssertionUtil.AssertPropsPerRow(received, "TheString,IntPrimitive,avgLong,cntBool".Split(','),
                    new object[][]{
                            new object[] {"c1", 0, 0.0, 1L},
                            new object[] {"c1", 1, 5.5, 2L},
                            new object[] {"c1", 2, 2.0, 1L},
                    });
    
            stmtSelect.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionFilteringConsumerLateStart(EPServiceProvider epService) {
            var fields = new[]{"sumvalue"};
    
            // create window
            string stmtTextCreate = "create window MyWindowFCLS#keepall as select TheString as key, IntPrimitive as value from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowFCLS select TheString as key, IntPrimitive as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendSupportBeanInt(epService, "G1", 5);
            SendSupportBeanInt(epService, "G2", 15);
            SendSupportBeanInt(epService, "G3", 2);
    
            // create consumer
            string stmtTextSelectOne = "select irstream sum(value) as sumvalue from MyWindowFCLS(value > 0, value < 10)";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {7}});
    
            SendSupportBeanInt(epService, "G4", 1);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{8});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{7});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {8}});
    
            SendSupportBeanInt(epService, "G5", 20);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {8}});
    
            SendSupportBeanInt(epService, "G6", 9);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{17});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{8});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {17}});
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowFCLS as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendMarketBean(epService, "G4");
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fields, new object[]{16});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fields, new object[]{17});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {16}});
    
            SendMarketBean(epService, "G5");
            Assert.IsFalse(listenerStmtOne.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fields, new[] {new object[] {16}});
    
            stmtSelectOne.Dispose();
            stmtDelete.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            Assert.AreEqual("Error starting statement: Named windows require one or more child views that are data window views [create window MyWindowI1#groupwin(value)#uni(value) as MyMap]",
                    TryInvalid(epService, "create window MyWindowI1#groupwin(value)#uni(value) as MyMap"));
    
            Assert.AreEqual("Named windows require one or more child views that are data window views [create window MyWindowI2 as MyMap]",
                    TryInvalid(epService, "create window MyWindowI2 as MyMap"));
    
            Assert.AreEqual("Named window or table 'dummy' has not been declared [on MyMap delete from dummy]",
                    TryInvalid(epService, "on MyMap delete from dummy"));
    
            epService.EPAdministrator.CreateEPL("create window SomeWindow#keepall as (a int)");
            SupportMessageAssertUtil.TryInvalid(epService, "update SomeWindow set a = 'a' where a = 'b'",
                    "Provided EPL expression is an on-demand query expression (not a continuous query), please use the runtime executeQuery API instead");
            SupportMessageAssertUtil.TryInvalidExecuteQuery(epService, "update istream SomeWindow set a = 'a' where a = 'b'",
                    "Provided EPL expression is a continuous query expression (not an on-demand query), please use the administrator createEPL API instead");
    
            // test model-after with no field
            var innerType = new Dictionary<string, object>();
            innerType.Put("key", typeof(string));
            epService.EPAdministrator.Configuration.AddEventType("InnerMap", innerType);
            var outerType = new Dictionary<string, object>();
            outerType.Put("innermap", "InnerMap");
            epService.EPAdministrator.Configuration.AddEventType("OuterMap", outerType);
            try {
                epService.EPAdministrator.CreateEPL("create window MyWindowI3#keepall as select innermap.abc from OuterMap");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Failed to validate select-clause expression 'innermap.abc': Failed to resolve property 'innermap.abc' to a stream or nested property in a stream [create window MyWindowI3#keepall as select innermap.abc from OuterMap]", ex.Message);
            }
        }
    
        private void RunAssertionAlreadyExists(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindowAE#keepall as MyMap");
            try {
                epService.EPAdministrator.CreateEPL("create window MyWindowAE#keepall as MyMap");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: A named window by name 'MyWindowAE' has already been created [create window MyWindowAE#keepall as MyMap]", ex.Message);
            }
        }
    
        private void RunAssertionConsumerDataWindow(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindowCDW#keepall as MyMap");
            try {
                epService.EPAdministrator.CreateEPL("select key, value as value from MyWindowCDW#time(10 sec)");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Error starting statement: Consuming statements to a named window cannot declare a data window view onto the named window [select key, value as value from MyWindowCDW#time(10 sec)]", ex.Message);
            }
        }
    
        private string TryInvalid(EPServiceProvider epService, string expression) {
            try {
                epService.EPAdministrator.CreateEPL(expression);
                Assert.Fail();
            } catch (EPException ex) {
                return ex.Message;
            }
            return null;
        }
    
        private void RunAssertionPriorStats(EPServiceProvider epService) {
            var fieldsPrior = new[]{"priorKeyOne", "priorKeyTwo"};
            var fieldsStat = new[]{"average"};
    
            string stmtTextCreate = "create window MyWindowPS#keepall as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindowPS select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            string stmtTextSelectOne = "select prior(1, key) as priorKeyOne, prior(2, key) as priorKeyTwo from MyWindowPS";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            string stmtTextSelectThree = "select average from MyWindowPS#uni(value)";
            EPStatement stmtSelectThree = epService.EPAdministrator.CreateEPL(stmtTextSelectThree);
            var listenerStmtThree = new SupportUpdateListener();
            stmtSelectThree.Events += listenerStmtThree.Update;
    
            // send events
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new object[]{null, null});
            EPAssertionUtil.AssertProps(listenerStmtThree.LastNewData[0], fieldsStat, new object[]{1d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new[] {new object[] {1d}});
    
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"E1", null});
            EPAssertionUtil.AssertProps(listenerStmtThree.LastNewData[0], fieldsStat, new object[]{1.5d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new[] {new object[] {1.5d}});
    
            SendSupportBean(epService, "E3", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"E2", "E1"});
            EPAssertionUtil.AssertProps(listenerStmtThree.LastNewData[0], fieldsStat, new object[]{5 / 3d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new[] {new object[] {5 / 3d}});
    
            SendSupportBean(epService, "E4", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fieldsPrior, new object[]{"E3", "E2"});
            EPAssertionUtil.AssertProps(listenerStmtThree.LastNewData[0], fieldsStat, new object[]{1.75});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectThree.GetEnumerator(), fieldsStat, new[] {new object[] {1.75d}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLateConsumer(EPServiceProvider epService) {
            var fieldsWin = new[]{"key", "value"};
            var fieldsStat = new[]{"average"};
            var fieldsCnt = new[]{"cnt"};
    
            string stmtTextCreate = "create window MyWindowLCL#keepall as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindowLCL select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // send events
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"E1", 1L});
    
            SendSupportBean(epService, "E2", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"E2", 2L});
    
            string stmtTextSelectOne = "select irstream average from MyWindowLCL#uni(value)";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new[] {new object[] {1.5d}});
    
            SendSupportBean(epService, "E3", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fieldsStat, new object[]{5 / 3d});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fieldsStat, new object[]{3 / 2d});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new[] {new object[] {5 / 3d}});
    
            SendSupportBean(epService, "E4", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fieldsStat, new object[]{7 / 4d});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new[] {new object[] {7 / 4d}});
    
            string stmtTextSelectTwo = "select count(*) as cnt from MyWindowLCL";
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            var listenerStmtTwo = new SupportUpdateListener();
            stmtSelectTwo.Events += listenerStmtTwo.Update;
            EPAssertionUtil.AssertPropsPerRow(stmtSelectTwo.GetEnumerator(), fieldsCnt, new[] {new object[] {4L}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new[] {new object[] {7 / 4d}});
    
            SendSupportBean(epService, "E5", 3L);
            EPAssertionUtil.AssertProps(listenerStmtOne.LastNewData[0], fieldsStat, new object[]{10 / 5d});
            EPAssertionUtil.AssertProps(listenerStmtOne.LastOldData[0], fieldsStat, new object[]{7 / 4d});
            listenerStmtOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtSelectOne.GetEnumerator(), fieldsStat, new[] {new object[] {10 / 5d}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelectTwo.GetEnumerator(), fieldsCnt, new[] {new object[] {5L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLateConsumerJoin(EPServiceProvider epService) {
            var fieldsWin = new[]{"key", "value"};
            var fieldsJoin = new[]{"key", "value", "symbol"};
    
            string stmtTextCreate = "create window MyWindowLCJ#keepall as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            Assert.AreEqual(typeof(string), stmtCreate.EventType.GetPropertyType("key"));
            Assert.AreEqual(typeof(long), stmtCreate.EventType.GetPropertyType("value"));
    
            string stmtTextInsert = "insert into MyWindowLCJ select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // send events
            SendSupportBean(epService, "E1", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"E1", 1L});
    
            SendSupportBean(epService, "E2", 1L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"E2", 1L});
    
            // This replays into MyWindow
            string stmtTextSelectTwo = "select key, value, symbol from MyWindowLCJ as s0" +
                    " left outer join " + typeof(SupportMarketDataBean).FullName + "#keepall as s1" +
                    " on s0.value = s1.volume";
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(stmtTextSelectTwo);
            var listenerStmtTwo = new SupportUpdateListener();
            stmtSelectTwo.Events += listenerStmtTwo.Update;
            Assert.IsFalse(listenerStmtTwo.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new[] {new object[] {"E1", 1L, null}, new object[] {"E2", 1L, null}});
    
            SendMarketBean(epService, "S1", 1);    // join on long
            Assert.AreEqual(2, listenerStmtTwo.LastNewData.Length);
            if (listenerStmtTwo.LastNewData[0].Get("key").Equals("E1")) {
                EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fieldsJoin, new object[]{"E1", 1L, "S1"});
                EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[1], fieldsJoin, new object[]{"E2", 1L, "S1"});
            } else {
                EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fieldsJoin, new object[]{"E2", 1L, "S1"});
                EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[1], fieldsJoin, new object[]{"E1", 1L, "S1"});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new[] {new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}});
            listenerStmtTwo.Reset();
    
            SendMarketBean(epService, "S2", 2);    // join on long
            Assert.IsFalse(listenerStmtTwo.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new[] {new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}});
    
            SendSupportBean(epService, "E3", 2L);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fieldsWin, new object[]{"E3", 2L});
            EPAssertionUtil.AssertProps(listenerStmtTwo.LastNewData[0], fieldsJoin, new object[]{"E3", 2L, "S2"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelectTwo.GetEnumerator(), fieldsJoin, new[] {new object[] {"E1", 1L, "S1"}, new object[] {"E2", 1L, "S1"}, new object[] {"E3", 2L, "S2"}});
        }
    
        private void RunAssertionPattern(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
            string stmtTextCreate = "create window MyWindowPAT#keepall as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            string stmtTextPattern = "select a.key as key, a.value as value from pattern [every a=MyWindowPAT(key='S1') or a=MyWindowPAT(key='S2')]";
            EPStatement stmtPattern = epService.EPAdministrator.CreateEPL(stmtTextPattern);
            var listenerStmtOne = new SupportUpdateListener();
            stmtPattern.Events += listenerStmtOne.Update;
    
            string stmtTextInsert = "insert into MyWindowPAT select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            SendSupportBean(epService, "E1", 1L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            SendSupportBean(epService, "S1", 2L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 2L});
    
            SendSupportBean(epService, "S1", 3L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S1", 3L});
    
            SendSupportBean(epService, "S2", 4L);
            EPAssertionUtil.AssertProps(listenerStmtOne.AssertOneGetNewAndReset(), fields, new object[]{"S2", 4L});
    
            SendSupportBean(epService, "S1", 1L);
            Assert.IsFalse(listenerStmtOne.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionExternallyTimedBatch(EPServiceProvider epService) {
            var fields = new[]{"key", "value"};
    
            // create window
            string stmtTextCreate = "create window MyWindowETB#ext_timed_batch(value, 10 sec, 0L) as MyMap";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsert = "insert into MyWindowETB select TheString as key, LongBoxed as value from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsert);
    
            // create consumer
            string stmtTextSelectOne = "select irstream key, value as value from MyWindowETB";
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(stmtTextSelectOne);
            var listenerStmtOne = new SupportUpdateListener();
            stmtSelectOne.Events += listenerStmtOne.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportMarketDataBean).FullName + " as s0 delete from MyWindowETB as s1 where s0.symbol = s1.key";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerStmtDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerStmtDelete.Update;
    
            SendSupportBean(epService, "E1", 1000L);
            SendSupportBean(epService, "E2", 8000L);
            SendSupportBean(epService, "E3", 9999L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1000L}, new object[] {"E2", 8000L}, new object[] {"E3", 9999L}});
    
            // delete E2
            SendMarketBean(epService, "E2");
            EPAssertionUtil.AssertPropsPerRow(listenerWindow.AssertInvokedAndReset(), fields, null, new[] {new object[] {"E2", 8000L}});
            EPAssertionUtil.AssertPropsPerRow(listenerStmtOne.AssertInvokedAndReset(), fields, null, new[] {new object[] {"E2", 8000L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}});
    
            SendSupportBean(epService, "E4", 10000L);
            EPAssertionUtil.AssertPropsPerRow(listenerWindow.AssertInvokedAndReset(), fields,
                    new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}}, null);
            EPAssertionUtil.AssertPropsPerRow(listenerStmtOne.AssertInvokedAndReset(), fields,
                    new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}}, null);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E4", 10000L}});
    
            // delete E4
            SendMarketBean(epService, "E4");
            EPAssertionUtil.AssertPropsPerRow(listenerWindow.AssertInvokedAndReset(), fields, null, new[] {new object[] {"E4", 10000L}});
            EPAssertionUtil.AssertPropsPerRow(listenerStmtOne.AssertInvokedAndReset(), fields, null, new[] {new object[] {"E4", 10000L}});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E5", 14000L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E5", 14000L}});
    
            SendSupportBean(epService, "E6", 21000L);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new[] {new object[] {"E6", 21000L}});
            EPAssertionUtil.AssertPropsPerRow(listenerWindow.AssertInvokedAndReset(), fields,
                    new[] {new object[] {"E5", 14000L}}, new[] {new object[] {"E1", 1000L}, new object[] {"E3", 9999L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBean SendSupportBean(EPServiceProvider epService, string theString, long longBoxed) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongBoxed = longBoxed;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private void SendSupportBeanInt(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol) {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendMarketBean(EPServiceProvider epService, string symbol, long volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        public class MyAvroTypeWidenerFactory : ObjectValueTypeWidenerFactory {
            public TypeWidener Make(ObjectValueTypeWidenerFactoryContext context) {
                if (context.GetClazz() == typeof(SupportBean_S0)) {
                    return val => { 
                        var row = new GenericRecord(GetSupportBeanS0Schema().AsRecordSchema());
                        row.Put("p00", ((SupportBean_S0) val).P00);
                        return row;
                    };
                }
                return null;
            }
        }
    
        public class MyAvroTypeRepMapper : TypeRepresentationMapper {
            public object Map(TypeRepresentationMapperContext context) {
                if (context.GetClazz() == typeof(SupportBean_S0)) {
                    return GetSupportBeanS0Schema();
                }
                return null;
            }
        }
    
        public static Schema GetSupportBeanS0Schema() {
            return SchemaBuilder.Record("SupportBean_S0", 
                TypeBuilder.RequiredString("p00"));
        }
    }
} // end of namespace
