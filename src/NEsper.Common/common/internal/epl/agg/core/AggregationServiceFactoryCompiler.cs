///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceFactoryCompilerRow;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceFactoryCompilerSerde;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceFactoryCompiler
    {
        internal static readonly IList<CodegenNamedParam> MAKESERVICEPARAMS = CodegenNamedParam.From(
            typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT,
            typeof(int?), NAME_STREAMNUM,
            typeof(int?), NAME_SUBQUERYNUMBER,
            typeof(int[]), NAME_GROUPID);

        internal static readonly IList<CodegenNamedParam> UPDPARAMS = CodegenNamedParam.From(
            typeof(EventBean[]), NAME_EPS,
            typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

        internal static readonly IList<CodegenNamedParam> GETPARAMS = CodegenNamedParam.From(
            typeof(int), NAME_VCOL,
            typeof(EventBean[]), NAME_EPS,
            typeof(bool), NAME_ISNEWDATA,
            typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

        private static readonly CodegenExpressionRef STATEMENT_FIELDS_REF = Ref("statementFields");
        private static readonly CodegenExpressionRef STATEMENT_FIELDS_FIELD_REF = Ref("this.statementFields");
        private static readonly CodegenExpressionRef STATEMENT_FIELDS_PARAM_REF = Ref("o.statementFields");

        public static AggregationServiceFactoryMakeResult MakeInnerClassesAndInit(
            AggregationServiceFactoryForge forge,
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            string providerClassName,
            AggregationClassNames classNames,
            bool isTargetHA)
        {
            if (forge is AggregationServiceFactoryForgeWMethodGen gen) {
                var initMethod = parent
                    .MakeChild(typeof(AggregationServiceFactory), typeof(AggregationServiceFactoryCompiler), classScope)
                    .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);
                var generator = (AggregationServiceFactoryForgeWMethodGen)forge;
                gen.ProviderCodegen(initMethod, classScope, classNames);

                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                var assignments = MakeRow(
                    false,
                    gen.RowLevelDesc,
                    gen.GetType(),
                    rowCtorDesc => generator.RowCtorCodegen(rowCtorDesc),
                    classScope,
                    innerClasses,
                    classNames);

                MakeRowFactory(
                    gen.RowLevelDesc,
                    gen.GetType(),
                    classScope,
                    innerClasses,
                    providerClassName,
                    classNames);

                MakeRowSerde<AggregationRow>(
                    isTargetHA,
                    assignments,
                    gen.GetType(),
                    (method, level) => generator.RowReadMethodCodegen(method, level),
                    (method, level) => generator.RowWriteMethodCodegen(method, level),
                    innerClasses,
                    classScope,
                    providerClassName,
                    classNames);

                MakeService(gen, innerClasses, classScope, providerClassName, classNames);

                MakeFactory(gen, classScope, innerClasses, providerClassName, classNames);

                return new AggregationServiceFactoryMakeResult(initMethod, innerClasses);
            }

            var symbols = new SAIFFInitializeSymbol();
            var initMethodX = parent
                .MakeChildWithScope(
                    typeof(AggregationServiceFactory),
                    typeof(AggregationServiceFactoryCompiler),
                    symbols,
                    classScope)
                .AddParam<EPStatementInitServices>(EPStatementInitServicesConstants.REF.Ref);

            var generatorX = (AggregationServiceFactoryForgeWProviderGen) forge;
            initMethodX.Block.MethodReturn(generatorX.MakeProvider(initMethodX, symbols, classScope));
            return new AggregationServiceFactoryMakeResult(initMethodX, EmptyList<CodegenInnerClass>.Instance);
        }

        public static IList<CodegenInnerClass> MakeTable(
            AggregationCodegenRowLevelDesc rowLevelDesc,
            Type forgeClass,
            CodegenClassScope classScope,
            AggregationClassNames classNames,
            string providerClassName,
            bool isTargetHA)
        {
            IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();
            var assignments = MakeRow(
                true,
                rowLevelDesc,
                forgeClass,
                rowCtorDesc => AggregationServiceCodegenUtil.GenerateIncidentals(false, false, rowCtorDesc),
                classScope,
                innerClasses,
                classNames);

            MakeRowFactory(rowLevelDesc, forgeClass, classScope, innerClasses, providerClassName, classNames);

            MakeRowSerde<AggregationRow>(
                isTargetHA,
                assignments,
                forgeClass,
                (method, level) => { },
                (method, level) => { },
                innerClasses,
                classScope,
                providerClassName,
                classNames);
            return innerClasses;
        }

        private static void MakeRowFactory(
            AggregationCodegenRowLevelDesc rowLevelDesc,
            Type forgeClass,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName,
            AggregationClassNames classNames)
        {
            if (rowLevelDesc.OptionalTopRow != null) {
                MakeRowFactoryForLevel(
                    classNames.RowTop,
                    classNames.RowFactoryTop,
                    forgeClass,
                    classScope,
                    innerClasses,
                    providerClassName);
            }

            if (rowLevelDesc.OptionalAdditionalRows != null) {
                for (var i = 0; i < rowLevelDesc.OptionalAdditionalRows.Length; i++) {
                    MakeRowFactoryForLevel(
                        classNames.GetRowPerLevel(i),
                        classNames.GetRowFactoryPerLevel(i),
                        forgeClass,
                        classScope,
                        innerClasses,
                        providerClassName);
                }
            }
        }

        private static void MakeRowFactoryForLevel(
            string classNameRow,
            string classNameFactory,
            Type forgeClass,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName)
        {
            var makeMethod = CodegenMethod.MakeParentNode(
                typeof(AggregationRow),
                typeof(AggregationServiceFactoryCompiler),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);

            IList<CodegenTypedParam> rowCtorParams = new List<CodegenTypedParam>();
            rowCtorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(forgeClass, classScope, rowCtorParams);

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            var members = new List<CodegenTypedParam> {
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassNameOptional,
                    null,
                    "statementFields",
                    false,
                    false)
            };

            ctor.Block.AssignRef(
                STATEMENT_FIELDS_FIELD_REF,
                STATEMENT_FIELDS_PARAM_REF);

            // --------------------------------------------------------------------------------
            
            if (forgeClass == AggregationServiceNullFactory.INSTANCE.GetType()) {
                makeMethod.Block.MethodReturn(ConstantNull());
            }
            else {
                makeMethod.Block.MethodReturn(NewInstanceInner(classNameRow, Ref("statementFields")));
            }

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(makeMethod, "Make", methods, properties);
            var innerClass = new CodegenInnerClass(
                classNameFactory,
                typeof(AggregationRowFactory),
                ctor,
                members,
                methods,
                properties);
            innerClasses.Add(innerClass);
        }

        private static void MakeService(
            AggregationServiceFactoryForgeWMethodGen forge,
            IList<CodegenInnerClass> innerClasses,
            CodegenClassScope classScope,
            string providerClassName,
            AggregationClassNames classNames)
        {
            var namedMethods = new CodegenNamedMethods();

            var applyEnterMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<object>(NAME_GROUPKEY)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.ApplyEnterCodegen(applyEnterMethod, classScope, namedMethods, classNames);

            var applyLeaveMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<object>(NAME_GROUPKEY)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.ApplyLeaveCodegen(applyLeaveMethod, classScope, namedMethods, classNames);

            var setCurrentAccessMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<object>(NAME_GROUPKEY)
                .AddParam(typeof(int), NAME_AGENTINSTANCEID)
                .AddParam<AggregationGroupByRollupLevel>(NAME_ROLLUPLEVEL);
            forge.SetCurrentAccessCodegen(setCurrentAccessMethod, classScope, classNames);

            var clearResultsMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.ClearResultsCodegen(clearResultsMethod, classScope);

            var setRemovedCallbackMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<AggregationRowRemovedCallback>(NAME_CALLBACK);
            forge.SetRemovedCallbackCodegen(setRemovedCallbackMethod);

            var acceptMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<AggregationServiceVisitor>(NAME_AGGVISITOR);
            forge.AcceptCodegen(acceptMethod, classScope);

            var acceptGroupDetailMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<AggregationServiceVisitorWGroupDetail>(NAME_AGGVISITOR);
            forge.AcceptGroupDetailCodegen(acceptGroupDetailMethod, classScope);

            var isGroupedProperty = CodegenProperty.MakePropertyNode(
                typeof(bool),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.IsGroupedCodegen(isGroupedProperty, classScope);

            var getContextPartitionAggregationServiceMethod = CodegenMethod
                .MakeParentNode(
                    typeof(AggregationService),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), NAME_AGENTINSTANCEID);
            getContextPartitionAggregationServiceMethod.Block.MethodReturn(Ref("this"));

            var getValueMethod = CodegenMethod
                .MakeParentNode(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), NAME_VCOL)
                .AddParam<int>(NAME_AGENTINSTANCEID)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.GetValueCodegen(getValueMethod, classScope, namedMethods);

            var getCollectionOfEventsMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ICollection<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_VCOL)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.GetCollectionOfEventsCodegen(getCollectionOfEventsMethod, classScope, namedMethods);

            var getEventBeanMethod = CodegenMethod
                .MakeParentNode(typeof(EventBean), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam<int>(NAME_VCOL)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.GetEventBeanCodegen(getEventBeanMethod, classScope, namedMethods);

            var getRowMethod = CodegenMethod
                .MakeParentNode(
                    typeof(AggregationRow),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_AGENTINSTANCEID)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.GetRowCodegen(getRowMethod, classScope, namedMethods);

            var getGroupKeyMethod = CodegenMethod
                .MakeParentNode(
                    typeof(object),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_AGENTINSTANCEID);
            forge.GetGroupKeyCodegen(getGroupKeyMethod, classScope);

            var getGroupKeysMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.GetGroupKeysCodegen(getGroupKeysMethod, classScope);

            var getCollectionScalarMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<int>(NAME_VCOL)
                .AddParam<EventBean[]>(NAME_EPS)
                .AddParam<bool>(NAME_ISNEWDATA)
                .AddParam<ExprEvaluatorContext>(NAME_EXPREVALCONTEXT);
            forge.GetCollectionScalarCodegen(getCollectionScalarMethod, classScope, namedMethods);

            var stopMethod = CodegenMethod
                .MakeParentNode(
                typeof(void),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.StopMethodCodegen(forge, stopMethod);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(AggregationServiceFactoryCompiler), classScope, ctorParams);

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            members.Add(
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassNameOptional,
                    null,
                    "statementFields",
                    false,
                    false));

            ctor.Block.AssignRef(
                STATEMENT_FIELDS_FIELD_REF,
                STATEMENT_FIELDS_PARAM_REF);

            // --------------------------------------------------------------------------------

            forge.CtorCodegen(ctor, members, classScope, classNames);

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(
                applyEnterMethod,
                "ApplyEnter",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                applyLeaveMethod,
                "ApplyLeave",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                setCurrentAccessMethod,
                "SetCurrentAccess",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                clearResultsMethod,
                "ClearResults",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                setRemovedCallbackMethod,
                "SetRemovedCallback",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                acceptMethod,
                "Accept",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                acceptGroupDetailMethod,
                "AcceptGroupDetail",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                isGroupedProperty,
                "IsGrouped",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getContextPartitionAggregationServiceMethod,
                "GetContextPartitionAggregationService",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getValueMethod,
                "GetValue",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionOfEventsMethod,
                "GetCollectionOfEvents",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getEventBeanMethod,
                "GetEventBean",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getRowMethod,
                "GetAggregationRow",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getGroupKeyMethod,
                "GetGroupKey",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getGroupKeysMethod,
                "GetGroupKeys",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionScalarMethod,
                "GetCollectionScalar",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                stopMethod,
                "Stop",
                innerMethods,
                innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                ctor,
                "ctor",
                innerMethods,
                innerProperties);
            foreach (var methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(
                    methodEntry.Value,
                    methodEntry.Key,
                    innerMethods,
                    innerProperties);
            }

            var innerClass = new CodegenInnerClass(
                classNames.Service,
                typeof(AggregationService),
                ctor,
                members,
                innerMethods,
                innerProperties);
            innerClasses.Add(innerClass);
        }

        private static void MakeFactory(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName,
            AggregationClassNames classNames)
        {
            var makeServiceMethod = CodegenMethod.MakeParentNode(
                    typeof(AggregationService),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(MAKESERVICEPARAMS);
            forge.MakeServiceCodegen(makeServiceMethod, classScope, classNames);

            var ctorParams = Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(AggregationServiceFactoryCompiler), classScope, ctorParams);

            // --------------------------------------------------------------------------------
            // Add statementFields
            // --------------------------------------------------------------------------------

            var members = new List<CodegenTypedParam> {
                new CodegenTypedParam(
                    classScope.NamespaceScope.FieldsClassNameOptional,
                    null,
                    "statementFields",
                    false,
                    false)
            };

            ctor.Block.AssignRef(
                STATEMENT_FIELDS_FIELD_REF,
                STATEMENT_FIELDS_PARAM_REF);

            // --------------------------------------------------------------------------------

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(makeServiceMethod, "MakeService", methods, properties);
            var innerClass = new CodegenInnerClass(
                classNames.ServiceFactory,
                typeof(AggregationServiceFactory),
                ctor,
                members,
                methods,
                properties);
            
            innerClasses.Add(innerClass);
        }
    }
} // end of namespace