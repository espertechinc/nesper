///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestLevenshteinDistance 
    {
        [Test]
        public void TestDistance()
        {
            Assert.AreEqual(1, LevenshteinDistance.ComputeLevenshteinDistance("abc", "abcd"));
            // Console.Out.WriteLine(result);
        }
    }
}
