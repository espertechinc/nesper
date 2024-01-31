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
using NUnit.Framework.Legacy;

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
            ClassicAssert.AreEqual("ba", refSet.MaxValue + refSet.MinValue);
            refSet.Remove("a");
            ClassicAssert.AreEqual("bb", refSet.MaxValue + refSet.MinValue);
            refSet.Remove("b");
            ClassicAssert.IsNull(refSet.MaxValue);
            ClassicAssert.IsNull(refSet.MinValue);

            refSet.Add("b");
            refSet.Add("a");
            refSet.Add("d");
            refSet.Add("a");
            refSet.Add("c");
            refSet.Add("a");
            refSet.Add("c");
            ClassicAssert.AreEqual("da", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("d");
            ClassicAssert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            ClassicAssert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            ClassicAssert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("c");
            ClassicAssert.AreEqual("ca", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("c");
            ClassicAssert.AreEqual("ba", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            ClassicAssert.AreEqual("bb", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("b");
            ClassicAssert.IsNull(refSet.MaxValue);
            ClassicAssert.IsNull(refSet.MinValue);
        }

        [Test]
        public void TestAdd()
        {
            refSet.Add("a");
            refSet.Add("b");
            refSet.Add("a");
            refSet.Add("c");
            refSet.Add("a");

            ClassicAssert.AreEqual("c", refSet.MaxValue);
            ClassicAssert.AreEqual("a", refSet.MinValue);
        }

        [Test]
        public void TestRemove()
        {
            refSet.Add("a");
            refSet.Remove("a");
            ClassicAssert.IsNull(refSet.MaxValue);
            ClassicAssert.IsNull(refSet.MinValue);

            refSet.Add("a");
            refSet.Add("a");
            ClassicAssert.AreEqual("aa", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            ClassicAssert.AreEqual("aa", refSet.MaxValue + refSet.MinValue);

            refSet.Remove("a");
            ClassicAssert.IsNull(refSet.MaxValue);
            ClassicAssert.IsNull(refSet.MinValue);

            // nothing to remove
            refSet.Remove("c");

            refSet.Add("a");
            refSet.Remove("a");
            refSet.Remove("a");
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
