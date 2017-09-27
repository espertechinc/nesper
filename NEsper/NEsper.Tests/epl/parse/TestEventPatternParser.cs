///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.collection;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.epl.parse;

using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.epl.parse
{
    [TestFixture]
    public class TestEventPatternParser 
    {
        [Test]
        public void TestDisplayAST()
        {
            String expression = "A -> [10] B until C -> D";
    
            Log.Debug(".testDisplayAST parsing: " + expression);
            Pair<ITree, CommonTokenStream> ast = Parse(expression);
            SupportParserHelper.DisplayAST(ast.First);
        }
    
        [Test]
        public void TestInvalidCases()
        {
            // Base vocabulary
            AssertIsInvalid("a(");
            AssertIsInvalid("a)");
            AssertIsInvalid("a((");
            AssertIsInvalid("a())");
            AssertIsInvalid("a(a=)");
            AssertIsInvalid("a(a=2 3)");
            AssertIsInvalid("a(a=2a)");
            AssertIsInvalid("a(a=\"2)");
            AssertIsInvalid("a(a=1x5)");
            AssertIsInvalid("a(a=)");
            AssertIsInvalid("a(=2)");
            AssertIsInvalid("a(a=2b=3)");
            AssertIsInvalid("a(a=2,)");
            AssertIsInvalid("a(a=2,b=)");
            AssertIsInvalid("e(a2>)");
            AssertIsInvalid("e(a2<=<4)");
            AssertIsInvalid("e(a2>=>6)");
            AssertIsInvalid("e(a in [])");
            AssertIsInvalid("e(a in [1:])");
            AssertIsInvalid("e(a in [:5])");
            AssertIsInvalid("e(a in [:5)))");
            AssertIsInvalid("e(a in ((:5)))");
            AssertIsInvalid("e(a in [[:5)");
            AssertIsInvalid("e(a in [1:5:8]");
            AssertIsInvalid("e(a in [1,,2])");
            AssertIsInvalid("e(a in [1:2]");
            AssertIsInvalid("e(a in [1:2] b=3)");
    
            // Followed by
            AssertIsInvalid("every e() ->");
            AssertIsInvalid("every e() -> b() ->");
            AssertIsInvalid("e() -> b)");
            AssertIsInvalid("e() -> -> b()");
            AssertIsInvalid("e() -> every -> b()");
            AssertIsInvalid("-> every e()");
            AssertIsInvalid("e() -> f() -> g() -> every");
    
            // Or
            AssertIsInvalid("e() or");
            AssertIsInvalid("or e()");
            AssertIsInvalid("a() or or e()");
            AssertIsInvalid("a() or e() or");
            AssertIsInvalid("f() or f() or f() o");
    
            // And
            AssertIsInvalid("e() and");
            AssertIsInvalid("and e()");
            AssertIsInvalid("a() and and e()");
            AssertIsInvalid("a() and e() and");
            AssertIsInvalid("f() and f() and f() an");
    
            // Not
            AssertIsInvalid("not e(");
            AssertIsInvalid("n o t e()");
            AssertIsInvalid("note a()");
            AssertIsInvalid("a() not");
            AssertIsInvalid("not f() not");
            AssertIsInvalid("not not f()");
            AssertIsInvalid("every not f()");
            AssertIsInvalid("not every not f()");
            AssertIsInvalid("every every not f()");
    
            // where a:b
            AssertIsInvalid("a() where a:b(33s)");
            AssertIsInvalid("a() where a:b('+1E4)");
            AssertIsInvalid("a() where a:b(22L2)");
            AssertIsInvalid("a() where a:b(2) or b() every where a:b(3)");
            AssertIsInvalid("a() where a:b(2) or b() not where a:b(3)");
            AssertIsInvalid("where a:b(5) a()");
            AssertIsInvalid("every where a:b(5) a()");
            AssertIsInvalid("every a() where a:b(1) every");
            AssertIsInvalid("(every a()) (where a:b(1))");
            AssertIsInvalid("every ((every a(id=\"A1\")) where a:b(10l) and c()");
    
            AssertIsInvalid("timer:interval(10) timer:interval(20)");
            AssertIsInvalid("a() timer:interval(10)");
            AssertIsInvalid("timer:interval(10) b()");
            AssertIsInvalid("timer:interval(10) -> b() or timer:interval(20))");
            AssertIsInvalid("timer:interval(1x)");
            AssertIsInvalid("a() or timer:interval(10) b()");
            AssertIsInvalid("unmatched timer:interval()");
    
            // At
            AssertIsInvalid("timer:at(,*,*,*,*)");
            AssertIsInvalid("timer:at(,*,*,*,*,*)");
            AssertIsInvalid("timer:at(*,,*,*,*)");
            AssertIsInvalid("timer:at(*,*,*,*,)");
            AssertIsInvalid("timer:at(*,*,*,*,*,)");
            AssertIsInvalid("timer:at(*,*,*,*,*,)");
            AssertIsInvalid("timer:at(*,*,*,*,1G)");
            AssertIsInvalid("timer:at(1a,*,*,*,*)");
            AssertIsInvalid("timer:at(*,*,=2*,*,*)");
            AssertIsInvalid("timer:at(*,*,*,*,1a=3) or b()");
            AssertIsInvalid("timer:at(*,[1a],*,*,*)");
            AssertIsInvalid("timer:at(*,[1a=3],*,*,*)");
            AssertIsInvalid("timer:at(*,[2,,5],*,*,*)");
            AssertIsInvalid("timer:at(*,[,2,5,7],*,*,*)");
            AssertIsInvalid("timer:at(*,[2,5,7,],*,*,*)");
            AssertIsInvalid("timer:at(*,[2,5][2,4],*,*,*)");
            AssertIsInvalid("timer:at(*,*,1x:1y,*,*)");
            AssertIsInvalid("timer:at(*,*,1:1y,*,*)");
            AssertIsInvalid("timer:at(*,*,1x:2,*,*)");
            AssertIsInvalid("timer:at(*,*,:2,*,*)");
            AssertIsInvalid("timer:at(*,*,2:,*,*)");
            AssertIsInvalid("timer:at(*,*,:,*,*)");
            AssertIsInvalid("timer:at(*,*,0/*,*,*)");
            AssertIsInvalid("timer:at(*,*,*/*,*,*)");
            AssertIsInvalid("timer:at([2:2x],*,*,*,*)");
            AssertIsInvalid("timer:at(3:3],*,*,*,*)");
            AssertIsInvalid("timer:at(3:3:3,*,*,*,*)");
            AssertIsInvalid("timer:at([3,*,*,*,*)");
            AssertIsInvalid("timer:at([3,*,*,*,*],q)");
            AssertIsInvalid("timer:at(3,3,3,*,*,*])");
    
            // Custom EventObserver
            AssertIsInvalid("a:b(");
            AssertIsInvalid("a:b)");
            AssertIsInvalid("a:b('a',, 10)");
    
            // Use of results
            AssertIsInvalid("a=A() -> b=B(pb1=a.)");
            AssertIsInvalid("a=A() -> b=B(pb1=.sss)");
            AssertIsInvalid("a=A() -> b=B(pb1=1.sss)");
            AssertIsInvalid("a=A() -> b=B(pb1=s.1)");
            AssertIsInvalid("a=A() -> b=B(pb1=s.1)");
            AssertIsInvalid("a=A() -> b=B(pb1 in [s.1:a.s2])");
            AssertIsInvalid("a=A() -> b=B(pb1 in [s.x:a.1])");
    
            // Use of index, mapped, nested and combined properties
            AssertIsInvalid("A(x[0 = 1)");
            AssertIsInvalid("A(x 0] = 1)");
            AssertIsInvalid("A(x(b = 1)");
            AssertIsInvalid("A(x y) = 1)");
            AssertIsInvalid("A(x[0][1] = 1)");
            AssertIsInvalid("A(x[0]. = 1)");
            AssertIsInvalid("A(x. = 1)");
            AssertIsInvalid("A(x.y. = 1)");
            AssertIsInvalid("A(.y = 1)");
            AssertIsInvalid("A(y[0](g) = 1)");
            AssertIsInvalid("A(y(g)..z = 1)");
        }
    
        [Test]
        public void TestValidCases()
        {
            String className = typeof(SupportBean).FullName;
    
            // Base vocabulary
            AssertIsValid(className);
            AssertIsValid(className + "(IntPrimitive=444)");
            AssertIsValid(className + "(TheString=\"\")");
            AssertIsValid(className + "(TheString=\"ddddd\")");
            AssertIsValid(className + "(DoubleBoxed=444E43)");
            AssertIsValid(className + "(DoubleBoxed=1.2345)");
            AssertIsValid(className + "(DoubleBoxed=0)");
            AssertIsValid(className + "(DoubleBoxed=-394847575)");
            AssertIsValid(className + "(DoublePrimitive=37374)");
            AssertIsValid(className + "(DoublePrimitive=+2)");
            AssertIsValid(className + "(BoolPrimitive=false)");
            AssertIsValid(className + "(BoolPrimitive=true)");
            AssertIsValid(className + "(BoolPrimitive=true,IntPrimitive=4,TheString=\"d\")");
            AssertIsValid(className + "(TheString=\"b\",\nByteBoxed=3)");
            AssertIsValid(className + "(IntPrimitive<5,IntBoxed>3,DoublePrimitive<=2,DoubleBoxed>=5)");
            AssertIsValid(className + " (  IntPrimitive<5, \n IntBoxed>3, \n DoublePrimitive<=2,\t DoubleBoxed>=5)");
            AssertIsValid(className + "(BoolBoxed=true)");
            AssertIsValid(className + "(IntPrimitive in [1:2])");
            AssertIsValid(className + "(IntPrimitive in (1:2])");
            AssertIsValid(className + "(IntPrimitive in (1:2))");
            AssertIsValid(className + "(IntPrimitive in [1:2))");
            AssertIsValid(className + "(IntPrimitive in (-34243232:+342342343])");
            AssertIsValid(className + "(LongPrimitive in (-34243232L:+342342343l])");
            AssertIsValid(className + "(DoublePrimitive in [1E20:-1])");
            AssertIsValid(className + "(DoublePrimitive in [45775.2244502:1.345674))");
            AssertIsValid(className + "(LongPrimitive in [   1   :  2  ])");
            AssertIsValid(className + "(IntPrimitive in [1:2], LongPrimitive=3)");
            AssertIsValid(className + "(IntPrimitive=3, string=\"a2\", LongPrimitive in [1:2], DoubleBoxed=3)");
            AssertIsValid(className + "(IntPrimitive in [2:10])");
            AssertIsValid(className + "(DoubleBoxed in [-0.00001:-0.1E2])");
            AssertIsValid(className + "(DoubleBoxed in [60.0:61.0])");
            AssertIsValid("e(a2>b3)");
            AssertIsValid("e(b3)");
            AssertIsValid("e(a2<d4)");
            AssertIsValid("e(a2<>8)");
            AssertIsValid("A(x(1) = 1)");
            AssertIsValid("A(x(aaa) = 1)");
    
            // With name
            AssertIsValid("se1=" + className);
            AssertIsValid("er2=" + className + "(IntPrimitive=444)");
            AssertIsValid("x3=" + className + "(IntPrimitive in [1:2])");
            AssertIsValid("gamma=" + className + "(DoubleBoxed in [-0.00001:-0.1E2])");
    
            // Every
            AssertIsValid("every " + className);
            AssertIsValid("every " + className + "(TheString=\"b\",\nIntPrimitive=3)");
            AssertIsValid("(every " + className + "())");
            AssertIsValid("every(" + className + "())");
            AssertIsValid("((every " + className + "()))");
            AssertIsValid("every(every((every " + className + "())))");
    
            // Followed by
            AssertIsValid(className + "() -> " + className);
            AssertIsValid("every " + className + "() -> every " + className);
            AssertIsValid(className + "() -> every " + className);
            AssertIsValid("every " + className + "() -> " + className);
            AssertIsValid("every " + className + "() -> every " + className);
            AssertIsValid(className + "() -> " + className + "() -> " + className);
    
            // Or
            AssertIsValid(className + "() or " + className);
            AssertIsValid(className + "() or " + className + "() or " + className);
            AssertIsValid(className + "() -> " + className + "() or " + className);
            AssertIsValid(className + "() -> " + className + "() -> " + className + "() or " + className);
            AssertIsValid("every " + className + "() -> every " + className + "() or every " + className);
    
            // And
            AssertIsValid(className + " and " + className);
            AssertIsValid(className + " and " + className + " and " + className);
            AssertIsValid(className + " -> " + className + " and " + className);
            AssertIsValid(className + " -> " + className + " -> " + className + "() and " + className);
            AssertIsValid("every "  + className + "() -> every " + className + "() and every " + className);
    
            // Not
            AssertIsValid("not " + className);
            AssertIsValid("not (" + className + "())");
            AssertIsValid("every(not (" + className + "()))");
            AssertIsValid(className + "() and not " + className);
            AssertIsValid(className + "() and " + className + "() and not " + className);
            AssertIsValid("not " + className + "(IntPrimitive=3) and " + className + "(DoubleBoxed=3)");
            AssertIsValid("(" + className + "() -> " + className + "()) and not " + className);
            AssertIsValid("((" + className + "() and not " + className + "()) and not " + className + "())");
            AssertIsValid(className + "() and not (" + className + "() -> " + className + "())");
            AssertIsValid("every " + className + "() -> every " + className + "() and not " + className);
            AssertIsValid("(not " + className + "())");
            AssertIsValid("not (" + className + "())");
    
            // where
            AssertIsValid(className + "() where a:b(5)");
            AssertIsValid(className + "() where a:b(100354)");
            AssertIsValid(className + "() where a:b(1595950)");
            AssertIsValid("(" + className + "()) where a:b(5)");
            AssertIsValid("every " + className + "() where a:b(45)");
            AssertIsValid("not (" + className + "()) where a:b(5)");
            AssertIsValid("every ((" + className + "())) where a:b(5)");
            AssertIsValid("(every " + className + "()) where a:b(1000000)");
            AssertIsValid("every ((every " + className + "(TheString=\"A1\")) where a:b(10))");
            AssertIsValid(className + "() -> " + className + "() where a:b(10)");
            AssertIsValid(className + "() -> (" + className + "() where a:b(10))");
            AssertIsValid(className + "() or " + className + "() where a:b(10)");
            AssertIsValid(className + "() or (" + className + "() where a:b(10))");
            AssertIsValid("((" + className + "() where a:b(10)) or (" + className + "() where a:b(5))) where a:b(15)");
    
            // timer:interval
            AssertIsValid("timer:interval(5)");
            AssertIsValid("timer:interval(100354)");
            AssertIsValid("timer:interval(15959500354)");
            AssertIsValid("timer:interval(1595950)");
            AssertIsValid("(timer:interval(5))");
            AssertIsValid("every timer:interval(5)");
            AssertIsValid("not timer:interval(5)");
            AssertIsValid("every (timer:interval(10) where a:b(5))");
            AssertIsValid("every timer:interval(10) where a:b(10)");
            AssertIsValid("every ((every timer:interval(4)) where a:b(10))");
            AssertIsValid(className + "() -> timer:interval(10)");
            AssertIsValid("timer:interval(20) -> timer:interval(10)");
            AssertIsValid("timer:interval(20) -> " + className + "(IntPrimitive=3)");
            AssertIsValid(className + "() -> every (timer:interval(10))");
            AssertIsValid("timer:interval(30) or " + className + "() where a:b(10)");
            AssertIsValid("timer:interval(30) or every timer:interval(40) where a:b(10)");
            AssertIsValid(className + "() or (timer:interval(30) where a:b(10))");
            AssertIsValid(className + "() and timer:interval(30)");
            AssertIsValid("((" + className + "() where a:b(10)) or (" + className + "() where a:b(5))) where a:b(15)");
    
            // timer:at
            AssertIsValid("timer:at(*,*,*,*,*)");
            AssertIsValid("timer:at(*,*,*,*,*,*)");
            AssertIsValid("timer:at(10,20,10,10,1)");
            AssertIsValid("timer:at(0,00,1,1,0)");
            AssertIsValid("timer:at(*,1,*,1,*)");
            AssertIsValid("timer:at(1,0,*,11,1,0)");
            AssertIsValid("timer:at([1],*,*,*,*,[2,3])");
            AssertIsValid("timer:at([0,0,0,0,0,  0,  1],[3,5,7],[1],[1,2],[3,4],[2,3])");
            AssertIsValid("timer:at(6:10,*,*,*,*)");
            AssertIsValid("timer:at(6:10,1:2,1:4,2:3,0:0)");
            AssertIsValid("timer:at(00000:00000,*,   1 : 2  ,*,*,9:10)");
            AssertIsValid("timer:at(*/1, *, *, *, *)");
            AssertIsValid("timer:at(*/1, * / 5 , */ 1, */ 1 , */1010210993,*/7)");
            AssertIsValid("timer:at(40,3:12,[2,3],*,0:3)");
            AssertIsValid("timer:at([2, */1, 1:5], [6:6, 6:6], [*/2, */3], [2,*/2,2,2:2], *, *)");
    
            // Custom EventObserver
            AssertIsValid("a:b()");
            AssertIsValid("a:b('a', 10, false, true, 20, 0.111, 1E6)");
    
            // Use of results
            AssertIsValid("a=A() -> b=B(pb1=a.pa1)");
            AssertIsValid("a=A() -> b=B(pb1 = a.pa1)");
            AssertIsValid("a=A(x=y.a) -> b=B(pb1 = a.pa1, o=5, p=q.a)");
            AssertIsValid("a=A() -> b=B(pb1 in [a.p1 : a.p2])");
            AssertIsValid("a=A() -> b=B(pb1 in [100.1 : a.p2])");
            AssertIsValid("a=A() -> b=B(pb1 in [a.xx : 200])");
    
            // Use of index, mapped, nested and combined properties
            AssertIsValid("A(x[0] = 1)");
            AssertIsValid("A(x[565656] = 1)");
            AssertIsValid("A(x[565656] = 1, y[2]=2)");
            AssertIsValid("A(x('home') = 1)");
            AssertIsValid("A(x.y = 1)");
            AssertIsValid("A(x.y.z = 1)");
            AssertIsValid("A(x[1].y('g').z('r').zz = 1)");
            AssertIsValid("A(x.y[11111].b.b = 1)");
            AssertIsValid("A(x('1') = 1)");
            AssertIsValid("A(x(\"1\") = 1)");
            AssertIsValid("B(a('aa').b.c[1].d.e(\"ee\")=2)");
            AssertIsValid("a=X -> b=Y(id=a.Indexed[0])");
    
            // intervals specs
            AssertIsValid("timer:interval(5 seconds)");
            AssertIsValid("timer:interval(5 seconds 3.3 milliseconds)");
            AssertIsValid("timer:interval(1 days 5 seconds 3.3 milliseconds)");
            AssertIsValid("timer:interval(1 days 3.3 milliseconds)");
            AssertIsValid("timer:interval(1 day 1E1 minute 3.3 milliseconds)");
            AssertIsValid("timer:interval(1.0001 hours 1E1 minute)");
            AssertIsValid("timer:interval(1.1 minute 2.2 seconds)");
            AssertIsValid("A where timer:within(5 seconds)");
            AssertIsValid("A where timer:within(1 days 1 milliseconds)");
            AssertIsValid("A where timer:within(100 days 0.00001 millisecond)");
            AssertIsValid("A where timer:within(100 hours 3 minutes 1.00001 millisecond)");
    
            // match until
            AssertIsValid("A until B");
            AssertIsValid("every A until B");
            AssertIsValid("every (A until B)");
            AssertIsValid("every ((A where timer:within(10 sec)) until (B where timer:within(10 sec)))");
            AssertIsValid("(A or B) until (B or C)");
            AssertIsValid("(A -> B) until (B -> C)");
            AssertIsValid("(A and B) until (B and C)");
            AssertIsValid("(a=A until b=B) or (d=D until e=E)");
            AssertIsValid("a=A until b=B or d=D until e=E");
            AssertIsValid("((a=A or X=x) until (b=B)) -> ((d=D) until (e=E and e=E))");
            AssertIsValid("every (A until B)");
            AssertIsValid("A -> B until C -> D");
            AssertIsValid("B until C -> B until C -> B until C");
            AssertIsValid("(B until B1) until (D until C) -> E");
            AssertIsValid("[1] A until B");
            AssertIsValid("[1:] A until B");
            AssertIsValid("[1 :] A until B");
            AssertIsValid("[:2] A until B");
            AssertIsValid("[: 2] A until B");
            AssertIsValid("[0 : 10] A until B");
            AssertIsValid("[0:10] A until B");
            AssertIsValid("[0 : 10] A until B");
            AssertIsValid("[5] A");   // no until
            
            // annotation
            AssertIsValid("@SomeTag(value='val', val2='val') [5] A");
            
            //every-distinct
            AssertIsValid("every-distinct(value, bcd) A");
            AssertIsValid("every-distinct(value, bcd) [5] A");
        }
    
        [Test]
        public void TestParserNodeGeneration()
        {
            String expression = "a(m=1) -> not b() or every c() and d() where a:b (5) and timer:interval(10)";
    
            Log.Debug(".testParserNodeGeneration parsing: " + expression);
            Pair<ITree, CommonTokenStream> parsed = Parse(expression);
            ITree ast = parsed.First;
            SupportParserHelper.DisplayAST(ast);
        }
    
        private void AssertIsValid(String text)
        {
            Log.Debug(".assertIsValid Trying text=" + text);
            Pair<ITree, CommonTokenStream> ast = Parse(text);
            Log.Debug(".assertIsValid success, tree walking...");
    
            SupportParserHelper.DisplayAST(ast.First);
            Log.Debug(".assertIsValid done");
        }
    
        private void AssertIsInvalid(String text)
        {
            Log.Debug(".assertIsInvalid Trying text=" + text);
    
            try
            {
                Parse(text);
                Assert.IsFalse(true);
            }
            catch (Exception ex)
            {
                Log.Debug(".assertIsInvalid Expected ParseException exception was thrown and ignored, message=" + ex.Message);
            }
        }
    
        private Pair<ITree, CommonTokenStream> Parse(String expression)
        {
            return SupportParserHelper.ParsePattern(expression);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
