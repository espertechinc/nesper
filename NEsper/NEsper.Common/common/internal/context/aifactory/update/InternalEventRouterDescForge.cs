///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class InternalEventRouterDescForge
    {
        private readonly Attribute[] annotations;
        private readonly ExprNode[] assignments;
        private readonly EventBeanCopyMethodForge copyMethod;
        private readonly EventType eventType;
        private readonly ExprNode optionalWhereClause;
        private readonly string[] properties;
        private readonly TypeWidenerSPI[] wideners;

        public InternalEventRouterDescForge(
            EventBeanCopyMethodForge copyMethod, TypeWidenerSPI[] wideners, EventType eventType,
            Attribute[] annotations, ExprNode optionalWhereClause, string[] properties, ExprNode[] assignments)
        {
            this.copyMethod = copyMethod;
            this.wideners = wideners;
            this.eventType = eventType;
            this.annotations = annotations;
            this.optionalWhereClause = optionalWhereClause;
            this.properties = properties;
            this.assignments = assignments;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(InternalEventRouterDesc), GetType(), classScope);
            method.Block
                .DeclareVar(typeof(InternalEventRouterDesc), "ire", NewInstance(typeof(InternalEventRouterDesc)))
                .ExprDotMethod(Ref("ire"), "setWideners", MakeWideners(wideners, method, classScope))
                .ExprDotMethod(
                    Ref("ire"), "setEventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .ExprDotMethod(
                    Ref("ire"), "setOptionalWhereClauseEval",
                    optionalWhereClause == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluator(
                            optionalWhereClause.Forge, method, GetType(), classScope))
                .ExprDotMethod(Ref("ire"), "setProperties", Constant(properties))
                .ExprDotMethod(
                    Ref("ire"), "setAssignments",
                    ExprNodeUtilityCodegen.CodegenEvaluators(assignments, method, GetType(), classScope))
                .MethodReturn(Ref("ire"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeWideners(
            TypeWidenerSPI[] wideners, CodegenMethod method, CodegenClassScope classScope)
        {
            var init = new CodegenExpression[wideners.Length];
            for (var i = 0; i < init.Length; i++) {
                if (wideners[i] != null) {
                    init[i] = TypeWidenerFactory.CodegenWidener(wideners[i], method, GetType(), classScope);
                }
                else {
                    init[i] = ConstantNull();
                }
            }

            return NewArrayWithInit(typeof(TypeWidener), init);
        }
    }
} // end of namespace