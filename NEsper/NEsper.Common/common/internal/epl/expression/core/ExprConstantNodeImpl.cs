///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Represents a constant in an expressiun tree.
    /// </summary>
    public class ExprConstantNodeImpl : ExprNodeBase,
        ExprConstantNode,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        private readonly EnumValue enumValue;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="value">is the constant's value.</param>
        public ExprConstantNodeImpl(object value)
        {
            ConstantValue = value;
            if (value == null) {
                EvaluationType = null;
            }
            else {
                EvaluationType = value.GetType().GetPrimitiveType();
            }
        }

        public ExprConstantNodeImpl(EnumValue enumValue)
        {
            this.enumValue = enumValue;
            EvaluationType = enumValue.EnumField.FieldType;
            try {
                ConstantValue = enumValue.EnumField.GetValue(null);
            }
            catch (MemberAccessException e) {
                throw new EPException("Exception accessing field '" + enumValue.EnumField.Name + "': " + e.Message, e);
            }
        }

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="value">is the constant's value.</param>
        /// <param name="valueType">is the constant's value type.</param>
        public ExprConstantNodeImpl(
            object value,
            Type valueType)
        {
            ConstantValue = value;
            if (value == null) {
                EvaluationType = valueType;
            }
            else {
                EvaluationType = value.GetType().GetPrimitiveType();
            }
        }

        /// <summary>
        ///     Ctor - for use when the constant should return a given type and the actual value is always null.
        /// </summary>
        /// <param name="clazz">the type of the constant null.</param>
        public ExprConstantNodeImpl(Type clazz)
        {
            EvaluationType = Boxing.GetBoxedType(clazz);
            ConstantValue = null;
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.COMPILETIMECONST;

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType { get; }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public override ExprForge Forge => this;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        /// <summary>
        ///     Returns the constant's value.
        /// </summary>
        /// <returns>value of constant</returns>
        public object ConstantValue { get; set; }

        public Type ConstantType => EvaluationType;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprConstantNodeImpl)) {
                return false;
            }

            var other = (ExprConstantNodeImpl) node;

            if (other.ConstantValue == null && ConstantValue != null) {
                return false;
            }

            if (other.ConstantValue != null && ConstantValue == null) {
                return false;
            }

            if (other.ConstantValue == null && ConstantValue == null) {
                return true;
            }

            return other.ConstantValue.Equals(ConstantValue);
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ConstantValue;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprConst",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Noqparam()
                .Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (ConstantValue == null) {
                return ConstantNull();
            }

            if (ConstantValue.GetType().IsEnum) {
                return EnumValue(ConstantValue.GetType(), ConstantValue.ToString());
            }

            if (enumValue != null) {
                return PublicConstValue(enumValue.EnumClass, enumValue.EnumField.Name);
            }

            return Constant(ConstantValue);
        }

        public bool IsConstantResult => true;

        /// <summary>
        ///     Sets the value of the constant.
        /// </summary>
        /// <param name="value">to set</param>
        public void SetValue(object value)
        {
            ConstantValue = value;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (ConstantValue is string) {
                writer.Write("\"" + ConstantValue + '\"');
            }
            else {
                writer.Write(CompatExtensions.RenderAny(ConstantValue));
            }
        }
    }
} // end of namespace