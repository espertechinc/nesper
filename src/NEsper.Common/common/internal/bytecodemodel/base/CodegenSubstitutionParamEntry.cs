///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static com.espertech.esper.common.@internal.bytecodemodel.core.CodeGenerationExtensions;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace com.espertech.esper.common.@internal.bytecodemodel.@base
{
    public class CodegenSubstitutionParamEntry
    {
        public CodegenSubstitutionParamEntry(
            CodegenField field,
            string name,
            Type type)
        {
            Field = field;
            Name = name;
            Type = type;
        }

        public CodegenField Field { get; }

        public string Name { get; }

        public Type Type { get; }

        public static void CodegenSetterBody(
            CodegenClassScope classScope,
            CodegenBlock enclosingBlock,
            CodegenExpression stmtFieldsInstance)
        {
            var numbered = classScope.NamespaceScope.SubstitutionParamsByNumber;
            var named = classScope.NamespaceScope.SubstitutionParamsByName;
            if (numbered.IsEmpty() && named.IsEmpty()) {
                return;
            }

            if (!numbered.IsEmpty() && !named.IsEmpty()) {
                throw new IllegalStateException("Both named and numbered substitution parameters are non-empty");
            }

            IList<CodegenSubstitutionParamEntry> fields;
            if (!numbered.IsEmpty()) {
                fields = numbered;
            }
            else {
                fields = new List<CodegenSubstitutionParamEntry>(named.Values);
            }

            enclosingBlock.DeclareVar<int>("zidx", Op(Ref("index"), "-", Constant(1)));
            var blocks = enclosingBlock.SwitchBlockOfLength(Ref("zidx"), fields.Count, false);
            for (var i = 0; i < blocks.Length; i++) {
                CodegenSubstitutionParamEntry param = fields[i];
                blocks[i].AssignRef(
                    ExprDotName(stmtFieldsInstance, param.Field.Name), 
                    Cast(param.Type.GetBoxedType(), Ref("value")));
            }
        }

        public static BlockSyntax CodegenSetterBody(
            CodegenClassScope classScope)
        {
            var numbered = classScope.NamespaceScope.SubstitutionParamsByNumber;
            var named = classScope.NamespaceScope.SubstitutionParamsByName;
            if (numbered.IsEmpty() && named.IsEmpty()) {
                return Block();
            }

            if (!numbered.IsEmpty() && !named.IsEmpty())
            {
                throw new IllegalStateException("Both named and numbered substitution parameters are non-empty");
            }

            IList<CodegenSubstitutionParamEntry> fields;
            if (!numbered.IsEmpty())
            {
                fields = numbered;
            }
            else
            {
                fields = new List<CodegenSubstitutionParamEntry>(named.Values);
            }

            var zidxDeclaration = LocalDeclarationStatement(
                DeclareVar<int>("zidx", SubtractFromVariable("index", 1)));
            var switchSections = new SyntaxList<SwitchSectionSyntax>();
            for (int ii = 0; ii < fields.Count; ii++) {
                var param = fields[ii];
                var paramType = TypeSyntaxFor(param.Type.GetBoxedType());
                switchSections = switchSections.Add(
                    SwitchSection()
                        .WithLabels(SingletonList<SwitchLabelSyntax>(CaseLabel(ii)))
                        .WithStatements(
                            List<StatementSyntax>(
                                new StatementSyntax[] {
                                    SimpleAssignment(
                                        param.Field.Name,
                                        CastExpression(paramType, IdentifierName("value"))),
                                    BreakStatement()
                                })));
            }

            var switchStatement = SwitchStatement(IdentifierName("zidx"))
                .WithOpenParenToken(Token(SyntaxKind.OpenParenToken))
                .WithCloseParenToken(Token(SyntaxKind.CloseParenToken))
                .WithSections(switchSections);

            return Block(new SyntaxList<StatementSyntax>()
                .Add(zidxDeclaration)
                .Add(switchStatement));
        }
    }
} // end of namespace