///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestIntSeqKeyFactory : AbstractCommonTest
    {
        [Test]
        public void TestFactory()
        {
            ClassicAssert.IsTrue(IntSeqKeyFactory.From(new int[0]) is IntSeqKeyRoot);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1 }), typeof(IntSeqKeyOne), 1);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1, 2 }), typeof(IntSeqKeyTwo), 1, 2);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1, 2, 3 }), typeof(IntSeqKeyThree), 1, 2, 3);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1, 2, 3, 4 }), typeof(IntSeqKeyFour), 1, 2, 3, 4);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1, 2, 3, 4, 5 }), typeof(IntSeqKeyFive), 1, 2, 3, 4, 5);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1, 2, 3, 4, 5, 6 }), typeof(IntSeqKeySix), 1, 2, 3, 4, 5, 6);
            AssertKey(IntSeqKeyFactory.From(new int[] { 1, 2, 3, 4, 5, 6, 7 }), typeof(IntSeqKeyMany), 1, 2, 3, 4, 5, 6, 7);
        }

        private void AssertKey(IntSeqKey key, Type clazz, params int[] expected)
        {
            ClassicAssert.AreEqual(key.GetType(), clazz);
            ClassicAssert.IsTrue(Arrays.AreEqual(expected, key.AsIntArray()));
        }
    }
} // end of namespace
