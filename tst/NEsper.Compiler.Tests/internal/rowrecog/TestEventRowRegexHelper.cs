///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compiler.@internal.parse;
using com.espertech.esper.compiler.@internal.util;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.compiler.@internal.rowrecog
{
	[TestFixture]
	public class TestEventRowRegexHelper : AbstractCompilerTest
	{
		[Test]
		public void TestVariableAnalysis()
		{
			var patternTests = new[] {
				new[] {"A", "[\"A\"]", "[]"},
				new[] {"A B", "[\"A\", \"B\"]", "[]"},
				new[] {"A B*", "[\"A\"]", "[\"B\"]"},
				new[] {"A B B", "[\"A\"]", "[\"B\"]"},
				new[] {"A B A", "[\"B\"]", "[\"A\"]"},
				new[] {"A B+ C", "[\"A\", \"C\"]", "[\"B\"]"},
				new[] {"A B?", "[\"A\", \"B\"]", "[]"},
				new[] {"(A B)* C", "[\"C\"]", "[\"A\", \"B\"]"},
				new[] {"D (A B)+ (G H)? C", "[\"D\", \"G\", \"H\", \"C\"]", "[\"A\", \"B\"]"},
				new[] {"A B | A C", "[\"A\", \"B\", \"C\"]", "[]"},
				new[] {"(A B*) | (A+ C)", "[\"C\"]", "[\"B\", \"A\"]"},
				new[] {"(A | B) | (C | A)", "[\"A\", \"B\", \"C\"]", "[]"},
			};

			for (var i = 0; i < patternTests.Length; i++) {
				var pattern = patternTests[i][0];
				var expression = "select * from MyEvent#keepall match_recognize (" +
				                 "  partition by string measures A.string as a_string pattern ( " +
				                 pattern +
				                 ") define A as (A.value = 1) )";

				var raw = CompilerHelperSingleEPL.ParseWalk(expression, SupportStatementSpecMapEnv.Make(container));

				var parent = raw.MatchRecognizeSpec.Pattern;
				var singles = new LinkedHashSet<string>();
				var multiples = new LinkedHashSet<string>();

				RowRecogHelper.RecursiveInspectVariables(parent, false, singles, multiples);

				var outText = "Failed in :" +
				              pattern +
				              " result is : single " +
				              singles.RenderAny() +
				              " multiple " +
				              multiples.RenderAny();

				ClassicAssert.AreEqual(patternTests[i][1], singles.RenderAny(), outText);
				ClassicAssert.AreEqual(patternTests[i][2], multiples.RenderAny(), outText);
			}
		}

		[Test]
		public void TestVisibilityAnalysis()
		{
			var patternTests = new[] {
				new[] {"A", "{}"},
				new[] {"A B", "{\"B\"=[\"A\"]}"},
				new[] {"A B*", "{\"B\"=[\"A\"]}"},
				new[] {"A B B", "{\"B\"=[\"A\"]}"},
				new[] {"A B A", "{\"A\"=[\"B\"], \"B\"=[\"A\"]}"},
				new[] {"A B+ C", "{\"B\"=[\"A\"], \"C\"=[\"A\", \"B\"]}"},
				new[] {"(A B)+ C", "{\"B\"=[\"A\"], \"C\"=[\"A\", \"B\"]}"},
				new[] {"D (A B)+ (G H)? C", "{\"A\"=[\"D\"], \"B\"=[\"A\", \"D\"], \"C\"=[\"A\", \"B\", \"D\", \"G\", \"H\"], \"G\"=[\"A\", \"B\", \"D\"], \"H\"=[\"A\", \"B\", \"D\", \"G\"]}"},
				new[] {"A B | A C", "{\"B\"=[\"A\"], \"C\"=[\"A\"]}"},
				new[] {"(A B*) | (A+ C)", "{\"B\"=[\"A\"], \"C\"=[\"A\"]}"},
				new[] {"A (B | C) D", "{\"B\"=[\"A\"], \"C\"=[\"A\"], \"D\"=[\"A\", \"B\", \"C\"]}"},
				new[] {"(((A))) (((B))) (( C | (D E)))", "{\"B\"=[\"A\"], \"C\"=[\"A\", \"B\"], \"D\"=[\"A\", \"B\"], \"E\"=[\"A\", \"B\", \"D\"]}"},
				new[] {"(A | B) C", "{\"C\"=[\"A\", \"B\"]}"},
				new[] {"(A | B) (C | A)", "{\"A\"=[\"B\"], \"C\"=[\"A\", \"B\"]}"},
			};

			for (var i = 0; i < patternTests.Length; i++) {
				var pattern = patternTests[i][0];
				var expected = patternTests[i][1];
				var expression = "select * from MyEvent#keepall match_recognize (" +
				                 "  partition by string measures A.string as a_string pattern ( " +
				                 pattern +
				                 ") define A as (A.value = 1) )";

				var raw = CompilerHelperSingleEPL.ParseWalk(expression, SupportStatementSpecMapEnv.Make(container));

				var parent = raw.MatchRecognizeSpec.Pattern;

				var visibility = RowRecogHelper.DetermineVisibility(parent);

				// sort, for comparing
				var visibilitySorted = new SortedDictionary<string, IList<string>>();
				foreach (var tag in visibility.Keys.OrderBy(k => k)) {
					var sorted = new List<string>(visibility.Get(tag).OrderBy(v => v));
					visibilitySorted.Put(tag, sorted);
				}

				var visibilityAsString = visibilitySorted.RenderAny();
				ClassicAssert.AreEqual(expected, visibilityAsString, "Failed in :" + pattern);
			}
		}
	}
} // end of namespace
