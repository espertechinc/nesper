///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.util.CollectionUtil;

namespace com.espertech.esper.common.@internal.util
{
    [TestFixture]
    public class TestCollectionUtil : AbstractCommonTest
    {
        private void RunAssertionShrink(
            string expected,
            string existing,
            int index)
        {
            var expectedArr = expected.Length == 0 ? new string[0] : expected.SplitCsv();
            var existingArr = existing.Length == 0 ? new string[0] : existing.SplitCsv();
            var resultAddColl = (string[]) CollectionUtil.ArrayShrinkRemoveSingle(existingArr, index);
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, resultAddColl);
        }

        private void RunAssertionExpandColl(
            string expected,
            string existing,
            string coll)
        {
            var expectedArr = expected.Length == 0 ? new string[0] : expected.SplitCsv();
            var existingArr = existing.Length == 0 ? new string[0] : existing.SplitCsv();
            ICollection<string> addCollection = Arrays.AsList(coll.Length == 0 ? new string[0] : coll.SplitCsv());
            var resultAddColl = CollectionUtil.ArrayExpandAddElements(existingArr, addCollection);
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, resultAddColl);

            var resultAddArr = CollectionUtil.ArrayExpandAddElements(existingArr, addCollection.Unwrap<object>())
                .Unwrap<string>();
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, resultAddArr);
        }

        private void RunAssertionExpandSingle(
            string expected,
            string existing,
            string single)
        {
            var expectedArr = expected.Length == 0 ? new string[0] : expected.SplitCsv();
            var existingArr = existing.Length == 0 ? new string[0] : existing.SplitCsv();
            var result = (string[]) CollectionUtil.ArrayExpandAddSingle(existingArr, single);
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, result);
        }

        private void RunAssertionSubdivide3(String csv, String expected) {
            RunAssertionSubdivide(csv, expected, 3);
        }

        private void RunAssertionSubdivide(String csv, String expected, int size) {
            IList<String> input = new List<String>(csv.SplitCsv());
            IList<IList<String>> lists = CollectionUtil.Subdivide(input, size);

            StringBuilder stringBuilder = new StringBuilder();
            String delimiter = "";
            foreach (var list in lists) {
                String items = String.Join(",", list.ToArray());
                    stringBuilder.Append(delimiter);
                    stringBuilder.Append(items);
                delimiter = "|";
            }

            ClassicAssert.AreEqual(expected, stringBuilder.ToString());
        }
        
        private void TryAddStringArr(
            string[] expected,
            object result)
        {
            ClassicAssert.IsTrue(result.GetType().IsArray);
            ClassicAssert.AreEqual(typeof(string), result.GetType().GetElementType());
            EPAssertionUtil.AssertEqualsExactOrder(expected, (string[]) result);
        }

        private ISet<string> ToSet(string[] arr)
        {
            if (arr == null)
            {
                return null;
            }

            if (arr.Length == 0)
            {
                return new HashSet<string>();
            }

            ISet<string> set = new LinkedHashSet<string>();
            foreach (var a in arr)
            {
                set.Add(a);
            }

            return set;
        }

        [Test]
        public void TestArrayAllNull()
        {
            ClassicAssert.IsTrue(IsArrayAllNull(null));
            ClassicAssert.IsTrue(IsArrayAllNull(new Object[0]));
            ClassicAssert.IsTrue(IsArrayAllNull(new Object[] {null}));
            ClassicAssert.IsTrue(IsArrayAllNull(new Object[] {null, null}));

            ClassicAssert.IsFalse(IsArrayAllNull(new Object[] {"a", null}));
            ClassicAssert.IsFalse(IsArrayAllNull(new Object[] {null, "b"}));
        }

        [Test]
        public void TestArraySameReferences()
        {
            String a = "a";
            String b = "b";

            ClassicAssert.IsTrue(IsArraySameReferences(new Object[0], new Object[0]));
            ClassicAssert.IsTrue(IsArraySameReferences(new Object[] {a}, new Object[] {a}));
            ClassicAssert.IsTrue(IsArraySameReferences(new Object[] {a, b}, new Object[] {a, b}));

            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] { }, new Object[] {b}));
            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] {a}, new Object[] { }));
            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] {a}, new Object[] {b}));
            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] {a}, new Object[] {b, a}));
            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] {a, b}, new Object[] {a}));
            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] {a, b}, new Object[] {b, a}));
            ClassicAssert.IsFalse(IsArraySameReferences(new Object[] {new String(new char[] {'a'})}, new Object[] {new String(new char[] {'a'})}));
        }

        [Test]
        public void TestGetMapValueChecked()
        {
            ClassicAssert.IsNull(GetMapValueChecked(null, "x"));
            ClassicAssert.IsNull(GetMapValueChecked("b", "x"));
            ClassicAssert.IsNull(GetMapValueChecked(EmptyDictionary<string, object>.Instance, "x"));
            ClassicAssert.AreEqual("y", GetMapValueChecked(Collections.SingletonDataMap("x", "y"), "x"));
        }

        [Test]
        public void TestGetMapKeyExistsChecked()
        {
            ClassicAssert.IsFalse(GetMapKeyExistsChecked(null, "x"));
            ClassicAssert.IsFalse(GetMapKeyExistsChecked("b", "x"));
            ClassicAssert.IsFalse(GetMapKeyExistsChecked(EmptyDictionary<string, object>.Instance, "x"));
            ClassicAssert.IsTrue(GetMapKeyExistsChecked(Collections.SingletonDataMap("x", "y"), "x"));
        }

        [Test]
        public void TestSubdivide()
        {
            RunAssertionSubdivide3("", "");
            RunAssertionSubdivide3("a", "a");
            RunAssertionSubdivide3("a,b", "a,b");
            RunAssertionSubdivide3("a,b,c", "a,b,c");
            RunAssertionSubdivide3("a,b,c,d", "a,b,c|d");
            RunAssertionSubdivide3("a,b,c,d,e", "a,b,c|d,e");
            RunAssertionSubdivide3("a,b,c,d,e,f", "a,b,c|d,e,f");
            RunAssertionSubdivide3("a,b,c,d,e,f,g", "a,b,c|d,e,f|g");
            RunAssertionSubdivide3("a,b,c,d,e,f,g,h", "a,b,c|d,e,f|g,h");
            RunAssertionSubdivide3("a,b,c,d,e,f,g,h,i", "a,b,c|d,e,f|g,h,i");
            RunAssertionSubdivide3("a,b,c,d,e,f,g,h,i,j", "a,b,c|d,e,f|g,h,i|j");

            RunAssertionSubdivide("", "", 2);
            RunAssertionSubdivide("a", "a", 2);
            RunAssertionSubdivide("a,b", "a,b", 2);
            RunAssertionSubdivide("a,b,c", "a,b|c", 2);
            RunAssertionSubdivide("a,b,c,d", "a,b|c,d", 2);
            RunAssertionSubdivide("a,b,c,d,e", "a,b|c,d|e", 2);

            RunAssertionSubdivide("", "", 1);
            RunAssertionSubdivide("a", "a", 1);
            RunAssertionSubdivide("a,b", "a|b", 1);
            RunAssertionSubdivide("a,b,c", "a|b|c", 1);
        }

        [Test]
        public void TestAddArray()
        {
            TryAddStringArr(new [] { "b","a" }, CollectionUtil.AddArrays(new[] { "b" }, new[] { "a" }));
            TryAddStringArr(new [] { "a" }, CollectionUtil.AddArrays(null, new[] { "a" }));
            TryAddStringArr(new [] { "b" }, CollectionUtil.AddArrays(new[] { "b" }, null));
            TryAddStringArr(new [] { "a","b","c","d" }, CollectionUtil.AddArrays(new[] { "a", "b" }, new[] { "c", "d" }));
            ClassicAssert.AreEqual(null, CollectionUtil.AddArrays(null, null));

            var result = CollectionUtil.AddArrays(new[] { 1, 2 }, new[] { 3, 4 });
            EPAssertionUtil.AssertEqualsExactOrder(new[] { 1, 2, 3, 4 }, (int[]) result);

            try
            {
                CollectionUtil.AddArrays("a", null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                ClassicAssert.AreEqual("Parameter is not an array: a", ex.Message);
            }

            try
            {
                CollectionUtil.AddArrays(null, "b");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                ClassicAssert.AreEqual("Parameter is not an array: b", ex.Message);
            }
        }

        [Test]
        public void TestAddArraySetSemantics()
        {
            var e = new EventBean[10];
            for (var i = 0; i < e.Length; i++)
            {
                e[i] = new MapEventBean(null);
            }

            ClassicAssert.IsFalse(e[0].Equals(e[1]));

            object[][] testData = {
                new object[] {new EventBean[] { }, new EventBean[] { }, "P2"},
                new object[] {new EventBean[] { }, new[] {e[0], e[1]}, "P2"},
                new object[] {new[] {e[0]}, new EventBean[] { }, "P1"},
                new object[] {new[] {e[0]}, new[] {e[0]}, "P1"},
                new object[] {new[] {e[0]}, new[] {e[1]}, new[] {e[0], e[1]}},
                new object[] {new[] {e[0], e[1]}, new[] {e[1]}, "P1"},
                new object[] {new[] {e[0], e[1]}, new[] {e[0]}, "P1"},
                new object[] {new[] {e[0]}, new[] {e[0], e[1]}, "P2"},
                new object[] {new[] {e[1]}, new[] {e[0], e[1]}, "P2"},
                new object[] {new[] {e[2]}, new[] {e[0], e[1]}, new[] {e[0], e[1], e[2]}},
                new object[] {new[] {e[2], e[0]}, new[] {e[0], e[1]}, new[] {e[0], e[1], e[2]}},
                new object[] {new[] {e[2], e[0]}, new[] {e[0], e[1], e[2]}, new[] {e[0], e[1], e[2]}}
            };

            for (var i = 0; i < testData.Length; i++)
            {
                var p1 = (EventBean[]) testData[i][0];
                var p2 = (EventBean[]) testData[i][1];
                var expectedObj = testData[i][2];

                object result = CollectionUtil.AddArrayWithSetSemantics(p1, p2);

                if (expectedObj.Equals("P1"))
                {
                    ClassicAssert.IsTrue(result == p1);
                }
                else if (expectedObj.Equals("P2"))
                {
                    ClassicAssert.IsTrue(result == p2);
                }
                else
                {
                    var resultArray = (EventBean[]) result;
                    var expectedArray = (EventBean[]) result;
                    EPAssertionUtil.AssertEqualsAnyOrder(resultArray, expectedArray);
                }
            }
        }

        [Test]
        public void TestArrayExpandCollectionAndArray()
        {
            RunAssertionExpandColl("", "", "");
            RunAssertionExpandColl("a,b", "a", "b");
            RunAssertionExpandColl("a,b", "", "a,b");
            RunAssertionExpandColl("b", "", "b");
            RunAssertionExpandColl("a,b,c", "a,b", "c");
            RunAssertionExpandColl("a,b,c", "", "a,b,c");
            RunAssertionExpandColl("a,b,c", "a", "b,c");
            RunAssertionExpandColl("a,b,c,d", "a,b,c", "d");
        }

        [Test]
        public void TestArrayExpandSingle()
        {
            RunAssertionExpandSingle("a", "", "a");
            RunAssertionExpandSingle("a,b", "a", "b");
            RunAssertionExpandSingle("a,b,c", "a,b", "c");
            RunAssertionExpandSingle("a,b,c,d", "a,b,c", "d");
        }

        [Test]
        public void TestArrayShrink()
        {
            RunAssertionShrink("a,c", "a,b,c", 1);
            RunAssertionShrink("b,c", "a,b,c", 0);
            RunAssertionShrink("a,b", "a,b,c", 2);
            RunAssertionShrink("a", "a,b", 1);
            RunAssertionShrink("b", "a,b", 0);
            RunAssertionShrink("", "a", 0);
        }

        [Test]
        public void TestCompare()
        {
            object[][] testdata = {
                new object[] {new[] {"a", "b"}, new[] {"a", "b"}, true},
                new object[] {new[] {"a"}, new[] {"a", "b"}, false},
                new object[] {new[] {"a"}, new[] {"a"}, true},
                new object[] {new[] {"b"}, new[] {"a"}, false},
                new object[] {new[] {"b", "a"}, new[] {"a", "b"}, true},
                new object[] {new[] {"a", "b", "b"}, new[] {"a", "b"}, false},
                new object[] {new[] {"a", "b", "b"}, new[] {"b", "a", "b"}, true},
                new object[] {new string[0], new string[0], true}
            };

            for (var i = 0; i < testdata.Length; i++)
            {
                var left = (string[]) testdata[i][0];
                var right = (string[]) testdata[i][1];
                var expected = (bool) testdata[i][2];
                ClassicAssert.AreEqual(expected, CollectionUtil.SortCompare(left, right), "Failed for input " + left.RenderAny());
                ClassicAssert.IsTrue(Equals(left, (string[]) testdata[i][0]));
                ClassicAssert.IsTrue(Equals(right, (string[]) testdata[i][1]));
            }
        }

        [Test]
        public void TestCopySort()
        {
            object[][] testdata = {
                new object[] {new[] {"a", "b"}, new[] {"a", "b"}},
                new object[] {new[] {"b", "a"}, new[] {"a", "b"}},
                new object[] {new[] {"a"}, new[] {"a"}},
                new object[] {new[] {"c", "b", "a"}, new[] {"a", "b", "c"}},
                new object[] {new string[0], new string[0]}
            };

            for (var i = 0; i < testdata.Length; i++)
            {
                var expected = (string[]) testdata[i][1];
                var input = (string[]) testdata[i][0];
                var received = CollectionUtil.CopySortArray(input);
                CollectionAssert.AreEqual(expected, received);

                //if (!Equals(expected, received))
                //{
                //    Assert.Fail("Failed for input " + input.RenderAny() +
                //                " expected " + expected.RenderAny() +
                //                " received " + received.RenderAny());
                //}

                ClassicAssert.AreNotSame(input, expected);
            }
        }

        [Test]
        public void TestToString()
        {
            object[][] testdata = {
                new object[] {new[] {"a", "b"}, "a, b"},
                new object[] {new[] {"a"}, "a"},
                new object[] {new[] {""}, ""},
                new object[] {new[] {"", ""}, ""},
                new object[] {new[] {null, "b"}, "b"},
                new object[] {new string[0], ""},
                new object[] {null, "null"}
            };

            for (var i = 0; i < testdata.Length; i++)
            {
                string expected = (string) testdata[i][1];
                string[] input = (string[]) testdata[i][0];
                ClassicAssert.AreEqual(expected, CollectionUtil.ToString(ToSet(input)), "Failed for input " + input);
            }
        }
    }
} // end of namespace