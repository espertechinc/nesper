///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public abstract class EnumForgeBaseWFields : EnumForge
    {
        public EnumForgeBaseWFields(
            ExprForge innerExpression,
            int streamNumLambda,
            ObjectArrayEventType fieldEventType)
        {
            InnerExpression = innerExpression;
            StreamNumLambda = streamNumLambda;
            FieldEventType = fieldEventType;
        }

        public EnumForgeBaseWFields(
            ExprDotEvalParamLambda lambda,
            ObjectArrayEventType fieldEventType)
            : this(lambda.BodyForge, lambda.StreamCountIncoming, fieldEventType)
        {
        }

        public ExprForge InnerExpression { get; }

        public int StreamNumLambda { get; }

        public ObjectArrayEventType FieldEventType { get; }

        public int StreamNumSize => StreamNumLambda + 2;
    }
} // end of namespace