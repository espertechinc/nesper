///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.bytecodemodel.util
{
    public class CodegenMakeableUtil
    {
        public static CodegenExpression MakeArray(
            string name,
            Type clazz,
            CodegenMakeable[] forges,
            Type generator,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var arrayType = TypeHelper.GetArrayType(clazz);
            if (forges == null || forges.Length == 0) {
                return NewArrayByLength(clazz, Constant(0));
            }

            var method = parent.MakeChild(arrayType, generator, classScope);
            method.Block.DeclareVar(arrayType, name, NewArrayByLength(clazz, Constant(forges.Length)));
            for (var i = 0; i < forges.Length; i++) {
                method.Block.AssignArrayElement(
                    Ref(name),
                    Constant(i),
                    forges[i] == null ? ConstantNull() : forges[i].Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref(name));
            return LocalMethod(method);
        }

        public static CodegenExpression MakeMap(
            string name,
            Type clazzKey,
            Type clazzValue,
            IDictionary<CodegenMakeable, CodegenMakeable> map,
            Type generator,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            if (map.IsEmpty()) {
                return StaticMethod(typeof(Collections), "GetEmptyMap", new Type[] { clazzKey, clazzValue });
            }

            var interfaceType = typeof(IDictionary<,>)
                .MakeGenericType(clazzKey, clazzValue);

            var method = parent.MakeChild(interfaceType, generator, classScope);
            var count = 0;
            foreach (var entry in map) {
                var nameKey = "key" + count;
                var nameValue = "value" + count;
                method.Block
                    .DeclareVar(clazzKey, nameKey, entry.Key.Make(method, symbols, classScope))
                    .DeclareVar(clazzValue, nameValue, entry.Value.Make(method, symbols, classScope));
                count++;
            }

            if (map.Count == 1) {
                method.Block.MethodReturn(
                    StaticMethod(
                        typeof(Collections), 
                        "SingletonMap",
                        new Type[] { clazzKey, clazzValue },
                        Ref("key0"), 
                        Ref("value0")));
            }
            else {
                var implementationType = typeof(Dictionary<,>).MakeGenericType(clazzKey, clazzValue);
                method.Block.DeclareVar(
                    interfaceType,
                    name,
                    NewInstance(implementationType, Constant(map.Count)));
                for (var i = 0; i < map.Count; i++) {
                    method.Block.ExprDotMethod(Ref(name), "Put", Ref("key" + i), Ref("value" + i));
                }

                method.Block.MethodReturn(Ref(name));
            }

            return LocalMethod(method);
        }
    }
} // end of namespace