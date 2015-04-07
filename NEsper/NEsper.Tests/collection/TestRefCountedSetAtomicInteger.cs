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
    public class TestRefCountedSetAtomicInteger  {
    
        [Test]
        public void TestFlow() {
            RefCountedSetAtomicInteger<String> set = new RefCountedSetAtomicInteger<String>();
    
            Assert.IsFalse(set.Remove("K1"));
            Assert.IsTrue(set.IsEmpty());
    
            Assert.IsTrue(set.Add("K1"));
            Assert.IsFalse(set.IsEmpty());
            Assert.IsTrue(set.Remove("K1"));
            Assert.IsTrue(set.IsEmpty());
            Assert.IsFalse(set.Remove("K1"));
    
            Assert.IsTrue(set.Add("K1"));
            Assert.IsFalse(set.IsEmpty());
            Assert.IsFalse(set.Add("K1"));
            Assert.IsFalse(set.Remove("K1"));
            Assert.IsFalse(set.IsEmpty());
            Assert.IsTrue(set.Remove("K1"));
            Assert.IsFalse(set.Remove("K1"));
            Assert.IsTrue(set.IsEmpty());
    
            Assert.IsTrue(set.Add("K1"));
            Assert.IsFalse(set.Add("K1"));
            Assert.IsFalse(set.Add("K1"));
            Assert.IsFalse(set.Remove("K1"));
            Assert.IsFalse(set.Remove("K1"));
            Assert.IsFalse(set.IsEmpty());
            Assert.IsTrue(set.Remove("K1"));
            Assert.IsFalse(set.Remove("K1"));
            Assert.IsTrue(set.IsEmpty());
    
            Assert.IsTrue(set.Add("K1"));
            Assert.IsFalse(set.Add("K1"));
            Assert.IsTrue(set.Add("K2"));
            set.RemoveAll("K1");
            Assert.IsFalse(set.IsEmpty());
            set.RemoveAll("K2");
            Assert.IsTrue(set.Add("K1"));
            Assert.IsTrue(set.Remove("K1"));
        }
    }
}
