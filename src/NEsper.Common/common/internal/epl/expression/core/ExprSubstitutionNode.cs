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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Represents a substitution value to be substituted in an expression tree, not valid for any purpose of use
    /// as an expression, however can take a place in an expression tree.
    /// </summary>
    public class ExprSubstitutionNode : ExprNodeBase,
        ExprForge,
        ExprNodeDeployTimeConst
    {
        private string optionalName;
        private ClassDescriptor optionalType;
        private Type type = typeof(object);
        private CodegenExpressionField field;

        public ExprSubstitutionNode(
            string optionalName,
            ClassDescriptor optionalType)
        {
            this.optionalName = optionalName;
            this.optionalType = optionalType;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (optionalType != null) {
                Type clazz = null;
                try {
                    clazz = TypeHelper.GetTypeForName(
                        optionalType.ClassIdentifier,
                        validationContext.ImportService.TypeResolver);
                }
                catch (TypeLoadException e) {
                }

                if (clazz == null) {
                    clazz = TypeHelper.GetTypeForSimpleName(
                        optionalType.ClassIdentifier,
                        validationContext.ImportService.TypeResolver);
                }

                if (clazz == null) {
                    try {
                        clazz = validationContext.ImportService.ResolveType(
                            optionalType.ClassIdentifier,
                            false,
                            validationContext.ClassProvidedExtension);
                    }
                    catch (ImportException e) {
                        throw new ExprValidationException(
                            "Failed to resolve type '" + optionalType.ClassIdentifier + "': " + e.Message,
                            e);
                    }
                }

                if (!OptionalType.IsArrayOfPrimitive) {
                    clazz = clazz.GetBoxedType();
                }
                else {
                    if (!clazz.IsPrimitive) {
                        throw new ExprValidationException(
                            "Invalid use of the '" +
                            ClassDescriptor.PRIMITIVE_KEYWORD +
                            "' keyword for non-primitive type '" +
                            clazz.CleanName() +
                            "'");
                    }
                }

                type = ImportTypeUtil.ParameterizeType(
                    true,
                    clazz,
                    optionalType,
                    validationContext.ImportService,
                    validationContext.ClassProvidedExtension);
            }

            return null;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("?");
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprSubstitutionNode)) {
                return false;
            }

            return true;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return AsField(codegenClassScope);
        }

        public CodegenExpression CodegenGetDeployTimeConstValue(CodegenClassScope classScope)
        {
            return AsField(classScope);
        }

        public void RenderForFilterPlan(StringBuilder @out)
        {
            @out.Append("substitution parameter");
            if (optionalName != null) {
                @out.Append(" name '").Append(optionalName).Append("'");
            }

            if (optionalType != null) {
                @out.Append(" type '").Append(optionalType.ToEPL()).Append("'");
            }
        }

        private CodegenExpressionField AsField(CodegenClassScope classScope)
        {
            if (field == null) {
                field = Field(classScope.AddSubstitutionParameter(optionalName, type));
            }

            return field;
        }

        public string OptionalName => optionalName;

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public override ExprForge Forge => this;

        public Type EvaluationType => type;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.DEPLOYCONST;

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable() {
                    ProcToEPL = (
                        writer,
                        parentPrecedence,
                        flags) => {
                        writer.Write("?");
                    }
                };
            }
        }

        public ClassDescriptor OptionalType => optionalType;

        public Type ResolvedType => type;
    }
} // end of namespace