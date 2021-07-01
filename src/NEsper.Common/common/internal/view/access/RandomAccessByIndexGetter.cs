///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.access
{
    /// <summary>
    ///     Getter that provides an index at runtime.
    /// </summary>
    public class RandomAccessByIndexGetter : RandomAccessByIndexObserver,
        PreviousGetterStrategy
    {
        /// <summary>
        ///     Returns the index for access.
        /// </summary>
        /// <returns>index</returns>
        public RandomAccessByIndex Accessor { get; private set; }

        public PreviousGetterStrategy GetStrategy(ExprEvaluatorContext ctx)
        {
            return this;
        }

        public void Updated(RandomAccessByIndex randomAccessByIndex)
        {
            Accessor = randomAccessByIndex;
        }
    }
} // end of namespace