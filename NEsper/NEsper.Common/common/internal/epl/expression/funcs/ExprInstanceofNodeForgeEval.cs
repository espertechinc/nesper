///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprInstanceofNodeForgeEval : ExprEvaluator
    {
        private readonly ExprEvaluator evaluator;
        private readonly ExprInstanceofNodeForge forge;

        private readonly CopyOnWriteList<Pair<Type, bool>> resultCache =
            new CopyOnWriteList<Pair<Type, bool>>();

        public ExprInstanceofNodeForgeEval(
            ExprInstanceofNodeForge forge,
            ExprEvaluator evaluator)
        {
            this.forge = forge;
            this.evaluator = evaluator;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = evaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (result == null) {
                return false;
            }

            return InstanceofCacheCheckOrAdd(forge.Classes, resultCache, result);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="classes">classes</param>
        /// <param name="resultCache">cache</param>
        /// <param name="result">result</param>
        /// <returns>bool</returns>
        public static bool InstanceofCacheCheckOrAdd(
            Type[] classes,
            CopyOnWriteList<Pair<Type, bool>> resultCache,
            object result)
        {
            // return cached value
            foreach (Pair<Type, bool> pair in resultCache) {
                if (pair.First == result.GetType()) {
                    return pair.Second;
                }
            }

            var @out = CheckAddType(classes, result.GetType(), resultCache);
            return @out;
        }

        public static CodegenExpression Codegen(
            ExprInstanceofNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression cache = codegenClassScope.AddFieldUnshared<CopyOnWriteList<Pair<Type, bool>>>(
                true, NewInstance(typeof(CopyOnWriteList<Pair<Type, bool>>)));
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool), typeof(ExprInstanceofNodeForgeEval), codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(
                    typeof(object), "result",
                    forge.ForgeRenderable.ChildNodes[0].Forge.EvaluateCodegen(
                        typeof(object), methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnFalse("result");
            block.MethodReturn(
                StaticMethod(
                    typeof(ExprInstanceofNodeForgeEval), "instanceofCacheCheckOrAdd", Constant(forge.Classes), cache,
                    Ref("result")));
            return LocalMethod(methodNode);
        }

        // Checks type and adds to cache
        private static bool CheckAddType(
            Type[] classes,
            Type type,
            CopyOnWriteList<Pair<Type, bool>> resultCache)
        {
            lock (resultCache) {
                // check again in synchronized block
                foreach (Pair<Type, bool> pair in resultCache) {
                    if (pair.First == type) {
                        return pair.Second;
                    }
                }

                // get the types superclasses and interfaces, and their superclasses and interfaces
                ISet<Type> classesToCheck = new HashSet<Type>();
                TypeHelper.GetBase(type, classesToCheck);
                classesToCheck.Add(type);

                // check type against each class
                var fits = false;
                foreach (var clazz in classes) {
                    if (classesToCheck.Contains(clazz)) {
                        fits = true;
                        break;
                    }
                }

                resultCache.Add(new Pair<Type, bool>(type, fits));
                return fits;
            }
        }
    }
} // end of namespace