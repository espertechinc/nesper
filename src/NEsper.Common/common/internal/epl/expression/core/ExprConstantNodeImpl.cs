///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        private object _value;
        private readonly Type _clazz;
        private readonly EnumValue _enumValue;
        private readonly string _stringConstantWhenProvided;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="value">is the constant's value.</param>
        public ExprConstantNodeImpl(object value, string stringConstantWhenProvided)
        {
            _value = value;
            _stringConstantWhenProvided = stringConstantWhenProvided;
            if (value == null) {
                _clazz = null;
            }
            else {
                _clazz = value.GetType().GetPrimitiveType();
            }
        }

        public ExprConstantNodeImpl(object value) : this(value, (string) null)
        {
        }

        public ExprConstantNodeImpl(EnumValue enumValue)
        {
            _stringConstantWhenProvided = null;
            _enumValue = enumValue;
            _clazz = enumValue.EnumField.FieldType;
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
            _stringConstantWhenProvided = null;
            _value = value;
            if (value == null) {
                _clazz = valueType.GetBoxedType();
            }
            else {
                _clazz = value.GetType().GetPrimitiveType();
            }
        }

        /// <summary>
        ///     Ctor - for use when the constant should return a given type and the actual value is always null.
        /// </summary>
        /// <param name="clazz">the type of the constant null.</param>
        public ExprConstantNodeImpl(Type clazz)
        {
            _clazz = clazz.GetBoxedType();
            _value = null;
            _stringConstantWhenProvided = null;
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.COMPILETIMECONST;

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => _clazz;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public override ExprForge Forge => this;

        public string StringConstantWhenProvided => _stringConstantWhenProvided;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        /// <summary>
        ///     Returns the constant's value.
        /// </summary>
        /// <returns>value of constant</returns>
        public object ConstantValue {
            get => _value;
            set => _value = value;
        }

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
            if (_value == null) {
                return ConstantNull();
            }

            if (_enumValue != null) {
                return PublicConstValue(_enumValue.EnumClass, _enumValue.EnumField.Name);
            }

            if (_value.GetType().IsEnum) {
                return EnumValue(_value.GetType(), _value.ToString());
            }

            return Constant(_value);
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

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
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