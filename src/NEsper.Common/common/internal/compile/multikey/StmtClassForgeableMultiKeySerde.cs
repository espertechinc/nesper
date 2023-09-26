///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.compile.multikey
{
    public class StmtClassForgeableMultiKeySerde : StmtClassForgeable
    {
        private const string OBJECT_NAME = "obj";
        private const string OUTPUT_NAME = "output";
        private const string INPUT_NAME = "input";
        private const string UNITKEY_NAME = "unitKey";
        private const string WRITER_NAME = "writer";
        
        private readonly string className;
        private readonly CodegenNamespaceScope namespaceScope;
        private readonly Type[] types;
        private readonly string classNameMK;
        private readonly DataInputOutputSerdeForge[] forges;

        public StmtClassForgeableMultiKeySerde(
            string className,
            CodegenNamespaceScope namespaceScope,
            Type[] types,
            string classNameMK,
            DataInputOutputSerdeForge[] forges)
        {
            this.className = className;
            this.namespaceScope = namespaceScope;
            this.types = types;
            this.classNameMK = classNameMK;
            this.forges = forges;
        }

        public CodegenClass Forge(
            bool includeDebugSymbols,
            bool fireAndForget)
        {
            var methods = new CodegenClassMethods();
            var properties = new CodegenClassProperties();
            var classScope = new CodegenClassScope(includeDebugSymbols, namespaceScope, className);
            var writeMethod = CodegenMethod
                .MakeParentNode(
                    typeof(void),
                    typeof(StmtClassForgeableMultiKeySerde),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam<object>(OBJECT_NAME)
                .AddParam(typeof(DataOutput), OUTPUT_NAME)
                .AddParam<byte[]>(UNITKEY_NAME)
                .AddParam<EventBeanCollatedWriter>(WRITER_NAME);
            if (!fireAndForget) {
                MakeWriteMethod(writeMethod);
            }

            CodegenStackGenerator.RecursiveBuildStack(writeMethod, "Write", methods, properties);
            var readMethod = CodegenMethod
                .MakeParentNode(
                    typeof(object),
                    typeof(StmtClassForgeableMultiKeySerde),
                    CodegenSymbolProviderEmpty.INSTANCE,
                    classScope)
                .AddParam(typeof(DataInput), INPUT_NAME)
                .AddParam<byte[]>(UNITKEY_NAME);
            if (!fireAndForget) {
                MakeReadMethod(readMethod);
            }
            else {
                readMethod.Block.MethodReturn(ConstantNull());
            }

            CodegenStackGenerator.RecursiveBuildStack(readMethod, "Read", methods, properties);
            IList<CodegenTypedParam> members = new List<CodegenTypedParam>();
            for (var i = 0; i < forges.Length; i++) {
                members.Add(new CodegenTypedParam(forges[i].ForgeClassName, "s" + i));
            }

            var providerCtor = new CodegenCtor(GetType(), includeDebugSymbols, EmptyList<CodegenTypedParam>.Instance);
            for (var i = 0; i < forges.Length; i++) {
                providerCtor.Block.AssignRef("s" + i, forges[i].Codegen(providerCtor, classScope, null));
            }

            return new CodegenClass(
                CodegenClassType.KEYPROVISIONINGSERDE,
                typeof(DataInputOutputSerde),
                className,
                classScope,
                members,
                providerCtor,
                methods,
                properties,
                EmptyList<CodegenInnerClass>.Instance);
        }

        private void MakeWriteMethod(CodegenMethod writeMethod)
        {
            writeMethod.Block.DeclareVar(classNameMK, "key", Cast(classNameMK, Ref(OBJECT_NAME)));
            for (var i = 0; i < types.Length; i++) {
                var key = Ref("key.k" + i);
                CodegenExpression serde = Ref("s" + i);
                writeMethod.Block.ExprDotMethod(
                    serde,
                    "Write",
                    key,
                    Ref(OUTPUT_NAME),
                    Ref(UNITKEY_NAME),
                    Ref(WRITER_NAME));
            }
        }

        private void MakeReadMethod(CodegenMethod readMethod)
        {
            var @params = new CodegenExpression[types.Length];
            for (var i = 0; i < types.Length; i++) {
                CodegenExpression serde = Ref("s" + i);
                var boxed = types[i].GetBoxedType();
                var read = ExprDotMethod(serde, "Read", Ref(INPUT_NAME), Ref(UNITKEY_NAME));
                if (boxed != null) {
                    @params[i] = Cast(boxed, read);
                }
                else {
                    @params[i] = read;
                }
            }

            readMethod.Block.MethodReturn(NewInstanceInner(classNameMK, @params));
        }

        public string ClassName => className;

        public StmtClassForgeableType ForgeableType => StmtClassForgeableType.MULTIKEY;
    }
} // end of namespace