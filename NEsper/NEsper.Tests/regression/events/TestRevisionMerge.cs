///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.events
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestRevisionMerge  {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerOne;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listenerOne = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _listenerOne = null;
        }
    
        [Test]
        public void TestMergeDeclared() {
            IDictionary<String, Object> fullType = MakeMap(new Object[][]{new Object[] {"P1", typeof(string)}, new Object[] {"P2", typeof(string)}, new Object[] {"P3", typeof(string)}, new Object[] {"Pf", typeof(string)}});
            _epService.EPAdministrator.Configuration.AddEventType("FullType", fullType);
    
            IDictionary<String, Object> deltaType = MakeMap(new Object[][]{new Object[] {"P1", typeof(string)}, new Object[] {"P2", typeof(string)}, new Object[] {"P3", typeof(string)}, new Object[] {"Pd", typeof(string)}});
            _epService.EPAdministrator.Configuration.AddEventType("DeltaType", deltaType);
    
            ConfigurationRevisionEventType revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("FullType");
            revEvent.AddNameDeltaEventType("DeltaType");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_DECLARED;
            revEvent.KeyPropertyNames = (new String[]{"P1"});
            _epService.EPAdministrator.Configuration.AddRevisionEventType("MyExistsRevision", revEvent);
    
            _epService.EPAdministrator.CreateEPL("create window MyWin.win:time(10 sec) as select * from MyExistsRevision");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from FullType");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from DeltaType");
    
            String[] fields = "P1,P2,P3,Pf,Pd".Split(',');
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from MyWin");
            consumerOne.Events += _listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf", "10,20,30,f0"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"10", "20", "30", "f0", null});
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2", "10,21"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "20", "30", "f0", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", null, "f0", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pf", "10,32,f1"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", null, "f0", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, "32", "f1", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd", "10,33,pd3"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, "32", "f1", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, "33", "f1", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf", "10,22,34,f2"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, "33", "f1", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1", "10"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, null, null, "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf,Pd", "10,23,35,pfx,pd4"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, null, null, "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "23", "35", null, "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2", "10,null"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "23", "35", null, "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, null, null, null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd,Pf", "10,36,pdx,f4"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, null, null, null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, "36", "f4", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd", "10,null,pd5"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, "36", "f4", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, null, "f4", "pd5"});
            _listenerOne.Reset();
        }
    
        [Test]
        public void TestMergeNonNull() {
            IDictionary<String, Object> fullType = MakeMap(new Object[][]{new Object[] {"P1", typeof(string)}, new Object[] {"P2", typeof(string)}, new Object[] {"P3", typeof(string)}, new Object[] {"Pf", typeof(string)}});
            _epService.EPAdministrator.Configuration.AddEventType("FullType", fullType);
    
            IDictionary<String, Object> deltaType = MakeMap(new Object[][]{new Object[] {"P1", typeof(string)}, new Object[] {"P2", typeof(string)}, new Object[] {"P3", typeof(string)}, new Object[] {"Pd", typeof(string)}});
            _epService.EPAdministrator.Configuration.AddEventType("DeltaType", deltaType);
    
            ConfigurationRevisionEventType revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("FullType");
            revEvent.AddNameDeltaEventType("DeltaType");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_NON_NULL;
            revEvent.KeyPropertyNames = (new String[]{"P1"});
            _epService.EPAdministrator.Configuration.AddRevisionEventType("MyExistsRevision", revEvent);
    
            _epService.EPAdministrator.CreateEPL("create window MyWin.win:time(10 sec) as select * from MyExistsRevision");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from FullType");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from DeltaType");
    
            String[] fields = "P1,P2,P3,Pf,Pd".Split(',');
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from MyWin");
            consumerOne.Events += _listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf", "10,20,30,f0"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"10", "20", "30", "f0", null});
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2", "10,21"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "20", "30", "f0", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", "30", "f0", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pf", "10,32,f1"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", "30", "f0", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", "32", "f1", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd", "10,33,pd3"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", "32", "f1", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", "33", "f1", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf", "10,22,34,f2"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", "33", "f1", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1", "10"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf,Pd", "10,23,35,pfx,pd4"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "23", "35", "f2", "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2", "10,null"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "23", "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "23", "35", "f2", "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd,Pf", "10,36,pdx,f4"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "23", "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "23", "36", "f4", "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd", "10,null,pd5"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "23", "36", "f4", "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "23", "36", "f4", "pd5"});
            _listenerOne.Reset();
        }
    
        [Test]
        public void TestMergeExists() {
            IDictionary<String, Object> fullType = MakeMap(new Object[][]{new Object[] {"P1", typeof(string)}, new Object[] {"P2", typeof(string)}, new Object[] {"P3", typeof(string)}, new Object[] {"Pf", typeof(string)}});
            _epService.EPAdministrator.Configuration.AddEventType("FullType", fullType);
    
            IDictionary<String, Object> deltaType = MakeMap(new Object[][]{new Object[] {"P1", typeof(string)}, new Object[] {"P2", typeof(string)}, new Object[] {"P3", typeof(string)}, new Object[] {"Pd", typeof(string)}});
            _epService.EPAdministrator.Configuration.AddEventType("DeltaType", deltaType);
    
            ConfigurationRevisionEventType revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("FullType");
            revEvent.AddNameDeltaEventType("DeltaType");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_EXISTS;
            revEvent.KeyPropertyNames = (new String[]{"P1"});
            _epService.EPAdministrator.Configuration.AddRevisionEventType("MyExistsRevision", revEvent);
    
            _epService.EPAdministrator.CreateEPL("create window MyWin.win:time(10 sec) as select * from MyExistsRevision");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from FullType");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from DeltaType");
    
            String[] fields = "P1,P2,P3,Pf,Pd".Split(',');
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL("select irstream * from MyWin");
            consumerOne.Events += _listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf", "10,20,30,f0"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"10", "20", "30", "f0", null});
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2", "10,21"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "20", "30", "f0", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", "30", "f0", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pf", "10,32,f1"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", "30", "f0", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", "32", "f1", null});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd", "10,33,pd3"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", "32", "f1", null});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "21", "33", "f1", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf", "10,22,34,f2"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "21", "33", "f1", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1", "10"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2,P3,Pf,Pd", "10,23,35,pfx,pd4"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", "23", "35", "f2", "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P2", "10,null"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", "23", "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, "35", "f2", "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd,Pf", "10,36,pdx,f4"), "FullType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, "36", "f4", "pd4"});
            _listenerOne.Reset();
    
            _epService.EPRuntime.SendEvent(MakeMap("P1,P3,Pd", "10,null,pd5"), "DeltaType");
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[]{"10", null, "36", "f4", "pd4"});
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[]{"10", null, null, "f4", "pd5"});
            _listenerOne.Reset();
        }
    
        [Test]
        public void TestNestedPropertiesNoDelta() {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanComplexProps>("Nested");
    
            ConfigurationRevisionEventType revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("Nested");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_DECLARED;
            revEvent.KeyPropertyNames = (new String[]{"SimpleProperty"});
            _epService.EPAdministrator.Configuration.AddRevisionEventType("NestedRevision", revEvent);
    
            _epService.EPAdministrator.CreateEPL("create window MyWin.win:time(10 sec) as select * from NestedRevision");
            _epService.EPAdministrator.CreateEPL("insert into MyWin select * from Nested");
    
            String[] fields = "key,f1".Split(',');
            String stmtText = "select irstream SimpleProperty as key, Nested.NestedValue as f1 from MyWin";
            EPStatement consumerOne = _epService.EPAdministrator.CreateEPL(stmtText);
            consumerOne.Events += _listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            EPAssertionUtil.AssertProps(_listenerOne.AssertOneGetNewAndReset(), fields, new Object[]{"Simple", "NestedValue"});
    
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
            bean.Nested.NestedValue = "val2";
            _epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(_listenerOne.LastOldData[0], fields, new Object[] { "Simple", "NestedValue" });
            EPAssertionUtil.AssertProps(_listenerOne.LastNewData[0], fields, new Object[] { "Simple", "val2" });
            _listenerOne.Reset();
        }
    
        private IDictionary<String, Object> MakeMap(Object[][] entries) {
            Map result = new Dictionary<String, Object>();
            for (int i = 0; i < entries.Length; i++) {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    
        private IDictionary<String, Object> MakeMap(String keysList, String valuesList) {
            String[] keys = keysList.Split(',');
            String[] values = valuesList.Split(',');
    
            Map result = new Dictionary<String, Object>();
            for (int i = 0; i < keys.Length; i++) {
                if (values[i].Equals("null")) {
                    result.Put(keys[i], null);
                } else {
                    result.Put(keys[i], values[i]);
                }
            }
            return result;
        }
    }
}
