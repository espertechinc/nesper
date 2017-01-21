///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.assemble
{
    /// <summary>
    /// Assembly node for an event stream that is a root with a two or more child nodes below it.
    /// </summary>
    public class RootCartProdAssemblyNode : BaseAssemblyNode
    {
        private readonly int[] _childStreamIndex; // maintain mapping of stream number to index in array
        private readonly IList<EventBean[]>[] _rowsPerStream;
        private readonly bool _allSubStreamsOptional;
    
        // maintain for each child the list of stream number descending that child
        private int[][] _subStreamsNumsPerChild;
        private int[][] _combinedSubStreams; // for any cartesian product past 2 streams
        private bool _haveChildResults;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">is the stream number</param>
        /// <param name="numStreams">is the number of streams</param>
        /// <param name="allSubStreamsOptional">true if all substreams are optional and none are required</param>
        public RootCartProdAssemblyNode(int streamNum, int numStreams, bool allSubStreamsOptional, int[] childStreamIndex)
            : base(streamNum, numStreams)
        {
            _allSubStreamsOptional = allSubStreamsOptional;
            _childStreamIndex = childStreamIndex;
            _rowsPerStream = new List<EventBean[]>[numStreams];
        }
    
        public override void Init(IList<Node>[] result)
        {
            if (_subStreamsNumsPerChild == null)
            {
                if (ChildNodes.Count < 2)
                {
                    throw new IllegalStateException("Expecting at least 2 child nodes");
                }

                var childNodes = ChildNodes;
                _subStreamsNumsPerChild = new int[childNodes.Count][];
                for (int i = 0; i < childNodes.Count; i++)
                {
                    _subStreamsNumsPerChild[i] = childNodes[i].Substreams;
                }
    
                _combinedSubStreams = ComputeCombined(_subStreamsNumsPerChild);
            }
    
            _haveChildResults = false;
            for (int i = 0; i < _rowsPerStream.Length; i++)
            {
                _rowsPerStream[i] = null;
            }
        }
    
        public override void Process(IList<Node>[] result, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {
            // If no child has posted any rows, generate row and done
            if ((!_haveChildResults) && (_allSubStreamsOptional))
            {
                // post an empty row
                var row = new EventBean[NumStreams];
                ParentNode.Result(row, StreamNum, null, null, resultFinalRows, resultRootEvent);
                return;
            }
    
            // Compute the cartesian product
            PostCartesian(_rowsPerStream, resultFinalRows, resultRootEvent);
        }
    
        public override void Result(EventBean[] row, int fromStreamNum, EventBean myEvent, Node myNode, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {
            _haveChildResults = true;
    
            // fill event in
            row[StreamNum] = myEvent;
            int childStreamArrIndex = _childStreamIndex[fromStreamNum];
    
            // keep a reference to the row to build a cartesian product on the call to process
            IList<EventBean[]> rows = _rowsPerStream[childStreamArrIndex];
            if (rows == null)
            {
                rows = new List<EventBean[]>();
                _rowsPerStream[childStreamArrIndex] = rows;
            }
            rows.Add(row);
        }
    
        public override void Print(IndentWriter indentWriter)
        {
            indentWriter.WriteLine("RootCartProdAssemblyNode StreamNum=" + StreamNum);
        }
    
        private void PostCartesian(IList<EventBean[]>[] rowsPerStream, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent)
        {
            IList<EventBean[]> result = new List<EventBean[]>();
            CartesianUtil.ComputeCartesian(
                    rowsPerStream[0], _subStreamsNumsPerChild[0],
                    rowsPerStream[1], _subStreamsNumsPerChild[1],
                    result);
    
            if (rowsPerStream.Length > 2)
            {
                for (int i = 0; i < _subStreamsNumsPerChild.Length - 2; i++)
                {
                    var product = new List<EventBean[]>();
                    CartesianUtil.ComputeCartesian(
                            result, _combinedSubStreams[i],
                            rowsPerStream[i + 2], _subStreamsNumsPerChild[i + 2],
                            product);
                    result = product;
                }
            }
    
            foreach (EventBean[] row in result)
            {
                ParentNode.Result(row, StreamNum, null, null, resultFinalRows, resultRootEvent);
            }
        }
    
        /// <summary>Compute an array of supersets of sub stream numbers per stream, for at least 3 or more streams. </summary>
        /// <param name="subStreamsPerChild">is for each stream number a list of direct child sub streams</param>
        /// <returns>an array in with length (subStreamsPerChild.lenght - 2) in whicharray[0] contains the streams for subStreamsPerChild[0] and subStreamsPerChild[1] combined, and array[1] contains the streams for subStreamsPerChild[0], subStreamsPerChild[1] and subStreamsPerChild[2] combined </returns>
        internal static int[][] ComputeCombined(int[][] subStreamsPerChild)
        {
            unchecked
            {
                if (subStreamsPerChild.Length < 3)
                {
                    return null;
                }

                // Add all substreams of (1 + 2)  up into = Sum3
                // Then add all substreams of (Sum3 + 3) => Sum4
                // Results in an array of size (subStreamsPerChild.lenght - 2) containing Sum3, Sum4 etc

                var result = new int[subStreamsPerChild.Length - 2][];

                result[0] = AddSubstreams(subStreamsPerChild[0], subStreamsPerChild[1]);
                for (int i = 0; i < subStreamsPerChild.Length - 3; i++)
                {
                    result[i + 1] = AddSubstreams(result[i], subStreamsPerChild[i + 2]);
                }

                return result;
            }
        }
    
        private static int[] AddSubstreams(int[] arrayOne, int[] arrayTwo)
        {
            unchecked
            {
                var result = new int[arrayOne.Length + arrayTwo.Length];

                int count = 0;
                for (int i = 0; i < arrayOne.Length; i++)
                {
                    result[count] = arrayOne[i];
                    count++;
                }

                for (int i = 0; i < arrayTwo.Length; i++)
                {
                    result[count] = arrayTwo[i];
                    count++;
                }
                return result;
            }
        }
    }
}
