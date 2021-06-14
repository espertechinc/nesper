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

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestSingleObjectIterator : AbstractCommonTest
    {
        [Test]
        public void TestNext()
        {
            IEnumerator<string> enumerator = EnumerationHelper.Singleton("a");
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo("a"));
            Assert.That(enumerator.MoveNext(), Is.False);
            //Assert.That(() => it.Current, Throws.InstanceOf<InvalidOperationException>());
        }
    }
} // end of namespace
