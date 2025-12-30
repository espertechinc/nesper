using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class LinkedHashSetTests
    {
        [Test]
        public void EnumerationIsInsertionOrder()
        {
            var set = new LinkedHashSet<int>();
            set.Add(3);
            set.Add(1);
            set.Add(2);

            Assert.That(set.ToArray(), Is.EqualTo(new[] { 3, 1, 2 }));
        }

        [Test]
        public void AddDuplicateDoesNotChangeCountOrOrder()
        {
            var set = new LinkedHashSet<int>();
            set.Add(1);
            set.Add(2);
            set.Add(1);

            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set.ToArray(), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void RemoveWhereRemovesMatchingItems()
        {
            var set = new LinkedHashSet<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);

            set.RemoveWhere(x => x % 2 == 1);

            Assert.That(set.ToArray(), Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void ClearResets()
        {
            var set = new LinkedHashSet<int>();
            set.Add(1);
            set.Clear();

            Assert.That(set.Count, Is.EqualTo(0));
            Assert.That(set.IsEmpty, Is.True);
        }
    }
}
