///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.spec;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.join.plan
{
    public class InnerJoinGraph
    {
        private readonly int _numStreams;
        private readonly bool _isAllInnerJoin;
        private readonly ICollection<InterchangeablePair<int, int>> _innerJoins;
    
        public InnerJoinGraph(int numStreams, ICollection<InterchangeablePair<int, int>> innerJoins)
        {
            _numStreams = numStreams;
            _isAllInnerJoin = false;
            _innerJoins = innerJoins;
        }
    
        public InnerJoinGraph(int numStreams, bool isAllInnerJoin)
        {
            _numStreams = numStreams;
            _isAllInnerJoin = isAllInnerJoin;
            _innerJoins = null;
        }

        public bool IsAllInnerJoin
        {
            get { return _isAllInnerJoin; }
        }

        public bool IsEmpty()
        {
            if (_isAllInnerJoin) {
                return false;
            }
            return _innerJoins.IsEmpty();
        }
    
        public bool HasInnerJoin(int toStream)
        {
            if (_isAllInnerJoin) {
                return true;
            }
            bool hasInnerJoin = false;
            foreach (InterchangeablePair<int, int> pair in _innerJoins)
            {
                if (pair.First == toStream)
                {
                    hasInnerJoin = true;
                }
                if (pair.Second == toStream)
                {
                    hasInnerJoin = true;
                }
            }
            return hasInnerJoin;
        }
    
        public static InnerJoinGraph GraphInnerJoins(int numStreams, OuterJoinDesc[] outerJoinDescList)
        {
            if ((outerJoinDescList.Length + 1) != numStreams)
            {
                throw new ArgumentException("Number of outer join descriptors and number of streams not matching up");
            }
    
            ICollection<InterchangeablePair<int, int>> graph = new HashSet<InterchangeablePair<int, int>>();
    
            bool allInnerJoin = true;
            for (int i = 0; i < outerJoinDescList.Length; i++)
            {
                OuterJoinDesc desc = outerJoinDescList[i];
                int streamMax = i + 1;       // the outer join must references streams less then streamMax
    
                // Check outer join on-expression, if provided
                if (desc.OptLeftNode != null) {
                    int streamOne = desc.OptLeftNode.StreamId;
                    int streamTwo = desc.OptRightNode.StreamId;
    
                    if ((streamOne > streamMax) || (streamTwo > streamMax) || (streamOne == streamTwo))
                    {
                        throw new ArgumentException("Outer join descriptors reference future streams, or same streams");
                    }
    
                    if (desc.OuterJoinType == OuterJoinType.INNER)
                    {
                        graph.Add(new InterchangeablePair<int, int>(streamOne, streamTwo));
                    }
                }
    
                if (desc.OuterJoinType != OuterJoinType.INNER)
                {
                    allInnerJoin = false;
                }
            }
    
            if (allInnerJoin) {
                return new InnerJoinGraph(numStreams, true);
            }
            return new InnerJoinGraph(numStreams, graph);
        }
    
        public void AddRequiredStreams(int streamNum, ICollection<int> requiredStreams, ICollection<int> completedStreams) {
            if (_isAllInnerJoin) {
                for (int i = 0; i < _numStreams; i++) {
                    if (!completedStreams.Contains(i)) {
                        requiredStreams.Add(i);
                    }
                }
                return;
            }
    
            foreach (InterchangeablePair<int, int> pair in _innerJoins)
            {
                if (pair.First == streamNum)
                {
                    if (!completedStreams.Contains(pair.Second))
                    {
                        requiredStreams.Add(pair.Second);
                    }
                }
                if (pair.Second == streamNum)
                {
                    if (!completedStreams.Contains(pair.First))
                    {
                        requiredStreams.Add(pair.First);
                    }
                }
            }
        }
    }
}
