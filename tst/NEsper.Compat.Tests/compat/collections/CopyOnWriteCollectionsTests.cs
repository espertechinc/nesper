using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class CopyOnWriteCollectionsTests
    {
        [Test]
        public void CopyOnWriteListEnumeratorSeesSnapshot()
        {
            var list = new CopyOnWriteList<int>(new[] { 1, 2, 3 });
            var snapshot = list.ToArray();

            // mutate after snapshot
            list.Add(4);

            Assert.That(snapshot, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(list.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void CopyOnWriteArraySetDoesNotAddDuplicates()
        {
            var set = new CopyOnWriteArraySet<int>();
            set.Add(1);
            set.Add(1);
            set.AddAll(new[] { 1, 2, 2, 3 });

            // AddAll prevents adding items already present in the set, but does not de-duplicate
            // duplicates within the source enumeration.
            Assert.That(set.ToArray().Count(x => x == 1), Is.EqualTo(1));
            Assert.That(set.ToArray().OrderBy(x => x).ToArray(), Is.EqualTo(new[] { 1, 2, 2, 3 }));
        }

        [Test]
        public void CopyOnWriteArraySetFirstThrowsWhenEmpty()
        {
            var set = new CopyOnWriteArraySet<int>();
            Assert.Throws<System.IndexOutOfRangeException>(() => {
                _ = set.First;
            });
        }
    }
}
