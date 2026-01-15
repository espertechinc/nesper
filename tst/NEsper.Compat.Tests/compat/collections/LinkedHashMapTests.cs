using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class LinkedHashMapTests
    {
        [Test]
        public void EnumerationIsInsertionOrder()
        {
            var map = new LinkedHashMap<string, int>();
            map["b"] = 2;
            map["a"] = 1;
            map["c"] = 3;

            Assert.That(map.Select(kvp => kvp.Key).ToArray(), Is.EqualTo(new[] { "b", "a", "c" }));
            Assert.That(map.Select(kvp => kvp.Value).ToArray(), Is.EqualTo(new[] { 2, 1, 3 }));
        }

        [Test]
        public void ShuffleOnAccessMovesAccessedItemToEnd()
        {
            var map = new LinkedHashMap<string, int>();
            map.ShuffleOnAccess = true;

            map["a"] = 1;
            map["b"] = 2;
            map["c"] = 3;

            Assert.That(map["a"], Is.EqualTo(1));

            Assert.That(map.Select(kvp => kvp.Key).ToArray(), Is.EqualTo(new[] { "b", "c", "a" }));
        }

        [Test]
        public void AddThrowsOnDuplicateKey()
        {
            var map = new LinkedHashMap<string, int>();
            map.Add("a", 1);
            Assert.Throws<System.ArgumentException>(() => map.Add("a", 2));
        }

        [Test]
        public void RemoveEldestCanRemoveHeadOnInsert()
        {
            var map = new LinkedHashMap<string, int>();
            map.RemoveEldest += entry => entry.Key == "a";

            map.Add("a", 1);
            map.Add("b", 2);

            Assert.That(map.ContainsKey("a"), Is.False);
            Assert.That(map.ContainsKey("b"), Is.True);
            Assert.That(map.Select(kvp => kvp.Key).ToArray(), Is.EqualTo(new[] { "b" }));
        }

        [Test]
        public void CopyToPreservesOrder()
        {
            var map = new LinkedHashMap<string, int>();
            map["a"] = 1;
            map["b"] = 2;

            var arr = new KeyValuePair<string, int>[2];
            map.CopyTo(arr, 0);

            Assert.That(arr.Select(p => p.Key).ToArray(), Is.EqualTo(new[] { "a", "b" }));
        }
    }
}
