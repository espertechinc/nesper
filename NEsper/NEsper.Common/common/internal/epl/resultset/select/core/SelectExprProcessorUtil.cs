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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprProcessorUtil
    {
        public static CodegenExpressionNewAnonymousClass MakeAnonymous(
            SelectExprProcessorForge insertHelper, 
            CodegenMethod method,
            CodegenExpressionRef initSvc,
            CodegenClassScope classScope)
        {
            var resultType = classScope.AddFieldUnshared(
                true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(insertHelper.ResultEventType, initSvc));
            var eventBeanFactory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);

            var exprSymbol = new ExprForgeCodegenSymbol(true, true);
            var selectEnv = new SelectExprProcessorCodegenSymbol();
            CodegenSymbolProvider symbolProvider = new ProxyCodegenSymbolProvider {
                ProcProvide = symbols => {
                    exprSymbol.Provide(symbols);
                    selectEnv.Provide(symbols);
                }
            };

            var anonymousSelect = NewAnonymousClass(method.Block, typeof(SelectExprProcessor));
            var processMethod = CodegenMethod.MakeParentNode(
                    typeof(EventBean), typeof(SelectExprProcessorUtil), symbolProvider, classScope)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(typeof(bool), SelectExprProcessorCodegenSymbol.NAME_ISSYNTHESIZE)
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            anonymousSelect.AddMethod("process", processMethod);
            processMethod.Block.Apply(
                Instblock(classScope, "qSelectClause", REF_EPS, REF_ISNEWDATA, REF_ISSYNTHESIZE, REF_EXPREVALCONTEXT));

            var performMethod = insertHelper.ProcessCodegen(
                resultType, eventBeanFactory, processMethod, selectEnv, exprSymbol, classScope);
            exprSymbol.DerivedSymbolsCodegen(processMethod, processMethod.Block, classScope);
            processMethod.Block
                .DeclareVar(typeof(EventBean), "result", LocalMethod(performMethod))
                .Apply(Instblock(classScope, "aSelectClause", REF_ISNEWDATA, Ref("result"), ConstantNull()))
                .MethodReturn(Ref("result"));

            return anonymousSelect;
        }
    }
} // end of namespace