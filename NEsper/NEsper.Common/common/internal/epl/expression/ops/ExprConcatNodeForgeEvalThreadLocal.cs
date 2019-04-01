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

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprConcatNodeForgeEvalThreadLocal : ExprEvaluator
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly ExprConcatNodeForge _form;

        private readonly IThreadLocal<StringBuilder> _localBuffer = new SystemThreadLocal<StringBuilder>(
            () => new StringBuilder());

        public ExprConcatNodeForgeEvalThreadLocal(ExprConcatNodeForge forge, ExprEvaluator[] evaluators)
        {
            _form = forge;
            _evaluators = evaluators;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var buffer = _localBuffer.GetOrCreate();
            buffer.Length = 0;
            return ExprConcatNodeForgeEvalWNew.Evaluate(
                eventsPerStream, isNewData, context, buffer, _evaluators, _form);
        }
    }
} // end of namespace