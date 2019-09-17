///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestMultiKey : AbstractCommonTest
    {
        private MultiKey<string> keys1 = new MultiKey<string>(new string[] { "a", "b" });
        private MultiKey<string> keys2 = new MultiKey<string>(new string[] { "a", "b" });
        private MultiKey<string> keys3 = new MultiKey<string>(new string[] { "a", null });
        private MultiKey<string> keys4 = new MultiKey<string>(new string[] { null, "b" });
        private MultiKey<string> keys5 = new MultiKey<string>(new string[] { null, null });
        private MultiKey<string> keys6 = new MultiKey<string>(new string[] { "a" });
        private MultiKey<string> keys7 = new MultiKey<string>(new string[] { "a", "b", "c" });
        private MultiKey<string> keys8 = new MultiKey<string>(new string[] { "a", "b", null });
        private MultiKey<string> keys9 = new MultiKey<string>(new string[] { "a", "b", "c", "d" });
        private MultiKey<string> keys10 = new MultiKey<string>(new string[] { "a", "b", "c", "d" });
        private MultiKey<string> keys11 = new MultiKey<string>(new string[] { "espera", "esperb" });
        private MultiKey<string> keys12 = new MultiKey<string>(new string[] { "esperc", "esperd" });
        private MultiKey<string> keys13 = new MultiKey<string>(new string[] { "espere", "esperf" });

        [Test]
        public void TestHashCode()
        {
            Assert.IsTrue(keys11.GetHashCode() != keys12.GetHashCode());
            Assert.IsTrue(keys12.GetHashCode() != keys13.GetHashCode());

            Assert.IsTrue(keys1.GetHashCode() == ("a".GetHashCode() * 397 ^ "b".GetHashCode()));
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

            Assert.AreEqual("a", keys1[0]);
            Assert.AreEqual("b", keys1[1]);
            Assert.IsTrue(null == keys4[0]);
            Assert.IsTrue("d" == keys10[3]);
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

            ISet<MultiKey<EventBean>> mapSet = new HashSet<MultiKey<EventBean>>();

            // Test contains
            mapSet.Add(new MultiKey<EventBean>(testEvents[0]));
            Assert.IsTrue(mapSet.Contains(new MultiKey<EventBean>(testEvents[0])));
            Assert.IsFalse(mapSet.Contains(new MultiKey<EventBean>(testEvents[1])));
            Assert.IsFalse(mapSet.Contains(new MultiKey<EventBean>(testEvents[2])));
            Assert.IsFalse(mapSet.Contains(new MultiKey<EventBean>(testEvents[3])));

            // Test unique
            mapSet.Add(new MultiKey<EventBean>(testEvents[0]));
            Assert.AreEqual(1, mapSet.Count);

            mapSet.Add(new MultiKey<EventBean>(testEvents[1]));
            mapSet.Add(new MultiKey<EventBean>(testEvents[2]));
            mapSet.Add(new MultiKey<EventBean>(testEvents[3]));
            Assert.AreEqual(4, mapSet.Count);

            mapSet.Remove(new MultiKey<EventBean>(testEvents[0]));
            Assert.AreEqual(3, mapSet.Count);
            Assert.IsFalse(mapSet.Contains(new MultiKey<EventBean>(testEvents[0])));

            mapSet.Remove(new MultiKey<EventBean>(testEvents[1]));
            mapSet.Remove(new MultiKey<EventBean>(testEvents[2]));
            mapSet.Remove(new MultiKey<EventBean>(testEvents[3]));
            Assert.AreEqual(0, mapSet.Count);
        }
    }
} // end of namespace
