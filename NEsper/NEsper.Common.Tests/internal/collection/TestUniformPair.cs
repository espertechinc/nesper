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
    public class TestUniformPair : AbstractCommonTest
    {
        private UniformPair<string> pair1 = new UniformPair<string>("a", "b");
        private UniformPair<string> pair2 = new UniformPair<string>("a", "b");
        private UniformPair<string> pair3 = new UniformPair<string>("a", null);
        private UniformPair<string> pair4 = new UniformPair<string>(null, "b");
        private UniformPair<string> pair5 = new UniformPair<string>(null, null);

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

            Assert.IsTrue(pair1 != pair3);
            Assert.IsTrue(pair3 != pair1);
            Assert.IsTrue(pair1 != pair4);
            Assert.IsTrue(pair2 != pair5);
            Assert.IsTrue(pair3 != pair4);
            Assert.IsTrue(pair4 != pair5);

            Assert.IsTrue(pair1 == pair1);
            Assert.IsTrue(pair2 == pair2);
            Assert.IsTrue(pair3 == pair3);
            Assert.IsTrue(pair4 == pair4);
            Assert.IsTrue(pair5 == pair5);
        }
    }
} // end of namespace
