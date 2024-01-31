///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    public class OnSplitItemEval
    {
        public ExprEvaluator WhereClause { get; set; }

        public bool IsNamedWindowInsert { get; set; }

        public Table InsertIntoTable { get; set; }

        public ResultSetProcessorFactoryProvider RspFactoryProvider { get; set; }

        public PropertyEvaluator PropertyEvaluator { get; set; }

        public ExprEvaluator EventPrecedence { get; set; }
    }
} // end of namespace