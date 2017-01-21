///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class TestTypeResolution
    {
        [Test]
        public void TestResolveItems()
        {
            Assert.AreEqual(
                typeof(A),
                TypeHelper.ResolveType("com.espertech.esper.compat.A"));
            Assert.AreEqual(
                typeof(TestData),
                TypeHelper.ResolveType("com.espertech.esper.compat.TestData"));
        }
    }

    public class TestData
    {
        public string AnyItem { get; set; }
    }

    public class A
    {
        public string AnyItem { get; set; }
    }
}
