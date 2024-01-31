///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestHashableMultiKey : AbstractCommonTest
    {
        private HashableMultiKey keys1 = new HashableMultiKey("a", "b");
        private HashableMultiKey keys2 = new HashableMultiKey("a", "b");
        private HashableMultiKey keys3 = new HashableMultiKey("a", null);
        private HashableMultiKey keys4 = new HashableMultiKey(null, "b");
        private HashableMultiKey keys5 = new HashableMultiKey(null, null);
        private HashableMultiKey keys6 = new HashableMultiKey("a");
        private HashableMultiKey keys7 = new HashableMultiKey("a", "b", "c");
        private HashableMultiKey keys8 = new HashableMultiKey("a", "b", null);
        private HashableMultiKey keys9 = new HashableMultiKey("a", "b", "c", "d");
        private HashableMultiKey keys10 = new HashableMultiKey("a", "b", "c", "d");

        [Test]
        public void TestHashCode()
        {
            ClassicAssert.IsTrue(keys1.GetHashCode() == ("a".GetHashCode() * 31 ^ "b".GetHashCode()));
            ClassicAssert.IsTrue(keys3.GetHashCode() == "a".GetHashCode());
            ClassicAssert.IsTrue(keys4.GetHashCode() == "b".GetHashCode());
            ClassicAssert.IsTrue(keys5.GetHashCode() == 0);

            ClassicAssert.IsTrue(keys8.GetHashCode() == keys1.GetHashCode());
            ClassicAssert.IsTrue(keys1.GetHashCode() == keys2.GetHashCode());
            ClassicAssert.IsTrue(keys1.GetHashCode() != keys3.GetHashCode());
            ClassicAssert.IsTrue(keys1.GetHashCode() != keys4.GetHashCode());
            ClassicAssert.IsTrue(keys1.GetHashCode() != keys5.GetHashCode());

            ClassicAssert.IsTrue(keys7.GetHashCode() != keys8.GetHashCode());
            ClassicAssert.IsTrue(keys9.GetHashCode() == keys10.GetHashCode());
        }

        [Test]
        public void TestEquals()
        {
            ClassicAssert.AreEqual(keys2, keys1);
            ClassicAssert.AreEqual(keys1, keys2);

            ClassicAssert.IsFalse(keys1.Equals(keys3));
            ClassicAssert.IsFalse(keys3.Equals(keys1));
            ClassicAssert.IsFalse(keys1.Equals(keys4));
            ClassicAssert.IsFalse(keys2.Equals(keys5));
            ClassicAssert.IsFalse(keys3.Equals(keys4));
            ClassicAssert.IsFalse(keys4.Equals(keys5));

            ClassicAssert.IsTrue(keys1.Equals(keys1));
            ClassicAssert.IsTrue(keys2.Equals(keys2));
            ClassicAssert.IsTrue(keys3.Equals(keys3));
            ClassicAssert.IsTrue(keys4.Equals(keys4));
            ClassicAssert.IsTrue(keys5.Equals(keys5));

            ClassicAssert.IsFalse(keys1.Equals(keys7));
            ClassicAssert.IsFalse(keys1.Equals(keys8));
            ClassicAssert.IsFalse(keys1.Equals(keys9));
            ClassicAssert.IsFalse(keys1.Equals(keys10));
            ClassicAssert.IsTrue(keys9.Equals(keys10));
        }

        [Test]
        public void TestGet()
        {
            ClassicAssert.AreEqual(1, keys6.Count);
            ClassicAssert.AreEqual(2, keys1.Count);
            ClassicAssert.AreEqual(3, keys8.Count);
            ClassicAssert.AreEqual(4, keys9.Count);

            ClassicAssert.AreEqual("a", keys1.Get(0));
            ClassicAssert.AreEqual("b", keys1.Get(1));
            ClassicAssert.IsTrue(null == keys4.Get(0));
            ClassicAssert.IsTrue("d" == keys10.Get(3));
        }

        [Test]
        public void TestWithSet()
        {
            EventBean[][] testEvents = new EventBean[][]{
                    SupportEventBeanFactory.MakeEvents(supportEventTypeFactory, new string[]{"a", "b"}),
                    SupportEventBeanFactory.MakeEvents(supportEventTypeFactory, new string[]{"a"}),
                    SupportEventBeanFactory.MakeEvents(supportEventTypeFactory, new string[]{"a", "b", "c"}),
                    SupportEventBeanFactory.MakeEvents(supportEventTypeFactory, new string[]{"a", "b"}),
            };

            ISet<HashableMultiKey> mapSet = new HashSet<HashableMultiKey>();

            // Test contains
            mapSet.Add(new HashableMultiKey(testEvents[0]));
            ClassicAssert.IsTrue(mapSet.Contains(new HashableMultiKey(testEvents[0])));
            ClassicAssert.IsFalse(mapSet.Contains(new HashableMultiKey(testEvents[1])));
            ClassicAssert.IsFalse(mapSet.Contains(new HashableMultiKey(testEvents[2])));
            ClassicAssert.IsFalse(mapSet.Contains(new HashableMultiKey(testEvents[3])));

            // Test unique
            mapSet.Add(new HashableMultiKey(testEvents[0]));
            ClassicAssert.AreEqual(1, mapSet.Count);

            mapSet.Add(new HashableMultiKey(testEvents[1]));
            mapSet.Add(new HashableMultiKey(testEvents[2]));
            mapSet.Add(new HashableMultiKey(testEvents[3]));
            ClassicAssert.AreEqual(4, mapSet.Count);

            mapSet.Remove(new HashableMultiKey(testEvents[0]));
            ClassicAssert.AreEqual(3, mapSet.Count);
            ClassicAssert.IsFalse(mapSet.Contains(new HashableMultiKey(testEvents[0])));

            mapSet.Remove(new HashableMultiKey(testEvents[1]));
            mapSet.Remove(new HashableMultiKey(testEvents[2]));
            mapSet.Remove(new HashableMultiKey(testEvents[3]));
            ClassicAssert.AreEqual(0, mapSet.Count);
        }
    }
} // end of namespace