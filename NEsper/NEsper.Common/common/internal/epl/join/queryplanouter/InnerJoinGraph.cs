///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanouter
{
    public class InnerJoinGraph
    {
        private readonly ISet<InterchangeablePair<int, int>> innerJoins;

        private readonly int numStreams;

        public InnerJoinGraph(int numStreams, ISet<InterchangeablePair<int, int>> innerJoins)
        {
            this.numStreams = numStreams;
            IsAllInnerJoin = false;
            this.innerJoins = innerJoins;
        }

        public InnerJoinGraph(int numStreams, bool isAllInnerJoin)
        {
            this.numStreams = numStreams;
            IsAllInnerJoin = isAllInnerJoin;
            innerJoins = null;
        }

        public bool IsAllInnerJoin { get; }

        public bool IsEmpty()
        {
            if (IsAllInnerJoin) {
                return false;
            }

            return innerJoins.IsEmpty();
        }

        public bool HasInnerJoin(int toStream)
        {
            if (IsAllInnerJoin) {
                return true;
            }

            var hasInnerJoin = false;
            foreach (var pair in innerJoins) {
                if (pair.First == toStream) {
                    hasInnerJoin = true;
                }

                if (pair.Second == toStream) {
                    hasInnerJoin = true;
                }
            }

            return hasInnerJoin;
        }

        public static InnerJoinGraph GraphInnerJoins(int numStreams, OuterJoinDesc[] outerJoinDescList)
        {
            if (outerJoinDescList.Length + 1 != numStreams) {
                throw new ArgumentException("Number of outer join descriptors and number of streams not matching up");
            }

            ISet<InterchangeablePair<int, int>> graph = new HashSet<InterchangeablePair<int, int>>();

            var allInnerJoin = true;
            for (var i = 0; i < outerJoinDescList.Length; i++) {
                var desc = outerJoinDescList[i];
                var streamMax = i + 1; // the outer join must references streams less then streamMax

                // Check outer join on-expression, if provided
                if (desc.OptLeftNode != null) {
                    var streamOne = desc.OptLeftNode.StreamId;
                    var streamTwo = desc.OptRightNode.StreamId;

                    if (streamOne > streamMax || streamTwo > streamMax ||
                        streamOne == streamTwo) {
                        throw new ArgumentException("Outer join descriptors reference future streams, or same streams");
                    }

                    if (desc.OuterJoinType == OuterJoinType.INNER) {
                        graph.Add(new InterchangeablePair<int, int>(streamOne, streamTwo));
                    }
                }

                if (desc.OuterJoinType != OuterJoinType.INNER) {
                    allInnerJoin = false;
                }
            }

            if (allInnerJoin) {
                return new InnerJoinGraph(numStreams, true);
            }

            return new InnerJoinGraph(numStreams, graph);
        }

        public void AddRequiredStreams(int streamNum, ISet<int> requiredStreams, ISet<int> completedStreams)
        {
            if (IsAllInnerJoin) {
                for (var i = 0; i < numStreams; i++) {
                    if (!completedStreams.Contains(i)) {
                        requiredStreams.Add(i);
                    }
                }

                return;
            }

            foreach (var pair in innerJoins) {
                if (pair.First == streamNum) {
                    if (!completedStreams.Contains(pair.Second)) {
                        requiredStreams.Add(pair.Second);
                    }
                }

                if (pair.Second == streamNum) {
                    if (!completedStreams.Contains(pair.First)) {
                        requiredStreams.Add(pair.First);
                    }
                }
            }
        }
    }
} // end of namespace