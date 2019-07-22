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

namespace com.espertech.esper.common.@internal.epl.join.assemble
{
    /// <summary>
    ///     Represents a node in a tree responsible for assembling outer join query results.
    ///     <para />
    ///     The tree is double-linked, child nodes know each parent and parent know all child nodes.
    ///     <para />
    ///     Each specific subclass of this abstract assembly node is dedicated to assembling results for
    ///     a certain event stream.
    /// </summary>
    public abstract class BaseAssemblyNode : ResultAssembler
    {
        /// <summary>
        ///     Child nodes.
        /// </summary>
        protected internal readonly IList<BaseAssemblyNode> childNodes;

        /// <summary>
        ///     Number of streams in statement.
        /// </summary>
        protected internal readonly int numStreams;

        /// <summary>
        ///     Stream number.
        /// </summary>
        protected internal readonly int streamNum;

        /// <summary>
        ///     Parent node.
        /// </summary>
        protected internal ResultAssembler parentNode;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="streamNum">stream number of the event stream that this node assembles results for.</param>
        /// <param name="numStreams">number of streams</param>
        protected internal BaseAssemblyNode(
            int streamNum,
            int numStreams)
        {
            this.streamNum = streamNum;
            this.numStreams = numStreams;
            childNodes = new List<BaseAssemblyNode>(4);
        }

        /// <summary>
        ///     Set parent node.
        /// </summary>
        /// <value>is the parent node</value>
        public ResultAssembler ParentAssembler {
            get => parentNode;
            set => parentNode = value;
        }

        /// <summary>
        ///     Returns the stream number.
        /// </summary>
        /// <value>stream number</value>
        internal int StreamNum => streamNum;

        /// <summary>
        ///     Returns child nodes.
        /// </summary>
        /// <value>child nodes</value>
        internal IList<BaseAssemblyNode> ChildNodes => childNodes;

        /// <summary>
        ///     Returns an array of stream numbers that lists all child node's stream numbers.
        /// </summary>
        /// <value>child node stream numbers</value>
        public int[] Substreams {
            get {
                IList<int> substreams = new List<int>();
                RecusiveAddSubstreams(substreams);

                // copy to array
                var substreamArr = new int[substreams.Count];
                var count = 0;
                foreach (var stream in substreams) {
                    substreamArr[count++] = stream;
                }

                return substreamArr;
            }
        }

        /// <summary>
        ///     Provides results to assembly nodes for initialization.
        /// </summary>
        /// <param name="result">is a list of result nodes per stream</param>
        public abstract void Init(IList<Node>[] result);

        /// <summary>
        ///     Process results.
        /// </summary>
        /// <param name="result">is a list of result nodes per stream</param>
        /// <param name="resultFinalRows">final row collection</param>
        /// <param name="resultRootEvent">root event</param>
        public abstract void Process(
            IList<Node>[] result,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent);

        /// <summary>
        ///     Output this node using writer, not outputting child nodes.
        /// </summary>
        /// <param name="indentWriter">to use for output</param>
        public abstract void Print(IndentWriter indentWriter);

        /// <summary>
        ///     Add a child node.
        /// </summary>
        /// <param name="childNode">to add</param>
        public void AddChild(BaseAssemblyNode childNode)
        {
            childNode.parentNode = this;
            childNodes.Add(childNode);
        }

        private void RecusiveAddSubstreams(IList<int> substreams)
        {
            substreams.Add(streamNum);
            foreach (var child in childNodes) {
                child.RecusiveAddSubstreams(substreams);
            }
        }

        public abstract void Result(
            EventBean[] row,
            int fromStreamNum,
            EventBean myEvent,
            Node myNode,
            ICollection<EventBean[]> resultFinalRows,
            EventBean resultRootEvent);
    }
} // end of namespace