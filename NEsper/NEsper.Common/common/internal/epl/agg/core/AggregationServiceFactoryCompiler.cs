///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.core.AggregationServiceCodegenNames;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;
using static com.espertech.esper.common.@internal.metrics.instrumentation.InstrumentationCode;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceFactoryCompiler
    {
        private static readonly IList<CodegenNamedParam> UPDPARAMS = CodegenNamedParam.From(
            typeof(EventBean[]), NAME_EPS,
            typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

        private static readonly IList<CodegenNamedParam> GETPARAMS = CodegenNamedParam.From(
            typeof(int), NAME_COLUMN,
            typeof(EventBean[]), NAME_EPS,
            typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA,
            typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);

        private static readonly IList<CodegenNamedParam> MAKESERVICEPARAMS = CodegenNamedParam.From(
            typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT,
            typeof(ImportServiceRuntime), NAME_ENGINEIMPORTSVC,
            typeof(bool), NAME_ISSUBQUERY,
            typeof(int?), NAME_SUBQUERYNUMBER,
            typeof(int[]), NAME_GROUPID);

        public static AggregationServiceFactoryMakeResult MakeInnerClassesAndInit(
            bool join,
            AggregationServiceFactoryForge forge,
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            string providerClassName,
            AggregationClassNames classNames)
        {
            if (forge is AggregationServiceFactoryForgeWMethodGen) {
                var initMethod = parent
                    .MakeChild(typeof(AggregationServiceFactory), typeof(AggregationServiceFactoryCompiler), classScope)
                    .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                var generator = (AggregationServiceFactoryForgeWMethodGen) forge;
                generator.ProviderCodegen(initMethod, classScope, classNames);

                IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();

                Consumer<AggregationRowCtorDesc> rowCtorDescConsumer =
                    rowCtorDesc => generator.RowCtorCodegen(rowCtorDesc);
                MakeRow(
                    false,
                    join,
                    generator.RowLevelDesc,
                    generator.GetType(),
                    rowCtorDescConsumer,
                    classScope,
                    innerClasses,
                    classNames);

                MakeRowFactory(
                    generator.RowLevelDesc,
                    generator.GetType(),
                    classScope,
                    innerClasses,
                    providerClassName,
                    classNames);

                BiConsumer<CodegenMethod, int> readConsumer = (
                        method,
                        level) =>
                    generator.RowReadMethodCodegen(method, level);
                BiConsumer<CodegenMethod, int> writeConsumer = (
                        method,
                        level) =>
                    generator.RowWriteMethodCodegen(method, level);
                MakeRowSerde(
                    generator.RowLevelDesc,
                    generator.GetType(),
                    readConsumer,
                    writeConsumer,
                    innerClasses,
                    classScope,
                    providerClassName,
                    classNames);

                MakeService(generator, innerClasses, classScope, providerClassName, classNames);

                MakeFactory(generator, classScope, innerClasses, providerClassName, classNames);

                return new AggregationServiceFactoryMakeResult(initMethod, innerClasses);
            }
            else {
                var symbols = new SAIFFInitializeSymbol();
                var initMethod = parent
                    .MakeChildWithScope(
                        typeof(AggregationServiceFactory),
                        typeof(AggregationServiceFactoryCompiler),
                        symbols,
                        classScope)
                    .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                var generator =
                    (AggregationServiceFactoryForgeWProviderGen) forge;
                initMethod.Block.MethodReturn(generator.MakeProvider(initMethod, symbols, classScope));
                return new AggregationServiceFactoryMakeResult(
                    initMethod,
                    Collections.GetEmptyList<CodegenInnerClass>());
            }
        }

        public static IList<CodegenInnerClass> MakeTable(
            AggregationCodegenRowLevelDesc rowLevelDesc,
            Type forgeClass,
            CodegenClassScope classScope,
            AggregationClassNames classNames,
            string providerClassName)
        {
            IList<CodegenInnerClass> innerClasses = new List<CodegenInnerClass>();
            MakeRow(
                true,
                false,
                rowLevelDesc,
                forgeClass,
                rowCtorDesc => AggregationServiceCodegenUtil.GenerateIncidentals(false, false, rowCtorDesc),
                classScope,
                innerClasses,
                classNames);

            MakeRowFactory(rowLevelDesc, forgeClass, classScope, innerClasses, providerClassName, classNames);

            BiConsumer<CodegenMethod, int> readConsumer = (
                method,
                level) => {
            };
            BiConsumer<CodegenMethod, int> writeConsumer = (
                method,
                level) => {
            };
            MakeRowSerde(
                rowLevelDesc,
                forgeClass,
                readConsumer,
                writeConsumer,
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
            var makeMethod = CodegenMethod.MakeMethod(
                typeof(AggregationRow),
                typeof(AggregationServiceFactoryCompiler),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);

            IList<CodegenTypedParam> rowCtorParams = new List<CodegenTypedParam>();
            rowCtorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(forgeClass, classScope, rowCtorParams);

            if (forgeClass == AggregationServiceNullFactory.INSTANCE.GetType()) {
                makeMethod.Block.MethodReturn(ConstantNull());
            }
            else {
                makeMethod.Block.MethodReturn(NewInstance(classNameRow));
            }

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(makeMethod, "Make", methods, properties);
            var innerClass = new CodegenInnerClass(
                classNameFactory,
                typeof(AggregationRowFactory),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods,
                properties);
            innerClasses.Add(innerClass);
        }

        private static void MakeRowSerde(
            AggregationCodegenRowLevelDesc levels,
            Type forgeClass,
            BiConsumer<CodegenMethod, int> readConsumer,
            BiConsumer<CodegenMethod, int> writeConsumer,
            IList<CodegenInnerClass> innerClasses,
            CodegenClassScope classScope,
            string providerClassName,
            AggregationClassNames classNames)
        {
            if (levels.OptionalTopRow != null) {
                MakeRowSerdeForLevel(
                    classNames.RowTop,
                    classNames.RowSerdeTop,
                    -1,
                    levels.OptionalTopRow,
                    forgeClass,
                    readConsumer,
                    writeConsumer,
                    classScope,
                    innerClasses,
                    providerClassName);
            }

            if (levels.OptionalAdditionalRows != null) {
                for (var i = 0; i < levels.OptionalAdditionalRows.Length; i++) {
                    MakeRowSerdeForLevel(
                        classNames.GetRowPerLevel(i),
                        classNames.GetRowSerdePerLevel(i),
                        i,
                        levels.OptionalAdditionalRows[i],
                        forgeClass,
                        readConsumer,
                        writeConsumer,
                        classScope,
                        innerClasses,
                        providerClassName);
                }
            }
        }

        private static void MakeRowSerdeForLevel(
            string classNameRow,
            string classNameSerde,
            int level,
            AggregationCodegenRowDetailDesc levelDesc,
            Type forgeClass,
            BiConsumer<CodegenMethod, int> readConsumer,
            BiConsumer<CodegenMethod, int> writeConsumer,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName)
        {
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(forgeClass, classScope, ctorParams);

            // Generic interface must be cast in Janino, is this true for Roslyn?
            var input = Ref("input");
            var output = Ref("output");
            var unitKey = Ref("unitKey");
            var writer = Ref("writer");
            var writeMethod = CodegenMethod.MakeMethod(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(
                    CodegenNamedParam.From(
                        typeof(object),
                        "@object",
                        typeof(DataOutput),
                        output.Ref,
                        typeof(byte[]),
                        unitKey.Ref,
                        typeof(EventBeanCollatedWriter),
                        writer.Ref))
                .AddThrown(typeof(IOException));

            var readMethod = CodegenMethod.MakeMethod(
                    typeof(object),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(CodegenNamedParam.From(typeof(DataInput), input.Ref, typeof(byte[]), unitKey.Ref))
                .AddThrown(typeof(IOException));

            if (forgeClass == AggregationServiceNullFactory.INSTANCE.GetType()) {
                readMethod.Block.MethodReturn(ConstantNull());
            }
            else {
                readMethod.Block.DeclareVar(classNameRow, "row", NewInstance(classNameRow));
                readConsumer.Invoke(readMethod, level);

                var methodFactories = levelDesc.StateDesc.MethodFactories;
                var accessStates = levelDesc.StateDesc.AccessStateForges;
                writeMethod.Block.DeclareVar(
                    classNameRow,
                    "row",
                    Cast(classNameRow, Ref("@object")));
                writeConsumer.Invoke(writeMethod, level);

                if (methodFactories != null) {
                    for (var i = 0; i < methodFactories.Length; i++) {
                        methodFactories[i]
                            .Aggregator.WriteCodegen(
                                Ref("row"),
                                i,
                                output,
                                unitKey,
                                writer,
                                writeMethod,
                                classScope);
                    }

                    for (var i = 0; i < methodFactories.Length; i++) {
                        methodFactories[i]
                            .Aggregator.ReadCodegen(
                                Ref("row"),
                                i,
                                input,
                                unitKey,
                                readMethod,
                                classScope);
                    }
                }

                if (accessStates != null) {
                    for (var i = 0; i < accessStates.Length; i++) {
                        accessStates[i]
                            .Aggregator.WriteCodegen(
                                Ref("row"),
                                i,
                                output,
                                unitKey,
                                writer,
                                writeMethod,
                                classScope);
                    }

                    for (var i = 0; i < accessStates.Length; i++) {
                        accessStates[i]
                            .Aggregator.ReadCodegen(
                                Ref("row"),
                                i,
                                input,
                                readMethod,
                                unitKey,
                                classScope);
                    }
                }

                readMethod.Block.MethodReturn(Ref("row"));
            }

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(readMethod, "Read", methods, properties);

            var innerClassInterface = typeof(DataInputOutputSerdeWCollation<>);
            var innerClass = new CodegenInnerClass(
                classNameSerde,
                innerClassInterface,
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods,
                properties);
            innerClass.InterfaceGenericClass = "object";
            //innerClass.InterfaceGenericClass = classNameRow;
            innerClasses.Add(innerClass);
        }

        private static void MakeRow(
            bool isGenerateTableEnter,
            bool isJoin,
            AggregationCodegenRowLevelDesc rowLevelDesc,
            Type forgeClass,
            Consumer<AggregationRowCtorDesc> rowCtorConsumer,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            AggregationClassNames classNames)
        {
            if (rowLevelDesc.OptionalTopRow != null) {
                MakeRowForLevel(
                    isGenerateTableEnter,
                    isJoin,
                    classNames.RowTop,
                    rowLevelDesc.OptionalTopRow,
                    forgeClass,
                    rowCtorConsumer,
                    classScope,
                    innerClasses);
            }

            if (rowLevelDesc.OptionalAdditionalRows != null) {
                for (var i = 0; i < rowLevelDesc.OptionalAdditionalRows.Length; i++) {
                    var className = classNames.GetRowPerLevel(i);
                    MakeRowForLevel(
                        isGenerateTableEnter,
                        isJoin,
                        className,
                        rowLevelDesc.OptionalAdditionalRows[i],
                        forgeClass,
                        rowCtorConsumer,
                        classScope,
                        innerClasses);
                }
            }
        }

        private static void MakeRowForLevel(
            bool isGenerateTableEnter,
            bool isJoin,
            string className,
            AggregationCodegenRowDetailDesc detail,
            Type forgeClass,
            Consumer<AggregationRowCtorDesc> rowCtorConsumer,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses)
        {
            var methodForges = detail.StateDesc.OptionalMethodForges;
            var methodFactories = detail.StateDesc.MethodFactories;
            var accessFactories = detail.StateDesc.AccessStateForges;
            var accessAccessors = detail.AccessAccessors;
            var numMethodFactories = methodFactories == null ? 0 : methodFactories.Length;

            // make member+ctor
            IList<CodegenTypedParam> rowCtorParams = new List<CodegenTypedParam>();
            var rowCtor = new CodegenCtor(forgeClass, classScope, rowCtorParams);
            var namedMethods = new CodegenNamedMethods();
            IList<CodegenTypedParam> rowMembers = new List<CodegenTypedParam>();
            rowCtorConsumer.Invoke(new AggregationRowCtorDesc(classScope, rowCtor, rowMembers, namedMethods));
            var membersColumnized = InitForgesMakeRowCtor(
                isJoin,
                rowCtor,
                classScope,
                methodFactories,
                accessFactories,
                methodForges);

            // make state-update
            var applyEnterMethod = MakeStateUpdate(
                !isGenerateTableEnter,
                AggregationCodegenUpdateType.APPLYENTER,
                methodForges,
                methodFactories,
                accessFactories,
                classScope,
                namedMethods);
            var applyLeaveMethod = MakeStateUpdate(
                !isGenerateTableEnter,
                AggregationCodegenUpdateType.APPLYLEAVE,
                methodForges,
                methodFactories,
                accessFactories,
                classScope,
                namedMethods);
            var clearMethod = MakeStateUpdate(
                !isGenerateTableEnter,
                AggregationCodegenUpdateType.CLEAR,
                methodForges,
                methodFactories,
                accessFactories,
                classScope,
                namedMethods);

            // make state-update for tables
            var enterAggMethod = MakeTableMethod(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.ENTER,
                methodFactories,
                classScope);
            var leaveAggMethod = MakeTableMethod(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.LEAVE,
                methodFactories,
                classScope);
            var enterAccessMethod = MakeTableAccess(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.ENTER,
                numMethodFactories,
                accessFactories,
                classScope,
                namedMethods);
            var leaveAccessMethod = MakeTableAccess(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.LEAVE,
                numMethodFactories,
                accessFactories,
                classScope,
                namedMethods);
            var getAccessStateMethod = MakeTableGetAccessState(
                isGenerateTableEnter,
                numMethodFactories,
                accessFactories,
                classScope);

            // make getters
            var getValueMethod = MakeGet(
                AggregationCodegenGetType.GETVALUE,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);
            var getEventBeanMethod = MakeGet(
                AggregationCodegenGetType.GETEVENTBEAN,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);
            var getCollectionScalarMethod = MakeGet(
                AggregationCodegenGetType.GETCOLLECTIONSCALAR,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);
            var getCollectionOfEventsMethod = MakeGet(
                AggregationCodegenGetType.GETCOLLECTIONOFEVENTS,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);

            var innerProperties = new CodegenClassProperties();
            var innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(applyEnterMethod, "ApplyEnter", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(applyLeaveMethod, "ApplyLeave", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(clearMethod, "Clear", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(enterAggMethod, "EnterAgg", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(leaveAggMethod, "LeaveAgg", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(enterAccessMethod, "EnterAccess", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(leaveAccessMethod, "LeaveAccess", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getAccessStateMethod, "GetAccessState", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getValueMethod, "GetValue", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getEventBeanMethod, "GetEventBean", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(getCollectionScalarMethod, "GetCollectionScalar", innerMethods, innerProperties);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionOfEventsMethod,
                "GetCollectionOfEvents",
                innerMethods,
                innerProperties);
            foreach (var methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(methodEntry.Value, methodEntry.Key, innerMethods, innerProperties);
            }

            foreach (var entry in membersColumnized.Members) {
                rowMembers.Add(new CodegenTypedParam(entry.Value, entry.Key.Ref, false, true));
            }

            var innerClass = new CodegenInnerClass(
                className,
                typeof(AggregationRow),
                rowCtor,
                rowMembers,
                innerMethods,
                innerProperties);
            innerClasses.Add(innerClass);
        }

        private static CodegenMethod MakeTableMethod(
            bool isGenerateTableEnter,
            AggregationCodegenTableUpdateType type,
            AggregationForgeFactory[] methodFactories,
            CodegenClassScope classScope)
        {
            var method = CodegenMethod
                .MakeMethod(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "column")
                .AddParam(typeof(object), "value");
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            var value = Ref("value");
            var blocks = method.Block.SwitchBlockOfLength("column", methodFactories.Length, true);
            for (var i = 0; i < methodFactories.Length; i++) {
                var factory = methodFactories[i];
                var evaluationTypes =
                    ExprNodeUtilityQuery.GetExprResultTypes(factory.AggregationExpression.PositionalParams);
                var updateMethod = method.MakeChild(typeof(void), factory.Aggregator.GetType(), classScope)
                    .AddParam(typeof(object), "value");
                if (type == AggregationCodegenTableUpdateType.ENTER) {
                    factory.Aggregator.ApplyTableEnterCodegen(value, evaluationTypes, updateMethod, classScope);
                }
                else {
                    factory.Aggregator.ApplyTableLeaveCodegen(value, evaluationTypes, updateMethod, classScope);
                }

                blocks[i].InstanceMethod(updateMethod, value).BlockReturnNoValue();
            }

            return method;
        }

        private static CodegenMethod MakeTableAccess(
            bool isGenerateTableEnter,
            AggregationCodegenTableUpdateType type,
            int offset,
            AggregationStateFactoryForge[] accessStateFactories,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var method = CodegenMethod
                .MakeMethod(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "column")
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            var colums = new int[accessStateFactories.Length];
            for (var i = 0; i < accessStateFactories.Length; i++) {
                colums[i] = offset + i;
            }

            var blocks = method.Block.SwitchBlockOptions("column", colums, true);
            for (var i = 0; i < accessStateFactories.Length; i++) {
                var stateFactoryForge = accessStateFactories[i];
                var aggregator = stateFactoryForge.Aggregator;

                var symbols = new ExprForgeCodegenSymbol(false, null);
                var updateMethod = method
                    .MakeChildWithScope(typeof(void), stateFactoryForge.GetType(), symbols, classScope)
                    .AddParam(PARAMS);
                if (type == AggregationCodegenTableUpdateType.ENTER) {
                    aggregator.ApplyEnterCodegen(updateMethod, symbols, classScope, namedMethods);
                    blocks[i].InstanceMethod(updateMethod, REF_EPS, ConstantTrue(), REF_EXPREVALCONTEXT);
                }
                else {
                    aggregator.ApplyLeaveCodegen(updateMethod, symbols, classScope, namedMethods);
                    blocks[i].InstanceMethod(updateMethod, REF_EPS, ConstantFalse(), REF_EXPREVALCONTEXT);
                }

                blocks[i].BlockReturnNoValue();
            }

            return method;
        }

        private static CodegenMethod MakeTableGetAccessState(
            bool isGenerateTableEnter,
            int offset,
            AggregationStateFactoryForge[] accessStateFactories,
            CodegenClassScope classScope)
        {
            var method = CodegenMethod.MakeMethod(
                    typeof(object),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "column");
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            var colums = new int[accessStateFactories.Length];
            for (var i = 0; i < accessStateFactories.Length; i++) {
                colums[i] = offset + i;
            }

            var blocks = method.Block.SwitchBlockOptions("column", colums, true);
            for (var i = 0; i < accessStateFactories.Length; i++) {
                var stateFactoryForge = accessStateFactories[i];
                var expr = stateFactoryForge.CodegenGetAccessTableState(i + offset, method, classScope);
                blocks[i].BlockReturn(expr);
            }

            return method;
        }

        private static CodegenMethod MakeGet(
            AggregationCodegenGetType getType,
            AggregationForgeFactory[] methodFactories,
            AggregationAccessorSlotPairForge[] accessAccessors,
            AggregationStateFactoryForge[] accessFactories,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var parent = CodegenMethod.MakeMethod(
                    getType.ReturnType,
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(GETPARAMS);

            // for non-get-value we can simply return null if this has no access aggs
            if (getType != AggregationCodegenGetType.GETVALUE && accessFactories == null) {
                parent.Block.MethodReturn(ConstantNull());
                return parent;
            }

            IList<CodegenMethod> methods = new List<CodegenMethod>();

            var count = 0;
            var numMethodStates = 0;
            if (methodFactories != null) {
                foreach (var factory in methodFactories) {
                    var method = parent.MakeChild(getType.ReturnType, factory.GetType(), classScope)
                        .AddParam(
                            CodegenNamedParam.From(
                                typeof(EventBean[]),
                                NAME_EPS,
                                typeof(bool),
                                ExprForgeCodegenNames.NAME_ISNEWDATA,
                                typeof(ExprEvaluatorContext),
                                NAME_EXPREVALCONTEXT));
                    methods.Add(method);

                    if (getType == AggregationCodegenGetType.GETVALUE) {
                        factory.Aggregator.GetValueCodegen(method, classScope);
                    }
                    else {
                        method.Block.MethodReturn(ConstantNull()); // method aggs don't do others
                    }

                    count++;
                    numMethodStates++;
                }
            }

            if (accessAccessors != null) {
                foreach (var accessorSlotPair in accessAccessors) {
                    var method = parent
                        .MakeChild(getType.ReturnType, accessorSlotPair.AccessorForge.GetType(), classScope)
                        .AddParam(
                            CodegenNamedParam.From(
                                typeof(EventBean[]),
                                NAME_EPS,
                                typeof(bool),
                                ExprForgeCodegenNames.NAME_ISNEWDATA,
                                typeof(ExprEvaluatorContext),
                                NAME_EXPREVALCONTEXT));
                    var stateNumber = numMethodStates + accessorSlotPair.Slot;

                    var ctx = new AggregationAccessorForgeGetCodegenContext(
                        stateNumber,
                        classScope,
                        accessFactories[accessorSlotPair.Slot],
                        method,
                        namedMethods);
                    if (getType == AggregationCodegenGetType.GETVALUE) {
                        accessorSlotPair.AccessorForge.GetValueCodegen(ctx);
                    }
                    else if (getType == AggregationCodegenGetType.GETEVENTBEAN) {
                        accessorSlotPair.AccessorForge.GetEnumerableEventCodegen(ctx);
                    }
                    else if (getType == AggregationCodegenGetType.GETCOLLECTIONSCALAR) {
                        accessorSlotPair.AccessorForge.GetEnumerableScalarCodegen(ctx);
                    }
                    else if (getType == AggregationCodegenGetType.GETCOLLECTIONOFEVENTS) {
                        accessorSlotPair.AccessorForge.GetEnumerableEventsCodegen(ctx);
                    }

                    methods.Add(method);
                    count++;
                }
            }

            var blocks = parent.Block.SwitchBlockOfLength("column", count, true);
            count = 0;
            foreach (var getValue in methods) {
                blocks[count++]
                    .BlockReturn(
                        LocalMethod(getValue, REF_EPS, ExprForgeCodegenNames.REF_ISNEWDATA, REF_EXPREVALCONTEXT));
            }

            return parent;
        }

        private static CodegenMethod MakeStateUpdate(
            bool isGenerate,
            AggregationCodegenUpdateType updateType,
            ExprForge[][] methodForges,
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessFactories,
            CodegenClassScope classScope,
            CodegenNamedMethods namedMethods)
        {
            var symbols = new ExprForgeCodegenSymbol(
                true,
                updateType == AggregationCodegenUpdateType.APPLYENTER);
            var parent = CodegenMethod.MakeMethod(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    symbols,
                    classScope)
                .AddParam(updateType.Params);

            var count = 0;
            IList<CodegenMethod> methods = new List<CodegenMethod>();

            if (methodFactories != null && isGenerate) {
                foreach (var factory in methodFactories) {
                    string exprText = null;
                    CodegenExpression getValue = null;
                    if (classScope.IsInstrumented) {
                        exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(
                            factory.AggregationExpression);
                        getValue = ExprDotMethod(
                            Ref("this"),
                            "GetValue",
                            Constant(count),
                            ConstantNull(),
                            ConstantTrue(),
                            ConstantNull());
                    }

                    var method = parent.MakeChild(typeof(void), factory.GetType(), classScope);
                    methods.Add(method);
                    if (updateType == AggregationCodegenUpdateType.APPLYENTER) {
                        method.Block.Apply(
                            Instblock(
                                classScope,
                                "qAggNoAccessEnterLeave",
                                ConstantTrue(),
                                Constant(count),
                                getValue,
                                Constant(exprText)));
                        factory.Aggregator.ApplyEvalEnterCodegen(method, symbols, methodForges[count], classScope);
                        method.Block.Apply(
                            Instblock(
                                classScope,
                                "aAggNoAccessEnterLeave",
                                ConstantTrue(),
                                Constant(count),
                                getValue));
                    }
                    else if (updateType == AggregationCodegenUpdateType.APPLYLEAVE) {
                        method.Block.Apply(
                            Instblock(
                                classScope,
                                "qAggNoAccessEnterLeave",
                                ConstantFalse(),
                                Constant(count),
                                getValue,
                                Constant(exprText)));
                        factory.Aggregator.ApplyEvalLeaveCodegen(method, symbols, methodForges[count], classScope);
                        method.Block.Apply(
                            Instblock(
                                classScope,
                                "aAggNoAccessEnterLeave",
                                ConstantFalse(),
                                Constant(count),
                                getValue));
                    }
                    else if (updateType == AggregationCodegenUpdateType.CLEAR) {
                        factory.Aggregator.ClearCodegen(method, classScope);
                    }

                    count++;
                }
            }

            if (accessFactories != null && isGenerate) {
                foreach (var factory in accessFactories) {
                    string exprText = null;
                    if (classScope.IsInstrumented) {
                        exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(factory.Expression);
                    }

                    var method = parent.MakeChild(typeof(void), factory.GetType(), classScope);
                    methods.Add(method);
                    if (updateType == AggregationCodegenUpdateType.APPLYENTER) {
                        method.Block.Apply(
                            Instblock(
                                classScope,
                                "qAggAccessEnterLeave",
                                ConstantTrue(),
                                Constant(count),
                                Constant(exprText)));
                        factory.Aggregator.ApplyEnterCodegen(method, symbols, classScope, namedMethods);
                        method.Block.Apply(
                            Instblock(classScope, "aAggAccessEnterLeave", ConstantTrue(), Constant(count)));
                    }
                    else if (updateType == AggregationCodegenUpdateType.APPLYLEAVE) {
                        method.Block.Apply(
                            Instblock(
                                classScope,
                                "qAggAccessEnterLeave",
                                ConstantFalse(),
                                Constant(count),
                                Constant(exprText)));
                        factory.Aggregator.ApplyLeaveCodegen(method, symbols, classScope, namedMethods);
                        method.Block.Apply(
                            Instblock(classScope, "aAggAccessEnterLeave", ConstantFalse(), Constant(count)));
                    }
                    else if (updateType == AggregationCodegenUpdateType.CLEAR) {
                        factory.Aggregator.ClearCodegen(method, classScope);
                    }

                    count++;
                }
            }

            // code for enter
            symbols.DerivedSymbolsCodegen(parent, parent.Block, classScope);
            foreach (var method in methods) {
                parent.Block.InstanceMethod(method);
            }

            return parent;
        }

        private static CodegenMemberCol InitForgesMakeRowCtor(
            bool join,
            CodegenCtor rowCtor,
            CodegenClassScope classScope,
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessFactories,
            ExprForge[][] methodForges)
        {
            var membersColumnized = new CodegenMemberCol();
            var count = 0;
            if (methodFactories != null) {
                foreach (var factory in methodFactories) {
                    factory.InitMethodForge(count, rowCtor, membersColumnized, classScope);
                    count++;
                }
            }

            if (accessFactories != null) {
                foreach (var factory in accessFactories) {
                    factory.InitAccessForge(count, join, rowCtor, membersColumnized, classScope);
                    count++;
                }
            }

            return membersColumnized;
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
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(object), NAME_GROUPKEY)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.ApplyEnterCodegen(applyEnterMethod, classScope, namedMethods, classNames);

            var applyLeaveMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(object), NAME_GROUPKEY)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.ApplyLeaveCodegen(applyLeaveMethod, classScope, namedMethods, classNames);

            var setCurrentAccessMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(object), NAME_GROUPKEY)
                .AddParam(typeof(int), NAME_AGENTINSTANCEID)
                .AddParam(
                    typeof(AggregationGroupByRollupLevel),
                    NAME_ROLLUPLEVEL);
            forge.SetCurrentAccessCodegen(setCurrentAccessMethod, classScope, classNames);

            var clearResultsMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            forge.ClearResultsCodegen(clearResultsMethod, classScope);

            var setRemovedCallbackMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AggregationRowRemovedCallback), NAME_CALLBACK);
            forge.SetRemovedCallbackCodegen(setRemovedCallbackMethod);

            var acceptMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AggregationServiceVisitor), NAME_AGGVISITOR);
            forge.AcceptCodegen(acceptMethod, classScope);

            var acceptGroupDetailMethod = CodegenMethod
                .MakeMethod(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(
                    typeof(AggregationServiceVisitorWGroupDetail),
                    NAME_AGGVISITOR);
            forge.AcceptGroupDetailCodegen(acceptGroupDetailMethod, classScope);

            var isGroupedProperty = CodegenProperty.MakePropertyNode(
                typeof(bool),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.IsGroupedCodegen(isGroupedProperty, classScope);

            var getContextPartitionAggregationServiceMethod = CodegenMethod
                .MakeMethod(
                    typeof(AggregationService),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), NAME_AGENTINSTANCEID);
            getContextPartitionAggregationServiceMethod.Block.MethodReturn(Ref("this"));

            var getValueMethod = CodegenMethod
                .MakeMethod(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), NAME_COLUMN)
                .AddParam(typeof(int), NAME_AGENTINSTANCEID)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetValueCodegen(getValueMethod, classScope, namedMethods);

            var getCollectionOfEventsMethod = CodegenMethod
                .MakeMethod(
                    typeof(ICollection<EventBean>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), NAME_COLUMN)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetCollectionOfEventsCodegen(getCollectionOfEventsMethod, classScope, namedMethods);

            var getEventBeanMethod = CodegenMethod
                .MakeMethod(typeof(EventBean), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), NAME_COLUMN)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetEventBeanCodegen(getEventBeanMethod, classScope, namedMethods);

            var getGroupKeyMethod = CodegenMethod
                .MakeMethod(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), NAME_AGENTINSTANCEID);
            forge.GetGroupKeyCodegen(getGroupKeyMethod, classScope);

            var getGroupKeysMethod = CodegenMethod
                .MakeMethod(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            forge.GetGroupKeysCodegen(getGroupKeysMethod, classScope);

            var getCollectionScalarMethod = CodegenMethod
                .MakeMethod(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), NAME_COLUMN)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetCollectionScalarCodegen(getCollectionScalarMethod, classScope, namedMethods);

            var stopMethod = CodegenMethod.MakeMethod(
                typeof(void),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.StopMethodCodegen(forge, stopMethod);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(AggregationServiceFactoryCompiler), classScope, ctorParams);
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
            var makeServiceMethod = CodegenMethod.MakeMethod(
                    typeof(AggregationService),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(MAKESERVICEPARAMS);
            forge.MakeServiceCodegen(makeServiceMethod, classScope, classNames);

            var ctorParams =
                Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            var ctor = new CodegenCtor(typeof(AggregationServiceFactoryCompiler), classScope, ctorParams);

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(makeServiceMethod, "MakeService", methods, properties);
            var innerClass = new CodegenInnerClass(
                classNames.ServiceFactory,
                typeof(AggregationServiceFactory),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods,
                properties);
            innerClasses.Add(innerClass);
        }

        private enum AggregationCodegenTableUpdateType
        {
            ENTER,
            LEAVE
        }

        private class AggregationCodegenUpdateType
        {
            public static readonly AggregationCodegenUpdateType APPLYENTER =
                new AggregationCodegenUpdateType(UPDPARAMS);

            public static readonly AggregationCodegenUpdateType APPLYLEAVE =
                new AggregationCodegenUpdateType(UPDPARAMS);

            public static readonly AggregationCodegenUpdateType CLEAR =
                new AggregationCodegenUpdateType(Collections.GetEmptyList<CodegenNamedParam>());

            private AggregationCodegenUpdateType(IList<CodegenNamedParam> @params)
            {
                Params = @params;
            }

            public IList<CodegenNamedParam> Params { get; }
        }

        private class AggregationCodegenGetType
        {
            public static readonly AggregationCodegenGetType GETVALUE =
                new AggregationCodegenGetType("GetValue", typeof(object));

            public static readonly AggregationCodegenGetType GETEVENTBEAN =
                new AggregationCodegenGetType("GetEnumerableEvent", typeof(EventBean));

            public static readonly AggregationCodegenGetType GETCOLLECTIONSCALAR =
                new AggregationCodegenGetType("GetEnumerableScalar", typeof(ICollection<object>));

            public static readonly AggregationCodegenGetType GETCOLLECTIONOFEVENTS =
                new AggregationCodegenGetType("GetEnumerableEvents", typeof(ICollection<EventBean>));

            private AggregationCodegenGetType(
                string accessorMethodName,
                Type returnType)
            {
                AccessorMethodName = accessorMethodName;
                ReturnType = returnType;
            }

            public Type ReturnType { get; }

            public string AccessorMethodName { get; }
        }
    }
} // end of namespace