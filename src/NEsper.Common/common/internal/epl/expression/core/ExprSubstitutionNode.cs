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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.compile.stage3.StmtClassForgeableAIFactoryProviderBase;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Represents a substitution value to be substituted in an expression tree, not valid for any purpose of use
    ///     as an expression, however can take a place in an expression tree.
    /// </summary>
    public class ExprSubstitutionNode : ExprNodeBase,
        ExprForge,
        ExprNodeDeployTimeConst
    {
        private CodegenExpressionField _field;

        public ExprSubstitutionNode(
            string optionalName,
            ClassIdentifierWArray optionalType)
        {
            OptionalName = optionalName;
            OptionalType = optionalType;
        }

        /// <summary>
        ///     Returns the substitution parameter name (or null if by-index).
        /// </summary>
        /// <returns>name</returns>
        public string OptionalName { get; }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ClassIdentifierWArray OptionalType { get; }

        public Type ResolvedType { get; private set; } = typeof(object);

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return InstanceField(Ref(MEMBERNAME_STATEMENT_FIELDS), AsField(codegenClassScope).Field);
        }

        public Type EvaluationType => ResolvedType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.DEPLOYCONST;

        public ExprNodeRenderable ExprForgeRenderable {
            get {
                return new ProxyExprNodeRenderable {
                    ProcToEPL = (writer, _, flags) => writer.Write("?")
                };
            }
        }

        public CodegenExpression CodegenGetDeployTimeConstValue(CodegenClassScope classScope)
        {
            return InstanceField(Ref(MEMBERNAME_STATEMENT_FIELDS), AsField(classScope).Field);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (OptionalType != null) {
                Type clazz = null;
                try {
                    clazz = TypeHelper.GetClassForName(
                        OptionalType.ClassIdentifier,
                        validationContext.ImportService.TypeResolver);
                }
                catch (TypeLoadException) {
                }

                if (clazz == null) {
                    clazz = TypeHelper.GetTypeForSimpleName(
                        OptionalType.ClassIdentifier,
                        validationContext.ImportService.TypeResolver);
                }

                if (clazz != null) {
                    ResolvedType = clazz;
                }
                else {
                    try {
                        ResolvedType = validationContext.ImportService.ResolveType(
                            OptionalType.ClassIdentifier,
                            false,
                            validationContext.ClassProvidedExtension);
                    }
                    catch (ImportException e) {
                        throw new ExprValidationException(
                            "Failed to resolve type '" + OptionalType.ClassIdentifier + "': " + e.Message,
                            e);
                    }
                }

                if (ResolvedType != null && OptionalType.IsArrayOfPrimitive && ResolvedType.CanBeNull()) {
                    throw new ExprValidationException(
                        "Invalid use of the '" +
                        ClassIdentifierWArray.PRIMITIVE_KEYWORD +
                        "' keyword for non-primitive type '" +
                        ResolvedType.Name +
                        "'");
                }

                if (!OptionalType.IsArrayOfPrimitive) {
                    ResolvedType = ResolvedType.GetBoxedType();
                }

                ResolvedType = TypeHelper.GetArrayType(ResolvedType, OptionalType.ArrayDimensions);
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

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprSubstitutionNode;
        }

        private CodegenExpressionField AsField(CodegenClassScope classScope)
        {
            if (_field == null) {
                _field = Field(classScope.AddSubstitutionParameter(OptionalName, ResolvedType));
            }

            return _field;
        }

        public void RenderForFilterPlan(StringBuilder stringBuilder)
        {
            stringBuilder.Append("substitution parameter");
            if (OptionalName != null) {
                stringBuilder
                    .Append(" name '")
                    .Append(OptionalName)
                    .Append("'");
            }

            if (OptionalType != null) {
                stringBuilder
                    .Append(" type '")
                    .Append(OptionalType.ToEPL())
                    .Append("'");
            }
        }
    }
} // end of namespace