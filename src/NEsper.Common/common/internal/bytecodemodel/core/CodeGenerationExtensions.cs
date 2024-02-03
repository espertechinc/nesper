using System;
using System.Linq;

using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.core
{
    public static class CodeGenerationExtensions
    {
        public static TypeSyntax GetNameForType(Type type)
        {
            if (type.IsGenericType) {
                throw new NotSupportedException();
            }
            else {
                return ParseName(type.FullName);
            }
        }

        #region Subtract

        public static BinaryExpressionSyntax SubtractFromVariable(
            string variableName,
            SyntaxToken value)
        {
            return BinaryExpression(
                SyntaxKind.SubtractExpression,
                IdentifierName(Identifier(variableName)),
                LiteralExpression(SyntaxKind.NumericLiteralExpression, value));
        }

        public static BinaryExpressionSyntax SubtractFromVariable(
            string variableName,
            int value)
        {
            return SubtractFromVariable(variableName, Literal(value));
        }

        #endregion

        public static TypeSyntax TypeSyntaxFor(Type t)
        {
            if (t == typeof(int)) {
                return PredefinedType(Token(SyntaxKind.IntKeyword));
            }
            else if (t == typeof(long)) {
                return PredefinedType(Token(SyntaxKind.LongKeyword));
            }
            else if (t == typeof(short)) {
                return PredefinedType(Token(SyntaxKind.ShortKeyword));
            }
            else if (t == typeof(float)) {
                return PredefinedType(Token(SyntaxKind.FloatKeyword));
            }
            else if (t == typeof(double)) {
                return PredefinedType(Token(SyntaxKind.DoubleKeyword));
            }
            else if (t == typeof(decimal)) {
                return PredefinedType(Token(SyntaxKind.DecimalKeyword));
            }
            else if (t == typeof(bool)) {
                return PredefinedType(Token(SyntaxKind.BoolKeyword));
            }
            else if (t == typeof(char)) {
                return PredefinedType(Token(SyntaxKind.CharKeyword));
            }
            else if (t == typeof(byte)) {
                return PredefinedType(Token(SyntaxKind.ByteKeyword));
            }
            else if (t == typeof(string)) {
                return PredefinedType(Token(SyntaxKind.StringKeyword));
            }
            else if (t == typeof(object)) {
                return PredefinedType(Token(SyntaxKind.ObjectKeyword));
            }
            else if (t.IsNullable()) {
                return NullableType(TypeSyntaxFor(t.GetGenericArguments()[0]));
            }
            else if (t.IsArray) {
                return ArrayType(TypeSyntaxFor(t.GetElementType()));
            }
            else if (t.IsGenericType) {
                return GenericName(t.Name)
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SeparatedList<TypeSyntax>(
                                t.GetGenericArguments().Select(TypeSyntaxFor))));
            }

            return IdentifierName(t.Name);
        }


        public static TypeSyntax TypeSyntaxFor<T>()
        {
            return TypeSyntaxFor(typeof(T));
        }

        public static VariableDeclarationSyntax DeclareVar<T>(
            string variableName,
            ExpressionSyntax variableInitializer)
        {
            return VariableDeclaration(
                TypeSyntaxFor<T>(),
                SingletonSeparatedList(
                    VariableDeclarator(Identifier(variableName))
                        .WithInitializer(EqualsValueClause(variableInitializer))));
        }

        public static CaseSwitchLabelSyntax CaseLabel(int intValue)
        {
            return CaseSwitchLabel(
                LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal(intValue)));
        }

        public static ExpressionStatementSyntax SimpleAssignment(
            string variableName,
            ExpressionSyntax value)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(variableName),
                    value));
        }
    }
}