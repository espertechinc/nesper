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
    ///     Represents a variable in an expression tree.
    /// </summary>
    public class ExprVariableNodeImpl : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator,
        ExprVariableNode
    {
        private readonly string optSubPropName;

        private EventPropertyGetterSPI optSubPropGetter;

        public ExprVariableNodeImpl(
            VariableMetaData variableMeta,
            string optSubPropName)
        {
            VariableMetadata = variableMeta;
            this.optSubPropName = optSubPropName;
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

        public Type EvaluationType { get; private set; }

        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var methodNode = parent.MakeChild(EvaluationType, typeof(ExprVariableNodeImpl), classScope);

            CodegenExpression readerExpression;
            if (VariableMetadata.OptionalContextName == null) {
                readerExpression =
                    classScope.AddOrGetFieldSharable(new VariableReaderCodegenFieldSharable(VariableMetadata));
            }
            else {
                var field = classScope.AddOrGetFieldSharable(
                    new VariableReaderPerCPCodegenFieldSharable(VariableMetadata));
                var cpid = ExprDotName(symbols.GetAddExprEvalCtx(methodNode), "AgentInstanceId");
                readerExpression = Cast(typeof(VariableReader), ExprDotMethod(field, "get", cpid));
            }

            var block = methodNode.Block
                .DeclareVar<VariableReader>("reader", readerExpression);
            if (VariableMetadata.EventType == null) {
                block.DeclareVar(
                        EvaluationType,
                        "value",
                        Cast(EvaluationType, ExprDotName(Ref("reader"), "Value")))
                    .MethodReturn(Ref("value"));
            }
            else {
                block.DeclareVar<object>("value", ExprDotName(Ref("reader"), "Value"))
                    .IfRefNullReturnNull("value")
                    .DeclareVar<EventBean>("theEvent", Cast(typeof(EventBean), Ref("value")));
                if (optSubPropName == null) {
                    block.MethodReturn(Cast(EvaluationType, ExprDotUnderlying(Ref("theEvent"))));
                }
                else {
                    block.MethodReturn(
                        CodegenLegoCast.CastSafeFromObjectType(
                            EvaluationType,
                            optSubPropGetter.EventBeanGetCodegen(Ref("theEvent"), methodNode, classScope)));
                }
            }

            return LocalMethod(methodNode);
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
                classScope.AddOrGetFieldSharable(new VariableReaderCodegenFieldSharable(VariableMetadata));
            if (VariableMetadata.EventType == null) {
                return Cast(EvaluationType, ExprDotName(readerExpression, "Value"));
            }

            var unpack = ExprDotUnderlying(Cast(typeof(EventBean), ExprDotName(readerExpression, "Value")));
            return Cast(EvaluationType, unpack);
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
            if (optSubPropName != null) {
                if (VariableMetadata.EventType == null) {
                    throw new ExprValidationException(
                        "Property '" + optSubPropName + "' is not valid for variable '" + variableName + "'");
                }

                optSubPropGetter = ((EventTypeSPI) VariableMetadata.EventType).GetGetterSPI(optSubPropName);
                if (optSubPropGetter == null) {
                    throw new ExprValidationException(
                        "Property '" + optSubPropName + "' is not valid for variable '" + variableName + "'");
                }

                EvaluationType = VariableMetadata.EventType.GetPropertyType(optSubPropName);
            }
            else {
                EvaluationType = VariableMetadata.Type;
            }

            EvaluationType = EvaluationType.GetBoxedType();
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

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(VariableMetadata.VariableName);
            if (optSubPropName != null) {
                writer.Write(".");
                writer.Write(optSubPropName);
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

            if (optSubPropName != null ? !optSubPropName.Equals(that.optSubPropName) : that.optSubPropName != null) {
                return false;
            }

            return that.VariableMetadata.VariableName.Equals(VariableMetadata.VariableName);
        }

        public string VariableNameWithSubProp {
            get {
                if (optSubPropName == null) {
                    return VariableMetadata.VariableName;
                }

                return VariableMetadata.VariableName + "." + optSubPropName;
            }
        }
    }
} // end of namespace