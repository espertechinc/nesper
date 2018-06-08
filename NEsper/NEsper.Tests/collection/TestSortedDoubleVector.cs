///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using com.espertech.esper.compat;
using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestSortedDoubleVector
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _vector = new SortedDoubleVector();
        }

        #endregion

        private SortedDoubleVector _vector;

        private int FindIndex(double[] data, double value)
        {
            _vector.Values.Clear();
            foreach (double aData in data) {
                _vector.Values.Add(aData);
            }
            return _vector.FindInsertIndex(value);
        }

        private void Compare(double[] expected, SortedDoubleVector vector)
        {
            Assert.AreEqual(expected.Length, vector.Count);
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i], vector[i]);
            }
        }

        [Test]
        public void TestAdd()
        {
            Assert.AreEqual(0, _vector.Count);

            _vector.Add(10);
            _vector.Add(0);
            _vector.Add(5);
            var expected = new double[] {0, 5, 10};
            Compare(expected, _vector);

            _vector.Add(10);
            _vector.Add(1);
            _vector.Add(5.5);
            expected = new[] {0, 1, 5, 5.5, 10, 10};
            Compare(expected, _vector);

            _vector.Add(9);
            _vector.Add(2);
            _vector.Add(5.5);
            expected = new[] {0, 1, 2, 5, 5.5, 5.5, 9, 10, 10};
            Compare(expected, _vector);
        }

        [Test]
        public void TestFindInsertIndex()
        {
            Assert.AreEqual(-1, _vector.FindInsertIndex(1));

            // test distinct values, 10 to 80
            _vector.Values.Add(10D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(-1, _vector.FindInsertIndex(11));

            _vector.Values.Add(20D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(-1, _vector.FindInsertIndex(21));

            _vector.Values.Add(30D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(2, _vector.FindInsertIndex(21));
            Assert.AreEqual(2, _vector.FindInsertIndex(29));
            Assert.AreEqual(2, _vector.FindInsertIndex(30));
            Assert.AreEqual(-1, _vector.FindInsertIndex(31));

            _vector.Values.Add(40D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(2, _vector.FindInsertIndex(21));
            Assert.AreEqual(2, _vector.FindInsertIndex(29));
            Assert.AreEqual(2, _vector.FindInsertIndex(30));
            Assert.AreEqual(3, _vector.FindInsertIndex(31));
            Assert.AreEqual(3, _vector.FindInsertIndex(39));
            Assert.AreEqual(3, _vector.FindInsertIndex(40));
            Assert.AreEqual(-1, _vector.FindInsertIndex(41));

            _vector.Values.Add(50D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(2, _vector.FindInsertIndex(21));
            Assert.AreEqual(2, _vector.FindInsertIndex(29));
            Assert.AreEqual(2, _vector.FindInsertIndex(30));
            Assert.AreEqual(3, _vector.FindInsertIndex(31));
            Assert.AreEqual(3, _vector.FindInsertIndex(39));
            Assert.AreEqual(3, _vector.FindInsertIndex(40));
            Assert.AreEqual(4, _vector.FindInsertIndex(41));
            Assert.AreEqual(4, _vector.FindInsertIndex(49));
            Assert.AreEqual(4, _vector.FindInsertIndex(50));
            Assert.AreEqual(-1, _vector.FindInsertIndex(51));

            _vector.Values.Add(60D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(2, _vector.FindInsertIndex(21));
            Assert.AreEqual(2, _vector.FindInsertIndex(29));
            Assert.AreEqual(2, _vector.FindInsertIndex(30));
            Assert.AreEqual(3, _vector.FindInsertIndex(31));
            Assert.AreEqual(3, _vector.FindInsertIndex(39));
            Assert.AreEqual(3, _vector.FindInsertIndex(40));
            Assert.AreEqual(4, _vector.FindInsertIndex(41));
            Assert.AreEqual(4, _vector.FindInsertIndex(49));
            Assert.AreEqual(4, _vector.FindInsertIndex(50));
            Assert.AreEqual(5, _vector.FindInsertIndex(51));
            Assert.AreEqual(5, _vector.FindInsertIndex(59));
            Assert.AreEqual(5, _vector.FindInsertIndex(60));
            Assert.AreEqual(-1, _vector.FindInsertIndex(61));

            _vector.Values.Add(70D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(2, _vector.FindInsertIndex(21));
            Assert.AreEqual(2, _vector.FindInsertIndex(29));
            Assert.AreEqual(2, _vector.FindInsertIndex(30));
            Assert.AreEqual(3, _vector.FindInsertIndex(31));
            Assert.AreEqual(3, _vector.FindInsertIndex(39));
            Assert.AreEqual(3, _vector.FindInsertIndex(40));
            Assert.AreEqual(4, _vector.FindInsertIndex(41));
            Assert.AreEqual(4, _vector.FindInsertIndex(49));
            Assert.AreEqual(4, _vector.FindInsertIndex(50));
            Assert.AreEqual(5, _vector.FindInsertIndex(51));
            Assert.AreEqual(5, _vector.FindInsertIndex(59));
            Assert.AreEqual(5, _vector.FindInsertIndex(60));
            Assert.AreEqual(6, _vector.FindInsertIndex(61));
            Assert.AreEqual(6, _vector.FindInsertIndex(69));
            Assert.AreEqual(6, _vector.FindInsertIndex(70));
            Assert.AreEqual(-1, _vector.FindInsertIndex(71));

            _vector.Values.Add(80D);
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(0, _vector.FindInsertIndex(10));
            Assert.AreEqual(1, _vector.FindInsertIndex(11));
            Assert.AreEqual(1, _vector.FindInsertIndex(19));
            Assert.AreEqual(1, _vector.FindInsertIndex(20));
            Assert.AreEqual(2, _vector.FindInsertIndex(21));
            Assert.AreEqual(2, _vector.FindInsertIndex(29));
            Assert.AreEqual(2, _vector.FindInsertIndex(30));
            Assert.AreEqual(3, _vector.FindInsertIndex(31));
            Assert.AreEqual(3, _vector.FindInsertIndex(39));
            Assert.AreEqual(3, _vector.FindInsertIndex(40));
            Assert.AreEqual(4, _vector.FindInsertIndex(41));
            Assert.AreEqual(4, _vector.FindInsertIndex(49));
            Assert.AreEqual(4, _vector.FindInsertIndex(50));
            Assert.AreEqual(5, _vector.FindInsertIndex(51));
            Assert.AreEqual(5, _vector.FindInsertIndex(59));
            Assert.AreEqual(5, _vector.FindInsertIndex(60));
            Assert.AreEqual(6, _vector.FindInsertIndex(61));
            Assert.AreEqual(6, _vector.FindInsertIndex(69));
            Assert.AreEqual(6, _vector.FindInsertIndex(70));
            Assert.AreEqual(7, _vector.FindInsertIndex(71));
            Assert.AreEqual(7, _vector.FindInsertIndex(79));
            Assert.AreEqual(7, _vector.FindInsertIndex(80));
            Assert.AreEqual(-1, _vector.FindInsertIndex(81));

            // test homogenous values, all 1
            _vector.Values.Clear();
            _vector.Values.Add(1D);
            Assert.AreEqual(0, _vector.FindInsertIndex(0));
            Assert.AreEqual(0, _vector.FindInsertIndex(1));
            Assert.AreEqual(-1, _vector.FindInsertIndex(2));
            for (int i = 0; i < 100; i++) {
                _vector.Values.Add(1D);
                Assert.AreEqual(0, _vector.FindInsertIndex(0), "for i=" + i);
                Assert.IsTrue(_vector.FindInsertIndex(1) != -1, "for i=" + i);
                Assert.AreEqual(-1, _vector.FindInsertIndex(2), "for i=" + i);
            }

            // test various other cases
            var vector = new double[] {1, 1, 2, 2, 2, 3, 4, 5, 5, 6};
            Assert.AreEqual(0, FindIndex(vector, 0));
            Assert.AreEqual(0, FindIndex(vector, 0.5));
            Assert.AreEqual(0, FindIndex(vector, 1));
            Assert.AreEqual(2, FindIndex(vector, 1.5));
            Assert.AreEqual(2, FindIndex(vector, 2));
            Assert.AreEqual(5, FindIndex(vector, 2.5));
            Assert.AreEqual(5, FindIndex(vector, 3));
            Assert.AreEqual(6, FindIndex(vector, 3.5));
            Assert.AreEqual(6, FindIndex(vector, 4));
            Assert.AreEqual(7, FindIndex(vector, 4.5));
            Assert.AreEqual(7, FindIndex(vector, 5));
            Assert.AreEqual(9, FindIndex(vector, 5.5));
            Assert.AreEqual(9, FindIndex(vector, 6));
            Assert.AreEqual(-1, FindIndex(vector, 6.5));
            Assert.AreEqual(-1, FindIndex(vector, 7));

            // test various other cases
            vector = new double[] {1, 8, 100, 1000, 1000, 10000, 10000, 99999};
            Assert.AreEqual(0, FindIndex(vector, 0));
            Assert.AreEqual(0, FindIndex(vector, 1));
            Assert.AreEqual(1, FindIndex(vector, 2));
            Assert.AreEqual(1, FindIndex(vector, 7));
            Assert.AreEqual(1, FindIndex(vector, 8));
            Assert.AreEqual(2, FindIndex(vector, 9));
            Assert.AreEqual(2, FindIndex(vector, 99));
            Assert.AreEqual(2, FindIndex(vector, 100));
            Assert.AreEqual(3, FindIndex(vector, 101));
            Assert.AreEqual(3, FindIndex(vector, 999));
            Assert.AreEqual(4, FindIndex(vector, 1000));
            Assert.AreEqual(5, FindIndex(vector, 1001));
            Assert.AreEqual(5, FindIndex(vector, 9999));
            Assert.AreEqual(6, FindIndex(vector, 10000));
            Assert.AreEqual(7, FindIndex(vector, 10001));
            Assert.AreEqual(7, FindIndex(vector, 99998));
            Assert.AreEqual(7, FindIndex(vector, 99999));
            Assert.AreEqual(-1, FindIndex(vector, 100000));
        }

        [Test]
        public void TestRemove()
        {
            _vector.Add(5);
            _vector.Add(1);
            _vector.Add(0);
            _vector.Add(-1);
            _vector.Add(1);
            _vector.Add(0.5);
            var expected = new[] {-1, 0, 0.5, 1, 1, 5};
            Compare(expected, _vector);

            _vector.Remove(1);
            expected = new[] {-1, 0, 0.5, 1, 5};
            Compare(expected, _vector);

            _vector.Remove(-1);
            _vector.Add(5);
            expected = new[] {0, 0.5, 1, 5, 5};
            Compare(expected, _vector);

            _vector.Remove(5);
            _vector.Remove(5);
            expected = new[] {0, 0.5, 1};
            Compare(expected, _vector);

            _vector.Add(99);
            _vector.Remove(99);
            try {
                _vector.Remove(99);
                Assert.Fail();
            }
            catch (IllegalStateException) {
                // expected
            }

            _vector.Add(Double.NaN);
            _vector.Remove(Double.NaN);
        }
    }
}
