///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationStateKeyWStream : AggregationMultiFunctionStateKey
    {
        private readonly int streamNum;
        private readonly EventType eventType;
        private readonly AggregationStateTypeWStream stateType;
        private readonly ExprNode[] criteraExprNodes;
        private readonly ExprNode filterExprNode;

        public AggregationStateKeyWStream(
            int streamNum,
            EventType eventType,
            AggregationStateTypeWStream stateType,
            ExprNode[] criteraExprNodes,
            ExprNode filterExprNode)
        {
            this.streamNum = streamNum;
            this.eventType = eventType;
            this.stateType = stateType;
            this.criteraExprNodes = criteraExprNodes;
            this.filterExprNode = filterExprNode;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            AggregationStateKeyWStream that = (AggregationStateKeyWStream) o;

            if (streamNum != that.streamNum) return false;
            if (stateType != that.stateType) return false;
            if (!ExprNodeUtilityCompare.DeepEquals(criteraExprNodes, that.criteraExprNodes, false)) return false;
            if (eventType != null) {
                if (that.eventType == null) {
                    return false;
                }

                if (!EventTypeUtility.IsTypeOrSubTypeOf(that.eventType, eventType)) return false;
            }

            if (filterExprNode == null) {
                return that.filterExprNode == null;
            }

            return that.filterExprNode != null &&
                   ExprNodeUtilityCompare.DeepEquals(filterExprNode, that.filterExprNode, false);
        }

        public override int GetHashCode()
        {
            int result = streamNum;
            result = 31 * result + stateType.GetHashCode();
            return result;
        }
    }
} // end of namespace