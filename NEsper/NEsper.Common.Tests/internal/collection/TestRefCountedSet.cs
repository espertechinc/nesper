///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestRefCountedSet : AbstractCommonTest
    {
        private RefCountedSet<string> refSet;

        [SetUp]
        public void SetUp()
        {
            refSet = new RefCountedSet<string>();
        }

        [Test]
        public void TestAdd()
        {
            Assert.IsTrue(refSet.Add("a"));
            Assert.AreEqual(1, refSet.Count);

            Assert.IsFalse(refSet.Add("a"));
            Assert.AreEqual(2, refSet.Count);

            Assert.IsTrue(refSet.Add("A"));
            Assert.AreEqual(3, refSet.Count);
        }

        [Test]
        public void TestRemove()
        {
            refSet.Add("a");
            refSet.Add("a");
            refSet.Add("a");
            Assert.AreEqual(3, refSet.Count);
            Assert.IsFalse(refSet.Remove("a"));
            Assert.AreEqual(2, refSet.Count);
            Assert.IsFalse(refSet.Remove("a"));
            Assert.AreEqual(1, refSet.Count);
            Assert.IsTrue(refSet.Remove("a"));
            Assert.AreEqual(0, refSet.Count);

            refSet.Add("a");
            Assert.IsTrue(refSet.Remove("a"));

            refSet.Add("b");
            refSet.Add("b");
            Assert.IsFalse(refSet.Remove("b"));
            Assert.IsTrue(refSet.Remove("b"));

            refSet.Add("C");
            refSet.Add("C");
            Assert.IsTrue(refSet.RemoveAll("C"));
            Assert.IsFalse(refSet.RemoveAll("C"));
        }
    }
} // end of namespace
