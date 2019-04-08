///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableEvalLockUtil
    {
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="lock">lock</param>
        /// <param name="exprEvaluatorContext">ctx</param>
        public static void ObtainLockUnless(
            ILockable @lock,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ObtainLockUnless(@lock, exprEvaluatorContext.TableExprEvaluatorContext);
        }

        public static void ObtainLockUnless(
            ILockable @lock,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            bool added = tableExprEvaluatorContext.AddAcquiredLock(@lock);
            if (added) {
                @lock.Lock();
            }
        }
    }
} // end of namespace