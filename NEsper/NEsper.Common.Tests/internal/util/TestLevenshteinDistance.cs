///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestLevenshteinDistance : AbstractCommonTest
    {
        [Test, RunInApplicationDomain]
        public void TestDistance()
        {
            Assert.AreEqual(1, LevenshteinDistance.ComputeLevenshteinDistance("abc", "abcd"));
            // System.out.println(result);
        }
    }
} // end of namespace
