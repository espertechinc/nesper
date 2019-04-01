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
            RunAssertionWindow(epService, "length_batch(context.miewl.IntSize)");
            RunAssertionWindow(epService, "time(context.miewl.IntSize)");
            RunAssertionWindow(epService, "ext_timed(LongPrimitive, context.miewl.IntSize)");
            RunAssertionWindow(epService, "time_batch(context.miewl.IntSize)");
            RunAssertionWindow(epService, "ext_timed_batch(LongPrimitive, context.miewl.IntSize)");
            RunAssertionWindow(epService, "time_length_batch(context.miewl.IntSize, context.miewl.IntSize)");
            RunAssertionWindow(epService, "time_accum(context.miewl.IntSize)");
            RunAssertionWindow(epService, "firstlength(context.miewl.IntSize)");
            RunAssertionWindow(epService, "firsttime(context.miewl.IntSize)");
            RunAssertionWindow(epService, "sort(context.miewl.IntSize, IntPrimitive)");
            RunAssertionWindow(epService, "rank(TheString, context.miewl.IntSize, TheString)");
            RunAssertionWindow(epService, "time_order(LongPrimitive, context.miewl.IntSize)");
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
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context CtxInitToTerm select context.miewl.Id as id, count(*) as cnt from SupportBean(TheString=context.miewl.Id)#length(context.miewl.IntSize)");
    
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
            public MyInitEventWLength(string id, int intSize) {
                this.Id = id;
                this.IntSize = intSize;
            }

            public string Id { get; }

            public int IntSize { get; }
        }
    }
} // end of namespace
