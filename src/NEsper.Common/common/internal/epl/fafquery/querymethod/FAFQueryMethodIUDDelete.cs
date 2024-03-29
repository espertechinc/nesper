///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class FAFQueryMethodIUDDelete : FAFQueryMethodIUDBase
    {
        private QueryGraph _queryGraph;
        private ExprEvaluator _optionalWhereClause;

        protected override EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance)
        {
            return fireAndForgetProcessorInstance.ProcessDelete(this);
        }

        public override QueryGraph QueryGraph {
            get => _queryGraph;
            set => _queryGraph = value;
        }

        public ExprEvaluator OptionalWhereClause {
            get => _optionalWhereClause;
            set => _optionalWhereClause = value;
        }
    }
} // end of namespace