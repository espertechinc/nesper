///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.order.OrderByProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    public class OrderByProcessorCompiler
    {
        public static void MakeOrderByProcessors(
            OrderByProcessorFactoryForge forge,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            IList<CodegenTypedParam> providerExplicitMembers,
            CodegenCtor providerCtor,
            string providerClassName,
            string memberOrderByFactory)
        {
            providerExplicitMembers.Add(new CodegenTypedParam(typeof(OrderByProcessorFactory), memberOrderByFactory));
            if (forge == null) {
                providerCtor.Block.AssignRef(memberOrderByFactory, ConstantNull());
                return;
            }

            MakeFactory(forge, classScope, innerClasses, providerClassName);
            MakeService(forge, classScope, innerClasses, providerClassName);

            providerCtor.Block.AssignRef(
                memberOrderByFactory,
                NewInstanceInner(CLASSNAME_ORDERBYPROCESSORFACTORY, Ref("this")));
        }

        private static void MakeFactory(
            OrderByProcessorFactoryForge forge,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName)
        {
            var instantiateMethod = CodegenMethod
                .MakeMethod(
                    typeof(OrderByProcessor),
                    typeof(OrderByProcessorCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(AgentInstanceContext), MEMBER_AGENTINSTANCECONTEXT.Ref);
            forge.InstantiateCodegen(instantiateMethod, classScope);
            
            var ctorParams = Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(OrderByProcessorCompiler), classScope, ctorParams);

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            
            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            var members = new List<CodegenTypedParam> {
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassName,
                    null,
                    "statementFields",
                    false,
                    false)
            };
            
            ctor.Block.AssignRef(
                Ref("this.statementFields"),
                Ref("o.statementFields"));

            // --------------------------------------------------------------------------------

            CodegenStackGenerator.RecursiveBuildStack(instantiateMethod, "Instantiate", methods, properties);
            var innerClass = new CodegenInnerClass(
                CLASSNAME_ORDERBYPROCESSORFACTORY,
                typeof(OrderByProcessorFactory),
                ctor,
                members,
                methods,
                properties);
            innerClasses.Add(innerClass);
        }

        private static void MakeService(
            OrderByProcessorFactoryForge forge,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName)
        {
            var namedMethods = new CodegenNamedMethods();

            var sortPlainMethod = CodegenMethod.MakeMethod(
                    typeof(EventBean[]),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(SORTPLAIN_PARAMS);
            forge.SortPlainCodegen(sortPlainMethod, classScope, namedMethods);

            var sortWGroupKeysMethod = CodegenMethod.MakeMethod(
                    typeof(EventBean[]),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(SORTWGROUPKEYS_PARAMS);
            forge.SortWGroupKeysCodegen(sortWGroupKeysMethod, classScope, namedMethods);

            var sortRollupMethod = CodegenMethod.MakeMethod(
                    typeof(EventBean[]),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(SORTROLLUP_PARAMS);
            forge.SortRollupCodegen(sortRollupMethod, classScope, namedMethods);

            var getSortKeyMethod = CodegenMethod
                .MakeMethod(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), REF_EPS.Ref)
                .AddParam(typeof(bool), ExprForgeCodegenNames.REF_ISNEWDATA.Ref)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    REF_EXPREVALCONTEXT.Ref);
            forge.GetSortKeyCodegen(getSortKeyMethod, classScope, namedMethods);

            var getSortKeyRollupMethod = CodegenMethod
                .MakeMethod(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), REF_EPS.Ref)
                .AddParam(typeof(bool), ExprForgeCodegenNames.REF_ISNEWDATA.Ref)
                .AddParam(typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref)
                .AddParam(
                    typeof(AggregationGroupByRollupLevel),
                    REF_ORDERROLLUPLEVEL.Ref);
            forge.GetSortKeyRollupCodegen(getSortKeyRollupMethod, classScope, namedMethods);

            var sortWOrderKeysMethod = CodegenMethod
                .MakeMethod(typeof(EventBean[]), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), REF_OUTGOINGEVENTS.Ref)
                .AddParam(typeof(object[]), REF_ORDERKEYS.Ref)
                .AddParam(typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref);
            forge.SortWOrderKeysCodegen(sortWOrderKeysMethod, classScope, namedMethods);

            var sortTwoKeysMethod = CodegenMethod.MakeMethod(
                    typeof(EventBean[]),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(SORTTWOKEYS_PARAMS);
            forge.SortTwoKeysCodegen(sortTwoKeysMethod, classScope, namedMethods);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(OrderByProcessorCompiler), classScope, ctorParams);

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            members.Add(
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassName,
                    null,
                    "statementFields",
                    false,
                    false));

            ctor.Block.AssignRef(
                Ref("this.statementFields"),
                Ref("o.statementFields"));

            // --------------------------------------------------------------------------------

            forge.CtorCodegen(ctor, members, classScope);

            CodegenClassProperties innerProperties = new CodegenClassProperties();

            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(sortPlainMethod, "SortPlain", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(sortWGroupKeysMethod, "SortWGroupKeys", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(sortRollupMethod, "SortRollup", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getSortKeyMethod, "GetSortKey", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getSortKeyRollupMethod, "GetSortKeyRollup", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(sortWOrderKeysMethod, "SortWOrderKeys", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(sortTwoKeysMethod, "SortTwoKeys", innerMethods, innerProperties);
            foreach (var methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(methodEntry.Value, methodEntry.Key, innerMethods, innerProperties);
            }

            var innerClass = new CodegenInnerClass(
                CLASSNAME_ORDERBYPROCESSOR,
                typeof(OrderByProcessor),
                ctor,
                members,
                innerMethods,
                innerProperties);
            innerClasses.Add(innerClass);
        }
    }
} // end of namespace