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
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprProcessorUtil
    {
        public static CodegenExpression MakeAnonymous(
            SelectExprProcessorForge insertHelper,
            CodegenMethod method,
            CodegenExpressionRef initSvc,
            CodegenClassScope classScope)
        {
            var resultType = classScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(insertHelper.ResultEventType, initSvc));
            var eventBeanFactory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);

            var exprSymbol = new ExprForgeCodegenSymbol(true, true);
            var selectEnv = new SelectExprProcessorCodegenSymbol();
            CodegenSymbolProvider symbolProvider = new ProxyCodegenSymbolProvider {
                ProcProvide = symbols => {
                    exprSymbol.Provide(symbols);
                    selectEnv.Provide(symbols);
                }
            };

            var processMethod = new CodegenExpressionLambda(method.Block)
                .WithParam(typeof(EventBean[]), NAME_EPS)
                .WithParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .WithParam(typeof(bool), SelectExprProcessorCodegenSymbol.NAME_ISSYNTHESIZE)
                .WithParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            var anonymousSelect = NewInstance<ProxySelectExprProcessor>(processMethod);

            //var anonymousSelect = NewAnonymousClass(method.Block, typeof(ProxySelectExprProcessor));
            //var processMethod = CodegenMethod.MakeMethod(
            //        typeof(EventBean),
            //        typeof(SelectExprProcessorUtil),
            //        symbolProvider,
            //        classScope)
            //    .AddParam(typeof(EventBean[]), NAME_EPS)
            //    .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
            //    .AddParam(typeof(bool), SelectExprProcessorCodegenSymbol.NAME_ISSYNTHESIZE)
            //    .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            //anonymousSelect.AddMethod("Process", processMethod);

            processMethod.Block.Apply(
                Instblock(
                    classScope,
                    "qSelectClause",
                    REF_EPS,
                    ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                    REF_ISSYNTHESIZE,
                    REF_EXPREVALCONTEXT));

            var performMethod = insertHelper.ProcessCodegen(
                resultType,
                eventBeanFactory,
                method, //processMethod,
                selectEnv,
                exprSymbol,
                classScope);
            exprSymbol.DerivedSymbolsCodegen(method, processMethod.Block, classScope);
            //exprSymbol.DerivedSymbolsCodegen(processMethod, processMethod.Block, classScope);
            processMethod.Block
                .DeclareVar<EventBean>("result", LocalMethod(performMethod))
                .Apply(
                    Instblock(
                        classScope,
                        "aSelectClause",
                        ResultSetProcessorCodegenNames.REF_ISNEWDATA,
                        Ref("result"),
                        ConstantNull()))
                .BlockReturn(Ref("result"));

            return anonymousSelect;
        }
    }
} // end of namespace