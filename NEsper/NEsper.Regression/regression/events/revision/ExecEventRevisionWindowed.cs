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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.events.revision
{
    public class ExecEventRevisionWindowed : RegressionExecution {
        private readonly string[] _fields = "K0,P1,P5".Split(',');
    
        public override void Configure(Configuration configuration) {
            // first revision event type
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("FullEvent", typeof(SupportRevisionFull));
            configuration.AddEventType("D1", typeof(SupportDeltaOne));
            configuration.AddEventType("D5", typeof(SupportDeltaFive));
    
            var configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = new string[]{"K0"};
            configRev.AddNameBaseEventType("FullEvent");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D5");
            configuration.AddRevisionEventType("RevisableQuote", configRev);
    
            // second revision event type
            configuration.AddEventType("MyMap", MakeMap(
                    new object[][] {
                        new object[] {"P5", typeof(string)},
                        new object[] {"P1", typeof(string)},
                        new object[] {"K0", typeof(string)},
                        new object[] {"m0", typeof(string)}
                    }));
            configRev = new ConfigurationRevisionEventType();
            configRev.KeyPropertyNames = new string[]{"P5", "P1"};
            configRev.AddNameBaseEventType("MyMap");
            configRev.AddNameDeltaEventType("D1");
            configRev.AddNameDeltaEventType("D5");
            configuration.AddRevisionEventType("RevisableMap", configRev);
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecEventRevisionWindowed))) {
                return;
            }

            RunAssertionSubclassInterface(epService);
            RunAssertionMultiPropertyMapMixin(epService);
            RunAssertionUnique(epService);
            RunAssertionGroupLength(epService);
        }
    
        private void RunAssertionSubclassInterface(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("ISupportRevisionFull", typeof(ISupportRevisionFull));
            epService.EPAdministrator.Configuration.AddEventType("ISupportDeltaFive", typeof(ISupportDeltaFive));
    
            var config = new ConfigurationRevisionEventType();
            config.AddNameBaseEventType("ISupportRevisionFull");
            config.KeyPropertyNames = new string[]{"K0"};
            config.AddNameDeltaEventType("ISupportDeltaFive");
            epService.EPAdministrator.Configuration.AddRevisionEventType("MyInterface", config);
    
            EPStatement stmtCreateWin = epService.EPAdministrator.CreateEPL
                ("create window MyInterfaceWindow#keepall as select * from MyInterface");
            epService.EPAdministrator.CreateEPL
                ("insert into MyInterfaceWindow select * from ISupportRevisionFull");
            epService.EPAdministrator.CreateEPL
                ("insert into MyInterfaceWindow select * from ISupportDeltaFive");
    
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL
                ("@Audit select irstream K0,P0,P1 from MyInterfaceWindow");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
            string[] fields = "K0,P0,P1".Split(',');
            EPAssertionUtil.AssertEqualsAnyOrder(consumerOne.EventType.PropertyNames, fields);
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull(null, "00", "10", "20", "30", "40", "50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{null, "00", "10"});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive(null, "999", null));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{null, "00", "999"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{null, "00", "10"});
            listenerOne.Reset();
    
            stmtCreateWin.Stop();
            stmtCreateWin.Start();
            consumerOne.Stop();
            consumerOne.Start();
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("zz", "xx", "yy", "20", "30", "40", "50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"zz", "xx", "yy"});
        }
    
        private void RunAssertionMultiPropertyMapMixin(EPServiceProvider epService) {
            string[] fields = "K0,P1,P5,m0".Split(',');
            EPStatement stmtCreateWin = epService.EPAdministrator.CreateEPL
                ("create window RevMap#length(3) as select * from RevisableMap");
            epService.EPAdministrator.CreateEPL
                ("insert into RevMap select * from MyMap");
            epService.EPAdministrator.CreateEPL
                ("insert into RevMap select * from D1");
            epService.EPAdministrator.CreateEPL
                ("insert into RevMap select * from D5");
    
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL
                ("select irstream * from RevMap order by K0");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(MakeMap(new object[][]{new object[] {"P5", "p5_1"}, new object[] {"P1", "p1_1"}, new object[] {"K0", "E1"}, new object[] {"m0", "M0"}}), "MyMap");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E1", "p1_1", "p5_1", "M0"});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("E2", "p1_1", "p5_1"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"E2", "p1_1", "p5_1", "M0"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"E1", "p1_1", "p5_1", "M0"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), fields, new object[]{"E2", "p1_1", "p5_1", "M0"});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap(new object[][]{new object[] {"P5", "p5_1"}, new object[] {"P1", "p1_2"}, new object[] {"K0", "E3"}, new object[] {"m0", "M1"}}), "MyMap");
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields, new object[][]{new object[] {"E2", "p1_1", "p5_1", "M0"}, new object[] {"E3", "p1_2", "p5_1", "M1"}});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("E4", "p1_1", "p5_1"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], fields, new object[]{"E4", "p1_1", "p5_1", "M0"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], fields, new object[]{"E2", "p1_1", "p5_1", "M0"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields, new object[][]{new object[] {"E3", "p1_2", "p5_1", "M1"}, new object[] {"E4", "p1_1", "p5_1", "M0"}});
            listenerOne.Reset();
    
            epService.EPRuntime.SendEvent(MakeMap(new object[][]{new object[] {"P5", "p5_2"}, new object[] {"P1", "p1_1"}, new object[] {"K0", "E5"}, new object[] {"m0", "M2"}}), "MyMap");
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{"E5", "p1_1", "p5_2", "M2"});
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3", "p1_2", "p5_1", "M1"}, new object[] {"E4", "p1_1", "p5_1", "M0"}, new object[] {"E5", "p1_1", "p5_2", "M2"}});
    
            epService.EPRuntime.SendEvent(new SupportDeltaOne("E6", "p1_1", "p5_2"));
            EPAssertionUtil.AssertProps(listenerOne.AssertPairGetIRAndReset(), fields,
                    new object[]{"E6", "p1_1", "p5_2", "M2"}, new object[]{"E5", "p1_1", "p5_2", "M2"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionUnique(EPServiceProvider epService) {
            var stmtCreateWin = epService.EPAdministrator.CreateEPL
                ("create window RevQuote#unique(P1) as select * from RevisableQuote");
            epService.EPAdministrator.CreateEPL
                ("insert into RevQuote select * from FullEvent");
            epService.EPAdministrator.CreateEPL
                ("insert into RevQuote select * from D1");
            epService.EPAdministrator.CreateEPL
                ("insert into RevQuote select * from D5");
    
            var consumerOne = epService.EPAdministrator.CreateEPL
                ("select irstream * from RevQuote");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "a10", "a50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), _fields, new object[]{"a", "a10", "a50"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), _fields, new object[]{"a", "a10", "a50"});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("a", "a11", "a51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], _fields, new object[]{"a", "a11", "a51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], _fields, new object[]{"a", "a10", "a50"});
            listenerOne.Reset();
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), _fields, new object[]{"a", "a11", "a51"});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("b", "b10", "b50"));
            epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "b10", "b50"));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), _fields, new object[]{"b", "b10", "b50"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreateWin.GetEnumerator(), _fields, new object[][] {
                new object[] {"a", "a11", "a51"},
                new object[] {"b", "b10", "b50"}
            });
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("b", "a11", "b51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], _fields, new object[]{"b", "a11", "b51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], _fields, new object[]{"a", "a11", "a51"});
            EPAssertionUtil.AssertProps(stmtCreateWin.First(), _fields, new object[]{"b", "a11", "b51"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGroupLength(EPServiceProvider epService) {
            EPStatement stmtCreateWin = epService.EPAdministrator.CreateEPL
                ("create window RevQuote#groupwin(P1)#length(2) as select * from RevisableQuote");
            epService.EPAdministrator.CreateEPL
                ("insert into RevQuote select * from FullEvent");
            epService.EPAdministrator.CreateEPL
                ("insert into RevQuote select * from D1");
            epService.EPAdministrator.CreateEPL
                ("insert into RevQuote select * from D5");
    
            EPStatement consumerOne = epService.EPAdministrator.CreateEPL
                ("select irstream * from RevQuote order by K0 asc");
            var listenerOne = new SupportUpdateListener();
            consumerOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SendEvent(new SupportRevisionFull("a", "P1", "a50"));
            epService.EPRuntime.SendEvent(new SupportDeltaFive("a", "P1", "a51"));
            epService.EPRuntime.SendEvent(new SupportRevisionFull("b", "P2", "b50"));
            epService.EPRuntime.SendEvent(new SupportRevisionFull("c", "P3", "c50"));
            epService.EPRuntime.SendEvent(new SupportDeltaFive("d", "P3", "d50"));
    
            listenerOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), _fields, new object[][]{new object[] {"a", "P1", "a51"}, new object[] {"b", "P2", "b50"}, new object[] {"c", "P3", "c50"}});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("b", "P1", "b51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], _fields, new object[]{"b", "P1", "b51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], _fields, new object[]{"b", "P2", "b50"});
            listenerOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), _fields, new object[][]{new object[] {"a", "P1", "a51"}, new object[] {"b", "P1", "b51"}, new object[] {"c", "P3", "c50"}});
    
            epService.EPRuntime.SendEvent(new SupportDeltaFive("c", "P1", "c51"));
            EPAssertionUtil.AssertProps(listenerOne.LastNewData[0], _fields, new object[]{"c", "P1", "c51"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[1], _fields, new object[]{"c", "P3", "c50"});
            EPAssertionUtil.AssertProps(listenerOne.LastOldData[0], _fields, new object[]{"a", "P1", "a51"});
            listenerOne.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreateWin.GetEnumerator(), _fields, new object[][]{new object[] {"b", "P1", "b51"}, new object[] {"c", "P1", "c51"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private IDictionary<string, Object> MakeMap(object[][] entries) {
            var result = new Dictionary<string, Object>();
            for (int i = 0; i < entries.Length; i++) {
                result.Put((string) entries[i][0], entries[i][1]);
            }
            return result;
        }
    }
} // end of namespace
