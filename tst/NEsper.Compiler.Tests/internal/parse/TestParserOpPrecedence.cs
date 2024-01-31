///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Reflection;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.compiler.@internal.parse
{
	/// <summary>
	/// Test operator precedence and on-expression equivalence.
	/// Precendences are similar to Java see <a>http://java.sun.com/docs/books/tutorial/java/nutsandbolts/expressions.html</a>)
	/// <para>
	/// Precedence ordering (highest on top):
	/// postfix operators   -   within
	/// unary operators     -   not, every
	/// AND                 -   and
	/// OR                  -   or
	/// FOLLOWED BY         -   -&gt;
	/// </para>
	/// </summary>
	/// 
	[TestFixture]
	public class TestParserOpPrecedence : AbstractCompilerTest
	{
		[Test]
		public void TestEquivalency()
		{
			AssertEquivalent(
				"every a",
				"(every a)");

			AssertEquivalent(
				"every a() or b()",
				"((every a()) or b())");

			AssertEquivalent(
				"every a -> b or c",
				"(every a) -> (b or c)");

			AssertEquivalent(
				"every a() -> b() and c()",
				"(every a()) -> (b() and c())");

			AssertEquivalent(
				"a() and b() or c()",
				"(a() and b()) or c()");

			AssertEquivalent(
				"a() or b() and c() or d()",
				"a() or (b() and c()) or d()");

			AssertEquivalent(
				"a() or b() and every e() -> f() -> c() or d()",
				"(a() or (b() and (every (e())))) -> f() -> (c() or d())");

			AssertEquivalent(
				"a() -> b() or e() -> f()",
				"a() -> (b() or e()) -> f()");

			string original = "every a() -> every b() and c() or d() and not e() -> f()";
			AssertEquivalent(original, "every a() -> (every b()) and c() or d() and (not (e())) -> f()");
			AssertEquivalent(original, "(every a()) -> ((every b()) and c()) or (d() and (not (e()))) -> f()");

			AssertEquivalent(
				"not a()",
				"(not a())");

			AssertEquivalent(
				"every a() where timer:within(5)",
				"every (a() where timer:within(5))");

			original = "every a() where timer:within(5) and not b() where timer:within(3) -> d() where timer:within(4)";
			AssertEquivalent(
				original,
				"every (a() where timer:within(5)) and not (b() where timer:within(3)) -> (d() where timer:within(4))");
			AssertEquivalent(
				original,
				"(every (a() where timer:within(5))) and (not (b() where timer:within(3))) -> (d() where timer:within(4))");
			AssertEquivalent(
				original,
				"((every (a() where timer:within(5))) and (not (b() where timer:within(3)))) -> (d() where timer:within(4))");

			AssertEquivalent(
				"((a() where timer:within(10)) or (b() where timer:within(5))) where timer:within(20)",
				"(a() where timer:within(10) or b() where timer:within(5)) where timer:within(20)");

			AssertEquivalent("timer:interval(20)", "(timer:interval(20))");
			AssertEquivalent("every timer:interval(20)", "every (timer:interval(20))");
			AssertEquivalent(
				"timer:interval(20) -> timer:interval(20) or timer:interval(22)",
				"((timer:interval(20)) -> (timer:interval(20) or timer:interval(22)))");
			AssertEquivalent("every a() -> every timer:interval(20) -> every c()", "(every a()) -> (every (timer:interval(20))) -> (every c())");

			original = "timer:at(5,0,[1,2],1:10,* /9,[1,2,5:8]) -> b()";
			AssertEquivalent(original, original);
		}

		[Test]
		public void TestNotEquivalent()
		{
			AssertNotEquivalent("a()", "every a()");
			AssertNotEquivalent("a(n=6)", "a(n=7)");
			AssertNotEquivalent("a(x=\"a\")", "a(x=\"b\")");
			AssertNotEquivalent("a()", "b()");

			AssertNotEquivalent("a() where timer:within(20)", "a() where timer:within(30)");
			AssertNotEquivalent("a() or b() where timer:within(20)", "(a() or b()) where timer:within(20)");

			AssertNotEquivalent("every a() or b()", "every (a() or b())");
			AssertNotEquivalent("every a() and b()", "every (a() and b())");

			AssertNotEquivalent("a() -> not b()", "not(a() -> b())");

			AssertNotEquivalent("a() -> b() or c()", "(a() -> b()) or c()");

			AssertNotEquivalent("a() and b() or c()", "a() and (b() or c())");

			AssertNotEquivalent("timer:interval(20)", "timer:interval(30)");

			AssertNotEquivalent("timer:at(20,*,*,*,*)", "timer:at(21,*,*,*,*)");
			AssertNotEquivalent("timer:at([1:10],*,*,*,*)", "timer:at([1:11],*,*,*,*)");
			AssertNotEquivalent("timer:at(*,*,3:2,*,*)", "timer:at(*,*,2:3,*,*)");

			AssertNotEquivalent("EventA(value in [2:5])", "EventA(value in [3:5])");
			AssertNotEquivalent("EventA(value in [2:5])", "EventA(value in [2:6])");
			AssertNotEquivalent("EventA(value in [2:5])", "EventA(value in (2:6])");
			AssertNotEquivalent("EventA(value in [2:5])", "EventA(value in [2:6))");
			AssertNotEquivalent("EventA(value in [2:5])", "EventA(value in (2:6))");
		}

		private void AssertEquivalent(
			string expressionOne,
			string expressionTwo)
		{
			EPLTreeWalkerListener l1 = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + expressionOne + "]");
			EPLTreeWalkerListener l2 = SupportParserHelper.ParseAndWalkEPL(container, "select * from pattern[" + expressionTwo + "]");

			string t1 = ToPatternText(l1);
			string t2 = ToPatternText(l2);
			ClassicAssert.AreEqual(t1, t2);
		}

		private string ToPatternText(EPLTreeWalkerListener walker)
		{
			PatternStreamSpecRaw raw = (PatternStreamSpecRaw) walker.StatementSpec.StreamSpecs[0];
			StringWriter writer = new StringWriter();
			raw.EvalForgeNode.ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
			return writer.ToString();
		}

		private void AssertNotEquivalent(
			string expressionOne,
			string expressionTwo)
		{
			Log.Debug(".assertEquivalent parsing: " + expressionOne);
			Pair<ITree, CommonTokenStream> astOne = Parse(expressionOne);

			Log.Debug(".assertEquivalent parsing: " + expressionTwo);
			Pair<ITree, CommonTokenStream> astTwo = Parse(expressionTwo);

			ClassicAssert.IsFalse(astOne.First.ToStringTree().Equals(astTwo.First.ToStringTree()));
		}

		private Pair<ITree, CommonTokenStream> Parse(string expression)
		{
			return SupportParserHelper.ParseEPL("select * from pattern[" + expression + "]");
		}

		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
