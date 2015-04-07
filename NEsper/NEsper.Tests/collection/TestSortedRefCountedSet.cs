///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class TestSortedRefCountedSet 
    {
        private SortedRefCountedSet<String> _refSet;
    
        [SetUp]
        public void SetUp()
        {
            _refSet = new SortedRefCountedSet<String>();
        }
    
        [Test]
        public void TestMaxMinValue()
        {
            _refSet.Add("a");
            _refSet.Add("b");
            Assert.AreEqual("ba", _refSet.MaxValue + _refSet.MinValue);
            _refSet.Remove("a");
            Assert.AreEqual("bb", _refSet.MaxValue + _refSet.MinValue);
            _refSet.Remove("b");
            Assert.IsNull(_refSet.MaxValue);
            Assert.IsNull(_refSet.MinValue);
    
            _refSet.Add("b");
            _refSet.Add("a");
            _refSet.Add("d");
            _refSet.Add("a");
            _refSet.Add("c");
            _refSet.Add("a");
            _refSet.Add("c");
            Assert.AreEqual("da", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("d");
            Assert.AreEqual("ca", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("a");
            Assert.AreEqual("ca", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("a");
            Assert.AreEqual("ca", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("c");
            Assert.AreEqual("ca", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("c");
            Assert.AreEqual("ba", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("a");
            Assert.AreEqual("bb", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("b");
            Assert.IsNull(_refSet.MaxValue);
            Assert.IsNull(_refSet.MinValue);
        }
    
        [Test]
        public void TestAdd()
        {
            _refSet.Add("a");
            _refSet.Add("b");
            _refSet.Add("a");
            _refSet.Add("c");
            _refSet.Add("a");
    
            Assert.AreEqual("c", _refSet.MaxValue);
            Assert.AreEqual("a", _refSet.MinValue);
        }
    
        [Test]
        public void TestRemove()
        {
            _refSet.Add("a");
            _refSet.Remove("a");
            Assert.IsNull(_refSet.MaxValue);
            Assert.IsNull(_refSet.MinValue);
    
            _refSet.Add("a");
            _refSet.Add("a");
            Assert.AreEqual("aa", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("a");
            Assert.AreEqual("aa", _refSet.MaxValue + _refSet.MinValue);
    
            _refSet.Remove("a");
            Assert.IsNull(_refSet.MaxValue);
            Assert.IsNull(_refSet.MinValue);
    
            // nothing to remove
            _refSet.Remove("c");
    
            _refSet.Add("a");
            _refSet.Remove("a");
            _refSet.Remove("a");
        }
    }
}
