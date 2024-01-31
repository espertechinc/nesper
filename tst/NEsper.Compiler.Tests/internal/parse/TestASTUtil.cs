///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.dot.walk;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.compiler.@internal.parse
{
	[TestFixture]
	public class TestASTUtil : AbstractCompilerTest
	{
		[Test]
		public void TestEscapeDot()
		{
			string[][] inout = new string[][] {
				new string[] {"a", "a"},
				new string[] {"", ""},
				new string[] {" ", " "},
				new string[] {".", "\\."},
				new string[] {". .", "\\. \\."},
				new string[] {"a.", "a\\."},
				new string[] {".a", "\\.a"},
				new string[] {"a.b", "a\\.b"},
				new string[] {"a..b", "a\\.\\.b"},
				new string[] {"a\\.b", "a\\.b"},
				new string[] {"a\\..b", "a\\.\\.b"},
				new string[] {"a.\\..b", "a\\.\\.\\.b"},
				new string[] {"a.b.c", "a\\.b\\.c"}
			};

			for (int i = 0; i < inout.Length; i++) {
				string input = inout[i][0];
				string expected = inout[i][1];
				ClassicAssert.AreEqual(expected, DotEscaper.EscapeDot(input), "for input " + input);
			}
		}

		[Test]
		public void TestUnescapeDot()
		{
			string[][] inout = new string[][] {
				new string[] {"a", "a"},
				new string[] {"", ""},
				new string[] {" ", " "},
				new string[] {".", "."},
				new string[] {" . .", " . ."},
				new string[] {"a\\.", "a."},
				new string[] {"\\.a", ".a"},
				new string[] {"a\\.b", "a.b"},
				new string[] {"a.b", "a.b"},
				new string[] {".a", ".a"},
				new string[] {"a.", "a."},
				new string[] {"a\\.\\.b", "a..b"},
				new string[] {"a\\..\\.b", "a...b"},
				new string[] {"a.\\..b", "a...b"},
				new string[] {"a\\..b", "a..b"},
				new string[] {"a.b\\.c", "a.b.c"},
			};

			for (int i = 0; i < inout.Length; i++) {
				string input = inout[i][0];
				string expected = inout[i][1];
				ClassicAssert.AreEqual(expected, DotEscaper.UnescapeDot(input), "for input " + input);
			}
		}
	}
} // end of namespace
