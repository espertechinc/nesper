///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestMultiKey : AbstractCommonTest
    {
        private MultiKeyArrayOfKeys<string> keys1 = new MultiKeyArrayOfKeys<string>(new string[] { "a", "b" });
        private MultiKeyArrayOfKeys<string> keys2 = new MultiKeyArrayOfKeys<string>(new string[] { "a", "b" });
        private MultiKeyArrayOfKeys<string> keys3 = new MultiKeyArrayOfKeys<string>(new string[] { "a", null });
        private MultiKeyArrayOfKeys<string> keys4 = new MultiKeyArrayOfKeys<string>(new string[] { null, "b" });
        private MultiKeyArrayOfKeys<string> keys5 = new MultiKeyArrayOfKeys<string>(new string[] { null, null });
        private MultiKeyArrayOfKeys<string> keys6 = new MultiKeyArrayOfKeys<string>(new string[] { "a" });
        private MultiKeyArrayOfKeys<string> keys7 = new MultiKeyArrayOfKeys<string>(new string[] { "a", "b", "c" });
        private MultiKeyArrayOfKeys<string> keys8 = new MultiKeyArrayOfKeys<string>(new string[] { "a", "b", null });
        private MultiKeyArrayOfKeys<string> keys9 = new MultiKeyArrayOfKeys<string>(new string[] { "a", "b", "c", "d" });
        private MultiKeyArrayOfKeys<string> keys10 = new MultiKeyArrayOfKeys<string>(new string[] { "a", "b", "c", "d" });
        private MultiKeyArrayOfKeys<string> keys11 = new MultiKeyArrayOfKeys<string>(new string[] { "espera", "esperb" });
        private MultiKeyArrayOfKeys<string> keys12 = new MultiKeyArrayOfKeys<string>(new string[] { "esperc", "esperd" });
        private MultiKeyArrayOfKeys<string> keys13 = new MultiKeyArrayOfKeys<string>(new string[] { "espere", "esperf" });

        [Test]
        public void TestHashCode()
        {
            ClassicAssert.IsTrue(keys11.GetHashCode() != keys12.GetHashCode());
            ClassicAssert.IsTrue(keys12.GetHashCode() != keys13.GetHashCode());

            ClassicAssert.IsTrue(keys1.GetHashCode() == ("a".GetHashCode() * 397 ^ "b".GetHashCode()));
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

            ClassicAssert.AreEqual("a", keys1[0]);
            ClassicAssert.AreEqual("b", keys1[1]);
            ClassicAssert.IsTrue(null == keys4[0]);
            ClassicAssert.IsTrue("d" == keys10[3]);
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

            ISet<MultiKeyArrayOfKeys<EventBean>> mapSet = new HashSet<MultiKeyArrayOfKeys<EventBean>>();

            // Test contains
            mapSet.Add(new MultiKeyArrayOfKeys<EventBean>(testEvents[0]));
            ClassicAssert.IsTrue(mapSet.Contains(new MultiKeyArrayOfKeys<EventBean>(testEvents[0])));
            ClassicAssert.IsFalse(mapSet.Contains(new MultiKeyArrayOfKeys<EventBean>(testEvents[1])));
            ClassicAssert.IsFalse(mapSet.Contains(new MultiKeyArrayOfKeys<EventBean>(testEvents[2])));
            ClassicAssert.IsFalse(mapSet.Contains(new MultiKeyArrayOfKeys<EventBean>(testEvents[3])));

            // Test unique
            mapSet.Add(new MultiKeyArrayOfKeys<EventBean>(testEvents[0]));
            ClassicAssert.AreEqual(1, mapSet.Count);

            mapSet.Add(new MultiKeyArrayOfKeys<EventBean>(testEvents[1]));
            mapSet.Add(new MultiKeyArrayOfKeys<EventBean>(testEvents[2]));
            mapSet.Add(new MultiKeyArrayOfKeys<EventBean>(testEvents[3]));
            ClassicAssert.AreEqual(4, mapSet.Count);

            mapSet.Remove(new MultiKeyArrayOfKeys<EventBean>(testEvents[0]));
            ClassicAssert.AreEqual(3, mapSet.Count);
            ClassicAssert.IsFalse(mapSet.Contains(new MultiKeyArrayOfKeys<EventBean>(testEvents[0])));

            mapSet.Remove(new MultiKeyArrayOfKeys<EventBean>(testEvents[1]));
            mapSet.Remove(new MultiKeyArrayOfKeys<EventBean>(testEvents[2]));
            mapSet.Remove(new MultiKeyArrayOfKeys<EventBean>(testEvents[3]));
            ClassicAssert.AreEqual(0, mapSet.Count);
        }
    }
} // end of namespace
