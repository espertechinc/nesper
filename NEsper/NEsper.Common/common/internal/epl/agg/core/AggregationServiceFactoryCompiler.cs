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
            typeof(EventBean[]),
            NAME_EPS,
            typeof(ExprEvaluatorContext),
            NAME_EXPREVALCONTEXT);

        private static readonly IList<CodegenNamedParam> GETPARAMS = CodegenNamedParam.From(
            typeof(int),
            AggregationServiceCodegenNames.NAME_COLUMN,
            typeof(EventBean[]),
            NAME_EPS,
            typeof(bool),
            ExprForgeCodegenNames.NAME_ISNEWDATA,
            typeof(ExprEvaluatorContext),
            NAME_EXPREVALCONTEXT);

        private static readonly IList<CodegenNamedParam> MAKESERVICEPARAMS = CodegenNamedParam.From(
            typeof(AgentInstanceContext),
            NAME_AGENTINSTANCECONTEXT,
            typeof(ImportServiceRuntime),
            AggregationServiceCodegenNames.NAME_ENGINEIMPORTSVC,
            typeof(bool),
            AggregationServiceCodegenNames.NAME_ISSUBQUERY,
            typeof(int?),
            AggregationServiceCodegenNames.NAME_SUBQUERYNUMBER,
            typeof(int[]),
            NAME_GROUPID);

        public static AggregationServiceFactoryMakeResult MakeInnerClassesAndInit(
            bool join,
            AggregationServiceFactoryForge forge,
            CodegenMethodScope parent,
            CodegenClassScope classScope,
            string providerClassName,
            AggregationClassNames classNames)
        {
            if (forge is AggregationServiceFactoryForgeWMethodGen) {
                CodegenMethod initMethod = parent
                    .MakeChild(typeof(AggregationServiceFactory), typeof(AggregationServiceFactoryCompiler), classScope)
                    .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                AggregationServiceFactoryForgeWMethodGen generator = (AggregationServiceFactoryForgeWMethodGen) forge;
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
                SAIFFInitializeSymbol symbols = new SAIFFInitializeSymbol();
                CodegenMethod initMethod = parent
                    .MakeChildWithScope(
                        typeof(AggregationServiceFactory),
                        typeof(AggregationServiceFactoryCompiler),
                        symbols,
                        classScope)
                    .AddParam(typeof(EPStatementInitServices), EPStatementInitServicesConstants.REF.Ref);
                AggregationServiceFactoryForgeWProviderGen generator =
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
                for (int i = 0; i < rowLevelDesc.OptionalAdditionalRows.Length; i++) {
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
            CodegenMethod makeMethod = CodegenMethod.MakeParentNode(
                typeof(AggregationRow),
                typeof(AggregationServiceFactoryCompiler),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);

            IList<CodegenTypedParam> rowCtorParams = new List<CodegenTypedParam>();
            rowCtorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            CodegenCtor ctor = new CodegenCtor(forgeClass, classScope, rowCtorParams);

            if (forgeClass == AggregationServiceNullFactory.INSTANCE.GetType()) {
                makeMethod.Block.MethodReturn(ConstantNull());
            }
            else {
                makeMethod.Block.MethodReturn(CodegenExpressionBuilder.NewInstance(classNameRow));
            }

            CodegenClassMethods methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(makeMethod, "make", methods);
            CodegenInnerClass innerClass = new CodegenInnerClass(
                classNameFactory,
                typeof(AggregationRowFactory),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods);
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
                for (int i = 0; i < levels.OptionalAdditionalRows.Length; i++) {
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
            CodegenCtor ctor = new CodegenCtor(forgeClass, classScope, ctorParams);

            // generic interface must still cast in Janino
            CodegenExpressionRef input = CodegenExpressionBuilder.Ref("input");
            CodegenExpressionRef output = CodegenExpressionBuilder.Ref("output");
            CodegenExpressionRef unitKey = CodegenExpressionBuilder.Ref("unitKey");
            CodegenExpressionRef writer = CodegenExpressionBuilder.Ref("writer");
            CodegenMethod writeMethod = CodegenMethod.MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(
                    CodegenNamedParam.From(
                        typeof(object),
                        "object",
                        typeof(DataOutput),
                        output.Ref,
                        typeof(byte[]),
                        unitKey.Ref,
                        typeof(EventBeanCollatedWriter),
                        writer.Ref))
                .AddThrown(typeof(IOException));

            CodegenMethod readMethod = CodegenMethod.MakeParentNode(
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
                readMethod.Block.DeclareVar(classNameRow, "row", CodegenExpressionBuilder.NewInstance(classNameRow));
                readConsumer.Invoke(readMethod, level);

                AggregationForgeFactory[] methodFactories = levelDesc.StateDesc.MethodFactories;
                AggregationStateFactoryForge[] accessStates = levelDesc.StateDesc.AccessStateForges;
                writeMethod.Block.DeclareVar(
                    classNameRow,
                    "row",
                    Cast(classNameRow, CodegenExpressionBuilder.Ref("object")));
                writeConsumer.Invoke(writeMethod, level);

                if (methodFactories != null) {
                    for (int i = 0; i < methodFactories.Length; i++) {
                        methodFactories[i]
                            .Aggregator.WriteCodegen(
                                CodegenExpressionBuilder.Ref("row"),
                                i,
                                output,
                                unitKey,
                                writer,
                                writeMethod,
                                classScope);
                    }

                    for (int i = 0; i < methodFactories.Length; i++) {
                        methodFactories[i]
                            .Aggregator.ReadCodegen(
                                CodegenExpressionBuilder.Ref("row"),
                                i,
                                input,
                                unitKey,
                                readMethod,
                                classScope);
                    }
                }

                if (accessStates != null) {
                    for (int i = 0; i < accessStates.Length; i++) {
                        accessStates[i]
                            .Aggregator.WriteCodegen(
                                CodegenExpressionBuilder.Ref("row"),
                                i,
                                output,
                                unitKey,
                                writer,
                                writeMethod,
                                classScope);
                    }

                    for (int i = 0; i < accessStates.Length; i++) {
                        accessStates[i]
                            .Aggregator.ReadCodegen(
                                CodegenExpressionBuilder.Ref("row"),
                                i,
                                input,
                                readMethod,
                                unitKey,
                                classScope);
                    }
                }

                readMethod.Block.MethodReturn(CodegenExpressionBuilder.Ref("row"));
            }

            CodegenClassMethods methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods);
            CodegenStackGenerator.RecursiveBuildStack(readMethod, "Read", methods);

            CodegenInnerClass innerClass = new CodegenInnerClass(
                classNameSerde,
                typeof(DataInputOutputSerdeWCollation<object>),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods);
            innerClass.InterfaceGenericClass = classNameRow;
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
                for (int i = 0; i < rowLevelDesc.OptionalAdditionalRows.Length; i++) {
                    string className = classNames.GetRowPerLevel(i);
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
            ExprForge[][] methodForges = detail.StateDesc.OptionalMethodForges;
            AggregationForgeFactory[] methodFactories = detail.StateDesc.MethodFactories;
            AggregationStateFactoryForge[] accessFactories = detail.StateDesc.AccessStateForges;
            AggregationAccessorSlotPairForge[] accessAccessors = detail.AccessAccessors;
            int numMethodFactories = methodFactories == null ? 0 : methodFactories.Length;

            // make member+ctor
            IList<CodegenTypedParam> rowCtorParams = new List<CodegenTypedParam>();
            CodegenCtor rowCtor = new CodegenCtor(forgeClass, classScope, rowCtorParams);
            CodegenNamedMethods namedMethods = new CodegenNamedMethods();
            IList<CodegenTypedParam> rowMembers = new List<CodegenTypedParam>();
            rowCtorConsumer.Invoke(new AggregationRowCtorDesc(classScope, rowCtor, rowMembers, namedMethods));
            CodegenMemberCol membersColumnized = InitForgesMakeRowCtor(
                isJoin,
                rowCtor,
                classScope,
                methodFactories,
                accessFactories,
                methodForges);

            // make state-update
            CodegenMethod applyEnterMethod = MakeStateUpdate(
                !isGenerateTableEnter,
                AggregationCodegenUpdateType.APPLYENTER,
                methodForges,
                methodFactories,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod applyLeaveMethod = MakeStateUpdate(
                !isGenerateTableEnter,
                AggregationCodegenUpdateType.APPLYLEAVE,
                methodForges,
                methodFactories,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod clearMethod = MakeStateUpdate(
                !isGenerateTableEnter,
                AggregationCodegenUpdateType.CLEAR,
                methodForges,
                methodFactories,
                accessFactories,
                classScope,
                namedMethods);

            // make state-update for tables
            CodegenMethod enterAggMethod = MakeTableMethod(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.ENTER,
                methodFactories,
                classScope);
            CodegenMethod leaveAggMethod = MakeTableMethod(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.LEAVE,
                methodFactories,
                classScope);
            CodegenMethod enterAccessMethod = MakeTableAccess(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.ENTER,
                numMethodFactories,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod leaveAccessMethod = MakeTableAccess(
                isGenerateTableEnter,
                AggregationCodegenTableUpdateType.LEAVE,
                numMethodFactories,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod getAccessStateMethod = MakeTableGetAccessState(
                isGenerateTableEnter,
                numMethodFactories,
                accessFactories,
                classScope);

            // make getters
            CodegenMethod getValueMethod = MakeGet(
                AggregationCodegenGetType.GETVALUE,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod getEventBeanMethod = MakeGet(
                AggregationCodegenGetType.GETEVENTBEAN,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod getCollectionScalarMethod = MakeGet(
                AggregationCodegenGetType.GETCOLLECTIONSCALAR,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);
            CodegenMethod getCollectionOfEventsMethod = MakeGet(
                AggregationCodegenGetType.GETCOLLECTIONOFEVENTS,
                methodFactories,
                accessAccessors,
                accessFactories,
                classScope,
                namedMethods);

            CodegenClassMethods innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(applyEnterMethod, "applyEnter", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(applyLeaveMethod, "applyLeave", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(clearMethod, "clear", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(enterAggMethod, "enterAgg", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(leaveAggMethod, "leaveAgg", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(enterAccessMethod, "enterAccess", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(leaveAccessMethod, "leaveAccess", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(getAccessStateMethod, "getAccessState", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(getValueMethod, "getValue", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(getEventBeanMethod, "getEventBean", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(getCollectionScalarMethod, "getCollectionScalar", innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionOfEventsMethod,
                "getCollectionOfEvents",
                innerMethods);
            foreach (KeyValuePair<string, CodegenMethod> methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(methodEntry.Value, methodEntry.Key, innerMethods);
            }

            foreach (KeyValuePair<CodegenExpressionRefWCol, Type> entry in membersColumnized.Members) {
                rowMembers.Add(new CodegenTypedParam(entry.Value, entry.Key.Ref, false, true));
            }

            CodegenInnerClass innerClass = new CodegenInnerClass(
                className,
                typeof(AggregationRow),
                rowCtor,
                rowMembers,
                innerMethods);
            innerClasses.Add(innerClass);
        }

        private static CodegenMethod MakeTableMethod(
            bool isGenerateTableEnter,
            AggregationCodegenTableUpdateType type,
            AggregationForgeFactory[] methodFactories,
            CodegenClassScope classScope)
        {
            CodegenMethod method = CodegenMethod
                .MakeParentNode(
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

            CodegenExpressionRef value = CodegenExpressionBuilder.Ref("value");
            CodegenBlock[] blocks = method.Block.SwitchBlockOfLength("column", methodFactories.Length, true);
            for (int i = 0; i < methodFactories.Length; i++) {
                AggregationForgeFactory factory = methodFactories[i];
                Type[] evaluationTypes =
                    ExprNodeUtilityQuery.GetExprResultTypes(factory.AggregationExpression.PositionalParams);
                CodegenMethod updateMethod = method.MakeChild(typeof(void), factory.Aggregator.GetType(), classScope)
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
            CodegenMethod method = CodegenMethod
                .MakeParentNode(
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

            int[] colums = new int[accessStateFactories.Length];
            for (int i = 0; i < accessStateFactories.Length; i++) {
                colums[i] = offset + i;
            }

            CodegenBlock[] blocks = method.Block.SwitchBlockOptions("column", colums, true);
            for (int i = 0; i < accessStateFactories.Length; i++) {
                AggregationStateFactoryForge stateFactoryForge = accessStateFactories[i];
                AggregatorAccess aggregator = stateFactoryForge.Aggregator;

                ExprForgeCodegenSymbol symbols = new ExprForgeCodegenSymbol(false, null);
                CodegenMethod updateMethod = method
                    .MakeChildWithScope(typeof(void), stateFactoryForge.GetType(), symbols, classScope)
                    .AddParam(ExprForgeCodegenNames.PARAMS);
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
            CodegenMethod method = CodegenMethod.MakeParentNode(
                    typeof(object),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), "column");
            if (!isGenerateTableEnter) {
                method.Block.MethodThrowUnsupported();
                return method;
            }

            int[] colums = new int[accessStateFactories.Length];
            for (int i = 0; i < accessStateFactories.Length; i++) {
                colums[i] = offset + i;
            }

            CodegenBlock[] blocks = method.Block.SwitchBlockOptions("column", colums, true);
            for (int i = 0; i < accessStateFactories.Length; i++) {
                AggregationStateFactoryForge stateFactoryForge = accessStateFactories[i];
                CodegenExpression expr = stateFactoryForge.CodegenGetAccessTableState(i + offset, method, classScope);
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
            CodegenMethod parent = CodegenMethod.MakeParentNode(
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

            int count = 0;
            int numMethodStates = 0;
            if (methodFactories != null) {
                foreach (AggregationForgeFactory factory in methodFactories) {
                    CodegenMethod method = parent.MakeChild(getType.ReturnType, factory.GetType(), classScope)
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
                foreach (AggregationAccessorSlotPairForge accessorSlotPair in accessAccessors) {
                    CodegenMethod method = parent
                        .MakeChild(getType.ReturnType, accessorSlotPair.AccessorForge.GetType(), classScope)
                        .AddParam(
                            CodegenNamedParam.From(
                                typeof(EventBean[]),
                                NAME_EPS,
                                typeof(bool),
                                ExprForgeCodegenNames.NAME_ISNEWDATA,
                                typeof(ExprEvaluatorContext),
                                NAME_EXPREVALCONTEXT));
                    int stateNumber = numMethodStates + accessorSlotPair.Slot;

                    AggregationAccessorForgeGetCodegenContext ctx = new AggregationAccessorForgeGetCodegenContext(
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

            CodegenBlock[] blocks = parent.Block.SwitchBlockOfLength("column", count, true);
            count = 0;
            foreach (CodegenMethod getValue in methods) {
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
            ExprForgeCodegenSymbol symbols = new ExprForgeCodegenSymbol(
                true,
                updateType == AggregationCodegenUpdateType.APPLYENTER);
            CodegenMethod parent = CodegenMethod.MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    symbols,
                    classScope)
                .AddParam(updateType.Params);

            int count = 0;
            IList<CodegenMethod> methods = new List<CodegenMethod>();

            if (methodFactories != null && isGenerate) {
                foreach (AggregationForgeFactory factory in methodFactories) {
                    string exprText = null;
                    CodegenExpression getValue = null;
                    if (classScope.IsInstrumented) {
                        exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(
                            factory.AggregationExpression);
                        getValue = ExprDotMethod(
                            CodegenExpressionBuilder.Ref("this"),
                            "getValue",
                            Constant(count),
                            ConstantNull(),
                            ConstantTrue(),
                            ConstantNull());
                    }

                    CodegenMethod method = parent.MakeChild(typeof(void), factory.GetType(), classScope);
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
                foreach (AggregationStateFactoryForge factory in accessFactories) {
                    string exprText = null;
                    if (classScope.IsInstrumented) {
                        exprText = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(factory.Expression);
                    }

                    CodegenMethod method = parent.MakeChild(typeof(void), factory.GetType(), classScope);
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
            foreach (CodegenMethod method in methods) {
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
            CodegenMemberCol membersColumnized = new CodegenMemberCol();
            int count = 0;
            if (methodFactories != null) {
                foreach (AggregationForgeFactory factory in methodFactories) {
                    factory.InitMethodForge(count, rowCtor, membersColumnized, classScope);
                    count++;
                }
            }

            if (accessFactories != null) {
                foreach (AggregationStateFactoryForge factory in accessFactories) {
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
            CodegenNamedMethods namedMethods = new CodegenNamedMethods();

            CodegenMethod applyEnterMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(object), AggregationServiceCodegenNames.NAME_GROUPKEY)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.ApplyEnterCodegen(applyEnterMethod, classScope, namedMethods, classNames);

            CodegenMethod applyLeaveMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(object), AggregationServiceCodegenNames.NAME_GROUPKEY)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.ApplyLeaveCodegen(applyLeaveMethod, classScope, namedMethods, classNames);

            CodegenMethod setCurrentAccessMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(object), AggregationServiceCodegenNames.NAME_GROUPKEY)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_AGENTINSTANCEID)
                .AddParam(
                    typeof(AggregationGroupByRollupLevel),
                    AggregationServiceCodegenNames.NAME_ROLLUPLEVEL);
            forge.SetCurrentAccessCodegen(setCurrentAccessMethod, classScope, classNames);

            CodegenMethod clearResultsMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            forge.ClearResultsCodegen(clearResultsMethod, classScope);

            CodegenMethod setRemovedCallbackMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AggregationRowRemovedCallback), AggregationServiceCodegenNames.NAME_CALLBACK);
            forge.SetRemovedCallbackCodegen(setRemovedCallbackMethod);

            CodegenMethod acceptMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(AggregationServiceVisitor), AggregationServiceCodegenNames.NAME_AGGVISITOR);
            forge.AcceptCodegen(acceptMethod, classScope);

            CodegenMethod acceptGroupDetailMethod = CodegenMethod
                .MakeParentNode(typeof(void), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(
                    typeof(AggregationServiceVisitorWGroupDetail),
                    AggregationServiceCodegenNames.NAME_AGGVISITOR);
            forge.AcceptGroupDetailCodegen(acceptGroupDetailMethod, classScope);

            CodegenMethod isGroupedMethod = CodegenMethod.MakeParentNode(
                typeof(bool),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.IsGroupedCodegen(isGroupedMethod, classScope);

            CodegenMethod getContextPartitionAggregationServiceMethod = CodegenMethod
                .MakeParentNode(
                    typeof(AggregationService),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_AGENTINSTANCEID);
            getContextPartitionAggregationServiceMethod.Block.MethodReturn(CodegenExpressionBuilder.Ref("this"));

            CodegenMethod getValueMethod = CodegenMethod
                .MakeParentNode(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_COLUMN)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_AGENTINSTANCEID)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetValueCodegen(getValueMethod, classScope, namedMethods);

            CodegenMethod getCollectionOfEventsMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_COLUMN)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetCollectionOfEventsCodegen(getCollectionOfEventsMethod, classScope, namedMethods);

            CodegenMethod getEventBeanMethod = CodegenMethod
                .MakeParentNode(typeof(EventBean), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_COLUMN)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetEventBeanCodegen(getEventBeanMethod, classScope, namedMethods);

            CodegenMethod getGroupKeyMethod = CodegenMethod
                .MakeParentNode(typeof(object), forge.GetType(), CodegenSymbolProviderEmpty.INSTANCE, classScope)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_AGENTINSTANCEID);
            forge.GetGroupKeyCodegen(getGroupKeyMethod, classScope);

            CodegenMethod getGroupKeysMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(ExprEvaluatorContext), NAME_EXPREVALCONTEXT);
            forge.GetGroupKeysCodegen(getGroupKeysMethod, classScope);

            CodegenMethod getCollectionScalarMethod = CodegenMethod
                .MakeParentNode(
                    typeof(ICollection<object>),
                    forge.GetType(),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(int), AggregationServiceCodegenNames.NAME_COLUMN)
                .AddParam(typeof(EventBean[]), NAME_EPS)
                .AddParam(typeof(bool), ExprForgeCodegenNames.NAME_ISNEWDATA)
                .AddParam(
                    typeof(ExprEvaluatorContext),
                    NAME_EXPREVALCONTEXT);
            forge.GetCollectionScalarCodegen(getCollectionScalarMethod, classScope, namedMethods);

            CodegenMethod stopMethod = CodegenMethod.MakeParentNode(
                typeof(void),
                forge.GetType(),
                CodegenSymbolProviderEmpty.INSTANCE,
                classScope);
            forge.StopMethodCodegen(forge, stopMethod);

            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            IList<CodegenTypedParam> ctorParams = new List<CodegenTypedParam>();
            ctorParams.Add(new CodegenTypedParam(providerClassName, "o"));
            CodegenCtor ctor = new CodegenCtor(typeof(AggregationServiceFactoryCompiler), classScope, ctorParams);
            forge.CtorCodegen(ctor, members, classScope, classNames);

            CodegenClassMethods innerMethods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(
                applyEnterMethod,
                "applyEnter",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                applyLeaveMethod,
                "applyLeave",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                setCurrentAccessMethod,
                "SetCurrentAccess",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                clearResultsMethod,
                "clearResults",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                setRemovedCallbackMethod,
                "setRemovedCallback",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                acceptMethod,
                "accept",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                acceptGroupDetailMethod,
                "acceptGroupDetail",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                isGroupedMethod,
                "isGrouped",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getContextPartitionAggregationServiceMethod,
                "getContextPartitionAggregationService",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getValueMethod,
                "getValue",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionOfEventsMethod,
                "getCollectionOfEvents",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getEventBeanMethod,
                "getEventBean",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getGroupKeyMethod,
                "getGroupKey",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getGroupKeysMethod,
                "getGroupKeys",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                getCollectionScalarMethod,
                "getCollectionScalar",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                stopMethod,
                "stop",
                innerMethods);
            CodegenStackGenerator.RecursiveBuildStack(
                ctor,
                "ctor",
                innerMethods);
            foreach (KeyValuePair<string, CodegenMethod> methodEntry in namedMethods.Methods) {
                CodegenStackGenerator.RecursiveBuildStack(methodEntry.Value, methodEntry.Key, innerMethods);
            }

            CodegenInnerClass innerClass = new CodegenInnerClass(
                classNames.Service,
                typeof(AggregationService),
                ctor,
                members,
                innerMethods);
            innerClasses.Add(innerClass);
        }

        private static void MakeFactory(
            AggregationServiceFactoryForgeWMethodGen forge,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName,
            AggregationClassNames classNames)
        {
            CodegenMethod makeServiceMethod = CodegenMethod.MakeParentNode(
                    typeof(AggregationService),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(MAKESERVICEPARAMS);
            forge.MakeServiceCodegen(makeServiceMethod, classScope, classNames);

            IList<CodegenTypedParam> ctorParams =
                Collections.SingletonList(new CodegenTypedParam(providerClassName, "o"));
            CodegenCtor ctor = new CodegenCtor(typeof(AggregationServiceFactoryCompiler), classScope, ctorParams);

            CodegenClassMethods methods = new CodegenClassMethods();
            CodegenStackGenerator.RecursiveBuildStack(makeServiceMethod, "MakeService", methods);
            CodegenInnerClass innerClass = new CodegenInnerClass(
                classNames.ServiceFactory,
                typeof(AggregationServiceFactory),
                ctor,
                Collections.GetEmptyList<CodegenTypedParam>(),
                methods);
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
                this.Params = @params;
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
                new AggregationCodegenGetType("GetEnumerableEvents", typeof(ICollection<object>));

            AggregationCodegenGetType(
                string accessorMethodName,
                Type returnType)
            {
                this.AccessorMethodName = accessorMethodName;
                this.ReturnType = returnType;
            }

            public Type ReturnType { get; }

            public string AccessorMethodName { get; }
        }
    }
} // end of namespace