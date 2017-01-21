///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.map;

using NUnit.Framework;

namespace com.espertech.esper.util
{
    [TestFixture]
    public class TestCollectionUtil 
    {
        [Test]
        public void TestArrayExpandSingle() {
            RunAssertionExpandSingle("a", "", "a");
            RunAssertionExpandSingle("a,b", "a", "b");
            RunAssertionExpandSingle("a,b,c", "a,b", "c");
            RunAssertionExpandSingle("a,b,c,d", "a,b,c", "d");
        }
    
        [Test]
        public void TestArrayExpandCollectionAndArray() {
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
        public void TestArrayShrink() {
            RunAssertionShrink("a,c", "a,b,c", 1);
            RunAssertionShrink("b,c", "a,b,c", 0);
            RunAssertionShrink("a,b", "a,b,c", 2);
            RunAssertionShrink("a", "a,b", 1);
            RunAssertionShrink("b", "a,b", 0);
            RunAssertionShrink("", "a", 0);
        }
    
        private void RunAssertionShrink(String expected, String existing, int index) {
            var expectedArr = expected.Length == 0 ? new String[0] : expected.Split(',');
            var existingArr = existing.Length == 0 ? new String[0] : existing.Split(',');
            var resultAddColl = (String[]) CollectionUtil.ArrayShrinkRemoveSingle(existingArr, index);
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, resultAddColl);
        }
    
        private void RunAssertionExpandColl(String expected, String existing, String coll) {
            var expectedArr = expected.Length == 0 ? new String[0] : expected.Split(',');
            var existingArr = existing.Length == 0 ? new String[0] : existing.Split(',');
            ICollection<String> addCollection = coll.Length == 0 ? new String[0] : coll.Split(',');
            var resultAddColl = (String[]) CollectionUtil.ArrayExpandAddElements(existingArr, addCollection);
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, resultAddColl);
    
