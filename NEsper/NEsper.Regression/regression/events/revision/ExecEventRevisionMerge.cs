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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.revision
{
    public class ExecEventRevisionMerge : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionMergeDeclared(epService);
            RunAssertionMergeNonNull(epService);
            RunAssertionMergeExists(epService);
            RunAssertionNestedPropertiesNoDelta(epService);
        }
    
        private void RunAssertionMergeDeclared(EPServiceProvider epService) {
            IDictionary<string, Object> fullType = MakeMap(new object[][]{new object[] {"p1", typeof(string)}, new object[] {"p2", typeof(string)}, new object[] {"p3", typeof(string)}, new object[] {"pf", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("FullTypeOne", fullType);
    
            IDictionary<string, Object> deltaType = MakeMap(new object[][]{new object[] {"p1", typeof(string)}, new object[] {"p2", typeof(string)}, new object[] {"p3", typeof(string)}, new object[] {"pd", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("DeltaTypeOne", deltaType);
    
            var revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("FullTypeOne");
            revEvent.AddNameDeltaEventType("DeltaTypeOne");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_DECLARED;
            revEvent.KeyPropertyNames = new string[]{"p1"};
            epService.EPAdministrator.Configuration.AddRevisionEventType("MyExistsRevisionOne", revEvent);
    
            epService.EPAdministrator.CreateEPL("create window MyWinOne#time(10 sec) as select * from MyExistsRevisionOne");
            epService.EPAdministrator.CreateEPL("insert into MyWinOne select * from FullTypeOne");
            epService.EPAdministrator.CreateEPL("insert into MyWinOne select * from DeltaTypeOne");
    
            string[] fields = "p1,p2,p3,pf,pd".Split(',');
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select irstream * from MyWinOne");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf", "10,20,30,f0"), "FullTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"10", "20", "30", "f0", null});
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2", "10,21"), "DeltaTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "20", "30", "f0", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", null, "f0", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pf", "10,32,f1"), "FullTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", null, "f0", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, "32", "f1", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd", "10,33,pd3"), "DeltaTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, "32", "f1", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, "33", "f1", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf", "10,22,34,f2"), "FullTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, "33", "f1", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1", "10"), "FullTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, null, null, "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf,pd", "10,23,35,pfx,pd4"), "DeltaTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, null, null, "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "23", "35", null, "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2", "10,null"), "DeltaTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "23", "35", null, "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, null, null, null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd,pf", "10,36,pdx,f4"), "FullTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, null, null, null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, "36", "f4", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd", "10,null,pd5"), "DeltaTypeOne");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, "36", "f4", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, null, "f4", "pd5"});
            listenerOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMergeNonNull(EPServiceProvider epService) {
            IDictionary<string, Object> fullType = MakeMap(new object[][]{new object[] {"p1", typeof(string)}, new object[] {"p2", typeof(string)}, new object[] {"p3", typeof(string)}, new object[] {"pf", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("FullTypeTwo", fullType);
    
            IDictionary<string, Object> deltaType = MakeMap(new object[][]{new object[] {"p1", typeof(string)}, new object[] {"p2", typeof(string)}, new object[] {"p3", typeof(string)}, new object[] {"pd", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("DeltaTypeTwo", deltaType);
    
            var revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("FullTypeTwo");
            revEvent.AddNameDeltaEventType("DeltaTypeTwo");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_NON_NULL;
            revEvent.KeyPropertyNames = new string[]{"p1"};
            epService.EPAdministrator.Configuration.AddRevisionEventType("MyExistsRevisionTwo", revEvent);
    
            epService.EPAdministrator.CreateEPL("create window MyWinTwo#time(10 sec) as select * from MyExistsRevisionTwo");
            epService.EPAdministrator.CreateEPL("insert into MyWinTwo select * from FullTypeTwo");
            epService.EPAdministrator.CreateEPL("insert into MyWinTwo select * from DeltaTypeTwo");
    
            string[] fields = "p1,p2,p3,pf,pd".Split(',');
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select irstream * from MyWinTwo");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf", "10,20,30,f0"), "FullTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"10", "20", "30", "f0", null});
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2", "10,21"), "DeltaTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "20", "30", "f0", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", "30", "f0", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pf", "10,32,f1"), "FullTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", "30", "f0", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", "32", "f1", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd", "10,33,pd3"), "DeltaTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", "32", "f1", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", "33", "f1", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf", "10,22,34,f2"), "FullTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", "33", "f1", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1", "10"), "FullTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf,pd", "10,23,35,pfx,pd4"), "DeltaTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "23", "35", "f2", "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2", "10,null"), "DeltaTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "23", "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "23", "35", "f2", "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd,pf", "10,36,pdx,f4"), "FullTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "23", "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "23", "36", "f4", "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd", "10,null,pd5"), "DeltaTypeTwo");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "23", "36", "f4", "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "23", "36", "f4", "pd5"});
            listenerOne.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMergeExists(EPServiceProvider epService) {
            IDictionary<string, Object> fullType = MakeMap(new object[][]{new object[] {"p1", typeof(string)}, new object[] {"p2", typeof(string)}, new object[] {"p3", typeof(string)}, new object[] {"pf", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("FullTypeThree", fullType);
    
            IDictionary<string, Object> deltaType = MakeMap(new object[][]{new object[] {"p1", typeof(string)}, new object[] {"p2", typeof(string)}, new object[] {"p3", typeof(string)}, new object[] {"pd", typeof(string)}});
            epService.EPAdministrator.Configuration.AddEventType("DeltaTypeThree", deltaType);
    
            var revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("FullTypeThree");
            revEvent.AddNameDeltaEventType("DeltaTypeThree");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_EXISTS;
            revEvent.KeyPropertyNames = new string[]{"p1"};
            epService.EPAdministrator.Configuration.AddRevisionEventType("MyExistsRevisionThree", revEvent);
    
            epService.EPAdministrator.CreateEPL("create window MyWinThree#time(10 sec) as select * from MyExistsRevisionThree");
            epService.EPAdministrator.CreateEPL("insert into MyWinThree select * from FullTypeThree");
            epService.EPAdministrator.CreateEPL("insert into MyWinThree select * from DeltaTypeThree");
    
            string[] fields = "p1,p2,p3,pf,pd".Split(',');
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL("select irstream * from MyWinThree");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf", "10,20,30,f0"), "FullTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"10", "20", "30", "f0", null});
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2", "10,21"), "DeltaTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "20", "30", "f0", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", "30", "f0", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pf", "10,32,f1"), "FullTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", "30", "f0", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", "32", "f1", null});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd", "10,33,pd3"), "DeltaTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", "32", "f1", null});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "21", "33", "f1", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf", "10,22,34,f2"), "FullTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "21", "33", "f1", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1", "10"), "FullTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2,p3,pf,pd", "10,23,35,pfx,pd4"), "DeltaTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "22", "34", "f2", "pd3"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", "23", "35", "f2", "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p2", "10,null"), "DeltaTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", "23", "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, "35", "f2", "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd,pf", "10,36,pdx,f4"), "FullTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, "35", "f2", "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, "36", "f4", "pd4"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap("p1,p3,pd", "10,null,pd5"), "DeltaTypeThree");
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"10", null, "36", "f4", "pd4"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"10", null, null, "f4", "pd5"});
            listenerOne.Reset();
        }
    
        private void RunAssertionNestedPropertiesNoDelta(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("Nested", typeof(SupportBeanComplexProps));
    
            var revEvent = new ConfigurationRevisionEventType();
            revEvent.AddNameBaseEventType("Nested");
            revEvent.PropertyRevision = PropertyRevisionEnum.MERGE_DECLARED;
            revEvent.KeyPropertyNames = new string[]{"SimpleProperty"};
            epService.EPAdministrator.Configuration.AddRevisionEventType("NestedRevision", revEvent);
    
            epService.EPAdministrator.CreateEPL("create window MyWinFour#time(10 sec) as select * from NestedRevision");
            epService.EPAdministrator.CreateEPL("insert into MyWinFour select * from Nested");
    
            string[] fields = "key,f1".Split(',');
            string stmtText = "select irstream SimpleProperty as key, Nested.NestedValue as f1 from MyWinFour";
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL(stmtText);
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"Simple", "NestedValue"});
    
            SupportBeanComplexProps bean = SupportBeanComplexProps.MakeDefaultBean();
            bean.Nested.NestedValue = "val2";
            epService.EPRuntime.SendEvent(bean);
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"Simple", "NestedValue"});
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"Simple", "val2"});
            listenerOne.Reset();
        }
    
        private IDictionary<string, Object> MakeMap(object[][] entries) {
            var result = new Dictionary<string, Object>();
            for (int i = 0; i < entries.Length; i++) {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    
        private IDictionary<string, Object> MakeMap(string keysList, string valuesList) {
            string[] keys = keysList.Split(',');
            string[] values = valuesList.Split(',');
    
            var result = new Dictionary<string, Object>();
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
} // end of namespace
