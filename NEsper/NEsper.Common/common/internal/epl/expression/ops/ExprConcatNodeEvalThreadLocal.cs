///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprConcatNodeEvalThreadLocal : ExprEvaluator
    {
        private readonly StringBuilder buffer;
        private readonly ExprEvaluator[] evaluators;
        private readonly IThreadLocal<StringBuilder> localBuffer;
        private readonly ExprConcatNode parent;

        public ExprConcatNodeEvalThreadLocal(
            ExprConcatNode parent,
            ExprEvaluator[] evaluators)
        {
            this.localBuffer = new SystemThreadLocal<StringBuilder>(() => new StringBuilder());
            this.buffer = localBuffer.GetOrCreate();
            this.parent = parent;
            this.evaluators = evaluators;
            buffer = localBuffer.GetOrCreate();
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            buffer.Length = 0;
            return ExprConcatNodeEvalWNew.Evaluate(eventsPerStream, isNewData, context, buffer, evaluators, parent);
        }
    }
} // end of namespace