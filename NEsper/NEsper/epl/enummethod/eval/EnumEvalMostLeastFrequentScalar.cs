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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;


namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalMostLeastFrequentScalar
        : EnumEvalBase
        , EnumEval
    {
        private readonly bool _isMostFrequent;
    
        public EnumEvalMostLeastFrequentScalar(int streamCountIncoming, bool isMostFrequent) 
            : base(streamCountIncoming)
        {
            _isMostFrequent = isMostFrequent;
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            IDictionary<Object, int?> items = new LinkedHashMap<Object, int?>();
    
            foreach (Object next in target) {
                int? existing = items.Get(next);
                if (existing == null) {
                    existing = 1;
                }
                else {
                    existing++;
                }
                items.Put(next, existing);
            }
    
            return EnumEvalMostLeastFrequentEvent.GetResult(items, _isMostFrequent);
        }
    }
}
