using System;
using System.Linq;

using NUnit.Framework;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTreeDictionaryTests
    {
        public class KeyCollectionTests
        {
            // --------------------------------------------------------------------------------
            // Keys
            // --------------------------------------------------------------------------------

            [Test]
            public void KeysMustNotBeNull()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                Assert.That(btreeDictionary.Keys, Is.Not.Null);
                Assert.That(btreeDictionary.Keys.Count, Is.EqualTo(0));
            }

            [Test]
            public void KeysAreReadOnly()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                Assert.That(btreeDictionary.Keys, Is.Not.Null);
                Assert.That(btreeDictionary.Keys.IsReadOnly, Is.True);

                var btreeKeys = btreeDictionary.Keys;
                Assert.Throws<NotSupportedException>(() => btreeKeys.Add("invalid-key"));
                Assert.Throws<NotSupportedException>(() => btreeKeys.Remove("invalid-key"));
                Assert.Throws<NotSupportedException>(() => btreeKeys.Clear());
            }

            [Test]
            public void KeysContainsCheck()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                btreeDictionary["123"] = "234";
                Assert.That(btreeDictionary.Keys, Is.Not.Null);
                Assert.That(btreeDictionary.Keys.IsReadOnly, Is.True);
                Assert.That(btreeDictionary.Keys.Contains("123"), Is.True);
                Assert.That(btreeDictionary.Keys.Contains("234"), Is.False);
                Assert.That(btreeDictionary.Keys.Contains(null), Is.False);
            }

            [Test]
            public void KeysCanBeCopied()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                btreeDictionary["123"] = "234";
                btreeDictionary["234"] = "345";
                btreeDictionary["345"] = "456";
                btreeDictionary["456"] = "567";
                btreeDictionary["567"] = "678";
                btreeDictionary["678"] = "789";
                btreeDictionary["789"] = "890";
                btreeDictionary["890"] = "123";

                Assert.That(btreeDictionary.Keys, Is.Not.Null);

                var keys = btreeDictionary.Keys;

                // Copy into array matching size, index zero
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count];
                        keys.CopyTo(array, 0);
                    });

                // Copy into array with matching size, index negative
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count];
                        keys.CopyTo(array, -100);
                    });

                // Copy into array with matching size, valid but overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count];
                        keys.CopyTo(array, 4);
                    });

                // Copy into array with matching size, invalid, overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count];
                        keys.CopyTo(array, keys.Count);
                    });

                // ------------------------------------------------------------

                // Copy into array too small, index zero
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count / 2];
                        keys.CopyTo(array, 0);
                    });

                // Copy into array too small, index negative
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count / 2];
                        keys.CopyTo(array, -100);
                    });

                // Copy into array too small, valid but overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count / 2];
                        keys.CopyTo(array, 4);
                    });

                // Copy into array too small, invalid, overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count / 2];
                        keys.CopyTo(array, keys.Count);
                    });

                // ------------------------------------------------------------

                // Copy into array too large, index zero
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count * 2];
                        keys.CopyTo(array, 0);
                    });

                // Copy into array too small, index negative
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count * 2];
                        keys.CopyTo(array, -100);
                    });

                // Copy into array too small, valid but overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count * 2];
                        keys.CopyTo(array, 4);
                    });

                // Copy into array too small, invalid, overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[keys.Count * 2];
                        keys.CopyTo(array, keys.Count);
                    });
            }

            [Test]
            public void CanEnumerateKeys()
            {
                int numKeys = 65536;

                var btreeDictionary = CreateRandomDictionary(numKeys);
                Assert.That(btreeDictionary.Keys, Is.Not.Null);
                Assert.That(btreeDictionary.Keys.Count, Is.EqualTo(numKeys));

                var expected = Enumerable.Range(0, numKeys)
                    .Select(v => v.ToString("000000"))
                    .ToList();
                CollectionAssert.AreEqual(
                    expected,
                    btreeDictionary.Keys);
            }
        }
    }
}