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
    public class EvaluateParams
    {
        private readonly EventBean[] _eventsPerStream;

        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly bool _isNewData;

        public EvaluateParams(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream = eventsPerStream;

            _isNewData = isNewData;

            _exprEvaluatorContext = exprEvaluatorContext;
        }

        public EventBean[] EventsPerStream
        {
            get { return _eventsPerStream; }
        }

        public bool IsNewData
        {
            get { return _isNewData; }
        }

        public ExprEvaluatorContext ExprEvaluatorContext
        {
            get { return _exprEvaluatorContext; }
        }

        public static readonly EvaluateParams Empty = new EvaluateParams(null, false, null);
    }
}
