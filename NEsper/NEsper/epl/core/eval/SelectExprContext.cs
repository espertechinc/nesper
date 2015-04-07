///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core.eval
{
    public class SelectExprContext
    {
        public SelectExprContext(ExprEvaluator[] expressionNodes,
                                 String[] columnNames,
                                 EventAdapterService eventAdapterService)
        {
            ExpressionNodes = expressionNodes;
            ColumnNames = columnNames;
            EventAdapterService = eventAdapterService;
        }

        public ExprEvaluator[] ExpressionNodes { get; private set; }

        public string[] ColumnNames { get; private set; }

        public EventAdapterService EventAdapterService { get; private set; }
    }
}