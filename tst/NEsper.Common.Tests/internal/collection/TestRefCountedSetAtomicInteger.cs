///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestRefCountedSetAtomicInteger : AbstractCommonTest
    {
        [Test]
        public void TestFlow()
        {
            RefCountedSetAtomicInteger<string> set = new RefCountedSetAtomicInteger<string>();

            ClassicAssert.IsFalse(set.Remove("K1"));
            ClassicAssert.IsTrue(set.IsEmpty());

            ClassicAssert.IsTrue(set.Add("K1"));
            ClassicAssert.IsFalse(set.IsEmpty());
            ClassicAssert.IsTrue(set.Remove("K1"));
            ClassicAssert.IsTrue(set.IsEmpty());
            ClassicAssert.IsFalse(set.Remove("K1"));

            ClassicAssert.IsTrue(set.Add("K1"));
            ClassicAssert.IsFalse(set.IsEmpty());
            ClassicAssert.IsFalse(set.Add("K1"));
            ClassicAssert.IsFalse(set.Remove("K1"));
            ClassicAssert.IsFalse(set.IsEmpty());
            ClassicAssert.IsTrue(set.Remove("K1"));
            ClassicAssert.IsFalse(set.Remove("K1"));
            ClassicAssert.IsTrue(set.IsEmpty());

            ClassicAssert.IsTrue(set.Add("K1"));
            ClassicAssert.IsFalse(set.Add("K1"));
            ClassicAssert.IsFalse(set.Add("K1"));
            ClassicAssert.IsFalse(set.Remove("K1"));
            ClassicAssert.IsFalse(set.Remove("K1"));
            ClassicAssert.IsFalse(set.IsEmpty());
            ClassicAssert.IsTrue(set.Remove("K1"));
            ClassicAssert.IsFalse(set.Remove("K1"));
            ClassicAssert.IsTrue(set.IsEmpty());

            ClassicAssert.IsTrue(set.Add("K1"));
            ClassicAssert.IsFalse(set.Add("K1"));
            ClassicAssert.IsTrue(set.Add("K2"));
            set.RemoveAll("K1");
            ClassicAssert.IsFalse(set.IsEmpty());
            set.RemoveAll("K2");
            ClassicAssert.IsTrue(set.Add("K1"));
            ClassicAssert.IsTrue(set.Remove("K1"));
        }
    }
} // end of namespace
