using NUnit.Framework;

namespace com.espertech.esper.compat
{
    [TestFixture]
    public class CacheTests
    {
        private sealed class Key
        {
            public Key(string id)
            {
                Id = id;
            }

            public string Id { get; }
        }

        [Test]
        public void Cache_HitAndMiss()
        {
            var cache = new Cache<Key, string>();
            var k1 = new Key("k1");
            var k2 = new Key("k2");

            Assert.That(cache.TryGet(k1, out _), Is.False);
            Assert.That(cache.Get(k1), Is.Null);

            cache.Put(k1, "v1");
            Assert.That(cache.TryGet(k1, out var v1), Is.True);
            Assert.That(v1, Is.EqualTo("v1"));

            // Reference equality is used for key compare
            Assert.That(cache.TryGet(k2, out _), Is.False);
            Assert.That(cache.Get(k2), Is.Null);
        }

        [Test]
        public void Cache_InvalidateClears()
        {
            var cache = new Cache<Key, string>();
            var k1 = new Key("k1");

            cache.Put(k1, "v1");
            cache.Invalidate();

            Assert.That(cache.TryGet(k1, out _), Is.False);
            Assert.That(cache.Get(k1), Is.Null);
        }

        [Test]
        public void Cache2D_KeepsTwoMostRecent()
        {
            var cache = new Cache2D<Key, string>();
            var k1 = new Key("k1");
            var k2 = new Key("k2");
            var k3 = new Key("k3");

            cache.Put(k1, "v1");
            cache.Put(k2, "v2");

            Assert.That(cache.TryGet(k1, out var v1), Is.True);
            Assert.That(v1, Is.EqualTo("v1"));
            Assert.That(cache.TryGet(k2, out var v2), Is.True);
            Assert.That(v2, Is.EqualTo("v2"));

            cache.Put(k3, "v3");

            // after third insert, oldest (k1) should be evicted
            Assert.That(cache.TryGet(k1, out _), Is.False);
            Assert.That(cache.TryGet(k2, out var v2b), Is.True);
            Assert.That(v2b, Is.EqualTo("v2"));
            Assert.That(cache.TryGet(k3, out var v3), Is.True);
            Assert.That(v3, Is.EqualTo("v3"));
        }

        [Test]
        public void Cache3D_KeepsThreeMostRecent()
        {
            var cache = new Cache3D<Key, string>();
            var k1 = new Key("k1");
            var k2 = new Key("k2");
            var k3 = new Key("k3");
            var k4 = new Key("k4");

            cache.Put(k1, "v1");
            cache.Put(k2, "v2");
            cache.Put(k3, "v3");

            Assert.That(cache.TryGet(k1, out var v1), Is.True);
            Assert.That(v1, Is.EqualTo("v1"));
            Assert.That(cache.TryGet(k2, out var v2), Is.True);
            Assert.That(v2, Is.EqualTo("v2"));
            Assert.That(cache.TryGet(k3, out var v3), Is.True);
            Assert.That(v3, Is.EqualTo("v3"));

            cache.Put(k4, "v4");

            // after fourth insert, oldest (k1) should be evicted
            Assert.That(cache.TryGet(k1, out _), Is.False);
            Assert.That(cache.TryGet(k2, out var v2b), Is.True);
            Assert.That(v2b, Is.EqualTo("v2"));
            Assert.That(cache.TryGet(k3, out var v3b), Is.True);
            Assert.That(v3b, Is.EqualTo("v3"));
            Assert.That(cache.TryGet(k4, out var v4), Is.True);
            Assert.That(v4, Is.EqualTo("v4"));
        }

        [Test]
        public void NoCache_NeverHits()
        {
            var cache = new NoCache<Key, string>();
            var k1 = new Key("k1");

            cache.Put(k1, "v1");

            Assert.That(cache.TryGet(k1, out _), Is.False);
            Assert.That(cache.Get(k1), Is.Null);
        }
    }
}
