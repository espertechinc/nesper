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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableRollup : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionRollupOneDim(epService);
            RunAssertionRollupTwoDim(epService);
            RunAssertionGroupingSetThreeDim(epService);
        }
    
        private void RunAssertionRollupOneDim(EPServiceProvider epService) {
            var listenerQuery = new SupportUpdateListener();
            var listenerOut = new SupportUpdateListener();
            string[] fieldsOut = "TheString,total".Split(',');
    
            epService.EPAdministrator.CreateEPL("create table MyTableR1D(pk string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table MyTableR1D insert into MyStreamOne select TheString, sum(IntPrimitive) as total from SupportBean#length(4) group by Rollup(TheString)").Events += listenerOut.Update;
            epService.EPAdministrator.CreateEPL("select MyTableR1D[p00].total as c0 from SupportBean_S0").Events += listenerQuery.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            AssertValuesListener(epService, listenerQuery, new object[][]{new object[] {null, 10}, new object[] {"E1", 10}, new object[] {"E2", null}});
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][]{new object[] {"E1", 10}, new object[] {null, 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 200));
            AssertValuesListener(epService, listenerQuery, new object[][]{new object[] {null, 210}, new object[] {"E1", 10}, new object[] {"E2", 200}});
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][]{new object[] {"E2", 200}, new object[] {null, 210}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            AssertValuesListener(epService, listenerQuery, new object[][]{new object[] {null, 221}, new object[] {"E1", 21}, new object[] {"E2", 200}});
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][]{new object[] {"E1", 21}, new object[] {null, 221}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 201));
            AssertValuesListener(epService, listenerQuery, new object[][]{new object[] {null, 422}, new object[] {"E1", 21}, new object[] {"E2", 401}});
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][]{new object[] {"E2", 401}, new object[] {null, 422}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 12)); // {"E1", 10} leaving window
            AssertValuesListener(epService, listenerQuery, new object[][]{new object[] {null, 424}, new object[] {"E1", 23}, new object[] {"E2", 401}});
            EPAssertionUtil.AssertPropsPerRow(listenerOut.GetAndResetLastNewData(), fieldsOut, new object[][]{new object[] {"E1", 23}, new object[] {null, 424}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionRollupTwoDim(EPServiceProvider epService) {
            string[] fields = "k0,k1,total".Split(',');
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventTwo(k0 int, k1 int, col int)");
            epService.EPAdministrator.CreateEPL("create table MyTableR2D(k0 int primary key, k1 int primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table MyTableR2D insert into MyStreamTwo select sum(col) as total from MyEventTwo#length(3) group by Rollup(k0,k1)");
    
            epService.EPRuntime.SendEvent(new object[]{1, 10, 100}, "MyEventTwo");
            epService.EPRuntime.SendEvent(new object[]{2, 10, 200}, "MyEventTwo");
            epService.EPRuntime.SendEvent(new object[]{1, 20, 300}, "MyEventTwo");
    
            AssertValuesIterate(epService, "MyTableR2D", fields, new object[][]{new object[] {null, null, 600}, new object[] {1, null, 400}, new object[] {2, null, 200},
                    new object[] {1, 10, 100}, 
                    new object[] {2, 10, 200},
                    new object[] {1, 20, 300}});
    
            epService.EPRuntime.SendEvent(new object[]{1, 10, 400}, "MyEventTwo"); // expires {1, 10, 100}
    
            AssertValuesIterate(epService, "MyTableR2D", fields, new object[][]{new object[] {null, null, 900}, new object[] {1, null, 700}, new object[] {2, null, 200},
                    new object[] {1, 10, 400},
                    new object[] {2, 10, 200},
                    new object[] {1, 20, 300}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionGroupingSetThreeDim(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema MyEventThree(k0 string, k1 string, k2 string, col int)");
            epService.EPAdministrator.CreateEPL("create table MyTableGS3D(k0 string primary key, k1 string primary key, k2 string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("into table MyTableGS3D insert into MyStreamThree select sum(col) as total from MyEventThree#length(3) group by grouping Sets(k0,k1,k2)");
    
            string[] fields = "k0,k1,k2,total".Split(',');
            epService.EPRuntime.SendEvent(new object[]{1, 10, 100, 1000}, "MyEventThree");
            epService.EPRuntime.SendEvent(new object[]{2, 10, 200, 2000}, "MyEventThree");
            epService.EPRuntime.SendEvent(new object[]{1, 20, 300, 3000}, "MyEventThree");
    
            AssertValuesIterate(epService, "MyTableGS3D", fields, new object[][]{
                    new object[] {1, null, null, 4000},
                    new object[] {2, null, null, 2000},
                    new object[] {null, 10, null, 3000},
                    new object[] {null, 20, null, 3000},
                    new object[] {null, null, 100, 1000},
                    new object[] {null, null, 200, 2000},
                    new object[] {null, null, 300, 3000}});
    
            epService.EPRuntime.SendEvent(new object[]{1, 10, 400, 4000}, "MyEventThree"); // expires {1, 10, 100, 1000}
    
            AssertValuesIterate(epService, "MyTableGS3D", fields, new object[][]{
                    new object[] {1, null, null, 7000},
                    new object[] {2, null, null, 2000},
                    new object[] {null, 10, null, 6000},
                    new object[] {null, 20, null, 3000},
                    new object[] {null, null, 100, null}, 
                    new object[] {null, null, 400, 4000}, 
                    new object[] {null, null, 200, 2000},
                    new object[] {null, null, 300, 3000}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertValuesIterate(EPServiceProvider epService, string name, string[] fields, object[][] objects) {
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery("select * from " + name);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields, objects);
        }
    
        private void AssertValuesListener(EPServiceProvider epService, SupportUpdateListener listenerQuery, object[][] objects) {
            for (int i = 0; i < objects.Length; i++) {
                string p00 = (string) objects[i][0];
                int? expected = (int?) objects[i][1];
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00));
                Assert.AreEqual(expected, listenerQuery.AssertOneGetNewAndReset().Get("c0"), "Failed at " + i + " for key " + p00);
            }
        }
    }
} // end of namespace
