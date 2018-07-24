///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class TestChainedArrayList
    {
        [Test]
        public void ShouldBeEmpty()
        {
            var testList = new ChainedArrayList<EventBean>(new EventBean[0], 1024);
            Assert.That(testList.Count, Is.EqualTo(0));
            Assert.That(testList.HasFirst(), Is.EqualTo(false));
        }

        [Test]
        public void ShouldMaintainOrder()
        {
            var testLength = 1000000;
            var testList = new ChainedArrayList<int>(
                GenerateIntRange(0, testLength), 1024);

            Assert.That(testList.Count, Is.EqualTo(testLength));

            var testEnum = testList.GetEnumerator();
            for (int ii = 0; ii < testLength; ii++)
            {
                Assert.That(testEnum.MoveNext(), Is.True);
                Assert.That(testEnum.Current, Is.EqualTo(ii));
            }
        }

        private IEnumerable<int> GenerateIntRange(int startInclusive, int endExclusive)
        {
            for (int ii = startInclusive; ii < endExclusive; ii++)
            {
                yield return ii;
            }
        }
    }
}

