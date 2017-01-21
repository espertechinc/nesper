///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class TestOrderedDictionary
    {
        [Test]
        public void ShouldBeEmptyWhenCreated()
        {
            var orderedDictionary = new OrderedDictionary<string, object>();
            Assert.That(orderedDictionary.Count, Is.EqualTo(0));
            Assert.That(orderedDictionary.HasFirst(), Is.EqualTo(false));
        }

        [Test]
        public void ShouldMaintainOrder()
        {
            var orderedDictionary = new OrderedDictionary<string, int>();
            orderedDictionary["a"] = 1;
            orderedDictionary["c"] = 2;
            orderedDictionary["e"] = 3;
            orderedDictionary["b"] = 4;
            orderedDictionary["z"] = 5;
            orderedDictionary["q"] = 6;

            Assert.That(orderedDictionary.Keys.Count, Is.EqualTo(6));
            Assert.That(string.Join(",", orderedDictionary.Keys), Is.EqualTo("a,b,c,e,q,z"));
            Assert.That(string.Join(",", orderedDictionary.Values), Is.EqualTo("1,4,2,3,6,5"));
        }

        [Test]
        public void ShouldCreateHeadMapInclusive()
        {
            var orderedDictionary = new OrderedDictionary<string, int>();
            orderedDictionary["a"] = 1;
            orderedDictionary["c"] = 2;
            orderedDictionary["e"] = 3;
            orderedDictionary["b"] = 4;
            orderedDictionary["z"] = 5;
            orderedDictionary["q"] = 6;

            Assert.That(orderedDictionary.Keys.Count, Is.EqualTo(6));

            var subMap = orderedDictionary.Head("e", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("a,b,c,e"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("1,4,2,3"));

            subMap = orderedDictionary.Head("f", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("a,b,c,e"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("1,4,2,3"));
        }

        [Test]
        public void ShouldCreateHeadMapExclusive()
        {
            var orderedDictionary = new OrderedDictionary<string, int>();
            orderedDictionary["a"] = 1;
            orderedDictionary["c"] = 2;
            orderedDictionary["e"] = 3;
            orderedDictionary["b"] = 4;
            orderedDictionary["z"] = 5;
            orderedDictionary["q"] = 6;

            Assert.That(orderedDictionary.Keys.Count, Is.EqualTo(6));

            var subMap = orderedDictionary.Head("e", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("a,b,c"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("1,4,2"));

            subMap = orderedDictionary.Head("d", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("a,b,c"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("1,4,2"));
        }

        [Test]
        public void ShouldCreateTailMapInclusive()
        {
            var orderedDictionary = new OrderedDictionary<string, int>();
            orderedDictionary["a"] = 1;
            orderedDictionary["c"] = 2;
            orderedDictionary["e"] = 3;
            orderedDictionary["b"] = 4;
            orderedDictionary["z"] = 5;
            orderedDictionary["q"] = 6;

            Assert.That(orderedDictionary.Keys.Count, Is.EqualTo(6));

            var subMap = orderedDictionary.Tail("c", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("c,e,q,z"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("2,3,6,5"));

            subMap = orderedDictionary.Tail("d", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("e,q,z"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("3,6,5"));
        }

        [Test]
        public void ShouldCreateTailMapExclusive()
        {
            var orderedDictionary = new OrderedDictionary<string, int>();
            orderedDictionary["a"] = 1;
            orderedDictionary["c"] = 2;
            orderedDictionary["e"] = 3;
            orderedDictionary["b"] = 4;
            orderedDictionary["z"] = 5;
            orderedDictionary["q"] = 6;

            Assert.That(orderedDictionary.Keys.Count, Is.EqualTo(6));

            var subMap = orderedDictionary.Tail("c", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("e,q,z"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("3,6,5"));

            subMap = orderedDictionary.Tail("d", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("e,q,z"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("3,6,5"));
        }

        [Test]
        public void ShouldGetItemsBetween()
        {
            var orderedDictionary = new OrderedDictionary<string, int>();
            orderedDictionary["a"] = 1;
            orderedDictionary["c"] = 2;
            orderedDictionary["e"] = 3;
            orderedDictionary["b"] = 4;
            orderedDictionary["z"] = 5;
            orderedDictionary["q"] = 6;

            Assert.That(orderedDictionary.Keys.Count, Is.EqualTo(6));

            var subMap = orderedDictionary.Between("b", true, "q", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("b,c,e,q"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("4,2,3,6"));

            subMap = orderedDictionary.Between("b", true, "r", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("b,c,e,q"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("4,2,3,6"));

            subMap = orderedDictionary.Between("b", true, "q", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("b,c,e"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("4,2,3"));

            subMap = orderedDictionary.Between("b", true, "r", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("b,c,e,q"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("4,2,3,6"));

            subMap = orderedDictionary.Between("b", true, "zz", false);
            Assert.That(subMap.Keys.Count, Is.EqualTo(5));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("b,c,e,q,z"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("4,2,3,6,5"));

            subMap = orderedDictionary.Between("b", false, "q", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(3));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("c,e,q"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("2,3,6"));

            subMap = orderedDictionary.Between("a", false, "q", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(4));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("b,c,e,q"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("4,2,3,6"));

            subMap = orderedDictionary.Between("0", false, "q", true);
            Assert.That(subMap.Keys.Count, Is.EqualTo(5));
            Assert.That(string.Join(",", subMap.Keys), Is.EqualTo("a,b,c,e,q"));
            Assert.That(string.Join(",", subMap.Values), Is.EqualTo("1,4,2,3,6"));
        }
    }
}
