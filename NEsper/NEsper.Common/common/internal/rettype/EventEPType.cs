///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.rettype
{
    public class EventEPType : EPType
    {
        public EventEPType(EventType type)
        {
            EventType = type;
        }

        public EventType EventType { get; }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression typeInitSvcRef)
        {
            return CodegenExpressionBuilder.NewInstance<EventEPType>(
                EventTypeUtility.ResolveTypeCodegen(EventType, typeInitSvcRef));
        }
    }
}