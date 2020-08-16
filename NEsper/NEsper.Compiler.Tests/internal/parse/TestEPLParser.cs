///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.compiler.@internal.parse
{
	[TestFixture]
	public class TestEPLParser : AbstractCompilerTest
	{

		[Test]
		public void TestDisplayAST()
		{
			var expression = "select * from A where exp > ANY (select a from B)";

			Log.Debug(".testDisplayAST parsing: " + expression);
			var ast = Parse(expression);
			SupportParserHelper.DisplayAST(ast.First);

			Log.Debug(".testDisplayAST walking...");
			var listener = SupportEPLTreeWalkerFactory.MakeWalker(container, ast.Second);

			var walker = new ParseTreeWalker(); // create standard walker
			walker.Walk(listener, (IParseTree) ast.First); // initiate walk of tree with listener
		}

		[Test]
		public void TestInvalidCases()
		{
			var className = typeof(SupportBean).Name;

			AssertIsInvalid(className + "(val=10000).");
			AssertIsInvalid("select * from com.xxx().std:win(3) where a not is null");
			AssertIsInvalid(className + "().a:someview");
			AssertIsInvalid(className + "().a:someview(");
			AssertIsInvalid(className + "().a:someview)");
			AssertIsInvalid(className + "().lenght()");
			AssertIsInvalid(className + "().:lenght()");
			AssertIsInvalid(className + "().win:lenght(0,)");
			AssertIsInvalid(className + "().win:lenght(,0)");
			AssertIsInvalid(className + "().win:lenght(0,0,)");
			AssertIsInvalid(className + "().win:lenght(0,0,\")");
			AssertIsInvalid(className + "().win:lenght(\"\"5)");
			AssertIsInvalid(className + "().win:lenght(,\"\")");
			AssertIsInvalid(className + "().win:lenght.(,\"\")");
			AssertIsInvalid(className + "().win:lenght().");
			AssertIsInvalid(className + "().win:lenght().lenght");
			AssertIsInvalid(className + "().win:lenght().lenght(");
			AssertIsInvalid(className + "().win:lenght().lenght)");
			AssertIsInvalid(className + "().win:lenght().lenght().");
			AssertIsInvalid(className + "().win:lenght().lenght().lenght");
			AssertIsInvalid(className + "().win:lenght({}))");
			AssertIsInvalid(className + "().win:lenght({\"s\")");
			AssertIsInvalid(className + "().win:lenght(\"s\"})");
			AssertIsInvalid(className + "().win:lenght({{\"s\"})");
			AssertIsInvalid(className + "().win:lenght({{\"s\"}})");
			AssertIsInvalid(className + "().win:lenght({\"s\"}})");
			AssertIsInvalid(className + "().win:lenght('s\"");
			AssertIsInvalid(className + "().win:lenght(\"s')");

			AssertIsInvalid("select * from A.std:win(3) where a not is null");
			AssertIsInvalid("select * from com.xxx().std:win(3) where a = not null");
			AssertIsInvalid("select * from com.xxx().std:win(3) where not not");
			AssertIsInvalid("select * from com.xxx().std:win(3) where not ||");
			AssertIsInvalid("select * from com.xxx().std:win(3) where a ||");
			AssertIsInvalid("select * from com.xxx().std:win(3) where || a");

			AssertIsInvalid("select a] from com.xxx().std:win(3)");
			AssertIsInvalid("select * from com.xxx().std:win(3) where b('aaa)=5");

			AssertIsInvalid("select sum(1+) from b#length(1)");
			AssertIsInvalid("select sum(distinct) from b#length(1)");
			AssertIsInvalid("select sum(distinct distinct a) from b#length(1)");
			AssertIsInvalid("select count(* *) from b#length(1)");
			AssertIsInvalid("select count(*2) from b#length(1)");
			AssertIsInvalid("select stddev(distinct) from b#length(1)");
			AssertIsInvalid("select avedev(distinct) from b#length(1)");

			// group-by
			AssertIsInvalid("select 1 from b#length(1) group by");
			AssertIsInvalid("select 1 from b#length(1) group by group");
			AssertIsInvalid("select 1 from b#length(1) group a");
			AssertIsInvalid("select 1 from b#length(1) group by a group by b");
			AssertIsInvalid("select 1 from b#length(1) by a ");
			AssertIsInvalid("select 1 from b#length(1) group by a a");
			AssertIsInvalid("select 1 from b#length(1) group by a as dummy");

			// having
			AssertIsInvalid("select 1 from b#length(1) group by a having a>5,b<4");

			// insert into
			AssertIsInvalid("insert into select 1 from b#length(1)");
			AssertIsInvalid("insert into 38484 select 1 from b#length(1)");
			AssertIsInvalid("insert into A B select 1 from b#length(1)");
			AssertIsInvalid("insert into A (a,) select 1 from b#length(1)");
			AssertIsInvalid("insert into A (,) select 1 from b#length(1)");
			AssertIsInvalid("insert into A(,a) select 1 from b#length(1)");
			AssertIsInvalid("insert xxx into A(,a) select 1 from b#length(1)");

			// time periods
			AssertIsInvalid("select * from x#time(sec 99)");
			AssertIsInvalid("select * from x#time(99 min min)");
			AssertIsInvalid("select * from x#time(88 sec day)");
			AssertIsInvalid("select * from x#time(1 sec 88 days)");
			AssertIsInvalid("select * from x#time(1 day 2 hours 1 day)");

			// in
			AssertIsInvalid("select * from x where a in()");
			AssertIsInvalid("select * from x where a in(a,)");
			AssertIsInvalid("select * from x where a in(,a)");
			AssertIsInvalid("select * from x where a in(, ,)");
			AssertIsInvalid("select * from x where a in not(1,2)");

			// between
			AssertIsInvalid("select * from x where between a");
			AssertIsInvalid("select * from x where between in and b");

			// like and regexp
			AssertIsInvalid("select * from x where like");
			AssertIsInvalid("select * from x where like escape");
			AssertIsInvalid("select * from x where like a escape");
			AssertIsInvalid("select * from x where order");
			AssertIsInvalid("select * from x where field rlike 'aa' escape '!'");
			AssertIsInvalid("select * from x where field regexp 'aa' escape '!'");
			AssertIsInvalid("select * from x where regexp 'aa'");
			AssertIsInvalid("select * from x where a like b escape c");

			// database join
			AssertIsInvalid("select * from x, sql ");
			AssertIsInvalid("select * from x, sql:xx ");
			AssertIsInvalid("select * from x, sql:xx ");
			AssertIsInvalid("select * from x, sql:xx [' dsfsdf \"]");
			AssertIsInvalid("select * from x, sql:xx [\"sfsf ']");

			// subqueries
			AssertIsInvalid("select (select a) from x");
			AssertIsInvalid("select (select a from X, Y) from x");
			AssertIsInvalid("select (select a from ) from x");
			AssertIsInvalid("select (select from X) from x");
			AssertIsInvalid("select * from x where (select q from pattern [A->B])");
			AssertIsInvalid("select c from A where q*9 in in (select g*5 from C#length(100)) and r=6");
			AssertIsInvalid("select c from A in (select g*5 from C#length(100)) and r=6");
			AssertIsInvalid("select c from A where a in (select g*5 from C#length(100)) 9");

			// Substitution parameters
			AssertIsInvalid("select ? ? from A");
			AssertIsInvalid("select * from A(??)");

			// cast, instanceof, isnumeric and exists dynamic property
			AssertIsInvalid("select * from A(boolean = exists(a, b))");
			AssertIsInvalid("select * from A (boolean = exists())");
			AssertIsInvalid("select * from A (boolean = exists(1))");
			AssertIsInvalid("select * from A where exists(1 + a.b.c?.d.e)");
			AssertIsInvalid("select * from A(boolean = instanceof(, a))");
			AssertIsInvalid("select * from A(boolean = instanceof('agc', ,))");
			AssertIsInvalid("select * from A(boolean = instanceof(b com.espertech.esper.support.AClass))");
			AssertIsInvalid("select * from A(cast((), a + 1))");

			// named window
			AssertIsInvalid("create window AAA as MyType B");
			AssertIsInvalid("create window AAA as select from MyType");
			AssertIsInvalid("create window AAA as , *, b from MyType");
			AssertIsInvalid("create window as select a from MyType");
			AssertIsInvalid("create window AAA as select from MyType");
			AssertIsInvalid("create window AAA#length(10)");
			AssertIsInvalid("create window AAA");
			AssertIsInvalid("create window AAA as select a*5 from MyType");

			// on-delete statement
			AssertIsInvalid("on MyEvent from MyNamedWindow");
			AssertIsInvalid("on  delete from MyNamedWindow");
			AssertIsInvalid("on MyEvent abc def delete from MyNamedWindow");
			AssertIsInvalid("on MyEvent(a<2)(a) delete from MyNamedWindow");
			AssertIsInvalid("on MyEvent delete from MyNamedWindow where");

			// on-select statement
			AssertIsInvalid("on MyEvent select from MyNamedWindow");
			AssertIsInvalid("on MyEvent select * from MyNamedWindow#time(30)");
			AssertIsInvalid("on MyEvent select * from MyNamedWindow where");
			AssertIsInvalid("on MyEvent insert into select * from MyNamedWindow");
			AssertIsInvalid("on MyEvent select a,c,b where a=y select 1,2,2,2 where 2=4");
			AssertIsInvalid("on MyEvent insert into A select a,c,b where a=y select 1,2,2,2 where 2=4");
			AssertIsInvalid("on MyEvent insert into A select a,c,b where a=y insert into D where 2=4");
			AssertIsInvalid("on MyEvent insert into A select a,c,b where a=y insert into D where 2=4 output xyz");
			AssertIsInvalid("on MyEvent insert into A select a,c,b where a=y insert into D where 2=4 output");

			// on-set statement
			AssertIsInvalid("on MyEvent set");
			AssertIsInvalid("on MyEvent set a=dkdkd a");
			AssertIsInvalid("on MyEvent set a=, b=");

			// on-update statement
			AssertIsInvalid("on MyEvent update ABC as abc");
			AssertIsInvalid("on MyEvent update ABC set");
			AssertIsInvalid("on pattern[every B] update ABC as abc set a=");
		}

		[Test]
		public void TestValidCases()
		{
			var className = typeof(SupportBean).Name;
			var preFill = "select * from " + className;

			// output rate limiting
			AssertIsValid("select a from B output snapshot every 1 milliseconds");
			AssertIsValid("select a from B output snapshot every 1 millisecond");
			AssertIsValid("select a from B output snapshot every 1 msec");
			AssertIsValid("select a from B output snapshot every 10 seconds");
			AssertIsValid("select a from B output snapshot every 10 second");
			AssertIsValid("select a from B output snapshot every 10 sec");
			AssertIsValid("select a from B output snapshot every 3 minutes");
			AssertIsValid("select a from B output snapshot every 3 minute");
			AssertIsValid("select a from B output snapshot every 3 min");
			AssertIsValid("select a from B output snapshot every 3 hours");
			AssertIsValid("select a from B output snapshot every 3 hour");
			AssertIsValid("select a from B output snapshot every 3 days");
			AssertIsValid("select a from B output snapshot every 3 day");
			AssertIsValid("select a from B output snapshot every 1 day 2 hours 3 minutes 4 seconds 5 milliseconds");
			AssertIsValid("select a from B output first every 5 events");
			AssertIsValid("select a from B output snapshot at (123, 333, 33, 33, 3)");
			AssertIsValid("select a from B output snapshot at (*, *, *, *, *)");
			AssertIsValid("select a from B output snapshot when myvar*count > 10");
			AssertIsValid("select a from B output snapshot when myvar*count > 10 then set myvar = 1, myvar2 = 2*5");

			AssertIsValid(preFill + "(string='test',intPrimitive=20).win:lenght(100)");
			AssertIsValid(preFill + "(string in ('b', 'a'))");
			AssertIsValid(preFill + "(string in ('b'))");
			AssertIsValid(preFill + "(string in ('b', 'c', 'x'))");
			AssertIsValid(preFill + "(string in [1:2))");
			AssertIsValid(preFill + "(string in [1:2])");
			AssertIsValid(preFill + "(string in (1:2))");
			AssertIsValid(preFill + "(string in (1:2])");
			AssertIsValid(preFill + "(intPrimitive = 08)");
			AssertIsValid(preFill + "(intPrimitive = 09)");
			AssertIsValid(preFill + "(intPrimitive = 008)");
			AssertIsValid(preFill + "(intPrimitive = 0008)");
			AssertIsValid(preFill + "(intPrimitive between 1 and 2)");
			AssertIsValid(preFill + "(intPrimitive not between 1 and 2)");
			AssertIsValid(preFill + "(intPrimitive not in [1:2])");
			AssertIsValid(preFill + "(intPrimitive not in (1, 2, 3))");
			AssertIsValid(preFill + "().win:lenght()");
			AssertIsValid(preFill + "().win:lenght(4,5)");
			AssertIsValid(preFill + "().win:lenght(4)");
			AssertIsValid(preFill + "().win:lenght(\"\",5)");
			AssertIsValid(preFill + "().win:lenght(10.9,1E30,-4.4,\"\",5)");
			AssertIsValid(preFill + "().win:lenght(4).n:c(3.3, -3.3).n:other(\"price\")");
			AssertIsValid(preFill + "().win:lenght().n:c().n:da().n:e().n:f().n:g().n:xh(2.0)");
			AssertIsValid(preFill + "().win:lenght({\"s\"})");
			AssertIsValid(preFill + "().win:lenght({\"a\",\"b\"})");
			AssertIsValid(preFill + "().win:lenght({\"a\",\"b\",\"c\"})");
			AssertIsValid(preFill + "().win:lenght('')");
			AssertIsValid(preFill + "().win:lenght('s')");
			AssertIsValid(preFill + "().win:lenght('s',5)");
			AssertIsValid(preFill + "().win:lenght({'s','t'},5)");
			AssertIsValid(preFill + "().win:some_window('count','l','a').win:lastevent('s','tyr')");
			AssertIsValid(preFill + "().win:some_view({'count'},'l','a')");
			AssertIsValid(preFill + "().win:some_view({})");
			AssertIsValid(preFill + "(string != 'test').win:lenght(100)");
			AssertIsValid(preFill + "(string in (1:2) or katc=3 or lax like '%e%')");
			AssertIsValid(preFill + "(string in (1:2) and dodo=3, lax like '%e%' and oppol / yyy = 5, yunc(3))");
			AssertIsValid(preFill + "()[myprop]");
			AssertIsValid(preFill + "[myprop]#keepall");
			AssertIsValid(preFill + "[myprop as orderId][mythirdprop]#keepall");
			AssertIsValid(preFill + "[select *, abc, a.b from myprop as orderId where a=s][mythirdprop]#keepall");
			AssertIsValid(preFill + "[xyz][select *, abc, a.b from myprop]#keepall");
			AssertIsValid(preFill + "[xyz][myprop where a=x]#keepall");
			AssertIsValid("select * from A where (select * from B[myprop])");

			AssertIsValid("select max(intPrimitive, intBoxed) from " + className + "().std:win(20)");
			AssertIsValid("select max(intPrimitive, intBoxed, longBoxed) from " + className + "().std:win(20)");
			AssertIsValid("select min(intPrimitive, intBoxed) from " + className + "().std:win(20)");
			AssertIsValid("select min(intPrimitive, intBoxed, longBoxed) from " + className + "().std:win(20)");

			AssertIsValid(preFill + "().win:lenght(3) where a = null");
			AssertIsValid(preFill + "().win:lenght(3) where a is null");
			AssertIsValid(preFill + "().win:lenght(3) where 10 is a");
			AssertIsValid(preFill + "().win:lenght(3) where 10 is not a");
			AssertIsValid(preFill + "().win:lenght(3) where 10 <> a");
			AssertIsValid(preFill + "().win:lenght(3) where a <> 10");
			AssertIsValid(preFill + "().win:lenght(3) where a != 10");
			AssertIsValid(preFill + "().win:lenght(3) where 10 != a");
			AssertIsValid(preFill + "().win:lenght(3) where not (a = 5)");
			AssertIsValid(preFill + "().win:lenght(3) where not (a = 5 or b = 3)");
			AssertIsValid(preFill + "().win:lenght(3) where not 5 < 4");
			AssertIsValid(preFill + "().win:lenght(3) where a or (not b)");
			AssertIsValid(preFill + "().win:lenght(3) where a % 3 + 6 * (c%d)");
			AssertIsValid(preFill + "().win:lenght(3) where a || b = 'a'");
			AssertIsValid(preFill + "().win:lenght(3) where a || b || c = 'a'");
			AssertIsValid(preFill + "().win:lenght(3) where a + b + c = 'a'");

			AssertIsValid(
				"select not a, not (b), not (a > 5) from " +
				className +
				"(a=1).win:lenght(10) as win1," +
				className +
				"(a=2).win:lenght(10) as win2 " +
				"where win1.f1 = win2.f2"
			);

			AssertIsValid(
				"select intPrimitive from " +
				className +
				"(a=1).win:lenght(10) as win1," +
				className +
				"(a=2).win:lenght(10) as win2 " +
				"where win1.f1 = win2.f2"
			);

			// outer joins
			TryJoin("left");
			TryJoin("right");
			TryJoin("full");
			AssertIsValid("select * from A left outer join B on a = b and c=d");
			AssertIsValid("select * from A left outer join B on a = b and c=d inner join C on d=c");

			// complex property access
			AssertIsValid(
				"select array[1], map('a'), map(\"b\"), nested.nested " +
				"from a.b(string='test',intPrimitive=20).win:lenght(100) " +
				"where array[1].map('a').nested = 5");
			AssertIsValid(
				"select array[1] as b " +
				"from a.b(string[0]='test').win:lenght(100) as x " +
				"left outer join " +
				"a.b(string[0]='test').win:lenght(100) as y " +
				"on y.array[1].map('a').nested = x.nested2");
			AssertIsValid(
				"select * " +
				"from A " +
				"left outer join " +
				"B" +
				" on a = b and c=d");
			AssertIsValid("select a and b from b#length(1)");
			AssertIsValid("select a or b from b#length(1)");
			AssertIsValid("select a = b from b#length(1)");
			AssertIsValid("select a != b from b#length(1)");
			AssertIsValid("select a.* from b#length(1) as a");
			AssertIsValid("select a.* as myfield from b#length(1) as abc");
			AssertIsValid("select a.*, b.*, c.* from b#length(1) as a");
			AssertIsValid("select a.* as x1, b.* as x2, x.* as x3 from b#length(1) as a, t as x");

			AssertIsValid("select sum(a), avg(b) from b#length(1)");
			AssertIsValid("select sum(all a), avg(all b), avg(all b/c) from b#length(1)");
			AssertIsValid("select sum(distinct a), avg(distinct b) from b#length(1)");
			AssertIsValid("select sum(sum(a)) from b#length(1)");
			AssertIsValid("select sum(3*a), sum(a - b - c) from b#length(1)");
			AssertIsValid("select count(*), count(a), count(all b), count(distinct 2*a), count(5*a/2) from b#length(1)");
			AssertIsValid("select max(volume), min(volume), min(all volume/44), min(distinct 2*a), max(distinct 5*a/2) from b#length(1)");
			AssertIsValid("select median(volume), median(all volume*2/3), median(distinct 2*a) from b#length(1)");
			AssertIsValid("select stddev(volume), stddev(all volume), stddev(distinct 2*a) from b#length(1)");
			AssertIsValid("select avedev(volume), avedev(all volume), avedev(distinct 2*a) from b#length(1)");

			// group-by
			AssertIsValid("select sum(a), x, y from b#length(1) group by a");
			AssertIsValid("select 1 from b#length(1) where a=b and b=d group by a,b,3*x,max(4, 3),'a', \"a\", true, 5*(1+a+y/2)");
			AssertIsValid("select 1 from b#length(1) where a"); // since a could be a boolean
			AssertIsValid("select sum(distinct a), x, y from b#length(1) group by a");

			// having
			AssertIsValid("select sum(a), x, y from b#length(1) group by a having x > y");
			AssertIsValid("select 1 from b#length(1) where a=b and b=d group by a having (max(3*b - 2, 5) > 1) or 'a'=b");
			AssertIsValid("select 1 from b#length(1) group by a having a"); // a could be boolean
			AssertIsValid("select 1 from b#length(1) having a>5");
			AssertIsValid("SELECT 1 FROM b#length(1) WHERE a=b AND b=d GROUP BY a HAVING (max(3*b - 2, 5) > 1) OR 'a'=b");

			// insert into
			AssertIsValid("insert into MyEvent select 1 from b#length(1)");
			AssertIsValid("insert into MyEvent (a) select 1 from b#length(1)");
			AssertIsValid("insert into MyEvent (a, b) select 1 from b#length(1)");
			AssertIsValid("insert into MyEvent (a, b, c) select 1 from b#length(1)");
			AssertIsValid("insert istream into MyEvent select 1 from b#length(1)");
			AssertIsValid("insert rstream into MyEvent select 1 from b#length(1)");

			// pattern inside
			AssertIsValid("select * from pattern [a=" + typeof(SupportBean).Name + "]");
			AssertIsValid("select * from pattern [a=" + typeof(SupportBean).Name + "] as xyz");
			AssertIsValid("select * from pattern [a=" + typeof(SupportBean).Name + "]#length(100) as xyz");
			AssertIsValid("select * from pattern [a=" + typeof(SupportBean).Name + "]#length(100)#someview() as xyz");
			AssertIsValid("select * from xxx");
			AssertIsValid("select rstream * from xxx");
			AssertIsValid("select istream * from xxx");
			AssertIsValid("select rstream 1, 2 from xxx");
			AssertIsValid("select istream 1, 2 from xxx");

			// coalesce
			AssertIsValid("select coalesce(processTimeEvent.price, 0) from x");
			AssertIsValid("select coalesce(processTimeEvent.price, null, -1) from x");
			AssertIsValid("select coalesce(processTimeEvent.price, processTimeEvent.price, processTimeEvent.price, processTimeEvent.price) from x");

			// time intervals
			AssertIsValid("select * from x#time(1 seconds)");
			AssertIsValid("select * from x#time(1.5 second)");
			AssertIsValid("select * from x#time(120230L sec)");
			AssertIsValid("select * from x#time(1.5d milliseconds)");
			AssertIsValid("select * from x#time(1E30 millisecond)");
			AssertIsValid("select * from x#time(1.0 msec)");
			AssertIsValid("select * from x#time(1.5d microseconds)");
			AssertIsValid("select * from x#time(1 usec)");
			AssertIsValid("select * from x#time(101L microsecond)");
			AssertIsValid("select * from x#time(0001 minutes)");
			AssertIsValid("select * from x#time(.1 minute)");
			AssertIsValid("select * from x#time(1.1111001 min)");
			AssertIsValid("select * from x#time(5 hours)");
			AssertIsValid("select * from x#time(5 hour)");
			AssertIsValid("select * from x#time(5 days)");
			AssertIsValid("select * from x#time(5 day)");
			AssertIsValid("select * from x#time(3 years 1 month 2 weeks 5 days 2 hours 88 minutes 1 seconds 9.8 milliseconds)");
			AssertIsValid("select * from x#time(3 years 1 month 2 weeks 5 days 2 hours 88 minutes 1 seconds 9.8 milliseconds 1001 microseconds)");
			AssertIsValid("select * from x#time(5 days 2 hours 88 minutes 1 seconds 9.8 milliseconds)");
			AssertIsValid("select * from x#time(5 day 2 hour 88 minute 1 second 9.8 millisecond)");
			AssertIsValid("select * from x#time(5 days 2 hours 88 minutes 1 seconds)");
			AssertIsValid("select * from x#time(5 days 2 hours 88 minutes)");
			AssertIsValid("select * from x#time(5 days 2 hours)");
			AssertIsValid("select * from x#time(2 hours 88 minutes 1 seconds 9.8 milliseconds)");
			AssertIsValid("select * from x#time(2 hours 88 minutes 1 seconds)");
			AssertIsValid("select * from x#time(2 hours 88 minutes)");
			AssertIsValid("select * from x#time(88 minutes 1 seconds 9.8 milliseconds)");
			AssertIsValid("select * from x#time(88 minutes 1 seconds)");
			AssertIsValid("select * from x#time(1 seconds 9.8 milliseconds)");
			AssertIsValid("select * from x#time(1 seconds 9.8 milliseconds)#goodie(1 sec)");
			AssertIsValid("select * from x#time(1 seconds 9.8 milliseconds)#win:goodie(1 sec)#win:otto(1.1 days 1.1 msec)");

			// in
			AssertIsValid("select * from x where a in('a')");
			AssertIsValid("select * from x where abc in ('a', 'b')");
			AssertIsValid("select * from x where abc in (8*2, 1.001, 'a' || 'b', coalesce(0,null), null)");
			AssertIsValid("select * from x where abc in (sum(x), max(2,2), true)");
			AssertIsValid("select * from x where abc in (y,z, y+z)");
			AssertIsValid("select * from x where abc not in (1)");
			AssertIsValid("select * from x where abc not in (1, 2, 3)");
			AssertIsValid("select * from x where abc*2/dog not in (1, 2, 3)");

			// between
			AssertIsValid("select * from x where abc between 1 and 10");
			AssertIsValid("select * from x where abc between 'a' and 'x'");
			AssertIsValid("select * from x where abc between 1.1 and 1E1000");
			AssertIsValid("select * from x where abc between a and b");
			AssertIsValid("select * from x where abc between a*2 and sum(b)");
			AssertIsValid("select * from x where abc*3 between a*2 and sum(b)");

			// custom aggregation func
			AssertIsValid("select myfunc(price) from x");

			// like and regexp
			AssertIsValid("select * from x where abc like 'dog'");
			AssertIsValid("select * from x where abc like '_dog'");
			AssertIsValid("select * from x where abc like '%dog'");
			AssertIsValid("select * from x where abc like null");
			AssertIsValid("select * from x where abc like '%dog' escape '\\\\'");
			AssertIsValid("select * from x where abc like '%dog%' escape '!'");
			AssertIsValid("select * from x where abc like '%dog' escape \"a\"");
			AssertIsValid("select * from x where abc||'hairdo' like 'dog'");
			AssertIsValid("select * from x where abc not like 'dog'");
			AssertIsValid("select * from x where abc not regexp '[a-z]'");
			AssertIsValid("select * from x where abc regexp '[a-z]'");
			AssertIsValid("select * from x where a like b escape 'aa'");

			// database joins
			AssertIsValid("select * from x, sql:mydb [\"whetever SQL $x.id google\"]");
			AssertIsValid("select * from x, sql:mydb ['whetever SQL $x.id google']");
			AssertIsValid("select * from x, sql:mydb ['']");
			AssertIsValid("select * from x, sql:mydb ['   ']");
			AssertIsValid("select * from x, sql:mydb ['whetever SQL $x.id google' metadatasql 'select 1 as myint']");

			// Previous and prior function
			AssertIsValid("select prev(10, price) from x");
			AssertIsValid("select prev(0, price) from x");
			AssertIsValid("select prev(1000, price) from x");
			AssertIsValid("select prev(index, price) from x");
			AssertIsValid("select prior(10, price) from x");
			AssertIsValid("select prior(0, price) from x");
			AssertIsValid("select prior(1000, price) from x");
			AssertIsValid("select prior(2, symbol) from x");

			// array constants and expressions
			AssertIsValid("select {'a', 'b'} from x");
			AssertIsValid("select {'a'} from x");
			AssertIsValid("select {} from x");
			AssertIsValid("select {'a', 'b'} as yyy from x");
			AssertIsValid("select * from x where MyFunc.func({1,2}, xx)");
			AssertIsValid("select {1,2,3} from x");
			AssertIsValid("select {1.1,'2',3E5, 7L} from x");
			AssertIsValid("select * from x where oo = {1,2,3}");
			AssertIsValid("select {a, b}, {c, d} from x");

			// subqueries
			AssertIsValid("select (select a from B) from x");
			AssertIsValid("select (select a, b,c from B) from x");
			AssertIsValid("select (select a||b||c from B) from x");
			AssertIsValid("select (select 3*222 from B) from x");
			AssertIsValid("select (select 3*222 from B#length(100)) from x");
			AssertIsValid("select (select x from B#length(100) where a=b) from x");
			AssertIsValid("select (select x from B#length(100) where a=b), (select y from C.w:g().e:o(11)) from x");
			AssertIsValid("select 3 + (select a from B) from x");
			AssertIsValid("select (select x from B) / 100, 9 * (select y from C.w:g().e:o(11))/2 from x");
			AssertIsValid("select * from x where id = (select a from B)");
			AssertIsValid("select * from x where id = -1 * (select a from B)");
			AssertIsValid("select * from x where id = (5-(select a from B))");
			AssertIsValid("select * from X where (select a from B where X.f = B.a) or (select a from B where X.f = B.c)");
			AssertIsValid("select * from X where exists (select * from B where X.f = B.a)");
			AssertIsValid("select * from X where exists (select * from B)");
			AssertIsValid("select * from X where not exists (select * from B where X.f = B.a)");
			AssertIsValid("select * from X where not exists (select * from B)");
			AssertIsValid("select exists (select * from B where X.f = B.a) from A");
			AssertIsValid("select B or exists (select * from B) from A");
			AssertIsValid("select c in (select * from C) from A");
			AssertIsValid("select c from A where b in (select * from C)");
			AssertIsValid("select c from A where b not in (select b from C)");
			AssertIsValid("select c from A where q*9 not in (select g*5 from C#length(100)) and r=6");

			// dynamic properties
			AssertIsValid("select b.c.d? from E");
			AssertIsValid("select b.c.d?.e? from E");
			AssertIsValid("select b? from E");
			AssertIsValid("select b? as myevent from E");
			AssertIsValid("select * from pattern [every OrderEvent(item.name?)]");
			AssertIsValid("select * from pattern [every OrderEvent(item?.parent.name?='foo')]");
			AssertIsValid("select b.c[0].d? from E");
			AssertIsValid("select b.c[0]?.mapped('a')? from E");
			AssertIsValid("select b?.c[0].mapped('a') from E");

			// Allow comments in EPL and patterns
			AssertIsValid("select b.c.d /* some comment */ from E");
			AssertIsValid("select b /* ajajaj */ .c.d /* some comment */ from E");
			AssertIsValid("select * from pattern [ /* filter */ every A() -> B() /* for B */]");
			AssertIsValid("select * from pattern [ \n// comment\nevery A() -> B() // same line\n]");

			// Substitution parameters
			AssertIsValid(preFill + "(string=?)");
			AssertIsValid(preFill + "(string in (?, ?))");
			AssertIsValid(preFill + " where string=? and ?=val");
			AssertIsValid(preFill + " having avg(volume) > ?");
			AssertIsValid(preFill + " having avg(?) > ?");
			AssertIsValid("select sum(?) from b#length(1)");
			AssertIsValid("select ?||'a' from B(a=?) where c=? group by ? having d>? output every 10 events order by a, ?");
			AssertIsValid("select a from B output snapshot every 10 events order by a, ?");

			// cast, instanceof, isnumeric and exists dynamic property
			AssertIsValid(preFill + "(boolean = exists(a))");
			AssertIsValid(preFill + "(boolean = exists(a?))");
			AssertIsValid(preFill + "(boolean = exists(a?))");
			AssertIsValid(preFill + " where exists(a.b.c?.d.e)");
			AssertIsValid(preFill + "(boolean = instanceof(a + 2, a))");
			AssertIsValid(preFill + "(boolean = instanceof(b, a))");
			AssertIsValid(preFill + "(boolean = instanceof('agc', string, String, System.String))");
			AssertIsValid(preFill + "(boolean = instanceof(b, com.espertech.esper.support.AClass))");
			AssertIsValid(preFill + "(boolean = instanceof(b, com.espertech.esper.support.AClass, int, long, System.Int64))");
			AssertIsValid(preFill + "(cast(b as boolean))");
			AssertIsValid(preFill + "(cast(b? as Boolean))");
			AssertIsValid(preFill + "(cast(b, boolean))");
			AssertIsValid(preFill + "(cast(b?, Boolean))");
			AssertIsValid(preFill + "(cast(b?, System.String))");
			AssertIsValid(preFill + "(cast(b?, long))");
			AssertIsValid(preFill + "(cast(a + 5, long))");
			AssertIsValid(preFill + "(isnumeric(b?))");
			AssertIsValid(preFill + "(isnumeric(b + 2))");
			AssertIsValid(preFill + "(isnumeric(\"aa\"))");

			// timestamp
			AssertIsValid("select timestamp() from B#length(1)");

			// named window
			AssertIsValid("create window AAA as MyType");
			AssertIsValid("create window AAA as com.myclass.MyType");
			AssertIsValid("create window AAA as select * from MyType");
			AssertIsValid("create window AAA as select a, *, b from MyType");
			AssertIsValid("create window AAA as select a from MyType");
			AssertIsValid("create window AAA#length(10) select a from MyType");
			AssertIsValid("create window AAA select a from MyType");
			AssertIsValid("create window AAA#length(10) as select a from MyType");
			AssertIsValid("create window AAA#length(10) as select a,b from MyType");
			AssertIsValid("create window AAA#length(10)#time(1 sec) as select a,b from MyType");
			AssertIsValid("create window AAA as select 0 as val, 2 as noway, '' as stringval, true as boolval from MyType");
			AssertIsValid("create window AAA as (a b, c d, e f)");
			AssertIsValid("create window AAA (a b, c d, e f)");
			AssertIsValid("create window AAA as select * from MyOtherNamedWindow insert");
			AssertIsValid("create window AAA as MyOtherNamedWindow insert where b=4");

			// on-delete statement
			AssertIsValid("on MyEvent delete from MyNamedWindow");
			AssertIsValid("on MyEvent delete from MyNamedWindow where key = myotherkey");
			AssertIsValid("on MyEvent(myval != 0) as myevent delete from MyNamedWindow as mywin where mywin.key = myevent.otherKey");
			AssertIsValid("on com.my.MyEvent(a=1, b=2 or c.d>3) as myevent delete from MyNamedWindow as mywin where a=b and c<d");
			AssertIsValid("on MyEvent yyy delete from MyNamedWindow xxx where mywin.key = myevent.otherKey");
			AssertIsValid("on pattern [every MyEvent or every MyEvent] delete from MyNamedWindow");

			// on-select statement
			AssertIsValid("on MyEvent select * from MyNamedWindow");
			AssertIsValid("on MyEvent select a, b, c from MyNamedWindow");
			AssertIsValid("on MyEvent select a, b, c from MyNamedWindow where a<b");
			AssertIsValid("on MyEvent as event select a, b, c from MyNamedWindow as win where a.b = b.a");
			AssertIsValid("on MyEvent(hello) select *, c from MyNamedWindow");
			AssertIsValid("on pattern [every X] select a, b, c from MyNamedWindow");
			AssertIsValid("on MyEvent insert into YooStream select a, b, c from MyNamedWindow");
			AssertIsValid("on MyEvent insert into YooStream (p, q) select a, b, c from MyNamedWindow");
			AssertIsValid("on MyEvent select a, b, c from MyNamedWindow where a=b group by c having d>e order by f");
			AssertIsValid("on MyEvent insert into A select * where 1=2 insert into B select * where 2=4");
			AssertIsValid("on MyEvent insert into A select * where 1=2 insert into B select * where 2=4 insert into C select *");
			AssertIsValid("on MyEvent insert into A select a,c,b insert into G select 1,2,2,2 where 2=4 insert into C select * where a=x");
			AssertIsValid(
				"on MyEvent insert into A select a,c,b where a=y group by p having q>r order by x,y insert into G select 1,2,2,2 where 2=4 insert into C select * where a=x");
			AssertIsValid("on MyEvent insert into A select a,c,b where a=y insert into D select * where 2=4 output first");
			AssertIsValid("on MyEvent insert into A select a,c,b where a=y insert into D select * where 2=4 output all");

			// on-set statement
			AssertIsValid("on MyEvent set var=1");
			AssertIsValid("on MyEvent set var = true");
			AssertIsValid("on MyEvent as event set var = event.val");
			AssertIsValid("on MyEvent as event set var = event.val");
			AssertIsValid("on MyEvent as event set var = event.val * 2, var2='abc', var3='def'");

			// on-update statement
			AssertIsValid("on MyEvent update ABC as abc set a=1, b=c");
			AssertIsValid("on MyEvent update ABC set a=1");
			AssertIsValid("on pattern[every B] update ABC as abc set a=1, b=abc.c");

			// create variable
			AssertIsValid("create variable integer a = 77");
			AssertIsValid("create variable sometype b = 77");
			AssertIsValid("create variable sometype b");

			// use variable in output clause
			AssertIsValid("select count(*) from A output every VAR1 events");

			// join with method result
			AssertIsValid("select * from A, method:myClass.myname() as b where a.x = b.x");
			AssertIsValid("select method, a, b from A, METHOD:com.maypack.myClass.myname() as b where a.x = b.x");
			AssertIsValid("select method, a, b from A, someident:com.maypack.myClass.myname() as b where a.x = b.x");

			// unidirectional join
			AssertIsValid("select * from A as x unidirectional, method:myClass.myname() as b where a.x = b.x");
			AssertIsValid("select a, b from A as y unidirectional, B as b where a.x = b.x");
			AssertIsValid("select a, b from A as y unidirectional, B unidirectional where a.x = b.x");

			// expessions and event properties are view/guard/observer parameters
			AssertIsValid("select * from A.win:x(myprop.nested, a.c('s'), 'ss', abc, null)");
			AssertIsValid("select * from pattern[every X where a:b(myprop.nested, a.c('s'), 'ss', *, null)]");
			AssertIsValid("select * from pattern[every X:b(myprop.nested, a.c('s'), 'ss', *, null)]");

			// properties escaped
			AssertIsValid("select a\\.b, a\\.b\\.c.d.e\\.f, zz\\.\\.\\.aa\\.\\.\\.b\\.\\. from A");
			AssertIsValid("select count from A");

			// limit
			AssertIsValid("select count from A limit 1");
			AssertIsValid("select count from A limit 1,2");
			AssertIsValid("select count from A limit 1 offset 2");
			AssertIsValid("select count from A where a=b group by x having c=d output every 5 events order by r limit 1 offset 2");
			AssertIsValid("select count from A limit myvar");
			AssertIsValid("select count from A limit myvar,myvar2");
			AssertIsValid("select count from A limit myvar offset myvar2");
			AssertIsValid("select count from A limit -1");

			// any, some, all
			AssertIsValid("select * from A where 1 = ANY (1, exp, 3)");
			AssertIsValid("select * from A where 1 = SOME ({1,2,3}, myvar, 2*2)");
			AssertIsValid("select * from A where exp = ALL ()");
			AssertIsValid("select * from A where 1 != ALL (select a from B)");
			AssertIsValid("select * from A where 1 = SOME (select a from B)");
			AssertIsValid("select * from A where exp > ANY (select a from B)");
			AssertIsValid("select * from A where 1 <= ANY (select a from B)");
			AssertIsValid("select * from A where {1,2,3} > ALL (1,2,3)");

			// annotations
			AssertIsValid("@SOMEANNOTATION select * from B");
			AssertIsValid("@SomeOther(a=1, b=true, c='a', d=\"alal\") select * from B");
			AssertIsValid("@SomeOther(@inner2(a=3)) select * from B");
			AssertIsValid("@SomeOther(@inner1) select * from B");
			AssertIsValid("@SomeOther(a=com.myenum.VAL1,b=a.VAL2) select * from B");
			AssertIsValid("@SomeOther(tags=@inner1(a=4), moretags=@inner2(a=3)) select * from B");
			AssertIsValid("@SomeOther(innerdata={1, 2, 3}) select * from B");
			AssertIsValid("@SomeOther(innerdata={1, 2, 3}) select * from B");
			var text = "@EPL(\n" +
			           "  name=\"MyStmtName\", \n" +
			           "  description=\"Selects all fields\", \n" +
			           "  onUpdate=\"some test\", \n" +
			           "  onUpdateRemove=\"some text\", \n" +
			           "  tags=@Tags" +
			           ")\n" +
			           "select * from MyField";
			AssertIsValid(text);
			text = "@EPL(name=\"MyStmtName\"," +
			       "  tags=@Tags(" +
			       "    {@Tag(name=\"vehicleId\", type='int', value=100), " +
			       "     @Tag(name=\"vehicleId\", type='int', value=100)" +
			       "    } " +
			       "  )" +
			       ")\n" +
			       "select * from MyField";
			AssertIsValid(text);
			AssertIsValid(
				"@Name('MyStatementName')\n" +
				"@Description('This statement does ABC')\n" +
				"@Tag(name='abc', value='cde')\n" +
				"select a from B");

			// row pattern recog
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern (A B)\n" +
				"define B as (B.price > A.price)" +
				")");
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern (A* B+ C D?)\n" +
				"define B as (B.price > A.price)" +
				")");
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern (A | B)\n" +
				"define B as (B.price > A.price)" +
				")");
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern ( (A B) | (C D))\n" +
				"define B as (B.price > A.price)" +
				")");
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern ( (A | B) (C | D) )\n" +
				"define B as (B.price > A.price)" +
				")");
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern ( (A) | (B | (D | E+)) )\n" +
				"define B as (B.price > A.price)" +
				")");
			AssertIsValid(
				"select * from A match_recognize (measures A.symbol as A\n" +
				"pattern ( A (C | D)? E )\n" +
				"define B as (B.price > A.price)" +
				")");
		}

		[Test]
		public void TestBitWiseCases()
		{
			var className = typeof(SupportBean).Name;
			var eplSmt = "select (intPrimitive & intBoxed) from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select boolPrimitive|boolBoxed from " + className;
			AssertIsValid(eplSmt + "().std:win(20)");
			eplSmt = "select bytePrimitive^byteBoxed from " + className;
			AssertIsValid(eplSmt + "().win:some_view({})");
		}

		[Test]
		public void TestIfThenElseCase()
		{
			var className = typeof(SupportBean).Name;
			var eplSmt = "select case when 1 then (a + 1) when 2 then (a*2) end from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select case a when 1 then (a + 1) end from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select case count(*) when 10 then sum(a) when 20 then max(a*b) end from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select case (a>b) when true then a when false then b end from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select case a when true then a when false then b end from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select case when (a=b) then (a+b) when false then b end as p1 from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
			eplSmt = "select case (a+b) when (a*b) then count(a+b) when false then a ^ b end as p1 from " + className;
			AssertIsValid(eplSmt + ".win:lenght()");
		}

		private void TryJoin(string joinType)
		{
			var className = typeof(SupportBean).Name;
			AssertIsValid(
				"select intPrimitive from " +
				className +
				"(a=1).win:lenght(10) as win1 " +
				joinType +
				" outer join " +
				className +
				"(a=2).win:lenght(10) as win2 " +
				"on win1.f1 = win2.f2"
			);

			AssertIsValid(
				"select intPrimitive from " +
				className +
				"(a=1).win:lenght(10) as win1 " +
				joinType +
				" outer join " +
				className +
				"(a=2).win:lenght(10) as win2 " +
				"on win1.f1 = win2.f2 " +
				joinType +
				" outer join " +
				className +
				"(a=2).win:lenght(10) as win3 " +
				"on win1.f1 = win3.f3"
			);
		}

		private void AssertIsValid(string text)
		{
			Log.Debug(".assertIsValid Trying text=" + text);
			var ast = Parse(text);
			Log.Debug(".assertIsValid success, tree walking...");

			SupportParserHelper.DisplayAST(ast.First);
			Log.Debug(".assertIsValid done");
		}

		private void AssertIsInvalid(string text)
		{
			Log.Debug(".assertIsInvalid Trying text=" + text);

			try {
				Parse(text);
				Assert.IsFalse(true);
			}
			catch (Exception ex) {
				Log.Debug(".assertIsInvalid Expected ParseException exception was thrown and ignored, message=" + ex.Message);
			}
		}

		private Pair<ITree, CommonTokenStream> Parse(string expression)
		{
			return SupportParserHelper.ParseEPL(expression);
		}

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