            var resultAddArr = (String[]) CollectionUtil.ArrayExpandAddElements(existingArr, addCollection.ToArray());
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, resultAddArr);
        }
    
        private void RunAssertionExpandSingle(String expected, String existing, String single) {
            var expectedArr = expected.Length == 0 ? new String[0] : expected.Split(',');
            var existingArr = existing.Length == 0 ? new String[0] : existing.Split(',');
            var result = (String[]) CollectionUtil.ArrayExpandAddSingle(existingArr, single);
            EPAssertionUtil.AssertEqualsExactOrder(expectedArr, result);
        }
    
        [Test]
        public void TestAddArraySetSemantics() {
    
            var e = new EventBean[10];
            for (var i = 0; i < e.Length; i++) {
                e[i] = new MapEventBean(null);
            }
            Assert.IsFalse(e[0].Equals(e[1]));
    
            var testData = new Object[][] {
                    new Object[] {new EventBean[] {}, new EventBean[] {}, "p2"},
                    new Object[] {new EventBean[] {}, new EventBean[] {e[0], e[1]}, "p2"},
                    new Object[] {new EventBean[] {e[0]}, new EventBean[] {}, "p1"},
                    new Object[] {new EventBean[] {e[0]}, new EventBean[] {e[0]}, "p1"},
                    new Object[] {new EventBean[] {e[0]}, new EventBean[] {e[1]}, new EventBean[] {e[0], e[1]}},
                    new Object[] {new EventBean[] {e[0], e[1]}, new EventBean[] {e[1]}, "p1"},
                    new Object[] {new EventBean[] {e[0], e[1]}, new EventBean[] {e[0]}, "p1"},
                    new Object[] {new EventBean[] {e[0]}, new EventBean[] {e[0], e[1]}, "p2"},
                    new Object[] {new EventBean[] {e[1]}, new EventBean[] {e[0], e[1]}, "p2"},
                    new Object[] {new EventBean[] {e[2]}, new EventBean[] {e[0], e[1]}, new EventBean[] {e[0], e[1], e[2]}},
                    new Object[] {new EventBean[] {e[2], e[0]}, new EventBean[] {e[0], e[1]}, new EventBean[] {e[0], e[1], e[2]}},
                    new Object[] {new EventBean[] {e[2], e[0]}, new EventBean[] {e[0], e[1], e[2]}, new EventBean[] {e[0], e[1], e[2]}}
            };
    
            for (var i = 0; i < testData.Length; i++) {
                var p1 = (EventBean[]) testData[i][0];
                var p2 = (EventBean[]) testData[i][1];
                var expectedObj = testData[i][2];
    
                Object result = CollectionUtil.AddArrayWithSetSemantics(p1, p2);
    
                if (expectedObj.Equals("p1")) {
                    Assert.IsTrue(result == p1);
                }
                else if (expectedObj.Equals("p2")) {
                    Assert.IsTrue(result == p2);
                }
                else {
                    var resultArray = (EventBean[]) result;
                    var expectedArray = (EventBean[]) result;
                    EPAssertionUtil.AssertEqualsAnyOrder(resultArray, expectedArray);
                }
            }
        }
    
        [Test]
        public void TestAddArray() {
            TryAddStringArr("b,a".Split(','), CollectionUtil.AddArrays(new String[] {"b"}, new String[] {"a"}));
            TryAddStringArr("a".Split(','), CollectionUtil.AddArrays(null, new String[] {"a"}));
            TryAddStringArr("b".Split(','), CollectionUtil.AddArrays(new String[] {"b"}, null));
            TryAddStringArr("a,b,c,d".Split(','), CollectionUtil.AddArrays(new String[] {"a", "b"}, new String[] {"c", "d"}));
            Assert.AreEqual(null, CollectionUtil.AddArrays(null, null));
    
            Object result = CollectionUtil.AddArrays(new int[] {1,2}, new int[] {3,4});
            EPAssertionUtil.AssertEqualsExactOrder(new int[] {1,2,3,4}, (int[]) result);
    
            try {
                CollectionUtil.AddArrays("a", null);
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual("Parameter is not an array: a", ex.Message);
            }
    
            try {
                CollectionUtil.AddArrays(null, "b");
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual("Parameter is not an array: b", ex.Message);
            }
        }
    
        private void TryAddStringArr(String[] expected, Object result) {
            Assert.IsTrue(result.GetType().IsArray);
            Assert.AreEqual(typeof (String), result.GetType().GetElementType());
            EPAssertionUtil.AssertEqualsExactOrder(expected, (String[]) result);
        }
    
        [Test]
        public void TestCopySort() {
            var testdata = new Object[][] {
                    new Object[] {new String[] {"a", "b"}, new String[] {"a", "b"}},
                    new Object[] {new String[] {"b", "a"}, new String[] {"a", "b"}},
                    new Object[] {new String[] {"a"}, new String[] {"a"}},
                    new Object[] {new String[] {"c", "b", "a"}, new String[] {"a", "b", "c"}},
                    new Object[] {new String[0], new String[0]},
                  };
    
            for (var i = 0; i < testdata.Length; i++) {
                var expected = (String[]) testdata[i][1];
                var input = (String[]) testdata[i][0];
                var received = CollectionUtil.CopySortArray(input);
                if (!Collections.AreEqual(expected, received)) {
                    Assert.Fail("Failed for input " + input.Render() + " expected " + expected.Render() + " received " + received.Render());
                }
                Assert.AreNotSame(input, expected);
            }
        }
    
        [Test]
        public void TestCompare()
        {
            var testdata = new Object[][] {
                    new Object[] {new String[] {"a", "b"}, new String[] {"a", "b"}, true},
                    new Object[] {new String[] {"a"}, new String[] {"a", "b"}, false},
                    new Object[] {new String[] {"a"}, new String[] {"a"}, true},
                    new Object[] {new String[] {"b"}, new String[] {"a"}, false},
                    new Object[] {new String[] {"b", "a"}, new String[] {"a", "b"}, true},
                    new Object[] {new String[] {"a", "b", "b"}, new String[] {"a", "b"}, false},
                    new Object[] {new String[] {"a", "b", "b"}, new String[] {"b", "a", "b"}, true},
                    new Object[] {new String[0], new String[0], true},
                  };
    
            for (var i = 0; i < testdata.Length; i++) {
                var left = (String[]) testdata[i][0];
                var right = (String[]) testdata[i][1];
                var expected = testdata[i][2].AsBoolean();
                Assert.AreEqual(expected, CollectionUtil.SortCompare(left, right), "Failed for input " + left.Render());
                Assert.IsTrue(Collections.AreEqual(left, (String[]) testdata[i][0]));
                Assert.IsTrue(Collections.AreEqual(right, (String[]) testdata[i][1]));
            }
        }
    
        [Test]
        public void TestToString()
        {
            var testdata = new Object[][] {
                    new Object[] {new String[] {"a", "b"}, "a, b"},
                    new Object[] {new String[] {"a"}, "a"},
                    new Object[] {new String[] {""}, ""},
                    new Object[] {new String[] {"", ""}, ""},
                    new Object[] {new String[] {null, "b"}, "b"},
                    new Object[] {new String[0], ""},
                    new Object[] {null, "null"}
                  };
    
            for (var i = 0; i < testdata.Length; i++)
            {
                var expected = (String) testdata[i][1];
                var input = (String[]) testdata[i][0];
                Assert.AreEqual(expected, CollectionUtil.ToString(ToSet(input)), "Failed for input " + input.Render());
            }
        }
    
        private ICollection<String> ToSet(String[] arr)
        {
            if (arr == null)
            {
                return null;
            }
            if (arr.Length == 0)
            {
                return new HashSet<String>();
            }
            ICollection<String> set = new LinkedHashSet<String>();
            foreach (var a in arr)
            {
                set.Add(a);
            }
            return set;
        }
    }
}
