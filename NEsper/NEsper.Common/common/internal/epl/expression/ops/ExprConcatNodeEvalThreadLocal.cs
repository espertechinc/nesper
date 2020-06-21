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
        private readonly StringBuilder _buffer;
        private readonly ExprEvaluator[] _evaluators;
        private readonly IThreadLocal<StringBuilder> _localBuffer;
        private readonly ExprConcatNode _parent;

        public ExprConcatNodeEvalThreadLocal(
            ExprConcatNode parent,
            ExprEvaluator[] evaluators)
        {
            _localBuffer = new SystemThreadLocal<StringBuilder>(() => new StringBuilder());
            _buffer = _localBuffer.GetOrCreate();
            this._parent = parent;
            this._evaluators = evaluators;
            _buffer = _localBuffer.GetOrCreate();
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            _buffer.Length = 0;
            return ExprConcatNodeEvalWNew.Evaluate(eventsPerStream, isNewData, context, _buffer, _evaluators, _parent);
        }
    }
} // end of namespace