///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.collection;

using NUnit.Framework;

namespace com.espertech.esper.type
{
    [TestFixture]
    public class TestStringPatternSetUtil 
    {
        [Test]
        public void TestEmpty()
        {
            var patterns = new List<Pair<StringPatternSet, Boolean>>();
            Assert.IsTrue(StringPatternSetUtil.Evaluate(true, patterns, "abc"));
            Assert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "abc"));
        }
    
        [Test]
        public void TestCombinationLike()
        {
            var patterns = new List<Pair<StringPatternSet, Boolean>>();
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%123%"), true));
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%abc%"), false));
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%def%"), true));
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetLike("%xyz%"), false));

            RunAssertion(patterns);
        }
    
        [Test]
        public void TestCombinationRegex()
        {
            var patterns = new List<Pair<StringPatternSet, Boolean>>();
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex("(.)*123(.)*"), true));
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex("(.)*abc(.)*"), false));
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex("(.)*def(.)*"), true));
            patterns.Add(new Pair<StringPatternSet, Boolean>(new StringPatternSetRegex("(.)*xyz(.)*"), false));
    
            RunAssertion(patterns);
        }
    
        private void RunAssertion(IEnumerable<Pair<StringPatternSet, bool>> patterns)
        {
            Assert.IsTrue(StringPatternSetUtil.Evaluate(false, patterns, "123"));
            Assert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "123abc"));
            Assert.IsTrue(StringPatternSetUtil.Evaluate(false, patterns, "123abcdef"));
            Assert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "123abcdefxyz"));
            Assert.IsFalse(StringPatternSetUtil.Evaluate(false, patterns, "456"));
            Assert.IsTrue(StringPatternSetUtil.Evaluate(true, patterns, "456"));
        }
    }
}
