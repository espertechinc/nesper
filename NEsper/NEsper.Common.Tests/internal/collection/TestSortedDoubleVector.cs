///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestSortedDoubleVector : AbstractTestBase
    {
        private SortedDoubleVector vector = new SortedDoubleVector();

        [SetUp]
        public void SetUp()
        {
            vector = new SortedDoubleVector();
        }

        [Test]
        public void TestAdd()
        {
            Assert.AreEqual(0, vector.Count);

            vector.Add(10);
            vector.Add(0);
            vector.Add(5);
            double[] expected = new double[] { 0, 5, 10 };
            Compare(expected, vector);

            vector.Add(10);
            vector.Add(1);
            vector.Add(5.5);
            expected = new double[] { 0, 1, 5, 5.5, 10, 10 };
            Compare(expected, vector);

            vector.Add(9);
            vector.Add(2);
            vector.Add(5.5);
            expected = new double[] { 0, 1, 2, 5, 5.5, 5.5, 9, 10, 10 };
            Compare(expected, vector);
        }

        [Test]
        public void TestRemove()
        {
            vector.Add(5);
            vector.Add(1);
            vector.Add(0);
            vector.Add(-1);
            vector.Add(1);
            vector.Add(0.5);
            double[] expected = new double[] {-1, 0, 0.5, 1, 1, 5};
            Compare(expected, vector);

            vector.Remove(1);
            expected = new double[] {-1, 0, 0.5, 1, 5};
            Compare(expected, vector);

            vector.Remove(-1);
            vector.Add(5);
            expected = new double[] {0, 0.5, 1, 5, 5};
            Compare(expected, vector);

            vector.Remove(5);
            vector.Remove(5);
            expected = new double[] {0, 0.5, 1};
            Compare(expected, vector);

            vector.Add(99);
            vector.Remove(99);
            vector.Remove(99);

            vector.Add(Double.NaN);
            vector.Remove(Double.NaN);
        }

        [Test]
        public void TestFindInsertIndex()
        {
            Assert.AreEqual(-1, vector.FindInsertIndex(1));

            // test distinct values, 10 to 80
            vector.Values.Add(10D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(-1, vector.FindInsertIndex(11));

            vector.Values.Add(20D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(-1, vector.FindInsertIndex(21));

            vector.Values.Add(30D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(2, vector.FindInsertIndex(21));
            Assert.AreEqual(2, vector.FindInsertIndex(29));
            Assert.AreEqual(2, vector.FindInsertIndex(30));
            Assert.AreEqual(-1, vector.FindInsertIndex(31));

            vector.Values.Add(40D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(2, vector.FindInsertIndex(21));
            Assert.AreEqual(2, vector.FindInsertIndex(29));
            Assert.AreEqual(2, vector.FindInsertIndex(30));
            Assert.AreEqual(3, vector.FindInsertIndex(31));
            Assert.AreEqual(3, vector.FindInsertIndex(39));
            Assert.AreEqual(3, vector.FindInsertIndex(40));
            Assert.AreEqual(-1, vector.FindInsertIndex(41));

            vector.Values.Add(50D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(2, vector.FindInsertIndex(21));
            Assert.AreEqual(2, vector.FindInsertIndex(29));
            Assert.AreEqual(2, vector.FindInsertIndex(30));
            Assert.AreEqual(3, vector.FindInsertIndex(31));
            Assert.AreEqual(3, vector.FindInsertIndex(39));
            Assert.AreEqual(3, vector.FindInsertIndex(40));
            Assert.AreEqual(4, vector.FindInsertIndex(41));
            Assert.AreEqual(4, vector.FindInsertIndex(49));
            Assert.AreEqual(4, vector.FindInsertIndex(50));
            Assert.AreEqual(-1, vector.FindInsertIndex(51));

            vector.Values.Add(60D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(2, vector.FindInsertIndex(21));
            Assert.AreEqual(2, vector.FindInsertIndex(29));
            Assert.AreEqual(2, vector.FindInsertIndex(30));
            Assert.AreEqual(3, vector.FindInsertIndex(31));
            Assert.AreEqual(3, vector.FindInsertIndex(39));
            Assert.AreEqual(3, vector.FindInsertIndex(40));
            Assert.AreEqual(4, vector.FindInsertIndex(41));
            Assert.AreEqual(4, vector.FindInsertIndex(49));
            Assert.AreEqual(4, vector.FindInsertIndex(50));
            Assert.AreEqual(5, vector.FindInsertIndex(51));
            Assert.AreEqual(5, vector.FindInsertIndex(59));
            Assert.AreEqual(5, vector.FindInsertIndex(60));
            Assert.AreEqual(-1, vector.FindInsertIndex(61));

            vector.Values.Add(70D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(2, vector.FindInsertIndex(21));
            Assert.AreEqual(2, vector.FindInsertIndex(29));
            Assert.AreEqual(2, vector.FindInsertIndex(30));
            Assert.AreEqual(3, vector.FindInsertIndex(31));
            Assert.AreEqual(3, vector.FindInsertIndex(39));
            Assert.AreEqual(3, vector.FindInsertIndex(40));
            Assert.AreEqual(4, vector.FindInsertIndex(41));
            Assert.AreEqual(4, vector.FindInsertIndex(49));
            Assert.AreEqual(4, vector.FindInsertIndex(50));
            Assert.AreEqual(5, vector.FindInsertIndex(51));
            Assert.AreEqual(5, vector.FindInsertIndex(59));
            Assert.AreEqual(5, vector.FindInsertIndex(60));
            Assert.AreEqual(6, vector.FindInsertIndex(61));
            Assert.AreEqual(6, vector.FindInsertIndex(69));
            Assert.AreEqual(6, vector.FindInsertIndex(70));
            Assert.AreEqual(-1, vector.FindInsertIndex(71));

            vector.Values.Add(80D);
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(0, vector.FindInsertIndex(10));
            Assert.AreEqual(1, vector.FindInsertIndex(11));
            Assert.AreEqual(1, vector.FindInsertIndex(19));
            Assert.AreEqual(1, vector.FindInsertIndex(20));
            Assert.AreEqual(2, vector.FindInsertIndex(21));
            Assert.AreEqual(2, vector.FindInsertIndex(29));
            Assert.AreEqual(2, vector.FindInsertIndex(30));
            Assert.AreEqual(3, vector.FindInsertIndex(31));
            Assert.AreEqual(3, vector.FindInsertIndex(39));
            Assert.AreEqual(3, vector.FindInsertIndex(40));
            Assert.AreEqual(4, vector.FindInsertIndex(41));
            Assert.AreEqual(4, vector.FindInsertIndex(49));
            Assert.AreEqual(4, vector.FindInsertIndex(50));
            Assert.AreEqual(5, vector.FindInsertIndex(51));
            Assert.AreEqual(5, vector.FindInsertIndex(59));
            Assert.AreEqual(5, vector.FindInsertIndex(60));
            Assert.AreEqual(6, vector.FindInsertIndex(61));
            Assert.AreEqual(6, vector.FindInsertIndex(69));
            Assert.AreEqual(6, vector.FindInsertIndex(70));
            Assert.AreEqual(7, vector.FindInsertIndex(71));
            Assert.AreEqual(7, vector.FindInsertIndex(79));
            Assert.AreEqual(7, vector.FindInsertIndex(80));
            Assert.AreEqual(-1, vector.FindInsertIndex(81));

            // test homogenous values, all 1
            vector.Values.Clear();
            vector.Values.Add(1D);
            Assert.AreEqual(0, vector.FindInsertIndex(0));
            Assert.AreEqual(0, vector.FindInsertIndex(1));
            Assert.AreEqual(-1, vector.FindInsertIndex(2));
            for (int i = 0; i < 100; i++)
            {
                vector.Values.Add(1D);
                Assert.AreEqual(0, vector.FindInsertIndex(0), "for i=" + i);
                Assert.IsTrue(vector.FindInsertIndex(1) != -1, "for i=" + i);
                Assert.AreEqual(-1, vector.FindInsertIndex(2), "for i=" + i);
            }

            // test various other cases
            double[] mvector = new double[] { 1, 1, 2, 2, 2, 3, 4, 5, 5, 6 };
            Assert.AreEqual(0, FindIndex(mvector, 0));
            Assert.AreEqual(0, FindIndex(mvector, 0.5));
            Assert.AreEqual(0, FindIndex(mvector, 1));
            Assert.AreEqual(2, FindIndex(mvector, 1.5));
            Assert.AreEqual(2, FindIndex(mvector, 2));
            Assert.AreEqual(5, FindIndex(mvector, 2.5));
            Assert.AreEqual(5, FindIndex(mvector, 3));
            Assert.AreEqual(6, FindIndex(mvector, 3.5));
            Assert.AreEqual(6, FindIndex(mvector, 4));
            Assert.AreEqual(7, FindIndex(mvector, 4.5));
            Assert.AreEqual(7, FindIndex(mvector, 5));
            Assert.AreEqual(9, FindIndex(mvector, 5.5));
            Assert.AreEqual(9, FindIndex(mvector, 6));
            Assert.AreEqual(-1, FindIndex(mvector, 6.5));
            Assert.AreEqual(-1, FindIndex(mvector, 7));

            // test various other cases
            mvector = new double[] { 1, 8, 100, 1000, 1000, 10000, 10000, 99999 };
            Assert.AreEqual(0, FindIndex(mvector, 0));
            Assert.AreEqual(0, FindIndex(mvector, 1));
            Assert.AreEqual(1, FindIndex(mvector, 2));
            Assert.AreEqual(1, FindIndex(mvector, 7));
            Assert.AreEqual(1, FindIndex(mvector, 8));
            Assert.AreEqual(2, FindIndex(mvector, 9));
            Assert.AreEqual(2, FindIndex(mvector, 99));
            Assert.AreEqual(2, FindIndex(mvector, 100));
            Assert.AreEqual(3, FindIndex(mvector, 101));
            Assert.AreEqual(3, FindIndex(mvector, 999));
            Assert.AreEqual(4, FindIndex(mvector, 1000));
            Assert.AreEqual(5, FindIndex(mvector, 1001));
            Assert.AreEqual(5, FindIndex(mvector, 9999));
            Assert.AreEqual(6, FindIndex(mvector, 10000));
            Assert.AreEqual(7, FindIndex(mvector, 10001));
            Assert.AreEqual(7, FindIndex(mvector, 99998));
            Assert.AreEqual(7, FindIndex(mvector, 99999));
            Assert.AreEqual(-1, FindIndex(mvector, 100000));
        }

        private int FindIndex(double[] data, double value)
        {
            vector.Values.Clear();
            foreach (double aData in data)
            {
                vector.Values.Add(aData);
            }
            return vector.FindInsertIndex(value);
        }

        private void Compare(double[] expected, SortedDoubleVector vectorArg)
        {
            Assert.AreEqual(expected.Length, vectorArg.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], vectorArg[i]);
            }
        }
    }
} // end of namespace