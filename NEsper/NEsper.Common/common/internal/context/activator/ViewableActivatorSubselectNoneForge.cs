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
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorSubselectNoneForge : ViewableActivatorForge
    {
        private readonly EventType eventType;

        public ViewableActivatorSubselectNoneForge(EventType eventType)
        {
            this.eventType = eventType;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var type = classScope.AddFieldUnshared(
                true, typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(eventType, EPStatementInitServicesConstants.REF));
            var method = parent.MakeChild(typeof(ViewableActivatorSubselectNone), GetType(), classScope);
            method.Block.DeclareVar(
                    typeof(ViewableActivatorSubselectNone), "none", NewInstance(typeof(ViewableActivatorSubselectNone)))
                .ExprDotMethod(Ref("none"), "setEventType", type)
                .MethodReturn(Ref("none"));
            return LocalMethod(method);
        }
    }
} // end of namespace