using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections.btree
{
    [TestFixture]
    public class BTreeTests
    {
        // --------------------------------------------------------------------------------
        // Constructors
        // --------------------------------------------------------------------------------

        [Test, RunInApplicationDomain]
        public void ConstructorWithValidSettings()
        {
            var accessor = new Func<string, string>(_ => _);
            var btree = new BTree<string, string>(accessor, Comparer<string>.Default, 3);
            Assert.That(btree.Count, Is.Zero);
            Assert.That(btree.KeyComparer, Is.EqualTo(Comparer<string>.Default));
            Assert.That(btree.KeyAccessor, Is.SameAs(accessor));
        }

        [Test, RunInApplicationDomain]
        public void ConstructorRequiresNonNullValues()
        {
            Assert.Throws<ArgumentNullException>(() => new BTree<string, string>(null, Comparer<string>.Default));
            Assert.Throws<ArgumentNullException>(() => new BTree<string, string>(_ => _, null));
        }

        // --------------------------------------------------------------------------------
        // Insert
        // --------------------------------------------------------------------------------

        [Test, RunInApplicationDomain]
        public void InsertUniqueNoRepeat()
        {
            var btree = new BTree<string, string>(_ => _, Comparer<string>.Default);
            foreach (var intValue in Enumerable.Range(0, 65536)) {
                var strValue = intValue.ToString("000000");
                var result = btree.InsertUnique(strValue);
                Assert.That(result.Succeeded, Is.True);
                Assert.That(result.Cursor, Is.Not.Null);
                Assert.That(result.Cursor.Key, Is.EqualTo(strValue));
                Assert.That(result.Cursor.Value, Is.EqualTo(strValue));
            }
        }

        [Test, RunInApplicationDomain]
        public void InsertUniqueWithRepeat()
        {
            var btree = new BTree<string, string>(_ => _, Comparer<string>.Default);

            // Seeding the tree
            foreach (var intValue in Enumerable.Range(0, 65536)) {
                var strValue = (intValue % 65536).ToString("000000");
                var result = btree.InsertUnique(strValue);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Succeeded, Is.True);
            }

            foreach (var intValue in Enumerable.Range(0, 65536)) {
                var strValue = (intValue % 65536).ToString("000000");
                var result = btree.InsertUnique(strValue);
                Assert.That(result.Succeeded, Is.False);
                Assert.That(result.Cursor, Is.Not.Null);
                Assert.That(result.Cursor.Key, Is.EqualTo(strValue));
                Assert.That(result.Cursor.Value, Is.EqualTo(strValue));
            }
        }

        // --------------------------------------------------------------------------------
        // Erase
        // --------------------------------------------------------------------------------

        [Test, RunInApplicationDomain]
        public void TryEraseSucceedsForExistingKeys()
        {
            var testKey = "000100";
            var btree = CreateTree(65536);
            Assert.That(btree.ContainsKey(testKey), Is.True);
            Assert.That(btree.TryEraseUnique(testKey, out var existingValue), Is.True);
            Assert.That(existingValue, Is.EqualTo(testKey));
        }

        [Test, RunInApplicationDomain]
        public void TryEraseAllKeys()
        {
            var numKeys = 65536;
            var btree = CreateTree(numKeys);
            for (int intValue = 0; intValue < numKeys; intValue++) {
                var strValue = intValue.ToString("000000");
                Assert.That(btree.ContainsKey(strValue), Is.True);
                Assert.That(btree.TryEraseUnique(strValue, out var existingValue), Is.True);
                Assert.That(existingValue, Is.EqualTo(strValue));
            }

            Assert.That(btree.Count, Is.Zero);
        }

        [Test, RunInApplicationDomain]
        public void TryEraseInsideOut()
        {
            var numKeys = 0x10000;
            var midKey = 0x08000;
            var loKey = 0x04000;
            var hiKey = 0x0C000;

            var btree = CreateTree(numKeys);
            
            // Deleting keys inside out forces a left-to-right rebalance
            
            for (int intValue = midKey; intValue <= hiKey; intValue++) {
                var strValue = intValue.ToString("000000");
                Assert.That(btree.ContainsKey(strValue), Is.True);
                Assert.That(btree.TryEraseUnique(strValue, out var existingValue), Is.True);
                Assert.That(existingValue, Is.EqualTo(strValue));
            }

            for (int intValue = midKey - 1; intValue >= loKey; intValue--) {
                var strValue = intValue.ToString("000000");
                Assert.That(btree.ContainsKey(strValue), Is.True);
                Assert.That(btree.TryEraseUnique(strValue, out var existingValue), Is.True);
                Assert.That(existingValue, Is.EqualTo(strValue));
            }
        }

        [Test, RunInApplicationDomain]
        public void TryEraseInRounds()
        {
            var rounds = 7;
            var numKeys = 65536;
            var btree = CreateTree(numKeys);

            for (int round = 0; round < rounds; round++) {
                for (int intValue = round; intValue < numKeys; intValue += rounds) {
                    var strValue = intValue.ToString("000000");
                    Assert.That(btree.ContainsKey(strValue), Is.True);
                    Assert.That(btree.TryEraseUnique(strValue, out var existingValue), Is.True);
                    Assert.That(existingValue, Is.EqualTo(strValue));
                }
            }
        }

        [Test, RunInApplicationDomain]
        public void TryEraseRandom()
        {
            var numKeys = 65536;
            var numSet = Enumerable.Range(0, numKeys).ToList();
            var btree = CreateTree(numKeys);
            var rand = new Random();

            for (int ii = 0 ; ii < numKeys ; ii++) {
                var index = rand.Next(0, numSet.Count);
                var intValue = numSet[index];
                numSet.DeleteAt(index);
                var strValue = intValue.ToString("000000");
                Assert.That(btree.ContainsKey(strValue), Is.True);
                Assert.That(btree.TryEraseUnique(strValue, out var existingValue), Is.True);
                Assert.That(existingValue, Is.EqualTo(strValue));
            }

            Assert.That(btree.IsEmpty);
        }

        // --------------------------------------------------------------------------------
        // Locate
        // --------------------------------------------------------------------------------

        [Test, RunInApplicationDomain]
        public void ContainsWhenInTree()
        {
            var btree = CreateKVTree(65536);
            // Every key between 0 and 65536 should exist
            foreach (var intValue in Enumerable.Range(0, 65536)) {
                var strValue = intValue.ToString("000000");
                Assert.That(btree.ContainsKey(strValue), Is.True);
                Assert.That(btree.ContainsValue(new KeyValuePair<string, int>(strValue, intValue)), Is.True);
                Assert.That(btree.ContainsValue(new KeyValuePair<string, int>(strValue, intValue - 1)), Is.False);
            }
        }

        [Test, RunInApplicationDomain]
        public void ContainsWhenNotInTree()
        {
            var btree = CreateKVTree(65536);
            foreach (var intValue in Enumerable.Range(65536, 65536 * 2)) {
                var strValue = intValue.ToString("000000");
                Assert.That(btree.ContainsKey(strValue), Is.False);
                Assert.That(btree.ContainsValue(new KeyValuePair<string, int>(strValue, intValue)), Is.False);
            }
        }

        // --------------------------------------------------------------------------------
        // Cursor
        // --------------------------------------------------------------------------------

        [Test, RunInApplicationDomain]
        public void HasRootCursor()
        {
            var btree = CreateKVTree(0);
            var cursor = btree.RootCursor;
            Assert.That(cursor, Is.Not.Null);
            Assert.That(cursor.IsEnd, Is.True);
        }
        
        [Test, RunInApplicationDomain]
        public void HasBeginCursor()
        {
            var btree = CreateKVTree(1);
            var cursor = btree.Begin();
            Assert.That(cursor, Is.Not.Null);
            Assert.That(cursor.IsEnd, Is.False);
            Assert.That(cursor.IsNotEnd, Is.True);
        }

        [Test, RunInApplicationDomain]
        public void HasEndCursor()
        {
            var btree = CreateKVTree(1);
            var cursor = btree.End();
            Assert.That(cursor, Is.Not.Null);
            Assert.That(cursor.IsEnd, Is.True);
            Assert.That(cursor.IsNotEnd, Is.False);
        }

        [Test, RunInApplicationDomain]
        public void CursorVisitsAllInOrder()
        {
            var btree = CreateKVTree(65536);
            var cursor = btree.Begin();
            for (int intValue = 0 ; intValue < 65536 ; intValue++) {
                var strValue = intValue.ToString("000000");
                Assert.That(cursor.IsEnd, Is.False);
                Assert.That(cursor.IsNotEnd, Is.True);
                Assert.That(cursor.Key, Is.EqualTo(strValue));
                Assert.That(cursor.Value, Is.EqualTo(new KeyValuePair<string, int>(strValue, intValue)));
                cursor.MoveNext();
            }
        }
        
        [Test, RunInApplicationDomain]
        public void CursorVisitsAllInReverse()
        {
            var btree = CreateKVTree(65536);
            var cursor = btree.End();
            Assert.That(cursor.IsEnd, Is.True);
            Assert.That(cursor.IsNotEnd, Is.False);
            cursor.MovePrevious();
            for (int intValue = 65535 ; intValue >= 0 ; intValue--) {
                var strValue = intValue.ToString("000000");
                Assert.That(cursor.IsEnd, Is.False);
                Assert.That(cursor.IsNotEnd, Is.True);
                Assert.That(cursor.Key, Is.EqualTo(strValue));
                Assert.That(cursor.Value, Is.EqualTo(new KeyValuePair<string, int>(strValue, intValue)));
                cursor.MovePrevious();
            }
        }
        
        // --------------------------------------------------------------------------------
        // Dump
        // --------------------------------------------------------------------------------

        [Test, RunInApplicationDomain]
        public void DumpExportsAllKeys()
        {
            var numKeys = 256;
            var btree = CreateTree(numKeys);
            var btreeDump = btree.Dump().TrimEnd();
            var btreeDumpByLine = btreeDump.Split('\n');
            Assert.That(btreeDumpByLine.Length, Is.EqualTo(numKeys));
            
        }
        
        // --------------------------------------------------------------------------------
        // Utilities
        // --------------------------------------------------------------------------------

        private static BTree<string, KeyValuePair<string, int>> CreateKVTree(int numKeys, Random random = null)
        {
            if (random == null) {
                random = new Random(1000); // consistent seed for testing
            }

            var btree = new BTree<string, KeyValuePair<string, int>>(_ => _.Key, Comparer<string>.Default, 3);
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var keyValue = new KeyValuePair<string, int>(key, ii);
                btree.InsertUnique(keyValue);
            }

            return btree;
        }

        private static BTree<string, string> CreateTree(int numKeys, Random random = null)
        {
            if (random == null) {
                random = new Random(1000); // consistent seed for testing
            }

            var btree = new BTree<string, string>(_ => _, Comparer<string>.Default, 3);
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                btree.InsertUnique(key);
            }

            return btree;
        }

        internal class KVComparer : IComparer<KeyValuePair<string, int>>
        {
            internal static readonly KVComparer Default = new KVComparer();

            public int Compare(
                KeyValuePair<string, int> x,
                KeyValuePair<string, int> y)
            {
                return String.Compare(x.Key, y.Key, StringComparison.Ordinal);
            }
        }
    }
}