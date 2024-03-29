///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestLevenshteinDistance : AbstractCommonTest
    {
        [Test]
        public void TestDistance()
        {
            ClassicAssert.AreEqual(1, LevenshteinDistance.ComputeLevenshteinDistance("abc", "abcd"));
            // Console.WriteLine(result);
        }
    }
} // end of namespace
