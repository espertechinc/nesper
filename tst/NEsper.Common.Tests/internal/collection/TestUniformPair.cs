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
            ClassicAssert.IsTrue(pair1.GetHashCode() == ("a".GetHashCode() ^ "b".GetHashCode()));
            ClassicAssert.IsTrue(pair3.GetHashCode() == "a".GetHashCode());
            ClassicAssert.IsTrue(pair4.GetHashCode() == "b".GetHashCode());
            ClassicAssert.IsTrue(pair5.GetHashCode() == 0);

            ClassicAssert.IsTrue(pair1.GetHashCode() == pair2.GetHashCode());
            ClassicAssert.IsTrue(pair1.GetHashCode() != pair3.GetHashCode());
            ClassicAssert.IsTrue(pair1.GetHashCode() != pair4.GetHashCode());
            ClassicAssert.IsTrue(pair1.GetHashCode() != pair5.GetHashCode());
        }

        [Test]
        public void TestEquals()
        {
            ClassicAssert.AreEqual(pair2, pair1);
            ClassicAssert.AreEqual(pair1, pair2);

            ClassicAssert.IsTrue(pair1 != pair3);
            ClassicAssert.IsTrue(pair3 != pair1);
            ClassicAssert.IsTrue(pair1 != pair4);
            ClassicAssert.IsTrue(pair2 != pair5);
            ClassicAssert.IsTrue(pair3 != pair4);
            ClassicAssert.IsTrue(pair4 != pair5);

            ClassicAssert.IsTrue(pair1 == pair1);
            ClassicAssert.IsTrue(pair2 == pair2);
            ClassicAssert.IsTrue(pair3 == pair3);
            ClassicAssert.IsTrue(pair4 == pair4);
            ClassicAssert.IsTrue(pair5 == pair5);
        }
    }
} // end of namespace
