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
    public class TestPair 
    {
        private readonly Pair<String, String> _pair1 = new Pair<String, String>("a", "b");
        private readonly Pair<String, String> _pair2 = new Pair<String, String>("a", "b");
        private readonly Pair<String, String> _pair3 = new Pair<String, String>("a", null);
        private readonly Pair<String, String> _pair4 = new Pair<String, String>(null, "b");
        private readonly Pair<String, String> _pair5 = new Pair<String, String>(null, null);
    
        [Test]
        public void TestHashCode()
        {
            Assert.IsTrue(_pair1.GetHashCode() == ("a".GetHashCode()*397 ^ "b".GetHashCode()));
            Assert.IsTrue(_pair3.GetHashCode() == ("a".GetHashCode()*397));
            Assert.IsTrue(_pair4.GetHashCode() == "b".GetHashCode());
            Assert.IsTrue(_pair5.GetHashCode() == 0);
    
            Assert.IsTrue(_pair1.GetHashCode() == _pair2.GetHashCode());
            Assert.IsTrue(_pair1.GetHashCode() != _pair3.GetHashCode());
            Assert.IsTrue(_pair1.GetHashCode() != _pair4.GetHashCode());
            Assert.IsTrue(_pair1.GetHashCode() != _pair5.GetHashCode());
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.AreEqual(_pair2, _pair1);
            Assert.AreEqual(_pair1, _pair2);
    
            Assert.IsTrue(_pair1 != _pair3);
            Assert.IsTrue(_pair3 != _pair1);
            Assert.IsTrue(_pair1 != _pair4);
            Assert.IsTrue(_pair2 != _pair5);
            Assert.IsTrue(_pair3 != _pair4);
            Assert.IsTrue(_pair4 != _pair5);

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(_pair1 == _pair1);
            Assert.IsTrue(_pair2 == _pair2);
            Assert.IsTrue(_pair3 == _pair3);
            Assert.IsTrue(_pair4 == _pair4);
            Assert.IsTrue(_pair5 == _pair5);
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }
}
