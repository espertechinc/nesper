///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestArrayBackedCollection : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            coll = new ArrayBackedCollection<int?>(5);
        }

        private ArrayBackedCollection<int?> coll;

        [Test, RunInApplicationDomain]
        public void TestGet()
        {
            Assert.AreEqual(0, coll.Count);
            Assert.AreEqual(5, coll.Array.Length);

            coll.Add(5);
            EPAssertionUtil.AssertEqualsExactOrder(coll.Array, new int?[] { 5, null, null, null, null });
            coll.Add(4);
            EPAssertionUtil.AssertEqualsExactOrder(coll.Array, new int?[] { 5, 4, null, null, null });
            Assert.AreEqual(2, coll.Count);

            coll.Add(1);
            coll.Add(2);
            coll.Add(3);
            EPAssertionUtil.AssertEqualsExactOrder(
                coll.Array,
                new int?[] { 5, 4, 1, 2, 3 });
            Assert.AreEqual(5, coll.Count);

            coll.Add(10);
            EPAssertionUtil.AssertEqualsExactOrder(
                coll.Array,
                new int?[] { 5, 4, 1, 2, 3, 10, null, null, null, null });
            Assert.AreEqual(6, coll.Count);

            coll.Add(11);
            coll.Add(12);
            coll.Add(13);
            coll.Add(14);
            coll.Add(15);
            EPAssertionUtil.AssertEqualsExactOrder(
                coll.Array,
                new int?[] {
                    5, 4, 1, 2, 3, 10, 11, 12, 13, 14, 15,
                    null, null, null, null, null, null, null, null, null
                });
            Assert.AreEqual(11, coll.Count);

            coll.Clear();
            Assert.AreEqual(0, coll.Count);
        }
    }
} // end of namespace