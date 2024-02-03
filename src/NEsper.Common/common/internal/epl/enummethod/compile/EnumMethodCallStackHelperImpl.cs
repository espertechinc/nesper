///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.enummethod.compile
{
    public class EnumMethodCallStackHelperImpl : EnumMethodCallStackHelper
    {
        private Deque<ExpressionResultCacheStackEntry> callStack;

        public void PushStack(ExpressionResultCacheStackEntry lambda)
        {
            if (callStack == null) {
                callStack = new ArrayDeque<ExpressionResultCacheStackEntry>();
            }

            callStack.AddFirst(lambda); //Push(lambda);
        }

        public bool PopLambda()
        {
            callStack.RemoveFirst(); //Remove();
            return callStack.IsEmpty();
        }

        public Deque<ExpressionResultCacheStackEntry> GetStack()
        {
            return callStack;
        }
    }
} // end of namespace