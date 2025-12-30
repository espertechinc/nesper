using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class SynchronizedCollectionsTests
    {
        [Test]
        public void SynchronizedListDelegatesOperations()
        {
            var baseList = new List<int>();
            var list = new SynchronizedList<int>(baseList);

            list.Add(1);
            list.Add(2);

            Assert.That(baseList.ToArray(), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(list.Contains(2), Is.True);
            Assert.That(list[0], Is.EqualTo(1));
        }

        [Test]
        public void SynchronizedSetBehavesLikeSet()
        {
            var baseSet = new HashSet<int>();
            var set = new SynchronizedSet<int>(baseSet);

            ((ISet<int>)set).Add(1);
            ((ISet<int>)set).Add(1);
            ((ISet<int>)set).Add(2);

            Assert.That(set.Count, Is.EqualTo(2));
            Assert.That(set.ToArray().OrderBy(x => x).ToArray(), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void SynchronizedCollectionAddAllAndRemoveAllWork()
        {
            var baseList = new List<int>();
            var coll = new SynchronizedCollection<int>(baseList);

            coll.AddAll(new[] { 1, 2, 3 });
            coll.RemoveAll(new[] { 2, 3 });

            Assert.That(coll.ToArray(), Is.EqualTo(new[] { 1 }));
        }
    }
}
