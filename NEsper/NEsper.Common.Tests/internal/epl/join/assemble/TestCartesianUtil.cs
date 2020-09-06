///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    [TestFixture]
    public class TestCartesianUtil : AbstractCommonTest
    {
        private const int NUM_COL = 4;

        private int[] _substreamsA;
        private int[] _substreamsB;
        private IList<EventBean[]> _results;

        [SetUp]
        public void SetUp()
        {
            _substreamsA = new[] { 0, 3 };
            _substreamsB = new[] { 1 };
            _results = new List<EventBean[]>();
        }

        private void TryCompute(
            IList<EventBean[]> rowsOne,
            IList<EventBean[]> rowsTwo)
        {
            _results.Clear();
            CartesianUtil.ComputeCartesian(rowsOne, _substreamsA, rowsTwo, _substreamsB, _results);
        }

        private IList<EventBean[]> MakeRowsA(int numRows)
        {
            return MakeRows(numRows, _substreamsA);
        }

        private IList<EventBean[]> MakeRowsB(int numRows)
        {
            return MakeRows(numRows, _substreamsB);
        }

        private IList<EventBean[]> MakeRows(
            int numRows,
            int[] substreamsPopulated)
        {
            IList<EventBean[]> result = new List<EventBean[]>();
            for (var i = 0; i < numRows; i++)
            {
                var row = new EventBean[NUM_COL];
                for (var j = 0; j < substreamsPopulated.Length; j++)
                {
                    var index = substreamsPopulated[j];
                    row[index] = supportJoinResultNodeFactory.MakeEvent();
                }

                result.Add(row);
            }

            return result;
        }

        [Test, RunInApplicationDomain]
        public void TestCompute()
        {
            // test null
            IList<EventBean[]> rowsA = null;
            IList<EventBean[]> rowsB = null;
            TryCompute(rowsA, rowsB);
            Assert.IsTrue(_results.IsEmpty());

            // test no rows A
            rowsA = new List<EventBean[]>();
            TryCompute(rowsA, rowsB);
            Assert.IsTrue(_results.IsEmpty());

            // test no rows B
            rowsA = null;
            rowsB = new List<EventBean[]>();
            TryCompute(rowsA, rowsB);
            Assert.IsTrue(_results.IsEmpty());

            // test side A one row, B empty
            rowsA = MakeRowsA(1);
            rowsB = null;
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(1, _results.Count);
            EPAssertionUtil.AssertEqualsExactOrder(_results[0], rowsA[0]);

            // test side B one row, A empty
            rowsA = null;
            rowsB = MakeRowsB(1);
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(1, _results.Count);
            EPAssertionUtil.AssertEqualsExactOrder(_results[0], rowsB[0]);

            // test A and B one row
            rowsA = MakeRowsA(1);
            rowsB = MakeRowsB(1);
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(1, _results.Count);
            EPAssertionUtil.AssertEqualsExactOrder(
                _results[0],
                new[] { rowsA[0][0], rowsB[0][1], null, rowsA[0][3] });

            // test A=2 rows and B=1 row
            rowsA = MakeRowsA(2);
            rowsB = MakeRowsB(1);
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(2, _results.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {rowsA[0][0], rowsB[0][1], null, rowsA[0][3]},
                    new[] {rowsA[1][0], rowsB[0][1], null, rowsA[1][3]}
                },
                supportJoinResultNodeFactory.ConvertTo2DimArr(_results));

            // test A=1 rows and B=2 row
            rowsA = MakeRowsA(1);
            rowsB = MakeRowsB(2);
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(2, _results.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {rowsA[0][0], rowsB[0][1], null, rowsA[0][3]},
                    new[] {rowsA[0][0], rowsB[1][1], null, rowsA[0][3]}
                },
                supportJoinResultNodeFactory.ConvertTo2DimArr(_results));

            // test A=2 rows and B=2 row
            rowsA = MakeRowsA(2);
            rowsB = MakeRowsB(2);
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(4, _results.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {rowsA[0][0], rowsB[0][1], null, rowsA[0][3]},
                    new[] {rowsA[0][0], rowsB[1][1], null, rowsA[0][3]},
                    new[] {rowsA[1][0], rowsB[0][1], null, rowsA[1][3]},
                    new[] {rowsA[1][0], rowsB[1][1], null, rowsA[1][3]}
                },
                supportJoinResultNodeFactory.ConvertTo2DimArr(_results));

            // test A=2 rows and B=3 row
            rowsA = MakeRowsA(2);
            rowsB = MakeRowsB(3);
            TryCompute(rowsA, rowsB);
            Assert.AreEqual(6, _results.Count);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {rowsA[0][0], rowsB[0][1], null, rowsA[0][3]},
                    new[] {rowsA[0][0], rowsB[1][1], null, rowsA[0][3]},
                    new[] {rowsA[0][0], rowsB[2][1], null, rowsA[0][3]},
                    new[] {rowsA[1][0], rowsB[0][1], null, rowsA[1][3]},
                    new[] {rowsA[1][0], rowsB[1][1], null, rowsA[1][3]},
                    new[] {rowsA[1][0], rowsB[2][1], null, rowsA[1][3]}
                },
                supportJoinResultNodeFactory.ConvertTo2DimArr(_results));
        }
    }
} // end of namespace
