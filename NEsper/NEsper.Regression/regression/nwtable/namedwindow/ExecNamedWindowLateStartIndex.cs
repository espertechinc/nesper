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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowLateStartIndex : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyCountAccessEvent));
            // prepare
            PreloadData(epService, false);
    
            // test join
            string eplJoin = "select * from SupportBean_S0 as s0 unidirectional, AWindow(p00='x') as aw where aw.id = s0.id";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(eplJoin).Events += listener.Update;
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(2, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "x"));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            // test subquery no-index-share
            string eplSubqueryNoIndexShare = "select (select id from AWindow(p00='x') as aw where aw.id = s0.id) " +
                    "from SupportBean_S0 as s0 unidirectional";
            epService.EPAdministrator.CreateEPL(eplSubqueryNoIndexShare).Events += listener.Update;
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(2, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "x"));
    
            // test subquery with index share
            epService.EPAdministrator.DestroyAllStatements();
            PreloadData(epService, true);
    
            string eplSubqueryWithIndexShare = "select (select id from AWindow(p00='x') as aw where aw.id = s0.id) " +
                    "from SupportBean_S0 as s0 unidirectional";
            epService.EPAdministrator.CreateEPL(eplSubqueryWithIndexShare).Events += listener.Update;
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(2, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(-1, "x"));
            Assert.IsTrue(listener.IsInvoked);
        }
    
        private void PreloadData(EPServiceProvider epService, bool indexShare) {
            string createEpl = "create window AWindow#keepall as MyCountAccessEvent";
            if (indexShare) {
                createEpl = "@Hint('enable_window_subquery_indexshare') " + createEpl;
            }
    
            epService.EPAdministrator.CreateEPL(createEpl);
            epService.EPAdministrator.CreateEPL("insert into AWindow select * from MyCountAccessEvent");
            epService.EPAdministrator.CreateEPL("create index I1 on AWindow(p00)");
            MyCountAccessEvent.GetAndResetCountGetterCalled();
            for (int i = 0; i < 100; i++) {
                epService.EPRuntime.SendEvent(new MyCountAccessEvent(i, "E" + i));
            }
            epService.EPRuntime.SendEvent(new MyCountAccessEvent(-1, "x"));
            if (!InstrumentationHelper.ENABLED) {
                Assert.AreEqual(101, MyCountAccessEvent.GetAndResetCountGetterCalled());
            }
        }
    
        public class MyCountAccessEvent {
            private static int countGetterCalled;
    
            private readonly int id;
            private readonly string p00;
    
            public MyCountAccessEvent(int id, string p00) {
                this.id = id;
                this.p00 = p00;
            }
    
            public static int GetAndResetCountGetterCalled() {
                int value = countGetterCalled;
                countGetterCalled = 0;
                return value;
            }
    
            public int GetId() {
                return id;
            }
    
            public string GetP00() {
                countGetterCalled++;
                return p00;
            }
        }
    }
} // end of namespace
