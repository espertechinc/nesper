///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationStateKeyWStream : AggregationStateKey
    {
        private readonly int _streamNum;
        private readonly EventType _eventType;
        private readonly AggregationStateTypeWStream _stateType;
        private readonly ExprNode[] _exprNodes;
    
        public AggregationStateKeyWStream(int streamNum, EventType eventType, AggregationStateTypeWStream stateType, ExprNode[] exprNodes)
        {
            _streamNum = streamNum;
            _eventType = eventType;
            _stateType = stateType;
            _exprNodes = exprNodes;
        }
    
        public override bool Equals(Object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
    
            AggregationStateKeyWStream that = (AggregationStateKeyWStream) o;
    
            if (_streamNum != that._streamNum) return false;
            if (_stateType != that._stateType) return false;
            if (!ExprNodeUtility.DeepEquals(_exprNodes, that._exprNodes)) return false;
            if (_eventType != null)
            {
                if (that._eventType == null)
                    return false;

                if (!EventTypeUtility.IsTypeOrSubTypeOf(that._eventType, _eventType)) 
                    return false;
            }
    
            return true;
        }
    
        public override int GetHashCode()
        {
            int result = _streamNum;
            result = 31 * result + _stateType.GetHashCode();
            return result;
        }
    }
}
