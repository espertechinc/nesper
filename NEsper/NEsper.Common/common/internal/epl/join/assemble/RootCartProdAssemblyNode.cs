///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.@join.rep;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Assembly node for an event stream that is a root with a two or more child nodes below it.
    /// </summary>
    public class RootCartProdAssemblyNode : BaseAssemblyNode
    {
        private readonly bool allSubStreamsOptional;
        private readonly int[] childStreamIndex; // maintain mapping of stream number to index in array
        private readonly IList<EventBean[]>[] rowsPerStream;
        private int[][] combinedSubStreams; // for any cartesian product past 2 streams
        private bool haveChildResults;

        // maintain for each child the list of stream number descending that child
        private int[][] subStreamsNumsPerChild;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="allSubStreamsOptional">true if all substreams are optional and none are required</param>
        /// <param name="childStreamIndex">indexes for child streams</param>
        public RootCartProdAssemblyNode(
            int streamNum, int numStreams, bool allSubStreamsOptional, int[] childStreamIndex) : base(
            streamNum, numStreams)
        {
            this.allSubStreamsOptional = allSubStreamsOptional;
            this.childStreamIndex = childStreamIndex;
            rowsPerStream = new IList<EventBean[]>[numStreams];
        }

        public override void Init(IList<Node>[] result)
        {
            if (subStreamsNumsPerChild == null) {
                if (childNodes.Count < 2) {
                    throw new IllegalStateException("Expecting at least 2 child nodes");
                }

                subStreamsNumsPerChild = new int[childNodes.Count][];
                for (var i = 0; i < childNodes.Count; i++) {
                    subStreamsNumsPerChild[i] = childNodes[i].Substreams;
                }

                combinedSubStreams = ComputeCombined(subStreamsNumsPerChild);
            }

            haveChildResults = false;
            for (var i = 0; i < rowsPerStream.Length; i++) {
                rowsPerStream[i] = null;
            }
        }

        public override void Process(
            IList<Node>[] result, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {
            // If no child has posted any rows, generate row and done
            if (!haveChildResults && allSubStreamsOptional) {
                // post an empty row
                var row = new EventBean[numStreams];
                parentNode.Result(row, streamNum, null, null, resultFinalRows, resultRootEvent);
                return;
            }

            // Compute the cartesian product
            PostCartesian(rowsPerStream, resultFinalRows, resultRootEvent);
        }

        public override void Result(
            EventBean[] row, int fromStreamNum, EventBean myEvent, Node myNode,
            ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {
            haveChildResults = true;

            // fill event in
            row[streamNum] = myEvent;
            var childStreamArrIndex = childStreamIndex[fromStreamNum];

            // keep a reference to the row to build a cartesian product on the call to process
            var rows = rowsPerStream[childStreamArrIndex];
            if (rows == null) {
                rows = new List<EventBean[]>();
                rowsPerStream[childStreamArrIndex] = rows;
            }

            rows.Add(row);
        }

        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("RootCartProdAssemblyNode streamNum=" + streamNum);
        }

        private void PostCartesian(
            IList<EventBean[]>[] rowsPerStream, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {
            IList<EventBean[]> result = new List<EventBean[]>();
            CartesianUtil.ComputeCartesian(
                rowsPerStream[0], subStreamsNumsPerChild[0],
                rowsPerStream[1], subStreamsNumsPerChild[1],
                result);

            if (rowsPerStream.Length > 2) {
                for (var i = 0; i < subStreamsNumsPerChild.Length - 2; i++) {
                    IList<EventBean[]> product = new List<EventBean[]>();
                    CartesianUtil.ComputeCartesian(
                        result, combinedSubStreams[i],
                        rowsPerStream[i + 2], subStreamsNumsPerChild[i + 2],
                        product);
                    result = product;
                }
            }

            foreach (var row in result) {
                parentNode.Result(row, streamNum, null, null, resultFinalRows, resultRootEvent);
            }
        }

        /// <summary>
        ///     Compute an array of supersets of sub stream numbers per stream, for at least 3 or more streams.
        /// </summary>
        /// <param name="subStreamsPerChild">is for each stream number a list of direct child sub streams</param>
        /// <returns>
        ///     an array in with length (subStreamsPerChild.lenght - 2) in whicharray[0] contains the streams for
        ///     subStreamsPerChild[0] and subStreamsPerChild[1] combined, and
        ///     array[1] contains the streams for subStreamsPerChild[0], subStreamsPerChild[1] and subStreamsPerChild[2] combined
        /// </returns>
        protected internal static int[][] ComputeCombined(int[][] subStreamsPerChild)
        {
            if (subStreamsPerChild.Length < 3) {
                return null;
            }

            // Add all substreams of (1 + 2)  up into = Sum3
            // Then add all substreams of (Sum3 + 3) => Sum4
            // Results in an array of size (subStreamsPerChild.lenght - 2) containing Sum3, Sum4 etc

            var result = new int[subStreamsPerChild.Length - 2][];

            result[0] = AddSubstreams(subStreamsPerChild[0], subStreamsPerChild[1]);
            for (var i = 0; i < subStreamsPerChild.Length - 3; i++) {
                result[i + 1] = AddSubstreams(result[i], subStreamsPerChild[i + 2]);
            }

            return result;
        }

        private static int[] AddSubstreams(int[] arrayOne, int[] arrayTwo)
        {
            var result = new int[arrayOne.Length + arrayTwo.Length];

            var count = 0;
            for (var i = 0; i < arrayOne.Length; i++) {
                result[count] = arrayOne[i];
                count++;
            }

            for (var i = 0; i < arrayTwo.Length; i++) {
                result[count] = arrayTwo[i];
                count++;
            }

            return result;
        }
    }
} // end of namespace