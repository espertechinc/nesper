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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

namespace com.espertech.esper.regression.context
{
    public class ExecContextWDeclaredExpression : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionDeclaredExpression(epService);
            RunAssertionAliasExpression(epService);
            RunAssertionContextFilter(epService);
        }
    
        private void RunAssertionDeclaredExpression(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean");
            epService.EPAdministrator.CreateEPL("create expression getLabelOne { context.label }");
            epService.EPAdministrator.CreateEPL("create expression getLabelTwo { 'x'||context.label||'x' }");
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("expression getLabelThree { context.label } " +
                    "context MyCtx " +
                    "select getLabelOne() as c0, getLabelTwo() as c1, getLabelThree() as c2 from SupportBean").Events += listener.Update;
    
            TryAssertionExpression(epService, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAliasExpression(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context MyCtx as " +
                    "group by IntPrimitive < 0 as n, " +
                    "group by IntPrimitive > 0 as p " +
                    "from SupportBean");
            epService.EPAdministrator.CreateEPL("create expression getLabelOne alias for { context.label }");
            epService.EPAdministrator.CreateEPL("create expression getLabelTwo alias for { 'x'||context.label||'x' }");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("expression getLabelThree alias for { context.label } " +
                    "context MyCtx " +
                    "select getLabelOne as c0, getLabelTwo as c1, getLabelThree as c2 from SupportBean").Events += listener.Update;
    
            TryAssertionExpression(epService, listener);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionContextFilter(EPServiceProvider epService) {
            string expr = "create expression THE_EXPRESSION alias for {TheString='x'}";
            epService.EPAdministrator.CreateEPL(expr);
    
            string context = "create context context2 initiated @now and pattern[Every(SupportBean(THE_EXPRESSION))] terminated after 10 minutes";
            epService.EPAdministrator.CreateEPL(context);
    
            var listener = new SupportUpdateListener();
            string statement = "context context2 select * from pattern[e1=SupportBean(THE_EXPRESSION) -> e2=SupportBean(TheString='y')]";
            epService.EPAdministrator.CreateEPL(statement).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("x", 1));
            epService.EPRuntime.SendEvent(new SupportBean("y", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "e1.IntPrimitive,e2.IntPrimitive".Split(','), new object[]{1, 2});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionExpression(EPServiceProvider epService, SupportUpdateListener listener) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"n", "xnx", "n"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"p", "xpx", "p"});
        }
    }
} // end of namespace
