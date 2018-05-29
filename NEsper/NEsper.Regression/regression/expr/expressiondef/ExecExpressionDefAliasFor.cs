///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.expressiondef
{
    public class ExecExpressionDefAliasFor : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionContextPartition(epService);
            RunAssertionDocSamples(epService);
            RunAssertionNestedAlias(epService);
            RunAssertionAliasAggregation(epService);
            RunAssertionGlobalAliasAndSODA(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionContextPartition(EPServiceProvider epService) {
            string epl =
                    "create expression the_expr alias for {TheString='a' and IntPrimitive=1};\n" +
                            "create context the_context start @now end after 10 minutes;\n" +
                            "@Name('s0') context the_context select * from SupportBean(the_expr)\n";
            DeploymentResult result = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.GetStatement("s0").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("a", 1));
            Assert.IsTrue(listener.IsInvokedAndReset());
    
            epService.EPRuntime.SendEvent(new SupportBean("b", 1));
            Assert.IsFalse(listener.IsInvokedAndReset());
    
            epService.EPAdministrator.DeploymentAdmin.UndeployRemove(result.DeploymentId);
        }
    
        private void RunAssertionDocSamples(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema SampleEvent()");
            epService.EPAdministrator.CreateEPL("expression twoPI alias for {Math.PI * 2}\n" +
                    "select twoPI from SampleEvent");
    
            epService.EPAdministrator.CreateEPL("create schema EnterRoomEvent()");
            epService.EPAdministrator.CreateEPL("expression countPeople alias for {count(*)} \n" +
                    "select countPeople from EnterRoomEvent#time(10 seconds) having countPeople > 10");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNestedAlias(EPServiceProvider epService) {
            string[] fields = "c0".Split(',');
            var listener = new SupportUpdateListener();
    
            epService.EPAdministrator.CreateEPL("create expression F1 alias for {10}");
            epService.EPAdministrator.CreateEPL("create expression F2 alias for {20}");
            epService.EPAdministrator.CreateEPL("create expression F3 alias for {F1+F2}");
            epService.EPAdministrator.CreateEPL("select F3 as c0 from SupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{30});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAliasAggregation(EPServiceProvider epService) {
            string epl = "@Audit expression total alias for {sum(IntPrimitive)} " +
                    "select total, total+1 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string[] fields = "total,total+1".Split(',');
            foreach (string field in fields) {
                Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(field).GetBoxedType());
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{10, 11});
    
            stmt.Dispose();
        }
    
        private void RunAssertionGlobalAliasAndSODA(EPServiceProvider epService) {
            string eplDeclare = "create expression myaliastwo alias for {2}";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(eplDeclare);
            Assert.AreEqual(eplDeclare, model.ToEPL());
            EPStatement stmtDeclare = epService.EPAdministrator.Create(model);
            Assert.AreEqual(eplDeclare, stmtDeclare.Text);
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create expression myalias alias for {1}");
            epService.EPAdministrator.CreateEPL("select myaliastwo from SupportBean(IntPrimitive = myalias)").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("myaliastwo"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "expression total alias for {sum(xxx)} select total+1 from SupportBean",
                    "Error starting statement: Failed to validate select-clause expression 'total+1': Error validating expression alias 'total': Failed to validate alias expression body expression 'sum(xxx)': Property named 'xxx' is not valid in any stream [expression total alias for {sum(xxx)} select total+1 from SupportBean]");
            TryInvalid(epService, "expression total xxx for {1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting 'alias' keyword but received 'xxx' [expression total xxx for {1} select total+1 from SupportBean]");
            TryInvalid(epService, "expression total(a) alias for {1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting no parameters but received 'a' [expression total(a) alias for {1} select total+1 from SupportBean]");
            TryInvalid(epService, "expression total alias for {a -> 1} select total+1 from SupportBean",
                    "For expression alias 'total' expecting an expression without parameters but received 'a ->' [expression total alias for {a -> 1} select total+1 from SupportBean]");
            TryInvalid(epService, "expression total alias for ['some text'] select total+1 from SupportBean",
                    "For expression alias 'total' expecting an expression but received a script [expression total alias for ['some text'] select total+1 from SupportBean]");
        }
    }
} // end of namespace
