///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternExpressionText : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("SupportBean", typeof(SupportBean).FullName);
            configuration.AddEventType("A", typeof(SupportBean_A).Name);
            configuration.AddEventType("B", typeof(SupportBean_B).Name);
            configuration.AddEventType("C", typeof(SupportBean_C).Name);
            configuration.AddEventType("D", typeof(SupportBean_D).Name);
            configuration.AddEventType("E", typeof(SupportBean_E).Name);
            configuration.AddEventType("F", typeof(SupportBean_F).Name);
            configuration.AddEventType("G", typeof(SupportBean_G).Name);
        }
    
        public override void Run(EPServiceProvider epService) {
            TryAssertion(epService, "every a=SupportBean -> b=SupportBean@consume", null);
            TryAssertion(epService, "every a=SupportBean -> b=SupportBean@consume", null);
            TryAssertion(epService, "every a=SupportBean -> b=SupportBean@Consume(2)", null);
            TryAssertion(epService, "a=A -> b=B", null);
            TryAssertion(epService, "b=B and every d=D", null);
            TryAssertion(epService, "every b=B and d=B", null);
            TryAssertion(epService, "b=B and d=D", null);
            TryAssertion(epService, "every (b=B and d=D)", null);
            TryAssertion(epService, "every (b=B and every d=D)", null);
            TryAssertion(epService, "every b=B and every d=D", null);
            TryAssertion(epService, "every (every b=B and d=D)", null);
            TryAssertion(epService, "every a=A and d=D and b=B", null);
            TryAssertion(epService, "every (every b=B and every d=D)", null);
            TryAssertion(epService, "a=A and d=D and b=B", null);
            TryAssertion(epService, "every a=A and every d=D and b=B", null);
            TryAssertion(epService, "b=B and b=B", null);
            TryAssertion(epService, "every a=A and every d=D and every b=B", null);
            TryAssertion(epService, "every (a=A and every d=D and b=B)", null);
            TryAssertion(epService, "every (b=B and b=B)", null);
            TryAssertion(epService, "every b=B", null);
            TryAssertion(epService, "b=B", null);
            TryAssertion(epService, "every (every (every b=B))", "every every every b=B");
            TryAssertion(epService, "every (every b=B())", "every every b=B");
            TryAssertion(epService, "b=B -> d=D or not d=D", null);
            TryAssertion(epService, "b=B -> (d=D or not d=D)", "b=B -> d=D or not d=D");
            TryAssertion(epService, "b=B -[1000]> d=D or not d=D", null);
            TryAssertion(epService, "b=B -> every d=D", null);
            TryAssertion(epService, "b=B -> d=D", null);
            TryAssertion(epService, "b=B -> not d=D", null);
            TryAssertion(epService, "b=B -[1000]> not d=D", null);
            TryAssertion(epService, "every b=B -> every d=D", null);
            TryAssertion(epService, "every b=B -> d=D", null);
            TryAssertion(epService, "every b=B -[10]> d=D", null);
            TryAssertion(epService, "every (b=B -> every d=D)", null);
            TryAssertion(epService, "every (a_1=A -> b=B -> a_2=A)", null);
            TryAssertion(epService, "c=C -> d=D -> a=A", null);
            TryAssertion(epService, "every (a_1=A -> b=B -> a_2=A)", null);
            TryAssertion(epService, "every (a_1=A -[10]> b=B -[10]> a_2=A)", null);
            TryAssertion(epService, "every (every a=A -> every b=B)", null);
            TryAssertion(epService, "every (a=A -> every b=B)", null);
            TryAssertion(epService, "a=A(id='A2') until D", "a=A(id=\"A2\") until D");
            TryAssertion(epService, "b=B until a=A", null);
            TryAssertion(epService, "b=B until D", null);
            TryAssertion(epService, "(a=A or b=B) until d=D", null);
            TryAssertion(epService, "(a=A or b=B) until (g=G or d=D)", null);
            TryAssertion(epService, "a=A until G", null);
            TryAssertion(epService, "[2] a=A", null);
            TryAssertion(epService, "[1:1] a=A", null);
            TryAssertion(epService, "[4] (a=A or b=B)", null);
            TryAssertion(epService, "[2] b=B until a=A", null);
            TryAssertion(epService, "[2:2] b=B until g=G", null);
            TryAssertion(epService, "[:4] b=B until g=G", null);
            TryAssertion(epService, "[1:] b=B until g=G", null);
            TryAssertion(epService, "[1:2] b=B until a=A", null);
            TryAssertion(epService, "c=C -> [2] b=B -> d=D", null);
            TryAssertion(epService, "d=D until timer:Interval(7 sec)", "d=D until timer:Interval(7 seconds)");
            TryAssertion(epService, "every (d=D until b=B)", null);
            TryAssertion(epService, "every d=D until b=B", null);
            TryAssertion(epService, "(every d=D) until b=B", "every d=D until b=B");
            TryAssertion(epService, "a=A until (every (timer:Interval(6 sec) and not A))", "a=A until every (timer:Interval(6 seconds) and not A)");
            TryAssertion(epService, "[2] (a=A or b=B)", null);
            TryAssertion(epService, "every [2] a=A", "every ([2] a=A)");
            TryAssertion(epService, "every [2] a=A until d=D", "every ([2] a=A) until d=D");  // every has precedence; ESPER-339
            TryAssertion(epService, "[3] (a=A or b=B)", null);
            TryAssertion(epService, "[4] (a=A or b=B)", null);
            TryAssertion(epService, "(a=A until b=B) until c=C", "a=A until b=B until c=C");
            TryAssertion(epService, "b=B and not d=D", null);
            TryAssertion(epService, "every b=B and not g=G", null);
            TryAssertion(epService, "every b=B and not g=G", null);
            TryAssertion(epService, "b=B and not a=A(id=\"A1\")", null);
            TryAssertion(epService, "every (b=B and not b3=B(id=\"B3\"))", null);
            TryAssertion(epService, "every (b=B or not D)", null);
            TryAssertion(epService, "every (every b=B and not B)", null);
            TryAssertion(epService, "every (b=B and not B)", null);
            TryAssertion(epService, "(b=B -> d=D) and G", null);
            TryAssertion(epService, "(b=B -> d=D) and (a=A -> e=E)", null);
            TryAssertion(epService, "b=B -> (d=D() or a=A)", "b=B -> d=D or a=A");
            TryAssertion(epService, "b=B -> ((d=D -> a=A) or (a=A -> e=E))", "b=B -> (d=D -> a=A) or (a=A -> e=E)");
            TryAssertion(epService, "(b=B -> d=D) or a=A", null);
            TryAssertion(epService, "(b=B and d=D) or a=A", "b=B and d=D or a=A");
            TryAssertion(epService, "a=A or a=A", null);
            TryAssertion(epService, "a=A or b=B or c=C", null);
            TryAssertion(epService, "every b=B or every d=D", null);
            TryAssertion(epService, "a=A or b=B", null);
            TryAssertion(epService, "a=A or every b=B", null);
            TryAssertion(epService, "every a=A or d=D", null);
            TryAssertion(epService, "every (every b=B or d=D)", null);
            TryAssertion(epService, "every (b=B or every d=D)", null);
            TryAssertion(epService, "every (every d=D or every b=B)", null);
            TryAssertion(epService, "timer:At(10,8,*,*,*)", null);
            TryAssertion(epService, "every timer:At(*/5,*,*,*,*,*)", null);
            TryAssertion(epService, "timer:At(10,9,*,*,*,10) or timer:At(30,9,*,*,*,*)", null);
            TryAssertion(epService, "b=B(id=\"B3\") -> timer:At(20,9,*,*,*,*)", null);
            TryAssertion(epService, "timer:At(59,8,*,*,*,59) -> d=D", null);
            TryAssertion(epService, "timer:At(22,8,*,*,*) -> b=B -> timer:At(55,*,*,*,*)", null);
            TryAssertion(epService, "timer:At(40,*,*,*,*,1) and b=B", null);
            TryAssertion(epService, "timer:At(40,9,*,*,*,1) or d=D", null);
            TryAssertion(epService, "timer:At(22,8,*,*,*) -> b=B -> timer:At(55,8,*,*,*)", null);
            TryAssertion(epService, "timer:At(22,8,*,*,*,1) where timer:Within(30 minutes)", null);
            TryAssertion(epService, "timer:At(*,9,*,*,*) and timer:At(55,*,*,*,*)", null);
            TryAssertion(epService, "timer:At(40,8,*,*,*,1) and b=B", null);
            TryAssertion(epService, "timer:Interval(2 seconds)", null);
            TryAssertion(epService, "timer:Interval(2.001)", null);
            TryAssertion(epService, "timer:Interval(2999 milliseconds)", null);
            TryAssertion(epService, "timer:Interval(4 seconds) -> b=B", null);
            TryAssertion(epService, "b=B -> timer:Interval(0)", null);
            TryAssertion(epService, "b=B -> timer:Interval(6.0) -> d=D", null);
            TryAssertion(epService, "every (b=B -> timer:Interval(2.0) -> d=D)", null);
            TryAssertion(epService, "b=B or timer:Interval(2.001)", null);
            TryAssertion(epService, "b=B or timer:Interval(8.5)", null);
            TryAssertion(epService, "timer:Interval(8.5) or timer:Interval(7.5)", null);
            TryAssertion(epService, "timer:Interval(999999 milliseconds) or g=G", null);
            TryAssertion(epService, "b=B and timer:Interval(4000 milliseconds)", null);
            TryAssertion(epService, "b=B(id=\"B1\") where timer:Within(2 seconds)", null);
            TryAssertion(epService, "(every b=B) where timer:Within(2.001)", null);
            TryAssertion(epService, "every (b=B) where timer:Within(6.001)", "every b=B where timer:Within(6.001)");
            TryAssertion(epService, "b=B -> d=D where timer:Within(4001 milliseconds)", null);
            TryAssertion(epService, "b=B -> d=D where timer:Within(4 seconds)", null);
            TryAssertion(epService, "every (b=B where timer:Within(4.001) and d=D where timer:Within(6.001))", null);
            TryAssertion(epService, "every b=B -> d=D where timer:Within(4000 seconds)", null);
            TryAssertion(epService, "every b=B -> every d=D where timer:Within(4000 seconds)", null);
            TryAssertion(epService, "b=B -> d=D where timer:Within(3999 seconds)", null);
            TryAssertion(epService, "every b=B -> (every d=D) where timer:Within(2001)", null);
            TryAssertion(epService, "every (b=B -> d=D) where timer:Within(6001)", null);
            TryAssertion(epService, "b=B where timer:Within(2000) or d=D where timer:Within(6000)", null);
            TryAssertion(epService, "(b=B where timer:Within(2000) or d=D where timer:Within(6000)) where timer:Within(1999)", null);
            TryAssertion(epService, "every (b=B where timer:Within(2001) and d=D where timer:Within(6001))", null);
            TryAssertion(epService, "b=B where timer:Within(2001) or d=D where timer:Within(6001)", null);
            TryAssertion(epService, "B where timer:Within(2000) or d=D where timer:Within(6001)", null);
            TryAssertion(epService, "every b=B where timer:Within(2001) and every d=D where timer:Within(6001)", null);
            TryAssertion(epService, "(every b=B) where timer:Within(2000) and every d=D where timer:Within(6001)", null);
            TryAssertion(epService, "b=B(id=\"B1\") where timer:Withinmax(2 seconds,100)", null);
            TryAssertion(epService, "(every b=B) where timer:Withinmax(4.001,2)", null);
            TryAssertion(epService, "every b=B where timer:Withinmax(2.001,4)", null);
            TryAssertion(epService, "every (b=B where timer:Withinmax(2001,0))", "every b=B where timer:Withinmax(2001,0)");
            TryAssertion(epService, "(every b=B) where timer:Withinmax(4.001,2)", null);
            TryAssertion(epService, "every b=B -> d=D where timer:Withinmax(4000 milliseconds,1)", null);
            TryAssertion(epService, "every b=B -> every d=D where timer:Withinmax(4000,1)", null);
            TryAssertion(epService, "every b=B -> (every d=D) where timer:Withinmax(1 days,3)", null);
            TryAssertion(epService, "a=A -> (every b=B) while (b.id!=\"B3\")", null);
            TryAssertion(epService, "(every b=B) while (b.id!=\"B1\")", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive,1) a=SupportBean(theString like \"A%\")", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive,1 seconds) a=SupportBean(theString like \"A%\")", null);
            TryAssertion(epService, "every-Distinct(intPrimitive) a=SupportBean", null);
            TryAssertion(epService, "[2] every-Distinct(a.intPrimitive) a=SupportBean", null);
            TryAssertion(epService, "every-Distinct(a[0].intPrimitive) ([2] a=SupportBean)", null);
            TryAssertion(epService, "every-Distinct(a[0].intPrimitive,a[0].intPrimitive,1 hours) ([2] a=SupportBean)", null);
            TryAssertion(epService, "(every-Distinct(a.intPrimitive) a=SupportBean) where timer:Within(10 seconds)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive) a=SupportBean where timer:Within(10)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive,1 hours) a=SupportBean where timer:Within(10)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive,b.intPrimitive) (a=SupportBean(theString like \"A%\") and b=SupportBean(theString like \"B%\"))", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive) (a=SupportBean and not SupportBean)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive,1 hours) (a=SupportBean and not SupportBean)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive+b.intPrimitive,1 hours) (a=SupportBean -> b=SupportBean)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive) a=SupportBean -> b=SupportBean(intPrimitive=a.intPrimitive)", null);
            TryAssertion(epService, "every-Distinct(a.intPrimitive) a=SupportBean -> every-Distinct(b.intPrimitive) b=SupportBean(theString like \"B%\")", null);
        }
    
        private void TryAssertion(EPServiceProvider epService, string patternText, string expectedIfDifferent) {
            string epl = "@Name('A') select * from pattern [" + patternText + "]";
            TryAssertionEPL(epService, epl, patternText, expectedIfDifferent);
    
            epl = "@Audit @Name('A') select * from pattern [" + patternText + "]";
            TryAssertionEPL(epService, epl, patternText, expectedIfDifferent);
        }
    
        private void TryAssertionEPL(EPServiceProvider epService, string epl, string patternText, string expectedIfDifferent) {
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            EPStatementSPI spi = (EPStatementSPI) epService.EPAdministrator.Create(model);
            StatementSpecCompiled spec = ((EPServiceProviderSPI) epService).StatementLifecycleSvc.GetStatementSpec(spi.StatementId);
            PatternStreamSpecCompiled pattern = (PatternStreamSpecCompiled) spec.StreamSpecs[0];
            var writer = new StringWriter();
            pattern.EvalFactoryNode.ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
            if (expectedIfDifferent == null) {
                Assert.AreEqual(patternText, writer.ToString());
            } else {
                Assert.AreEqual(expectedIfDifferent, writer.ToString());
            }
            spi.Destroy();
        }
    }
} // end of namespace
