///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.util;


namespace com.espertech.esper.epl.datetime.eval
{
    public class ExprDotNodeFilterAnalyzerDTBetweenDesc : ExprDotNodeFilterAnalyzerDesc
    {
        private readonly EventType[] typesPerStream;
        private readonly int targetStreamNum;
        private readonly String targetPropertyName;
        private readonly ExprNode start;
        private readonly ExprNode end;
        private readonly bool includeLow;
        private readonly bool includeHigh;
    
        public ExprDotNodeFilterAnalyzerDTBetweenDesc(EventType[] typesPerStream, int targetStreamNum, String targetPropertyName, ExprNode start, ExprNode end, bool includeLow, bool includeHigh) {
            this.typesPerStream = typesPerStream;
            this.targetStreamNum = targetStreamNum;
            this.targetPropertyName = targetPropertyName;
            this.start = start;
            this.end = end;
            this.includeLow = includeLow;
            this.includeHigh = includeHigh;
        }
    
        public void Apply(QueryGraph queryGraph) {
            ExprIdentNode targetExpr = ExprNodeUtility.GetExprIdentNode(typesPerStream, targetStreamNum, targetPropertyName);
            RangeFilterAnalyzer.Apply(targetExpr, start, end, includeLow, includeHigh, false, queryGraph);
        }
    }
    
}
