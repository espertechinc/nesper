///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestSortedRefCountedSet : AbstractCommonTest
    {
        private SortedRefCountedSet<string> refSet;
        private readonly Random random = new Random();

        [SetUp]
        public void SetUp()
        {
            refSet = new SortedRefCountedSet<string>();
        }

        [Test]
        public void TestMaxMinValue()
        {
            refSet.Add("a");
            refSet.Add("b");
            Assert.AreEqual("ba", refSet.MaxValue + refSet.MinValue);
            refSet.Remove("a");
            Assert.AreEqual("bb", refSet.MaxValue + refSet.MinValue);
            refSet.Remove("b");
            Assert.IsNull(refSet.MaxValue);
            Assert.IsNull(refSet.MinValue);

            refSet.Add("b");
            refSet.Add("a");
            refSet.Add("d");
            refSet.Add("a");
            refSet.Add("c");
            refSet.Add("a");
            refSet.Add("c");
            Assert.AreEqual("da", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("d");
            Assert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            Assert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            Assert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("c");
            Assert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("c");
            Assert.AreEqual("ba", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            Assert.AreEqual("bb", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("b");
            Assert.IsNull(refSet.MaxValue);
            Assert.IsNull(refSet.MinValue);
        }

        [Test]
        public void TestAdd()
        {
            refSet.Add("a");
            refSet.Add("b");
            refSet.Add("a");
            refSet.Add("c");
            refSet.Add("a");

            Assert.AreEqual("c", refSet.MaxValue);
            Assert.AreEqual("a", refSet.MinValue);
        }

        [Test]
        public void TestRemove()
        {
            refSet.Add("a");
            refSet.Remove("a");
            Assert.IsNull(refSet.MaxValue);
            Assert.IsNull(refSet.MinValue);

            refSet.Add("a");
            refSet.Add("a");
            Assert.AreEqual("aa", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            Assert.AreEqual("aa", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            Assert.IsNull(refSet.MaxValue);
            Assert.IsNull(refSet.MinValue);

            // nothing to remove
            refSet.Remove("c");

            refSet.Add("a");
            refSet.Remove("a");
            refSet.Remove("a");
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
