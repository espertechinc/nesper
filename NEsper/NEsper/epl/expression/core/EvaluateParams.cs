///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.epl.expression.core
{
    public struct EvaluateParams
    {
        public static readonly EvaluateParams EmptyTrue = new EvaluateParams(null, true, null);
        public static readonly EvaluateParams EmptyFalse = new EvaluateParams(null, false, null);

#if false
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly bool _isNewData;
#endif

        public EvaluateParams(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventsPerStream = eventsPerStream;
            IsNewData = isNewData;
            ExprEvaluatorContext = exprEvaluatorContext;
        }

        public readonly EventBean[] EventsPerStream;
        public readonly bool IsNewData;
        public readonly ExprEvaluatorContext ExprEvaluatorContext;
    }
}
