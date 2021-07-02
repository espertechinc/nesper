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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class BindSelectExprProcessorForge : SelectExprProcessorForge
    {
        private readonly SelectExprProcessorForge _syntheticProcessorForge;
        private readonly BindProcessorForge _bindProcessorForge;

        public BindSelectExprProcessorForge(
            SelectExprProcessorForge syntheticProcessorForge,
            BindProcessorForge bindProcessorForge)
        {
            this._syntheticProcessorForge = syntheticProcessorForge;
            this._bindProcessorForge = bindProcessorForge;
        }

        public EventType ResultEventType {
            get => _syntheticProcessorForge.ResultEventType;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod processMethod = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);

            CodegenExpressionRef isSynthesize = selectSymbol.GetAddSynthesize(processMethod);
            CodegenMethod syntheticMethod = _syntheticProcessorForge.ProcessCodegen(
                resultEventType,
                eventBeanFactory,
                processMethod,
                selectSymbol,
                exprSymbol,
                codegenClassScope);
            CodegenMethod bindMethod = _bindProcessorForge.ProcessCodegen(processMethod, exprSymbol, codegenClassScope);
            CodegenExpression isNewData = exprSymbol.GetAddIsNewData(processMethod);
            CodegenExpression exprCtx = exprSymbol.GetAddExprEvalCtx(processMethod);

            var stmtResultSvc = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(StatementResultService),
                ExprDotName(
                    EPStatementInitServicesConstants.REF,
                    EPStatementInitServicesConstants.STATEMENTRESULTSERVICE));

            processMethod.Block
                .DeclareVar<bool>("makeNatural", ExprDotName(stmtResultSvc, "IsMakeNatural"))
                .DeclareVar<bool>("synthesize", Or(isSynthesize, ExprDotName(stmtResultSvc, "IsMakeSynthetic")))
                .IfCondition(Not(Ref("makeNatural")))
                .IfCondition(Ref("synthesize"))
                .DeclareVar<EventBean>("synthetic", LocalMethod(syntheticMethod))
                .BlockReturn(Ref("synthetic"))
                .BlockReturn(ConstantNull())
                .DeclareVar<EventBean>("syntheticEvent", ConstantNull())
                .IfCondition(Ref("synthesize"))
                .AssignRef("syntheticEvent", LocalMethod(syntheticMethod))
                .BlockEnd()
                .DeclareVar<object[]>("parameters", LocalMethod(bindMethod))
                .MethodReturn(NewInstance<NaturalEventBean>(resultEventType, Ref("parameters"), Ref("syntheticEvent")));

            return processMethod;
        }
    }
} // end of namespace