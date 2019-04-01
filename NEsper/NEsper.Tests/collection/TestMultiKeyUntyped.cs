///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestMultiKeyUntyped 
    {
        MultiKeyUntyped keys1 = new MultiKeyUntyped("a", "b");
        MultiKeyUntyped keys2 = new MultiKeyUntyped("a", "b");
        MultiKeyUntyped keys3 = new MultiKeyUntyped("a", null);
        MultiKeyUntyped keys4 = new MultiKeyUntyped(null, "b");
        MultiKeyUntyped keys5 = new MultiKeyUntyped(null, null);
        MultiKeyUntyped keys6 = new MultiKeyUntyped("a");
        MultiKeyUntyped keys7 = new MultiKeyUntyped("a", "b", "c");
        MultiKeyUntyped keys8 = new MultiKeyUntyped("a", "b", null);
        MultiKeyUntyped keys9 = new MultiKeyUntyped("a", "b", "c", "d");
        MultiKeyUntyped keys10 = new MultiKeyUntyped("a", "b", "c", "d");

        public static int GetMultiHashCode(params string[] stringList)
        {
            int hashCode = 0;
            for (int ii = 0; ii < stringList.Length; ii++)
            {
                hashCode *= 397;
                hashCode ^= stringList[ii].GetHashCode();
            }

            return hashCode;
        }

        [Test]
        public void TestHashCode()
        {
            Assert.IsTrue(keys1.GetHashCode() == GetMultiHashCode("a", "b"));
            Assert.IsTrue(keys10.GetHashCode() == GetMultiHashCode("a", "b", "c", "d"));
            Assert.IsTrue(keys3.GetHashCode() == "a".GetHashCode());
            Assert.IsTrue(keys4.GetHashCode() == "b".GetHashCode());
            Assert.IsTrue(keys5.GetHashCode() == 0);
    
            Assert.IsTrue(keys8.GetHashCode() == keys1.GetHashCode());
            Assert.IsTrue(keys1.GetHashCode() == keys2.GetHashCode());
            Assert.IsTrue(keys1.GetHashCode() != keys3.GetHashCode());
            Assert.IsTrue(keys1.GetHashCode() != keys4.GetHashCode());
            Assert.IsTrue(keys1.GetHashCode() != keys5.GetHashCode());
    
            Assert.IsTrue(keys7.GetHashCode() != keys8.GetHashCode());
            Assert.IsTrue(keys9.GetHashCode() == keys10.GetHashCode());
        }
    
        [Test]
        public void TestEquals()
        {
            Assert.AreEqual(keys2, keys1);
            Assert.AreEqual(keys1, keys2);
    
            Assert.IsFalse(keys1.Equals(keys3));
            Assert.IsFalse(keys3.Equals(keys1));
            Assert.IsFalse(keys1.Equals(keys4));
            Assert.IsFalse(keys2.Equals(keys5));
            Assert.IsFalse(keys3.Equals(keys4));
            Assert.IsFalse(keys4.Equals(keys5));
    
            Assert.IsTrue(keys1.Equals(keys1));
            Assert.IsTrue(keys2.Equals(keys2));
            Assert.IsTrue(keys3.Equals(keys3));
            Assert.IsTrue(keys4.Equals(keys4));
            Assert.IsTrue(keys5.Equals(keys5));
    
            Assert.IsFalse(keys1.Equals(keys7));
            Assert.IsFalse(keys1.Equals(keys8));
            Assert.IsFalse(keys1.Equals(keys9));
            Assert.IsFalse(keys1.Equals(keys10));
            Assert.IsTrue(keys9.Equals(keys10));
        }
    
        [Test]
        public void TestGet()
        {
            Assert.AreEqual(1, keys6.Count);
            Assert.AreEqual(2, keys1.Count);
            Assert.AreEqual(3, keys8.Count);
            Assert.AreEqual(4, keys9.Count);
    
            Assert.AreEqual("a", keys1.Get(0));
            Assert.AreEqual("b", keys1.Get(1));
            Assert.IsTrue(null == keys4.Get(0));
            Assert.IsTrue("d" == (string) keys10.Get(3));
        }
    
        [Test]
        public void TestWithSet()
        {
            EventBean[][] testEvents = new EventBean[][] {
                SupportEventBeanFactory.MakeEvents(new String[] {"a", "b"}),
                SupportEventBeanFactory.MakeEvents(new String[] {"a"}),
                SupportEventBeanFactory.MakeEvents(new String[] {"a", "b", "c"}),
                SupportEventBeanFactory.MakeEvents(new String[] {"a", "b"}),
            };
    
            ICollection<MultiKeyUntyped> mapSet = new HashSet<MultiKeyUntyped>();
    
            // Test contains
            mapSet.Add(new MultiKeyUntyped(testEvents[0]));
            Assert.IsTrue(mapSet.Contains(new MultiKeyUntyped(testEvents[0])));
            Assert.IsFalse(mapSet.Contains(new MultiKeyUntyped(testEvents[1])));
            Assert.IsFalse(mapSet.Contains(new MultiKeyUntyped(testEvents[2])));
            Assert.IsFalse(mapSet.Contains(new MultiKeyUntyped(testEvents[3])));
    
            // Test unique
            mapSet.Add(new MultiKeyUntyped(testEvents[0]));
            Assert.AreEqual(1, mapSet.Count);
    
            mapSet.Add(new MultiKeyUntyped(testEvents[1]));
            mapSet.Add(new MultiKeyUntyped(testEvents[2]));
            mapSet.Add(new MultiKeyUntyped(testEvents[3]));
            Assert.AreEqual(4, mapSet.Count);
    
            mapSet.Remove(new MultiKeyUntyped(testEvents[0]));
            Assert.AreEqual(3, mapSet.Count);
            Assert.IsFalse(mapSet.Contains(new MultiKeyUntyped(testEvents[0])));
    
            mapSet.Remove(new MultiKeyUntyped(testEvents[1]));
            mapSet.Remove(new MultiKeyUntyped(testEvents[2]));
            mapSet.Remove(new MultiKeyUntyped(testEvents[3]));
            Assert.AreEqual(0, mapSet.Count);
        }
    }
}
