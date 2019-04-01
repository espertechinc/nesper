///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public abstract class EnumForgeBaseScalar : EnumForgeBase
    {
        internal readonly ObjectArrayEventType type;

        public EnumForgeBaseScalar(ExprForge innerExpression, int streamCountIncoming, ObjectArrayEventType type)
            : base(innerExpression, streamCountIncoming)
        {
            this.type = type;
        }

        public ObjectArrayEventType Type => type;
    }
} // end of namespace