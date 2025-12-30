using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections
{
    [TestFixture]
    public class CompatExtensionsTests
    {
        [Test]
        public void DeepEquals_SameReferenceAndNullHandling()
        {
            Assert.That(CompatExtensions.DeepEquals(null, null), Is.True);
            Assert.That(CompatExtensions.DeepEquals(new[] { 1, 2 }, null), Is.False);

            var arr = new[] { 1, 2 };
            Assert.That(CompatExtensions.DeepEquals(arr, arr), Is.True);
        }

        [Test]
        public void DeepEquals_ArraysAndNestedEnumerables()
        {
            var a1 = new object[] { 1, new[] { "a", "b" }, new List<int> { 10, 20 } };
            var a2 = new object[] { 1, new[] { "a", "b" }, new List<int> { 10, 20 } };

            Assert.That(CompatExtensions.DeepEquals(a1, a2), Is.True);

            var a3 = new object[] { 1, new[] { "a", "X" }, new List<int> { 10, 20 } };
            Assert.That(CompatExtensions.DeepEquals(a1, a3), Is.False);
        }

        [Test]
        public void DeepEquals_TypeMismatchIsFalse()
        {
            // DeepEquals(IEnumerable,IEnumerable) compares sequence content, not concrete type.
            Assert.That(CompatExtensions.DeepEquals(new[] { 1, 2 }, new List<int> { 1, 2 }), Is.True);
        }

        [Test]
        public void DeepHash_IsStableForSameValues()
        {
            var v1 = new object[] { 1, new[] { "a", "b" }, new List<int> { 10, 20 } };
            var v2 = new object[] { 1, new[] { "a", "b" }, new List<int> { 10, 20 } };

            Assert.That(CompatExtensions.DeepHash(v1), Is.EqualTo(CompatExtensions.DeepHash(v2)));
        }

        [Test]
        public void UnwrapEnumerable_FromNonGenericEnumerableFiltersByType()
        {
            IEnumerable source = new object[] { 1, "x", 2, null };

            var intsNoNull = CompatExtensions.UnwrapEnumerable<int>(source, includeNullValues: false).ToArray();
            Assert.That(intsNoNull, Is.EqualTo(new[] { 1, 2 }));

            var strings = CompatExtensions.UnwrapEnumerable<string>(source, includeNullValues: true).ToArray();
            Assert.That(strings, Is.EqualTo(new[] { "x", null }));
        }

        [Test]
        public void UnwrapIntoArray_Generic_FromEnumerable()
        {
            object source = new List<int> { 1, 2, 3 };
            var arr = source.UnwrapIntoArray<int>();
            Assert.That(arr, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void UnwrapIntoList_Generic_FromArray()
        {
            object source = new[] { 1, 2, 3 };
            var list = source.UnwrapIntoList<int>();
            Assert.That(list, Is.EqualTo(new List<int> { 1, 2, 3 }));
        }

        [Test]
        public void UnwrapIntoSet_Generic_FromEnumerable()
        {
            object source = new[] { 1, 2, 2, 3 };
            var set = source.UnwrapIntoSet<int>();
            Assert.That(set.OrderBy(x => x).ToArray(), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void UnwrapStringDictionary_FromDictionaryWithStringKeys()
        {
            object source = new Dictionary<string, object> {
                ["a"] = 1,
                ["b"] = "x"
            };

            var dict = source.UnwrapStringDictionary();
            Assert.That(dict["a"], Is.EqualTo(1));
            Assert.That(dict["b"], Is.EqualTo("x"));
        }

        [Test]
        public void AsStringDictionary_ThrowsWhenRequested()
        {
            object notADict = 10;
            Assert.Throws<ArgumentException>(() => notADict.AsStringDictionary(throwError: true));
        }

        [Test]
        public void SubList_IsViewAndReflectsParentMutations()
        {
            var parent = new List<int> { 1, 2, 3, 4, 5 };
            var sub = parent.SubList(1, 4); // [2,3,4]

            Assert.That(sub.Count, Is.EqualTo(3));
            Assert.That(sub.ToArray(), Is.EqualTo(new[] { 2, 3, 4 }));

            sub[0] = 20;
            Assert.That(parent[1], Is.EqualTo(20));

            sub.RemoveAt(1); // remove 3
            Assert.That(parent.ToArray(), Is.EqualTo(new[] { 1, 20, 4, 5 }));
            Assert.That(sub.ToArray(), Is.EqualTo(new[] { 20, 4 }));

            sub.Insert(1, 99);
            Assert.That(parent.ToArray(), Is.EqualTo(new[] { 1, 20, 99, 4, 5 }));
            Assert.That(sub.ToArray(), Is.EqualTo(new[] { 20, 99, 4 }));
        }

        [Test]
        public void UnwrapEnumerable_ThrowsForNonEnumerable()
        {
            Assert.Throws<ArgumentException>(() => CompatExtensions.UnwrapEnumerable<int>(123));
        }
    }
}
