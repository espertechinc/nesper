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
using static com.espertech.esper.common.@internal.compile.stage3.StmtClassForgeableAIFactoryProviderBase;

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
        private CodegenExpressionField _field;

        public ExprSubstitutionNode(
            string optionalName,
            ClassDescriptor optionalType)
        {
            this.OptionalName = optionalName;
            this.OptionalType = optionalType;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (OptionalType != null) {
                Type clazz = null;
                try {
                    clazz = TypeHelper.GetTypeForName(
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

                if (clazz == null) {
                    try {
                        clazz = validationContext.ImportService.ResolveType(
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

                ResolvedType = ImportTypeUtil.ParameterizeType(
                    true,
                    clazz,
                    OptionalType,
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
            return node is ExprSubstitutionNode;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return InstanceField(Ref(MEMBERNAME_STATEMENT_FIELDS), AsField(codegenClassScope).Field);
        }

        public CodegenExpression CodegenGetDeployTimeConstValue(CodegenClassScope classScope)
        {
            return InstanceField(Ref(MEMBERNAME_STATEMENT_FIELDS), AsField(classScope).Field);
        }

        public void RenderForFilterPlan(StringBuilder @out)
        {
            @out.Append("substitution parameter");
            if (OptionalName != null) {
                @out.Append(" name '").Append(OptionalName).Append("'");
            }

            if (OptionalType != null) {
                @out.Append(" type '").Append(OptionalType.ToEPL()).Append("'");
            }
        }

        private CodegenExpressionField AsField(CodegenClassScope classScope)
        {
            _field ??= Field(classScope.AddSubstitutionParameter(OptionalName, ResolvedType));
            return _field;
        }

        public string OptionalName { get; }

        public ExprEvaluator ExprEvaluator => throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();

        public override ExprForge Forge => this;

        public Type EvaluationType => ResolvedType;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.DEPLOYCONST;

        public ClassDescriptor OptionalType { get; }

        public Type ResolvedType { get; private set; } = typeof(object);

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
    }
} // end of namespace