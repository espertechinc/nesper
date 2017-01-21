///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestUniformPair 
    {
        private UniformPair<String> pair1 = new UniformPair<String>("a", "b");
        private UniformPair<String> pair2 = new UniformPair<String>("a", "b");
        private UniformPair<String> pair3 = new UniformPair<String>("a", null);
        private UniformPair<String> pair4 = new UniformPair<String>(null, "b");
        private UniformPair<String> pair5 = new UniformPair<String>(null, null);
    
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
    
            Assert.AreSame(pair1, pair1);
            Assert.AreSame(pair2, pair2);
            Assert.AreSame(pair3, pair3);
            Assert.AreSame(pair4, pair4);
            Assert.AreSame(pair5, pair5);
        }
    }
}
