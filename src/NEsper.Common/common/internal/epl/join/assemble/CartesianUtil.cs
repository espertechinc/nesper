///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Helper class to compute the cartesian product of the events from two streams.
    /// </summary>
    public class CartesianUtil
    {
        /// <summary>
        ///     Form the 2-ary cartesian product between zero or more events from 2 streams.
        /// </summary>
        /// <param name="streamOne">is the events from stream one</param>
        /// <param name="subStreamNumsOne">is the list of substream numbers to stream one to include in the product</param>
        /// <param name="streamTwo">is the events from stream two</param>
        /// <param name="subStreamNumsTwo">is the list of substream numbers to stream two to include in the product</param>
        /// <param name="resultList">is where the result of the cartesian product is added to</param>
        public static void ComputeCartesian(
            IList<EventBean[]> streamOne,
            int[] subStreamNumsOne,
            IList<EventBean[]> streamTwo,
            int[] subStreamNumsTwo,
            IList<EventBean[]> resultList)
        {
            if (streamTwo == null || streamTwo.IsEmpty()) {
                if (streamOne == null || streamOne.IsEmpty()) {
                    return;
                }

                resultList.AddAll(streamOne);
                return;
            }

            if (streamOne == null || streamOne.IsEmpty()) {
                resultList.AddAll(streamTwo);
                return;
            }

            var streamOneSize = streamOne.Count;
            var streamTwoSize = streamTwo.Count;

            if (streamOneSize == 1) {
                // Yes we are re-using the results of stream two, same row reference
                CopyToEach(subStreamNumsOne, streamOne[0], streamTwo);
                resultList.AddAll(streamTwo);
                return;
            }

            if (streamTwoSize == 1) {
                // Yes we are re-using the results of stream one, same row reference
                CopyToEach(subStreamNumsTwo, streamTwo[0], streamOne);
                resultList.AddAll(streamOne);
                return;
            }

            // we have more then 1 rows each child stream

            // Exchange streams if one is smaller then two
            // Since if one has 100 rows the other has 2 then we can re-use the 100 event rows.
            if (streamTwoSize > streamOneSize) {
                var holdRows = streamOne;
                var holdSize = streamOneSize;

                streamOne = streamTwo;
                streamOneSize = streamTwoSize;

                streamTwo = holdRows;
                streamTwoSize = holdSize;
                subStreamNumsTwo = subStreamNumsOne;
            }

            // allocate resultList of join
            var cartesianTotalRows = streamOneSize * streamTwoSize;
            var numColumns = streamOne[0].Length;
            var results = new EventBean[cartesianTotalRows][];

            // Allocate and pre-populate copies of stream 1
            var streamOneCount = 0;
            foreach (var row in streamOne) {
                // first use all events in stream 1
                results[streamOneCount] = row;

                // then allocate copies for each in stream 2
                for (var i = 1; i < streamTwoSize; i++) {
                    var dupRow = new EventBean[numColumns];
                    Array.Copy(row, 0, dupRow, 0, numColumns);

                    var index = streamOneSize * i + streamOneCount;
                    results[index] = dupRow;
                }

                streamOneCount++;
            }

            // Copy stream 2 rows into rows of stream 1
            var streamTwoCount = 0;
            foreach (var row in streamTwo) {
                for (var i = 0; i < streamOneSize; i++) {
                    var index = streamTwoCount * streamOneSize + i;
                    Copy(subStreamNumsTwo, row, results[index]);
                }

                streamTwoCount++;
            }

            // Add results
            resultList.AddAll(results);
        }

        private static void CopyToEach(
            int[] subStreamNums,
            EventBean[] sourceRow,
            IList<EventBean[]> destRows)
        {
            foreach (var destRow in destRows) {
                Copy(subStreamNums, sourceRow, destRow);
            }
        }

        private static void Copy(
            int[] subStreamsFrom,
            EventBean[] from,
            EventBean[] to)
        {
            foreach (var index in subStreamsFrom) {
                to[index] = from[index];
            }
        }
    }
} // end of namespace