///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;


namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalTakeLast
        : EnumEval
    {
        private readonly ExprEvaluator _sizeEval;
        private readonly int _numStreams;
    
        public EnumEvalTakeLast(ExprEvaluator sizeEval, int numStreams) {
            _sizeEval = sizeEval;
            _numStreams = numStreams;
        }

        public int StreamNumSize
        {
            get { return _numStreams; }
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context) {
    
            Object sizeObj = _sizeEval.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
            if (sizeObj == null) {
                return null;
            }
    
            if (target.IsEmpty()) {
                return target;
            }

            int size = sizeObj.AsInt();
            if (size <= 0) {
                return new object[0];
            }
    
            if (target.Count < size) {
                return target;
            }
    
            if (size == 1) {
                object last = null;
                foreach (object next in target)
                {
                    last = next;
                }
                return new object[] { last };
            }

            var result = new List<object>();
            foreach (object next in target)
            {
                result.Add(next);
                if (result.Count > size) {
                    result.RemoveAt(0);
                }
            }
            return result;
        }
    }
}
