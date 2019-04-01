///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class CodegenMakeableUtil
    {
        public static CodegenExpression MakeArray<T>(
            string name, 
            Type clazz,
            CodegenMakeable<T>[] forges,
            Type generator, 
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
            where T : CodegenSymbolProvider
        {
            Type arrayType = TypeHelper.GetArrayType(clazz);
            if (forges == null || forges.Length == 0) {
                return NewArrayByLength(clazz, Constant(0));
            }

            CodegenMethod method = parent.MakeChild(arrayType, generator, classScope);
            method.Block.DeclareVar(arrayType, name, NewArrayByLength(clazz, Constant(forges.Length)));
            for (int i = 0; i < forges.Length; i++) {
                method.Block.AssignArrayElement(
                    @Ref(name), Constant(i),
                    forges[i] == null ? ConstantNull() : forges[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(@Ref(name));
            return LocalMethod(method);
        }

        public static CodegenExpression MakeMap<K, V>(
            string name,
            Type clazzKey,
            Type clazzValue,
            IDictionary<CodegenMakeable<K>, CodegenMakeable<V>> map, 
            Type generator,
            CodegenMethodScope parent, 
            SAIFFInitializeSymbol symbols, 
            CodegenClassScope classScope)
            where K : CodegenSymbolProvider
            where V : CodegenSymbolProvider
        {
            if (map.IsEmpty()) {
                return StaticMethod(typeof(Collections), "emptyMap");
            }

            CodegenMethod method = parent.MakeChild(typeof(IDictionary<string, object>), generator, classScope);
            int count = 0;
            foreach (var entry in map) {
                string nameKey = "key" + count;
                string nameValue = "value" + count;
                method.Block
                    .DeclareVar(clazzKey, nameKey, entry.Key.Make(method, symbols, classScope))
                    .DeclareVar(clazzValue, nameValue, entry.Value.Make(method, symbols, classScope));
                count++;
            }

            if (map.Count == 1) {
                method.Block.MethodReturn(
                    StaticMethod(typeof(Collections), "singletonMap", @Ref("key0"), @Ref("value0")));
            }
            else {
                method.Block.DeclareVar(
                    typeof(IDictionary<object, object>), name, NewInstance(typeof(LinkedHashMap), Constant(map.Count)));
                for (int i = 0; i < map.Count; i++) {
                    method.Block.ExprDotMethod(@Ref(name), "put", @Ref("key" + i), @Ref("value" + i));
                }

                method.Block.MethodReturn(@Ref(name));
            }

            return LocalMethod(method);
        }
    }
} // end of namespace