///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.datetime.eval
{
    public class ExprDotEvalDTMethodDesc
    {
        public ExprDotEvalDTMethodDesc(ExprDotEval eval, EPType returnType, ExprDotNodeFilterAnalyzerDesc intervalFilterDesc)
        {
            Eval = eval;
            ReturnType = returnType;
            IntervalFilterDesc = intervalFilterDesc;
        }

        public ExprDotEval Eval { get; private set; }

        public EPType ReturnType { get; private set; }

        public ExprDotNodeFilterAnalyzerDesc IntervalFilterDesc { get; private set; }
    }
}
