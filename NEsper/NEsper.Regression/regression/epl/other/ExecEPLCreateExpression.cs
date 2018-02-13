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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
// using static org.junit.Assert.*;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLCreateExpression : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportCollection));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionParseSpecialAndMixedExprAndScript(epService);
            RunAssertionExprAndScriptLifecycleAndFilter(epService);
            RunAssertionScriptUse(epService);
            RunAssertionExpressionUse(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create expression E1 {''}");
            TryInvalid(epService, "create expression E1 {''}",
                    "Error starting statement: Expression 'E1' has already been declared [create expression E1 {''}]");
    
            epService.EPAdministrator.CreateEPL("create expression int js:Abc(p1, p2) [p1*p2]");
            TryInvalid(epService, "create expression int js:Abc(a, a) [p1*p2]",
                    "Error starting statement: Script 'abc' that takes the same number of parameters has already been declared [create expression int js:Abc(a, a) [p1*p2]]");
        }
    
        private void RunAssertionParseSpecialAndMixedExprAndScript(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create expression string js:Myscript(p1) [\"--\"+p1+\"--\"]");
            epService.EPAdministrator.CreateEPL("create expression myexpr {sb => '--'||theString||'--'}");
    
            // test mapped property syntax
            string eplMapped = "select Myscript('x') as c0, Myexpr(sb) as c1 from SupportBean as sb";
            EPStatement stmtMapped = epService.EPAdministrator.CreateEPL(eplMapped);
            stmtMapped.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[]{"--x--", "--E1--"});
            stmtMapped.Dispose();
    
            // test expression chained syntax
            string eplExpr = "" +
                    "create expression scalarfilter {s => " +
                    "   strvals.Where(y => y != 'E1') " +
                    "}";
            epService.EPAdministrator.CreateEPL(eplExpr);
            string eplSelect = "select Scalarfilter(t).Where(x => x != 'E2') as val1 from SupportCollection as t";
            epService.EPAdministrator.CreateEPL(eplSelect).AddListener(listener);
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E3", "E4");
            epService.EPAdministrator.DestroyAllStatements();
            listener.Reset();
    
            // test script chained synax
            string eplScript = "create expression " + typeof(SupportBean).FullName + " js:CallIt() [ new " + typeof(SupportBean).FullName + "('E1', 10); ]";
            epService.EPAdministrator.CreateEPL(eplScript);
            epService.EPAdministrator.CreateEPL("select CallIt() as val0, CallIt().TheString as val1 from SupportBean as sb").AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0.theString,val0.intPrimitive,val1".Split(','), new Object[]{"E1", 10, "E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionScriptUse(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create expression int js:Abc(p1, p2) [p1*p2*10]");
            epService.EPAdministrator.CreateEPL("create expression int js:Abc(p1) [p1*10]");
    
            string epl = "select Abc(intPrimitive, doublePrimitive) as c0, Abc(intPrimitive) as c1 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            epService.EPRuntime.SendEvent(MakeBean("E1", 10, 3.5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[]{350, 100});
    
            stmt.Dispose();
    
            // test SODA
            string eplExpr = "create expression Somescript(i1) ['a']";
            EPStatementObjectModel modelExpr = epService.EPAdministrator.CompileEPL(eplExpr);
            Assert.AreEqual(eplExpr, modelExpr.ToEPL());
            EPStatement stmtSODAExpr = epService.EPAdministrator.Create(modelExpr);
            Assert.AreEqual(eplExpr, stmtSODAExpr.Text);
    
            string eplSelect = "select Somescript(1) from SupportBean";
            EPStatementObjectModel modelSelect = epService.EPAdministrator.CompileEPL(eplSelect);
            Assert.AreEqual(eplSelect, modelSelect.ToEPL());
            EPStatement stmtSODASelect = epService.EPAdministrator.Create(modelSelect);
            Assert.AreEqual(eplSelect, stmtSODASelect.Text);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionExpressionUse(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create expression TwoPi {Math.PI * 2}");
            epService.EPAdministrator.CreateEPL("create expression factorPi {sb => Math.PI * intPrimitive}");
    
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "TwoPi() as c0," +
                    "(select TwoPi() from SupportBean_S0#lastevent) as c1," +
                    "FactorPi(sb) as c2 " +
                    "from SupportBean sb";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(epl);
            stmtSelect.AddListener(listener);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));   // factor is 3
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new Object[]{Math.PI * 2, Math.PI * 2, Math.PI * 3});
    
            stmtSelect.Dispose();
    
            // test local expression override
            EPStatement stmtOverride = epService.EPAdministrator.CreateEPL("expression TwoPi {Math.PI * 10} select TwoPi() as c0 from SupportBean");
            stmtOverride.AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{Math.PI * 10});
    
            // test SODA
            string eplExpr = "create expression JoinMultiplication {(s1,s2) => s1.intPrimitive*s2.id}";
            EPStatementObjectModel modelExpr = epService.EPAdministrator.CompileEPL(eplExpr);
            Assert.AreEqual(eplExpr, modelExpr.ToEPL());
            EPStatement stmtSODAExpr = epService.EPAdministrator.Create(modelExpr);
            Assert.AreEqual(eplExpr, stmtSODAExpr.Text);
    
            // test SODA and join and 2-stream parameter
            string eplJoin = "select JoinMultiplication(sb,s0) from SupportBean#lastevent as sb, SupportBean_S0#lastevent as s0";
            EPStatementObjectModel modelJoin = epService.EPAdministrator.CompileEPL(eplJoin);
            Assert.AreEqual(eplJoin, modelJoin.ToEPL());
            EPStatement stmtSODAJoin = epService.EPAdministrator.Create(modelJoin);
            Assert.AreEqual(eplJoin, stmtSODAJoin.Text);
            epService.EPAdministrator.DestroyAllStatements();
    
            // test subquery against named window and table defined in declared expression
            TryAssertionTestExpressionUse(epService, true);
            TryAssertionTestExpressionUse(epService, false);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionTestExpressionUse(EPServiceProvider epService, bool namedWindow) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create expression myexpr {(select intPrimitive from MyInfra)}");
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as SupportBean" :
                    "create table MyInfra(theString string, intPrimitive int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select theString, intPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("select Myexpr() as c0 from SupportBean_S0").AddListener(listener);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new Object[]{100});
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionExprAndScriptLifecycleAndFilter(EPServiceProvider epService) {
            // expression assertion
            TryAssertionLifecycleAndFilter(epService, "create expression MyFilter {sb => intPrimitive = 1}",
                    "select * from SupportBean(MyFilter(sb)) as sb",
                    "create expression MyFilter {sb => intPrimitive = 2}");
    
            // script assertion
            TryAssertionLifecycleAndFilter(epService, "create expression bool js:MyFilter(intPrimitive) [intPrimitive==1]",
                    "select * from SupportBean(MyFilter(intPrimitive)) as sb",
                    "create expression bool js:MyFilter(intPrimitive) [intPrimitive==2]");
        }
    
        private void TryAssertionLifecycleAndFilter(EPServiceProvider epService, string expressionBefore,
                                                    string selector,
                                                    string expressionAfter) {
            var l1 = new SupportUpdateListener();
            var l2 = new SupportUpdateListener();
    
            EPStatement stmtExpression = epService.EPAdministrator.CreateEPL(expressionBefore);
    
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(selector);
            stmtSelectOne.AddListener(l1);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(l1.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsTrue(l1.GetAndClearIsInvoked());
    
            stmtExpression.Dispose();
            epService.EPAdministrator.CreateEPL(expressionAfter);
    
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(selector);
            stmtSelectTwo.AddListener(l2);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            Assert.IsFalse(l1.GetAndClearIsInvoked() || l2.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsTrue(l1.GetAndClearIsInvoked());
            Assert.IsFalse(l2.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            Assert.IsFalse(l1.GetAndClearIsInvoked());
            Assert.IsTrue(l2.GetAndClearIsInvoked());
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private SupportBean MakeBean(string theString, int intPrimitive, double doublePrimitive) {
            var sb = new SupportBean();
            sb.IntPrimitive = intPrimitive;
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    }
} // end of namespace
