///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Getter that provides an index at runtime.
    /// </summary>
    public class RegexPartitionStateRandomAccessGetter : RegexExprPreviousEvalStrategy
    {
        private readonly int[] _randomAccessIndexesRequested;
        private readonly int _maxPriorIndex;

        private RegexPartitionStateRandomAccess _randomAccess;
        private readonly bool _isUnbound;

        public RegexPartitionStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext) {
            return _randomAccess;
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="randomAccessIndexesRequested">requested indexes</param>
        /// <param name="isUnbound">true if unbound</param>
        public RegexPartitionStateRandomAccessGetter(int[] randomAccessIndexesRequested, bool isUnbound)
        {
            _randomAccessIndexesRequested = randomAccessIndexesRequested;
            _isUnbound = isUnbound;
    
            // Determine the maximum prior index to retain
            int maxPriorIndex = 0;
            foreach (int priorIndex in randomAccessIndexesRequested)
            {
                if (priorIndex > maxPriorIndex)
                {
                    maxPriorIndex = priorIndex;
                }
            }

            _maxPriorIndex = maxPriorIndex;
        }

        /// <summary>Returns max index. </summary>
        /// <value>index</value>
        public int MaxPriorIndex
        {
            get { return _maxPriorIndex; }
        }

        /// <summary>Returns indexs. </summary>
        /// <value>indexes.</value>
        public int[] IndexesRequested
        {
            get { return _randomAccessIndexesRequested; }
        }

        /// <summary>Returns length of indexes. </summary>
        /// <value>index len</value>
        public int IndexesRequestedLen
        {
            get { return _randomAccessIndexesRequested.Length; }
        }

        /// <summary>Returns true for unbound. </summary>
        /// <value>unbound indicator</value>
        public bool IsUnbound
        {
            get { return _isUnbound; }
        }

        /// <summary>Returns the index for access. </summary>
        /// <value>index</value>
        public RegexPartitionStateRandomAccess Accessor
        {
            get { return _randomAccess; }
        }

        /// <summary>Sets the random access. </summary>
        /// <value>to use</value>
        public RegexPartitionStateRandomAccess RandomAccess
        {
            set { _randomAccess = value; }
        }
    }
}
