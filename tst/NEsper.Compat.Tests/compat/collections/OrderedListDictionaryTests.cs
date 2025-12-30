using System;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class OrderedListDictionaryTests
    {
        [Test]
        public void AddMaintainsSortedOrder()
        {
            var dict = new OrderedListDictionary<int, string>();
            dict.Add(2, "b");
            dict.Add(1, "a");
            dict.Add(3, "c");

            Assert.That(dict.Select(kvp => kvp.Key).ToArray(), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void IndexerSetUpdatesIfKeyExistsOrInsertsIfMissing()
        {
            var dict = new OrderedListDictionary<int, string>();
            dict[2] = "b";
            dict[1] = "a";
            dict[2] = "b2";

            Assert.That(dict.Count, Is.EqualTo(2));
            Assert.That(dict[2], Is.EqualTo("b2"));
            Assert.That(dict.Select(kvp => kvp.Key).ToArray(), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void AddThrowsOnDuplicateKey()
        {
            var dict = new OrderedListDictionary<int, string>();
            dict.Add(1, "a");
            Assert.Throws<ArgumentException>(() => dict.Add(1, "a2"));
        }

        [Test]
        public void TryGreaterThanOrEqualToFindsNextOrEqual()
        {
            var dict = new OrderedListDictionary<int, string>();
            dict.Add(10, "a");
            dict.Add(20, "b");
            dict.Add(30, "c");

            Assert.That(dict.TryGreaterThanOrEqualTo(20, out var pairEqual), Is.True);
            Assert.That(pairEqual.Key, Is.EqualTo(20));

            Assert.That(dict.TryGreaterThanOrEqualTo(25, out var pairNext), Is.True);
            Assert.That(pairNext.Key, Is.EqualTo(30));

            Assert.That(dict.TryGreaterThanOrEqualTo(35, out _), Is.False);
        }

        [Test]
        public void TryLessThanOrEqualToFindsPrevOrEqual()
        {
            var dict = new OrderedListDictionary<int, string>();
            dict.Add(10, "a");
            dict.Add(20, "b");
            dict.Add(30, "c");

            Assert.That(dict.TryLessThanOrEqualTo(20, out var pairEqual), Is.True);
            Assert.That(pairEqual.Key, Is.EqualTo(20));

            Assert.That(dict.TryLessThanOrEqualTo(25, out var pairPrev), Is.True);
            Assert.That(pairPrev.Key, Is.EqualTo(20));

            Assert.That(dict.TryLessThanOrEqualTo(5, out _), Is.False);
        }
    }
}
