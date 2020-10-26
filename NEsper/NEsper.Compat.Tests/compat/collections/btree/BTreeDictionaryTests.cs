using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections.btree
{
    [TestFixture]
    public partial class BTreeDictionaryTests
    {
        [Test]
        public void ConstructWithDefaultComparer()
        {
            BTreeDictionary<string, string> dictionary = null;
            Assert.DoesNotThrow(() => dictionary = new BTreeDictionary<string, string>());
            Assert.That(dictionary.Underlying, Is.Not.Null);
            Assert.That(dictionary.KeyComparer, Is.Not.Null);
            Assert.That(dictionary.KeyComparer, Is.EqualTo(Comparer<string>.Default));
            Assert.That(dictionary.IsReadOnly, Is.False);
        }

        [Test]
        public void ConstructWithCustomComparer()
        {
            var myComparer = new MyComparer();
            BTreeDictionary<string, string> dictionary = null;
            Assert.DoesNotThrow(() => dictionary = new BTreeDictionary<string, string>(myComparer));
            Assert.That(dictionary.Underlying, Is.Not.Null);
            Assert.That(dictionary.KeyComparer, Is.Not.Null);
            Assert.That(dictionary.KeyComparer, Is.SameAs(myComparer));
            Assert.That(dictionary.IsReadOnly, Is.False);
        }

        // --------------------------------------------------------------------------------
        // Contains & ContainsKey
        // --------------------------------------------------------------------------------

        [Test]
        public void ContainsByKeyForValid()
        {
            var numKeys = 1024;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = ii.ToString("000000");
                Assert.That(dictionary.ContainsKey(strKey), Is.True);
            }
        }

        [Test]
        public void ContainsByKeyForInvalid()
        {
            var numKeys = 1024;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = Guid.NewGuid().ToString();
                Assert.That(dictionary.ContainsKey(strKey), Is.False);
            }
        }
        
        [Test]
        public void ContainsByKeyAndValue()
        {
            var numKeys = 1024;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = ii.ToString("000000");
                var strVal = ii.ToString("X6");
                var tstVal = Guid.NewGuid().ToString();
                var badKey = (ii + numKeys).ToString("000000");

                Assert.That(
                    dictionary.Contains(new KeyValuePair<string, string>(strKey, strVal)),
                    Is.True);
                Assert.That(
                    dictionary.Contains(new KeyValuePair<string, string>(strKey, tstVal)),
                    Is.False);
                Assert.That(
                    dictionary.Contains(new KeyValuePair<string, string>(badKey, strVal)),
                    Is.False);
            }
        }

        // --------------------------------------------------------------------------------
        // Getters & Setters
        // --------------------------------------------------------------------------------

        [Test]
        public void CanGetByKey()
        {
            var numKeys = 1024;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = ii.ToString("000000");
                var strVal = ii.ToString("X6");
                var tstVal = dictionary[strKey];
                Assert.That(tstVal, Is.Not.Null);
                Assert.That(tstVal, Is.EqualTo(strVal));
            }

            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = (ii + numKeys).ToString("000000");
                Assert.Throws<KeyNotFoundException>(() => {
                    DoNothing(dictionary[strKey]);
                });
            }
        }

        [Test]
        public void CanTryGetByKey()
        {
            var numKeys = 1024;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = ii.ToString("000000");
                var strVal = ii.ToString("X6");
                Assert.That(dictionary.TryGetValue(strKey, out var tstVal), Is.True);
                Assert.That(tstVal, Is.Not.Null);
                Assert.That(tstVal, Is.EqualTo(strVal));
            }

            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = (ii + numKeys).ToString("000000");
                Assert.That(dictionary.TryGetValue(strKey, out var tstVal), Is.False);
            }
        }

        [Test]
        public void CanUpdateValues()
        {
            var numKeys = 1024;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var strKey = ii.ToString("000000");
                var strVal = ii.ToString("X6");
                var rndVal = Guid.NewGuid().ToString();
                Assert.That(dictionary.TryGetValue(strKey, out var tstVal), Is.True);
                Assert.That(tstVal, Is.EqualTo(strVal));
                dictionary[strKey] = rndVal;
                Assert.That(dictionary.TryGetValue(strKey, out tstVal), Is.True);
                Assert.That(tstVal, Is.EqualTo(rndVal));
            }
        }

        // --------------------------------------------------------------------------------
        // Insert
        // --------------------------------------------------------------------------------

        [Test]
        public void InsertMustKeepCorrectCountAndHeight()
        {
            var random = new Random(1000); // consistent seed for testing
            var dictionary = new BTreeDictionary<string, string>();
            for (int ii = 0; ii < 10000; ii++) {
                var key = ii.ToString("0000");
                var val = random.Next().ToString();
                dictionary[key] = val;
                // Expectations
                var expectedCount = ii + 1;
                var expectedHeight = 1 + (int) Math.Floor(Math.Log(expectedCount, 4));
                Assert.That(dictionary.Count, Is.EqualTo(expectedCount));
                Assert.That(dictionary.Height, Is.EqualTo(expectedHeight));
            }
        }

        [Test]
        public void AddByKeyValuePair()
        {
            var numKeys = 10000;
            var dictionary = new BTreeDictionary<string, string>();
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = ii.ToString("X6");
                var kvp = new KeyValuePair<string, string>(key, val);
                dictionary.Add(kvp);
            }

            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = Guid.NewGuid().ToString();
                var kvp = new KeyValuePair<string, string>(key, val);
                Assert.Throws<ArgumentException>(() => dictionary.Add(kvp));
            }
        }

        [Test]
        public void AddByKeyAndValue()
        {
            var numKeys = 10000;
            var dictionary = new BTreeDictionary<string, string>();
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = ii.ToString("X6");
                dictionary.Add(key, val);
            }

            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = Guid.NewGuid().ToString();
                Assert.Throws<ArgumentException>(() => dictionary.Add(key, val));
            }
        }
        
        // --------------------------------------------------------------------------------
        // Clear & Remove
        // --------------------------------------------------------------------------------

        [Test]
        public void CanClear()
        {
            var dictionary = CreateRandomDictionary(1024);
            Assert.That(dictionary.IsEmpty, Is.False);
            
            dictionary.Clear();
            Assert.That(dictionary.IsEmpty, Is.True);
            Assert.That(dictionary.Count, Is.Zero);
        }

        [Test]
        public void RemoveMustKeepCorrectCount()
        {
            var dictionary = CreateRandomDictionary(1024);

            int expectedCount = dictionary.Count;

            for (int ii = 0; ii < 512; ii++) {
                var key = ii.ToString("000000");
                Assert.That(dictionary.Remove(key), Is.True);
                
                // Expectations
                // Count should decrease each time
                expectedCount--;
                Assert.That(dictionary.Count, Is.EqualTo(expectedCount));
                
                // Height depends on which nodes are deleted and shrinkage.  Our test deletes in order which
                //    should leave the tree with one leaf node per parent. Once a leaf node with one parent
                //    is deleted, the tree must shrink.
                //
                // Example:
                //                    R
                //           +-----+-----+-----+
                //           I     I     I     I
                //           |     |     |     |
                //           L     L     L     L
                //
                // At this point, the tree height must shrink.
            }
        }
            
        [Test]
        public void RemoveByKeyValuePair()
        {
            var numKeys = 10000;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = Guid.NewGuid().ToString();
                var kvp = new KeyValuePair<string, string>(key, val);
                Assert.That(dictionary.Remove(kvp), Is.False);
            }

            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = ii.ToString("X6");
                var kvp = new KeyValuePair<string, string>(key, val);
                Assert.That(dictionary.Remove(kvp), Is.True);
            }
        }

        [Test]
        public void RemoveByKey()
        {
            var numKeys = 10000;
            var dictionary = CreateFixedDictionary(numKeys);
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                Assert.That(dictionary.Remove(key), Is.True);
            }

            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                Assert.That(dictionary.Remove(key), Is.False);
            }
        }

        // --------------------------------------------------------------------------------
        // Enumerator
        // --------------------------------------------------------------------------------

        [Test]
        public void CanEnumerateInOrder()
        {
            var numKeys = 10000;
            var dictionary = CreateFixedDictionary(numKeys);
            var enumerator = dictionary.GetEnumerator();
            for (int ii = 0; ii < numKeys; ii++) {
                Assert.That(enumerator.MoveNext(), Is.True);
                var kvp = enumerator.Current;
                var strKey = ii.ToString("000000");
                var strVal = ii.ToString("X6");
                Assert.That(kvp.Key, Is.EqualTo(strKey));
                Assert.That(kvp.Value, Is.EqualTo(strVal));
            }

            Assert.That(enumerator.MoveNext(), Is.False);
        }

        [Test]
        public void CanEnumerateGenericInOrder()
        {
            var numKeys = 10000;
            var dictionary = CreateFixedDictionary(numKeys);
            var enumerator = ((IEnumerable) dictionary).GetEnumerator();
            for (int ii = 0; ii < numKeys; ii++) {
                Assert.That(enumerator.MoveNext(), Is.True);
                
                var kvo = enumerator.Current;
                Assert.That(kvo, Is.Not.Null);
                Assert.That(kvo, Is.InstanceOf<KeyValuePair<string, string>>());

                var kvp = (KeyValuePair<string, string>) kvo;
                var strKey = ii.ToString("000000");
                var strVal = ii.ToString("X6");
                Assert.That(kvp.Key, Is.EqualTo(strKey));
                Assert.That(kvp.Value, Is.EqualTo(strVal));
            }

            Assert.That(enumerator.MoveNext(), Is.False);
        }

        // --------------------------------------------------------------------------------
        // Utilities
        // --------------------------------------------------------------------------------

        private static BTreeDictionary<string, string> CreateRandomDictionary(int numKeys, Random random = null)
        {
            if (random == null) {
                random = new Random(1000); // consistent seed for testing
            }

            var dictionary = new BTreeDictionary<string, string>();
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = random.Next().ToString();
                dictionary[key] = val;
            }

            return dictionary;
        }

        private static BTreeDictionary<string, string> CreateFixedDictionary(int numKeys)
        {
            var dictionary = new BTreeDictionary<string, string>();
            for (int ii = 0; ii < numKeys; ii++) {
                var key = ii.ToString("000000");
                var val = ii.ToString("X6");
                dictionary[key] = val;
            }

            return dictionary;
        }

        public static void DoNothing<T>(T anyValue)
        {
        }
        
        internal class MyComparer : IComparer<string>
        {
            public int Compare(
                string x,
                string y)
            {
                throw new NotImplementedException();
            }
        }
    }
}