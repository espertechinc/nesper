///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    [TestFixture]
    public class TestIdentifierUtil : AbstractCommonTest
    {
        private void AssertDiff(
            string expected,
            string input)
        {
            ClassicAssert.AreEqual(expected, IdentifierUtil.GetIdentifierMayStartNumeric(input));
        }

        private void AssertNoop(string input)
        {
            ClassicAssert.AreEqual(input, IdentifierUtil.GetIdentifierMayStartNumeric(input));
        }

        [Test]
        public void TestGetIdent()
        {
            // These tests are not applicable given the adjustments we had to make to identifier
            // construction.  Leaving these for future placeholders where we will test the new
            // mechanism.

#if false
            AssertNoop("a");
            AssertNoop("ab");
            AssertNoop("a_b");
            AssertNoop("a__b");
            AssertNoop("converts_0_or_not");
            AssertNoop("0123456789");
            AssertNoop("$");
            AssertNoop("package");
            AssertNoop("class");

            AssertDiff("46", ".");
            AssertDiff("32", " ");
            AssertDiff("45", "-");
            AssertDiff("43", "+");
            AssertDiff("40", "(");
            AssertDiff("59", ";");
            AssertDiff("9", "\t");
            AssertDiff("10", "\n");

            AssertDiff("x32y32z", "x y z");
            AssertDiff("a46b", "a.b");
#endif
        }
    }
} // end of namespace