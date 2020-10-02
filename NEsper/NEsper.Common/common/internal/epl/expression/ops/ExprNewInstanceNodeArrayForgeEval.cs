///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
	/// <summary>
	///     Represents the "new Class[dim][dim]" operator in an expression tree.
	/// </summary>
	public class ExprNewInstanceNodeArrayForgeEval : ExprEvaluator
    {
        private const string NULL_MSG = "new-array received a null value for dimension";

        private readonly ExprNewInstanceNodeArrayForge _forge;

        public ExprNewInstanceNodeArrayForgeEval(ExprNewInstanceNodeArrayForge forge)
        {
            _forge = forge;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_forge.Parent.IsArrayInitializedByExpr) {
                return _forge.Parent.ChildNodes[0].Forge.ExprEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            var children = _forge.Parent.ChildNodes;
            var dimensions = new int[children.Length];
            for (var i = 0; i < children.Length; i++) {
                var size = (int?) _forge.Parent.ChildNodes[i].Forge.ExprEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                if (size == null) {
                    throw new EPException(NULL_MSG);
                }

                dimensions[i] = size.Value;
            }

            return Array.CreateInstance(_forge.TargetClass, dimensions);
        }

        public static CodegenExpression EvaluateCodegen(
            Type requiredType,
            ExprNewInstanceNodeArrayForge forge,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (forge.Parent.IsArrayInitializedByExpr) {
                return forge.Parent.ChildNodes[0].Forge.EvaluateCodegen(requiredType, parent, symbols, classScope);
            }

            var method = parent.MakeChild(requiredType, typeof(ExprNewInstanceNodeArrayForgeEval), classScope);
            var dimensions = forge.Parent.ChildNodes;

            var dimValue = new CodegenExpression[dimensions.Length];
            for (var i = 0; i < dimensions.Length; i++) {
                var dimForge = forge.Parent.ChildNodes[i].Forge;
                var dimForgeType = dimForge.EvaluationType;
                var dimExpr = dimForge.EvaluateCodegen(typeof(int?), method, symbols, classScope);
                if (dimForge.ForgeConstantType == ExprForgeConstantType.COMPILETIMECONST) {
                    dimValue[i] = Unbox(dimExpr, dimForgeType);
                }
                else {
                    var name = "dim" + i;
                    method.Block
                        .DeclareVar(typeof(int?), name, dimExpr)
                        .IfRefNull(name)
                        .BlockThrow(NewInstance(typeof(EPException), Constant(NULL_MSG)));
                    dimValue[i] = Unbox(Ref(name));
                }
            }

            CodegenExpression make;
            if (dimValue.Length == 1) {
                make = NewArrayByLength(forge.TargetClass, dimValue[0]);
            }
            else {
                var @params = new CodegenExpression[dimValue.Length + 1];
                @params[0] = Clazz(forge.TargetClass);
                Array.Copy(dimValue, 0, @params, 1, dimValue.Length);
                make = StaticMethod(typeof(Array), "CreateInstance", @params);
            }
            
            method.Block.MethodReturn(CodegenLegoCast.CastSafeFromObjectType(requiredType, make));
            return LocalMethod(method);
        }
    }
} // end of namespace