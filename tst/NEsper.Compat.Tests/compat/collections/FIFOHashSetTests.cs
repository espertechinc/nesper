using System;
using System.Linq;

using com.espertech.esper.collection;
using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class FIFOHashSetTests
    {
        [Test]
        public void AddPreservesInsertionOrderForEnumeration()
        {
            var set = new FIFOHashSet<int>();
            set.Add(3);
            set.Add(1);
            set.Add(2);

            Assert.That(set.ToArray(), Is.EqualTo(new[] { 3, 1, 2 }));
        }

        [Test]
        public void AddDuplicateDoesNotChangeOrderOrCount()
        {
            var set = new FIFOHashSet<int>();
            var wasPresentFirst = set.Add(1);
            var wasPresentSecond = set.Add(1);

            Assert.That(wasPresentFirst, Is.False);
            Assert.That(wasPresentSecond, Is.True);
            Assert.That(set.Count, Is.EqualTo(1));
            Assert.That(set.ToArray(), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void EnumeratorThrowsIfModified()
        {
            var set = new FIFOHashSet<int>();
            set.Add(1);
            set.Add(2);

            using var en = set.GetEnumerator();
            Assert.That(en.MoveNext(), Is.True);

            set.Add(3);

            Assert.Throws<InvalidOperationException>(() => {
                _ = en.MoveNext();
            });
        }

        [Test]
        public void RemoveRemovesItemAndPreservesRemainingOrder()
        {
            var set = new FIFOHashSet<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);

            Assert.That(set.Remove(2), Is.True);
            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set.ToArray(), Is.EqualTo(new[] { 1, 3 }));
        }

        [Test]
        public void ClearEmptiesSet()
        {
            var set = new FIFOHashSet<int>();
            set.Add(1);
            set.Add(2);

            set.Clear();

            Assert.That(set.Count, Is.EqualTo(0));
            Assert.That(set.ToArray(), Is.Empty);
        }
    }
}
