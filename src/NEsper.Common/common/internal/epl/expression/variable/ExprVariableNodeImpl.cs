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
        private readonly VariableMetaData variableMeta;
        private readonly string optSubPropName;
        private EventPropertyGetterSPI optSubPropGetter;
        private Type returnType;

        public ExprVariableNodeImpl(
            VariableMetaData variableMeta,
            string optSubPropName)
        {
            this.variableMeta = variableMeta;
            this.optSubPropName = optSubPropName;
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
                var variableName = variableMeta.VariableName;
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

            var variableNameX = variableMeta.VariableName;
            if (optSubPropName != null) {
                if (variableMeta.EventType == null) {
                    throw new ExprValidationException(
                        "Property '" + optSubPropName + "' is not valid for variable '" + variableNameX + "'");
                }

                optSubPropGetter = ((EventTypeSPI)variableMeta.EventType).GetGetterSPI(optSubPropName);
                if (optSubPropGetter == null) {
                    throw new ExprValidationException(
                        "Property '" + optSubPropName + "' is not valid for variable '" + variableNameX + "'");
                }

                returnType = variableMeta.EventType.GetPropertyType(optSubPropName);
            }
            else {
                returnType = variableMeta.Type;
            }

            returnType = returnType.GetBoxedType();
            return null;
        }

        public override string ToString()
        {
            return "variableName=" + variableMeta.VariableName;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (variableMeta.IsCompileTimeConstant) {
                return variableMeta.ValueWhenAvailable;
            }

            throw new IllegalStateException("Cannot evaluate at compile time");
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (returnType == null) {
                return ConstantNull();
            }

            var returnClass = returnType;
            var methodNode = parent.MakeChild(returnClass, typeof(ExprVariableNodeImpl), classScope);
            var readerExpression = GetReaderExpression(variableMeta, methodNode, symbols, classScope);
            var block = methodNode.Block.DeclareVar<VariableReader>("reader", readerExpression);
            if (variableMeta.EventType == null) {
                block.DeclareVar(returnClass, "value", Cast(returnClass, ExprDotName(Ref("reader"), "Value")))
                    .MethodReturn(Ref("value"));
            }
            else {
                block.DeclareVar<object>("value", ExprDotName(Ref("reader"), "Value"))
                    .IfRefNullReturnNull("value")
                    .DeclareVar<EventBean>("theEvent", Cast(typeof(EventBean), Ref("value")));
                if (optSubPropName == null) {
                    block.MethodReturn(Cast(returnClass, ExprDotUnderlying(Ref("theEvent"))));
                }
                else {
                    block.MethodReturn(
                        CodegenLegoCast.CastSafeFromObjectType(
                            returnType,
                            optSubPropGetter.EventBeanGetCodegen(Ref("theEvent"), methodNode, classScope)));
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
            if (returnType == null) {
                return ConstantNull();
            }

            var returnClass = returnType;
            CodegenExpression readerExpression =
                classScope.AddOrGetDefaultFieldSharable(new VariableReaderCodegenFieldSharable(variableMeta));
            if (variableMeta.EventType == null) {
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
            writer.Write(variableMeta.VariableName);
            if (optSubPropName != null) {
                writer.Write(".");
                writer.Write(optSubPropName);
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

            if (!optSubPropName?.Equals(that.optSubPropName) ?? that.optSubPropName != null) {
                return false;
            }

            return that.variableMeta.VariableName.Equals(variableMeta.VariableName);
        }

        public void RenderForFilterPlan(StringBuilder @out)
        {
            @out.Append("variable '").Append(VariableNameWithSubProp).Append("'");
        }

        public ExprEvaluator ExprEvaluator => this;

        public VariableMetaData VariableMetadata => variableMeta;

        public Type EvaluationType => returnType;

        public override ExprForge Forge => this;

        public ExprNodeRenderable ForgeRenderable => this;
        
        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprForgeConstantType ForgeConstantType {
            get {
                if (variableMeta.OptionalContextName != null) {
                    return ExprForgeConstantType.NONCONST;
                }

                // for simple-value variables that are constant and created by the same module and not preconfigured we can use compile-time constant
                if (variableMeta.IsConstant &&
                    variableMeta.IsCreatedByCurrentModule &&
                    variableMeta.VariableVisibility != NameAccessModifier.PRECONFIGURED &&
                    variableMeta.EventType == null) {
                    return ExprForgeConstantType.COMPILETIMECONST;
                }

                return variableMeta.IsConstant ? ExprForgeConstantType.DEPLOYCONST : ExprForgeConstantType.NONCONST;
            }
        }

        public string VariableNameWithSubProp {
            get {
                if (optSubPropName == null) {
                    return variableMeta.VariableName;
                }

                return variableMeta.VariableName + "." + optSubPropName;
            }
        }
    }
} // end of namespace