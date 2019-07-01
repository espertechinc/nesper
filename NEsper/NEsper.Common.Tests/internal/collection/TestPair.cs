///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestPair : AbstractTestBase
    {
        private Pair<string, string> pair1 = new Pair<string, string>("a", "b");
        private Pair<string, string> pair2 = new Pair<string, string>("a", "b");
        private Pair<string, string> pair3 = new Pair<string, string>("a", null);
        private Pair<string, string> pair4 = new Pair<string, string>(null, "b");
        private Pair<string, string> pair5 = new Pair<string, string>(null, null);

        [Test]
        public void TestHashCode()
        {
            Assert.IsTrue(pair1.GetHashCode() == ("a".GetHashCode() ^ "b".GetHashCode()));
            Assert.IsTrue(pair3.GetHashCode() == "a".GetHashCode());
            Assert.IsTrue(pair4.GetHashCode() == "b".GetHashCode());
            Assert.IsTrue(pair5.GetHashCode() == 0);

            Assert.IsTrue(pair1.GetHashCode() == pair2.GetHashCode());
            Assert.IsTrue(pair1.GetHashCode() != pair3.GetHashCode());
            Assert.IsTrue(pair1.GetHashCode() != pair4.GetHashCode());
            Assert.IsTrue(pair1.GetHashCode() != pair5.GetHashCode());
        }

        [Test]
        public void TestEquals()
        {
            Assert.AreEqual(pair2, pair1);
            Assert.AreEqual(pair1, pair2);

            Assert.That(pair1, Is.Not.SameAs(pair3));
            Assert.That(pair3, Is.Not.SameAs(pair1));
            Assert.That(pair1, Is.Not.SameAs(pair4));
            Assert.That(pair2, Is.Not.SameAs(pair5));
            Assert.That(pair3, Is.Not.SameAs(pair4));
            Assert.That(pair4, Is.Not.SameAs(pair5));

            Assert.That(pair1, Is.SameAs(pair1));
            Assert.That(pair2, Is.SameAs(pair2));
            Assert.That(pair3, Is.SameAs(pair3));
            Assert.That(pair4, Is.SameAs(pair4));
            Assert.That(pair5, Is.SameAs(pair5));
        }
    }
} // end of namespace
