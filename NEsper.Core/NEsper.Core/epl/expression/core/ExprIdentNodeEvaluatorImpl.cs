///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprIdentNodeEvaluatorImpl : ExprIdentNodeEvaluator
    {
        private readonly int _streamNum;
        private readonly EventPropertyGetter _propertyGetter;
        private readonly Type _propertyType;
        private readonly ExprIdentNode _identNode;
    
        public ExprIdentNodeEvaluatorImpl(int streamNum, EventPropertyGetter propertyGetter, Type propertyType, ExprIdentNode identNode)
        {
            _streamNum = streamNum;
            _propertyGetter = propertyGetter;
            _propertyType = propertyType; // .GetBoxedType();
            _identNode = identNode;
        }

        public virtual object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprIdent(_identNode.FullUnresolvedName);}
            EventBean theEvent = evaluateParams.EventsPerStream[_streamNum];
            if (theEvent == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprIdent(null);}
                return null;
            }
            if (InstrumentationHelper.ENABLED) {
                Object result = _propertyGetter.Get(theEvent);
                InstrumentationHelper.Get().AExprIdent(result);
                return result;
            }
    
            return _propertyGetter.Get(theEvent);
        }

        public Type ReturnType
        {
            get { return _propertyType; }
        }

        public EventPropertyGetter Getter
        {
            get { return _propertyGetter; }
        }

        /// <summary>Returns true if the property exists, or false if not. </summary>
        /// <param name="eventsPerStream">each stream's events</param>
        /// <param name="isNewData">if the stream represents insert or remove stream</param>
        /// <returns>true if the property exists, false if not</returns>
        public bool EvaluatePropertyExists(EventBean[] eventsPerStream, bool isNewData)
        {
            EventBean theEvent = eventsPerStream[_streamNum];
            if (theEvent == null)
            {
                return false;
            }
            return _propertyGetter.IsExistsProperty(theEvent);
        }

        public int StreamNum
        {
            get { return _streamNum; }
        }

        public bool IsContextEvaluated
        {
            get { return false; }
        }
    }
}
