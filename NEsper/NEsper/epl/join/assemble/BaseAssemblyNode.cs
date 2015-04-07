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

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.assemble
{
    /// <summary>
    /// Represents a node in a tree responsible for assembling outer join query results.
    /// 
    /// The tree is double-linked, child nodes know each parent and parent know all child nodes.
    /// 
    /// Each specific subclass of this abstract assembly node is dedicated to assembling results for
    /// a certain event stream.
    /// </summary>

    public abstract class BaseAssemblyNode : ResultAssembler
    {
        /// <summary> Returns the stream number.</summary>
        /// <returns> stream number
        /// </returns>

        public virtual int StreamNum
        {
            get { return _streamNum; }
        }

        /// <summary> Returns an array of stream numbers that lists all child node's stream numbers.</summary>
        /// <returns> child node stream numbers
        /// </returns>
        public virtual int[] Substreams
        {
            get
            {
                IList<Int32> substreams = new List<Int32>();
                RecusiveAddSubstreams(substreams);

                // copy to array
                int[] substreamArr = new int[substreams.Count];
                int count = 0;

                foreach (Int32 stream in substreams)
                {
                    substreamArr[count++] = stream;
                }

                return substreamArr;
            }

        }
        /// <summary> Parent node.</summary>
        protected ResultAssembler ParentNode;

        /// <summary> Child nodes.</summary>
        private readonly IList<BaseAssemblyNode> _childNodes;

        /// <summary> Stream number.</summary>
        private readonly int _streamNum;

        /// <summary> Number of streams in statement.</summary>
        private readonly int _numStreams;

        /// <summary> Ctor.</summary>
        /// <param name="streamNum">stream number of the event stream that this node assembles results for.
        /// </param>
        /// <param name="numStreams">number of streams
        /// </param>

        protected BaseAssemblyNode(int streamNum, int numStreams)
        {
            _streamNum = streamNum;
            _numStreams = numStreams;
            _childNodes = new List<BaseAssemblyNode>();
        }

        /// <summary> Provides results to assembly nodes for initialization.</summary>
        /// <param name="result">is a list of result nodes per stream
        /// </param>
        public abstract void Init(IList<Node>[] result);

        /// <summary>
        /// Process results.
        /// </summary>
        /// <param name="result">is a list of result nodes per stream</param>
        /// <param name="resultFinalRows">The result final rows.</param>
        /// <param name="resultRootEvent">The result root event.</param>
        public abstract void Process(IList<Node>[] result, ICollection<EventBean[]> resultFinalRows, EventBean resultRootEvent);

        /// <summary> Output this node using writer, not outputting child nodes.</summary>
        /// <param name="indentWriter">to use for output
        /// </param>
        public abstract void Print(IndentWriter indentWriter);

        /// <summary> Add a child node.</summary>
        /// <param name="childNode">to add
        /// </param>
        public virtual void AddChild(BaseAssemblyNode childNode)
        {
            childNode.ParentNode = this;
            _childNodes.Add(childNode);
        }

        internal int NumStreams
        {
            get { return _numStreams; }
        }

        /// <summary> Returns child nodes.</summary>
        /// <returns> child nodes
        /// </returns>
        public IList<BaseAssemblyNode> ChildNodes
        {
            get
            {
                return _childNodes;
            }
        }

        /// <summary> Gets or sets the parent node.</summary>
        /// <returns> parent node
        /// </returns>
        public virtual ResultAssembler ParentAssembler
        {
            // get { return ParentNode; }
            set { ParentNode = value; }
        }

        private void RecusiveAddSubstreams(IList<Int32> substreams)
        {
            substreams.Add(_streamNum);

            foreach (BaseAssemblyNode child in _childNodes)
            {
                child.RecusiveAddSubstreams(substreams);
            }
        }

        public abstract void Result(EventBean[] row,
                                    int fromStreamNum,
                                    EventBean myEvent,
                                    Node myNode,
                                    ICollection<EventBean[]> resultFinalRows,
                                    EventBean resultRootEvent);
    }
}
