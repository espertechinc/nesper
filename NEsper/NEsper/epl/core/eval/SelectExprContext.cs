///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class SelectExprContext
    {
        public SelectExprContext(ExprEvaluator[] expressionNodes, string[] columnNames, EventAdapterService eventAdapterService)
        {
            ExpressionNodes = expressionNodes;
            ColumnNames = columnNames;
            EventAdapterService = eventAdapterService;
        }

        public ExprEvaluator[] ExpressionNodes { get; set; }

        public string[] ColumnNames { get; private set; }

        public EventAdapterService EventAdapterService { get; private set; }
    }
} // end of namespace
