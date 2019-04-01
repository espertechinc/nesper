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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.other
{
    public class ExecEPLCreateExpression : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType<SupportCollection>();
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
    
            epService.EPAdministrator.CreateEPL("create expression int jscript:abc(p1, p2) [return p1*p2;]");
            TryInvalid(epService, "create expression int jscript:abc(a, a) [return p1*p2;]",
                    "Error starting statement: Script 'abc' that takes the same number of parameters has already been declared [create expression int jscript:abc(a, a) [return p1*p2;]]");
        }
    
        private void RunAssertionParseSpecialAndMixedExprAndScript(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create expression string jscript:myscript(p1) [return \"--\"+p1+\"--\";]");
            epService.EPAdministrator.CreateEPL("create expression myexpr {sb => '--'||TheString||'--'}");
    
            // test mapped property syntax
            string eplMapped = "select myscript('x') as c0, myexpr(sb) as c1 from SupportBean as sb";
            EPStatement stmtMapped = epService.EPAdministrator.CreateEPL(eplMapped);
            stmtMapped.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[]{"--x--", "--E1--"});
            stmtMapped.Dispose();
    
            // test expression chained syntax
            string eplExpr = "" +
                    "create expression scalarfilter {s => " +
                    "   Strvals.where(y => y != 'E1') " +
                    "}";
            epService.EPAdministrator.CreateEPL(eplExpr);
            string eplSelect = "select scalarfilter(t).where(x => x != 'E2') as val1 from SupportCollection as t";
            epService.EPAdministrator.CreateEPL(eplSelect).Events += listener.Update;
            epService.EPRuntime.SendEvent(SupportCollection.MakeString("E1,E2,E3,E4"));
            LambdaAssertionUtil.AssertValuesArrayScalar(listener, "val1", "E3", "E4");
            epService.EPAdministrator.DestroyAllStatements();
            listener.Reset();

            // test script chained syntax
            var beanType = typeof(SupportBean).FullName;
            var eplScript = $"create expression {beanType} jscript:callIt() [ return host.newObj(host.resolveType('{beanType}'), 'E1', 10); ]";
            epService.EPAdministrator.CreateEPL(eplScript);
            epService.EPAdministrator.CreateEPL("select callIt() as val0, callIt().get_TheString() as val1 from SupportBean as sb").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean());
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0.TheString,val0.IntPrimitive,val1".Split(','), new object[]{"E1", 10, "E1"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionScriptUse(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create expression int jscript:abc(p1, p2) [return p1*p2*10;]");
            epService.EPAdministrator.CreateEPL("create expression int jscript:abc(p1) [return p1*10;]");
    
            string epl = "select abc(IntPrimitive, DoublePrimitive) as c0, abc(IntPrimitive) as c1 from SupportBean";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean("E1", 10, 3.5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new object[]{350, 100});
    
            stmt.Dispose();
    
            // test SODA
            string eplExpr = "create expression somescript(i1) ['a']";
            EPStatementObjectModel modelExpr = epService.EPAdministrator.CompileEPL(eplExpr);
            Assert.AreEqual(eplExpr, modelExpr.ToEPL());
            EPStatement stmtSODAExpr = epService.EPAdministrator.Create(modelExpr);
            Assert.AreEqual(eplExpr, stmtSODAExpr.Text);
    
            string eplSelect = "select somescript(1) from SupportBean";
            EPStatementObjectModel modelSelect = epService.EPAdministrator.CompileEPL(eplSelect);
            Assert.AreEqual(eplSelect, modelSelect.ToEPL());
            EPStatement stmtSODASelect = epService.EPAdministrator.Create(modelSelect);
            Assert.AreEqual(eplSelect, stmtSODASelect.Text);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionExpressionUse(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("create expression twoPi {Math.PI * 2}");
            epService.EPAdministrator.CreateEPL("create expression factorPi {sb => Math.PI * IntPrimitive}");
    
            string[] fields = "c0,c1,c2".Split(',');
            string epl = "select " +
                    "twoPi() as c0," +
                    "(select twoPi() from SupportBean_S0#lastevent) as c1," +
                    "factorPi(sb) as c2 " +
                    "from SupportBean sb";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(epl);
            stmtSelect.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));   // factor is 3
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields,
                    new object[]{Math.PI * 2, Math.PI * 2, Math.PI * 3});
    
            stmtSelect.Dispose();
    
            // test local expression override
            EPStatement stmtOverride = epService.EPAdministrator.CreateEPL("expression twoPi {Math.PI * 10} select twoPi() as c0 from SupportBean");
            stmtOverride.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[]{Math.PI * 10});
    
            // test SODA
            string eplExpr = "create expression joinMultiplication {(s1,s2) => s1.IntPrimitive*s2.id}";
            EPStatementObjectModel modelExpr = epService.EPAdministrator.CompileEPL(eplExpr);
            Assert.AreEqual(eplExpr, modelExpr.ToEPL());
            EPStatement stmtSODAExpr = epService.EPAdministrator.Create(modelExpr);
            Assert.AreEqual(eplExpr, stmtSODAExpr.Text);
    
            // test SODA and join and 2-stream parameter
            string eplJoin = "select joinMultiplication(sb,s0) from SupportBean#lastevent as sb, SupportBean_S0#lastevent as s0";
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
            epService.EPAdministrator.CreateEPL("create expression myexpr {(select IntPrimitive from MyInfra)}");
            string eplCreate = namedWindow ?
                    "create window MyInfra#keepall as SupportBean" :
                    "create table MyInfra(TheString string, IntPrimitive int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("select myexpr() as c0 from SupportBean_S0").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "c0".Split(','), new object[]{100});
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionExprAndScriptLifecycleAndFilter(EPServiceProvider epService) {
            // expression assertion
            TryAssertionLifecycleAndFilter(
                epService,
                "create expression myFilter {sb => IntPrimitive = 1}",
                "select * from SupportBean(myFilter(sb)) as sb",
                "create expression myFilter {sb => IntPrimitive = 2}");
    
            // script assertion
            TryAssertionLifecycleAndFilter(
                epService, 
                "create expression bool jscript:myFilter(IntPrimitive) [return IntPrimitive==1]",
                "select * from SupportBean(myFilter(IntPrimitive)) as sb",
                "create expression bool jscript:myFilter(IntPrimitive) [return IntPrimitive==2]");
        }

        private void TryAssertionLifecycleAndFilter(
            EPServiceProvider epService,
            string expressionBefore,
            string selector,
            string expressionAfter)
        {
            var l1 = new SupportUpdateListener();
            var l2 = new SupportUpdateListener();
    
            EPStatement stmtExpression = epService.EPAdministrator.CreateEPL(expressionBefore);
    
            EPStatement stmtSelectOne = epService.EPAdministrator.CreateEPL(selector);
            stmtSelectOne.Events += l1.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.IsFalse(l1.GetAndClearIsInvoked());
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsTrue(l1.GetAndClearIsInvoked());
    
            stmtExpression.Dispose();
            epService.EPAdministrator.CreateEPL(expressionAfter);
    
            EPStatement stmtSelectTwo = epService.EPAdministrator.CreateEPL(selector);
            stmtSelectTwo.Events += l2.Update;
    
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
