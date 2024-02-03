///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.IsTrue(refSet.Add("a"));
            ClassicAssert.AreEqual(1, refSet.Count);

            ClassicAssert.IsFalse(refSet.Add("a"));
            ClassicAssert.AreEqual(2, refSet.Count);

            ClassicAssert.IsTrue(refSet.Add("A"));
            ClassicAssert.AreEqual(3, refSet.Count);
        }

        [Test]
        public void TestRemove()
        {
            refSet.Add("a");
            refSet.Add("a");
            refSet.Add("a");
            ClassicAssert.AreEqual(3, refSet.Count);
            ClassicAssert.IsFalse(refSet.Remove("a"));
            ClassicAssert.AreEqual(2, refSet.Count);
            ClassicAssert.IsFalse(refSet.Remove("a"));
            ClassicAssert.AreEqual(1, refSet.Count);
            ClassicAssert.IsTrue(refSet.Remove("a"));
            ClassicAssert.AreEqual(0, refSet.Count);

            refSet.Add("a");
            ClassicAssert.IsTrue(refSet.Remove("a"));

            refSet.Add("b");
            refSet.Add("b");
            ClassicAssert.IsFalse(refSet.Remove("b"));
            ClassicAssert.IsTrue(refSet.Remove("b"));

            refSet.Add("C");
            refSet.Add("C");
            ClassicAssert.IsTrue(refSet.RemoveAll("C"));
            ClassicAssert.IsFalse(refSet.RemoveAll("C"));
        }
    }
} // end of namespace
