using System;
using System.Linq;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.compat.collections.btree
{
    public partial class BTreeDictionaryTests
    {
        public class ValueCollectionTests
        {
            // --------------------------------------------------------------------------------
            // Values
            // --------------------------------------------------------------------------------

            [Test]
            public void ValuesMustNotBeNull()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                Assert.That(btreeDictionary.Values, Is.Not.Null);
                Assert.That(btreeDictionary.Values.Count, Is.EqualTo(0));
            }

            [Test]
            public void ValuesAreReadOnly()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                Assert.That(btreeDictionary.Values, Is.Not.Null);
                Assert.That(btreeDictionary.Values.IsReadOnly, Is.True);

                var btreeValues = btreeDictionary.Values;
                Assert.Throws<NotSupportedException>(() => btreeValues.Add("invalid-value"));
                Assert.Throws<NotSupportedException>(() => btreeValues.Remove("invalid-value"));
                Assert.Throws<NotSupportedException>(() => btreeValues.Clear());
            }

            [Test]
            public void ValuesContainsCheck()
            {
                var btreeDictionary = new BTreeDictionary<string, string>();
                btreeDictionary["123"] = "234";
                Assert.That(btreeDictionary.Values, Is.Not.Null);
                Assert.That(btreeDictionary.Values.IsReadOnly, Is.True);
                Assert.That(btreeDictionary.Values.Contains("123"), Is.False);
                Assert.That(btreeDictionary.Values.Contains("234"), Is.True);
                Assert.That(btreeDictionary.Values.Contains(null), Is.False);
            }

            [Test]
            public void ValuesCanBeCopied()
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

                Assert.That(btreeDictionary.Values, Is.Not.Null);

                var values = btreeDictionary.Values;

                // Copy into array matching size, index zero
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count];
                        values.CopyTo(array, 0);
                    });

                // Copy into array with matching size, index negative
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count];
                        values.CopyTo(array, -100);
                    });

                // Copy into array with matching size, valid but overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count];
                        values.CopyTo(array, 4);
                    });

                // Copy into array with matching size, invalid, overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count];
                        values.CopyTo(array, values.Count);
                    });

                // ------------------------------------------------------------

                // Copy into array too small, index zero
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count / 2];
                        values.CopyTo(array, 0);
                    });

                // Copy into array too small, index negative
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count / 2];
                        values.CopyTo(array, -100);
                    });

                // Copy into array too small, valid but overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count / 2];
                        values.CopyTo(array, 4);
                    });

                // Copy into array too small, invalid, overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count / 2];
                        values.CopyTo(array, values.Count);
                    });

                // ------------------------------------------------------------

                // Copy into array too large, index zero
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count * 2];
                        values.CopyTo(array, 0);
                    });

                // Copy into array too small, index negative
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count * 2];
                        values.CopyTo(array, -100);
                    });

                // Copy into array too small, valid but overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count * 2];
                        values.CopyTo(array, 4);
                    });

                // Copy into array too small, invalid, overflow
                Assert.DoesNotThrow(
                    () => {
                        string[] array = new string[values.Count * 2];
                        values.CopyTo(array, values.Count);
                    });
            }

            [Test]
            public void CanEnumerateValues()
            {
                int numKeys = 65536;

                var btreeDictionary = CreateFixedDictionary(numKeys);
                Assert.That(btreeDictionary.Values, Is.Not.Null);
                Assert.That(btreeDictionary.Values.Count, Is.EqualTo(numKeys));

                var expected = Enumerable.Range(0, numKeys)
                    .Select(v => v.ToString("X6"))
                    .ToList();
                CollectionAssert.AreEqual(
                    expected,
                    btreeDictionary.Values);
            }
        }
    }
}