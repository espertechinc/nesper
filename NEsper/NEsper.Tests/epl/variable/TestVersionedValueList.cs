///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.epl.variable
{
    [TestFixture]
    public class TestVersionedValueList 
    {
        private VersionedValueList<String> _list;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
            var rLock = _container.LockManager().CreateLock(GetType());
            _list = new VersionedValueList<String>("abc", 2, "a", 1000, 10000, rLock, 10, true);
        }
    
        [Test]
        public void TestFlowNoTime()
        {
            TryInvalid(0);
            TryInvalid(1);
            Assert.AreEqual("a", _list.GetVersion(2));
            Assert.AreEqual("a", _list.GetVersion(3));
    
            _list.AddValue(4, "b", 0);
            TryInvalid(1);
            Assert.AreEqual("a", _list.GetVersion(2));
            Assert.AreEqual("a", _list.GetVersion(3));
            Assert.AreEqual("b", _list.GetVersion(4));
            Assert.AreEqual("b", _list.GetVersion(5));
    
            _list.AddValue(6, "c", 0);
            TryInvalid(1);
            Assert.AreEqual("a", _list.GetVersion(2));
            Assert.AreEqual("a", _list.GetVersion(3));
            Assert.AreEqual("b", _list.GetVersion(4));
            Assert.AreEqual("b", _list.GetVersion(5));
            Assert.AreEqual("c", _list.GetVersion(6));
            Assert.AreEqual("c", _list.GetVersion(7));
    
            _list.AddValue(7, "d", 0);
            TryInvalid(1);
            Assert.AreEqual("a", _list.GetVersion(2));
            Assert.AreEqual("a", _list.GetVersion(3));
            Assert.AreEqual("b", _list.GetVersion(4));
            Assert.AreEqual("b", _list.GetVersion(5));
            Assert.AreEqual("c", _list.GetVersion(6));
            Assert.AreEqual("d", _list.GetVersion(7));
            Assert.AreEqual("d", _list.GetVersion(8));
    
            _list.AddValue(9, "e", 0);
            TryInvalid(1);
            Assert.AreEqual("a", _list.GetVersion(2));
            Assert.AreEqual("a", _list.GetVersion(3));
            Assert.AreEqual("b", _list.GetVersion(4));
            Assert.AreEqual("b", _list.GetVersion(5));
            Assert.AreEqual("c", _list.GetVersion(6));
            Assert.AreEqual("d", _list.GetVersion(7));
            Assert.AreEqual("d", _list.GetVersion(8));
            Assert.AreEqual("e", _list.GetVersion(9));
            Assert.AreEqual("e", _list.GetVersion(10));
        }
    
        [Test]
        public void TestHighWatermark()
        {
            _list.AddValue(3, "b", 3000);
            _list.AddValue(4, "c", 4000);
            _list.AddValue(5, "d", 5000);
            _list.AddValue(6, "e", 6000);
            _list.AddValue(7, "f", 7000);
            _list.AddValue(8, "g", 8000);
            _list.AddValue(9, "h", 9000);
            _list.AddValue(10, "i", 10000);
            _list.AddValue(11, "j", 10500);
            _list.AddValue(12, "k", 10600);
            Assert.AreEqual(9, _list.OlderVersions.Count);
    
            TryInvalid(0);
            TryInvalid(1);
            Assert.AreEqual("a", _list.GetVersion(2));
            Assert.AreEqual("b", _list.GetVersion(3));
            Assert.AreEqual("c", _list.GetVersion(4));
            Assert.AreEqual("d", _list.GetVersion(5));
            Assert.AreEqual("e", _list.GetVersion(6));
            Assert.AreEqual("f", _list.GetVersion(7));
            Assert.AreEqual("g", _list.GetVersion(8));
            Assert.AreEqual("k", _list.GetVersion(12));
            Assert.AreEqual("k", _list.GetVersion(13));
    
            _list.AddValue(15, "x", 11000);  // 11th value added
            Assert.AreEqual(9, _list.OlderVersions.Count);
            
            TryInvalid(0);
            TryInvalid(1);
            TryInvalid(2);
            Assert.AreEqual("b", _list.GetVersion(3));
            Assert.AreEqual("c", _list.GetVersion(4));
            Assert.AreEqual("d", _list.GetVersion(5));
            Assert.AreEqual("k", _list.GetVersion(13));
            Assert.AreEqual("k", _list.GetVersion(14));
            Assert.AreEqual("x", _list.GetVersion(15));
    
            // expire all before 5.5 sec
            _list.AddValue(20, "y", 15500);  // 11th value added
            Assert.AreEqual(7, _list.OlderVersions.Count);
    
            TryInvalid(0);
            TryInvalid(1);
            TryInvalid(2);
            TryInvalid(3);
            TryInvalid(4);
            TryInvalid(5);
            Assert.AreEqual("e", _list.GetVersion(6));
            Assert.AreEqual("k", _list.GetVersion(13));
            Assert.AreEqual("x", _list.GetVersion(15));
            Assert.AreEqual("x", _list.GetVersion(16));
            Assert.AreEqual("y", _list.GetVersion(20));
    
            // expire all before 10.5 sec
            _list.AddValue(21, "z1", 20500);  
            _list.AddValue(22, "z2", 20500);  
            _list.AddValue(23, "z3", 20501);
            Assert.AreEqual(4, _list.OlderVersions.Count);
            TryInvalid(9);
            TryInvalid(10);
            TryInvalid(11);
            Assert.AreEqual("k", _list.GetVersion(12));
            Assert.AreEqual("k", _list.GetVersion(13));
            Assert.AreEqual("k", _list.GetVersion(14));
            Assert.AreEqual("x", _list.GetVersion(15));
            Assert.AreEqual("x", _list.GetVersion(16));
            Assert.AreEqual("y", _list.GetVersion(20));
            Assert.AreEqual("z1", _list.GetVersion(21));
            Assert.AreEqual("z2", _list.GetVersion(22));
            Assert.AreEqual("z3", _list.GetVersion(23));
            Assert.AreEqual("z3", _list.GetVersion(24));
        }
    
        private void TryInvalid(int version)
        {
            try
            {
                _list.GetVersion(version);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
            }
        }
    }
}
