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
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewParameterizedByContext : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in Collections.List(typeof(MyInitEventWLength), typeof(SupportBean))) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionLengthWindow(epService);
            RunAssertionDocSample(epService);
    
            epService.EPAdministrator.CreateEPL("create context CtxInitToTerm initiated by MyInitEventWLength as miewl terminated after 1 year");
            RunAssertionWindow(epService, "Length_batch(context.miewl.intSize)");
            RunAssertionWindow(epService, "Time(context.miewl.intSize)");
            RunAssertionWindow(epService, "Ext_timed(longPrimitive, context.miewl.intSize)");
            RunAssertionWindow(epService, "Time_batch(context.miewl.intSize)");
            RunAssertionWindow(epService, "Ext_timed_batch(longPrimitive, context.miewl.intSize)");
            RunAssertionWindow(epService, "Time_length_batch(context.miewl.intSize, context.miewl.intSize)");
            RunAssertionWindow(epService, "Time_accum(context.miewl.intSize)");
            RunAssertionWindow(epService, "Firstlength(context.miewl.intSize)");
            RunAssertionWindow(epService, "Firsttime(context.miewl.intSize)");
            RunAssertionWindow(epService, "Sort(context.miewl.intSize, intPrimitive)");
            RunAssertionWindow(epService, "Rank(theString, context.miewl.intSize, theString)");
            RunAssertionWindow(epService, "Time_order(longPrimitive, context.miewl.intSize)");
        }
    
        private void RunAssertionDocSample(EPServiceProvider epService) {
            string epl = "create schema ParameterEvent(windowSize int);" +
                    "create context MyContext initiated by ParameterEvent as params terminated after 1 year;" +
                    "context MyContext select * from SupportBean#length(context.params.windowSize);";
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(deployed.DeploymentId);
        }
    
        private void RunAssertionWindow(EPServiceProvider epService, string window) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context CtxInitToTerm select * from SupportBean#" + window);
            epService.EPRuntime.SendEvent(new MyInitEventWLength("P1", 2));
            stmt.Dispose();
        }
    
        private void RunAssertionLengthWindow(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context CtxInitToTerm initiated by MyInitEventWLength as miewl terminated after 1 year");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context CtxInitToTerm select context.miewl.id as id, count(*) as cnt from SupportBean(theString=context.miewl.id)#length(context.miewl.intSize)");
    
            epService.EPRuntime.SendEvent(new MyInitEventWLength("P1", 2));
            epService.EPRuntime.SendEvent(new MyInitEventWLength("P2", 4));
            epService.EPRuntime.SendEvent(new MyInitEventWLength("P3", 3));
            for (int i = 0; i < 10; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("P1", 0));
                epService.EPRuntime.SendEvent(new SupportBean("P2", 0));
                epService.EPRuntime.SendEvent(new SupportBean("P3", 0));
            }
    
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), "id,cnt".Split(','), new object[][]{new object[] {"P1", 2L}, new object[] {"P2", 4L}, new object[] {"P3", 3L}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public class MyInitEventWLength {
            private readonly string id;
            private readonly int intSize;
    
            public MyInitEventWLength(string id, int intSize) {
                this.id = id;
                this.intSize = intSize;
            }
    
            public string GetId() {
                return id;
            }
    
            public int GetIntSize() {
                return intSize;
            }
        }
    }
} // end of namespace
