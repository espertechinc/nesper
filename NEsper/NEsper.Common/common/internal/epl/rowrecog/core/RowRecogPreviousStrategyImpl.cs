///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    ///     Getter that provides an index at runtime.
    /// </summary>
    public class RowRecogPreviousStrategyImpl : RowRecogPreviousStrategy
    {
        private readonly bool isUnbound;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="randomAccessIndexesRequested">requested indexes</param>
        /// <param name="isUnbound">true if unbound</param>
        public RowRecogPreviousStrategyImpl(
            int[] randomAccessIndexesRequested,
            bool isUnbound)
        {
            IndexesRequested = randomAccessIndexesRequested;
            this.isUnbound = isUnbound;

            // Determine the maximum prior index to retain
            var maxPriorIndex = 0;
            foreach (var priorIndex in randomAccessIndexesRequested) {
                if (priorIndex > maxPriorIndex) {
                    maxPriorIndex = priorIndex;
                }
            }

            MaxPriorIndex = maxPriorIndex;
        }

        /// <summary>
        ///     Returns max index.
        /// </summary>
        /// <returns>index</returns>
        public int MaxPriorIndex { get; }

        /// <summary>
        ///     Returns length of indexes.
        /// </summary>
        /// <returns>index len</returns>
        public int IndexesRequestedLen => IndexesRequested.Length;

        /// <summary>
        ///     Returns the index for access.
        /// </summary>
        /// <returns>index</returns>
        public RowRecogStateRandomAccess Accessor { get; private set; }

        /// <summary>
        ///     Sets the random access.
        /// </summary>
        /// <value>to use</value>
        public RowRecogStateRandomAccess RandomAccess {
            get => Accessor;
            set => Accessor = value;
        }

        /// <summary>
        ///     Returns indexs.
        /// </summary>
        /// <value>indexes.</value>
        public int[] IndexesRequested { get; }

        public RowRecogStateRandomAccess GetAccess(ExprEvaluatorContext exprEvaluatorContext)
        {
            return Accessor;
        }

        /// <summary>
        ///     Returns true for unbound.
        /// </summary>
        /// <returns>unbound indicator</returns>
        public bool IsUnbound()
        {
            return isUnbound;
        }
    }
} // end of namespace