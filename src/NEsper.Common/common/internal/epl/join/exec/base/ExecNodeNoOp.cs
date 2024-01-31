///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    public class ExecNodeNoOp : ExecNode
    {
        public static readonly ExecNodeNoOp INSTANCE = new ExecNodeNoOp();

        private ExecNodeNoOp()
        {
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("ExecNodeNoOp");
        }
    }
} // end of namespace