///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestArrayDeque
    {
        [Test]
        public void TestEmptyArrayDeque()
        {
            var deque = new ArrayDeque<int>();
            Assert.That(deque.Count, Is.EqualTo(0));
            Assert.That(deque.Count(), Is.EqualTo(0));
            Assert.That(() => deque.First(), Throws.InvalidOperationException.With.Message.EqualTo("Sequence contains no elements"));
        }

        [Test]
        public void TestAddFront()
        {
            var deque = new ArrayDeque<int>();
            Assert.That(deque.Count, Is.EqualTo(0));

            for (int ii = 1; ii <= 1000; ii++)
            {
                deque.AddFirst(ii);
                Assert.That(deque.Count, Is.EqualTo(ii));
                Assert.That(deque.First, Is.EqualTo(ii));
            }

            int index = 1000;
            foreach (var value in deque)
            {
                Assert.That(value, Is.EqualTo(index--));
            }
        }

        [Test]
        public void TestArrayFirst()
        {
            var deque = new ArrayDeque<int>();
            Assert.That(deque.Count, Is.EqualTo(0));

            deque.Add(1);
            Assert.That(deque.Count, Is.EqualTo(1));
            Assert.That(deque.First, Is.EqualTo(1));

            deque.Add(2);
            Assert.That(deque.Count, Is.EqualTo(2));
            Assert.That(deque.First, Is.EqualTo(1));

            deque.Add(3);
            Assert.That(deque.Count, Is.EqualTo(3));
            Assert.That(deque.First, Is.EqualTo(1));

            deque.Add(4);
            Assert.That(deque.Count, Is.EqualTo(4));
            Assert.That(deque.First, Is.EqualTo(1));

            Assert.That(deque.RemoveFirst(), Is.EqualTo(1));
            Assert.That(deque.Count, Is.EqualTo(3));
            Assert.That(deque.First, Is.EqualTo(2));
        }

        [Test]
        public void TestArrayLast()
        {
            var deque = new ArrayDeque<int>();
            Assert.That(deque.Count, Is.EqualTo(0));

            deque.Add(1);
            Assert.That(deque.Count, Is.EqualTo(1));
            Assert.That(deque.Last, Is.EqualTo(1));

            deque.Add(2);
            Assert.That(deque.Count, Is.EqualTo(2));
            Assert.That(deque.Last, Is.EqualTo(2));

            deque.Add(3);
            Assert.That(deque.Count, Is.EqualTo(3));
            Assert.That(deque.Last, Is.EqualTo(3));

            deque.Add(4);
            Assert.That(deque.Count, Is.EqualTo(4));
            Assert.That(deque.Last, Is.EqualTo(4));

            Assert.That(deque.RemoveFirst(), Is.EqualTo(1));
            Assert.That(deque.Count, Is.EqualTo(3));
            Assert.That(deque.Last, Is.EqualTo(4));

            Assert.That(deque.RemoveLast(), Is.EqualTo(4));
            Assert.That(deque.Count, Is.EqualTo(2));
            Assert.That(deque.Last, Is.EqualTo(3));
        }

        [Test]
        public void TestEnumeration()
        {
            var deque = new ArrayDeque<int>();
            Assert.That(deque.Count, Is.EqualTo(0));

            for (int ii = 1; ii <= 1000; ii++)
            {
                deque.Add(ii);
                Assert.That(deque.Count, Is.EqualTo(ii));
                Assert.That(deque.Last, Is.EqualTo(ii));
            }

            int index = 1;
            foreach (var value in deque)
            {
                Assert.That(value, Is.EqualTo(index++));
            }
        }

        [Test]
        public void TestBadRemoval()
        {
            var deque = new ArrayDeque<int>();
            deque.Add(1);
            deque.Add(1);
            deque.Add(2);

            Assert.That(deque[0], Is.EqualTo(1));
            Assert.That(deque[1], Is.EqualTo(1));
            Assert.That(deque[2], Is.EqualTo(2));

            deque.Remove(1);

            Assert.That(deque[0], Is.EqualTo(1));
            Assert.That(deque[1], Is.EqualTo(2));
        }
    }
}
