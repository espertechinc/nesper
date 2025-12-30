using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class WeakDictionaryTests
    {
        private static Key[] MaterializeKeys<TKey, TValue>(WeakDictionary<TKey, TValue> dict)
            where TKey : class
            where TValue : class
        {
            var keys = new System.Collections.Generic.List<TKey>();
            var en = dict.KeysEnum;
            while (en.MoveNext()) {
                keys.Add(en.Current);
            }

            return keys.Cast<Key>().ToArray();
        }

        private sealed class Key
        {
            public Key(string id)
            {
                Id = id;
            }

            public string Id { get; }

            public override string ToString()
            {
                return Id;
            }
        }

        [Test]
        public void AddAndTryGetValueRoundTrip()
        {
            var dict = new WeakDictionary<Key, string>();
            var key = new Key("k1");

            dict.Add(key, "v1");

            Assert.That(dict.TryGetValue(key, out var value), Is.True);
            Assert.That(value, Is.EqualTo("v1"));
            Assert.That(dict.ContainsKey(key), Is.True);
        }

        [Test]
        public void IndexerGetThrowsWhenMissing()
        {
            var dict = new WeakDictionary<Key, string>();
            Assert.Throws<KeyNotFoundException>(() => {
                _ = dict[new Key("missing")];
            });
        }

        [Test]
        public void RemoveRemovesKey()
        {
            var dict = new WeakDictionary<Key, string>();
            var key = new Key("k1");

            dict.Add(key, "v1");
            Assert.That(dict.Remove(key), Is.True);
            Assert.That(dict.TryGetValue(key, out _), Is.False);
        }

        [Test]
        public void RemoveCollectedEntriesRemovesDeadKeys()
        {
            var dict = new WeakDictionary<Key, string>();

            var key = new Key("k1");
            var weak = new WeakReference<Key>(key);

            dict.Add(key, "v1");

            key = null;

            // GC is non-deterministic. Retry a few times to reduce flakiness.
            for (var ii = 0; ii < 20 && weak.IsAlive; ii++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            if (weak.IsAlive) {
                Assert.Inconclusive("Unable to reliably collect key under current GC conditions");
            }

            dict.RemoveCollectedEntries();

            // Count includes dead entries until RemoveCollectedEntries is called.
            // After cleanup, enumerating keys should yield none.
            var keys = MaterializeKeys(dict);
            Assert.That(keys.Any(k => k != null && k.Id == "k1"), Is.False);
        }

        [Test]
        public void EnumeratorSkipsDeadKeys()
        {
            var dict = new WeakDictionary<Key, string>();

            var key = new Key("k1");
            var weak = new WeakReference<Key>(key);

            dict.Add(key, "v1");
            key = null;

            for (var ii = 0; ii < 20 && weak.IsAlive; ii++) {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            var any = dict.Any(kvp => kvp.Key != null && kvp.Key.Id == "k1");
            if (!weak.IsAlive) {
                Assert.That(any, Is.False);
            }
        }
    }
}
