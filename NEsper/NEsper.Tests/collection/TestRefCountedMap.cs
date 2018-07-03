///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestRefCountedMap 
    {
        private RefCountedMap<String, int?> _refMap;
    
        [SetUp]
        public void SetUp()
        {
            _refMap = new RefCountedMap<String, int?>();
            _refMap["a"] = 100;
        }
    
        [Test]
        public void TestPut()
        {
            try
            {
                _refMap["a"] = 10;
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // Expected exception
            }
    
            try
            {
                _refMap[null] = 10;
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
            int? val = _refMap["b"];
            Assert.IsNull(val);

            val = _refMap["a"];
            Assert.AreEqual(100, (int) val);
        }
    
        [Test]
        public void TestReference()
        {
            _refMap.Reference("a");
    
            try
            {
                _refMap.Reference("b");
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
            bool isLast = _refMap.Dereference("a");
            Assert.IsTrue(isLast);

            _refMap["b"] = 100;
            _refMap.Reference("b");
            Assert.IsFalse(_refMap.Dereference("b"));
            Assert.IsTrue(_refMap.Dereference("b"));
    
            try
            {
                _refMap.Dereference("b");
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
            _refMap["b"] = -1;
            _refMap.Reference("b");
    
            Assert.AreEqual(-1, (int) _refMap["b"]);
            Assert.IsFalse(_refMap.Dereference("b"));
            Assert.AreEqual(-1, (int) _refMap["b"]);
            Assert.IsTrue(_refMap.Dereference("b"));
            Assert.IsNull(_refMap["b"]);
    
            _refMap["b"] = 2;
            _refMap.Reference("b");

            _refMap["c"] = 3;
            _refMap.Reference("c");
    
            _refMap.Dereference("b");
            _refMap.Reference("b");
    
            Assert.AreEqual(2, (int) _refMap["b"]);
            Assert.IsFalse(_refMap.Dereference("b"));
            Assert.IsTrue(_refMap.Dereference("b"));
            Assert.IsNull(_refMap["b"]);
    
            Assert.AreEqual(3, (int) _refMap["c"]);
            Assert.IsFalse(_refMap.Dereference("c"));
            Assert.AreEqual(3, (int) _refMap["c"]);
            Assert.IsTrue(_refMap.Dereference("c"));
            Assert.IsNull(_refMap["c"]);
        }
    }
}
