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

        public static bool CanRenderConstant(object constant)
        {
            switch (constant) {
                case string _:
                case char _:
                case null:
                case byte _:
                case short _:
                case int _:
                case long _:
                case float _:
                case double _:
                case decimal _:
                case bool _:
                case BigInteger _:
                case Array _:
                case Type _:
                    return true;

                default:
                    return constant.GetType().IsEnum;
            }
        }

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
            else if (constant is char c) {
                builder.Append(Literal(c).ToFullString());
            }
            else if (constant == null) {
                builder.Append("null");
            }
            else if (constant is int i) {
                builder.Append(Literal(i).ToFullString());
            }
            else if (constant is long l) {
                builder.Append(Literal(l).ToFullString());
            }
            else if (constant is float f) {
                builder.Append(Literal(f).ToFullString());
            }
            else if (constant is double d) {
                var literal = Literal(d).ToFullString();
                if (!literal.EndsWith("d")) {
                    literal += "d";
                }

                builder.Append(literal);
            }
            else if (constant is decimal constant1) {
                var literal = Literal(constant1).ToFullString();
                if (!literal.EndsWith("m") && !literal.EndsWith("M")) {
                    literal += "m";
                }

                builder.Append(literal);
            }
            else if (constant is short s) {
                builder.Append("(short) ");
                builder.Append(Literal(s).ToFullString());
            }
            else if (constant is byte b) {
                builder.Append("(byte)");
                builder.Append(Literal(b).ToFullString());
            }
            else if (constant is bool booleanConstant) {
                builder.Append(booleanConstant ? "true" : "false");
            }
            else if (constant is Array asArray) {
                RenderArray(builder, asArray);
            }
            else if (constant is Type type) {
                CodegenExpressionClass.RenderClass(type, builder);
            }
            else if (constant is BigInteger integer) {
                RenderBigInteger(integer, builder);
            }
            else if (constant.GetType().IsEnum) {
                AppendClassName(builder, constant.GetType());
                builder.Append(".").Append(constant);
            }
            else {
                builder.Append(constant);
            }
        }

        private static void RenderArray(
            StringBuilder builder,
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
                    .Select(b => (SyntaxNodeOrToken)ToSyntax(b))
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