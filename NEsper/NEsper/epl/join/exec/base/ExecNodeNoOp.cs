///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.exec.@base
{
    public class ExecNodeNoOp : ExecNode
    {
        public override void Process(EventBean lookupEvent, EventBean[] prefillPath, ICollection<EventBean[]> result, ExprEvaluatorContext exprEvaluatorContext)
        {
        }
    
        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("ExecNodeNoOp");
        }
    }
}
