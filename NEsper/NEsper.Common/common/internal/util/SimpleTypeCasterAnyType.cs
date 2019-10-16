///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    ///     Cast implementation for non-numeric values that caches allowed casts assuming there is a small set of casts
    ///     allowed.
    /// </summary>
    public class SimpleTypeCasterAnyType : SimpleTypeCaster
    {
        private readonly Type typeToCastTo;
        private readonly CopyOnWriteArraySet<Pair<Type, bool>> pairs = new CopyOnWriteArraySet<Pair<Type, bool>>();

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="typeToCastTo">is the target type</param>
        public SimpleTypeCasterAnyType(Type typeToCastTo)
        {
            this.typeToCastTo = typeToCastTo;
        }

        public bool IsNumericCast => false;

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="object">to cast</param>
        /// <param name="typeToCastTo">target</param>
        /// <param name="pairs">cache</param>
        /// <returns>null or object</returns>
        public static object SimpleTypeCasterCast(
            object @object,
            Type typeToCastTo,
            CopyOnWriteArraySet<Pair<Type, bool>> pairs)
        {
            if (@object.GetType() == typeToCastTo) {
                return @object;
            }

            // check cache to see if this is cast-able
            foreach (var pair in pairs) {
                if (pair.First == typeToCastTo) {
                    if (!pair.Second) {
                        return null;
                    }

                    return @object;
                }
            }

            // Not found in cache, add to cache;
            lock (pairs) {
                // search cache once more
                foreach (var pair in pairs) {
                    if (pair.First == typeToCastTo) {
                        if (!pair.Second) {
                            return null;
                        }

                        return @object;
                    }
                }

                // Determine if any of the super-types and interfaces that the object implements or extends
                // is the same as any of the target types
                var passed = TypeHelper.IsSubclassOrImplementsInterface(@object.GetType(), typeToCastTo);

                if (passed) {
                    pairs.Add(new Pair<Type, bool>(@object.GetType(), true));
                    return @object;
                }

                pairs.Add(new Pair<Type, bool>(@object.GetType(), false));
                return null;
            }
        }

        public object Cast(object @object)
        {
            return SimpleTypeCasterCast(@object, typeToCastTo, pairs);
        }

        public CodegenExpression Codegen(
            CodegenExpression input,
            Type inputType,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (TypeHelper.IsSubclassOrImplementsInterface(inputType, typeToCastTo)) {
                return input;
            }

            var cache = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(CopyOnWriteArraySet<Pair<Type, bool>>),
                NewInstance(typeof(CopyOnWriteArraySet<Pair<Type, bool>>)));
            return CodegenExpressionBuilder.Cast(
                typeToCastTo,
                StaticMethod(
                    typeof(SimpleTypeCasterAnyType),
                    "SimpleTypeCasterCast",
                    input,
                    Constant(typeToCastTo),
                    cache));
        }
    }
} // end of namespace