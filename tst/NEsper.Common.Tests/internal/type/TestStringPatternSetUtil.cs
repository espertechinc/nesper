///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.type
{
    [TestFixture]
    public class TestStringPatternSetUtil : AbstractCommonTest
    {
        private IList<Pair<StringPatternSet, bool>> patterns;

        [SetUp]
        public void SetUp()
        {
            patterns = new List<Pair<StringPatternSet, bool>>();
        }

        private void RunAssertion()
        {
            ClassicAssert.IsTrue(StringPatternSetUtil.Evaluate(false, patterns, "123"));
            ClassicAssert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "123abc"));
            ClassicAssert.IsTrue(StringPatternSetUtil.Evaluate(false, patterns, "123abcdef"));
            ClassicAssert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "123abcdefxyz"));
            ClassicAssert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "456"));
            ClassicAssert.IsTrue(StringPatternSetUtil.Evaluate(true, patterns, "456"));
        }

        [Test]
        public void TestCombinationLike()
        {
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike("%123%"), true));
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike("%abc%"), false));
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike("%def%"), true));
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetLike("%xyz%"), false));

            RunAssertion();
        }

        [Test]
        public void TestCombinationRegex()
        {
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex("(.)*123(.)*"), true));
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex("(.)*abc(.)*"), false));
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex("(.)*def(.)*"), true));
            patterns.Add(new Pair<StringPatternSet, bool>(new StringPatternSetRegex("(.)*xyz(.)*"), false));

            RunAssertion();
        }

        [Test]
        public void TestEmpty()
        {
            ClassicAssert.IsTrue(StringPatternSetUtil.Evaluate(true, patterns, "abc"));
            ClassicAssert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "abc"));
        }
    }
} // end of namespace