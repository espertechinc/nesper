///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

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

        private void TryAddStringArr(
            string[] expected,
            object result)
        {
            Assert.IsTrue(result.GetType().IsArray);
            Assert.AreEqual(typeof(string), result.GetType().GetElementType());
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
        public void TestAddArray()
        {
            TryAddStringArr("b,a".SplitCsv(), CollectionUtil.AddArrays(new[] { "b" }, new[] { "a" }));
            TryAddStringArr("a".SplitCsv(), CollectionUtil.AddArrays(null, new[] { "a" }));
            TryAddStringArr("b".SplitCsv(), CollectionUtil.AddArrays(new[] { "b" }, null));
            TryAddStringArr("a,b,c,d".SplitCsv(), CollectionUtil.AddArrays(new[] { "a", "b" }, new[] { "c", "d" }));
            Assert.AreEqual(null, CollectionUtil.AddArrays(null, null));

            var result = CollectionUtil.AddArrays(new[] { 1, 2 }, new[] { 3, 4 });
            EPAssertionUtil.AssertEqualsExactOrder(new[] { 1, 2, 3, 4 }, (int[]) result);

            try
            {
                CollectionUtil.AddArrays("a", null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Parameter is not an array: a", ex.Message);
            }

            try
            {
                CollectionUtil.AddArrays(null, "b");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Parameter is not an array: b", ex.Message);
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

            Assert.IsFalse(e[0].Equals(e[1]));

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
                    Assert.IsTrue(result == p1);
                }
                else if (expectedObj.Equals("P2"))
                {
                    Assert.IsTrue(result == p2);
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
                Assert.AreEqual(expected, CollectionUtil.SortCompare(left, right), "Failed for input " + left.RenderAny());
                Assert.IsTrue(Equals(left, (string[]) testdata[i][0]));
                Assert.IsTrue(Equals(right, (string[]) testdata[i][1]));
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
                if (!Equals(expected, received))
                {
                    Assert.Fail("Failed for input " + input.RenderAny() +
                                " expected " + expected.RenderAny() +
                                " received " + received.RenderAny());
                }

                Assert.AreNotSame(input, expected);
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
                var expected = (string) testdata[i][1];
                var input = (string[]) testdata[i][0];
                Assert.AreEqual("Failed for input " + input, expected, CollectionUtil.ToString(ToSet(input)));
            }
        }
    }
} // end of namespace
