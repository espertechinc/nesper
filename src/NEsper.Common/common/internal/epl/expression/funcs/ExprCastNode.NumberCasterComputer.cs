///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        /// <summary>
        ///     Casting and parsing computer.
        /// </summary>
        public class NumberCasterComputer : CasterParserComputerForge,
            CasterParserComputer
        {
            private readonly SimpleTypeCaster _numericTypeCaster;

            public NumberCasterComputer(SimpleTypeCaster numericTypeCaster)
            {
                this._numericTypeCaster = numericTypeCaster;
            }

            public object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                if (input.IsNumber()) {
                    return _numericTypeCaster.Cast(input);
                }

                return null;
            }

            public bool IsConstantForConstInput => true;

            public CodegenExpression CodegenPremade(
                Type evaluationType,
                CodegenExpression input,
                Type inputType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                if (inputType.CanNotBeNull() || inputType.IsNumeric()) {
                    return _numericTypeCaster.Codegen(input, inputType, codegenMethodScope, codegenClassScope);
                }

                var methodNode = codegenMethodScope
                    .MakeChild(evaluationType, typeof(NumberCasterComputer), codegenClassScope)
                    .AddParam(inputType, "input");

                methodNode.Block
                    .IfInstanceOf("input", typeof(object))
                    .BlockReturn(
                        _numericTypeCaster.Codegen(
                            CodegenExpressionBuilder.Ref("input"),
                            inputType,
                            methodNode,
                            codegenClassScope))
                    .MethodReturn(CodegenExpressionBuilder.ConstantNull());
                return CodegenExpressionBuilder.LocalMethod(methodNode, input);
            }

            public CasterParserComputer EvaluatorComputer => this;
        }
    }
}