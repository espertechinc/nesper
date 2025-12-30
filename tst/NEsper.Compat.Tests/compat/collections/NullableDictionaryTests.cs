using System;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class NullableDictionaryTests
    {
        [Test]
        public void SupportsNullKeyAddGetRemove()
        {
            var dict = new NullableDictionary<string, int>();

            dict.Add(null, 10);
            Assert.That(dict.ContainsKey(null), Is.True);
            Assert.That(dict[null], Is.EqualTo(10));

            dict[null] = 20;
            Assert.That(dict[null], Is.EqualTo(20));

            Assert.That(dict.Remove((string) null), Is.True);
            Assert.That(dict.ContainsKey(null), Is.False);
        }

        [Test]
        public void AddNullKeyTwiceThrows()
        {
            var dict = new NullableDictionary<string, int>();
            dict.Add(null, 1);
            Assert.Throws<ArgumentException>(() => dict.Add(null, 2));
        }

        [Test]
        public void KeysAndValuesIncludeNullEntryWhenPresent()
        {
            var dict = new NullableDictionary<string, int>();
            dict.Add(null, 1);
            dict.Add("a", 2);

            Assert.That(dict.Keys.Contains(null), Is.True);
            Assert.That(dict.Values.Contains(1), Is.True);
            Assert.That(dict.Count, Is.EqualTo(2));
        }

        [Test]
        public void EnumerationIncludesNullEntryFirst()
        {
            var dict = new NullableDictionary<string, int>();
            dict.Add(null, 1);
            dict.Add("a", 2);

            var first = dict.First();
            Assert.That(first.Key, Is.Null);
            Assert.That(first.Value, Is.EqualTo(1));
        }
    }
}
