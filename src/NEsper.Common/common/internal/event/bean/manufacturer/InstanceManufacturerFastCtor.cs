///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class InstanceManufacturerFastCtor : InstanceManufacturer
    {
        private readonly InstanceManufacturerFactoryFastCtor factory;
        private readonly ExprEvaluator[] evaluators;

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
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="targetClassName">name</param>
        /// <param name="thrown">ex</param>
        /// <returns>exception</returns>
        public static EPException GetTargetExceptionAsEPException(
            string targetClassName,
            Exception thrown)
        {
            var targetException = thrown is TargetInvocationException targetInvocationException
                ? targetInvocationException.InnerException
                : thrown;
            return new EPException(
                "InvocationTargetException received invoking constructor for type '" +
                targetClassName +
                "': " +
                targetException.Message,
                targetException);
        }

        public static CodegenExpression Codegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            Type targetClass,
            ExprForge[] forges)
        {
            var methodNode = codegenMethodScope.MakeChild(
                targetClass,
                typeof(InstanceManufacturerFastCtor),
                codegenClassScope);

            var @params = new CodegenExpression[forges.Length];
            for (var i = 0; i < forges.Length; i++) {
                var type = forges[i].EvaluationType;
                if (type == null) {
                    @params[i] = ConstantNull();
                }
                else {
                    @params[i] = forges[i].EvaluateCodegen(type, methodNode, exprSymbol, codegenClassScope);
                }
            }

            methodNode.Block
                .TryCatch()
                .TryReturn(NewInstance(targetClass, @params))
                .AddCatch(typeof(Exception), "ex")
                .BlockThrow(
                    StaticMethod(
                        typeof(InstanceManufacturerFastCtor),
                        "GetTargetExceptionAsEPException",
                        Constant(targetClass.FullName),
                        Ref("t")))
                .MethodEnd();
            return LocalMethod(methodNode);
        }
    }
} // end of namespace