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
        private readonly EventBeanCopyMethodForge copyMethod;
        private readonly TypeWidenerSPI[] wideners;
        private readonly EventType eventType;
        private readonly Attribute[] annotations;
        private readonly ExprNode optionalWhereClause;
        private readonly string[] properties;
        private readonly ExprNode[] assignments;
        private readonly InternalEventRouterWriterForge[] writers;

        public InternalEventRouterDescForge(
            EventBeanCopyMethodForge copyMethod,
            TypeWidenerSPI[] wideners,
            EventType eventType,
            Attribute[] annotations,
            ExprNode optionalWhereClause,
            string[] properties,
            ExprNode[] assignments,
            InternalEventRouterWriterForge[] writers)
        {
            this.copyMethod = copyMethod;
            this.wideners = wideners;
            this.eventType = eventType;
            this.annotations = annotations;
            this.optionalWhereClause = optionalWhereClause;
            this.properties = properties;
            this.assignments = assignments;
            this.writers = writers;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(InternalEventRouterDesc), GetType(), classScope);
            var eventTypeExpr = EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method));
            var optionalWhereClauseExpr = optionalWhereClause == null
                ? ConstantNull()
                : ExprNodeUtilityCodegen.CodegenEvaluator(
                    optionalWhereClause.Forge,
                    method,
                    GetType(),
                    classScope);
            var assignmentsExpr = ExprNodeUtilityCodegen.CodegenEvaluators(assignments, method, GetType(), classScope);
            var writersExpr = MakeWriters(writers, method, symbols, classScope);
            
            method.Block
                .DeclareVar<InternalEventRouterDesc>("ire", NewInstance(typeof(InternalEventRouterDesc)))
                .SetProperty(Ref("ire"), "Wideners", MakeWideners(wideners, method, classScope))
                .SetProperty(Ref("ire"), "EventType", eventTypeExpr)
                .SetProperty(Ref("ire"), "OptionalWhereClauseEval", optionalWhereClauseExpr)
                .SetProperty(Ref("ire"), "Properties", Constant(properties))
                .SetProperty(Ref("ire"), "Assignments", assignmentsExpr)
                .SetProperty(Ref("ire"), "Writers", writersExpr)
                .MethodReturn(Ref("ire"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeWideners(
            TypeWidenerSPI[] wideners,
            CodegenMethod method,
            CodegenClassScope classScope)
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

        private CodegenExpression MakeWriters(
            InternalEventRouterWriterForge[] writers,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var init = new CodegenExpression[writers.Length];
            for (int i = 0; i < init.Length; i++) {
                if (writers[i] != null) {
                    init[i] = writers[i].Codegen(writers[i], method, symbols, classScope);
                }
                else {
                    init[i] = ConstantNull();
                }
            }

            return NewArrayWithInit(typeof(InternalEventRouterWriter), init);
        }
    }
} // end of namespace