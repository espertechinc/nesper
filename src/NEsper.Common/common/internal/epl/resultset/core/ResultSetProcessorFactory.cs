///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.order;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    /// <summary>
    ///     Processor prototype for result sets for instances that apply the select-clause, group-by-clause and having-clauses
    ///     as supplied.
    /// </summary>
    public interface ResultSetProcessorFactory
    {
        ResultSetProcessor Instantiate(
            OrderByProcessor orderByProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace