///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.variable
{
    /// <summary>
    ///     Represents a variable in an expression tree.
    /// </summary>
    public class ExprVariableNodeImpl : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator,
        ExprVariableNode
    {
        private readonly string _optSubPropName;
        private EventPropertyGetterSPI _optSubPropGetter;
        private Type _returnType;

        public ExprVariableNodeImpl(
            VariableMetaData variableMeta,
            string optSubPropName)
        {
            VariableMetadata = variableMeta;
            _optSubPropName = optSubPropName;
        }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (VariableMetadata.IsCompileTimeConstant) {
                return VariableMetadata.ValueWhenAvailable;
            }

            throw new IllegalStateException("Cannot evaluate at compile time");
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => _returnType;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (_returnType.IsNullType()) {
                return ConstantNull();
            }
            
            var methodNode = parent.MakeChild(_returnType, typeof(ExprVariableNodeImpl), classScope);
            var readerExpression = GetReaderExpression(VariableMetadata, methodNode, symbols, classScope);

            var block = methodNode.Block
                .DeclareVar<VariableReader>("reader", readerExpression);
            if (VariableMetadata.EventType == null) {
                block.DeclareVar(
                        _returnType,
                        "value",
                        Cast(_returnType, ExprDotName(Ref("reader"), "Value")))
                    .MethodReturn(Ref("value"));
            }
            else {
                block.DeclareVar<object>("value", ExprDotName(Ref("reader"), "Value"))
                    .IfRefNullReturnNull("value")
                    .DeclareVar<EventBean>("theEvent", Cast(typeof(EventBean), Ref("value")));
                if (_optSubPropName == null) {
                    block.MethodReturn(Cast(_returnType, ExprDotUnderlying(Ref("theEvent"))));
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
                readerExpression = classScope
                    .AddOrGetDefaultFieldSharable(new VariableReaderCodegenFieldSharable(variableMeta));
            }
            else {
                var field = classScope.AddOrGetDefaultFieldSharable(
                    new VariableReaderPerCPCodegenFieldSharable(variableMeta));
                var cpid = ExprDotName(symbols.GetAddExprEvalCtx(methodNode), "AgentInstanceId");
                readerExpression = Cast(typeof(VariableReader), ExprDotMethod(field, "Get", cpid));
            }

            return readerExpression;
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

        public VariableMetaData VariableMetadata { get; }

        public CodegenExpression CodegenGetDeployTimeConstValue(CodegenClassScope classScope)
        {
            CodegenExpression readerExpression =
                classScope.AddOrGetDefaultFieldSharable(new VariableReaderCodegenFieldSharable(VariableMetadata));
            if (VariableMetadata.EventType == null) {
                return Cast(_returnType, ExprDotName(readerExpression, "Value"));
            }

            var unpack = ExprDotUnderlying(Cast(typeof(EventBean), ExprDotName(readerExpression, "Value")));
            return Cast(_returnType, unpack);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // determine if any types are property agnostic; If yes, resolve to variable
            var hasPropertyAgnosticType = false;
            var types = validationContext.StreamTypeService.EventTypes;
            for (var i = 0; i < validationContext.StreamTypeService.EventTypes.Length; i++) {
                if (types[i] is EventTypeSPI) {
                    hasPropertyAgnosticType |= ((EventTypeSPI) types[i]).Metadata.IsPropertyAgnostic;
                }
            }

            string variableName;

            if (!hasPropertyAgnosticType) {
                variableName = VariableMetadata.VariableName;
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

            variableName = VariableMetadata.VariableName;
            if (_optSubPropName != null) {
                if (VariableMetadata.EventType == null) {
                    throw new ExprValidationException(
                        "Property '" + _optSubPropName + "' is not valid for variable '" + variableName + "'");
                }

                _optSubPropGetter = ((EventTypeSPI) VariableMetadata.EventType).GetGetterSPI(_optSubPropName);
                if (_optSubPropGetter == null) {
                    throw new ExprValidationException(
                        "Property '" + _optSubPropName + "' is not valid for variable '" + variableName + "'");
                }

                _returnType = VariableMetadata.EventType.GetPropertyType(_optSubPropName);
            }
            else {
                _returnType = VariableMetadata.Type;
            }

            _returnType = _returnType.GetBoxedType();
            return null;
        }

        public override string ToString()
        {
            return "variableName=" + VariableMetadata.VariableName;
        }

        public ExprForgeConstantType ForgeConstantType {
            get {
                if (VariableMetadata.OptionalContextName != null) {
                    return ExprForgeConstantType.NONCONST;
                }

                // for simple-value variables that are constant and created by the same module and not preconfigured we can use compile-time constant
                if (VariableMetadata.IsConstant &&
                    VariableMetadata.IsCreatedByCurrentModule &&
                    VariableMetadata.VariableVisibility != NameAccessModifier.PRECONFIGURED &&
                    VariableMetadata.EventType == null) {
                    return ExprForgeConstantType.COMPILETIMECONST;
                }

                return VariableMetadata.IsConstant ? ExprForgeConstantType.DEPLOYCONST : ExprForgeConstantType.NONCONST;
            }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(VariableMetadata.VariableName);
            if (_optSubPropName != null) {
                writer.Write(".");
                writer.Write(_optSubPropName);
            }
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprVariableNodeImpl)) {
                return false;
            }

            var that = (ExprVariableNodeImpl) node;

            if (_optSubPropName != null ? !_optSubPropName.Equals(that._optSubPropName) : that._optSubPropName != null) {
                return false;
            }

            return that.VariableMetadata.VariableName.Equals(VariableMetadata.VariableName);
        }


        public void RenderForFilterPlan(StringBuilder @out)
        {
            @out.Append("variable '");
            @out.Append(VariableNameWithSubProp);
            @out.Append("'");
        }

        public string VariableNameWithSubProp {
            get {
                if (_optSubPropName == null) {
                    return VariableMetadata.VariableName;
                }

                return VariableMetadata.VariableName + "." + _optSubPropName;
            }
        }
    }
} // end of namespace