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
using com.espertech.esper.supportregression.execution;
using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternConsumingFilter : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            RunAssertionFollowedBy(epService);
            RunAssertionAnd(epService);
            RunAssertionOr(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionFollowedBy(EPServiceProvider epService) {
            string[] fields = "a,b".Split(',');
            string pattern = "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean -> b=SupportBean@consume]";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(pattern).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", "E4"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E5", "E6"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionAnd(EPServiceProvider epService) {
            string[] fields = "a,b".Split(',');
            string pattern = "select a.TheString as a, b.TheString as b from pattern[every (a=SupportBean and b=SupportBean)]";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(pattern).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "E1"});
            epService.EPAdministrator.DestroyAllStatements();
    
            pattern = "select a.TheString as a, b.TheString as b from pattern [every (a=SupportBean and b=SupportBean(IntPrimitive=10)@consume(2))]";
            epService.EPAdministrator.CreateEPL(pattern).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E1"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E3", "E5"});
            epService.EPAdministrator.DestroyAllStatements();
    
            // test SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(pattern);
            Assert.AreEqual(pattern, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(pattern, stmt.Text);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "E1"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionOr(EPServiceProvider epService) {
            string[] fields = "a,b".Split(',');
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean or b=SupportBean] order by a asc",
                    new object[][]{new object[] {null, "E1"}, new object[] {"E1", null}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(1)] order by a asc",
                    new object[][]{new object[] {null, "E1"}, new object[] {"E1", null}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(2) or b=SupportBean@consume(1)] order by a asc",
                    new object[]{"E1", null});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2)] order by a asc",
                    new object[]{null, "E1"});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean or b=SupportBean@consume(2)] order by a asc",
                    new object[]{null, "E1"});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean@consume(1) or b=SupportBean] order by a asc",
                    new object[]{"E1", null});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean(IntPrimitive=11)@consume(1) or b=SupportBean] order by a asc",
                    new object[]{null, "E1"});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b from pattern[every a=SupportBean(IntPrimitive=10)@consume(1) or b=SupportBean] order by a asc",
                    new object[]{"E1", null});
    
            fields = "a,b,c".Split(',');
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(1) or b=SupportBean@consume(2) or c=SupportBean@consume(3)] order by a,b,c",
                    new object[][]{new object[] {null, null, "E1"}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(1) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new object[][]{new object[] {null, null, "E1"}, new object[] {null, "E1", null}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(2)] order by a,b,c",
                    new object[][]{new object[] {null, null, "E1"}, new object[] {null, "E1", null}, new object[] {"E1", null, null}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(2) or every c=SupportBean@consume(1)] order by a,b,c",
                    new object[][]{new object[] {null, "E1", null}, new object[] {"E1", null, null}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(2) or every b=SupportBean@consume(1) or every c=SupportBean@consume(2)] order by a,b,c",
                    new object[][]{new object[] {null, null, "E1"}, new object[] {"E1", null, null}});
    
            TryAssertion(epService, fields, "select a.TheString as a, b.TheString as b, c.TheString as c from pattern[every a=SupportBean@consume(0) or every b=SupportBean or every c=SupportBean] order by a,b,c",
                    new object[][]{new object[] {null, null, "E1"}, new object[] {null, "E1", null}, new object[] {"E1", null, null}});
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            TryInvalid(epService, "select * from pattern[every a=SupportBean@consume()]",
                    "Incorrect syntax near ')' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a closing parenthesis ')' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume()]]");
            TryInvalid(epService, "select * from pattern[every a=SupportBean@consume(-1)]",
                    "Incorrect syntax near '-' expecting any of the following tokens {IntegerLiteral, FloatingPointLiteral} but found a minus '-' at line 1 column 50, please check the filter specification within the pattern expression within the from clause [select * from pattern[every a=SupportBean@consume(-1)]]");
            TryInvalid(epService, "select * from pattern[every a=SupportBean@xx]",
                    "Error in expression: Unexpected pattern filter @ annotation, expecting 'consume' but received 'xx' [select * from pattern[every a=SupportBean@xx]]");
        }
    
        private void TryAssertion(EPServiceProvider epService, string[] fields, string pattern, Object expected) {
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(pattern).Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
    
            if (expected is object[][]) {
                EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, (object[][]) expected);
            } else {
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, (object[]) expected);
            }
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
    
    
} // end of namespace
