///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationServiceFactoryCompilerSerde
    {
        private static readonly CodegenExpressionRef INPUT = Ref("input");
        private static readonly CodegenExpressionRef OUTPUT = Ref("output");
        private static readonly CodegenExpressionRef UNIT_KEY = Ref("unitKey");
        private static readonly CodegenExpressionRef WRITER = Ref("writer");

        internal static void MakeRowSerde<T>(
            bool isTargetHA,
            AggregationClassAssignmentPerLevel assignments,
            Type forgeClass,
            BiConsumer<CodegenMethod, int> readConsumer,
            BiConsumer<CodegenMethod, int> writeConsumer,
            IList<CodegenInnerClass> innerClasses,
            CodegenClassScope classScope,
            string providerClassName,
            AggregationClassNames classNames)
        {
            if (assignments.OptionalTop != null) {
                MakeRowSerdeForLevel<T>(
                    isTargetHA,
                    assignments.OptionalTop,
                    classNames.RowTop,
                    classNames.RowSerdeTop,
                    -1,
                    forgeClass,
                    readConsumer,
                    writeConsumer,
                    classScope,
                    innerClasses,
                    providerClassName);
            }

            if (assignments.OptionalPerLevel != null) {
                for (var i = 0; i < assignments.OptionalPerLevel.Length; i++) {
                    MakeRowSerdeForLevel<T>(
                        isTargetHA,
                        assignments.OptionalPerLevel[i],
                        classNames.GetRowPerLevel(i),
                        classNames.GetRowSerdePerLevel(i),
                        i,
                        forgeClass,
                        readConsumer,
                        writeConsumer,
                        classScope,
                        innerClasses,
                        providerClassName);
                }
            }
        }

        private static void MakeRowSerdeForLevel<T>(
            bool isTargetHA,
            AggregationClassAssignment[] assignments,
            string classNameRow,
            string classNameSerde,
            int level,
            Type forgeClass,
            BiConsumer<CodegenMethod, int> readConsumer,
            BiConsumer<CodegenMethod, int> writeConsumer,
            CodegenClassScope classScope,
            IList<CodegenInnerClass> innerClasses,
            string providerClassName)
        {
            // make flat
            if (assignments.Length == 1 ||
                !isTargetHA ||
                forgeClass == AggregationServiceNullFactory.INSTANCE.GetType()) {
                var inner = MakeRowSerdeForLevel<T>(
                    isTargetHA,
                    assignments[0],
                    classNameRow,
                    classNameSerde,
                    level,
                    forgeClass,
                    readConsumer,
                    writeConsumer,
                    classScope,
                    providerClassName);
                inner.BaseList.AssignType(typeof(DataInputOutputSerdeBase<T>));
                //inner.AddInterfaceImplemented(typeof(DataInputOutputSerde<T>));
                innerClasses.Add(inner);
                return;
            }

            // make leafs
            var classNamesSerde = new string[assignments.Length];
            for (var i = 0; i < assignments.Length; i++) {
                classNamesSerde[i] = classNameSerde + "_" + i;
                var inner = MakeRowSerdeForLevel<T>(
                    isTargetHA,
                    assignments[i],
                    assignments[i].ClassName,
                    classNamesSerde[i],
                    level,
                    forgeClass,
                    readConsumer,
                    writeConsumer,
                    classScope,
                    providerClassName);
                innerClasses.Add(inner);
            }

            // make members
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>(classNameSerde.Length);
            for (var i = 0; i < assignments.Length; i++) {
                members.Add(new CodegenTypedParam(classNamesSerde[i], "s" + i));
            }

            // make ctor
            var ctor = MakeCtor(forgeClass, providerClassName, classScope);
            for (var i = 0; i < assignments.Length; i++) {
                ctor.Block.AssignRef("s" + i, NewInstanceInner(classNamesSerde[i], Ref("o")));
            }

            // make write
            var writeMethod = MakeWriteMethod<T>(classScope);
            for (var i = 0; i < assignments.Length; i++) {
                writeMethod.Block.ExprDotMethod(Ref("s" + i), "Write", Ref("@object"), OUTPUT, UNIT_KEY, WRITER);
            }

            // make read
            var readMethod = MakeReadMethod<T>(classNameRow, classScope);
            readMethod.Block.DeclareVar(classNameRow, "r", NewInstanceInner(classNameRow));
            for (var i = 0; i < assignments.Length; i++) {
                readMethod.Block.AssignRef("r." + "l" + i, ExprDotMethod(Ref("s" + i), "ReadValue", INPUT, UNIT_KEY));
            }

            readMethod.Block.MethodReturn(Ref("r"));

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(readMethod, "ReadValue", methods, properties);

            var serde = new CodegenInnerClass(
                classNameSerde,
                typeof(DataInputOutputSerde),
                ctor,
                members,
                methods,
                properties);
            innerClasses.Add(serde);
        }

        private static CodegenInnerClass MakeRowSerdeForLevel<T>(
            bool isTargetHA,
            AggregationClassAssignment assignment,
            string classNameRow,
            string classNameSerde,
            int level,
            Type forgeClass,
            BiConsumer<CodegenMethod, int> readConsumer,
            BiConsumer<CodegenMethod, int> writeConsumer,
            CodegenClassScope classScope,
            string providerClassName)
        {
            var ctor = MakeCtor(forgeClass, providerClassName, classScope);

            var writeMethod = MakeWriteMethod<T>(classScope);
            var readMethod = MakeReadMethod<T>(classNameRow, classScope);

            if (!isTargetHA) {
                var message = "Serde not implemented because the compiler target is not HA";
                readMethod.Block.MethodThrowUnsupported(message);
                writeMethod.Block.MethodThrowUnsupported(message);
            }
            else if (forgeClass == AggregationServiceNullFactory.INSTANCE.GetType()) {
                readMethod.Block.MethodReturn(DefaultValue());
            }
            else {
                readMethod.Block.DeclareVar(classNameRow, "row", NewInstanceInner(classNameRow));
                readConsumer.Invoke(readMethod, level);

                var methodFactories = assignment.MethodFactories;
                var accessStates = assignment.AccessStateFactories;
                writeMethod.Block.DeclareVar(classNameRow, "row", Cast(classNameRow, Ref("@object")));
                writeConsumer.Invoke(writeMethod, level);
                
                if (methodFactories != null) {
                    for (var i = 0; i < methodFactories.Length; i++) {
                        methodFactories[i]
                            .Aggregator.WriteCodegen(Ref("row"), i, OUTPUT, UNIT_KEY, WRITER, writeMethod, classScope);
                    }

                    for (var i = 0; i < methodFactories.Length; i++) {
                        methodFactories[i]
                            .Aggregator.ReadCodegen(Ref("row"), i, INPUT, UNIT_KEY, readMethod, classScope);
                    }
                }

                if (accessStates != null) {
                    for (var i = 0; i < accessStates.Length; i++) {
                        accessStates[i]
                            .Aggregator.WriteCodegen(Ref("row"), i, OUTPUT, UNIT_KEY, WRITER, writeMethod, classScope);
                    }

                    for (var i = 0; i < accessStates.Length; i++) {
                        accessStates[i].Aggregator.ReadCodegen(Ref("row"), i, INPUT, readMethod, UNIT_KEY, classScope);
                    }
                }

                readMethod.Block.MethodReturn(Ref("row"));
            }

            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);
            CodegenStackGenerator.RecursiveBuildStack(readMethod, "ReadValue", methods, properties);

            return new CodegenInnerClass(
                classNameSerde,
                null,
                ctor,
                EmptyList<CodegenTypedParam>.Instance,
                methods,
                properties);
        }

        private static CodegenMethod MakeReadMethod<T>(
            string returnType,
            CodegenClassScope classScope)
        {
            return CodegenMethod
                .MakeParentNode(
                    typeof(T), // returnType <- DataInputObjectSerde<T> returns T and is not covariant
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(
                    CodegenNamedParam.From(
                        typeof(DataInput),
                        INPUT.Ref,
                        typeof(byte[]),
                        UNIT_KEY.Ref))
                .WithOverride();
        }

        private static CodegenMethod MakeWriteMethod<T>(
            CodegenClassScope classScope)
        {
            return CodegenMethod
                .MakeParentNode(
                    typeof(void),
                    typeof(AggregationServiceFactoryCompiler),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(
                    CodegenNamedParam.From(
                        typeof(T), "@object",
                        typeof(DataOutput), OUTPUT.Ref,
                        typeof(byte[]), UNIT_KEY.Ref,
                        typeof(EventBeanCollatedWriter), WRITER.Ref))
                .WithOverride();
        }

        private static CodegenCtor MakeCtor(
            Type forgeClass,
            string providerClassName,
            CodegenClassScope classScope)
        {
            var param = new CodegenTypedParam(providerClassName, "o");
            return new CodegenCtor(forgeClass, classScope, Collections.SingletonList(param));
        }
    }
} // end of namespace