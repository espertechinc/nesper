///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    /// <summary>
    ///     Table of events allowing add and remove. Lookup in table is coordinated
    ///     through the underlying implementation.
    /// </summary>
    public interface EventTableFactory
    {
        Type EventTableClass { get; }

        EventTable[] MakeEventTables(
            ExprEvaluatorContext exprEvaluatorContext,
            int? subqueryNumber);

        string ToQueryPlan();
    }
} // end of namespace