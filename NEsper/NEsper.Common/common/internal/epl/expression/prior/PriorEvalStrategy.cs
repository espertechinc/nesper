///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.prior
{
    /// <summary>
    /// Represents the 'prior' prior event resolution strategy for use in an expression node tree.
    /// </summary>
    public interface PriorEvalStrategy
    {
        EventBean GetSubstituteEvent(
            EventBean originalEvent,
            bool isNewData,
            int constantIndexNumber,
            int relativeIndex,
            ExprEvaluatorContext exprEvaluatorContext,
            int streamNum);
    }
} // end of namespace