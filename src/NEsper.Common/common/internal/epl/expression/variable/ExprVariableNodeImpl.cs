///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.variable
{
    /// <summary>
    /// Represents a variable in an expression tree.
    /// </summary>
    public class ExprVariableNodeImpl : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator,
        ExprVariableNode
    {
        private readonly VariableMetaData _variableMeta;
        private readonly string _optSubPropName;
        private EventPropertyGetterSPI _optSubPropGetter;
        private Type _returnType;

        public ExprVariableNodeImpl(
            VariableMetaData variableMeta,
            string optSubPropName)
        {
            _variableMeta = variableMeta;
            _optSubPropName = optSubPropName;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // determine if any types are property agnostic; If yes, resolve to variable
            var hasPropertyAgnosticType = false;
            var types = validationContext.StreamTypeService.EventTypes;
            for (var i = 0; i < validationContext.StreamTypeService.EventTypes.Length; i++) {
                if (types[i] is EventTypeSPI) {
                    hasPropertyAgnosticType |= ((EventTypeSPI)types[i]).Metadata.IsPropertyAgnostic;
                }
            }

            if (!hasPropertyAgnosticType) {
                var variableName = _variableMeta.VariableName;
                // the variable name should not overlap with a property name
                try {
                    validationContext.StreamTypeService.ResolveByPropertyName(variableName, false);
                    throw new ExprValidationException(
                        "The variable by name '" + variableName + "' is ambiguous to a property of the same name");
                }
                catch (DuplicatePropertyException) {
                    throw new ExprValidationException(
                        "The variable by name '" + variableName + "' is ambiguous to a property of the same name");
                }
                catch (PropertyNotFoundException) {
                    // expected
                }
            }

            var variableNameX = _variableMeta.VariableName;
            if (_optSubPropName != null) {
                if (_variableMeta.EventType == null) {
                    throw new ExprValidationException(
                        "Property '" + _optSubPropName + "' is not valid for variable '" + variableNameX + "'");
                }

                _optSubPropGetter = ((EventTypeSPI)_variableMeta.EventType).GetGetterSPI(_optSubPropName);
                if (_optSubPropGetter == null) {
                    throw new ExprValidationException(
                        "Property '" + _optSubPropName + "' is not valid for variable '" + variableNameX + "'");
                }

                _returnType = _variableMeta.EventType.GetPropertyType(_optSubPropName);
            }
            else {
                _returnType = _variableMeta.Type;
            }

            _returnType = _returnType.GetBoxedType();
            return null;
        }

        public override string ToString()
        {
            return "variableName=" + _variableMeta.VariableName;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_variableMeta.IsCompileTimeConstant) {
                return _variableMeta.ValueWhenAvailable;
            }

            throw new IllegalStateException("Cannot evaluate at compile time");
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_returnType == null) {
                return ConstantNull();
            }

            var returnClass = _returnType;
            var methodNode = parent.MakeChild(returnClass, typeof(ExprVariableNodeImpl), classScope);
            var readerExpression = GetReaderExpression(_variableMeta, methodNode, symbols, classScope);
            var block = methodNode.Block.DeclareVar<VariableReader>("reader", readerExpression);
            if (_variableMeta.EventType == null) {
                block
                    .DeclareVar(returnClass, "value", Cast(returnClass, ExprDotName(Ref("reader"), "Value")))
                    .MethodReturn(Ref("value"));
            }
            else {
                block
                    .DeclareVar<object>("value", ExprDotName(Ref("reader"), "Value"))
                    .IfRefNullReturnNull("value")
                    .DeclareVar<EventBean>("theEvent", Cast(typeof(EventBean), Ref("value")));
                if (_optSubPropName == null) {
                    block.MethodReturn(Cast(returnClass, ExprDotUnderlying(Ref("theEvent"))));
                }
                else {
                    block.MethodReturn(
                        CodegenLegoCast.CastSafeFromObjectType(
                            _returnType,
                            _optSubPropGetter.EventBeanGetCodegen(Ref("theEvent"), methodNode, classScope)));
                }
            }

            return LocalMethod(methodNode);
        }

        public static CodegenExpression GetReaderExpression(
            VariableMetaData variableMeta,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression readerExpression;
            if (variableMeta.OptionalContextName == null) {
                readerExpression =
                    classScope.AddOrGetDefaultFieldSharable(new VariableReaderCodegenFieldSharable(variableMeta));
            }
            else {
                var field = classScope.AddOrGetDefaultFieldSharable(new VariableReaderPerCPCodegenFieldSharable(variableMeta));
                var cpid = ExprDotName(symbols.GetAddExprEvalCtx(methodNode), "AgentInstanceId");
                readerExpression = Cast(typeof(VariableReader), ExprDotMethod(field, "Get", cpid));
            }

            return readerExpression;
        }

        public CodegenExpression CodegenGetDeployTimeConstValue(CodegenClassScope classScope)
        {
            if (_returnType == null) {
                return ConstantNull();
            }

            var returnClass = _returnType;
            CodegenExpression readerExpression =
                classScope.AddOrGetDefaultFieldSharable(new VariableReaderCodegenFieldSharable(_variableMeta));
            if (_variableMeta.EventType == null) {
                return Cast(returnClass, ExprDotName(readerExpression, "Value"));
            }

            var unpack = ExprDotUnderlying(Cast(typeof(EventBean), ExprDotName(readerExpression, "Value")));
            return Cast(returnClass, unpack);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprVariable",
                requiredType,
                parent,
                symbols,
                classScope).Build();
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(_variableMeta.VariableName);
            if (_optSubPropName != null) {
                writer.Write(".");
                writer.Write(_optSubPropName);
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprVariableNodeImpl that)) {
                return false;
            }

            if (!_optSubPropName?.Equals(that._optSubPropName) ?? that._optSubPropName != null) {
                return false;
            }

            return that._variableMeta.VariableName.Equals(_variableMeta.VariableName);
        }

        public void RenderForFilterPlan(StringBuilder @out)
        {
            @out.Append("variable '").Append(VariableNameWithSubProp).Append("'");
        }

        public ExprEvaluator ExprEvaluator => this;

        public VariableMetaData VariableMetadata => _variableMeta;

        public Type EvaluationType => _returnType;

        public override ExprForge Forge => this;

        public ExprNodeRenderable ForgeRenderable => this;
        
        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprForgeConstantType ForgeConstantType {
            get {
                if (_variableMeta.OptionalContextName != null) {
                    return ExprForgeConstantType.NONCONST;
                }

                // for simple-value variables that are constant and created by the same module and not preconfigured we can use compile-time constant
                if (_variableMeta.IsConstant &&
                    _variableMeta.IsCreatedByCurrentModule &&
                    _variableMeta.VariableVisibility != NameAccessModifier.PRECONFIGURED &&
                    _variableMeta.EventType == null) {
                    return ExprForgeConstantType.COMPILETIMECONST;
                }

                return _variableMeta.IsConstant ? ExprForgeConstantType.DEPLOYCONST : ExprForgeConstantType.NONCONST;
            }
        }

        public string VariableNameWithSubProp {
            get {
                if (_optSubPropName == null) {
                    return _variableMeta.VariableName;
                }

                return _variableMeta.VariableName + "." + _optSubPropName;
            }
        }
    }
} // end of namespace