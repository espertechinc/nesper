///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprIdentNodeEvaluatorContext : ExprIdentNodeEvaluator
    {
        private readonly int _streamNum;
        private readonly Type _resultType;
        private readonly EventPropertyGetter _getter;
    
        public ExprIdentNodeEvaluatorContext(int streamNum, Type resultType, EventPropertyGetter getter)
        {
            _streamNum = streamNum;
            _resultType = resultType;
            _getter = getter;
        }
    
        public bool EvaluatePropertyExists(EventBean[] eventsPerStream, bool isNewData)
        {
            return true;
        }

        public int StreamNum
        {
            get { return _streamNum; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var context = evaluateParams.ExprEvaluatorContext;
            if (context.ContextProperties != null)
            {
                return _getter.Get(context.ContextProperties);
            }
            return null;
        }

        public Type ReturnType
        {
            get { return _resultType; }
        }

        public EventPropertyGetter Getter
        {
            get { return _getter; }
        }

        public bool IsContextEvaluated
        {
            get { return true; }
        }
    }
}
