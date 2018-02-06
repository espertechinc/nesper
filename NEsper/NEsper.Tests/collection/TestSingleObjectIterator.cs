///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestSingleObjectIterator
    {
        [Test]
        public void TestNext()
        {
            IEnumerator<string> it = EnumerationHelper.Singleton("a");
            Assert.IsTrue(it.MoveNext());
            Assert.AreEqual("a", it.Current);
            Assert.IsFalse(it.MoveNext());
        }
    }
} // end of namespace
