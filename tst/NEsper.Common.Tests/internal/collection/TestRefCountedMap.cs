///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestRefCountedMap : AbstractCommonTest
    {
        private RefCountedMap<string, int> refMap;

        [SetUp]
        public void SetUp()
        {
            refMap = new RefCountedMap<string, int>();
            refMap.Put("a", 100);
        }

        [Test]
        public void TestPut()
        {
            try
            {
                refMap.Put("a", 10);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected exception
            }

            try
            {
                refMap.Put(null, 10);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // Expected exception
            }
        }

        [Test]
        public void TestGet()
        {
            Assert.That(refMap.Contains("b"), Is.False);
            Assert.That(refMap.TryGetValue("b", out int val), Is.False);

            Assert.That(refMap.TryGetValue("a", out val), Is.True);
            ClassicAssert.AreEqual(100, val);
        }

        [Test]
        public void TestReference()
        {
            refMap.Reference("a");

            try
            {
                refMap.Reference("b");
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected exception
            }
        }

        [Test]
        public void TestDereference()
        {
            bool isLast = refMap.Dereference("a");
            ClassicAssert.IsTrue(isLast);

            refMap.Put("b", 100);
            refMap.Reference("b");
            ClassicAssert.IsFalse(refMap.Dereference("b"));
            ClassicAssert.IsTrue(refMap.Dereference("b"));

            try
            {
                refMap.Dereference("b");
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected exception
            }
        }

        [Test]
        public void TestFlow()
        {
            refMap.Put("b", -1);
            refMap.Reference("b");

            ClassicAssert.AreEqual(-1, refMap["b"]);
            ClassicAssert.IsFalse(refMap.Dereference("b"));
            ClassicAssert.AreEqual(-1, refMap["b"]);
            ClassicAssert.IsTrue(refMap.Dereference("b"));
            Assert.That(refMap.Contains("b"), Is.False);

            refMap.Put("b", 2);
            refMap.Reference("b");

            refMap.Put("c", 3);
            refMap.Reference("c");

            refMap.Dereference("b");
            refMap.Reference("b");

            ClassicAssert.AreEqual(2, refMap["b"]);
            ClassicAssert.IsFalse(refMap.Dereference("b"));
            ClassicAssert.IsTrue(refMap.Dereference("b"));
            Assert.That(refMap.Contains("b"), Is.False);

            ClassicAssert.AreEqual(3, refMap["c"]);
            ClassicAssert.IsFalse(refMap.Dereference("c"));
            ClassicAssert.AreEqual(3, refMap["c"]);
            ClassicAssert.IsTrue(refMap.Dereference("c"));
            Assert.That(refMap.Contains("c"), Is.False);
        }
    }
} // end of namespace
