///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

using com.espertech.esper.compat.logging;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationHelper;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.model.expression
{
    public class CodegenExpressionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void RenderConstant(
            StringBuilder builder,
            object constant)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("RenderConstant: {0} => {1}", constant?.GetType(), constant);
            }

            if (constant is string stringConstant) {
                // StringEscapeUtils.EscapeJava((string) constant));
                builder.Append(Literal(stringConstant).ToFullString());
            }
            else if (constant is char) {
                builder.Append(Literal((char) constant).ToFullString());
            }
            else if (constant == null) {
                builder.Append("null");
            }
            else if (constant is long) {
                builder.Append(Literal((long) constant).ToFullString());
            }
            else if (constant is float) {
                builder.Append(Literal((float) constant).ToFullString());
            }
            else if (constant is double) {
                var literal = Literal((double) constant).ToFullString();
                if (!literal.EndsWith("d")) {
                    literal += "d";
                }
                builder.Append(literal);
            }
            else if (constant is decimal) {
                var literal = Literal((decimal) constant).ToFullString();
                if (!literal.EndsWith("m") && !literal.EndsWith("M")) {
                    literal += "m";
                }

                builder.Append(literal);
            }
            else if (constant is short) {
                builder.Append("(short) ");
                builder.Append(Literal((short) constant).ToFullString());
            }
            else if (constant is byte) {
                builder.Append("(byte)");
                builder.Append(Literal((byte) constant).ToFullString());
            }
            else if (constant is bool booleanConstant) {
                builder.Append(booleanConstant ? "true" : "false");
            }
            else if (constant is Array asArray) {
                RenderArray(builder, asArray);
            }
            else if (constant.GetType().IsEnum) {
                AppendClassName(builder, constant.GetType());
                builder.Append(".").Append(constant);
            }
            else if (constant is Type) {
                CodegenExpressionClass.RenderClass((Type) constant, builder);
            }
            else if (constant is BigInteger) {
                RenderBigInteger((BigInteger) constant, builder);
            }
            else {
                builder.Append(constant);
            }
        }

        private static void RenderArray(StringBuilder builder,
            Array asArray)
        {
            if (asArray.Length == 0) {
                builder.Append("new ");
                AppendClassName(builder, asArray.GetType().GetElementType());
                builder.Append("[]{}");
            }
            else {
                builder.Append("new ");
                AppendClassName(builder, asArray.GetType().GetElementType());
                builder.Append("[] {");
                var delimiter = "";
                for (var i = 0; i < asArray.Length; i++) {
                    builder.Append(delimiter);
                    RenderConstant(builder, asArray.GetValue(i));
                    delimiter = ",";
                }

                builder.Append("}");
            }
        }

        private static CastExpressionSyntax ToSyntax(byte byteValue)
        {
            return CastExpression(
                PredefinedType(
                    Token(SyntaxKind.ByteKeyword)),
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(byteValue)));
        }

        private static SeparatedSyntaxList<ExpressionSyntax> ToSyntax(byte[] byteArray)
        {
            return SeparatedList<ExpressionSyntax>(
                byteArray
                    .Select(b => (SyntaxNodeOrToken) ToSyntax(b))
                    .ToArray());
        }

        private static ArrayTypeSyntax DeclareByteArray()
        {
            return ArrayType(
                    PredefinedType(
                        Token(SyntaxKind.ByteKeyword)))
                .WithRankSpecifiers(
                    SingletonList<ArrayRankSpecifierSyntax>(
                        ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(
                                OmittedArraySizeExpression()
                            ))));
        }

        private static InitializerExpressionSyntax InitializeByteArray(byte[] byteArray)
        {
            return InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                ToSyntax(byteArray));
        }

        private static void RenderBigInteger(
            BigInteger constant,
            StringBuilder builder)
        {
            var argumentList = ArgumentList(
                SingletonSeparatedList<ArgumentSyntax>(
                    Argument(
                        ArrayCreationExpression(DeclareByteArray())
                            .WithInitializer(InitializeByteArray(constant.ToByteArray())))));

            var typeName = QualifiedName(
                QualifiedName(
                    IdentifierName("System"),
                    IdentifierName("Numeric")),
                IdentifierName("BigInteger"));

            var newValueExpression = ObjectCreationExpression(
                    IdentifierName(typeof(BigInteger).FullName))
                .WithArgumentList(argumentList);

            builder
                .Append("new ")
                .Append(typeof(BigInteger).FullName)
                .Append("(");
            RenderConstant(builder, constant.ToByteArray());
            builder.Append(")");
        }

        private static void AppendSequenceEscapeDQ(
            StringBuilder builder,
            string seq)
        {
            for (var i = 0; i < seq.Length; i++) {
                var c = seq[i];
                if (c == '\"') {
                    builder.Append('\\');
                    builder.Append(c);
                }
                else {
                    builder.Append(c);
                }
            }
        }
    }
} // end of namespace