///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class InstanceManufacturerFastCtor : InstanceManufacturer
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly InstanceManufacturerFactoryFastCtor factory;

        public InstanceManufacturerFastCtor(
            InstanceManufacturerFactoryFastCtor factory,
            ExprEvaluator[] evaluators)
        {
            this.factory = factory;
            this.evaluators = evaluators;
        }

        public object Make(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var row = new object[evaluators.Length];
            for (var i = 0; i < row.Length; i++) {
                row[i] = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return MakeUnderlyingFromFastCtor(row, factory.Ctor, factory.TargetClass);
        }

        public static object MakeUnderlyingFromFastCtor(
            object[] properties,
            ConstructorInfo ctor,
            Type target)
        {
            try {
                return ctor.Invoke(properties);
            }
            catch (TargetException e) {
                throw GetTargetExceptionAsEPException(target.Name, e);
            }
            catch (MemberAccessException e) {
                throw GetTargetExceptionAsEPException(target.Name, e);
            }
            catch (TypeLoadException e) {
                throw GetTargetExceptionAsEPException(target.Name, e);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="targetClassName">name</param>
        /// <param name="thrown">ex</param>
        /// <returns>exception</returns>
        public static EPException GetTargetExceptionAsEPException(
            string targetClassName,
            Exception thrown)
        {
            var targetException = thrown is TargetException ? ((TargetException) thrown).InnerException : thrown;
            return new EPException(
                "TargetException received invoking constructor for type '" +
                targetClassName +
                "': " +
                targetException.Message,
                targetException);
        }

        public static CodegenExpression Codegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ConstructorInfo targetCtor,
            ExprForge[] forges)
        {
            var targetClass = targetCtor.DeclaringType;
            var methodNode = codegenMethodScope.MakeChild(
                targetClass,
                typeof(InstanceManufacturerFastCtor),
                codegenClassScope);

            var targetCtorParams = targetCtor.GetParameters();
            var paramList = new CodegenExpression[forges.Length];
            for (var i = 0; i < forges.Length; i++) {
                var targetCtorParam = targetCtorParams[i];
                var targetCtorParamType = targetCtorParam.ParameterType;
                var currentForge = forges[i];
                var currentForgeEvaluationType = currentForge.EvaluationType;

                if (targetCtorParamType.IsAssignableFrom(currentForgeEvaluationType)) {
                    paramList[i] = currentForge
                        .EvaluateCodegen(
                            currentForgeEvaluationType,
                            methodNode,
                            exprSymbol,
                            codegenClassScope);
                }
                else if (targetCtorParamType.IsUnboxedType() && currentForgeEvaluationType.IsUnboxedType()) {
                    // Not directly assignable, but looks like we're relying on IL type conversion.
                    
                    paramList[i] = currentForge
                        .EvaluateCodegen(
                            currentForgeEvaluationType,
                            methodNode,
                            exprSymbol,
                            codegenClassScope);
                }
                else if (targetCtorParamType.IsUnboxedType() && currentForgeEvaluationType.IsBoxedType()) {
                    // Widening and narrowing tests should have occurred by this point.  Normally what we find is that the
                    // currentForgeEvaluationType is boxed and the targetCtorParamType is unboxed.  If this is the case,
                    // we can solve this by unboxing the currentForgeEvaluationType.

                    paramList[i] = Unbox(
                        currentForge
                            .EvaluateCodegen(
                                currentForgeEvaluationType,
                                methodNode,
                                exprSymbol,
                                codegenClassScope));
                }
                else {
                    throw new IllegalStateException("mismatch between constructor and forge");
                }
            }

            methodNode.Block
                .TryCatch()
                .TryReturn(NewInstance(targetClass, paramList))
                .AddCatch(typeof(Exception), "t")
                .BlockThrow(
                    StaticMethod(
                        typeof(InstanceManufacturerFastCtor),
                        "GetTargetExceptionAsEPException",
                        Constant(targetClass.Name),
                        Ref("t")))
                .MethodEnd();
            return LocalMethod(methodNode);
        }
    }
} // end of namespace