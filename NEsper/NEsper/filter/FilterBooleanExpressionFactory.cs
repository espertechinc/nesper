///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.filter
{
    public interface FilterBooleanExpressionFactory
    {
        ExprNodeAdapterBase Make(
            FilterSpecParamExprNode filterSpecParamExprNode,
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContext statementContext,
            int agentInstanceId);
    }
} // end of namespace